using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{

    public class ProgrammationTour
    {
        private Heure _heureParDefaut;
        private List<DateTime> _joursDeMatchs;
        private List<DecalagesTV> _decalagesTV;
        private DateTime _initialisation;

        public Heure HeureParDefaut { get => _heureParDefaut; }
        public List<DateTime> JoursDeMatchs { get => _joursDeMatchs; }
        public List<DecalagesTV> DecalagesTV { get => _decalagesTV; }
        public DateTime Initialisation { get => _initialisation; }

        public ProgrammationTour(Heure heure, List<DateTime> jours, List<DecalagesTV> decalages, DateTime initialisation)
        {
            _heureParDefaut = heure;
            _joursDeMatchs = new List<DateTime>(jours);
            _decalagesTV = decalages;
            _initialisation = initialisation;
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

        public Qualification(int classement, int idtour, Competition competition)
        {
            Classement = classement;
            IDTour = idtour;
            Competition = competition;
        }
    }

    public abstract class Tour
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

        public string Nom { get => _nom; }
        public List<Club> Clubs { get => _clubs; }
        public List<Match> Matchs { get => _matchs; }
        public bool AllerRetour { get => _allerRetour; }
        public ProgrammationTour Programmation { get => _programmation; }
        public List<Qualification> Qualifications { get => _qualifications; }

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

        public Tour(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, DateTime initialisation, bool allerRetour)
        {
            _nom = nom;
            _clubs = new List<Club>();
            _matchs = new List<Match>();
            _programmation = new ProgrammationTour(heure, dates, decalages, initialisation);
            _allerRetour = allerRetour;
            _qualifications = new List<Qualification>();
        }

        /// <summary>
        /// Joue les matchs du jour
        /// </summary>
        /// <returns>Vrai si au moins un match a été joué, faux sinon</returns>
        public bool JouerMatchs()
        {
            bool res = false;
            foreach (Match m in _matchs)
            {
                if (Session.Instance.Partie.Date.Date == m.Jour.Date)
                {
                    m.Jouer();
                }
            }

            return res;
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

        public abstract void Initialiser();
        public abstract void QualifierClubs();
    }
}