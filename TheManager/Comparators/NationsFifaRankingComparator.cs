using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class NationsFifaRankingComparator : IComparer<NationalTeam>
    {

        private bool _inverted;

        public NationsFifaRankingComparator(bool inverted = false)
        {
            _inverted = inverted;
        }
        public int Compare(NationalTeam x, NationalTeam y)
        {
            double yPoints = y.archivalFifaPoints[y.archivalFifaPoints.Count - 1];
            double xPoints = x.archivalFifaPoints[x.archivalFifaPoints.Count - 1];
            int res;
            if (x == null || y == null)
            {
                res = 0;
            }
            else if(yPoints == xPoints)
            {
                res = 0;
            }
            else
            {
                res = yPoints - xPoints > 0 ? 1 : -1;
            }

            return _inverted ? res * -1 : res;
        }
    }
}
