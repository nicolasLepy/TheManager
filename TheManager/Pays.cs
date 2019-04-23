using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    public class Pays : ILocalisation
    {
        private List<Ville> _villes;
        private List<Stade> _stades;
        private Langue _langue;
        private List<Competition> _competitions;
        private string _nom;

        public List<Ville> Villes { get { return _villes; } }
        public List<Stade> Stades { get { return _stades; } }
        public Langue Langue { get => _langue; }

        public string Drapeau
        {
            get
            {
                string drapeau = _nom;
                drapeau = drapeau.Replace(" ", "");
                drapeau = drapeau.ToLower();
                return drapeau;
            }
        }

        public Pays(string nom, Langue langue)
        {
            _nom = nom;
            _langue = langue;
            _villes = new List<Ville>();
            _stades = new List<Stade>();
            _competitions = new List<Competition>();
        }

        public List<Competition> Competitions()
        {
            return _competitions;
        }

        public string Nom()
        {
            return _nom;
        }

        public override string ToString()
        {
            return _nom;
        }

    }
}