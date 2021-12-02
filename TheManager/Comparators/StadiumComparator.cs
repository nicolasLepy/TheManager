using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class StadiumComparator : IComparer<Stadium>
    {
        public int Compare(Stadium x, Stadium y)
        {
            return y.capacity - x.capacity;
        }
    }
}
