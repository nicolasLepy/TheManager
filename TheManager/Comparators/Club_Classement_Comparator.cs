using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class Club_Classement_Comparator : IComparer<Club>
    {
        private Tour _tour;

        public Club_Classement_Comparator(Tour t)
        {
            _tour = t;
        }

        public int Compare(Club x, Club y)
        {
            int res = 1;
            if (Points(y) < Points(x)) res = -1;
            else if (Points(y) == Points(x)) if(Difference(y) < Difference(x)) res = -1;
            return res;
        }

        private int Points(Club c)
        {
            return _tour.Points(c);
        }

        private int Difference(Club c)
        {
            return _tour.Difference(c);
        }
    }
}