using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{


    public struct RetourMatch
    {
        public RetourMatchEvenement Evenement { get; set; }
        public object Acteur { get; set; }

        public RetourMatch(RetourMatchEvenement evenement, object acteur)
        {
            Evenement = evenement;
            Acteur = acteur;
        }
    }

    public enum RetourMatchEvenement
    {
        EVENEMENT,
        REMPLACEMENT,
        ACTION,
        FIN_MITEMPS,
        FIN_MATCH
    }

    [DataContract(IsReference =true)]
    public class Statistiques
    {
        [DataMember]
        public int Possession1 { get; set; }
        [DataMember]
        public int Possession2 { get; set; }

        public float PossessionDomicile
        {
            get
            {
                return Possession1 / (Possession1 + Possession2 + 0.0f);
            }
        }

        public float PossessionExterieur { get => 1 - PossessionDomicile; }

        [DataMember]
        public int TirsDomicile { get; set; }
        [DataMember]
        public int TirsExterieurs { get; set; }


        public Statistiques()
        {
            TirsDomicile = 0;
            TirsExterieurs = 0;
            Possession1 = 0;
            Possession2 = 0;
        }
    }

    [DataContract(IsReference =true)]
    public class Match
    {
        //Attributs propres à la gestion du match
        [DataMember]
        private int _minute;
        [DataMember]
        private int _miTemps;
        [DataMember]
        private int _tempsAdditionnel;
        [DataMember]
        private int _diffNiveau;
        [DataMember]
        private float _diffNiveauRatio;
        [DataMember]
        private List<Joueur> _compo1Terrain;
        [DataMember]
        private List<Joueur> _compo2Terrain;


        [DataMember]
        private int _score1;
        [DataMember]
        private int _score2;
        [DataMember]
        private List<EvenementMatch> _evenements;
        [DataMember]
        private List<KeyValuePair<string, string>> _actions;
        [DataMember]
        private Statistiques _statistiques;
        [DataMember]
        private List<Joueur> _compo1;
        [DataMember]
        private List<Joueur> _compo2;
        [DataMember]
        private bool _prolongations;
        [DataMember]
        private int _tab1;
        [DataMember]
        private int _tab2;
        [DataMember]
        private bool _prolongationSiNul;
        [DataMember]
        private Match _matchAller;
        [DataMember]
        private int _affluence;
        [DataMember]
        private List<Journaliste> _journalistes;

        public int Affluence { get => _affluence; }
        [DataMember]
        public DateTime Jour { get; set; }
        [DataMember]
        public Club Domicile { get; set; }
        [DataMember]
        public Club Exterieur { get; set; }
        public int Score1 { get => _score1; }
        public int Score2 { get => _score2; }
        public List<EvenementMatch> Evenements { get => _evenements; }
        public List<Joueur> Compo1 { get => _compo1; }
        public List<Joueur> Compo2 { get => _compo2; }
        public bool Prolongations { get => _prolongations; }
        public int Tab1 { get => _tab1; }
        public int Tab2 { get => _tab2; }
        public List<Journaliste> Journalistes { get => _journalistes; }
        /// <summary>
        /// Description des actions du match [minute , action]
        /// </summary>
        public List<KeyValuePair<string,string>> Actions { get => _actions; }
        public Statistiques Statistiques { get => _statistiques; }
        [DataMember]
        public float Cote1 { get; set; }
        [DataMember]
        public float CoteN { get; set; }
        [DataMember]
        public float Cote2 { get; set; }

        public Competition Competition
        {
            get
            {
                Competition res = null;
                foreach (Competition c in Session.Instance.Partie.Gestionnaire.Competitions)
                {
                    foreach(Tour t in c.Tours)
                    {
                        foreach(Match m in t.Matchs)
                        {
                            if (m == this) res = c;
                        }
                    }
                }
                return res;
            }
        }

        public Tour Tour
        {
            get
            {
                Tour res = null;
                foreach (Competition c in Session.Instance.Partie.Gestionnaire.Competitions)
                {
                    foreach (Tour t in c.Tours)
                    {
                        foreach (Match m in t.Matchs)
                        {
                            if (m == this) res = t;
                        }
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// Si un match a été joué ou non
        /// </summary>
        public bool Joue
        {
            get
            {
                bool res = false;
                //Si la date de la partie est supérieure ou égale au jour du match, alors le match a été joué
                if (_minute > 0 || _miTemps > 1) res = true;
                /*if(DateTime.Compare(Session.Instance.Partie.Date,Jour) >= 0)
                {
                    res = true;
                }*/
                return res;
            }
        }

        public string Temps
        {
            get
            {
                string temps = "";

                int tmp = _minute;
                switch (_miTemps)
                {
                    case 2: tmp += 45;break;
                    case 3: tmp += 90;break;
                    case 4: tmp += 105;break;
                }
                int tmpAdd = _minute - ((_miTemps < 3) ? 45 : 15);
                temps = tmp.ToString();
                if (tmpAdd > 0) temps += "+" + tmpAdd;
                temps += "°";
                return temps;
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
            return res / (11.0f);
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
            Joueur res = null;
            if(joueurs.Count > 0)
            {
                res = joueurs[Session.Instance.Random(0, joueurs.Count)];
            }
            return res;
            
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
            Joueur res = null;
            if(joueurs.Count > 0)
                res = joueurs[Session.Instance.Random(0, joueurs.Count)];
            return res;

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

        public float PossessionDomicile
        {
            get { return _statistiques.PossessionDomicile; }
        }

        public float PossessionExterieur
        {
            get { return _statistiques.PossessionExterieur; }
        }

        public int ScoreMT1
        {
            get
            {
                int res = 0;
                foreach(EvenementMatch em in _evenements)
                {
                    if (em.Club == Domicile && em.MiTemps == 1 && (em.Type == Evenement.BUT || em.Type == Evenement.BUT_CSC || em.Type == Evenement.BUT_PENALTY))
                        res++;
                }
                return res;
            }
        }

        public int ScoreMT2
        {
            get
            {
                int res = 0;
                foreach (EvenementMatch em in _evenements)
                {
                    if (em.Club == Exterieur && em.MiTemps == 1 && (em.Type == Evenement.BUT || em.Type == Evenement.BUT_CSC || em.Type == Evenement.BUT_PENALTY))
                        res++;
                }
                return res;
            }
        }

        private void EtablirCotes()
        {
            float domN = Domicile.Niveau() * 1.1f;
            float extN = Exterieur.Niveau();
            
            float rapportD = domN / extN;
            rapportD *= rapportD * rapportD * rapportD;

            float rapportE = extN / domN;
            rapportE *= rapportE * rapportE * rapportE;

            
            Cote1 = (float)(1.01f + (1 / Math.Exp((2.5f * rapportD - 3f))));
            Cote2 = (float)(1.01f + (1 / Math.Exp((2.5f * rapportE - 3f))));
            

            /*Cote1 = (float)(-1.64266 * Math.Pow(rapportD, 6) + 24.1675 * Math.Pow(rapportD, 5) - 88.8353 * Math.Pow(rapportD, 4) + 117.695 * Math.Pow(rapportD, 3) - 29.6458 * Math.Pow(rapportD, 2) - 49.5219 * rapportD + 30.2832);
            Cote2 = (float)(-1.64266 * Math.Pow(rapportE, 6) + 24.1675 * Math.Pow(rapportE, 5) - 88.8353 * Math.Pow(rapportE, 4) + 117.695 * Math.Pow(rapportE, 3) - 29.6458 * Math.Pow(rapportE, 2) - 49.5219 * rapportE + 30.2832);
            */
            /*
            if(rapportD < 1)
            {
                Cote1 = 1 / rapportD;
                Cote2 = 1 / (1-rapportD);
            }
            else
            {
                Cote2 = 1 / rapportE;
                Cote1 = 1 / (1 - rapportE);
            }*/

            CoteN = (Cote1 + Cote2) / 2;

            /*
            if (domN > extN)
            {
                domN *= 2f;
            }
            else
            {
                extN *= 2f;
            }
            float ratioD = domN / extN;
            float ratioE = extN / domN;
            float ratio = ratioD / (ratioD + ratioE);
            //float ratio = Domicile.Niveau() / Exterieur.Niveau();
            Cote1 = 1 / ratio;// 1 / ((ratio * 50) / 100);
            //ratio = Exterieur.Niveau() / Domicile.Niveau();
            ratio = ratioE / (ratioD + ratioE);
            Cote2 = 1 / ratio;// ((ratio * 50) / 100);
            CoteN = (Cote1 + Cote2) / 2;*/
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
            _tempsAdditionnel = 0;
            _statistiques = new Statistiques();
            _prolongations = false;
            _evenements = new List<EvenementMatch>();
            _compo1 = new List<Joueur>();
            _compo2 = new List<Joueur>();
            _prolongationSiNul = prolongationSiNul;
            _matchAller = matchAller;
            _minute = 0;
            _miTemps = 1;
            _compo1Terrain = new List<Joueur>();
            _compo2Terrain = new List<Joueur>();
            _affluence = 0;
            _journalistes = new List<Journaliste>();
            _actions = new List<KeyValuePair<string, string>>();
            EtablirCotes();
        }

        private void EtablirAffluence()
        {
            _affluence = (int)(Domicile.Supporters * (Session.Instance.Random(6, 14) / 10.0f));
            _affluence = (int)(_affluence * (Exterieur.Niveau() / (Domicile.Niveau())));
            if (_affluence > Domicile.Stade.Capacite) _affluence = Domicile.Stade.Capacite;
            if(Domicile as Club_Ville != null)
            {
                (Domicile as Club_Ville).ModifierBudget(_affluence * Domicile.PrixBillet);
            }
        }

        public void DefinirCompo()
        {
            _compo1 = new List<Joueur>(Domicile.Composition());
            _compo2 = new List<Joueur>(Exterieur.Composition());
            _compo1Terrain = new List<Joueur>(_compo1);
            _compo2Terrain = new List<Joueur>(_compo2);
            
        }

        /// <summary>
        /// Définir une composition en la passant en paramètre
        /// </summary>
        /// <param name="compo">Les joueurs</param>
        /// <param name="club">Le club</param>
        public void DefinirCompo(List<Joueur> compo, Club club)
        {
            if(club == Domicile)
            {
                _compo1 = new List<Joueur>(compo);
                _compo1Terrain = new List<Joueur>(compo);
            }
            else if(club == Exterieur)
            {
                _compo2 = new List<Joueur>(compo);
                _compo2Terrain = new List<Joueur>(compo);
            }
        }

        public void CalculerDifferenceNiveau()
        {
            float diffF = Math.Abs(NiveauCompo(_compo1Terrain)*1.05f - NiveauCompo(_compo2Terrain));

            this._diffNiveau = (int)diffF;
            this._diffNiveauRatio = (NiveauCompo(_compo1Terrain) * 1.05f) / NiveauCompo(_compo2Terrain);
        }

        public List<RetourMatch> MinuteSuivante()
        {
            List<RetourMatch> retours = new List<RetourMatch>();
            //Au début du match
            if(_minute == 0 && _miTemps == 1)
            {
                CalculerDifferenceNiveau();
                EtablirAffluence();
            }

            Club a = Domicile;
            Club b = Exterieur;

            _minute++;
            retours = JouerMinute(a, b);

            int dureeMiTemps = (_miTemps < 3) ? 45 : 15;
            //Fin temps réglementaire
            if (_minute == dureeMiTemps) _tempsAdditionnel = Session.Instance.Random(1, 6);

            //Fin miTemps
            if (_minute == dureeMiTemps + _tempsAdditionnel)
            {
                _miTemps++;
                _minute = 0;
                _tempsAdditionnel = 0;
                if(_miTemps == 3)
                {
                    if ((_prolongationSiNul && (_score1 == _score2)) || MatchRetourNul())
                    {
                        _prolongations = true;
                    }
                    else
                    {
                        retours.Add(new RetourMatch(RetourMatchEvenement.FIN_MATCH, null));
                    }
                }
                if(_miTemps == 5)
                {
                    if ((_prolongationSiNul && _score1 == _score2) || MatchRetourNul())
                    {
                        JouerTAB();
                    }
                    retours.Add(new RetourMatch(RetourMatchEvenement.FIN_MATCH, null));

                }
            }

            
            return retours;

        }

        public void Jouer()
        {
            Club a = Domicile;
            Club b = Exterieur;
            CalculerDifferenceNiveau();
            EtablirAffluence();

            for (_miTemps = 1; _miTemps < 3; _miTemps++)
            {
                for (_minute = 1; _minute < 50; _minute++)
                {
                    JouerMinute(a, b);
                }
            }

            if((_prolongationSiNul && (_score1 == _score2)) || MatchRetourNul())
            {
                _prolongations = true;
                for(_miTemps = 3; _miTemps<5;_miTemps++)
                {
                    for(_minute = 1; _minute<16; _minute++)
                    {
                        JouerMinute(a, b);
                    }
                }
                if((_prolongationSiNul && _score1 == _score2) || MatchRetourNul())
                {
                    JouerTAB();
                }
            }
           
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
                if (Session.Instance.Random(1, 4) != 1) _tab1++;
                if (Session.Instance.Random(1, 4) != 1) _tab2++;
            }
            while(_tab1 == _tab2)
            {
                if (Session.Instance.Random(1, 4) != 1) _tab1++;
                if (Session.Instance.Random(1, 4) != 1) _tab2++;
            }
        }

        private List<RetourMatch> JouerMinute(Club a, Club b)
        {
            List<RetourMatch> retours = new List<RetourMatch>();
            foreach (Joueur j in _compo1)
            {
                if (Session.Instance.Random(2, 7) == 3) j.Energie--;
                j.MatchsJoues++;
            }
            foreach (Joueur j in _compo2)
            {
                if (Session.Instance.Random(2, 7) == 3) j.Energie--;
                j.MatchsJoues++;
            }

            int diff = _diffNiveau;
            float diffRatio = _diffNiveauRatio;

            if(diffRatio > 1)
            {
                diffRatio = 1 / diffRatio;
                Club temp = a;
                a = b;
                b = temp;
            }

            if (diffRatio < 0.05) retours = IterationMatch(a, b, 22, 22, 208, 295);
            else if (diffRatio < 0.1) retours = IterationMatch(a, b, 22, 22, 208, 280);
            else if (diffRatio >= 0.1 && diffRatio < 0.2) retours = IterationMatch(a, b, 22, 22, 208, 256);
            else if (diffRatio >= 0.2 && diffRatio < 0.3) retours = IterationMatch(a, b, 22, 22, 208, 246);
            else if (diffRatio >= 0.3 && diffRatio < 0.4) retours = IterationMatch(a, b, 22, 22, 208, 240);
            else if (diffRatio >= 0.4 && diffRatio < 0.5) retours = IterationMatch(a, b, 22, 23, 208, 235);
            else if (diffRatio >= 0.5 && diffRatio < 0.6) retours = IterationMatch(a, b, 22, 23, 208, 230);
            else if (diffRatio >= 0.6 && diffRatio < 0.65) retours = IterationMatch(a, b, 22, 23, 208, 220);
            else if (diffRatio >= 0.65 && diffRatio < 0.7) retours = IterationMatch(a, b, 21, 23, 208, 219);
            else if (diffRatio >= 0.7 && diffRatio < 0.74) retours = IterationMatch(a, b, 21, 23, 208, 218);
            else if (diffRatio >= 0.74 && diffRatio < 0.78) retours = IterationMatch(a, b, 21, 24, 208, 218);
            else if (diffRatio >= 0.78 && diffRatio < 0.81) retours = IterationMatch(a, b, 21, 24, 208, 217);
            else if (diffRatio >= 0.81 && diffRatio < 0.85) retours = IterationMatch(a, b, 21, 25, 208, 217);
            else if (diffRatio >= 0.85 && diffRatio < 0.89) retours = IterationMatch(a, b, 21, 25, 208, 216);
            else if (diffRatio >= 0.89 && diffRatio < 0.92) retours = IterationMatch(a, b, 21, 25, 208, 215);
            else if (diffRatio >= 0.92 && diffRatio < 0.95) retours = IterationMatch(a, b, 21, 25, 208, 214);
            else if (diffRatio >= 0.95 && diffRatio < 0.98) retours = IterationMatch(a, b, 21, 26, 208, 214);
            else if (diffRatio >= 0.98 && diffRatio < 1.01) retours = IterationMatch(a, b, 21, 26, 208, 213);
            /*if (diff < 1) IterationMatch(a, b, 1, 6, 8, 13);
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
            if (diff >= 90 && diff <= 100) IterationMatch(a, b, 1, 43, 44, 44);*/

            return retours;
        }

        private List<RetourMatch> IterationMatch(Club a, Club b, int min_a, int max_a, int min_b,int max_b)
        {

            List<RetourMatch> res = new List<RetourMatch>();

            int hasard = Session.Instance.Random(0, 500);

            //Tirs
            if (hasard >= min_a && hasard <= max_a + ((max_a + 1 - min_a) * 4))
            {
                if (a == Domicile)
                {
                    _statistiques.TirsDomicile++;
                    Tir(Domicile);
                    res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                }
                else
                {
                    _statistiques.TirsExterieurs++;
                    Tir(Exterieur);
                    res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                }
            }
            else if (hasard >= min_b && hasard <= max_b + ((max_b + 1 - min_b) * 4))
            {
                if (a == Domicile)
                {
                    _statistiques.TirsExterieurs++;
                    Tir(Exterieur);
                    res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                }
                else
                {
                    _statistiques.TirsDomicile++;
                    Tir(Domicile);
                    res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                }
            }

            //Buts
            if (hasard >= min_a && hasard <= max_a)
            {
                if (a == Domicile) _score1++;
                else _score2++;
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                But(a);
            }
            else if (hasard >= min_b && hasard <= max_b)
            {
                if (a == Domicile) _score2++;
                else _score1++;
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                But(b);
            }
            //Cartons jaunes
            else if (hasard >= 4 && hasard <= 9)
            {
                CartonJaune(a);
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
            }
            else if (hasard >= 14 && hasard <= 19)
            {
                CartonJaune(b);
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
            }
            //Cartons rouges
            else if (hasard == 2)
            {
                CartonRouge(a);
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
            }
            else if (hasard == 3)
            {
                CartonRouge(b);
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
            }
            

            return res;
        }

        private void But(Club c)
        {
            Joueur j = Buteur(c == Domicile ? Compo1 : Compo2);
            if(j != null)
            {
                EvenementMatch em = new EvenementMatch(Evenement.BUT, c, j, _minute, _miTemps);
                if (j != null) j.ButsMarques++;
                _evenements.Add(em);
                AjouterAction(em.MinuteStr, Session.Instance.Partie.Gestionnaire.Commentaire(em));
            }
        }

        private void CartonJaune(Club c)
        {
            Joueur j = Carton(c == Domicile ? Compo1 : Compo2);
            if(j != null)
            {

                bool deuxiemeJaune = false;
                foreach (EvenementMatch ev in _evenements) if (ev.Joueur == j && ev.Type == Evenement.CARTON_JAUNE) deuxiemeJaune = true;


                EvenementMatch em = new EvenementMatch(Evenement.CARTON_JAUNE, c, j, _minute, _miTemps);
                _evenements.Add(em);
                AjouterAction(em.MinuteStr, Session.Instance.Partie.Gestionnaire.Commentaire(em));

                //Si c'est son deuxième jaune, carte rouge attribué
                if (deuxiemeJaune == true)
                {
                    List<Joueur> compo = c == Domicile ? _compo1Terrain : _compo2Terrain;
                    compo.Remove(j);
                    CalculerDifferenceNiveau();
                    em = new EvenementMatch(Evenement.CARTON_ROUGE, c, j, _minute, _miTemps);
                    _evenements.Add(em);

                }
            }
        }

        private void CartonRouge(Club c)
        {
            List<Joueur> compo = c == Domicile ? _compo1Terrain : _compo2Terrain;
            Joueur j = Carton(compo);
            if(j != null)
            {
                compo.Remove(j);
                CalculerDifferenceNiveau();
                EvenementMatch em = new EvenementMatch(Evenement.CARTON_ROUGE, c, j, _minute, _miTemps);
                _evenements.Add(em);
                AjouterAction(em.MinuteStr, Session.Instance.Partie.Gestionnaire.Commentaire(em));
            }
        }

        private void Tir(Club c)
        {
            List<Joueur> compo = c == Domicile ? _compo1Terrain : _compo2Terrain;
            Joueur j = Buteur(compo);
            if(j != null)
            {
                EvenementMatch em = new EvenementMatch(Evenement.TIR, c, j, _minute, _miTemps);
                _evenements.Add(em);
                AjouterAction(em.MinuteStr, Session.Instance.Partie.Gestionnaire.Commentaire(em));
            }
        }

        /// <summary>
        /// Ajoute la description d'une action dans le match
        /// </summary>
        /// <param name="minute">Minute de l'action</param>
        /// <param name="action">Description de l'action</param>
        public void AjouterAction(string minute, string action)
        {
            _actions.Add(new KeyValuePair<string, string>(minute, action));
        }
    }
}