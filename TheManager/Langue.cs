using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Langue
    {
        [DataMember]
        private string _nom;
        [DataMember]
        private List<string> _prenoms;
        [DataMember]
        private List<string> _noms;

        public string Nom { get => _nom; }

        public Langue(string nom)
        {
            _nom = nom;
            _prenoms = new List<string>();
            _noms = new List<string>();
        }

        public void AjouterPrenom(string prenom)
        {
            _prenoms.Add(prenom);
        }

        public void AjouterNom(string nom)
        {
            _noms.Add(nom);
        }
        
        public string ObtenirPrenom()
        {
            return _prenoms[Session.Instance.Random(0,_prenoms.Count)];
        }

        public string ObtenirNom()
        {
            return _noms[Session.Instance.Random(0, _noms.Count)];
        }

    }
}