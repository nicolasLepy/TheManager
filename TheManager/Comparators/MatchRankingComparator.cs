using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class MatchRankingComparator : IComparer<Match>
    {

        private readonly ChampionshipRound _round;

        public MatchRankingComparator(ChampionshipRound round )
        {
            _round = round;
        }

        public int Compare(Match x, Match y)
        {
            List<Club> ranking = _round.Ranking();
            int nivMatchX = ranking.IndexOf(x.home) + ranking.IndexOf(x.away);
            int nivMatchY = ranking.IndexOf(y.home) + ranking.IndexOf(y.away);

            return nivMatchX - nivMatchY;
        }
    }
}