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
        private List<Stade> _stades;
        private Langue _langue;

        public string Nom { get; set; }
        public List<Ville> Villes { get { return _villes; } }
        public List<Stade> Stades { get { return _stades; } }
        public Langue Langue { get => _langue; }

        public Pays(string nom, Langue langue)
        {
            Nom = nom;
            _langue = langue;
            _villes = new List<Ville>();
            _stades = new List<Stade>();
        }
    }
}
