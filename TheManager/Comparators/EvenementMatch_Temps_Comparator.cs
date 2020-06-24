using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class EvenementMatch_Temps_Comparator : IComparer<MatchEvent>
    {
        public int Compare(MatchEvent x, MatchEvent y)
        {
            int tempsX = x.minute;
            if (x.period == 2) tempsX += 45;
            else if (x.period == 3) tempsX += 90;
            else if (x.period == 4) tempsX += 105;
            int tempsY = y.minute;
            if (y.period == 2) tempsY += 45;
            else if (y.period == 3) tempsY += 90;
            else if (y.period == 4) tempsY += 105;

            return tempsX - tempsY;
        }
    }
}