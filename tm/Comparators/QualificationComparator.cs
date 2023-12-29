using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{
    public class QualificationComparator : IComparer<Qualification>
    {
        public int Compare(Qualification x, Qualification y)
        {
            return x.ranking - y.ranking;
            
        }
    }
}