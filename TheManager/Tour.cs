using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{


    public enum Regle
    {
        RECOIT_SI_DEUX_DIVISION_ECART
    }

 

    public enum MethodeRecuperation
    {
        ALEATOIRE,
        MEILLEURS,
        PIRES
    }

    public struct RecuperationEquipes
    {
        public IEquipesRecuperables Source { get; set; }
        public int Nombre { get; set; }
        public MethodeRecuperation Methode { get; set; }
        public RecuperationEquipes(IEquipesRecuperables source, int nombre, MethodeRecuperation methode)
        {
            Source = source;
            Nombre = nombre;
            Methode = methode;
        }
    }


    public class ProgrammationTour
    {
        private Heure _heureParDefaut;
        private List<DateTime> _joursDeMatchs;
        private List<DecalagesTV> _decalagesTV;
        private DateTime _initialisation;
        private DateTime _fin;
        private int _dernieresJourneesMemeJour;

        public Heure HeureParDefaut { get => _heureParDefaut; }
        public List<DateTime> JoursDeMatchs { get => _joursDeMatchs; }
        public List<DecalagesTV> DecalagesTV { get => _decalagesTV; }
        public DateTime Initialisation { get => _initialisation; }
        public DateTime Fin { get => _fin; }
        public int DernieresJourneesMemeJour { get => _dernieresJourneesMemeJour; }

        public ProgrammationTour(Heure heure, List<DateTime> jours, List<DecalagesTV> decalages, DateTime initialisation, DateTime fin, int dernieresJourneesMemeJour)
        {
            _heureParDefaut = heure;
            _joursDeMatchs = new List<DateTime>(jours);
            _decalagesTV = decalages;
            _initialisation = initialisation;
            _fin = fin;
            _dernieresJourneesMemeJour = dernieresJourneesMemeJour;
        }
    }

    public struct DecalagesTV
    {
        public int DecalageJours { get; set; }
        public Heure Heure { get; set; }
        
        public DecalagesTV(int nbjours, Heure heure)
        {
            DecalageJours = nbjours;
            Heure = heure;
        }
    }

    public struct Qualification
    {
        public int Classement { get; set; }
        public int IDTour { get; set; }
        public Competition Competition { get; set; }
        public bool AnneeSuivante { get; set; }

        public Qualification(int classement, int idtour, Competition competition, bool anneeSuivante)
        {
            Classement = classement;
            IDTour = idtour;
            Competition = competition;
            AnneeSuivante = anneeSuivante;
        }
    }

    public struct Dotation
    {
        public int Classement { get; set; }
        public int Somme { get; set; }

        public Dotation(int classement, int somme)
        {
            Classement = classement;
            Somme = somme;
        }
    }

    public abstract class Tour : IEquipesRecuperables
    {
        /// <summary>
        /// Nom du tour
        /// </summary>
        protected string _nom;
        /// <summary>
        /// Liste des clubs participant à ce tour
        /// </summary>
        protected List<Club> _clubs;
        /// <summary>
        /// Liste des matchs du tour
        /// </summary>
        protected List<Match> _matchs;
        /// <summary>
        /// Si le tour se déroule en matchs aller-retour
        /// </summary>
        protected bool _allerRetour;

        /// <summary>
        /// Concerne la programmation générale des matchs du tour (TV, heure, jours)
        /// </summary>
        protected ProgrammationTour _programmation;

        protected List<Qualification> _qualifications;

        /// <summary>
        /// Liste des équipes récupérées d'autres compétitions en cours
        /// </summary>
        protected List<RecuperationEquipes> _recuperationsEquipes;

        /// <summary>
        /// Règles concernant le tour
        /// Exemple : l'équipe reçoit si elle évolue deux division en moins
        /// </summary>
        protected List<Regle> _regles;

        /// <summary>
        /// Liste des dotations données aux clubs à la fin du tour
        /// </summary>
        protected List<Dotation> _dotations;


        public string Nom { get => _nom; }
        public List<Club> Clubs { get => _clubs; }
        public List<Match> Matchs { get => _matchs; }
        public bool AllerRetour { get => _allerRetour; }
        public ProgrammationTour Programmation { get => _programmation; }
        public List<Qualification> Qualifications { get => _qualifications; }
        public List<RecuperationEquipes> RecuperationEquipes { get => _recuperationsEquipes; }
        public List<Regle> Regles { get => _regles; }
        public List<Dotation> Dotations { get => _dotations; }

        /*public Competition Competition
        {
            get
            {
                Competition competition = null;

                foreach(Competition c in Session.Instance.Partie.Gestionnaire.Competitions)
                {
                    foreach(Tour t in c.Tours)
                    {
                        if(t == this)
                        {
                            competition = c;
                        }
                    }
                }

                return competition;
            }
        }*/

        public Tour(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, DateTime initialisation, DateTime fin, bool allerRetour, int dernieresJourneesMemeJour)
        {
            _nom = nom;
            _clubs = new List<Club>();
            _matchs = new List<Match>();
            _programmation = new ProgrammationTour(heure, dates, decalages, initialisation, fin, dernieresJourneesMemeJour);
            _allerRetour = allerRetour;
            _qualifications = new List<Qualification>();
            _recuperationsEquipes = new List<RecuperationEquipes>();
            _regles = new List<Regle>();
            _dotations = new List<Dotation>();
        }


        public float MoyenneButs()
        {
            float res = 0;
            foreach(Match m in _matchs)
            {
                res += m.Score1 + m.Score2;
            }
            return res / ( _matchs.Count+0.0f);
        }

        /// <summary>
        /// Liste des buteurs par ordre décroissant
        /// </summary>
        /// <returns>Une liste de KeyValuePair avec le joueur en clé et son nombre de buts en valeur</returns>
        public List<KeyValuePair<Joueur, int>> Buteurs()
        {
            Dictionary<Joueur, int> buteurs = new Dictionary<Joueur, int>();
            foreach(Match m in _matchs)
            {
                foreach(EvenementMatch em in m.Evenements)
                {
                    if(em.Type == Evenement.BUT || em.Type == Evenement.BUT_PENALTY)
                    {
                        if (buteurs.ContainsKey(em.Joueur)) buteurs[em.Joueur]++;
                        else buteurs[em.Joueur] = 1;
                    }
                }
            }

            List<KeyValuePair<Joueur, int>> liste = buteurs.ToList();

            liste.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            return liste;
        }

        /// <summary>
        /// Renvoi la liste des prochains matchs à se jouer selon la date
        /// </summary>
        /// <returns></returns>
        public List<Match> ProchainsMatchs()
        {
            List<Match> res = new List<Match>();
            bool continuer = true;
            DateTime date = new DateTime(2000, 1, 1);
            int i = 0;
            if (_matchs.Count == 0) continuer = false;
            while (continuer)
            {
                Match m = _matchs[i];
                if (!m.Joue)
                {
                    if (date.Year == 2000)
                    {
                        date = m.Jour;
                        res.Add(m);
                    }
                    else if (date.Date == m.Jour.Date)
                        res.Add(m);
                    else continuer = false;
                }
                if (i == _matchs.Count - 1) continuer = false;
                i++;
            }


            return res;
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

        public int Gagnes(Club c)
        {
            int res = 0;
            foreach (Match m in _matchs)
            {
                if (m.Domicile == c)
                {
                    if (m.Score1 > m.Score2) res++;
                }
                else if (m.Exterieur == c)
                {
                    if (m.Score2 > m.Score1) res++;
                }
            }
            return res;
        }

        public int Nuls(Club c)
        {
            int res = 0;
            foreach (Match m in _matchs)
            {
                if (m.Joue && (m.Domicile == c || m.Exterieur == c))
                {
                    if (m.Score1 == m.Score2) res++;
                }
            }
            return res;
        }

        public int Perdus(Club c)
        {
            int res = 0;
            foreach (Match m in _matchs)
            {
                if (m.Domicile == c)
                {
                    if (m.Score1 < m.Score2) res++;
                }
                else if (m.Exterieur == c)
                {
                    if (m.Score2 < m.Score1) res++;
                }
            }
            return res;
        }

        public int ButsPour(Club c)
        {
            int res = 0;
            foreach (Match m in _matchs)
            {
                if (m.Domicile == c)
                {
                    res += m.Score1;
                }
                else if (m.Exterieur == c)
                {
                    res += m.Score2;
                }
            }
            return res;
        }

        public int ButsContre(Club c)
        {
            int res = 0;
            foreach (Match m in _matchs)
            {
                if (m.Domicile == c)
                {
                    res += m.Score2;
                }
                else if (m.Exterieur == c)
                {
                    res += m.Score1;
                }
            }
            return res;
        }

        public int Difference(Club c)
        {
            return ButsPour(c) - ButsContre(c);
        }

        public void RAZ()
        {
            _matchs = new List<Match>();
            _clubs = new List<Club>();
        }

        /// <summary>
        /// Ajoute les équipes à récupérer d'autres compétitions
        /// Ex : 32ème CDF -> L1
        /// Ex : Euro -> Equipes européennes
        /// </summary>
        public void AjouterEquipesARecuperer()
        {
            foreach(RecuperationEquipes re in _recuperationsEquipes)
            {
                foreach(Club c in re.Source.RecupererEquipes(re.Nombre, re.Methode))
                {
                    _clubs.Add(c);
                }
            }
        }

        /// <summary>
        /// Initialise le tour (tirage au sort, calendrier)
        /// </summary>
        public abstract void Initialiser();
        /// <summary>
        /// Qualifie les clubs pour les tours suivants
        /// </summary>
        public abstract void QualifierClubs();
        public abstract Tour Copie();
        public abstract void DistribuerDotations();
        /// <summary>
        /// Vainqueur du tour
        /// </summary>
        /// <returns></returns>
        public abstract Club Vainqueur();
        


        public List<Club> RecupererEquipes(int nombre, MethodeRecuperation methode)
        {
            List<Club> clubs = new List<Club>(_clubs);
            switch (methode)
            {
                case MethodeRecuperation.ALEATOIRE:
                    clubs = Utils.MelangerListe<Club>(clubs);
                    break;
                case MethodeRecuperation.MEILLEURS:
                    clubs.Sort(new Club_Niveau_Comparator());
                    break;
                case MethodeRecuperation.PIRES:
                    clubs.Sort(new Club_Niveau_Comparator(true));
                    break;
            }
            List<Club> res = new List<Club>();
            for (int i = 0; i < nombre; i++) res.Add(clubs[i]);
            return res;
        }

        public override string ToString()
        {
            return _nom;
        }
    }
}