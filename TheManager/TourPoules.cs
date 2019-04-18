using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    public class TourPoules : Tour
    {
        private int _nombrePoules;
        private List<Club>[] _poules;

        public List<Club>[] Poules { get => _poules; }

        public int NombrePoules { get { return _nombrePoules; } }

        public TourPoules(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, int nombrePoules, bool allerRetour, DateTime initialisation, DateTime fin) : base(nom, heure, dates, decalages, initialisation,fin, allerRetour,0)
        {
            _nombrePoules = nombrePoules;
            _poules = new List<Club>[_nombrePoules];
            for (int i = 0; i < _nombrePoules; i++) _poules[i] = new List<Club>();
        }

        public override Tour Copie()
        {
            TourPoules t = new TourPoules(Nom, this.Programmation.HeureParDefaut, new List<DateTime>(Programmation.JoursDeMatchs), new List<DecalagesTV>(Programmation.DecalagesTV), NombrePoules, AllerRetour, Programmation.Initialisation, Programmation.Fin);
            foreach (Match m in this.Matchs) t.Matchs.Add(m);
            foreach (Club c in this.Clubs) t.Clubs.Add(c);
            int i = 0;
            foreach (List<Club> c in _poules)
            {
                t._poules[i] = new List<Club>(c);
                i++;
            }
            return t;
        }

        public override void Initialiser()
        {
            _poules = new List<Club>[_nombrePoules];
            for (int i = 0; i < _nombrePoules; i++) _poules[i] = new List<Club>();
            AjouterEquipesARecuperer();
            DefinirPoules();
            for (int i = 0; i < _nombrePoules; i++)
            {
                _matchs.AddRange(Calendrier.GenererCalendrier(_poules[i], _programmation, AllerRetour));
            }

        }

        public override void QualifierClubs()
        {
            List<Club>[] poules = new List<Club>[_nombrePoules];
            for (int i = 0; i < _nombrePoules; i++)
            {
                poules[i] = new List<Club>(Classement(i));
            }
            for (int i = 0; i < _nombrePoules; i++)
            {
                foreach(Qualification q in _qualifications)
                {
                    Club c = poules[i][q.Classement - 1];
                    Console.WriteLine(c.Nom + " qualifié poule");
                    if (!q.AnneeSuivante) q.Competition.Tours[q.IDTour].Clubs.Add(c);
                    else q.Competition.AjouterClubAnneeSuivante(c, q.IDTour);
                }
            }
        }

        public List<Match> ProchaineJournee()
        {
            List<Match> res = new List<Match>();
            return ProchainsMatchs();

        }

        public List<Match> Journee(int journee)
        {
            List<Match> res = new List<Match>();
            int matchsParPoule = MatchsParJournee() * ((_clubs.Count / _nombrePoules) - 1);
            if (AllerRetour) matchsParPoule *= 2;
            for (int i = 0; i < _nombrePoules; i++)
            {
                int indiceBase = (matchsParPoule * i) + (MatchsParJournee() * (journee - 1));
                for (int j = 0; j < MatchsParJournee(); j++)
                {
                    res.Add(_matchs[j + indiceBase]);
                }

            }

            return res;
        }

        public int MatchsParJournee()
        {
            return (_clubs.Count / _nombrePoules) / 2;
        }

        public List<Club> Classement(int poule)
        {
            List<Club> res = new List<Club>(_poules[poule]);
            Club_Classement_Comparator comparator = new Club_Classement_Comparator(this);
            res.Sort(comparator);
            return res;
        }


        private void DefinirPoules()
        {
            List<Club> pot = new List<Club>(_clubs);
            pot.Sort(new Club_Niveau_Comparator());
            int equipesParPoule = _clubs.Count / _nombrePoules;
            List<Club>[] pots = new List<Club>[equipesParPoule];
            int ind = 0;
            for (int i = 0; i < equipesParPoule; i++)
            {
                pots[i] = new List<Club>();
                for (int j = 0; j < _nombrePoules; j++)
                {
                    pots[i].Add(pot[ind]);
                    ind++;
                }

            }
            //Pour chaque poule
            for (int i = 0; i < _nombrePoules; i++)
            {
                //Pour chaque pot
                for (int j = 0; j < equipesParPoule; j++)
                {
                    Club c = pots[j][Session.Instance.Random(0, pots[j].Count)];
                    pots[j].Remove(c);
                    _poules[i].Add(c);
                }
            }
        }

        public override void DistribuerDotations()
        {
        }
    }
}