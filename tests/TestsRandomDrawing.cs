using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm;
using tm.Tournaments;

namespace tests
{
    [TestClass]
    public class TestsRandomDrawing
    {

        [TestMethod]
        public void TestRandomDrawingLevel()
        {
            Association world = new Association(0, "world", "", null, null, 0, false, null, false);
            Association p1 = new Association(1, "p1", "", null, world, 0, false, null, true);
            Association p2 = new Association(2, "p2", "", null, world, 0, false, null, true);
            Association p3 = new Association(3, "p3", "", null, world, 0, false, null, true);
            Association p4 = new Association(4, "p4", "", null, world, 0, false, null, true);
            Association p3a = new Association(5, "p3a", "", null, p3, 0, false, null, false);
            Association p3b = new Association(6, "p3b", "", null, p3, 0, false, null, false);
            world.associations.Add(p1);
            world.associations.Add(p2);
            world.associations.Add(p3);
            p3.associations.Add(p3a);
            p3.associations.Add(p3b);
            world.associations.Add(p4);

            Club ca1 = new CityClub(0, "a1", null, "", 0, 0, 0, 0, null, "", null, "", false, p1, ClubStatus.Professional);
            Club ca2 = new CityClub(0, "a2", null, "", 0, 0, 0, 0, null, "", null, "", false, p1, ClubStatus.Professional);
            Club ca3 = new CityClub(0, "a3", null, "", 0, 0, 0, 0, null, "", null, "", false, p1, ClubStatus.Professional);
            Club ca4 = new CityClub(0, "a4", null, "", 0, 0, 0, 0, null, "", null, "", false, p1, ClubStatus.Professional);
            Club ca5 = new CityClub(0, "a5", null, "", 0, 0, 0, 0, null, "", null, "", false, p1, ClubStatus.Professional);
            Club cb6 = new CityClub(0, "b6", null, "", 0, 0, 0, 0, null, "", null, "", false, p2, ClubStatus.Professional);
            Club cb7 = new CityClub(0, "b7", null, "", 0, 0, 0, 0, null, "", null, "", false, p2, ClubStatus.Professional);
            Club cb8 = new CityClub(0, "b8", null, "", 0, 0, 0, 0, null, "", null, "", false, p2, ClubStatus.Professional);
            Club cb9 = new CityClub(0, "b9", null, "", 0, 0, 0, 0, null, "", null, "", false, p2, ClubStatus.Professional);
            Club cb10 = new CityClub(0, "b10", null, "", 0, 0, 0, 0, null, "", null, "", false, p2, ClubStatus.Professional);
            Club cc11 = new CityClub(0, "c11", null, "", 0, 0, 0, 0, null, "", null, "", false, p3a, ClubStatus.Professional);
            Club cc12 = new CityClub(0, "c12", null, "", 0, 0, 0, 0, null, "", null, "", false, p3a, ClubStatus.Professional);
            Club cc13 = new CityClub(0, "c13", null, "", 0, 0, 0, 0, null, "", null, "", false, p3b, ClubStatus.Professional);
            Club cc14 = new CityClub(0, "c14", null, "", 0, 0, 0, 0, null, "", null, "", false, p3b, ClubStatus.Professional);
            Club cd15 = new CityClub(0, "d15", null, "", 0, 0, 0, 0, null, "", null, "", false, p4, ClubStatus.Professional);
            Club cd16 = new CityClub(0, "d16", null, "", 0, 0, 0, 0, null, "", null, "", false, p4, ClubStatus.Professional);
            Club cd17 = new CityClub(0, "d17", null, "", 0, 0, 0, 0, null, "", null, "", false, p4, ClubStatus.Professional);
            Club cd18 = new CityClub(0, "d18", null, "", 0, 0, 0, 0, null, "", null, "", false, p4, ClubStatus.Professional);

            Tournament t = new Tournament();
            world.tournaments.Add(t);
            GroupsRound r = new GroupActiveRound(0, "", t, null, new List<GameDay>(), new List<TvOffset>(), 4, false, 0, 1, null, null, 0, RandomDrawingMethod.Level, false, 0, 0, 1, 1);
            r.rules.Add(Rule.OneTeamByAssociationInGroup);
            t.rounds.Add(r);
            r.clubs.AddRange(new List<Club> { ca1, ca2, ca3, ca4, ca5, cb6, cb7, cb8, cb9, cb10, cc11, cc12, cc13, cc14, cd15, cd16, cd17, cd18 });

            Session.Instance.Game = new Game();
            Session.Instance.Game.kernel.worldAssociation = world;

            Dictionary<Club, float> coefficients = new Dictionary<Club, float>
            {
                [ca1] = 20,
                [ca2] = 18,
                [ca3] = 16,
                [ca4] = 15,
                [ca5] = 12,
                [cb6] = 17,
                [cb7] = 15,
                [cb8] = 13,
                [cb9] = 12,
                [cb10] = 11,
                [cc11] = 9,
                [cc12] = 10,
                [cc13] = 14,
                [cc14] = 15,
                [cd15] = 9,
                [cd16] = 8,
                [cd17] = 7,
                [cd18] = 6
            };
            List<Club> clubsSorted = new List<Club>() { ca1, ca2, cb6, ca3, ca4, cb7, cc14, cc13, cb8, ca5, cb9, cb10, cc12, cc11, cd15, cd16, cd17, cd18};

            RandomDrawingLevel rdl = new RandomDrawingLevel(r, coefficients);
            for(int i = 0; i < 200; i++)
            {
                if(i == 100)
                {
                    rdl = new RandomDrawingLevel(r, clubsSorted);
                }
                rdl.RandomDrawing();

                List<Club>[] groups = r.groups;
                //(g1, g2) -> 5 équipes, (g3, g4) -> 4 équipes
                Assert.AreEqual(groups[0].Count, 5);
                Assert.AreEqual(groups[1].Count, 5);
                Assert.AreEqual(groups[2].Count, 4);
                Assert.AreEqual(groups[3].Count, 4);

                //(a, b) -> Pas plus de deux par groupe
                //(c, d) -> un par groupe
                Dictionary<Association, int> occByGroup = new Dictionary<Association, int>
                {
                    [p1] = 2,
                    [p2] = 2,
                    [p3] = 1,
                    [p4] = 1
                };
                foreach(KeyValuePair<Association, int> occ in occByGroup)
                {
                    foreach(List<Club> lc in groups)
                    {
                        int ii = 0;
                        foreach(Club c in lc)
                        {
                            if(c.Association() == occ.Key)
                            {
                                ii++;
                            }
                        }
                        Assert.IsTrue(ii <= occ.Value);
                    }
                }
                //(ca1, ca2, cb6, ca3) ne peuvent pas être dans le même groupe
                //(ca4, cc14, cb7, cc13) ne peuvent pas être dans le même groupe
                //(cb10, cb9, ca5, cb8) ne peuvent pas être dans le même groupe
                //(cc12, cd16, cd15, cc11) ne peuvent pas être dans le même groupe
                //(cd17, cd18) ne peuvent pas être dans le même groupe
                List<List<Club>> hats = new List<List<Club>>();
                hats.Add(new List<Club>() { ca1, ca2, cb6, ca3 });
                hats.Add(new List<Club>() { ca4, cc14, cb7, cc13 });
                hats.Add(new List<Club>() { cb10, cb9, ca5, cb8 });
                hats.Add(new List<Club>() { cc12, cd16, cd15, cc11 });
                hats.Add(new List<Club>() { cd17, cd18 });
                foreach(List<Club> hat in hats)
                {
                    foreach(Club c in hat)
                    {
                        List<Club> cGroup = GetGroupOfClub(groups, c);
                        foreach(Club c2 in hat)
                        {
                            if(c2 != c)
                            {
                                Assert.IsFalse(cGroup.Contains(c2));
                            }
                        }
                    }
                }
            }
        }

        private List<Club> GetGroupOfClub(List<Club>[] groups, Club club)
        {
            List<Club> res = null;
            foreach(List<Club> group in groups)
            {
                if(group.Contains(club))
                {
                    res = group;
                }
            }
            return res;
        }
    }
}
