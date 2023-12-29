using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace tm.Comparators
{
    public class ClubPlayoffsComparator : IComparer<KeyValuePair<Club, int>>
    {
        private readonly List<Club> _ranking;

        public ClubPlayoffsComparator(List<Club> ranking)
        {
            _ranking = ranking;
        }

        public int Compare(KeyValuePair<Club, int> x, KeyValuePair<Club, int> y)
        {
            int res = 0;
            if (x.Value != y.Value)
            {
                res = x.Value - y.Value;
            }
            else
            {
                res = _ranking.IndexOf(y.Key) - _ranking.IndexOf(x.Key);
            }
            return res;
        }
    }
}