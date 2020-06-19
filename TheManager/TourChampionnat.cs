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
    public class TourChampionnat : Tour
    {

        public TourChampionnat(string nom, Heure heure, List<DateTime> jours, bool allerRetour, List<DecalagesTV> decalages, DateTime initialisation, DateTime fin, int dernieresJourneesMemeJour) : base(nom, heure, jours, decalages, initialisation,fin, allerRetour, dernieresJourneesMemeJour)
        {
        }

        public override Tour Copie()
        {
            Tour t = new TourChampionnat(Nom, this.Programmation.HeureParDefaut, new List<DateTime>(Programmation.JoursDeMatchs), AllerRetour, new List<DecalagesTV>(Programmation.DecalagesTV), Programmation.Initialisation, Programmation.Fin, Programmation.DernieresJourneesMemeJour);
            foreach (Match m in this.Matchs) t.Matchs.Add(m);
            foreach (Club c in this.Clubs) t.Clubs.Add(c);
            return t;
        }

        public override void Initialiser()
        {
            AjouterEquipesARecuperer();
            _matchs = Calendrier.GenererCalendrier(this.Clubs, this.Programmation, AllerRetour);

        }

        public override void QualifierClubs()
        {
            List<Club> classement = Classement();

            List<Club> qualifies = new List<Club>();
            foreach(Qualification q in Qualifications)
            {
                Club c = classement[q.Classement-1];
                if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                if(q.Competition.Championnat && c.Championnat != null)
                {
                    if (q.Competition.Niveau > c.Championnat.Niveau)
                        c.Supporters = (int)(c.Supporters * 1.4f);
                    else if (q.Competition.Niveau < c.Championnat.Niveau)
                        c.Supporters = (int)(c.Supporters / 1.4f);
                }
            }
        }

        public List<Club> Classement()
        {
            Club_Classement_Comparator comparator = new Club_Classement_Comparator(this.Matchs);
            List<Club> classement = new List<Club>(_clubs);
            classement.Sort(comparator);
            return classement;
        }

        public List<Club> ClassementSansReserves(int poule)
        {
            List<Club> res = new List<Club>();
            foreach (Club c in _clubs)
            {
                if ((c as Club_Reserve) == null) res.Add(c);
            }
            Club_Classement_Comparator comparator = new Club_Classement_Comparator(this.Matchs);
            res.Sort(comparator);
            return res;
        }

        public int NombreJournees()
        {
            int nbEquipes = Clubs.Count;
            int nbMatchs = Matchs.Count;
            if (nbEquipes % 2 == 1) nbEquipes++;
            int nbJournees = nbMatchs / nbEquipes;
            if (AllerRetour) nbJournees *= 2;
            return nbJournees ;
        }
        
        public override List<Match> ProchaineJournee()
        {
            List<Match> res = new List<Match>();

            int indMatch = -1;
            DateTime plusTot = new DateTime(2000, 1, 1);
            int i = 0;
            foreach (Match m in _matchs)
            {
                if (!m.Joue && (DateTime.Compare(m.Jour, plusTot) < 0 || plusTot.Year == 2000))
                {
                    plusTot = m.Jour;
                    indMatch = i;
                }
                i++;
            }

            int total = MatchsParJournee();
            int journee = (indMatch / total);
            res = Journee(journee + 1); //+1 car journee va de 0 à n-1
            res.Sort(new Match_Date_Comparator());

            return res;
        }

        /// <summary>
        /// Journée d'un match
        /// </summary>
        /// <param name="match">Le match</param>
        /// <returns>La journée durant laquelle est joué le match</returns>
        public int Journee(Match match)
        {
            return (_matchs.IndexOf(match) / MatchsParJournee()) + 1;
        }

        /// <summary>
        /// Nombre de matchs par jourées
        /// </summary>
        /// <returns></returns>
        private int MatchsParJournee()
        {
            int total = _clubs.Count;
            total /= 2;
            return total;
        }

        /// <summary>
        /// Liste les matchs d'une journée
        /// </summary>
        /// <param name="journee">La journée</param>
        /// <returns>Les matchs d'une journée j</returns>
        public List<Match> Journee(int journee)
        {
            List<Match> res = new List<Match>();
            int total = MatchsParJournee();
            int indice = journee - 1;
            for (int i = indice * total; i < (indice + 1) * total; i++)
            {
                res.Add(_matchs[i]);
            }
            return res;
        }

        public override void DistribuerDotations()
        {
            List<Club> classement = new List<Club>(_clubs);
            classement.Sort(new Club_Classement_Comparator(this.Matchs));
            foreach(Dotation d in _dotations)
            {
                Club_Ville cv = classement[d.Classement-1] as Club_Ville;
                if (cv != null)
                {
                    cv.ModifierBudget(d.Somme);
                }
                    
            }
        }

        public override Club Vainqueur()
        {
            return Classement()[0];
        }
    }
}