using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm;

namespace tests.tm
{
    [TestClass]
    public class TestsRetrogradations : TheManagerTest
    {
        [TestMethod]
        public void TestRetrogradation()
        {
            InitGame("database_france_nat", new List<string>());
        }
    }
}
