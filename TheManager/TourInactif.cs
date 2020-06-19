using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference =true)]
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

        public override List<Match> ProchaineJournee()
        {
            return new List<Match>();
        }

        public override Club Vainqueur()
        {
            return _clubs[0];
        }

        public override void QualifierClubs()
        {
            List<Club> classement = new List<Club>(_clubs);
            try
            {
                classement.Sort(new Club_Classement_Random_Comparator());
            }
            catch
            {
                Console.WriteLine("Le classement aléatoire n'a pu être généré");
            }

            //Simuler des gains d'argent de matchs pour les clubs (affluence)


            DateTime fin = new DateTime(Programmation.Fin.Year, Programmation.Fin.Month, Programmation.Fin.Day);
            if (fin.Month < Programmation.Initialisation.Month)
                fin = fin.AddYears(1);
            else if (fin.Month == Programmation.Initialisation.Month && fin.Day < Programmation.Initialisation.Day)
                fin = fin.AddYears(1);
            int nbMatchs = (int)((fin - Programmation.Initialisation).TotalDays) / 14;
            foreach (Club c in classement)
            {
                Club_Ville cv = c as Club_Ville;
                if(cv != null)
                {
                    cv.ModifierBudget(nbMatchs * cv.Supporters * cv.PrixBillet);
                }
            }
            foreach(Qualification q in _qualifications)
            {
                Club c = classement[q.Classement - 1];
                if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                if (q.Competition.Championnat && c.Championnat != null)
                {
                    if (q.Competition.Niveau > c.Championnat.Niveau)
                        c.Supporters = (int)(c.Supporters * 1.4f);
                    else if (q.Competition.Niveau < c.Championnat.Niveau)
                        c.Supporters = (int)(c.Supporters / 1.4f);
                }
            }
        }
    }
}