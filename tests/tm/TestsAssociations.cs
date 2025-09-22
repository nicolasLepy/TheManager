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

        private void CheckClubs(Association az, int expectedTotalClubs)
        {
            Dictionary<Club, int> occurences = new Dictionary<Club, int>();
            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                if((c as NationalTeam) == null && c.Association().IsDirectConnected(az))
                {
                    occurences[c] = 0;
                }
            }
            foreach(Tournament t in az.Leagues())
            {
                Round r = t.rounds[0];
                foreach(Club c in r.clubs)
                {
                    occurences[c]++;
                }
            }
            Assert.AreEqual(expectedTotalClubs, occurences.Count);
            foreach(KeyValuePair<Club, int> kvp in occurences)
            {
                Assert.AreEqual(1, kvp.Value);
            }

        }

        private void CheckRegionalLeague(Association az, Tournament t, List<Association> associations, Dictionary<Association, int> expectedTeams)
        {
            GroupsRound r = t.rounds[0] as GroupsRound;
            Assert.IsNotNull(r);
            foreach(Association a in associations)
            {
                int teams = 0;
                List<int> groups = r.GetGroupsFromAssociation(a);
                foreach(int g in groups)
                {
                    List<Club> cGroups = r.groups[g];
                    foreach(Club c in cGroups)
                    {
                        Assert.IsTrue(a.ContainsAssociation(c.Association()));
                        teams++;
                    }
                }
                if(expectedTeams.ContainsKey(a))
                {
                    Assert.AreEqual(teams, expectedTeams[a]);
                }

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

                Dictionary<Association, int> aTeams3 = new Dictionary<Association, int>
                {
                    [aLevel0[2]] = 8
                };

                Dictionary<Association, int> aTeams4 = new Dictionary<Association, int>
                {
                    [aLevel0[2]] = 16
                };

                Dictionary<Association, int> aTeams5 = new Dictionary<Association, int>
                {
                    [aLevel1[0]] = 8,
                    [aLevel1[1]] = 8
                };


                CheckRegionalLeague(aAz, t3, aLevel0, aTeams3);
                CheckRegionalLeague(aAz, t4, aLevel0, aTeams4);
                CheckRegionalLeague(aAz, t5, aLevel1, aTeams5);
                CheckRegionalLeague(aAz, t6, aLevel1, new());

                //Check each club (and eventual reserve) have a league associated, and no doublons
                CheckClubs(aAz, 108);
            }

        }

        [TestMethod]
        public void TestLeagueStructureConservedFranceExtended()
        {
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
                Assert.AreEqual(t5.rounds[0].clubs.Count, 171);

                //Check each regional league have teams of its association, and the correct number. Number of teams in the last level can vary
                Tournament t6 = aFr.League(6);
                Tournament t7 = aFr.League(7);

                Dictionary<Association, int> aTeams6 = new Dictionary<Association, int>
                {
                    [aLevel0[0]] = 35,
                    [aLevel0[1]] = 47,
                    [aLevel0[2]] = 36
                };

                CheckRegionalLeague(aFr, t6, aLevel0, aTeams6);
                CheckRegionalLeague(aFr, t7, aLevel1, new());

                //Check each club (and eventual reserve) have a league associated, and no doublons
                CheckClubs(aFr, 1444);
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

    }
}
