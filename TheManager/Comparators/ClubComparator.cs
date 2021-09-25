using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{

    public enum ClubAttribute
    {
        STADIUM,
        LEVEL,
        POTENTIEL,
        BUDGET
    }

    public class ClubComparator : IComparer<Club>
    {

        private readonly ClubAttribute _attribute;

        public ClubComparator(ClubAttribute attribute)
        {
            _attribute = attribute;
        }

        public int Compare(Club x, Club y)
        {
            switch (_attribute)
            {
                case ClubAttribute.STADIUM:
                    return y.stadium.capacity - x.stadium.capacity;
                case ClubAttribute.LEVEL:
                    return x.Level() > y.Level() ? -1 : 1;
                case ClubAttribute.POTENTIEL:
                    return x.Potential() > y.Potential() ? -1 : 1;
                case ClubAttribute.BUDGET:
                    if(x as CityClub == null && y as CityClub == null)
                    {
                        return 0;
                    }
                    else if (x as CityClub != null && y as CityClub == null)
                    {
                        return -1;
                    }
                    else if (x as CityClub == null && y as CityClub != null)
                    {
                        return 1;
                    }
                    else
                    {
                        return (y as CityClub).budget - (x as CityClub).budget;
                    }
                default:
                    return x.Level() > y.Level() ? -1 : 1;
            }
        }
    }
}
