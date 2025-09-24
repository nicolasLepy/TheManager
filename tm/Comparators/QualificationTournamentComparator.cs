using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{
    public class QualificationTournamentComparator : IComparer<Qualification>
    {
        public int Compare(Qualification x, Qualification y)
        {
            int xALevel = x.target.GetAssociationLevel();
            int yALevel = y.target.GetAssociationLevel();
            int res = xALevel - yALevel;
            if(res == 0)
            {
                int xTLevel = x.target.GetTournamentLevel();
                int yTLevel = y.target.GetTournamentLevel();
                res = xTLevel - yTLevel;
                if(res == 0)
                {
                    res = y.roundId - x.roundId;
                }
            }
            return res;
        }
    }
}
