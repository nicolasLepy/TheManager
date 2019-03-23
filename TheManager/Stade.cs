using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Stade
    {
        public string Nom { get; set; }
        public int Capacite { get; set; }

        public Stade(string nom, int capacite)
        {
            Nom = nom;
            Capacite = capacite;
        }
    }
}