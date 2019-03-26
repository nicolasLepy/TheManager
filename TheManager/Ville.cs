using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Ville
    {
        public string Nom { get; set; }
        public int Population { get; set; }

        public Ville(string nom, int population)
        {
            Nom = nom;
            Population = population;
        }

        public Pays Pays(Gestionnaire gestionnaire)
        {
            Pays res = null;
            foreach(Continent c in gestionnaire.Continents)
            {
                foreach(Pays p in c.Pays)
                {
                    foreach(Ville v in p.Villes)
                    {
                        if (v == this) res = p;
                    }
                }
            }
            return res;
        }
    }
}