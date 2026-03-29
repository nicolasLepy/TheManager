using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Linq.Functions;
using tm;
using tm.Algorithms;

namespace tests.tm
{
    [TestClass]
    public class TestsCupCreator : TheManagerTest
    {

        /// <summary>
        /// Check every rounds have the right number of teams.
        /// Check every expected teams appear and doesn't appear twice or more
        /// </summary>
        private void CheckCupStructure(CupStructureResult res, List<int> expectedTeamsByRound, Dictionary<Tournament, int> expectedTeamsByLeague, List<Club> expectedTeams)
        {
            int expectedRounds = expectedTeamsByRound.Count;
            Assert.AreEqual(res.roundsCount, expectedRounds);
            List<Club> teamsAppearance = new List<Club>();
            Dictionary<Tournament, int> teamsByLeague = new Dictionary<Tournament, int>();
            int teams = 0;
            for (int i = 0; i < expectedRounds; i++)
            {
                int newTeams = 0;
                foreach (RecoverTeams rt in res.structure[i])
                {
                    List<Club> roundTeams = rt.Source.RetrieveTeams(rt.Number, rt.Flags, false, null);
                    newTeams += roundTeams.Count;
                    foreach(Club c in roundTeams)
                    {
                        Assert.IsTrue(!teamsAppearance.Contains(c));
                        teamsAppearance.Add(c);
                        if((rt.Source as Round) != null)
                        {
                            Tournament t = (rt.Source as Round).Tournament;
                            teamsByLeague[t] = teamsByLeague.ContainsKey(t) ? teamsByLeague[t] + 1 : 1;
                        }
                    }
                }
                teams = teams + newTeams;
                Assert.AreEqual(res.teamsByRound[i], expectedTeamsByRound[i]);
                Assert.AreEqual(teams, expectedTeamsByRound[i]);
                teams = teams / 2; //For the next round
            }
            
            foreach(Club c in expectedTeams)
            {
                Assert.IsTrue(teamsAppearance.Contains(c));
            }
            if(expectedTeamsByLeague != null)
            {
                foreach(KeyValuePair<Tournament, int> t in expectedTeamsByLeague)
                {
                    Assert.AreEqual(t.Value, teamsByLeague[t.Key]);
                }
            }
        }


        /// <summary>
        /// Basic Cup - default case
        /// </summary>
        [TestMethod]
        public void TestCupCase0()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator();
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();

            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(1).rounds[0], 20, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(2).rounds[0], 12, RetrieveFlags.Best));
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>()
            };
            int winners = 1;
            CupStructure structure = new CupStructure(false, true, constaints, pool, winners, false, null);
            CupStructureResult res = creator.CreateStructure(fr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 32, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            Dictionary<Tournament, int> expectedTeamsByLeague = new Dictionary<Tournament, int>();
            expectedTeamsByLeague[fr.League(1)] = 20;
            expectedTeamsByLeague[fr.League(2)] = 12;
            CheckCupStructure(res, expectedTeamsByRound, expectedTeamsByLeague, expectedTeams);
        }

        /// <summary>
        /// Basic Cup - default case with a preliminary round
        /// </summary>
        [TestMethod]
        public void TestCupCase1()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator();
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();

            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(1).rounds[0], 20, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(2).rounds[0], 20, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(3).rounds[0], 18, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>()
            };
            int winners = 1;
            CupStructure structure = new CupStructure(false, true, constaints, pool, winners, false, null);
            CupStructureResult res = creator.CreateStructure(fr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 52, 32, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(3).rounds[0].clubs);
            Dictionary<Tournament, int> expectedTeamsByLeague = new Dictionary<Tournament, int>();
            expectedTeamsByLeague[fr.League(1)] = 20;
            expectedTeamsByLeague[fr.League(2)] = 20;
            expectedTeamsByLeague[fr.League(3)] = 18;
            CheckCupStructure(res, expectedTeamsByRound, expectedTeamsByLeague, expectedTeams);
        }

        /// <summary>
        /// Basic cup - Not constrained to draw all teams
        /// </summary>
        [TestMethod]
        public void TestCupCase2()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(true);
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();

            List<RecoverTeams> pool = new List<RecoverTeams>();
            //TODO: Teams count is ignored (put null, -1 or Infinity)
            pool.Add(new RecoverTeams(fr.League(1).rounds[0], 20, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(2).rounds[0], 20, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(3).rounds[0], 18, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(4).rounds[0], 30, RetrieveFlags.Best)); //No extra round allowed, so only 6 teams from N2 will be drawn
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
            };
            CupStructure structure = new CupStructure(false, true, constaints, pool, 1, true, 6);
            CupStructureResult res = creator.CreateStructure(fr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 64, 32, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(3).rounds[0].clubs);
            Dictionary<Tournament, int> expectedTeamsByLeague = new Dictionary<Tournament, int>();
            expectedTeamsByLeague[fr.League(1)] = 20;
            expectedTeamsByLeague[fr.League(2)] = 20;
            expectedTeamsByLeague[fr.League(3)] = 18;
            expectedTeamsByLeague[fr.League(4)] = 6;
            CheckCupStructure(res, expectedTeamsByRound, expectedTeamsByLeague, expectedTeams);
        }

        /// <summary>
        /// Basic cup - Not constrained to draw all teams
        /// </summary>
        [TestMethod]
        public void TestCupCase2b()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(true);
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();

            List<RecoverTeams> pool = new List<RecoverTeams>();
            //TODO: Teams count is ignored (put null, -1 or Infinity)
            pool.Add(new RecoverTeams(fr.League(1).rounds[0], 20, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(2).rounds[0], 20, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(3).rounds[0], 18, RetrieveFlags.Best | RetrieveFlags.AllTeams));
            pool.Add(new RecoverTeams(fr.League(4).rounds[0], 30, RetrieveFlags.Best)); //Difference with Case2 : extra round allowed, so the 30 teams from N2 must be drawn
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
            };
            CupStructure structure = new CupStructure(false, true, constaints, pool, 1, false, null);
            CupStructureResult res = creator.CreateStructure(fr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 48, 64, 32, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(3).rounds[0].clubs);
            Dictionary<Tournament, int> expectedTeamsByLeague = new Dictionary<Tournament, int>();
            expectedTeamsByLeague[fr.League(1)] = 20;
            expectedTeamsByLeague[fr.League(2)] = 20;
            expectedTeamsByLeague[fr.League(3)] = 18;
            expectedTeamsByLeague[fr.League(4)] = 30;
            CheckCupStructure(res, expectedTeamsByRound, expectedTeamsByLeague, expectedTeams);
        }

        /// <summary>
        /// Test regional path cup - More than one game in the last round
        /// </summary>
        [TestMethod]
        public void TestCupCase3()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(true);
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(){new RecoverTeams(fr.League(3).rounds[0], 4, RetrieveFlags.Best)},
            };
            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(4).rounds[0], 30, RetrieveFlags.Best));
            CupStructure structure = new CupStructure(false, true, constaints, pool, 5, true, 2);
            CupStructureResult res = creator.CreateStructure(fr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 12, 10 };
            List<Club> expectedTeams = new List<Club>();
            Dictionary<Tournament, int> expectedTeamsByLeague = new Dictionary<Tournament, int>();
            expectedTeamsByLeague[fr.League(3)] = 4;
            expectedTeamsByLeague[fr.League(4)] = 12;
            CheckCupStructure(res, expectedTeamsByRound, expectedTeamsByLeague, expectedTeams);

        }

        /// <summary>
        /// Test regional path cup - More than one game in the last round.
        /// But there is too few teams on the pool so some of them are added to the second round
        /// </summary>
        [TestMethod]
        public void TestCupCase4()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator();
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(){new RecoverTeams(fr.League(2).rounds[0], 4, RetrieveFlags.Best)},
            };
            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(3).rounds[0], 18, RetrieveFlags.Best));
            CupStructure structure = new CupStructure(false, true, constaints, pool, 10, true, 2);
            CupStructureResult res = creator.CreateStructure(aFr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 4, 20};
            List<Club> expectedTeams = new List<Club>();
            Dictionary<Tournament, int> expectedTeamsByLeague = new Dictionary<Tournament, int>();
            expectedTeamsByLeague[fr.League(2)] = 4;
            expectedTeamsByLeague[fr.League(3)] = 18;
            CheckCupStructure(res, expectedTeamsByRound, expectedTeamsByLeague, expectedTeams);

        }


        /// <summary>
        /// Test regional path cup with multiple entries
        /// </summary>
        [TestMethod]
        public void TestCupCase5()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator();
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();

            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(){new RecoverTeams(fr.League(3).rounds[0], 6, RetrieveFlags.Best)},
                new List<RecoverTeams>(){new RecoverTeams(fr.League(2).rounds[0], 4, RetrieveFlags.Best)},
                new List<RecoverTeams>(),
            };
            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(4).rounds[0], 1000, RetrieveFlags.Best));
            CupStructure structure = new CupStructure(false, true, constaints, pool, 4, true, 4);
            CupStructureResult res = creator.CreateStructure(fr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 36, 24, 16, 8 };
            List<Club> expectedTeams = new List<Club>();
            Dictionary<Tournament, int> expectedTeamsByLeague = new Dictionary<Tournament, int>();
            expectedTeamsByLeague[fr.League(2)] = 4;
            expectedTeamsByLeague[fr.League(3)] = 6;
            CheckCupStructure(res, expectedTeamsByRound, expectedTeamsByLeague, expectedTeams);
        }

        /// <summary>
        /// League Cup : Case 1
        /// Default case, without changes
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase1()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator();
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();
            Tournament leagueCup = fr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(fr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 24, 12, 20, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            CheckCupStructure(res, expectedTeamsByRound, null, expectedTeams);
        }

        /// <summary>
        /// League Cup : Case 2
        /// N: 15 pro teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase2()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();
            Tournament l3 = fr.League(3);
            for (int i = 0; i < 15; i++)
            {
                l3.rounds[0].clubs[i].ChangeStatus(ClubStatus.Professional);
            }

            CupCreator creator = new CupCreator();
            Tournament leagueCup = fr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(fr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 22, 24, 12, 20, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            Dictionary<Tournament, int> expectedTeamsByLeague = new Dictionary<Tournament, int>();
            expectedTeamsByLeague[fr.League(1)] = 20;
            expectedTeamsByLeague[fr.League(2)] = 20;
            expectedTeamsByLeague[fr.League(3)] = 15;
            CheckCupStructure(res, expectedTeamsByRound, expectedTeamsByLeague, expectedTeams);
        }

        /// <summary>
        /// League Cup : Case 3
        /// N: 0 pro teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase3()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();
            Tournament l3 = fr.League(3);
            foreach (Club c in l3.rounds[0].clubs)
            {
                c.ChangeStatus(ClubStatus.SemiProfessional);
            }

            CupCreator creator = new CupCreator();
            Tournament leagueCup = fr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(fr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 16, 12, 20, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            CheckCupStructure(res, expectedTeamsByRound, null, expectedTeams);
        }

        /// <summary>
        /// League Cup : Case 4
        /// L1 : 12 international teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase4()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Association eur = Session.Instance.Game.kernel.String2Association("Europe");
            HashSet<Club> intClubs = GetContinentalClubs(eur);
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();
            Tournament l1 = fr.League(1);
            int i = 0;
            foreach (Club c in l1.rounds[0].clubs)
            {
                if (intClubs.Contains(c))
                {
                    i++;
                }
                else if (i < 15)
                {
                    eur.GetContinentalClubTournament(1).rounds[0].clubs.Add(c);
                    i++;
                }
            }

            CupCreator creator = new CupCreator();
            Tournament leagueCup = fr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(fr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 26, 16, 8, 4, 2, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            CheckCupStructure(res, expectedTeamsByRound, null, expectedTeams);
        }

        /// <summary>
        /// League Cup : Case 5
        /// L1 : 0 international teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase5()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();
            Association eur = Session.Instance.Game.kernel.String2Association("Europe");
            Tournament l1 = fr.League(1);

            foreach (Tournament t in eur.GetContinentalClubTournaments())
            {
                foreach (Round r in t.rounds)
                {
                    foreach (Club c in l1.Clubs())
                    {
                        if (r.clubs.Contains(c))
                        {
                            r.clubs.Remove(c);
                        }
                    }
                }
            }

            CupCreator creator = new CupCreator();
            Tournament leagueCup = fr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(fr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 24, 32, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            CheckCupStructure(res, expectedTeamsByRound, null, expectedTeams);
        }

        /// <summary>
        /// League Cup : Case 6
        /// L1 : 6 international teams
        /// L2 : 2 international teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase6()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Association fr = Session.Instance.Game.kernel.String2Country("France").GetCountryAssociation();
            Tournament l2 = fr.League(2);

            CupCreator creator = new CupCreator();
            Tournament leagueCup = fr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            Association eur = Session.Instance.Game.kernel.String2Association("Europe");
            eur.GetContinentalClubTournament(1).rounds[0].clubs.Add(l2.rounds[0].clubs[0]);
            eur.GetContinentalClubTournament(1).rounds[0].clubs.Add(l2.rounds[0].clubs[1]);

            CupStructureResult res = creator.CreateStructure(fr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 12, 16, 8, 4, 16, 16, 8, 4, 2 };
            List<Club> expectedTeams = new List<Club>();
            expectedTeams.AddRange(fr.League(1).rounds[0].clubs);
            expectedTeams.AddRange(fr.League(2).rounds[0].clubs);
            CheckCupStructure(res, expectedTeamsByRound, null, expectedTeams);
        }

        private HashSet<Club> GetContinentalClubs(Association a)
        {
            List<Club> intClubs = new List<Club>();
            foreach (Tournament t in a.GetContinentalClubTournaments())
            {
                foreach (Round r in t.rounds)
                {
                    intClubs.AddRange(r.clubs);
                }
            }
            return new HashSet<Club>(intClubs);
        }

    }
}
