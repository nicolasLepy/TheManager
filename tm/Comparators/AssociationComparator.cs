using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Comparators
{

    public enum AssociationAttribute
    {
        CONTINENTAL_COEFFICIENT
    }

    public class AssociationComparator : IComparer<Association>
    {

        private readonly AssociationAttribute _attribute;
        private bool _inverted;

        public AssociationComparator(AssociationAttribute attribute, bool inverted = false)
        {
            _attribute = attribute;
            _inverted = inverted;
        }
        private int Inverted()
        {
            return _inverted ? -1 : 1;
        }

        public int Compare(Association x, Association y)
        {
            int res;
            switch (_attribute)
            {
                case AssociationAttribute.CONTINENTAL_COEFFICIENT:
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
