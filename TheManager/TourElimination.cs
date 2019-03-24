using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class TourElimination : Tour
    {
        public TourElimination(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, bool allerRetour) : base(nom, heure, dates, decalages, allerRetour)
        {

        }
    }
}