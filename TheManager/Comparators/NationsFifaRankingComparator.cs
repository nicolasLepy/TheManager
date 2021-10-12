using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class NationsFifaRankingComparator : IComparer<NationalTeam>
    {
        public int Compare(NationalTeam x, NationalTeam y)
        {
            return y.archivalFifaPoints[y.archivalFifaPoints.Count-1] - x.archivalFifaPoints[x.archivalFifaPoints.Count - 1] > 0 ? 1 : -1;
        }
    }
}
