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

        private static int TEST_YEARS = 2;

        private void CheckAssociationLeagueSystem(Association association, Dictionary<Club, int> occurences)
        {
            foreach(Tournament t in association.Leagues())
            {
                Round r = t.rounds[0];
                foreach(Club c in r.clubs)
                {
                    occurences[c]++;
                    Assert.IsTrue(c.Association().IsDirectConnected(association));
                }
            }
            foreach(Association a in association.associations)
            {
                CheckAssociationLeagueSystem(a, occurences);
            }
        }

        private void CheckLeagueSystem(Association az, int expectedTotalClubs)
        {
            Dictionary<Club, int> occurences = new Dictionary<Club, int>();
            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                if((c as NationalTeam) == null && c.Association().IsDirectConnected(az))
                {
                    occurences[c] = 0;
                }
            }

            CheckAssociationLeagueSystem(az, occurences);

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
            InitGame("database_unitttests", new List<string>() { "France"});
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
                Tournament t3 = aAz.League(3);
                Tournament t4 = aAz.League(4);
                Tournament t5 = aAz.League(5);
                Tournament t6 = aAz.League(6);

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
                CheckLeagueSystem(aAz, 108);
            }

        }

        [TestMethod]
        public void TestLeagueStructureConservedFranceExtended()
        {
            //Objectif : <30 sec [28/05/2022] [debug]
            InitGame("database_france_nat", new List<string>() { "France"});
            for (int y = 0; y < TEST_YEARS; y++)
            {
                for (int i = 0; i < 365; i++)
                {
                    Session.Instance.Game.NextDay();
                    Session.Instance.Game.UpdateTournaments();
                }

                Country fr = Session.Instance.Game.kernel.String2Country("France");
                Association aFr = fr.GetCountryAssociation();
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
                GroupsRound r40 = t4.rounds[0] as GroupsRound;
                Assert.AreEqual(r40.groupsCount, 4);
                Assert.AreEqual(r40.Ranking(0).Count, 16);
                Assert.AreEqual(r40.Ranking(1).Count, 16);
                Assert.AreEqual(r40.Ranking(2).Count, 16);
                Assert.AreEqual(r40.Ranking(3).Count, 16);

                Tournament t5 = aFr.League(5);
                Assert.AreEqual(t5.rounds[0].clubs.Count, 181);

                //Check each regional league have teams of its association, and the correct number. Number of teams in the last level can vary
                Association aReg1 = aFr.associations[0]; //BFC
                Association aReg2 = aFr.associations[1]; //GE
                Association aReg3 = aFr.associations[2]; //HDF
                Assert.AreEqual("Bourgogne Franche-Comte", aReg1.name);
                Assert.AreEqual("Grand-Est", aReg2.name);
                Assert.AreEqual("Hauts de France", aReg3.name);

                Tournament t6_a = aReg1.League(1);
                Tournament t6_b = aReg2.League(1);
                Tournament t6_c = aReg3.League(1);

                Tournament t7_a = aReg1.League(2);
                Tournament t7_b = aReg2.League(2);
                Tournament t7_c = aReg3.League(2);

                CheckRegionalLeague(t6_a, aReg1, 35);
                CheckRegionalLeague(t6_b, aReg2, 47);
                CheckRegionalLeague(t6_c, aReg3, 36);
                CheckRegionalLeague(t7_a, aReg1, -1);
                CheckRegionalLeague(t7_b, aReg2, -1);
                CheckRegionalLeague(t7_c, aReg3, -1);

                //Check each club (and eventual reserve) have a league associated, and no doublons
                CheckLeagueSystem(aFr, 1404);
            }
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
            expectedId = new List<int>() { 1, 2, 3, 4, 5, 6 };
            CheckTournaments(expectedId, ta);

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
            expectedId = new List<int>() { 5 };  //BFC
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

            Assert.AreEqual(world.GetLevelOfAssociation(fr, 0), -1);
            Assert.AreEqual(world.GetLevelOfAssociation(bfc, 0), -1);
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
