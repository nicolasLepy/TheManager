using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    public class Gestionnaire
    {

        private List<Club> _clubs;
        private List<Competition> _competitions;
        private List<Joueur> _joueurs;
        private List<Continent> _continents;

        public List<Club> Clubs { get => _clubs; }
        public List<Competition> Competition { get => _competitions; }
        public List<Joueur> Joueurs { get => _joueurs; }
        public List<Continent> Continents { get => _continents; }

        public Gestionnaire()
        {
            _clubs = new List<Club>();
            _competitions = new List<Competition>();
            _joueurs = new List<Joueur>();
            _continents = new List<Continent>();
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

        public Pays String2Pays(string pays)
        {
            Pays res = null;

            foreach(Continent c in _continents)
            {
                foreach(Pays p in c.Pays)
                {
                    if (p.Nom == pays) res = p;
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

    }
}
