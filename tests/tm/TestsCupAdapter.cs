using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm;
using tm.Algorithms;

namespace tests.tm
{
    [TestClass]
    public class TestsCupAdapter : TheManagerTest
    {

        private void CheckLeagueCup(CupAdapterResult res, List<int> expectedTeamsByRound)
        {
            for(int i = 0; i < res.removedRounds; i++)
            {
                expectedTeamsByRound.Insert(0, 0);
            }
            int expectedRounds = expectedTeamsByRound.Count;
            Assert.AreEqual(res.qualifications.Count, expectedRounds);
            int teams = 0;
            for(int i = 0; i < expectedRounds; i++)
            {
                int newTeams = 0;
                foreach (RecoverTeams rt in res.qualifications[i])
                {
                    newTeams += rt.Source.RetrieveTeams(rt.Number, rt.Flags, false, null).Count;
                }
                teams = teams + newTeams;
                Assert.AreEqual(teams, expectedTeamsByRound[i]);
                teams = teams / 2; //For the next round
            }
        }

        /// <summary>
        /// Case 1
        /// Default case, without changes
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase1()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupAdapter adapter = new CupAdapter();
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupAdapterResult res = adapter.AdaptLeagueCup(leagueCup);
            List<int> expectedTeamsByRound = new List<int>() { 24, 12, 20, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// Case 2
        /// N: 15 pro teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase2()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament l3 = aFr.League(3);
            for(int i = 0; i < 15; i++)
            {
                l3.rounds[0].clubs[i].ChangeStatus(ClubStatus.Professional);
            }

            CupAdapter adapter = new CupAdapter();
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupAdapterResult res = adapter.AdaptLeagueCup(leagueCup);
            List<int> expectedTeamsByRound = new List<int>() { 22, 24, 12, 20, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// Case 3
        /// N: 0 pro teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase3()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament l3 = aFr.League(3);
            foreach (Club c in l3.rounds[0].clubs)
            {
                c.ChangeStatus(ClubStatus.SemiProfessional);
            }

            CupAdapter adapter = new CupAdapter();
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupAdapterResult res = adapter.AdaptLeagueCup(leagueCup);
            List<int> expectedTeamsByRound = new List<int>() { 16, 12, 20, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// Case 4
        /// L1 : 12 international teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase4()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Association eur = Session.Instance.Game.kernel.String2Association("Europe");
            HashSet<Club> intClubs = GetContinentalClubs(eur);
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament l1 = aFr.League(1);
            int i = 0;
            foreach(Club c in l1.rounds[0].clubs)
            {
                if(intClubs.Contains(c))
                {
                    i++;
                }
                else if(i < 15)
                {
                    eur.GetContinentalClubTournament(1).rounds[0].clubs.Add(c);
                    i++;
                }
            }

            CupAdapter adapter = new CupAdapter();
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupAdapterResult res = adapter.AdaptLeagueCup(leagueCup);
            List<int> expectedTeamsByRound = new List<int>() { 26, 16, 8, 4, 2, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// Case 5
        /// L1 : 0 international teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase5()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Association eur = Session.Instance.Game.kernel.String2Association("Europe");
            Tournament l1 = aFr.League(1);

            foreach (Tournament t in eur.GetContinentalClubTournaments())
            {
                foreach(Round r in t.rounds)
                {
                    foreach(Club c in l1.Clubs())
                    {
                        if (r.clubs.Contains(c))
                        {
                            r.clubs.Remove(c);
                        }
                    }
                }
            }

            CupAdapter adapter = new CupAdapter();
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupAdapterResult res = adapter.AdaptLeagueCup(leagueCup);
            List<int> expectedTeamsByRound = new List<int>() { 24, 32, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// Case 6
        /// L1 : 6 international teams
        /// L2 : 2 international teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase6()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament l2 = aFr.League(2);

            CupAdapter adapter = new CupAdapter();
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            Association eur = Session.Instance.Game.kernel.String2Association("Europe");
            eur.GetContinentalClubTournament(1).rounds[0].clubs.Add(l2.rounds[0].clubs[0]);
            eur.GetContinentalClubTournament(1).rounds[0].clubs.Add(l2.rounds[0].clubs[1]);

            CupAdapterResult res = adapter.AdaptLeagueCup(leagueCup);
            List<int> expectedTeamsByRound = new List<int>() { 12, 16, 8, 4, 16, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
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
