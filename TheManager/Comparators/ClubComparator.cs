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
        BUDGET,
        SPONSOR,
        CONTINENTAL_COEFFICIENT,
        ELO
    }

    public class ClubComparator : IComparer<Club>
    {

        private readonly ClubAttribute _attribute;
        private readonly bool _inverted;


        public ClubComparator(ClubAttribute attribute, bool inverted = false)
        {
            _attribute = attribute;
            _inverted = inverted;
        }

        private int Inverted()
        {
            return _inverted ? -1 : 1;
        }

        public int Compare(Club x, Club y)
        {
            int res;
            switch (_attribute)
            {
                case ClubAttribute.STADIUM:
                    res = y.stadium.capacity - x.stadium.capacity;
                    break;
                case ClubAttribute.LEVEL:
                    res = x.Level() > y.Level() ? -1 : 1;
                    break;
                case ClubAttribute.POTENTIEL:
                    res = x.Potential() > y.Potential() ? -1 : 1;
                    break;
                case ClubAttribute.CONTINENTAL_COEFFICIENT:
                    res = x.ClubCoefficient() > y.ClubCoefficient() ? -1 : 1;
                    break;
                case ClubAttribute.BUDGET:
                    if(x as CityClub == null && y as CityClub == null)
                    {
                        res = 0;
                    }
                    else if (x as CityClub != null && y as CityClub == null)
                    {
                        res = -1;
                    }
                    else if (x as CityClub == null && y as CityClub != null)
                    {
                        res = 1;
                    }
                    else
                    {
                        res = (y as CityClub).budget - (x as CityClub).budget;
                    }
                    break;
                case ClubAttribute.SPONSOR:
                    if(x as CityClub != null)
                    {
                        res = (x as CityClub).sponsor > (y as CityClub).sponsor ? -1 : 1;
                    }
                    else
                    {
                        res = 0;
                    }
                    break;
                case ClubAttribute.ELO:
                    return x.elo > y.elo ? -1 : 1;
                default:
                    res = x.Level() > y.Level() ? -1 : 1;
                    break;
            }
            return res * Inverted();
        }
    }
}
