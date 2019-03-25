using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class TourPoules : Tour, ITourAvecClassement
    {
        private int _nombrePoules;
        private List<Club>[] _poules;
        private int _qualifiesParPoule;

        public int NombrePoules { get { return _nombrePoules; } }
        public int QualifiesParPoule { get { return _qualifiesParPoule; } }

        public TourPoules(string nom, DateTime heure, List<DateTime> dates, List<Decalage> decalages, int nombrePoules, bool allerRetour, int qualifiesParPoule) : base(nom, heure, dates, decalages, allerRetour)
        {
            _nombrePoules = nombrePoules;
            _poules = new List<Club>[_nombrePoules];
            for (int i = 0; i < _nombrePoules; i++)
            {
                _poules[i] = new List<Club>();
            }
            _qualifiesParPoule = qualifiesParPoule;
        }

        public override void Initialiser(List<Club> clubs)
        {
            foreach (Club c in clubs)
                _clubs.Add(c);

            DefinirPoules();
            for (int i = 0; i < _nombrePoules; i++)
            {
                _matchs.AddRange(GestionCalendrier.GenererCalendrier(_poules[i], _dateMatchs, _heure, _decalages));
            }

        }

        public override List<Club> Qualifies()
        {
            List<Club> res = new List<Club>();
            List<Club>[] poules = new List<Club>[_nombrePoules];
            for (int i = 0; i < _nombrePoules; i++)
            {
                poules[i] = new List<Club>(Classement(i));
            }
            for (int i = 0; i < _nombrePoules; i++)
            {
                for (int j = 0; j < _qualifiesParPoule; j++)
                {
                    res.Add(poules[i][j]);
                }
            }
            return res;
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
            ClubComparator comparator = new ClubComparator(this);
            res.Sort(comparator);
            return res;
        }


        private void DefinirPoules()
        {
            List<Club> pot = new List<Club>(_clubs);
            pot.Sort(new ClubNiveauComparator());
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
                    Club c = pots[i][UnityEngine.Random.Range(0, pots[i].Count)];
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
                if (m.Equipe1 == c) res += m.Score1;
                else if (m.Equipe2 == c) res += m.Score2;
            }
            return res;
        }

        public int Joues(Club c)
        {
            int res = 0;
            foreach (Match m in _matchs)
            {
                if (m.Termine && (m.Equipe1 == c || m.Equipe2 == c))
                    res++;
            }

            return res;
        }
    }
}