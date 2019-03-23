using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    public class Pays
    {
        private List<Ville> _villes;

        public string Nom { get; set; }
        public List<Ville> Villes { get { return _villes; } }


        public Pays(string nom)
        {
            Nom = nom;
            _villes = new List<Ville>();
        }
    }
}
