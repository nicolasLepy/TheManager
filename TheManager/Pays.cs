using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Pays : ILocalisation
    {
        [DataMember]
        private List<Ville> _villes;
        [DataMember]
        private List<Stade> _stades;
        [DataMember]
        private Language _langue;
        [DataMember]
        private List<Tournament> _competitions;
        [DataMember]
        private string _nom;

        public List<Ville> Villes { get { return _villes; } }
        public List<Stade> Stades { get { return _stades; } }
        public Language Langue { get => _langue; }

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

        public Pays(string nom, Language langue)
        {
            _nom = nom;
            _langue = langue;
            _villes = new List<Ville>();
            _stades = new List<Stade>();
            _competitions = new List<Tournament>();
        }

        public List<Tournament> Tournaments()
        {
            return _competitions;
        }

        public string Name()
        {
            return _nom;
        }

        public override string ToString()
        {
            return _nom;
        }

    }
}