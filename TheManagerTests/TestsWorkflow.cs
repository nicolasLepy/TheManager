using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager.Tournaments;
using TheManager;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace TheManagerTests
{
    [TestClass]
    public class TestsWorkflow
    {
        //dotnet test --collect:"XPlat Code Coverage"
        //Results are stored into TestResults folder
        //Install tool for html report
        ///dotnet tool install -g dotnet-reportgenerator-globaltool
        // reportgenerator -reports:"TheManagerTests\TestResults\ffe9acf3-b390-4734-aa2a-26f41f6a445a\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

        private void InitGame(string dataset, bool keepOnlyFrance)
        {
            Game partie = new Game();
            Session.Instance.Game = partie;
            Kernel g = partie.kernel;
            Utils.dataFolderName = "data\\" + dataset;
            DatabaseLoader cbdd = new DatabaseLoader(g);

            cbdd.LoadLanguages();
            cbdd.LoadWorld();
            cbdd.LoadCalendars();
            cbdd.LoadCities();
            cbdd.LoadStadiums();
            cbdd.LoadClubs();
            cbdd.LoadTournaments();
            cbdd.LoadInternationalDates();
            cbdd.LoadPlayers();
            cbdd.LoadManagers();
            cbdd.InitTeams();
            cbdd.InitPlayers();
            cbdd.InitTournaments();
            cbdd.LoadMedias();
            cbdd.LoadGamesComments();
            cbdd.LoadRules();
            cbdd.GenerateNationalCup();
            cbdd.CreateRegionalPathForCups();
            cbdd.LoadArchives();
            Country fr = Session.Instance.Game.kernel.String2Country("France");

            if(keepOnlyFrance)
            {
                foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
                {
                    if (c.isChampionship && Session.Instance.Game.kernel.LocalisationTournament(c) != fr)
                        c.DisableTournament();
                }
            }

            Club club = Session.Instance.Game.kernel.Clubs[70];
            Session.Instance.Game.club = club as CityClub;
            Session.Instance.Game.SetBeginDate(Session.Instance.Game.GetBeginDate(club.Country()));
            Manager manager = new Manager("Name", "Name", 70, new DateTime(1980, 1, 1), fr);
            Session.Instance.Game.club.ChangeManager(manager);
            Session.Instance.Game.options.simulateGames = true;
        }

        [TestMethod]
        public void TestSeasonsNational()
        {
            InitGame("database_france_nat", true);
            int years = 2;
            for (int i = 0; i < 365*years; i++)
            {
                Session.Instance.Game.NextDay();
                Session.Instance.Game.UpdateTournaments();
            }

        }

        [TestMethod]
        public void TestSeasonsLight() //About 15 minutes / season
        {
            InitGame("database_france_light", false);

            int years = 2;
            for(int i = 0; i < 365*years; i++)
            {
                Session.Instance.Game.NextDay();
                Session.Instance.Game.UpdateTournaments();
            }

            //TODO: Check everything are correct : league structure doesn't changed, cup with right teams count
        }

    }
}
