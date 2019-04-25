using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Stade
    {

        [DataMember]
        private string _nom;
        [DataMember]
        private Ville _ville;

        [DataMember]
        public int Capacite { get; set; }
        public string Nom { get => _nom; }
        public Ville Ville { get => _ville; }

        public Stade(string nom, int capacite, Ville ville)
        {
            _nom = nom;
            Capacite = capacite;
            _ville = ville;
        }
    }
}