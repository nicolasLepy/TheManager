using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class TourElimination : Tour
    {
        public TourElimination(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, bool allerRetour,DateTime initialisation, DateTime fin) : base(nom, heure, dates, decalages, initialisation,fin, allerRetour)
        {

        }

        public override void Initialiser()
        {
            _matchs = Calendrier.TirageAuSort(this);
        }

        public override void QualifierClubs()
        {

            foreach (Qualification q in _qualifications)
            {
                foreach (Match m in _matchs)
                {
                    //Vainqueurs
                    if(q.Classement == 1)
                    {
                        q.Competition.Tours[q.IDTour].Clubs.Add(m.Vainqueur);
                    }
                    else
                        q.Competition.Tours[q.IDTour].Clubs.Add(m.Perdant);
                }
            }
        }
    }
}