using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Contrat
    {
        private float _salaire;
        private DateTime _fin;

        public float Salaire { get { return _salaire; } }
        public DateTime Fin { get => _fin; }
        public bool Transferable { get; set; }

        public Contrat(float salaire, DateTime fin)
        {
            _salaire = salaire;
            _fin = fin;
            Transferable = false;
        }
    }
}