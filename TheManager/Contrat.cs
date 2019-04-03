using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Contrat
    {
        private int _salaire;
        private DateTime _fin;
        private Joueur _joueur;

        public int Salaire { get { return _salaire; } }
        public DateTime Fin { get => _fin; }
        public bool Transferable { get; set; }
        public Joueur Joueur { get => _joueur; }

        public Contrat(Joueur joueur, int salaire, DateTime fin)
        {
            _joueur = joueur;
            _salaire = salaire;
            _fin = fin;
            Transferable = false;
        }

        public void MettreAJour(int salaire, DateTime fin)
        {
            _salaire = salaire;
            _fin = fin;
        }
    }
}