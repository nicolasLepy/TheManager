using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    public abstract class Club
    {
        private string _nom;
        private int _reputation;
        private float _supporters;
        private int _centreFormation;
        private Stade _stade;
        private string _logo;
        private string _nomCourt;

        public string Nom { get => _nom; }
        public int Reputation { get => _reputation; }
        public float Supporters { get => _supporters; }
        public int CentreFormation { get => _centreFormation; }
        public Stade Stade { get => _stade; }
        public string Logo { get => _logo; }
        public string NomCourt { get => _nomCourt; }

        public abstract List<Joueur> Joueurs();
        public abstract float Niveau();

        public Club(string nom, string nomCourt, int reputation, float supporters, int centreFormation, string logo, Stade stade)
        {
            _nom = nom;
            _nomCourt = nomCourt;
            _reputation = reputation;
            _supporters = supporters;
            _centreFormation = centreFormation;
            _logo = logo;
            _stade = stade;
        }

        public List<Joueur> ListerJoueurPoste(Poste poste)
        {
            List<Joueur> res = new List<Joueur>();
            foreach(Joueur j in Joueurs())
            {
                if(j.Poste == poste)
                {
                    res.Add(j);
                }
            }
            return res;
        }

        private List<Joueur> ListerJoueursPosteComposition(Poste poste)
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

            List<Joueur> joueursPoste = ListerJoueursPosteComposition(Poste.GARDIEN);
            joueursPoste.Sort(new Joueur_Composition_Comparator());
            if (joueursPoste.Count >= 1)
                res.Add(joueursPoste[0]);

            joueursPoste = ListerJoueursPosteComposition(Poste.DEFENSEUR);
            joueursPoste.Sort(new Joueur_Composition_Comparator());
            
            if (joueursPoste.Count >= 4)
            {
                res.Add(joueursPoste[0]);
                res.Add(joueursPoste[1]);
                res.Add(joueursPoste[2]);
                res.Add(joueursPoste[3]);
            }

            joueursPoste = ListerJoueursPosteComposition(Poste.MILIEU);
            joueursPoste.Sort(new Joueur_Composition_Comparator());
            if (joueursPoste.Count >= 4)
            {
                res.Add(joueursPoste[0]);
                res.Add(joueursPoste[1]);
                res.Add(joueursPoste[2]);
                res.Add(joueursPoste[3]);
            }

            joueursPoste = ListerJoueursPosteComposition(Poste.ATTAQUANT);
            joueursPoste.Sort(new Joueur_Composition_Comparator());
            if (joueursPoste.Count >= 2)
            {
                res.Add(joueursPoste[0]);
                res.Add(joueursPoste[1]);
            }

            return res;
        }

        //BUDGET TRANSFERT / SALAIRE
        //CHAMPIONNAT PARTICIPATION
    }
}