using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm;

namespace tests.tm
{
    [TestClass]
    public class TestsTournaments : TheManagerTest
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

            Tournament l1 = fr.League(1);
            Assert.IsNotNull(l1);
            Tournament l2 = fr.League(2);
            Assert.IsNotNull(l2);
            Tournament r1 = bfc.League(1);
            Assert.IsNotNull(r1);
            Tournament r2 = bfc.League(2);
            Assert.IsNotNull(r2);
            Tournament r3 = bfc.League(3);
            Assert.IsNotNull(r3);
            Tournament r1_nord = nord.League(1);
            Assert.IsNotNull(r1_nord);
            Tournament r2_nord = nord.League(2);
            Assert.IsNotNull(r2_nord);
            Tournament es1 = es.League(1);
            Assert.IsNotNull(es1);
            Tournament es2 = es.League(2);
            Assert.IsNotNull(es2);

            Assert.IsFalse(l1.IsAbove(new QualificationTournament(l1)));
            Assert.IsTrue(l1.IsAbove(new QualificationTournament(l2)));
            Assert.IsTrue(l1.IsAbove(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsTrue(l1.IsAbove(new QualificationTournament(r1)));
            Assert.IsTrue(l1.IsAbove(new QualificationTournament(r2)));
            Assert.IsTrue(l1.IsAbove(new QualificationTournament(r3)));
            Assert.IsFalse(l1.IsAbove(new QualificationTournament(es1)));
            Assert.IsTrue(l1.IsAbove(new QualificationTournament(es2)));

            Assert.IsFalse(l2.IsAbove(new QualificationTournament(l1)));
            Assert.IsFalse(l2.IsAbove(new QualificationTournament(l2)));
            Assert.IsTrue(l2.IsAbove(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsTrue(l2.IsAbove(new QualificationTournament(r1)));
            Assert.IsTrue(l2.IsAbove(new QualificationTournament(r2)));
            Assert.IsTrue(l2.IsAbove(new QualificationTournament(r3)));
            Assert.IsFalse(l2.IsAbove(new QualificationTournament(es1)));
            Assert.IsFalse(l2.IsAbove(new QualificationTournament(es2)));

            Assert.IsFalse(r1.IsAbove(new QualificationTournament(l1)));
            Assert.IsFalse(r1.IsAbove(new QualificationTournament(l2)));
            Assert.IsFalse(r1.IsAbove(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(r1.IsAbove(new QualificationTournament(r1)));
            Assert.IsTrue(r1.IsAbove(new QualificationTournament(r2)));
            Assert.IsTrue(r1.IsAbove(new QualificationTournament(r3)));
            Assert.IsFalse(r1.IsAbove(new QualificationTournament(es1)));
            Assert.IsFalse(r1.IsAbove(new QualificationTournament(es2)));
            Assert.IsFalse(r1.IsAbove(new QualificationTournament(r1_nord)));
            Assert.IsTrue(r1.IsAbove(new QualificationTournament(r2_nord)));

            Assert.IsFalse(r2.IsAbove(new QualificationTournament(l1)));
            Assert.IsFalse(r2.IsAbove(new QualificationTournament(l2)));
            Assert.IsFalse(r2.IsAbove(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(r2.IsAbove(new QualificationTournament(r1)));
            Assert.IsFalse(r2.IsAbove(new QualificationTournament(r2)));
            Assert.IsTrue(r2.IsAbove(new QualificationTournament(r3)));
            Assert.IsFalse(r2.IsAbove(new QualificationTournament(es1)));
            Assert.IsFalse(r2.IsAbove(new QualificationTournament(es2)));
            Assert.IsFalse(r2.IsAbove(new QualificationTournament(r1_nord)));
            Assert.IsFalse(r2.IsAbove(new QualificationTournament(r2_nord)));

            Assert.IsFalse(r3.IsAbove(new QualificationTournament(l1)));
            Assert.IsFalse(r3.IsAbove(new QualificationTournament(l2)));
            Assert.IsFalse(r3.IsAbove(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(r3.IsAbove(new QualificationTournament(r1)));
            Assert.IsFalse(r3.IsAbove(new QualificationTournament(r2)));
            Assert.IsFalse(r3.IsAbove(new QualificationTournament(r3)));
            Assert.IsFalse(r3.IsAbove(new QualificationTournament(es1)));
            Assert.IsFalse(r3.IsAbove(new QualificationTournament(es2)));
            Assert.IsFalse(r3.IsAbove(new QualificationTournament(r1_nord)));
            Assert.IsFalse(r3.IsAbove(new QualificationTournament(r2_nord)));

            Assert.IsFalse(es1.IsAbove(new QualificationTournament(l1)));
            Assert.IsTrue(es1.IsAbove(new QualificationTournament(l2)));
            // Assert.IsTrue(es1.IsAbove(new QualificationExcludeLeagueSystem(fr))); // Not implemented for now
            // Assert.IsTrue(es1.IsAbove(new QualificationTournament(r1))); //Not implemented for now
            // Assert.IsTrue(es1.IsAbove(new QualificationTournament(r2))); //Not implemented for now
            // Assert.IsTrue(es1.IsAbove(new QualificationTournament(r3))); //Not implemented for now
            Assert.IsFalse(es1.IsAbove(new QualificationTournament(es1)));
            Assert.IsTrue(es1.IsAbove(new QualificationTournament(es2)));

            Assert.IsFalse(es2.IsAbove(new QualificationTournament(l1)));
            Assert.IsFalse(es2.IsAbove(new QualificationTournament(l2)));
            // Assert.IsTrue(es2.IsAbove(new QualificationExcludeLeagueSystem(fr))); // Not implemented for now
            // Assert.IsTrue(es2.IsAbove(new QualificationTournament(r1))); //Not implemented for now
            // Assert.IsTrue(es2.IsAbove(new QualificationTournament(r2))); //Not implemented for now
            // Assert.IsTrue(es2.IsAbove(new QualificationTournament(r3))); //Not implemented for now
            Assert.IsFalse(es2.IsAbove(new QualificationTournament(es1)));
            Assert.IsFalse(es2.IsAbove(new QualificationTournament(es2)));

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

            Tournament l1 = fr.League(1);
            Assert.IsNotNull(l1);
            Tournament l2 = fr.League(2);
            Assert.IsNotNull(l2);
            Tournament r1 = bfc.League(1);
            Assert.IsNotNull(r1);
            Tournament r2 = bfc.League(2);
            Assert.IsNotNull(r2);
            Tournament r3 = bfc.League(3);
            Assert.IsNotNull(r3);
            Tournament r1_nord = nord.League(1);
            Assert.IsNotNull(r1_nord);
            Tournament r2_nord = nord.League(2);
            Assert.IsNotNull(r2_nord);
            Tournament es1 = es.League(1);
            Assert.IsNotNull(es1);
            Tournament es2 = es.League(2);
            Assert.IsNotNull(es2);

            Assert.IsFalse(l1.IsBelow(new QualificationTournament(l1)));
            Assert.IsFalse(l1.IsBelow(new QualificationTournament(l2)));
            Assert.IsFalse(l1.IsBelow(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(l1.IsBelow(new QualificationTournament(r1)));
            Assert.IsFalse(l1.IsBelow(new QualificationTournament(r2)));
            Assert.IsFalse(l1.IsBelow(new QualificationTournament(r3)));
            Assert.IsFalse(l1.IsBelow(new QualificationTournament(es1)));
            Assert.IsFalse(l1.IsBelow(new QualificationTournament(es2)));

            Assert.IsTrue(l2.IsBelow(new QualificationTournament(l1)));
            Assert.IsFalse(l2.IsBelow(new QualificationTournament(l2)));
            Assert.IsFalse(l2.IsBelow(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(l2.IsBelow(new QualificationTournament(r1)));
            Assert.IsFalse(l2.IsBelow(new QualificationTournament(r2)));
            Assert.IsFalse(l2.IsBelow(new QualificationTournament(r3)));
            Assert.IsTrue(l2.IsBelow(new QualificationTournament(es1)));
            Assert.IsFalse(l2.IsBelow(new QualificationTournament(es2)));

            Assert.IsTrue(r1.IsBelow(new QualificationTournament(l1)));
            Assert.IsTrue(r1.IsBelow(new QualificationTournament(l2)));
            Assert.IsFalse(r1.IsBelow(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(r1.IsBelow(new QualificationTournament(r1)));
            Assert.IsFalse(r1.IsBelow(new QualificationTournament(r2)));
            Assert.IsFalse(r1.IsBelow(new QualificationTournament(r3)));
            // Assert.IsTrue(r1.IsBelow(new QualificationTournament(es1))); //Not implemented yet
            // Assert.IsTrue(r1.IsBelow(new QualificationTournament(es2)));  //Not implemented yet
            Assert.IsFalse(r1.IsBelow(new QualificationTournament(r1_nord)));
            Assert.IsFalse(r1.IsBelow(new QualificationTournament(r2_nord)));

            Assert.IsTrue(r2.IsBelow(new QualificationTournament(l1)));
            Assert.IsTrue(r2.IsBelow(new QualificationTournament(l2)));
            Assert.IsTrue(r2.IsBelow(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsTrue(r2.IsBelow(new QualificationTournament(r1)));
            Assert.IsFalse(r2.IsBelow(new QualificationTournament(r2)));
            Assert.IsFalse(r2.IsBelow(new QualificationTournament(r3)));
            // Assert.IsTrue(r2.IsBelow(new QualificationTournament(es1)));  //Not implemented yet
            // Assert.IsTrue(r2.IsBelow(new QualificationTournament(es2)));  //Not implemented yet
            Assert.IsTrue(r2.IsBelow(new QualificationTournament(r1_nord)));
            Assert.IsFalse(r2.IsBelow(new QualificationTournament(r2_nord)));

            Assert.IsTrue(r3.IsBelow(new QualificationTournament(l1)));
            Assert.IsTrue(r3.IsBelow(new QualificationTournament(l2)));
            Assert.IsTrue(r3.IsBelow(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsTrue(r3.IsBelow(new QualificationTournament(r1)));
            Assert.IsTrue(r3.IsBelow(new QualificationTournament(r2)));
            Assert.IsFalse(r3.IsBelow(new QualificationTournament(r3)));
            // Assert.IsTrue(r3.IsBelow(new QualificationTournament(es1)));  //Not implemented yet
            // Assert.IsTrue(r3.IsBelow(new QualificationTournament(es2)));  //Not implemented yet
            Assert.IsTrue(r3.IsBelow(new QualificationTournament(r1_nord)));
            Assert.IsTrue(r3.IsBelow(new QualificationTournament(r2_nord)));

            Assert.IsFalse(es1.IsBelow(new QualificationTournament(l1)));
            Assert.IsFalse(es1.IsBelow(new QualificationTournament(l2)));
            // Assert.IsFalse(es1.IsBelow(new QualificationExcludeLeagueSystem(fr))); // Not implemented for now
            // Assert.IsFalse(es1.IsBelow(new QualificationTournament(r1))); //Not implemented for now
            // Assert.IsFalse(es1.IsBelow(new QualificationTournament(r2))); //Not implemented for now
            // Assert.IsFalse(es1.IsBelow(new QualificationTournament(r3))); //Not implemented for now
            Assert.IsFalse(es1.IsBelow(new QualificationTournament(es1)));
            Assert.IsFalse(es1.IsBelow(new QualificationTournament(es2)));

            Assert.IsTrue(es2.IsBelow(new QualificationTournament(l1)));
            Assert.IsFalse(es2.IsBelow(new QualificationTournament(l2)));
            // Assert.IsFalse(es2.IsBelow(new QualificationExcludeLeagueSystem(fr))); // Not implemented for now
            // Assert.IsFalse(es2.IsBelow(new QualificationTournament(r1))); //Not implemented for now
            // Assert.IsFalse(es2.IsBelow(new QualificationTournament(r2))); //Not implemented for now
            // Assert.IsFalse(es2.IsBelow(new QualificationTournament(r3))); //Not implemented for now
            Assert.IsTrue(es2.IsBelow(new QualificationTournament(es1)));
            Assert.IsFalse(es2.IsBelow(new QualificationTournament(es2)));
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

            Tournament l1 = fr.League(1);
            Assert.IsNotNull(l1);
            Tournament l2 = fr.League(2);
            Assert.IsNotNull(l2);
            Tournament r1 = bfc.League(1);
            Assert.IsNotNull(r1);
            Tournament r2 = bfc.League(2);
            Assert.IsNotNull(r2);
            Tournament r3 = bfc.League(3);
            Assert.IsNotNull(r3);
            Tournament r1_nord = nord.League(1);
            Assert.IsNotNull(r1_nord);
            Tournament r2_nord = nord.League(2);
            Assert.IsNotNull(r2_nord);
            Tournament es1 = es.League(1);
            Assert.IsNotNull(es1);
            Tournament es2 = es.League(2);
            Assert.IsNotNull(es2);

            Assert.IsTrue(l1.IsSameLevel(new QualificationTournament(l1)));
            Assert.IsFalse(l1.IsSameLevel(new QualificationTournament(l2)));
            Assert.IsFalse(l1.IsSameLevel(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(l1.IsSameLevel(new QualificationTournament(r1)));
            Assert.IsFalse(l1.IsSameLevel(new QualificationTournament(r2)));
            Assert.IsFalse(l1.IsSameLevel(new QualificationTournament(r3)));
            Assert.IsTrue(l1.IsSameLevel(new QualificationTournament(es1)));
            Assert.IsFalse(l1.IsSameLevel(new QualificationTournament(es2)));

            Assert.IsFalse(l2.IsSameLevel(new QualificationTournament(l1)));
            Assert.IsTrue(l2.IsSameLevel(new QualificationTournament(l2)));
            Assert.IsFalse(l2.IsSameLevel(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(l2.IsSameLevel(new QualificationTournament(r1)));
            Assert.IsFalse(l2.IsSameLevel(new QualificationTournament(r2)));
            Assert.IsFalse(l2.IsSameLevel(new QualificationTournament(r3)));
            Assert.IsFalse(l2.IsSameLevel(new QualificationTournament(es1)));
            Assert.IsTrue(l2.IsSameLevel(new QualificationTournament(es2)));

            Assert.IsFalse(r1.IsSameLevel(new QualificationTournament(l1)));
            Assert.IsFalse(r1.IsSameLevel(new QualificationTournament(l2)));
            Assert.IsTrue(r1.IsSameLevel(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsTrue(r1.IsSameLevel(new QualificationTournament(r1)));
            Assert.IsFalse(r1.IsSameLevel(new QualificationTournament(r2)));
            Assert.IsFalse(r1.IsSameLevel(new QualificationTournament(r3)));
            Assert.IsFalse(r1.IsSameLevel(new QualificationTournament(es1)));
            Assert.IsFalse(r1.IsSameLevel(new QualificationTournament(es2)));
            Assert.IsTrue(r1.IsSameLevel(new QualificationTournament(r1_nord)));
            Assert.IsFalse(r1.IsSameLevel(new QualificationTournament(r2_nord)));

            Assert.IsFalse(r2.IsSameLevel(new QualificationTournament(l1)));
            Assert.IsFalse(r2.IsSameLevel(new QualificationTournament(l2)));
            Assert.IsFalse(r2.IsSameLevel(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(r2.IsSameLevel(new QualificationTournament(r1)));
            Assert.IsTrue(r2.IsSameLevel(new QualificationTournament(r2)));
            Assert.IsFalse(r2.IsSameLevel(new QualificationTournament(r3)));
            Assert.IsFalse(r2.IsSameLevel(new QualificationTournament(es1)));
            Assert.IsFalse(r2.IsSameLevel(new QualificationTournament(es2)));
            Assert.IsFalse(r2.IsSameLevel(new QualificationTournament(r1_nord)));
            Assert.IsTrue(r2.IsSameLevel(new QualificationTournament(r2_nord)));

            Assert.IsFalse(r3.IsSameLevel(new QualificationTournament(l1)));
            Assert.IsFalse(r3.IsSameLevel(new QualificationTournament(l2)));
            Assert.IsFalse(r3.IsSameLevel(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(r3.IsSameLevel(new QualificationTournament(r1)));
            Assert.IsFalse(r3.IsSameLevel(new QualificationTournament(r2)));
            Assert.IsTrue(r3.IsSameLevel(new QualificationTournament(r3)));
            Assert.IsFalse(r3.IsSameLevel(new QualificationTournament(es1)));
            Assert.IsFalse(r3.IsSameLevel(new QualificationTournament(es2)));
            Assert.IsFalse(r3.IsSameLevel(new QualificationTournament(r1_nord)));
            Assert.IsFalse(r3.IsSameLevel(new QualificationTournament(r2_nord)));

            Assert.IsTrue(es1.IsSameLevel(new QualificationTournament(l1)));
            Assert.IsFalse(es1.IsSameLevel(new QualificationTournament(l2)));
            Assert.IsFalse(es1.IsSameLevel(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(es1.IsSameLevel(new QualificationTournament(r1)));
            Assert.IsFalse(es1.IsSameLevel(new QualificationTournament(r2)));
            Assert.IsFalse(es1.IsSameLevel(new QualificationTournament(r3)));
            Assert.IsTrue(es1.IsSameLevel(new QualificationTournament(es1)));
            Assert.IsFalse(es1.IsSameLevel(new QualificationTournament(es2)));

            Assert.IsFalse(es2.IsSameLevel(new QualificationTournament(l1)));
            Assert.IsTrue(es2.IsSameLevel(new QualificationTournament(l2)));
            Assert.IsFalse(es2.IsSameLevel(new QualificationExcludeLeagueSystem(fr)));
            Assert.IsFalse(es2.IsSameLevel(new QualificationTournament(r1)));
            Assert.IsFalse(es2.IsSameLevel(new QualificationTournament(r2)));
            Assert.IsFalse(es2.IsSameLevel(new QualificationTournament(r3)));
            Assert.IsFalse(es2.IsSameLevel(new QualificationTournament(es1)));
            Assert.IsTrue(es2.IsSameLevel(new QualificationTournament(es2)));
        }
    }
}
