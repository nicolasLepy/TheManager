using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class Journalistes_Comparator : IComparer<Journalist>
    {
        private Ville _ville;

        public Journalistes_Comparator(Ville ville)
        {
            _ville = ville;
        }

        public int Compare(Journalist x, Journalist y)
        {
            float distanceX = Math.Abs(Utils.Distance(x.@base, _ville)) + x.offset;
            float distanceY = Math.Abs(Utils.Distance(y.@base, _ville)) + y.offset;
            return (int)(distanceX - distanceY);
        }
    }
}