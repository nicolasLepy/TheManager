using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    public class TourPoules : Tour, ITourAvecClassement
    {
        private int _nombrePoules;
        private List<Club>[] _poules;

        public int NombrePoules { get { return _nombrePoules; } }

        public TourPoules(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, int nombrePoules, bool allerRetour) : base(nom, heure, dates, decalages, allerRetour)
        {
            _nombrePoules = nombrePoules;
            _poules = new List<Club>[_nombrePoules];
            for (int i = 0; i < _nombrePoules; i++)
            {
                _poules[i] = new List<Club>();
            }
        }

        public override void Initialiser()
        {

            DefinirPoules();
            for (int i = 0; i < _nombrePoules; i++)
            {
                _matchs.AddRange(Calendrier.GenererCalendrier(_poules[i], _programmation.JoursDeMatchs, _programmation.HeureParDefaut, _programmation.DecalagesTV));
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
                    q.Competition.Tours[q.IDTour].Clubs.Add(c);
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
            int equipeParPoule = _clubs.Count / _nombrePoules;
            List<Club>[] pots = new List<Club>[equipeParPoule];
            int ind = 0;
            for (int i = 0; i < _nombrePoules; i++)
            {
                pots[i] = new List<Club>();
                for (int j = 0; j < equipeParPoule; j++)
                {
                    pots[i].Add(pot[ind]);
                    ind++;
                }

            }
            for (int i = 0; i < _nombrePoules; i++)
            {
                for (int j = 0; j < _clubs.Count / _nombrePoules; j++)
                {
                    Club c = pots[i][Session.Instance.Random(0, pots[i].Count)];
                    pots[i].Remove(c);
                    _poules[i].Add(c);
                }
            }
        }

        public int Points(Club c)
        {
            int res = 0;

            foreach (Match m in _matchs)
            {
                if (m.Domicile == c) res += m.Score1;
                else if (m.Exterieur == c) res += m.Score2;
            }
            return res;
        }

        public int Joues(Club c)
        {
            int res = 0;
            foreach (Match m in _matchs)
            {
                if (m.Joue && (m.Domicile == c || m.Exterieur == c))
                    res++;
            }

            return res;
        }
        
    }
}