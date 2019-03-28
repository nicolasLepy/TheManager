using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    public class Match
    {

        private int _score1;
        private int _score2;
        private List<EvenementMatch> _evenements;
        private List<Joueur> _compo1;
        private List<Joueur> _compo2;

        public DateTime Jour { get; set; }
        public Club Domicile { get; set; }
        public Club Exterieur { get; set; }
        public int Score1 { get => _score1; }
        public int Score2 { get => _score2; }
        public List<EvenementMatch> Evenements { get => _evenements; }
        public List<Joueur> Compo1 { get => _compo1; }
        public List<Joueur> Compo2 { get => _compo2; }
        
        /// <summary>
        /// Si un match a été joué ou non
        /// </summary>
        public bool Joue
        {
            get
            {
                bool res = false;
                //Si la date de la partie est supérieure ou égale au jour du match, alors le match a été joué
                if(DateTime.Compare(Session.Instance.Partie.Date,Jour) >= 0)
                {
                    res = true;
                }
                return res;
            }
        }

       /// <summary>
       /// Donne le vainqueur du match (propriété utile pour les confrontations à élimination directes)
       /// </summary>
        public Club Vainqueur
        {
            get
            {
                Club c = null;
                if(Score1>Score2)
                {
                    c = Domicile;
                }
                else
                {
                    c = Exterieur;
                }
                return c;
            }
        }

        public Club Perdant
        {
            get
            {
                Club c;
                if (Vainqueur == Domicile)
                    c = Exterieur;
                else
                    c = Domicile;
                return c;
            }
        }
        
        public int CartonsJaunes
        {
            get
            {
                int res = 0;
                foreach(EvenementMatch em in _evenements)
                {
                    if (em.Type == Evenement.CARTON_JAUNE)
                        res++;
                }
                return res;
            }
        }

        private float NiveauCompo(List<Joueur> compo)
        {
            float res = 0;
            foreach(Joueur j in compo)
            {
                res += j.Niveau;
            }
            return res / (compo.Count+0.0f);
        }

        private Joueur Carton(List<Joueur> compo)
        {
            List<Joueur> joueurs = new List<Joueur>();
            foreach (Joueur j in compo)
            {
                switch (j.Poste)
                {
                    case Poste.GARDIEN:
                        for (int i = 0; i < j.Niveau; i++) joueurs.Add(j);
                        break;
                    case Poste.DEFENSEUR:
                        for (int i = 0; i < j.Niveau * 2; i++) joueurs.Add(j);
                        break;
                    case Poste.MILIEU:
                        for (int i = 0; i < j.Niveau; i++) joueurs.Add(j);
                        break;
                    case Poste.ATTAQUANT:
                        int k = j.Niveau / 2;
                        for (int i = 0; i < k; i++) joueurs.Add(j);
                        break;
                }
            }
            return joueurs[Session.Instance.Random(0, joueurs.Count)];
        }

        private Joueur Buteur(List<Joueur> compo)
        {
            List<Joueur> joueurs = new List<Joueur>();
            foreach(Joueur j in compo)
            {
                switch (j.Poste)
                {
                    case Poste.DEFENSEUR:
                        int k = j.Niveau / 2;
                        for (int i = 0; i < k; i++) joueurs.Add(j);
                        break;
                    case Poste.MILIEU:
                        for (int i = 0; i < j.Niveau; i++) joueurs.Add(j);
                        break;
                    case Poste.ATTAQUANT:
                        for (int i = 0; i < j.Niveau*2; i++) joueurs.Add(j);
                        break;
                }
            }
            return joueurs[Session.Instance.Random(0, joueurs.Count)];

        }

        public Match(Club domicile, Club exterieur, DateTime jour)
        {
            Domicile = domicile;
            Exterieur = exterieur;
            Jour = jour;
            _score1 = 0;
            _score2 = 0;
            _evenements = new List<EvenementMatch>();
            _compo1 = new List<Joueur>();
            _compo2 = new List<Joueur>();
        }

        private void DefinirCompo()
        {
            _compo1 = new List<Joueur>(Domicile.Composition());
            _compo2 = new List<Joueur>(Exterieur.Composition());
        }

        public void Jouer()
        {
            DefinirCompo();
            Club a;
            Club b;
            float diffF = Math.Abs(NiveauCompo(Compo1) - NiveauCompo(Compo2));
            int diff = (int)diffF;
            if(NiveauCompo(Compo1) > NiveauCompo(Compo2))
            {
                a = Domicile;
                b = Exterieur;
            }
            else
            {
                a = Exterieur;
                b = Domicile;
            }
            for (int i = 0; i < 20; i++)
            {
                if (diff < 1) IterationMatch(a, b, 1, 5, 6, 10);
                if (diff >= 1 && diff <= 2) IterationMatch(a, b, 1, 6, 8, 12);
                if (diff >= 3 && diff <= 4) IterationMatch(a, b, 1, 7, 8, 12);
                if (diff >= 5 && diff <= 7) IterationMatch(a, b, 1, 8, 10, 13);
                if (diff >= 8 && diff <= 10) IterationMatch(a, b, 1, 9, 12, 15);
                if (diff >= 11 && diff <= 14) IterationMatch(a, b, 1, 10, 15, 18);
                if (diff >= 15 && diff <= 18) IterationMatch(a, b, 1, 11, 17, 20);
                if (diff >= 19 && diff <= 23) IterationMatch(a, b, 1, 13, 20, 22);
                if (diff >= 24 && diff <= 28) IterationMatch(a, b, 1, 14, 23, 25);
                if (diff >= 29 && diff <= 33) IterationMatch(a, b, 1, 16, 23, 25);
                if (diff >= 34 && diff <= 40) IterationMatch(a, b, 1, 18, 23, 24);
                if (diff >= 40 && diff <= 47) IterationMatch(a, b, 1, 20, 23, 24);
                if (diff >= 48 && diff <= 55) IterationMatch(a, b, 1, 22, 23, 24);
                if (diff >= 55 && diff <= 63) IterationMatch(a, b, 1, 24, 25, 26);
                if (diff >= 64 && diff <= 70) IterationMatch(a, b, 1, 26, 40, 40);
                if (diff >= 71 && diff <= 79) IterationMatch(a, b, 1, 36, 40, 40);
                if (diff >= 80 && diff <= 89) IterationMatch(a, b, 1, 39, 40, 40);
                if (diff >= 90 && diff <= 100) IterationMatch(a, b, 1, 43, 44, 44);
            }
            string afficher = Jour.ToString() + " : " + Domicile.Nom + " - " + Exterieur.Nom;
            int ecart = 70 - afficher.Length;
            for (int i = 0; i < ecart; i++) afficher += " ";
            afficher += _score1 + "-" + _score2;
            Console.WriteLine(afficher);
            /*List<EvenementMatch> evenements = new List<EvenementMatch>(_evenements);
            evenements.Sort(new EvenementMatch_Temps_Comparator());
            Console.WriteLine("");
            foreach (EvenementMatch ev in evenements)
            {
                if(ev.Type == Evenement.BUT || ev.Type == Evenement.BUT_PENALTY || ev.Type == Evenement.BUT_CSC)
                Console.WriteLine(ev.MinuteEv + "min :  " + ev.Type + "(" + ev.Joueur.Nom + ") pour " + ev.Club.Nom);
            }
            Console.WriteLine("");*/
        }

        private void IterationMatch(Club a, Club b, int min_a, int max_a, int min_b,int max_b)
        {
            int hasard = Session.Instance.Random(0, 100);
            if (hasard >= min_a && hasard <= max_a)
            {
                if (a == Domicile) _score1++;
                else _score2++;
                But(a);
            }
            else if (hasard >= min_b && hasard <= max_b)
            {
                if (a == Domicile) _score2++;
                else _score1++;
                But(b);
            }
            else if (hasard >= 71 && hasard <= 75) CartonJaune(a);
            else if (hasard >= 76 && hasard <= 80) CartonJaune(b);
            else if (hasard == 81) CartonRouge(a);
            else if (hasard == 82) CartonRouge(b);
        }

        private void But(Club c)
        {
            Joueur j = Buteur(c == Domicile ? Compo1 : Compo2);
            int minute = Session.Instance.Random(1, 50);
            int miTemps = Session.Instance.Random(1, 3);
            EvenementMatch em = new EvenementMatch(Evenement.BUT, c, j, minute, miTemps);
            _evenements.Add(em);
        }

        private void CartonJaune(Club c)
        {
            Joueur j = Carton(c == Domicile ? Compo1 : Compo2);
            int minute = Session.Instance.Random(1, 50);
            int miTemps = Session.Instance.Random(1, 3);
            EvenementMatch em = new EvenementMatch(Evenement.CARTON_JAUNE, c, j, minute, miTemps);
            _evenements.Add(em);
        }

        private void CartonRouge(Club c)
        {
            Joueur j = Carton(c == Domicile ? Compo1 : Compo2);
            int minute = Session.Instance.Random(1, 50);
            int miTemps = Session.Instance.Random(1, 3);
            EvenementMatch em = new EvenementMatch(Evenement.CARTON_ROUGE, c, j, minute, miTemps);
            _evenements.Add(em);
        }
    }
}