using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;
using System.Runtime.Serialization;

namespace TheManager
{
    [DataContract(IsReference =true)]
    [KnownType(typeof(Club_Ville))]
    [System.Xml.Serialization.XmlInclude(typeof(Club_Ville))]
    [KnownType(typeof(SelectionNationale))]
    [System.Xml.Serialization.XmlInclude(typeof(SelectionNationale))]
    [KnownType(typeof(Club_Reserve))]
    [System.Xml.Serialization.XmlInclude(typeof(Club_Reserve))]
    public abstract class Club
    {
        [DataMember]
        private string _nom;
        [DataMember]
        private Entraineur _entraineur;
        [DataMember]
        private int _reputation;
        [DataMember]
        private int _supporters;
        [DataMember]
        protected int _centreFormation;
        [DataMember]
        private Stade _stade;
        [DataMember]
        private string _logo;
        [DataMember]
        private string _nomCourt;
        [DataMember]
        private int _prixBillet;

        [DataMember]
        private string _musiqueBut;

        public string Nom { get => _nom; }
        public Entraineur Entraineur { get => _entraineur; set => _entraineur = value; }
        public int Reputation { get => _reputation; }
        public int Supporters { get => _supporters; set => _supporters = value; }
        public int CentreFormation { get => _centreFormation;}
        public Stade Stade { get => _stade; }
        public string Logo { get => _logo; }
        public string NomCourt { get => _nomCourt; }
        public int PrixBillet { get => _prixBillet; }
        public string MusiqueBut { get => _musiqueBut; }

        /// <summary>
        /// Liste des matchs joués par le club
        /// </summary>
        public List<Match> Matchs
        {
            get
            {
                List<Match> res = new List<Match>();

                foreach (Match m in Session.Instance.Partie.Gestionnaire.Matchs)
                {
                    if (m.Domicile == this || m.Exterieur == this) res.Add(m);
                }
                res.Sort(new Match_Date_Comparator());
                return res;
            }
        }

        /// <summary>
        /// Renvoi vrai si le club joue un match proche à la date passée en paramètre (un nombre de jour inférieur au seuil)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool MatchProche(DateTime date, int seuil)
        {
            bool matchProche = false;
            List<Match> matchs = Matchs;
            foreach (Match match in matchs)
            {
                int diff = Utils.NombreJoursEntreDeuxDates(date.Date, match.Jour);
                if (diff < seuil) matchProche = true;
            }
            return matchProche;
        }

        /// <summary>
        /// Donne le nombre de jours entre la date et le match le plus proche joué
        /// </summary>
        /// <param name="date">Regarder par rapport à cette date</param>
        /// <returns></returns>
        /*public int NombreJoursMatchPlusProche(DateTime date)
        {
                int res = -1;
                List<Match> matchs = Matchs;
                foreach(Match m in matchs)
                {
                    if(m.Jour.CompareTo(date) > 0)
                    {
                        //On a dépassé ajd
                        int diffM = Utils.NombreJoursEntreDeuxDates(m.Jour, date); //Match à venir
                        int indexMatch = matchs.IndexOf(m);
                        int diffN = indexMatch > 0 ? Utils.NombreJoursEntreDeuxDates(matchs[indexMatch-1].Jour, date) : -1; //Dernier match du club
                        res = diffM >= diffN ? diffN : diffM;
                    }
                }
                if (res == -1 && matchs.Count > 0)
                    res = Utils.NombreJoursEntreDeuxDates(date, matchs[matchs.Count - 1].Jour);
            if (res == -1) res = 365;
                return res;
            
        }*/

        /// <summary>
        /// Championnat où joue le club
        /// null si le club ne joue dans aucun championnat (par exemple les sélections nationales)
        /// </summary>
        public Competition Championnat
        {
            get
            {
                Competition res = null;

                foreach(Competition c in Session.Instance.Partie.Gestionnaire.Competitions)
                {
                    if(c.Championnat)
                    {
                        
                        foreach (Club cl in c.Tours[0].Clubs)
                        {
                            if (cl == this)
                            {
                                res = c;
                            }
                        }
                            
                                
                    }
                }

                return res;
            }
        }

        public abstract List<Joueur> Joueurs();
        public abstract float Niveau();

        public float Etoiles
        {
            get
            {
                float etoiles = 0.5f;
                float niveau = Niveau();
                if (niveau < 40) etoiles = 0.5f;
                else if (niveau < 50) etoiles = 1f;
                else if (niveau < 57) etoiles = 1.5f;
                else if (niveau < 62) etoiles = 2f;
                else if (niveau < 66) etoiles = 2.5f;
                else if (niveau < 69) etoiles = 3f;
                else if (niveau < 72) etoiles = 3.5f;
                else if (niveau < 75) etoiles = 4f;
                else if (niveau < 79) etoiles = 4.5f;
                else etoiles = 5f;
                return etoiles;
            }
        }

        public Club(string nom, Entraineur entraineur, string nomCourt, int reputation, int supporters, int centreFormation, string logo, Stade stade, string musiqueBut)
        {
            _nom = nom;
            _entraineur = entraineur;
            _nomCourt = nomCourt;
            _reputation = reputation;
            _supporters = supporters;
            _centreFormation = centreFormation;
            _logo = logo;
            _stade = stade;
            _musiqueBut = musiqueBut;
        }

        public List<Joueur> ListerJoueurPoste(Position poste)
        {
            return Utils.JoueursPoste(Joueurs(), poste);
        }

        private List<Joueur> ListerJoueursPosteComposition(Position poste)
        {
            List<Joueur> res = new List<Joueur>();
            foreach (Joueur j in Joueurs())
            {
                if (j.Poste == poste && !j.Suspendu)
                {
                    res.Add(j);
                }
            }
            return res;
        }

        public List<Joueur> Composition()
        {
            List<Joueur> res = new List<Joueur>();

            List<Joueur> joueursPoste = ListerJoueursPosteComposition(Position.Goalkeeper);
            joueursPoste.Sort(new Joueur_Composition_Comparator());
            if (joueursPoste.Count >= 1)
                res.Add(joueursPoste[0]);

            joueursPoste = ListerJoueursPosteComposition(Position.Defender);
            joueursPoste.Sort(new Joueur_Composition_Comparator());
            
            if (joueursPoste.Count > 0) res.Add(joueursPoste[0]);
            if (joueursPoste.Count > 1) res.Add(joueursPoste[1]);
            if (joueursPoste.Count > 2) res.Add(joueursPoste[2]);
            if (joueursPoste.Count > 3) res.Add(joueursPoste[3]);
            

            joueursPoste = ListerJoueursPosteComposition(Position.Midfielder);
            joueursPoste.Sort(new Joueur_Composition_Comparator());

            if (joueursPoste.Count > 0) res.Add(joueursPoste[0]);
            if (joueursPoste.Count > 1) res.Add(joueursPoste[1]);
            if (joueursPoste.Count > 2) res.Add(joueursPoste[2]);
            if (joueursPoste.Count > 3) res.Add(joueursPoste[3]);


            joueursPoste = ListerJoueursPosteComposition(Position.Striker);
            joueursPoste.Sort(new Joueur_Composition_Comparator());
            if (joueursPoste.Count > 0) res.Add(joueursPoste[0]);
            if (joueursPoste.Count > 1) res.Add(joueursPoste[1]);


            return res;
        }

        public void DefinirPrixBillet()
        {
            int niveau = (int)Niveau();
            int prix = 0;

            if (niveau < 20) prix = 1;
            else if (niveau < 30) prix = 2;
            else if (niveau < 40) prix = 3;
            else if (niveau < 50) prix = 5;
            else if (niveau < 60) prix = 10;
            else if (niveau < 70) prix = 20;
            else if (niveau < 80) prix = 30;
            else prix = 45;
            _prixBillet = prix;
        }

        /// <summary>
        /// Change l'entraineur du club et met l'ancien entraineur dans les entraineurs libres
        /// </summary>
        /// <param name="nouvelEntraineur"></param>
        public void ChangerEntraineur(Entraineur nouvelEntraineur)
        {
            Session.Instance.Partie.Gestionnaire.EntraineursLibres.Add(_entraineur);
            _entraineur = nouvelEntraineur;
        }


        public override string ToString()
        {
            return Nom;
        }


    }
}