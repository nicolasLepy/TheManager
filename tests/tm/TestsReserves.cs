using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm;

namespace tests.tm
{
    [TestClass]
    public class TestsReserves : TheManagerTest
    {
        [TestMethod]
        public void TestReserves()
        {
            InitGame("database_france_nat", new List<Country>());
            for (int i = 0; i < 100; i++)
            {
                Session.Instance.Game.NextDay();
                Session.Instance.Game.UpdateTournaments();
            }
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            BackupLeagues(fr, "reserves");
        }
    }
}
