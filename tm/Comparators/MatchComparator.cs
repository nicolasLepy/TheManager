using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{

    public enum MatchAttribute
    {
        TOURNAMENT,
        DATE
    }

    public class MatchComparator : IComparer<Match>
    {

        private List<MatchAttribute> _sort;

        public MatchComparator(List<MatchAttribute> sortOrder)
        {
            _sort = sortOrder;
        }

        public int CompareTournament(Match x, Match y)
        {
            ILocalisation localisationX = Session.Instance.Game.kernel.LocalisationTournament(x.Tournament);
            ILocalisation localisationY = Session.Instance.Game.kernel.LocalisationTournament(y.Tournament);
            int res = localisationX.Name().CompareTo(localisationY.Name());
            if(res == 0)
            {
                res = x.Tournament.name.CompareTo(y.Tournament.name);
            }
            return res;
        }

        public int CompareDate(Match x, Match y)
        {
            int res = 1;
            int diff = DateTime.Compare(x.day, y.day);
            if (diff < 0)
            {
                res = -1;
            }
            else if (diff == 0)
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

        public int Compare(Match x, Match y)
        {
            int res = 0;
            int i = 0;
            while(res == 0 && i < _sort.Count)
            {
                switch (_sort[i])
                {
                    case MatchAttribute.TOURNAMENT:
                        res = CompareTournament(x, y);
                        break;
                    case MatchAttribute.DATE:
                        res = CompareDate(x, y);
                        break;
                }
                i++;
            }
            return res;
        }
    }
}
