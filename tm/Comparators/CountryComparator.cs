using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{

    public enum CountryAttribute
    {
        CONTINENTAL_COEFFICIENT
    }

    public class CountryComparator : IComparer<Country>
    {

        private readonly CountryAttribute _attribute;
        private bool _inverted;

        public CountryComparator(CountryAttribute attribute, bool inverted = false)
        {
            _attribute = attribute;
            _inverted = inverted;
        }
        private int Inverted()
        {
            return _inverted ? -1 : 1;
        }

        public int Compare(Country x, Country y)
        {
            int res;
            switch (_attribute)
            {
                case CountryAttribute.CONTINENTAL_COEFFICIENT:
                    res = x.AssociationCoefficient > y.AssociationCoefficient ? -1 : 1;
                    break;
                default:
                    res = x.AssociationCoefficient > y.AssociationCoefficient ? -1 : 1;
                    break;
            }
            return res * Inverted();
        }

    }
}
