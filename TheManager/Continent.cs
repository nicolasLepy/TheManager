using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Continent
    {
        private List<Pays> _pays;

        public string Nom { get; set; }
        public List<Pays> Pays { get { return _pays; } }
        
        public Continent(string nom)
        {
            Nom = nom;
            _pays = new List<Pays>();
        }
    }
}