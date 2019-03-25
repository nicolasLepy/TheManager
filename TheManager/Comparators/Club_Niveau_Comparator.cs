using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class Club_Niveau_Comparator : IComparer<Club>
    {
        public int Compare(Club x, Club y)
        {
            return (int)(y.Niveau() - x.Niveau());
        }
    }
}
