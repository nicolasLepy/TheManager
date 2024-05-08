using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using tm;
using tm.persistance.nhibernate;
using tm.persistance.sqlite;

namespace tests
{
    [TestClass]
    public class TestsSerialization
    {
        public void save(String csaveFile)
        {
            Game game = new Game();
            Session.Instance.Game = game;
            game.Load(csaveFile);
            game.kernel.Resume();

            var timeEFCore = System.Diagnostics.Stopwatch.StartNew();
            EfCoreSqLiteProvider sqlProvider = new EfCoreSqLiteProvider("D:\\Projets\\TheManager\\ui\\bin\\Debug\\test.db");
            sqlProvider.Save(game);
            timeEFCore.Stop();
            Console.WriteLine(String.Format("[Save] EFCore : {0} ms", timeEFCore.ElapsedMilliseconds));

            game.Save(csaveFile);
        }

        public void load(String csaveFile)
        {
            Game game = new Game();
            Session.Instance.Game = game;
            game.Load(csaveFile);

            var timeDataContract = System.Diagnostics.Stopwatch.StartNew();
            EfCoreSqLiteProvider provider = new EfCoreSqLiteProvider("D:\\Projets\\TheManager\\ui\\bin\\Debug\\test.db");
            game = provider.Load();
            Console.WriteLine(String.Format("[Load] EFCore : {0} ms", timeDataContract.ElapsedMilliseconds));
            game.kernel.Resume();
            // Console.WriteLine(String.Format("[Tournaments] {0}", game.kernel.Competitions.Count)); NEED WORLD
        }

        [TestMethod]
        public void TestSave()
        {
            save("D:\\Projets\\TheManager\\ui\\bin\\Debug\\tb.csave");
        }

        [TestMethod]
        public void TestLoad()
        {
            load("D:\\Projets\\TheManager\\ui\\bin\\Debug\\tb.csave");
        }

        [TestMethod]
        public void TestSaveLight()
        {
            save("D:\\Projets\\TheManager\\ui\\bin\\Debug\\test_light.csave");
        }

        [TestMethod]
        public void TestLoadLight()
        {
            load("D:\\Projets\\TheManager\\ui\\bin\\Debug\\test_light.csave");
        }
    }
}