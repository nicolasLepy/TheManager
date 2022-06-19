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
        private readonly RankingType _rankingType;
        private readonly bool _inverted;

        public ClubRankingComparator(List<Match> games, RankingType rankingType = RankingType.General, bool inverted = false)
        {
            _round = games;
            _rankingType = rankingType;
            _inverted = inverted;
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
            
            
            return _inverted ? -res : res;
        }

        private int Points(Club c)
        {
            return Utils.Points(_round, c, _rankingType);
        }

        private int Difference(Club c)
        {
            return Utils.Difference(_round, c, _rankingType);
        }

        private int GoalFor(Club c)
        {
            return Utils.Gf(_round, c, _rankingType);
        }
    }
}