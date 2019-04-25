using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Exportation;

namespace TheManager
{

    [DataContract]
    public struct StatistiquesCompetitions
    {
        [DataMember]
        public Match PlusGrandScore { get; set; }
        [DataMember]
        public Match PlusGrandEcart { get; set; }
        [DataMember]
        public KeyValuePair<int, Joueur> MeilleurButeursUneSaison { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> PlusGrosseAttaque { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> PlusFaibleAttaque { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> PlusGrosseDefense { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> PlusFaibleDefense { get; set; }

        public StatistiquesCompetitions(int i)
        {
            PlusGrandScore = null;
            PlusGrandEcart = null;
            MeilleurButeursUneSaison = new KeyValuePair<int, Joueur>(0, null);
            PlusGrosseAttaque = new KeyValuePair<int, Club>(0, null);
            PlusFaibleAttaque = new KeyValuePair<int, Club>(0, null);
            PlusGrosseDefense = new KeyValuePair<int, Club>(0, null);
            PlusFaibleDefense = new KeyValuePair<int, Club>(0, null);
        }
    }

    [DataContract(IsReference =true)]
    public class Competition
    {

        [DataMember]
        private string _nom;
        [DataMember]
        private List<Tour> _tours;
        [DataMember]
        private string _logo;
        [DataMember]
        private DateTime _debutSaison;
        [DataMember]
        private string _nomCourt;
        [DataMember]
        private List<Club>[] _qualificationAnneeSuivante;
        [DataMember]
        private bool _championnat;
        [DataMember]
        private int _niveau;
        [DataMember]
        private StatistiquesCompetitions _statistiques;
        [DataMember]
        private List<Competition> _editionsPrecedentes;

        public string Nom { get => _nom; }
        public List<Tour> Tours { get => _tours; }
        public string Logo { get => _logo; }
        [DataMember]
        public int TourActuel { get; set; }
        public DateTime DebutSaison { get { return _debutSaison; } }
        public string NomCourt { get => _nomCourt; }
        public List<Competition> EditionsPrecedentes { get => _editionsPrecedentes; }
        public StatistiquesCompetitions Statistiques { get => _statistiques; set => _statistiques = value; }
        /// <summary>
        /// Est un championnat (L1, L2)
        /// </summary>
        public bool Championnat { get => _championnat; }
        /// <summary>
        /// Niveau dans la hiérarchie (L1 = 1, L2 = 2 ...)
        /// </summary>
        public int Niveau { get => _niveau; }

        public Competition(string nom, string logo, DateTime debutSaison, string nomCourt, bool championnat, int niveau)
        {
            _tours = new List<Tour>();
            _nom = nom;
            _logo = logo;
            _debutSaison = debutSaison;
            TourActuel = -1;
            _nomCourt = nomCourt;
            _championnat = championnat;
            _niveau = niveau;
            _statistiques = new StatistiquesCompetitions(0);
            _editionsPrecedentes = new List<Competition>();
        }

        public void InitialiserQualificationsAnneesSuivantes()
        {
            _qualificationAnneeSuivante = new List<Club>[Tours.Count];
            for(int i =0;i < Tours.Count; i++)
            {
                _qualificationAnneeSuivante[i] = new List<Club>();
            }
        }

        /// <summary>
        /// Fin de la saison, tous les tours sont remis à zéro et les équipes qualifiés pour l'année suivante dispatchées
        /// </summary>
        public void RAZ()
        {
            MAJRecords();
            Competition copieArchive = new Competition(_nom, _logo, _debutSaison, _nomCourt, _championnat, _niveau);
            foreach (Tour t in Tours) copieArchive.Tours.Add(t.Copie());
            copieArchive.Statistiques = Statistiques;
            _editionsPrecedentes.Add(copieArchive);
            for (int i = 0; i<Tours.Count; i++)
            {
                Tours[i].RAZ();
                List<Club> clubs = new List<Club>(_qualificationAnneeSuivante[i]);
                foreach (Club c in clubs) Tours[i].Clubs.Add(c);
            }
            InitialiserQualificationsAnneesSuivantes();
            TourActuel = -1;
            
        }

        private void MAJRecords()
        {
            foreach(Tour t in _tours)
            {
                foreach(Match m in t.Matchs)
                {
                    if (_statistiques.PlusGrandEcart == null || Math.Abs(m.Score1 - m.Score2) > Math.Abs(_statistiques.PlusGrandEcart.Score1 - _statistiques.PlusGrandEcart.Score2))
                        _statistiques.PlusGrandEcart = m;
                    if (_statistiques.PlusGrandScore == null || m.Score1 + m.Score2 > _statistiques.PlusGrandScore.Score1 + _statistiques.PlusGrandScore.Score2)
                        _statistiques.PlusGrandScore = m;
                }
            }
        }

        public void TourSuivant()
        {
            if(TourActuel > -1)
                _tours[TourActuel].DistribuerDotations();
            if (_tours.Count > TourActuel + 1)
            {
                TourActuel++;
                _tours[TourActuel].Initialiser();
            }
        }
        
        /// <summary>
        /// Qualifie un club à un tour de l'édition suivante de la compétition
        /// </summary>
        /// <param name="c">Le club à ajouter</param>
        /// <param name="indexTour">L'index du tour où le club est qualifié</param>
        public void AjouterClubAnneeSuivante(Club c, int indexTour)
        {
            _qualificationAnneeSuivante[indexTour].Add(c);
        }

        public override string ToString()
        {
            return _nom;
        }

        public int AffluenceMoyenne(Club c)
        {
            int i = 0;
            int affluence = 0;
            foreach(Tour t in _tours)
            {
                foreach(Match m in t.Matchs)
                {
                    if((m.Domicile == c) && m.Joue)
                    {
                        affluence += m.Affluence;
                        i++;
                    }
                }
            }
            return i != 0 ? affluence/i : 0;
        }

        public Club Vainqueur()
        {
            Club res = null;
            if(Championnat)
            {
                return _tours[0].Vainqueur();
            }
            else
            {
                return _tours[_tours.Count - 1].Vainqueur();
            }
            return res;
        }

    }
}