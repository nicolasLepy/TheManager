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
        public List<Competition> Competitions { get => _competitions; }
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

        public Competition String2Competition(string nom)
        {
            Competition res = null;
            foreach(Competition competition in _competitions)
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

    }
}
