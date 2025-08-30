using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{
    public class CoefficientComparator<T> : IComparer<T>
    {

        private Dictionary<T, float> _coefficients;
        public CoefficientComparator(Dictionary<T, float> coefficients)
        {
            _coefficients = coefficients;
        }

        public int Compare(T x, T y)
        {
            int res = 0;
            if(_coefficients[x] > _coefficients[y])
            {
                res = -1;
            }
            else if (_coefficients[x] < _coefficients[y])
            {
                res = 1;
            }
            return res;
        }


    }
}
