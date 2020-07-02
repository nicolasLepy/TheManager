using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class JournalistsComparator : IComparer<Journalist>
    {
        private readonly City _city;

        public JournalistsComparator(City city)
        {
            _city = city;
        }

        public int Compare(Journalist x, Journalist y)
        {
            float distanceX = Math.Abs(Utils.Distance(x.baseCity, _city)) + x.offset;
            float distanceY = Math.Abs(Utils.Distance(y.baseCity, _city)) + y.offset;
            return (int)(distanceX - distanceY);
        }
    }
}