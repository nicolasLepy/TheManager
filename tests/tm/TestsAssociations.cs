using FluentNHibernate.Testing.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm;
using tm.Tournaments;

namespace tests.tm
{
    [TestClass]
    public class TestsAssociations : TheManagerTest
    {

        //dotnet test --collect:"XPlat Code Coverage"
        //Results are stored into TestResults folder
        //Install tool for html report
        ///dotnet tool install -g dotnet-reportgenerator-globaltool
        // reportgenerator -reports:"TheManagerTests\TestResults\ffe9acf3-b390-4734-aa2a-26f41f6a445a\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

        private static int TEST_YEARS = 10;

        private Round NextRound(Tournament tournament, Round round, bool isRegional)
        {
            Round res = null;
            int idxRound = tournament.rounds.IndexOf(round);
            if(idxRound == tournament.rounds.Count - 1)
            {
                if(isRegional)
                {
                    res = tournament.parent.rounds[2];
                }
            }
            else
            {
                res = tournament.rounds[idxRound+1];
            }
            return res;
        }

        private void CheckRoundGeneral(Tournament tournament, Round round, bool isRegional)
        {
            Round nextRound = NextRound(tournament, round, isRegional);
            if(nextRound != null)
            {
                foreach (Match m in round.matches)
                {
                    if(m.Played)
                    {
                        //Chaque équipe qui gagne doit être qualifiée au tour suivant
                        Assert.IsTrue(nextRound.clubs.Contains(m.Winner));
                        //Chaque équipe qui perd ne doit pas être qualifiée au tour suivant
                        Assert.IsFalse(nextRound.clubs.Contains(m.Looser));
                    }
                }
            }
            List<Club> clubs = new List<Club>();
            foreach(Club c in round.clubs)
            {
                Assert.IsFalse(clubs.Contains(c));
                clubs.Add(c);
            }
        }

        private void CheckCup(Association association, Tournament cup, List<int> expectedTeamsByRounds)
        {
            Dictionary<Association, int> teamsByAssociations = cup.rounds[2].teamsByAssociation;

            TestUtils.PrintTournament(cup);

            //Bon nombre de matchs à chaque tour
            for (int i = 0; i < cup.rounds.Count; i++)
            {
                Round round = cup.rounds[cup.rounds.Count - (i + 1)];
                if (expectedTeamsByRounds.Count - (i + 1) >= 0)
                {
                    Assert.AreEqual(round.clubs.Count, expectedTeamsByRounds[expectedTeamsByRounds.Count - (i + 1)]);
                }
                CheckRoundGeneral(cup, round, false);
                if (round.clubs.Count < 65 && round.clubs.Count > 0)
                {
                    foreach (Match m in round.matches)
                    {
                        //Ultramarine team can't play home
                        Assert.IsTrue(m.home.Association().IsDirectConnected(association));
                    }
                }
            }

            foreach (KeyValuePair<Association, int> regionalPath in teamsByAssociations)
            {
                Tournament regionalCup = regionalPath.Key.Cup(1000);
                TestUtils.PrintTournament(regionalCup);
                int i = 0;
                foreach(Round r in regionalCup.rounds)
                {
                    bool haveUltraMarine = false;

                    foreach(Club c in r.clubs)
                    {
                        haveUltraMarine = haveUltraMarine || new List<string>() { "Saint Pierre et Miquelon", "Guadeloupe", "Martinique", "Guyane", "Réunion", "Mayotte", "Nouvelle-Calédonie" }.Contains(c.Association().name);

                        // Pas de L2 en tour régionaux
                        Assert.IsTrue(Session.Instance.Game.kernel.LocalisationTournament(c.Championship) != association || c.Championship.level > 2);

                        if(!haveUltraMarine)
                        {
                            // Chaque coupe régionale possède uniquement des équipes de la bonne association
                            Assert.IsTrue(c.Association().IsDirectConnected(regionalPath.Key));
                        }
                    }
                    CheckRoundGeneral(regionalCup, r, true);

                    // Chaque coupe régionale possède le bon nombre de matchs (5ème tour, 6ème tour)
                    if(!haveUltraMarine)
                    {
                        //Assert.AreEqual(r.clubs.Count, regionalPath.Value * Math.Pow(2, regionalCup.rounds.Count - i));
                    }
                    i++;

                }
            }
        }

        private void CheckAssociationLeagueSystem(Association association, Dictionary<Club, int> occurences, int maxLevelReservesAllowed)
        {
            foreach(Tournament t in association.Leagues())
            {
                Round r = t.rounds[0];
                foreach(Club c in r.clubs)
                {
                    occurences[c]++;
                    Assert.IsTrue(c.Association().IsDirectConnected(association));

                    ReserveClub rc = c as ReserveClub;
                    if(rc != null)
                    {
                        Assert.IsTrue(maxLevelReservesAllowed == -1 || t.level >= maxLevelReservesAllowed);
                        Club clubAbove = rc.GetTeamAbove();
                        Assert.IsTrue(association.LeagueBelow(t) == null || clubAbove.Championship.IsAbove(new QualificationTournament(rc.Championship)));
                    }
                }
            }
            foreach(Association a in association.associations)
            {
                CheckAssociationLeagueSystem(a, occurences, -1);
            }
        }

        /// <summary>
        /// Check only one team from each N3 group was promoted to N2
        /// </summary>
        /// <param name="masterAssociation"></param>
        private void CheckN3N2(Association masterAssociation)
        {
            Tournament n2 = masterAssociation.League(4);
            Tournament n3 = masterAssociation.League(5);
            if(n2.previousEditions.Count > 0)
            {
                int maxValueKey = n3.previousEditions.Aggregate((x, y) => x.Key > y.Key ? x : y).Key;
                Tournament n3previous = n3.previousEditions[maxValueKey];
                GroupsRound n3previousRound = n3previous.rounds[0] as GroupsRound;
                Assert.IsTrue(n3previousRound != null);
                for(int i = 0; i < n3previousRound.groupsCount; i++)
                {
                    int teamsPromotedInN2 = 0;
                    List<Club> ranking = n3previousRound.Ranking(i);
                    foreach (Club c in ranking)
                    {
                        if (c.Championship.level == 4)
                        {
                            teamsPromotedInN2++;
                        }
                    }
                    Assert.AreEqual(teamsPromotedInN2, 1);
                }
            }
        }

        private void CheckBottomTeamsWereRelegated(Association masterAssociation)
        {
            foreach(Tournament league in masterAssociation.Leagues())
            {
                if (masterAssociation.LeagueBelow(league) != null && league.previousEditions.Count > 0)
                {
                    int maxValueKey = league.previousEditions.Aggregate((x, y) => x.Key > y.Key ? x : y).Key;
                    Tournament leaguePrevious = league.previousEditions[maxValueKey];
                    GroupsRound gr = leaguePrevious.rounds[0] as GroupsRound;
                    Assert.IsTrue(gr != null);
                    for(int i = 0; i < gr.groupsCount; i++)
                    {
                        List<Club> ranking = gr.Ranking(i);
                        Club last = ranking[ranking.Count - 1];
                        //R1->R2: Min 2 relegations. If >2 groups, some groups could have no team relegated.
                        if (gr.groupsCount <= 2)
                        {
                            Assert.IsTrue(last.Championship.IsBelow(new QualificationTournament(league)));
                        }
                    }
                }
            }
            foreach(Association a in masterAssociation.associations)
            {
                CheckBottomTeamsWereRelegated(a);
            }
        }

        private void CheckLeagueSystem(Association az, int expectedTotalClubs, int maxLevelReservesAllowed)
        {
            Dictionary<Club, int> occurences = new Dictionary<Club, int>();
            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                if((c as NationalTeam) == null && c.Association().IsDirectConnected(az))
                {
                    occurences[c] = 0;
                }
            }

            CheckAssociationLeagueSystem(az, occurences, maxLevelReservesAllowed);

            Assert.AreEqual(expectedTotalClubs, occurences.Count);
            foreach(KeyValuePair<Club, int> kvp in occurences)
            {
                Assert.AreEqual(1, kvp.Value);
            }
        }

        private void CheckRegionalLeague(Tournament t, Association aReg, int expectedTeams)
        {
            GroupsRound r = t.rounds[0] as GroupsRound;
            Assert.IsNotNull(r);
            int teams = 0;
            foreach(List<Club> cGroups in r.groups)
            {
                foreach(Club c in cGroups)
                {
                    Assert.IsTrue(aReg.ContainsAssociation(c.Association()));
                    teams++;
                }
            }
            if(expectedTeams > -1)
            {
                Assert.AreEqual(teams, expectedTeams);
            }
        }

        [TestMethod]
        public void TestLeagueStructureConservedBasicStructure()
        {
            InitGame("ui", "database_unitttests", new List<string>() { "France"});
            for(int y = 0; y < TEST_YEARS; y++)
            {
                for (int i = 0; i < 365; i++)
                {
                    Session.Instance.Game.NextDay();
                    Session.Instance.Game.UpdateTournaments();
                }

                Country az = Session.Instance.Game.kernel.String2Country("Azerbaïdjan");
                Association aAz = az.GetCountryAssociation();
                List<Association> aLevel0 = az.GetCountryAssociation().associations;

                List<Association> aLevel1 = new List<Association>();
                foreach (Association a in aLevel0)
                {
                    aLevel1.AddRange(a.GetAllChilds());
                }
                Assert.AreEqual(3, aLevel0.Count);
                Assert.AreEqual("Tatooine", aLevel0[2].name);
                Assert.AreEqual(2, aLevel1.Count);

                //Check each national league have the required number of teams
                Tournament t1 = aAz.League(1);

                int numberOfTeams = t1.rounds[0].clubs.Count;
                Assert.AreEqual(12, numberOfTeams);

                Tournament t2 = aAz.League(2);
                numberOfTeams = t2.rounds[0].clubs.Count;
                Assert.AreEqual(numberOfTeams, 24);
                GroupsRound r20 = t2.rounds[0] as GroupsRound;
                Assert.AreEqual(r20.groupsCount, 3);
                Assert.AreEqual(r20.Ranking(0).Count, 8);
                Assert.AreEqual(r20.Ranking(1).Count, 8);
                Assert.AreEqual(r20.Ranking(2).Count, 8);

                //Check each regional league have teams of its association, and the correct number. Number of teams in the last level can vary

                Association a02 = aLevel0[2];
                Association a10 = aLevel1[0];
                Association a11 = aLevel1[1];

                Tournament a02_1 = a02.League(1);
                Tournament a02_2 = a02.League(2);
                Tournament a10_1 = a10.League(1);
                Tournament a11_1 = a11.League(1);

                CheckRegionalLeague(a02_1, a02, 8);
                CheckRegionalLeague(a02_2, a02, 16);
                CheckRegionalLeague(a10_1, a10, 8);
                CheckRegionalLeague(a11_1, a11, 8);

                //Check each club (and eventual reserve) have a league associated, and no doublons
                CheckLeagueSystem(aAz, 108, -1);
            }
        }

        [TestMethod]
        public void TestLeagueStructureConservedFranceExtended()
        {
            bool disabled = false;
            InitGame("ui", "database_france_nat", disabled ? new List<string>() { } : new List<string>() { "France"});
            for (int y = 0; y < TEST_YEARS; y++)
            {
                Country fr = Session.Instance.Game.kernel.String2Country("France");
                Association aFr = fr.GetCountryAssociation();

                Country ma = Session.Instance.Game.kernel.String2Country("Martinique");
                Association aMa = ma.GetCountryAssociation();

                for (int i = 0; i < 365; i++)
                {
                    if (Utils.CompareDates(aFr.League(1).seasonBeginning.ConvertToDateTime().AddDays(-30), Session.Instance.Game.date))
                    {
                        CheckCup(aFr, aFr.Cup(1), new List<int>() { 168, 88, 64, 32, 16, 8, 4, 2 });
                    }
                    if (Utils.CompareDates(aFr.League(1).seasonBeginning.ConvertToDateTime().AddDays(-1), Session.Instance.Game.date))
                    {
                        Console.WriteLine("[{0}] Classements finaux - France", Session.Instance.Game.date.Year);
                        TestUtils.PrintLeagueSystem(aFr);
                        Console.WriteLine("[{0}] Classements finaux - Martinique", Session.Instance.Game.date.Year);
                        TestUtils.PrintLeagueSystem(aMa);
                    }
                    if (Utils.CompareDates(aFr.League(1).seasonBeginning.ConvertToDateTime().AddDays(31), Session.Instance.Game.date))
                    {
                        Console.WriteLine("[{0}-{1}] Nouveaux championnats", Session.Instance.Game.date.Year, Session.Instance.Game.date.Year + 1);
                        TestUtils.PrintLeagueSystem(aFr);
                    }

                    Session.Instance.Game.NextDay();
                    Session.Instance.Game.UpdateTournaments();
                }

                List<Association> aLevel0 = fr.GetCountryAssociation().associations;

                List<Association> aLevel1 = new List<Association>();
                foreach (Association a in aLevel0)
                {
                    aLevel1.AddRange(a.GetAllChilds());
                }
                Assert.AreEqual(15, aLevel0.Count);
                Assert.AreEqual("Hauts de France", aLevel0[2].name);
                Assert.AreEqual(92, aLevel1.Count);

                //Check each national league have the required number of teams
                Tournament t1 = aFr.League(1);

                int numberOfTeams = t1.rounds[0].clubs.Count;
                Assert.AreEqual(20, numberOfTeams);

                Tournament t2 = aFr.League(2);
                numberOfTeams = t2.rounds[0].clubs.Count;
                Assert.AreEqual(numberOfTeams, 20);

                Tournament t3 = aFr.League(3);
                Assert.AreEqual(t3.rounds[0].clubs.Count, 18);

                Tournament t4 = aFr.League(4);
                Assert.AreEqual(t4.rounds[0].clubs.Count, 64);
                GroupsRound r40 = t4.rounds[0] as GroupsRound;
                if(!disabled)
                {
                    Assert.AreEqual(r40.groupsCount, 4);
                    Assert.AreEqual(r40.Ranking(0).Count, 16);
                    Assert.AreEqual(r40.Ranking(1).Count, 16);
                    Assert.AreEqual(r40.Ranking(2).Count, 16);
                    Assert.AreEqual(r40.Ranking(3).Count, 16);
                }
                Tournament t5 = aFr.League(5);
                Assert.AreEqual(t5.rounds[0].clubs.Count, 181);

                //Check each regional league have teams of its association, and the correct number. Number of teams in the last level can vary
                Association aReg1 = aFr.associations[0]; //BFC
                Association aReg2 = aFr.associations[1]; //GE
                Association aReg3 = aFr.associations[2]; //HDF
                Association aReg4 = aFr.associations[3]; //Nor
                Association aReg5 = aFr.associations[4]; //IDF
                Association aReg6 = aFr.associations[5]; //Bre
                Association aReg7 = aFr.associations[6]; //PdL
                Association aReg8 = aFr.associations[7]; //CVL
                Association aReg9 = aFr.associations[8]; //NA
                Association aReg10 = aFr.associations[9]; //Occ
                Association aReg11 = aFr.associations[10]; //ARH
                Association aReg12 = aFr.associations[11]; //Cor
                Association aReg13 = aFr.associations[12]; //Med
                Assert.AreEqual("Bourgogne Franche-Comte", aReg1.name);
                Assert.AreEqual("Grand-Est", aReg2.name);
                Assert.AreEqual("Hauts de France", aReg3.name);
                Assert.AreEqual("Normandie", aReg4.name);
                Assert.AreEqual("Ile de France", aReg5.name);
                Assert.AreEqual("Bretagne", aReg6.name);
                Assert.AreEqual("Pays De La Loire", aReg7.name);
                Assert.AreEqual("Centre-Val de Loire", aReg8.name);
                Assert.AreEqual("Nouvelle Aquitaine", aReg9.name);
                Assert.AreEqual("Occitanie", aReg10.name);
                Assert.AreEqual("Auvergne Rhone-Alpes", aReg11.name);
                Assert.AreEqual("Corse", aReg12.name);
                Assert.AreEqual("Méditerranée", aReg13.name);

                Tournament t6_a = aReg1.League(1);
                Tournament t6_b = aReg2.League(1);
                Tournament t6_c = aReg3.League(1);
                Tournament t6_d = aReg4.League(1);
                Tournament t6_e = aReg5.League(1);
                Tournament t6_f = aReg6.League(1);
                Tournament t6_g = aReg7.League(1);
                Tournament t6_h = aReg8.League(1);
                Tournament t6_i = aReg9.League(1);
                Tournament t6_j = aReg10.League(1);
                Tournament t6_k = aReg11.League(1);
                Tournament t6_l = aReg12.League(1);
                Tournament t6_m = aReg13.League(1);

                Tournament t7_a = aReg1.League(2);
                Tournament t7_b = aReg2.League(2);
                Tournament t7_c = aReg3.League(2);
                Tournament t7_d = aReg4.League(2);
                Tournament t7_e = aReg5.League(2);
                Tournament t7_f = aReg6.League(2);
                Tournament t7_g = aReg7.League(2);
                Tournament t7_h = aReg8.League(2);
                Tournament t7_i = aReg9.League(2);
                Tournament t7_j = aReg10.League(2);
                Tournament t7_k = aReg11.League(2);
                Tournament t7_l = aReg12.League(2);
                Tournament t7_m = aReg13.League(2);

                CheckRegionalLeague(t6_a, aReg1, 35);
                CheckRegionalLeague(t6_b, aReg2, 47);
                CheckRegionalLeague(t6_c, aReg3, 36);
                CheckRegionalLeague(t6_d, aReg4, 26);
                CheckRegionalLeague(t6_e, aReg5, 27);
                CheckRegionalLeague(t6_f, aReg6, 34);
                CheckRegionalLeague(t6_g, aReg7, 27);
                CheckRegionalLeague(t6_h, aReg8, 18);
                CheckRegionalLeague(t6_i, aReg9, 44);
                CheckRegionalLeague(t6_j, aReg10, 42);
                CheckRegionalLeague(t6_k, aReg11, 35);
                CheckRegionalLeague(t6_l, aReg12, 13);
                CheckRegionalLeague(t6_m, aReg13, 16);

                CheckRegionalLeague(t7_a, aReg1, -1);
                CheckRegionalLeague(t7_b, aReg2, -1);
                CheckRegionalLeague(t7_c, aReg3, -1);
                CheckRegionalLeague(t7_d, aReg4, -1);
                CheckRegionalLeague(t7_e, aReg5, -1);
                CheckRegionalLeague(t7_f, aReg6, -1);
                CheckRegionalLeague(t7_g, aReg7, -1);
                CheckRegionalLeague(t7_h, aReg8, -1);
                CheckRegionalLeague(t7_i, aReg9, -1);
                CheckRegionalLeague(t7_j, aReg10, -1);
                CheckRegionalLeague(t7_k, aReg11, -1);
                CheckRegionalLeague(t7_l, aReg12, -1);
                CheckRegionalLeague(t7_m, aReg13, -1);

                //Check each club (and eventual reserves) have a league associated, and no doublons
                CheckLeagueSystem(aFr, 1404, 4);
                if(!disabled)
                {
                    CheckN3N2(aFr);
                }
                CheckBottomTeamsWereRelegated(aFr);
            }
        }

        [TestMethod]
        public void TestLeagueStructureConservedFranceLight()
        {
            InitGame("ui", "database_france_light", null);
            for (int y = 0; y < TEST_YEARS; y++)
            {
                Country fr = Session.Instance.Game.kernel.String2Country("France");
                Association aFr = fr.GetCountryAssociation();

                for (int i = 0; i < 365; i++)
                {
                    Session.Instance.Game.NextDay();
                    Session.Instance.Game.UpdateTournaments();
                }

                //Check each national league have the required number of teams
                Tournament t1 = aFr.League(1);

                int numberOfTeams = t1.rounds[0].clubs.Count;
                Assert.AreEqual(20, numberOfTeams);

                Tournament t2 = aFr.League(2);
                numberOfTeams = t2.rounds[0].clubs.Count;
                Assert.AreEqual(numberOfTeams, 20);

                Tournament t3 = aFr.League(3);
                Assert.AreEqual(t3.rounds[0].clubs.Count, 18);

                Tournament t4 = aFr.League(4);
                GroupsRound r40 = t4.rounds[0] as GroupsRound;
                Assert.AreEqual(r40.groupsCount, 4);
                Assert.AreEqual(r40.Ranking(0).Count, 16);
                Assert.AreEqual(r40.Ranking(1).Count, 16);
                Assert.AreEqual(r40.Ranking(2).Count, 16);
                Assert.AreEqual(r40.Ranking(3).Count, 16);

                CheckLeagueSystem(aFr, 122, 4);
            }
        }

        // TODO [TestMethod]
        public void TestSeasonsLight() //About 3 minutes / season
        {
            InitGame("ui", "database_france_light", null);

            int years = 2;
            for (int i = 0; i < 365 * years; i++)
            {
                Session.Instance.Game.NextDay();
                Session.Instance.Game.UpdateTournaments();
            }

            Session.Instance.Game.Save("D:\\Projets\\TheManager\\ui\\bin\\Debug\\test_big.csave");

            //TODO: Check everything are correct : league structure doesn't changed, cup with right teams count
        }

        private void CheckAssociations(List<int> expectedId, List<Association> associations)
        {
            foreach (Association a in associations)
            {
                Assert.IsTrue(expectedId.Contains(a.Id));
                expectedId.Remove(a.Id);
            }
            Assert.IsTrue(expectedId.Count == 0);
        }

        private void CheckTournaments(List<int> expectedId, List<Tournament> tournaments)
        {
            foreach (Tournament t in tournaments)
            {
                Assert.IsTrue(expectedId.Contains(t.Id));
                expectedId.Remove(t.Id);
            }
            Assert.IsTrue(expectedId.Count == 0);
        }

        [TestMethod]
        public void TestTournamentAbove()
        {
            Association world = MakeBasicStructure();

            Association fr = world.associations[0].associations[0];
            Assert.AreEqual(fr.Id, 3);

            List<Tournament> ta = fr.TournamentsAbove(false);
            List<int> expectedId = new List<int>() { 1, 2, 3, 4 };
            CheckTournaments(expectedId, ta);

            Association bfc = fr.associations[0];
            Assert.AreEqual(bfc.Id, 5);

            ta = bfc.TournamentsAbove(false);
            expectedId = new List<int>() { 1, 2, 3, 4, 5, 6, 11 };
            CheckTournaments(expectedId, ta);

        }

        [TestMethod]
        public void TestTournamentLevel()
        {
            Association world = MakeBasicStructure();

            Association fr = world.associations[0].associations[0];
            Assert.AreEqual(fr.Id, 3);
            Association bfc = fr.associations[0];
            Assert.AreEqual(bfc.Id, 5);

            Tournament wl1 = world.League(1);
            Tournament fr1 = fr.League(1);
            Tournament fr2 = fr.League(2);
            Tournament bf1 = bfc.League(1);
            Tournament bf2 = bfc.League(2);
            Tournament bf3 = bfc.League(3);

            Assert.AreEqual(world.TournamentLevel(wl1), 1);
            Assert.AreEqual(world.TournamentLevel(fr1), 3);
            Assert.AreEqual(world.TournamentLevel(fr2), 4);
            Assert.AreEqual(world.TournamentLevel(bf1), 5);
            Assert.AreEqual(world.TournamentLevel(bf2), 6);
            Assert.AreEqual(world.TournamentLevel(bf3), 7);
            Assert.AreEqual(fr.TournamentLevel(fr1), 1);
            Assert.AreEqual(fr.TournamentLevel(fr2), 2);
            Assert.AreEqual(fr.TournamentLevel(bf1), 3);
            Assert.AreEqual(fr.TournamentLevel(bf2), 4);
            Assert.AreEqual(fr.TournamentLevel(bf3), 5);
            Assert.AreEqual(bfc.TournamentLevel(bf1), 1);
            Assert.AreEqual(bfc.TournamentLevel(bf2), 2);
            Assert.AreEqual(bfc.TournamentLevel(bf3), 3);
            Assert.AreEqual(bfc.TournamentLevel(fr1), -1);

        }

        [TestMethod]
        public void TestGetAllChilds()
        {
            Association world = MakeBasicStructure();

            List<Association> childs1 = world.GetAllChilds(1);
            List<int> expectedId = new List<int>() { 2 };  //Europe
            CheckAssociations(expectedId, childs1);

            List<Association> childs2 = world.GetAllChilds(2);
            expectedId = new List<int>() { 3, 4};  //France, Spain
            CheckAssociations(expectedId, childs2);

            List<Association> childs3 = world.GetAllChilds(3);
            expectedId = new List<int>() { 5, 6 };  //BFC, Nord
            CheckAssociations(expectedId, childs3);

            List<Association> childs4 = world.GetAllChilds(4);
            expectedId = new List<int>() { };  //Nothing
            CheckAssociations(expectedId, childs4);
        }

        [TestMethod]
        public void TestGetLevelOfAssociation()
        {
            Association world = MakeBasicStructure();

            Association fr = world.associations[0].associations[0];
            Assert.AreEqual(fr.Id, 3);

            Association bfc = fr.associations[0];
            Assert.AreEqual(bfc.Id, 5);

            Assert.AreEqual(world.GetLevelOfAssociation(fr, 0), 2);
            Assert.AreEqual(world.GetLevelOfAssociation(bfc, 0), 3);
            Assert.AreEqual(bfc.GetLevelOfAssociation(world, 0), -1);
        }

        [TestMethod]
        public void TestLeagueAbove()
        {
            Association world = MakeBasicStructure();

            Association europe = world.associations[0];
            Assert.AreEqual(europe.Id, 2);

            Association fr = world.associations[0].associations[0];
            Assert.AreEqual(fr.Id, 3);

            Association bfc = fr.associations[0];
            Assert.AreEqual(bfc.Id, 5);

            Assert.IsNull(world.LeagueAbove(world.League(1)));
            Assert.AreEqual(fr.LeagueAbove(fr.League(1)).Tournament(), europe.League(1));
            Assert.AreEqual(fr.LeagueAbove(fr.League(2)).Tournament(), fr.League(1));
            Assert.AreEqual(bfc.LeagueAbove(bfc.League(1)).Tournament(), fr.League(2));
            Assert.AreEqual(bfc.LeagueAbove(bfc.League(3)).Tournament(), bfc.League(2));
            try
            {
                fr.LeagueAbove(bfc.League(1));
                Assert.Fail();
            } catch(Exception e){}
        }

        [TestMethod]
        public void TestLeagueBelow()
        {
            Association world = MakeBasicStructure();

            Association europe = world.associations[0];
            Assert.AreEqual(europe.Id, 2);

            Association fr = world.associations[0].associations[0];
            Assert.AreEqual(fr.Id, 3);

            Association bfc = fr.associations[0];
            Assert.AreEqual(bfc.Id, 5);

            Assert.AreEqual(fr.LeagueBelow(fr.League(1)).Tournament(), fr.League(2));
            Assert.AreEqual(fr.LeagueBelow(fr.League(2)).Type, QualificationTargetType.ExcludeFromLeagueSystem);
            Assert.AreEqual(bfc.LeagueBelow(bfc.League(1)).Tournament(), bfc.League(2));
            Assert.IsNull(bfc.LeagueBelow(bfc.League(3)));
            try
            {
                fr.LeagueBelow(bfc.League(1));
                Assert.Fail();
            }
            catch (Exception e) { }
        }
    }
}
