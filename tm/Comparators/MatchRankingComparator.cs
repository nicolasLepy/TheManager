using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace tm.Comparators
{
    public class MatchRankingComparator : IComparer<Match>
    {

        private readonly GroupsRound _round;

        public MatchRankingComparator(GroupsRound round )
        {
            _round = round;
        }

        public int Compare(Match x, Match y)
        {
            int xrh = _round.Ranking(x.home);
            int xra = _round.Ranking(x.away);
            int yrh = _round.Ranking(y.home);
            int yra = _round.Ranking(y.away);
            int nivMatchX = xrh + xra;
            int nivMatchY = yrh + yra;

            return nivMatchX - nivMatchY;
        }
    }
}