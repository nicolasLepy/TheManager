using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class ClubRankingComparator : IComparer<Club>
    {
        private readonly List<Match> _round;

        public ClubRankingComparator(List<Match> games)
        {
            _round = games;
        }

        public int Compare(Club x, Club y)
        {
            int res = 1;
            if (Points(y) < Points(x))
            {
                res = -1;
            }

            else if (Points(y) == Points(x))
            {
                if (Difference(y) < Difference(x))
                {
                    res = -1;
                }
                else if (Difference(x) == Difference(y))
                {
                    if (GoalFor(y) < GoalFor(x))
                    {
                        res = -1;
                    }
                }
            }
            
            
            return res;
        }

        private int Points(Club c)
        {
            return Utils.Points(_round, c);
            //return _tour.Points(c);
        }

        private int Difference(Club c)
        {
            return Utils.Difference(_round, c);
            //return _tour.Difference(c);
        }

        private int GoalFor(Club c)
        {
            return Utils.Gf(_round, c);
        }
    }
}