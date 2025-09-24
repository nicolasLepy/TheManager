using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm;

namespace tests.tm
{
    [TestClass]
    public class TestsQualifications : TheManagerTest
    {

        [TestMethod]
        public void TestIsAbove()
        {
            Association world = MakeBasicStructure();
            Association fr = world.associations[0].associations[0];
            Assert.AreEqual(fr.Id, 3);

            Association es = world.associations[0].associations[1];
            Assert.AreEqual(es.Id, 4);

            Association bfc = fr.associations[0];
            Assert.AreEqual(bfc.Id, 5);

            Association nord = fr.associations[1];
            Assert.AreEqual(nord.Id, 6);

            QualificationTarget l1 = new QualificationTournament(fr.League(1));
            QualificationTarget l2 = new QualificationTournament(fr.League(2));
            QualificationTarget toR1 = new QualificationExcludeLeagueSystem(fr);
            QualificationTarget r1 = new QualificationTournament(bfc.League(1));
            QualificationTarget r2 = new QualificationTournament(bfc.League(2));
            QualificationTarget r3 = new QualificationTournament(bfc.League(3));
            QualificationTarget r1_nord = new QualificationTournament(nord.League(1));
            QualificationTarget r2_nord = new QualificationTournament(nord.League(2));
            QualificationTarget es1 = new QualificationTournament(es.League(1));
            QualificationTarget es2 = new QualificationTournament(es.League(2));

            Assert.IsFalse(l1.IsAbove(l1));
            Assert.IsTrue(l1.IsAbove(l2));
            Assert.IsTrue(l1.IsAbove(toR1));
            Assert.IsTrue(l1.IsAbove(r1));
            Assert.IsTrue(l1.IsAbove(r2));
            Assert.IsTrue(l1.IsAbove(r3));
            Assert.IsFalse(l1.IsAbove(es1));
            Assert.IsTrue(l1.IsAbove(es2));

            Assert.IsFalse(l2.IsAbove((l1)));
            Assert.IsFalse(l2.IsAbove((l2)));
            Assert.IsTrue(l2.IsAbove(toR1));
            Assert.IsTrue(l2.IsAbove((r1)));
            Assert.IsTrue(l2.IsAbove((r2)));
            Assert.IsTrue(l2.IsAbove((r3)));
            Assert.IsFalse(l2.IsAbove((es1)));
            Assert.IsFalse(l2.IsAbove((es2)));

            Assert.IsFalse(r1.IsAbove(l1));
            Assert.IsFalse(r1.IsAbove(l2));
            Assert.IsFalse(r1.IsAbove(toR1));
            Assert.IsFalse(r1.IsAbove(r1));
            Assert.IsTrue(r1.IsAbove(r2));
            Assert.IsTrue(r1.IsAbove(r3));
            Assert.IsFalse(r1.IsAbove(es1));
            Assert.IsFalse(r1.IsAbove(es2));
            Assert.IsFalse(r1.IsAbove(r1_nord));
            Assert.IsTrue(r1.IsAbove(r2_nord));

            Assert.IsFalse(r2.IsAbove(l1));
            Assert.IsFalse(r2.IsAbove(l2));
            Assert.IsFalse(r2.IsAbove(toR1));
            Assert.IsFalse(r2.IsAbove(r1));
            Assert.IsFalse(r2.IsAbove(r2));
            Assert.IsTrue(r2.IsAbove(r3));
            Assert.IsFalse(r2.IsAbove(es1));
            Assert.IsFalse(r2.IsAbove(es2));
            Assert.IsFalse(r2.IsAbove(r1_nord));
            Assert.IsFalse(r2.IsAbove(r2_nord));

            Assert.IsFalse(r3.IsAbove(l1));
            Assert.IsFalse(r3.IsAbove(l2));
            Assert.IsFalse(r3.IsAbove(toR1));
            Assert.IsFalse(r3.IsAbove(r1));
            Assert.IsFalse(r3.IsAbove(r2));
            Assert.IsFalse(r3.IsAbove(r3));
            Assert.IsFalse(r3.IsAbove(es1));
            Assert.IsFalse(r3.IsAbove(es2));
            Assert.IsFalse(r3.IsAbove(r1_nord));
            Assert.IsFalse(r3.IsAbove(r2_nord));

            Assert.IsFalse(es1.IsAbove(l1));
            Assert.IsTrue(es1.IsAbove(l2));
            Assert.IsTrue(es1.IsAbove(toR1));
            Assert.IsTrue(es1.IsAbove(r1));
            Assert.IsTrue(es1.IsAbove(r2));
            Assert.IsTrue(es1.IsAbove(r3));
            Assert.IsFalse(es1.IsAbove(es1));
            Assert.IsTrue(es1.IsAbove(es2));

            Assert.IsFalse(es2.IsAbove(l1));
            Assert.IsFalse(es2.IsAbove(l2));
            Assert.IsTrue(es2.IsAbove(toR1));
            Assert.IsTrue(es2.IsAbove(r1));
            Assert.IsTrue(es2.IsAbove(r2));
            Assert.IsTrue(es2.IsAbove(r3));
            Assert.IsFalse(es2.IsAbove(es1));
            Assert.IsFalse(es2.IsAbove(es2));

            Assert.IsFalse(toR1.IsAbove(l1));
            Assert.IsFalse(toR1.IsAbove(l2));
            Assert.IsFalse(toR1.IsAbove(toR1));
            Assert.IsFalse(toR1.IsAbove(r1));
            Assert.IsTrue(toR1.IsAbove(r2));
            Assert.IsTrue(toR1.IsAbove(r3));
            Assert.IsFalse(toR1.IsAbove(es1));
            Assert.IsFalse(toR1.IsAbove(es2));
        }

        [TestMethod]
        public void TestIsBelow()
        {
            Association world = MakeBasicStructure();
            Association fr = world.associations[0].associations[0];
            Assert.AreEqual(fr.Id, 3);

            Association es = world.associations[0].associations[1];
            Assert.AreEqual(es.Id, 4);

            Association bfc = fr.associations[0];
            Assert.AreEqual(bfc.Id, 5);

            Association nord = fr.associations[1];
            Assert.AreEqual(nord.Id, 6);

            QualificationTarget l1 = new QualificationTournament(fr.League(1));
            QualificationTarget l2 = new QualificationTournament(fr.League(2));
            QualificationTarget toR1 = new QualificationExcludeLeagueSystem(fr);
            QualificationTarget r1 = new QualificationTournament(bfc.League(1));
            QualificationTarget r2 = new QualificationTournament(bfc.League(2));
            QualificationTarget r3 = new QualificationTournament(bfc.League(3));
            QualificationTarget r1_nord = new QualificationTournament(nord.League(1));
            QualificationTarget r2_nord = new QualificationTournament(nord.League(2));
            QualificationTarget es1 = new QualificationTournament(es.League(1));
            QualificationTarget es2 = new QualificationTournament(es.League(2));

            Assert.IsFalse(l1.IsBelow(l1));
            Assert.IsFalse(l1.IsBelow(l2));
            Assert.IsFalse(l1.IsBelow(toR1));
            Assert.IsFalse(l1.IsBelow(r1));
            Assert.IsFalse(l1.IsBelow(r2));
            Assert.IsFalse(l1.IsBelow(r3));
            Assert.IsFalse(l1.IsBelow(es1));
            Assert.IsFalse(l1.IsBelow(es2));

            Assert.IsTrue(l2.IsBelow((l1)));
            Assert.IsFalse(l2.IsBelow((l2)));
            Assert.IsFalse(l2.IsBelow(toR1));
            Assert.IsFalse(l2.IsBelow((r1)));
            Assert.IsFalse(l2.IsBelow((r2)));
            Assert.IsFalse(l2.IsBelow((r3)));
            Assert.IsTrue(l2.IsBelow((es1)));
            Assert.IsFalse(l2.IsBelow((es2)));

            Assert.IsTrue(r1.IsBelow(l1));
            Assert.IsTrue(r1.IsBelow(l2));
            Assert.IsFalse(r1.IsBelow(toR1));
            Assert.IsFalse(r1.IsBelow(r1));
            Assert.IsFalse(r1.IsBelow(r2));
            Assert.IsFalse(r1.IsBelow(r3));
            Assert.IsTrue(r1.IsBelow(es1));
            Assert.IsTrue(r1.IsBelow(es2));
            Assert.IsFalse(r1.IsBelow(r1_nord));
            Assert.IsFalse(r1.IsBelow(r2_nord));

            Assert.IsTrue(r2.IsBelow(l1));
            Assert.IsTrue(r2.IsBelow(l2));
            Assert.IsTrue(r2.IsBelow(toR1));
            Assert.IsTrue(r2.IsBelow(r1));
            Assert.IsFalse(r2.IsBelow(r2));
            Assert.IsFalse(r2.IsBelow(r3));
            Assert.IsTrue(r2.IsBelow(es1));
            Assert.IsTrue(r2.IsBelow(es2));
            Assert.IsTrue(r2.IsBelow(r1_nord));
            Assert.IsFalse(r2.IsBelow(r2_nord));

            Assert.IsTrue(r3.IsBelow(l1));
            Assert.IsTrue(r3.IsBelow(l2));
            Assert.IsTrue(r3.IsBelow(toR1));
            Assert.IsTrue(r3.IsBelow(r1));
            Assert.IsTrue(r3.IsBelow(r2));
            Assert.IsFalse(r3.IsBelow(r3));
            Assert.IsTrue(r3.IsBelow(es1));
            Assert.IsTrue(r3.IsBelow(es2));
            Assert.IsTrue(r3.IsBelow(r1_nord));
            Assert.IsTrue(r3.IsBelow(r2_nord));

            Assert.IsFalse(es1.IsBelow(l1));
            Assert.IsFalse(es1.IsBelow(l2));
            Assert.IsFalse(es1.IsBelow(toR1));
            Assert.IsFalse(es1.IsBelow(r1));
            Assert.IsFalse(es1.IsBelow(r2));
            Assert.IsFalse(es1.IsBelow(r3));
            Assert.IsFalse(es1.IsBelow(es1));
            Assert.IsFalse(es1.IsBelow(es2));

            Assert.IsTrue(es2.IsBelow(l1));
            Assert.IsFalse(es2.IsBelow(l2));
            Assert.IsFalse(es2.IsBelow(toR1));
            Assert.IsFalse(es2.IsBelow(r1));
            Assert.IsFalse(es2.IsBelow(r2));
            Assert.IsFalse(es2.IsBelow(r3));
            Assert.IsTrue(es2.IsBelow(es1));
            Assert.IsFalse(es2.IsBelow(es2));

            Assert.IsTrue(toR1.IsBelow(l1));
            Assert.IsTrue(toR1.IsBelow(l2));
            Assert.IsFalse(toR1.IsBelow(toR1));
            Assert.IsFalse(toR1.IsBelow(r1));
            Assert.IsFalse(toR1.IsBelow(r2));
            Assert.IsFalse(toR1.IsBelow(r3));
            Assert.IsTrue(toR1.IsBelow(es1));
            Assert.IsTrue(toR1.IsBelow(es2));
        }

        [TestMethod]
        public void TestIsSameLevel()
        {
            Association world = MakeBasicStructure();
            Association fr = world.associations[0].associations[0];
            Assert.AreEqual(fr.Id, 3);

            Association es = world.associations[0].associations[1];
            Assert.AreEqual(es.Id, 4);

            Association bfc = fr.associations[0];
            Assert.AreEqual(bfc.Id, 5);

            Association nord = fr.associations[1];
            Assert.AreEqual(nord.Id, 6);

            QualificationTarget l1 = new QualificationTournament(fr.League(1));
            QualificationTarget l2 = new QualificationTournament(fr.League(2));
            QualificationTarget toR1 = new QualificationExcludeLeagueSystem(fr);
            QualificationTarget r1 = new QualificationTournament(bfc.League(1));
            QualificationTarget r2 = new QualificationTournament(bfc.League(2));
            QualificationTarget r3 = new QualificationTournament(bfc.League(3));
            QualificationTarget r1_nord = new QualificationTournament(nord.League(1));
            QualificationTarget r2_nord = new QualificationTournament(nord.League(2));
            QualificationTarget es1 = new QualificationTournament(es.League(1));
            QualificationTarget es2 = new QualificationTournament(es.League(2));

            Assert.IsTrue(l1.SameLevel(l1));
            Assert.IsFalse(l1.SameLevel(l2));
            Assert.IsFalse(l1.SameLevel(toR1));
            Assert.IsFalse(l1.SameLevel(r1));
            Assert.IsFalse(l1.SameLevel(r2));
            Assert.IsFalse(l1.SameLevel(r3));
            Assert.IsTrue(l1.SameLevel(es1));
            Assert.IsFalse(l1.SameLevel(es2));

            Assert.IsFalse(l2.SameLevel((l1)));
            Assert.IsTrue(l2.SameLevel((l2)));
            Assert.IsFalse(l2.SameLevel(toR1));
            Assert.IsFalse(l2.SameLevel((r1)));
            Assert.IsFalse(l2.SameLevel((r2)));
            Assert.IsFalse(l2.SameLevel((r3)));
            Assert.IsFalse(l2.SameLevel((es1)));
            Assert.IsTrue(l2.SameLevel((es2)));

            Assert.IsFalse(r1.SameLevel(l1));
            Assert.IsFalse(r1.SameLevel(l2));
            Assert.IsTrue(r1.SameLevel(toR1));
            Assert.IsTrue(r1.SameLevel(r1));
            Assert.IsFalse(r1.SameLevel(r2));
            Assert.IsFalse(r1.SameLevel(r3));
            Assert.IsFalse(r1.SameLevel(es1));
            Assert.IsFalse(r1.SameLevel(es2));
            Assert.IsTrue(r1.SameLevel(r1_nord));
            Assert.IsFalse(r1.SameLevel(r2_nord));

            Assert.IsFalse(r2.SameLevel(l1));
            Assert.IsFalse(r2.SameLevel(l2));
            Assert.IsFalse(r2.SameLevel(toR1));
            Assert.IsFalse(r2.SameLevel(r1));
            Assert.IsTrue(r2.SameLevel(r2));
            Assert.IsFalse(r2.SameLevel(r3));
            Assert.IsFalse(r2.SameLevel(es1));
            Assert.IsFalse(r2.SameLevel(es2));
            Assert.IsFalse(r2.SameLevel(r1_nord));
            Assert.IsTrue(r2.SameLevel(r2_nord));

            Assert.IsFalse(r3.SameLevel(l1));
            Assert.IsFalse(r3.SameLevel(l2));
            Assert.IsFalse(r3.SameLevel(toR1));
            Assert.IsFalse(r3.SameLevel(r1));
            Assert.IsFalse(r3.SameLevel(r2));
            Assert.IsTrue(r3.SameLevel(r3));
            Assert.IsFalse(r3.SameLevel(es1));
            Assert.IsFalse(r3.SameLevel(es2));
            Assert.IsFalse(r3.SameLevel(r1_nord));
            Assert.IsFalse(r3.SameLevel(r2_nord));

            Assert.IsTrue(es1.SameLevel(l1));
            Assert.IsFalse(es1.SameLevel(l2));
            Assert.IsFalse(es1.SameLevel(toR1));
            Assert.IsFalse(es1.SameLevel(r1));
            Assert.IsFalse(es1.SameLevel(r2));
            Assert.IsFalse(es1.SameLevel(r3));
            Assert.IsTrue(es1.SameLevel(es1));
            Assert.IsFalse(es1.SameLevel(es2));

            Assert.IsFalse(es2.SameLevel(l1));
            Assert.IsTrue(es2.SameLevel(l2));
            Assert.IsFalse(es2.SameLevel(toR1));
            Assert.IsFalse(es2.SameLevel(r1));
            Assert.IsFalse(es2.SameLevel(r2));
            Assert.IsFalse(es2.SameLevel(r3));
            Assert.IsFalse(es2.SameLevel(es1));
            Assert.IsTrue(es2.SameLevel(es2));

            Assert.IsFalse(toR1.SameLevel(l1));
            Assert.IsFalse(toR1.SameLevel(l2));
            Assert.IsTrue(toR1.SameLevel(toR1));
            Assert.IsTrue(toR1.SameLevel(r1));
            Assert.IsFalse(toR1.SameLevel(r2));
            Assert.IsFalse(toR1.SameLevel(r3));
            Assert.IsTrue(toR1.SameLevel(r1_nord));
            Assert.IsFalse(toR1.SameLevel(r2_nord));
            Assert.IsFalse(toR1.SameLevel(es1));
            Assert.IsFalse(toR1.SameLevel(es2));
        }

    }
}
