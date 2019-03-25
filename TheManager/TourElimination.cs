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

        public override void Initialiser()
        {
            _matchs = Calendrier.TirageAuSort(this);
        }

        public override List<Club> Qualifies()
        {
            List<Club> res = new List<Club>();

            foreach (Match m in _matchs)
            {
                res.Add(m.Vainqueur);
            }

            return res;
        }
    }
}