using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{

    [DataContract]
    public class CommentairesEvenementMatch
    {
        [DataMember]
        private Evenement _evenement;
        [DataMember]
        private List<string> _commentaires;

        public List<string> Commentaires { get => _commentaires; }
        public Evenement Evenement { get => _evenement; }

        public CommentairesEvenementMatch(Evenement evenement)
        {
            _evenement = evenement;
            _commentaires = new List<string>();
        }

        public string Commentaire(EvenementMatch em)
        {
            string commentaireBrut = Commentaires[Session.Instance.Random(0, Commentaires.Count - 1)];
            commentaireBrut = commentaireBrut.Replace(" CLUB ", " " + em.Club.NomCourt + " ");
            commentaireBrut = commentaireBrut.Replace(" JOUEUR ", " " + em.Joueur.Nom + " ");
            return commentaireBrut;
        }
    }
    

    [DataContract(IsReference =true)]
    public class Gestionnaire
    {

        

        [DataMember]
        private List<Club> _clubs;
        [DataMember]
        private List<Joueur> _joueursLibres;
        [DataMember]
        private List<Entraineur> _entraineursLibres;
        [DataMember]
        private List<Continent> _continents;
        [DataMember]
        private List<Langue> _langues;
        [DataMember]
        private List<Media> _medias;
        [DataMember]
        private List<CommentairesEvenementMatch> _commentairesMatchs;

        public List<Club> Clubs { get => _clubs; }

        public List<Competition> Competitions
        {
            get
            {
                List<Competition> res = new List<Competition>();
                foreach(Continent c in _continents)
                {
                    foreach (Competition cp in c.Competitions()) res.Add(cp);
                    foreach (Pays p in c.Pays) foreach (Competition cp in p.Competitions()) res.Add(cp);
                }
                return res;
            }
        }
        public List<Joueur> JoueursLibres { get => _joueursLibres; }
        public List<Entraineur> EntraineursLibres { get => _entraineursLibres; }
        public List<Continent> Continents { get => _continents; }
        public List<Langue> Langues { get => _langues; }
        public List<Media> Medias { get => _medias; }
        public List<CommentairesEvenementMatch> CommentairesMatchs { get => _commentairesMatchs; }

        public Gestionnaire()
        {
            _clubs = new List<Club>();
            _joueursLibres = new List<Joueur>();
            _continents = new List<Continent>();
            _langues = new List<Langue>();
            _medias = new List<Media>();
            _entraineursLibres = new List<Entraineur>();
            _commentairesMatchs = new List<CommentairesEvenementMatch>();
            _commentairesMatchs.Add(new CommentairesEvenementMatch(Evenement.BUT));
            _commentairesMatchs.Add(new CommentairesEvenementMatch(Evenement.BUT_PENALTY));
            _commentairesMatchs.Add(new CommentairesEvenementMatch(Evenement.CARTON_JAUNE));
            _commentairesMatchs.Add(new CommentairesEvenementMatch(Evenement.CARTON_ROUGE));
            _commentairesMatchs.Add(new CommentairesEvenementMatch(Evenement.TIR));
        }

        public Ville String2Ville(string nom)
        {
            Ville res = null;
            foreach(Continent c in _continents)
            {
                foreach(Pays p in c.Pays)
                {
                    foreach(Ville v in p.Villes)
                    {
                        if (v.Nom == nom) res = v;
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Donne la liste de tous les matchs des compétitions en cours
        /// </summary>
        public List<Match> Matchs
        {
            get
            {
                List<Match> res = new List<Match>();
                foreach(Competition c in Competitions)
                {
                    foreach(Tour t in c.Tours)
                    {
                        foreach (Match m in t.Matchs) res.Add(m);
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// Liste des joueurs transférables d'un championnat
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public List<Joueur> ListeTransferts(Competition c)
        {
            List<Joueur> joueurs = new List<Joueur>();
            Tour tourChampionnat = c.Tours[0];

            foreach(Club club in tourChampionnat.Clubs)
            {
                Club_Ville cv = club as Club_Ville;
                if(cv != null)
                {
                    foreach (Contrat ct in cv.Contrats)
                    {
                        if (ct.Transferable) joueurs.Add(ct.Joueur);
                    }
                }
            }

            return joueurs;
        }

        public Pays String2Pays(string pays)
        {
            Pays res = null;

            foreach(Continent c in _continents)
            {
                foreach(Pays p in c.Pays)
                {
                    if (p.Nom() == pays) res = p;
                }
            }
            return res;
        }

        public Stade String2Stade(string stade)
        {
            Stade res = null;

            foreach (Continent c in _continents)
            {
                foreach(Pays p in c.Pays)
                {
                    foreach(Stade s in p.Stades)
                    {
                        if (s.Nom == stade) res = s;
                    }
                }
            }

            return res;
        }

        public Competition String2Competition(string nom)
        {
            Competition res = null;
            foreach(Competition competition in Competitions)
            {
                if (competition.Nom == nom) res = competition;
            }
            return res;
        }

        public Club String2Club(string nom)
        {
            Club res = null;
            foreach(Club c in _clubs)
            {
                if (c.Nom == nom) res = c;
            }

            return res;
        }

        public Langue String2Langue(string nom)
        {
            Langue res = null;

            foreach (Langue l in _langues) if (l.Nom == nom) res = l;

            return res;
        }

        public Continent String2Continent(string nom)
        {
            Continent res = null;
            foreach (Continent c in _continents) if (c.Nom() == nom) res = c;
            return res;
        }

        public int NombreJoueursPays(Pays p)
        {
            int res = 0;
            foreach(Joueur j in _joueursLibres)
            {
                if (j.Nationalite == p) res++;
            }
            foreach(Club c in _clubs)
            {
                if (c as Club_Ville != null) foreach (Joueur j in c.Joueurs()) if (j.Nationalite == p) res++;
            }
            return res;
        }

        public List<Joueur> ListerJoueursPays(Pays p)
        {
            List<Joueur> res = new List<Joueur>();
            foreach (Joueur j in _joueursLibres)
            {
                if (j.Nationalite == p) res.Add(j); ;
            }
            foreach (Club c in _clubs)
            {
                if (c as Club_Ville != null) foreach (Joueur j in c.Joueurs()) if (j.Nationalite == p) res.Add(j);
            }
            return res;
        }

        public void AppelsSelection()
        {
            foreach(Club c in _clubs)
            {
                SelectionNationale sn = c as SelectionNationale;
                if (sn != null) sn.AppelSelection(ListerJoueursPays(sn.Pays));
            }
        }

        /// <summary>
        /// Fait partir en retraite les joueurs libres trop vieux
        /// </summary>
        public void RetraiteJoueursLibres()
        {
            List<Joueur> partentEnRetraite = new List<Joueur>();
            foreach(Joueur j in _joueursLibres)
            {
                if (j.Age > 33)
                    if (Session.Instance.Random(1, 3) == 1)
                        partentEnRetraite.Add(j);
            }

            foreach (Joueur j in partentEnRetraite)
            {
                _joueursLibres.Remove(j);
            }
        }

        /// <summary>
        /// Récupére une localisation (continent ou pays) à partir d'un nom
        /// </summary>
        /// <param name="nom"></param>
        /// <returns></returns>
        public ILocalisation String2Localisation(string nom)
        {
            ILocalisation res = null;
            foreach(Continent c in _continents)
            {
                if (c.Nom() == nom) res = c;
                foreach(Pays p in c.Pays)
                {
                    if (p.Nom() == nom) res = p;
                }
            }
            return res;
        }

        public ILocalisation LocalisationCompetition(Competition competition)
        {
            ILocalisation res = null;
            foreach(Continent c in _continents)
            {
                if(c.Competitions().Contains(competition)) res = c;
                foreach (Pays p in c.Pays) if (p.Competitions().Contains(competition)) res = p;
            }
            return res;
        }

        public void AjouterMatchAmical(Match m)
        {
            Competition amc = String2Competition("Matchs amicaux");
            amc.Tours[0].Matchs.Add(m);
        }

        public void AjouterCommmentaireMatch(Evenement evenement, string commentaire)
        {
            foreach(CommentairesEvenementMatch cem in _commentairesMatchs)
            {
                if (cem.Evenement == evenement) cem.Commentaires.Add(commentaire);
            }
        }

        public string Commentaire(EvenementMatch evenement)
        {
            string res = "";
            foreach(CommentairesEvenementMatch cem in _commentairesMatchs)
            {
                if (cem.Evenement == evenement.Type)
                    res = cem.Commentaire(evenement);
            }

            return res;
        }

    }
}
