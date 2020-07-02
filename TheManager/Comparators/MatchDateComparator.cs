using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class MatchDateComparator : IComparer<Match>
    {
        public int Compare(Match x, Match y)
        {
            int res = 1;
            int diff = DateTime.Compare(x.day, y.day);
            if (diff < 0)
            {
                res = -1;
            } 
            else if(diff == 0)
            {
                int X = 0;
                int Y = 0;
                CityClub home = x.home as CityClub;
                CityClub away = x.away as CityClub;
                if (home != null && home.Championship != null)
                {
                    X += (int)Math.Pow(2, 10 - home.Championship.level);
                }

                if (away != null && away.Championship != null)
                {
                    X += (int)Math.Pow(2, 10 - away.Championship.level);
                }
                home = y.home as CityClub;
                away = y.away as CityClub;
                if (home != null && home.Championship != null)
                {
                    Y += (int)Math.Pow(2, 10 - home.Championship.level);
                }

                if (away != null && away.Championship != null)
                {
                    Y += (int)Math.Pow(2, 10 - away.Championship.level);
                }
                if (X > Y)
                {
                    res = -1;
                }
            }
            return res;
        }
    }
}
