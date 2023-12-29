using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{
    public class JournalistOffsetComparator : IComparer<Journalist>
    {
        public int Compare(Journalist x, Journalist y)
        {
            return (int)(x.offset - y.offset);
        }
    }

}
