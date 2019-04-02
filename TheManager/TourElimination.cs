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
            AjouterEquipesARecuperer();
            _matchs = Calendrier.TirageAuSort(this);
            Console.WriteLine(Session.Instance.Partie.Date.ToShortDateString() + " - " + Nom + " - " + _matchs.Count);
        }

        public override void QualifierClubs()
        {
            List<Match> matchs = new List<Match>();
            if (!AllerRetour) matchs = new List<Match>(_matchs);
            else
            {
                for (int i = 0; i < _matchs.Count / 2; i++) matchs.Add(_matchs[_matchs.Count / 2 + i]);
            }

            foreach (Qualification q in _qualifications)
            {
                foreach (Match m in matchs)
                {
                    //Vainqueurs
                    if (q.Classement == 1)
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

            /*if(AllerRetour)
            {
                foreach (Qualification q in _qualifications)
                {
                    for(int i = 0; i<_matchs.Count/2; i++)
                    {
                        Match aller = _matchs[i];
                        Match retour = _matchs[i+_matchs.Count/2];
                        int score1 = aller.Score1 + retour.Score2;
                        int score2 = aller.Score2 + retour.Score1;
                        if(score1 == score2)
                        {
                            score1 = aller.Score1 + 2 * retour.Score2;
                            score2 = 2* aller.Score2 + retour.Score1;
                        }
                        if(q.Classement == 1)
                        {
                            if (score1 > score2)
                            {
                                Club c = aller.Domicile;
                                if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                                else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                            }
                            else
                            {
                                Club c = aller.Exterieur;
                                if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                                else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                            }
                        }
                        else
                        {
                            if (score1 < score2)
                            {
                                Club c = aller.Domicile;
                                if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                                else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                            }
                            else
                            {
                                Club c = aller.Exterieur;
                                if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                                else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                            }
                        }
                        
                    }
                }
            }
            else
            {
                foreach (Qualification q in _qualifications)
                {
                    foreach (Match m in _matchs)
                    {
                        //Vainqueurs
                        if (q.Classement == 1)
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
            }*/
        }
    }
}