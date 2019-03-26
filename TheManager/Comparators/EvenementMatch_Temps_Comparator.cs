using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class EvenementMatch_Temps_Comparator : IComparer<EvenementMatch>
    {
        public int Compare(EvenementMatch x, EvenementMatch y)
        {
            int tempsX = x.Minute;
            if (x.MiTemps == 2) tempsX += 45;
            else if (x.MiTemps == 3) tempsX += 90;
            else if (x.MiTemps == 4) tempsX += 105;
            int tempsY = y.Minute;
            if (y.MiTemps == 2) tempsY += 45;
            else if (y.MiTemps == 3) tempsY += 90;
            else if (y.MiTemps == 4) tempsY += 105;

            return tempsX - tempsY;
        }
    }
}