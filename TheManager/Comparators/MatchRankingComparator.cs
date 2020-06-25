using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class MatchRankingComparator : IComparer<Match>
    {

        private TourChampionnat _round;

        public MatchRankingComparator(TourChampionnat round )
        {
            _round = round;
        }

        public int Compare(Match x, Match y)
        {
            List<Club> ranking = _round.Classement();
            int nivMatchX = ranking.IndexOf(x.Domicile) + ranking.IndexOf(x.Exterieur);
            int nivMatchY = ranking.IndexOf(y.Domicile) + ranking.IndexOf(y.Exterieur);

            return nivMatchX - nivMatchY;
        }
    }
}