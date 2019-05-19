using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{


    [DataContract(IsReference =true)]
    public class TourElimination : Tour
    {


        public TourElimination(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, bool allerRetour,DateTime initialisation, DateTime fin) : base(nom, heure, dates, decalages, initialisation,fin, allerRetour,0)
        {
        }

        public override Tour Copie()
        {
            Tour t = new TourElimination(Nom, this.Programmation.HeureParDefaut, new List<DateTime>(Programmation.JoursDeMatchs), new List<DecalagesTV>(Programmation.DecalagesTV), AllerRetour, Programmation.Initialisation, Programmation.Fin);
            foreach (Match m in this.Matchs) t.Matchs.Add(m);
            foreach (Club c in this.Clubs) t.Clubs.Add(c);
            return t;
        }

        public override void DistribuerDotations()
        {
            List<Match> matchs;
            if(AllerRetour)
            {
                matchs = new List<Match>(_matchs);
            }
            else
            {
                matchs = new List<Match>();
                int nbMatchs = _matchs.Count;
                for(int i = 0; i<nbMatchs; i++)
                {
                    matchs.Add(_matchs[i]);
                }
            }
            foreach(Dotation d in _dotations)
            {
                if(d.Classement == 1)
                {
                    foreach(Match m in matchs)
                    {
                        Club_Ville cv = m.Vainqueur as Club_Ville;
                        if(cv != null)
                        {
                            cv.ModifierBudget(d.Somme);
                        }
                    }
                }
                if(d.Classement == 2)
                {
                    foreach (Match m in matchs)
                    {
                        Club_Ville cv = m.Perdant as Club_Ville;
                        if (cv != null)
                        {
                            cv.ModifierBudget(d.Somme);
                        }
                    }
                }
            }
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
                    Club c = null;
                    //Vainqueurs
                    if (q.Classement == 1)
                    {
                        c = m.Vainqueur;
                        if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                        else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                    }
                    //Perdants
                    else if (q.Classement == 2)
                    {
                        c = m.Perdant;
                        if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                        else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                    }
                    if(c != null)
                    {
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
        public override Club Vainqueur()
        {
            return _matchs[_matchs.Count - 1].Vainqueur;
        }
    }
}