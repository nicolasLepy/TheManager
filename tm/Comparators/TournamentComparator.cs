using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{
    public class TournamentComparator : IComparer<Tournament>
    {

        private readonly Association _reference;

        public TournamentComparator(Association reference)
        {
            _reference = reference;
        }

        private int Level(Tournament t)
        {
            return _reference.TournamentLevel(t);
        }

        public int Compare(Tournament x, Tournament y)
        {
            int res = 1;
            if(!y.isChampionship && x.isChampionship)
            {
                res = -1;
            }
            else if(y.isChampionship == x.isChampionship)
            {
                res = Level(x) - Level(y);
            }
            return res;
        }
    }
}
