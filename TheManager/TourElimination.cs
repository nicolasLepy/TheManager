using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;
using TheManager.Comparators;

namespace TheManager
{


    [DataContract(IsReference =true)]
    public class TourElimination : Tour
    {


        public TourElimination(string nom, Hour heure, List<DateTime> dates, List<DecalagesTV> decalages, bool allerRetour,DateTime initialisation, DateTime fin) : base(nom, heure, dates, decalages, initialisation,fin, allerRetour,0)
        {
        }

        public override Tour Copie()
        {
            Tour t = new TourElimination(Nom, this.Programmation.HeureParDefaut, new List<DateTime>(Programmation.JoursDeMatchs), new List<DecalagesTV>(Programmation.DecalagesTV), AllerRetour, Programmation.Initialisation, Programmation.Fin);
            foreach (Match m in this.Matchs) t.Matchs.Add(m);
            foreach (Club c in this.Clubs) t.Clubs.Add(c);
            return t;
        }

        public override List<Match> ProchaineJournee()
        {
            List<Match> res = new List<Match>(this.Matchs);

            if (AllerRetour)
            {
                bool matchsAllersTousJoues = true;
                for(int i = 0;i<res.Count/2; i++)
                {
                    if (!Matchs[i].Played) matchsAllersTousJoues = false;
                }
                int deb = 0;
                if (matchsAllersTousJoues)
                    deb = res.Count / 2;
                res = new List<Match>(res.GetRange(deb, res.Count/2));
            }

            try
            {
                res.Sort(new MatchDateComparator());
            }
            catch
            {
                Console.WriteLine("TourElimination : match date comparator failed");
            }

            return res;
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
                        CityClub cv = m.Winner as CityClub;
                        if(cv != null)
                        {
                            cv.ModifyBudget(d.Somme);
                        }
                    }
                }
                if(d.Classement == 2)
                {
                    foreach (Match m in matchs)
                    {
                        CityClub cv = m.Looser as CityClub;
                        if (cv != null)
                        {
                            cv.ModifyBudget(d.Somme);
                        }
                    }
                }
            }
        }

        public override void Initialiser()
        {
            AjouterEquipesARecuperer();
            _matchs = Calendar.Draw(this);
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
                    //Winners
                    if (q.Classement == 1)
                    {
                        c = m.Winner;
                        if (!q.AnneeSuivante) q.Competition.rounds[q.IDTour].Clubs.Add(c);
                        else q.Competition.AddClubForNextYear(c, q.IDTour);
                    }
                    //Losers
                    else if (q.Classement == 2)
                    {
                        c = m.Looser;
                        if (!q.AnneeSuivante) q.Competition.rounds[q.IDTour].Clubs.Add(c);
                        else q.Competition.AddClubForNextYear(c, q.IDTour);
                    }
                    if(c != null)
                    {
                        if (q.Competition.isChampionship && c.Championship != null)
                        {
                            if (q.Competition.level > c.Championship.level)
                                c.supporters = (int)(c.supporters * 1.4f);
                            else if (q.Competition.level < c.Championship.level)
                                c.supporters = (int)(c.supporters / 1.4f);
                        }
                    }
                }
            }
            
        }
        public override Club Vainqueur()
        {
            Club res = null;
            if(_clubs.Count > 0)
            {
                res = _clubs[0];
            }
            return res;
        }
    }
}