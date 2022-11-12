using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class TournamentComparator : IComparer<Tournament>
    {
        public int Compare(Tournament x, Tournament y)
        {
            int res = 1;
            if(!y.isChampionship && x.isChampionship)
            {
                res = -1;
            }
            else if(y.isChampionship == x.isChampionship)
            {
                res = x.level - y.level;
            }
            return res;
        }
    }
}
