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
    public class TourPoules : Tour
    {
        [DataMember]
        private int _nombrePoules;
        [DataMember]
        private List<Club>[] _poules;

        [DataMember]
        private MethodeTirageAuSort _methodeTirageAuSort;
        [DataMember]
        private List<Position> _localisationGroupes;
        [DataMember]
        private List<string> _nomGroupes;
        

        public List<Club>[] Poules { get => _poules; }

        public int NombrePoules { get { return _nombrePoules; } }

        public List<Position> LocalisationGroupes { get => _localisationGroupes; }

        public string NomGroupe(int idGroupe)
        {
            string res = "";
            if (_nomGroupes.Count > idGroupe) res = _nomGroupes[idGroupe];
            else res = "Groupe " + (idGroupe + 1);
            return res;
        }

        public void AjouterNomGroupe(string nom)
        {
            _nomGroupes.Add(nom);
        }

        public TourPoules(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, int nombrePoules, bool allerRetour, DateTime initialisation, DateTime fin, MethodeTirageAuSort methodeTirageAuSort) : base(nom, heure, dates, decalages, initialisation,fin, allerRetour,0)
        {
            _nombrePoules = nombrePoules;
            _poules = new List<Club>[_nombrePoules];
            _nomGroupes = new List<string>();
            for (int i = 0; i < _nombrePoules; i++) _poules[i] = new List<Club>();
            _methodeTirageAuSort = methodeTirageAuSort;
            _localisationGroupes = new List<Position>();
        }

        public override Tour Copie()
        {
            TourPoules t = new TourPoules(Nom, this.Programmation.HeureParDefaut, new List<DateTime>(Programmation.JoursDeMatchs), new List<DecalagesTV>(Programmation.DecalagesTV), NombrePoules, AllerRetour, Programmation.Initialisation, Programmation.Fin, _methodeTirageAuSort);
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
            Club_Classement_Comparator comparator = new Club_Classement_Comparator(this.Matchs);
            res.Sort(comparator);
            return res;
        }

        private void DefinirPoules()
        {
            ITirageAuSort tirage = null;
            switch (_methodeTirageAuSort)
            {
                case MethodeTirageAuSort.NIVEAU:
                    tirage = new TirageAuSortParNiveau(this);
                    break;
                case MethodeTirageAuSort.GEOGRAPHIQUE:
                    tirage = new TirageAuSortGeographique(this);
                    break;
            }
            tirage.TirerAuSort();
        }

        public override void DistribuerDotations()
        {
            foreach(Dotation d in _dotations)
            {
                for(int i = 0;i<NombrePoules; i++)
                {
                    Club_Ville cv = Classement(i)[d.Classement - 1] as Club_Ville;
                    if (cv != null)
                    {
                        cv.ModifierBudget(d.Somme);
                    }
                }

            }
        }

        /// <summary>
        /// N'a pas de sens, il n'y a aucune compétition qui se termine avec une phase de poules
        /// </summary>
        /// <returns></returns>
        public override Club Vainqueur()
        {
            return null;
        }
    }
}