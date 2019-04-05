using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Journaliste
    {

        private string _prenom;
        private string _nom;
        private Ville _base;
        private int _retrait;

        public string Prenom { get => _prenom; }
        public string Nom { get => _nom; }
        public int Age { get; set; }
        public bool EstPris { get; set; }
        public Ville Base { get => _base; }
        public int Retrait { get => _retrait; }

        public Media Media
        {
            get
            {
                Media res = null;
                foreach(Media m in Session.Instance.Partie.Gestionnaire.Medias)
                {
                    foreach (Journaliste j in m.Journalistes) if (j == this) res = m;
                }
                return res;
            }
        }

        public Journaliste(string prenom, string nom, int age, Ville _base, int retrait)
        {
            EstPris = false;
            _prenom = prenom;
            _nom = nom;
            Age = age;
            this._base = _base;
            _retrait = retrait;
        }
    }
}