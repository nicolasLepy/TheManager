using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class ClubLocalisationComparator : IComparer<Club>
    {

        private readonly GeographicPosition _reference;

        public ClubLocalisationComparator(GeographicPosition reference)
        {
            _reference = reference;
        }

        public int Compare(Club x, Club y)
        {
            int res = 0;
            float distX = Utils.Distance(x.stadium.city.Position, _reference);
            float distY = Utils.Distance(y.stadium.city.Position, _reference);
            if (distX > distY)
            {                
                res = 1;
            }
            else if(distY > distX)
            {
                res = -1;            
            }
            return res;
        }
    }
}
