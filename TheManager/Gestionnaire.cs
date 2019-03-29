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
        private List<Joueur> _joueursLibres;
        private List<Continent> _continents;
        private List<Langue> _langues;
        private List<Competition> _competitionsArchives;

        public List<Club> Clubs { get => _clubs; }
        public List<Competition> Competitions { get => _competitions; }
        public List<Joueur> JoueursLibres { get => _joueursLibres; }
        public List<Continent> Continents { get => _continents; }
        public List<Langue> Langues { get => _langues; }
        public List<Competition> CompetitionsArchives { get => _competitionsArchives; }

        public Gestionnaire()
        {
            _clubs = new List<Club>();
            _competitions = new List<Competition>();
            _joueursLibres = new List<Joueur>();
            _continents = new List<Continent>();
            _langues = new List<Langue>();
            _competitionsArchives = new List<Competition>();
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

        public Langue String2Langue(string nom)
        {
            Langue res = null;

            foreach (Langue l in _langues) if (l.Nom == nom) res = l;

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

    }
}
