using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Journaliste
    {

        [DataMember]
        private string _prenom;
        [DataMember]
        private string _nom;
        [DataMember]
        private Ville _base;
        [DataMember]
        private int _retrait;

        public string Prenom { get => _prenom; }
        public string Nom { get => _nom; }
        [DataMember]
        public int Age { get; set; }
        [DataMember]
        public bool EstPris { get; set; }
        public Ville Base { get => _base; }
        public int Retrait { get => _retrait; }

        public Media Media
        {
            get
            {
                Media res = null;
                foreach(Media m in Session.Instance.Partie.kernel.medias)
                {
                    foreach (Journaliste j in m.journalists) if (j == this) res = m;
                }
                return res;
            }
        }

        /// <summary>
        /// Donne le nombre de matchs commentés par le journaliste sur l'ensemble des compétitions non archivées
        /// </summary>
        public int NombreMatchsCommentes
        {
            get
            {
                int res = 0;

                foreach(Match m in Session.Instance.Partie.kernel.Matchs)
                {
                    foreach(KeyValuePair<Media, Journaliste> j in m.Journalistes)
                    {
                        if (j.Value == this) res++;
                    }
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

        public override string ToString()
        {
            return Prenom + " " + Nom;
        }
    }
}