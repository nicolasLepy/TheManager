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

        //Attributs propres à la gestion du match
        private int _minute;
        private int _miTemps;

        private int _score1;
        private int _score2;
        private List<EvenementMatch> _evenements;
        private List<Joueur> _compo1;
        private List<Joueur> _compo2;
        private bool _prolongations;
        private int _tab1;
        private int _tab2;
        private bool _prolongationSiNul;
        private Match _matchAller;

        public DateTime Jour { get; set; }
        public Club Domicile { get; set; }
        public Club Exterieur { get; set; }
        public int Score1 { get => _score1; }
        public int Score2 { get => _score2; }
        public List<EvenementMatch> Evenements { get => _evenements; }
        public List<Joueur> Compo1 { get => _compo1; }
        public List<Joueur> Compo2 { get => _compo2; }
        public bool Prolongations { get => _prolongations; }
        public int Tab1 { get => _tab1; }
        public int Tab2 { get => _tab2; }

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
                if (_matchAller == null)
                {
                    if (Score1 > Score2) c = Domicile;
                    else if (Score1 < Score2) c = Exterieur;
                    else if (Prolongations)
                    {
                        if (Tab1 > Tab2) c = Domicile;
                        else c = Exterieur;
                    }
                    if (c == null)
                    {
                        Console.WriteLine(_miTemps + " - " + Session.Instance.Partie.Date.ToShortDateString() + " " + Domicile.Nom + " - " + Exterieur.Nom + "  prolongations jouées + " + Prolongations + " prolongations si nul : " + _prolongationSiNul + " date du match " + Jour.ToShortDateString());
                        c = Domicile;
                    }
                }
                else
                {
                    int score1 = Score1 + _matchAller.Score2;
                    int score2 = Score2 + _matchAller.Score1;
                    if(score1 == score2)
                    {
                        score1 = Score1 + 2 * _matchAller.Score2;
                        score2 = 2 * Score2 + _matchAller.Score1;
                    }
                    if (score1 > score2) c = Domicile;
                    else if (score2 > score1) c = Exterieur;
                    else
                    {
                        if (Tab1 > Tab2) c = Domicile;
                        else c = Exterieur;
                    }

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

        public bool TAB
        {
            get
            {
                bool res = false;
                if (_tab1 != 0 || _tab2 != 0) res = true;
                return res;
            }
        }

        public Match(Club domicile, Club exterieur, DateTime jour, bool prolongationSiNul, Match matchAller = null)
        {
            Domicile = domicile;
            Exterieur = exterieur;
            Jour = jour;
            _score1 = 0;
            _score2 = 0;
            _tab2 = 0;
            _tab1 = 0;
            _prolongations = false;
            _evenements = new List<EvenementMatch>();
            _compo1 = new List<Joueur>();
            _compo2 = new List<Joueur>();
            _prolongationSiNul = prolongationSiNul;
            _matchAller = matchAller;
            _minute = 0;
            _miTemps = 1;
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

            for(_miTemps = 1; _miTemps < 3; _miTemps++)
            {
                for (_minute = 1; _minute < 50; _minute++)
                {
                    JouerMinute(a, b, diff);
                }
            }

            if((_prolongationSiNul && (_score1 == _score2)) || MatchRetourNul())
            {
                _prolongations = true;
                for(_miTemps = 3; _miTemps<5;_miTemps++)
                {
                    for(_minute = 1; _minute<16; _minute++)
                    {
                        JouerMinute(a, b, diff);
                    }
                }
                if((_prolongationSiNul && _score1 == _score2) || MatchRetourNul())
                {
                    JouerTAB();
                }
            }

            string afficher = Jour.ToString() + " : " + Domicile.Nom + " - " + Exterieur.Nom;
            int ecart = 70 - afficher.Length;
            for (int i = 0; i < ecart; i++) afficher += " ";
            afficher += _score1 + "-" + _score2;
            Console.WriteLine(afficher);
           
        }

        private bool MatchRetourNul()
        {
            bool res = false;
            if(_matchAller != null)
            {
                int score1 = Score1 + _matchAller.Score2;
                int score2 = Score2 + _matchAller.Score1;
                if (score1 == score2)
                {
                    score1 = Score1 + 2 * _matchAller.Score2;
                    score2 = 2 * Score2 + _matchAller.Score1;
                }
                if (score1 == score2) res = true;
            }
            return res;
        }

        private void JouerTAB()
        {
            _tab1 = 0;
            _tab2 = 0;

            for(int i = 0; i<5; i++)
            {
                if (Session.Instance.Random(1, 4) == 1) _tab1++;
                if (Session.Instance.Random(1, 4) == 1) _tab2++;
            }
            while(_tab1 == _tab2)
            {
                if (Session.Instance.Random(1, 4) == 1) _tab1++;
                if (Session.Instance.Random(1, 4) == 1) _tab2++;
            }
        }

        private void JouerMinute(Club a, Club b, int diff)
        {
            if (diff < 1) IterationMatch(a, b, 1, 6, 8, 13);
            if (diff >= 1 && diff <= 2) IterationMatch(a, b, 1, 7, 8, 13);
            if (diff >= 3 && diff <= 4) IterationMatch(a, b, 1, 8, 9, 14);
            if (diff >= 5 && diff <= 7) IterationMatch(a, b, 1, 9, 10, 14);
            if (diff >= 8 && diff <= 10) IterationMatch(a, b, 1, 10, 12, 16);
            if (diff >= 11 && diff <= 14) IterationMatch(a, b, 1, 11, 15, 19);
            if (diff >= 15 && diff <= 18) IterationMatch(a, b, 1, 12, 17, 21);
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

        private void IterationMatch(Club a, Club b, int min_a, int max_a, int min_b,int max_b)
        {
            /*min_a = (int)(min_a * 2.5f);
            max_a = (int)(max_a * 2.5f);
            min_b = (int)(min_b * 2.5f);
            max_b = (int)(max_b * 2.5f);*/
            int hasard = Session.Instance.Random(0, 500);
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
            else if (hasard >= 171 && hasard <= 176) CartonJaune(a);
            else if (hasard >= 186 && hasard <= 191) CartonJaune(b);
            else if (hasard == 181) CartonRouge(a);
            else if (hasard == 183) CartonRouge(b);
        }

        private void But(Club c)
        {
            Joueur j = Buteur(c == Domicile ? Compo1 : Compo2);
            //int minute = Session.Instance.Random(1, 50);
            //int miTemps = Session.Instance.Random(1, 3);
            EvenementMatch em = new EvenementMatch(Evenement.BUT, c, j, _minute, _miTemps);
            _evenements.Add(em);
        }

        private void CartonJaune(Club c)
        {
            Joueur j = Carton(c == Domicile ? Compo1 : Compo2);
            //int minute = Session.Instance.Random(1, 50);
            //int miTemps = Session.Instance.Random(1, 3);
            EvenementMatch em = new EvenementMatch(Evenement.CARTON_JAUNE, c, j, _minute, _miTemps);
            _evenements.Add(em);
        }

        private void CartonRouge(Club c)
        {
            Joueur j = Carton(c == Domicile ? Compo1 : Compo2);
            //int minute = Session.Instance.Random(1, 50);
            //int miTemps = Session.Instance.Random(1, 3);
            EvenementMatch em = new EvenementMatch(Evenement.CARTON_ROUGE, c, j, _minute, _miTemps);
            _evenements.Add(em);
        }
    }
}