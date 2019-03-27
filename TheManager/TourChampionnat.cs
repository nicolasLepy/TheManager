using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    public class TourChampionnat : Tour, ITourAvecClassement
    {

        public TourChampionnat(string nom, Heure heure, List<DateTime> jours, bool allerRetour, List<DecalagesTV> decalages) : base(nom, heure, jours, decalages, allerRetour)
        {
        }

        public override void Initialiser()
        {
            _matchs = Calendrier.GenererCalendrier(this.Clubs, this.Programmation.JoursDeMatchs, this.Programmation.HeureParDefaut, this.Programmation.DecalagesTV);
        }

        public override void QualifierClubs()
        {
            List<Club> classement = Classement();
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




        public int Points(Club c)
        {
            int points = 0;
            foreach (Match m in _matchs)
            {
                if (m.Joue)
                {
                    if (m.Domicile == c)
                    {
                        if (m.Score1 > m.Score2)
                            points += 3;
                        else if (m.Score2 == m.Score1)
                            points++;
                    }
                    else if (m.Exterieur == c)
                    {
                        if (m.Score2 > m.Score1)
                            points += 3;
                        else if (m.Score2 == m.Score1)
                            points++;
                    }
                }
            }

            return points;
        }

        public int Joues(Club c)
        {
            int joues = 0;
            foreach (Match m in _matchs)
            {
                if (m.Joue && (m.Domicile == c || m.Exterieur == c)) joues++;
            }
            return joues;
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