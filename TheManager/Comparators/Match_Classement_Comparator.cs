using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class Match_Classement_Comparator : IComparer<Match>
    {

        private TourChampionnat _tour;

        public Match_Classement_Comparator(TourChampionnat tour )
        {
            _tour = tour;
        }

        public int Compare(Match x, Match y)
        {
            List<Club> classement = _tour.Classement();
            int nivMatchX = classement.IndexOf(x.Domicile) + classement.IndexOf(x.Exterieur);
            int nivMatchY = classement.IndexOf(y.Domicile) + classement.IndexOf(y.Exterieur);

            return nivMatchX - nivMatchY;
        }
    }
}