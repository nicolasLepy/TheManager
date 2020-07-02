using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class ClubLevelComparator : IComparer<Club>
    {

        private readonly bool _inverted;

        public ClubLevelComparator() : this(false)
        {
        }
        
        public ClubLevelComparator(bool inverted)
        {
            _inverted = inverted;
        }

        public int Compare(Club x, Club y)
        {
            int res;
            if (Math.Abs(x.Level() - y.Level()) < 0.01)
            {
                res = 0;
            }
            else
            {
                res = (int)(y.Level() - x.Level());
            }

            if (_inverted)
            {
                res = -res;
            }
            return res;
        }
    }
}
