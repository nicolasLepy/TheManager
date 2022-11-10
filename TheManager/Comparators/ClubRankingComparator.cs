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
        private readonly bool _knockoutRound;

        public ClubRankingComparator(List<Match> games, RankingType rankingType = RankingType.General, bool inverted = false, bool knockoutRound = false)
        {
            _round = games;
            _rankingType = rankingType;
            _inverted = inverted;
            _knockoutRound = knockoutRound;
        }

        public int CompareKnockoutRound(Club x, Club y)
        {
            int res = 0;
            bool gameFound = false;
            for(int j = this._round.Count-1; j >= 0 && !gameFound; j--)
            {
                Match game = this._round[j];
                if((game.home == x && game.away == y) || (game.home == y && game.away == x))
                {
                    res = 1;
                    if(game.Winner == x)
                    {
                        res = -1;
                    }
                    gameFound = true;
                }
            }
            return res;
        }

        public int Compare(Club x, Club y)
        {
            int res = 1;
            if(_knockoutRound)
            {
                res = CompareKnockoutRound(x, y);
            }
            else
            {
                int pointsY = Points(y);
                int pointsX = Points(x);
                if (pointsY < pointsX)
                {
                    res = -1;
                }
                else if (pointsY == pointsX)
                {
                    int differenceY = Difference(y);
                    int differenceX = Difference(x);
                    if (differenceY < differenceX)
                    {
                        res = -1;
                    }
                    else if (differenceX == differenceY)
                    {
                        int goalForY = GoalFor(y);
                        int goalForX = GoalFor(x);
                        if (goalForY < goalForX)
                        {
                            res = -1;
                        }
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