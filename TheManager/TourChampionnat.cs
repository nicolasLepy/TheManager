using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    public class TourChampionnat : Tour
    {

        public TourChampionnat(string nom, Heure heure, List<DateTime> jours, bool allerRetour, List<DecalagesTV> decalages, DateTime initialisation, DateTime fin) : base(nom, heure, jours, decalages, initialisation,fin, allerRetour)
        {
        }

        public override void Initialiser()
        {
            _matchs = Calendrier.GenererCalendrier(this.Clubs, this.Programmation, AllerRetour);
        }

        public override void QualifierClubs()
        {
            List<Club> classement = Classement();

            foreach(Club c in classement)
            {
                string affiche = c.Nom;
                int ecart = 30 - affiche.Length;
                for (int i = 0; i < ecart; i++) affiche += " ";
                affiche += Points(c) + "  " + Joues(c) + "  " + Gagnes(c) + "  " + Nuls(c) + "  " + Perdus(c) + "  " + ButsPour(c) + "  " + ButsContre(c) + "  " + Difference(c);
                Console.WriteLine(affiche);
            }

            List<Club> qualifies = new List<Club>();
            foreach(Qualification q in Qualifications)
            {
                Club c = classement[q.Classement-1];
                q.Competition.Tours[q.IDTour].Clubs.Add(c);
            }
        }

        public List<Club> Classement()
        {
            Club_Classement_Comparator comparator = new Club_Classement_Comparator(this);
            List<Club> classement = new List<Club>(_clubs);
            classement.Sort(comparator);
            return classement;
        }




        

        public List<Match> ProchaineJournee()
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

    }
}