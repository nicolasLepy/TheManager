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
            return Points(y) - Points(x);
        }

        private int Points(Club c)
        {
            return _tour.Points(c);
        }
    }
}