using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    public class TourInactif : Tour
    {

        public TourInactif(string nom, Heure heure, DateTime initialisation, DateTime fin) :base(nom,heure,new List<DateTime>(),new List<DecalagesTV>(),initialisation,fin,false,0)
        { }

        public override Tour Copie()
        {
            Tour t = new TourElimination(Nom, this.Programmation.HeureParDefaut, new List<DateTime>(Programmation.JoursDeMatchs), new List<DecalagesTV>(Programmation.DecalagesTV), AllerRetour, Programmation.Initialisation, Programmation.Fin);
            foreach (Match m in this.Matchs) t.Matchs.Add(m);
            foreach (Club c in this.Clubs) t.Clubs.Add(c);
            return t;

        }

        public override void DistribuerDotations()
        {
        }

        public override void Initialiser()
        {
            
        }

        public override void QualifierClubs()
        {
            List<Club> classement = new List<Club>(_clubs);
            classement.Sort(new Club_Classement_Random_Comparator());
            Console.WriteLine("Tour inactif : " + _nom);
            foreach(Club c in classement)
            {
                Console.WriteLine(c.Nom + " - " + c.Niveau());
            }
            foreach(Qualification q in _qualifications)
            {
                Club c = classement[q.Classement - 1];
                if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
            }
        }
    }
}