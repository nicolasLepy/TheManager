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

        private void CheckClubs(Country az)
        {
            Dictionary<Club, int> occurences = new Dictionary<Club, int>();
            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                if((c as NationalTeam) == null && c.Country() == az)
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
            Assert.AreEqual(108, occurences.Count);
            foreach(KeyValuePair<Club, int> kvp in occurences)
            {
                Assert.AreEqual(1, kvp.Value);
            }

        }

        private void CheckRegionalLeague(Country az, Tournament t, List<Association> associations, Dictionary<Association, int> expectedTeams)
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
        public void TestLeagueStructureConserved()
        {
            InitGame("database_unitttests", new List<string>());
            for(int y = 0; y < TEST_YEARS; y++)
            {
                for (int i = 0; i < 365; i++)
                {
                    Session.Instance.Game.NextDay();
                    Session.Instance.Game.UpdateTournaments();
                }

                Country az = Session.Instance.Game.kernel.String2Country("Azerbaïdjan");
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
                Tournament t1 = az.League(1);

                int numberOfTeams = t1.rounds[0].clubs.Count;
                Assert.AreEqual(12, numberOfTeams);

                Tournament t2 = az.League(2);
                numberOfTeams = t2.rounds[0].clubs.Count;
                Assert.AreEqual(numberOfTeams, 24);
                GroupsRound r20 = t2.rounds[0] as GroupsRound;
                Assert.AreEqual(r20.groupsCount, 3);
                Assert.AreEqual(r20.Ranking(0).Count, 8);
                Assert.AreEqual(r20.Ranking(1).Count, 8);
                Assert.AreEqual(r20.Ranking(2).Count, 8);

                //Check each regional league have teams of its association, and the correct number. Number of teams in the last level can vary
                Tournament t3 = az.League(3);
                Tournament t4 = az.League(4);
                Tournament t5 = az.League(5);
                Tournament t6 = az.League(6);

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


                CheckRegionalLeague(az, t3, aLevel0, aTeams3);
                CheckRegionalLeague(az, t4, aLevel0, aTeams4);
                CheckRegionalLeague(az, t5, aLevel1, aTeams5);
                CheckRegionalLeague(az, t6, aLevel1, new());

                //Check each club (and eventual reserve) have a league associated, and no doublons
                CheckClubs(az);
            }

        }
    }
}
