using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Exportation;

namespace TheManager
{

    public class Competition
    {
        
        private string _nom;
        private List<Tour> _tours;
        private string _logo;
        private DateTime _debutSaison;
        private string _nomCourt;
        private List<Club>[] _qualificationAnneeSuivante;
        private bool _championnat;
        private int _niveau;

        public string Nom { get => _nom; }
        public List<Tour> Tours { get => _tours; }
        public string Logo { get => _logo; }
        public int TourActuel { get; set; }
        public DateTime DebutSaison { get { return _debutSaison; } }
        public string NomCourt { get => _nomCourt; }
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
            Exporteur.Exporter(this);
            Competition copieArchive = new Competition(_nom, _logo, _debutSaison, _nomCourt, _championnat, _niveau);
            foreach (Tour t in Tours) copieArchive.Tours.Add(t);
            Session.Instance.Partie.Gestionnaire.CompetitionsArchives.Add(copieArchive);
            for (int i = 0; i<Tours.Count; i++)
            {
                Tours[i].RAZ();
                List<Club> clubs = new List<Club>(_qualificationAnneeSuivante[i]);
                foreach (Club c in clubs) Tours[i].Clubs.Add(c);
            }
            InitialiserQualificationsAnneesSuivantes();
            TourActuel = -1;
        }

        public void TourSuivant()
        {
            if (_tours.Count > TourActuel + 1)
            {
                TourActuel++;
                //if(TourActuel > 0) _tours[TourActuel - 1].QualifierClubs();
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

    }
}