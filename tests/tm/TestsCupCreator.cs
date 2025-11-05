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
    public class TestsCupCreator : TheManagerTest
    {

        private void CheckLeagueCup(CupStructureResult res, List<int> expectedTeamsByRound)
        {
            int expectedRounds = expectedTeamsByRound.Count;
            Assert.AreEqual(res.roundsCount, expectedRounds);
            int teams = 0;
            for (int i = 0; i < expectedRounds; i++)
            {
                int newTeams = 0;
                foreach (RecoverTeams rt in res.structure[i])
                {
                    newTeams += rt.Source.RetrieveTeams(rt.Number, rt.Method, false, null).Count;
                }
                teams = teams + newTeams;
                Assert.AreEqual(res.teamsByRound[i], expectedTeamsByRound[i]);
                Assert.AreEqual(teams, expectedTeamsByRound[i]);
                teams = teams / 2; //For the next round
            }
        }
        
        /// <summary>
        /// Basic Cup - default case
        /// </summary>
        [TestMethod]
        public void TestCupCase1()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();

            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(1).rounds[0], 20, RecuperationMethod.Best | RecuperationMethod.AllTeams));
            pool.Add(new RecoverTeams(fr.League(2).rounds[0], 20, RecuperationMethod.Best | RecuperationMethod.AllTeams));
            pool.Add(new RecoverTeams(fr.League(3).rounds[0], 18, RecuperationMethod.Best | RecuperationMethod.AllTeams));
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>()
            };
            CupStructure structure = new CupStructure(false, true, constaints, pool, 1);
            CupStructureResult res = creator.CreateStructure(aFr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 52, 32, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// Basic cup - Not constrained to draw all teams
        /// </summary>
        [TestMethod]
        public void TestCupCase2()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();

            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(1).rounds[0], 20, RecuperationMethod.Best | RecuperationMethod.AllTeams));
            pool.Add(new RecoverTeams(fr.League(2).rounds[0], 20, RecuperationMethod.Best | RecuperationMethod.AllTeams));
            pool.Add(new RecoverTeams(fr.League(3).rounds[0], 18, RecuperationMethod.Best | RecuperationMethod.AllTeams));
            pool.Add(new RecoverTeams(fr.League(4).rounds[0], 30, RecuperationMethod.Best));
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>(),
                new List<RecoverTeams>()
            };
            CupStructure structure = new CupStructure(false, true, constaints, pool, 1);
            CupStructureResult res = creator.CreateStructure(aFr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 64, 32, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// Test regional path cup - More than one game in the last round
        /// </summary>
        public void TestCupCase3()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(){new RecoverTeams(fr.League(3).rounds[0], 4, RecuperationMethod.Best)},
            };
            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(4).rounds[0], 30, RecuperationMethod.Best));
            CupStructure structure = new CupStructure(false, true, constaints, pool, 5);
            CupStructureResult res = creator.CreateStructure(aFr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 20, 10 };
            CheckLeagueCup(res, expectedTeamsByRound);

        }

        /// <summary>
        /// Test regional path cup - More than one game in the last round.
        /// But there is too few teams on the pool so some of them are added to the second round
        /// </summary>
        public void TestCupCase4()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(){new RecoverTeams(fr.League(2).rounds[0], 4, RecuperationMethod.Best)},
            };
            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(3).rounds[0], 18, RecuperationMethod.Best));
            CupStructure structure = new CupStructure(false, true, constaints, pool, 10);
            CupStructureResult res = creator.CreateStructure(aFr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 4, 20};
            CheckLeagueCup(res, expectedTeamsByRound);

        }


        /// <summary>
        /// Test regional path cup with multiple entries
        /// </summary>
        public void TestCupCase5()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            List<List<RecoverTeams>> constaints = new List<List<RecoverTeams>>
            {
                new List<RecoverTeams>(),
                new List<RecoverTeams>(){new RecoverTeams(fr.League(3).rounds[0], 6, RecuperationMethod.Best)},
                new List<RecoverTeams>(){new RecoverTeams(fr.League(2).rounds[0], 4, RecuperationMethod.Best)},
                new List<RecoverTeams>(),
            };
            List<RecoverTeams> pool = new List<RecoverTeams>();
            pool.Add(new RecoverTeams(fr.League(4).rounds[0], 1000, RecuperationMethod.Best));
            CupStructure structure = new CupStructure(false, true, constaints, pool, 4);
            CupStructureResult res = creator.CreateStructure(aFr, structure);

            List<int> expectedTeamsByRound = new List<int>() { 36, 24, 16, 8 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// League Cup : Case 1
        /// Default case, without changes
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase1()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(aFr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 24, 12, 20, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// League Cup : Case 2
        /// N: 15 pro teams
        /// </summary>
        [TestMethod]
        public void TestLeagueCupCase2()
        {
            InitGame("ui", "database_france_light", new List<string>() { "France" });
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament l3 = aFr.League(3);
            for (int i = 0; i < 15; i++)
            {
                l3.rounds[0].clubs[i].ChangeStatus(ClubStatus.Professional);
            }

            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(aFr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 22, 24, 12, 20, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// League Cup : Case 3
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

            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(aFr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 16, 12, 20, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
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
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament l1 = aFr.League(1);
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

            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(aFr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 26, 16, 8, 4, 2, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
        }

        /// <summary>
        /// League Cup : Case 5
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

            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            CupStructureResult res = creator.CreateStructure(aFr, leagueCup.cupStructure);
            List<int> expectedTeamsByRound = new List<int>() { 24, 32, 16, 8, 4, 2 };
            CheckLeagueCup(res, expectedTeamsByRound);
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
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Association aFr = fr.GetCountryAssociation();
            Tournament l2 = aFr.League(2);

            CupCreator creator = new CupCreator(Session.Instance.Game.kernel);
            Tournament leagueCup = aFr.Cup(2);
            Assert.AreEqual(leagueCup.name, "Coupe de la Ligue");

            Association eur = Session.Instance.Game.kernel.String2Association("Europe");
            eur.GetContinentalClubTournament(1).rounds[0].clubs.Add(l2.rounds[0].clubs[0]);
            eur.GetContinentalClubTournament(1).rounds[0].clubs.Add(l2.rounds[0].clubs[1]);

            CupStructureResult res = creator.CreateStructure(aFr, leagueCup.cupStructure);
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
