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

        public override Tour Copie()
        {
            Tour t = new TourElimination(Nom, this.Programmation.HeureParDefaut, new List<DateTime>(Programmation.JoursDeMatchs), new List<DecalagesTV>(Programmation.DecalagesTV), AllerRetour, Programmation.Initialisation, Programmation.Fin);
            foreach (Match m in this.Matchs) t.Matchs.Add(m);
            foreach (Club c in this.Clubs) t.Clubs.Add(c);
            return t;
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
                        Club c = m.Vainqueur;
                        if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                        else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                    }
                    //Perdants
                    else if (q.Classement == 2)
                    {
                        Club c = m.Perdant;
                        if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                        else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                    }
                }
            }
        }
    }
}