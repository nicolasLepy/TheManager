using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class Match_Date_Comparator : IComparer<Match>
    {
        public int Compare(Match x, Match y)
        {
            return DateTime.Compare(x.Jour, y.Jour);
        }
    }
}
