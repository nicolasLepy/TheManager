using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class MatchLevelComparator : IComparer<Match>
    {
        public int Compare(Match x, Match y)
        {
            return (int)(((y.home.elo + y.away.elo)) - (x.home.elo + x.away.elo));
        }
    }
}