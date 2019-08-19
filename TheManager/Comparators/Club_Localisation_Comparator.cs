using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class Club_Localisation_Comparator : IComparer<Club>
    {

        private Position _reference;

        public Club_Localisation_Comparator(Position reference)
        {
            _reference = reference;
        }

        public int Compare(Club x, Club y)
        {
            int res = 0;
            float distX = Utils.Distance(x.Stade.Ville.Position, _reference);
            float distY = Utils.Distance(y.Stade.Ville.Position, _reference);
            if (distX > distY)
                res = 1;
            else if(distY > distX)
                res = -1;
            return res;
        }
    }
}
