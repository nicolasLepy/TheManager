using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm.Tournaments;
using tm;
using tm.Comparators;

namespace tests
{
    [TestClass]
    public class TestsComparators
    {

        [TestMethod]
        public void TestCoefficientComparator()
        {

            String item1 = "item1";
            String item2 = "item2";
            String item3 = "item3";
            String item4 = "item4";
            String item5 = "item5";
            Dictionary<String, float> coefficients = new Dictionary<string, float>
            {
                [item1] = 20.5f,
                [item2] = 10.5f,
                [item3] = 15f,
                [item4] = 25f,
                [item5] = 7f
            };

            List<String> items = new List<String> { item1, item2, item3, item4, item5 };

            List<String> sorted = new List<String>(items);
            sorted.Sort(new CoefficientComparator<String>(coefficients));

            List<String> expected = new List<String> { item4, item1, item3, item2, item5 };

            CollectionAssert.AreEqual(expected, sorted);

        }

        [TestMethod]
        public void TestAssociationCountComparator()
        {
            Association a1 = new Association(0, "A1", "", null, null, 0, false, null, false);
            Association a2 = new Association(1, "A2", "", null, null, 0, false, null, false);
            Association a3 = new Association(2, "A3", "", null, null, 0, false, null, false);
            Association a4 = new Association(3, "A4", "", null, null, 0, false, null, false);
            Association a5 = new Association(4, "A5", "", null, null, 0, false, null, false);
            Association a6 = new Association(5, "A6", "", null, null, 0, false, null, false);
            List<AssociationCount> ac = new List<AssociationCount>();
            ac.Add(new AssociationCount() { association= a1 , count = 2});
            ac.Add(new AssociationCount() { association = a2, count = 4 });
            ac.Add(new AssociationCount() { association = a3, count = 5 });
            ac.Add(new AssociationCount() { association = a4, count = 3 });
            ac.Add(new AssociationCount() { association = a5, count = 4 });
            ac.Add(new AssociationCount() { association = a6, count = 7 });

            ac.Sort(new AssociationCountComparator());

            Assert.IsTrue(ac[0].association == a6);
            Assert.IsTrue(ac[1].association == a3);
            Assert.IsTrue(ac[4].association == a4);
            Assert.IsTrue(ac[5].association == a1);
        }

        [TestMethod]
        public void TestGroupCompositionComparator()
        {
            List<GroupComposition> gc = new List<GroupComposition>();
            gc.Add(new GroupComposition() { id = 1, associationOccurences = 2, groupSize = 2 });
            gc.Add(new GroupComposition() { id = 2, associationOccurences = 2, groupSize = 3 });
            gc.Add(new GroupComposition() { id = 3, associationOccurences = 2, groupSize = 1 });
            gc.Add(new GroupComposition() { id = 4, associationOccurences = 1, groupSize = 1 });
            gc.Add(new GroupComposition() { id = 5, associationOccurences = 2, groupSize = 0 });
            gc.Add(new GroupComposition() { id = 6, associationOccurences = 0, groupSize = 4 });
            gc.Add(new GroupComposition() { id = 7, associationOccurences = 1, groupSize = 2 });

            gc.Sort(new GroupCompositionComparator());

            Assert.IsTrue(gc[0].id == 6);
            Assert.IsTrue(gc[1].id == 4);
            Assert.IsTrue(gc[2].id == 7);
            Assert.IsTrue(gc[3].id == 5);
            Assert.IsTrue(gc[4].id == 3);
            Assert.IsTrue(gc[5].id == 1);
            Assert.IsTrue(gc[6].id == 2);
        }
    }
}
