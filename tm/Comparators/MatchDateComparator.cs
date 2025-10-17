using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{
    public class MatchDateComparator : IComparer<Match>
    {

        private Dictionary<Club, int> clubsIndex;

        public MatchDateComparator()
        {
            clubsIndex = new Dictionary<Club, int>();
        }

        private int GetClubIndex(Club c)
        {
            if(!clubsIndex.ContainsKey(c))
            {
                CityClub cc = c as CityClub;
                if (cc != null && cc.Championship != null)
                {
                    int clubIndex = (int)Math.Pow(2, 10 - Session.Instance.Game.kernel.worldAssociation.TournamentLevel(cc.Championship));
                    clubsIndex[c] = clubIndex;
                }
                else
                {
                    clubsIndex[c] = 0;
                }
            }
            return clubsIndex[c];
        }

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
                int X = GetClubIndex(x.home) + GetClubIndex(x.away);
                int Y = GetClubIndex(y.home) + GetClubIndex(y.away);
                if (X > Y)
                {
                    res = -1;
                }
            }
            return res;
        }
    }
}
