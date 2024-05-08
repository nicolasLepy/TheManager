using System.Diagnostics.CodeAnalysis;
using tm;
using tm.Tournaments;

namespace tests
{
    [TestClass]
    public class TestsUtils
    {
        /*
            IsBefore, IsBeforeWithoutYear
            PlayersByPosition
            ShuffleList
            Modulo
            Distance, Deg2Rad
            Points, Played, Wins, Loses, Draws, Gf, Ga, Difference
            Flag, Logo
            CreateGeographicClusters, GetClustersCapacity
            FormatMoney

            [Env needed]
            RuleIsRespected
            AdjustQualifications
         */

        [TestMethod]
        public void TestDaysNumberBetweenTwoDates()
        {
            DateTime a = new DateTime(2022, 2, 10);
            DateTime b = new DateTime(2022, 2, 20);
            int daysTest = Utils.DaysNumberBetweenTwoDates(a, b);
            Assert.AreEqual(daysTest, 10);
        }

        [TestMethod]
        public void TestCompareDates()
        {
            Assert.IsFalse(Utils.CompareDates(new DateTime(2020, 1, 1), new DateTime(2022, 1, 1)));
            Assert.IsTrue(Utils.CompareDates(new DateTime(2022, 1, 1), new DateTime(2022, 1, 1)));
            Assert.IsFalse(Utils.CompareDates(new DateTime(2022, 10, 1), new DateTime(2022, 1, 1)));
            Assert.IsTrue(Utils.CompareDates(new GameDay(25, false, 0, -3).ConvertToDateTime(2022), new DateTime(2022, 6, 22)));
            Assert.IsTrue(Utils.CompareDates(new GameDay(2, false, 0, -3).ConvertToDateTime(2022).AddDays(-14), new DateTime(2021, 12, 29)));
        }

        [TestMethod]
        public void TestCompareDatesWithoutYear()
        {
            Assert.IsTrue(Utils.CompareDatesWithoutYear(new DateTime(2020, 1, 1), new DateTime(2022, 1, 1)));
            Assert.IsTrue(Utils.CompareDatesWithoutYear(new DateTime(2022, 1, 1), new DateTime(2022, 1, 1)));
            Assert.IsFalse(Utils.CompareDatesWithoutYear(new DateTime(2022, 10, 1), new DateTime(2022, 1, 1)));
        }


    }
}