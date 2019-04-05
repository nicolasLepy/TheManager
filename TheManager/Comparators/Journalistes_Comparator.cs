using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class Journalistes_Comparator : IComparer<Journaliste>
    {
        private Ville _ville;

        public Journalistes_Comparator(Ville ville)
        {
            _ville = ville;
        }

        public int Compare(Journaliste x, Journaliste y)
        {
            float distanceX = Math.Abs(Utils.Distance(x.Base, _ville)) + x.Retrait;
            float distanceY = Math.Abs(Utils.Distance(y.Base, _ville)) + y.Retrait;
            return (int)(distanceX - distanceY);
        }
    }
}