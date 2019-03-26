using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public enum Poste
    {
        GARDIEN,
        DEFENSEUR,
        MILIEU,
        ATTAQUANT
    }

    public class Joueur
    {
        private string _nom;
        private string _prenom;
        private DateTime _naissance;
        private int _niveau;
        private int _potentiel;
        private Pays _nationalite;
        private Poste _poste;
        private bool _suspendu;
        private int _energie;

        public string Nom { get => _nom; }
        public string Prenom { get => _prenom;  }
        public DateTime Naissance { get => _naissance; }
        public int Niveau { get => _niveau; set => _niveau = value; }
        public int Potentiel { get => _potentiel; }
        public Pays Nationalite { get => _nationalite;}
        public Poste Poste { get => _poste;}
        public bool Suspendu { get => _suspendu; set => _suspendu = value; }
        public int Energie { get => _energie; }

        public Joueur(string prenom, string nom, DateTime naissance, int niveau, int potentiel, Pays nationalite, Poste poste)
        {
            _prenom = prenom;
            _nom = nom;
            _naissance = naissance;
            _niveau = niveau;
            _potentiel = potentiel;
            _poste = poste;
            _nationalite = nationalite;
            Suspendu = false;
            _energie = 100;
        }

        //Propriétés
        //nb de jaunes cette saison
        //nb de rouges cette saison
        //nb de buts cette saison
        //appelé en sélection
    }
}