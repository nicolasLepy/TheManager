using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Stade
    {

        private string _nom;
        private Ville _ville;

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