using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class TourChampionnat : Tour
    {
        private int _nombreQualifies;

        public TourChampionnat(string nom, Heure heure, List<DateTime> jours, bool allerRetour, List<DecalagesTV> decalages, int qualifies) : base(nom, heure, jours, decalages, allerRetour)
        {
            _nombreQualifies = qualifies;
        }
    }
}