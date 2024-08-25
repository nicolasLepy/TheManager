using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using tests.tm;
using tm;
using tm.persistance;

namespace tests
{

    [TestClass]
    public class TestsWorkflow : TheManagerTest
    {
        //dotnet test --collect:"XPlat Code Coverage"
        //Results are stored into TestResults folder
        //Install tool for html report
        ///dotnet tool install -g dotnet-reportgenerator-globaltool
        // reportgenerator -reports:"TheManagerTests\TestResults\ffe9acf3-b390-4734-aa2a-26f41f6a445a\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html


        [TestMethod]
        public void TestSeasonsNational()
        {
            InitGame("database_france_nat", new List<string>() { "France"});
            int years = 2;
            for (int i = 0; i < 365*years; i++)
            {
                Session.Instance.Game.NextDay();
                Session.Instance.Game.UpdateTournaments();
            }
        }

        [TestMethod]
        public void TestSeasonsLight() //About 3 minutes / season
        {
            InitGame("database_france_light", null);

            int years = 2;
            for(int i = 0; i < 365*years; i++)
            {
                Session.Instance.Game.NextDay();
                Session.Instance.Game.UpdateTournaments();
            }

            Session.Instance.Game.Save("D:\\Projets\\TheManager\\ui\\bin\\Debug\\test_big.csave");

            //TODO: Check everything are correct : league structure doesn't changed, cup with right teams count
        }

        [TestMethod]
        public void TestSeasonsLightFast()
        {
            InitGame("database_france_light", new List<string>() { "France"});

            int years = 10;
            for (int i = 0; i < 365 * years; i++)
            {
                Session.Instance.Game.NextDay();
                Session.Instance.Game.UpdateTournaments();
            }

            //TODO: Check everything are correct : league structure doesn't changed, cup with right teams count
        }

    }
}
