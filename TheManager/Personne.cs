using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(Joueur))]
    [System.Xml.Serialization.XmlInclude(typeof(Joueur))]
    [KnownType(typeof(Manager))]
    [System.Xml.Serialization.XmlInclude(typeof(Manager))]
    public class Personne
    {
        [DataMember]
        private string _nom;
        [DataMember]
        private string _prenom;
        [DataMember]
        private DateTime _naissance;
        [DataMember]
        private Pays _nationalite;


        public string Nom { get => _nom; }
        public string Prenom { get => _prenom; }
        public DateTime Naissance { get => _naissance; }
        public Pays Nationalite { get => _nationalite; }

        public int Age
        {
            get
            {
                DateTime date = Session.Instance.Partie.Date;
                int age = date.Year - Naissance.Year;
                if (date.Month < Naissance.Month)
                {
                    age--;
                }
                else if (date.Month == Naissance.Month && date.Day < Naissance.Day)
                {
                    age--;
                }
                return age;
            }
        }

        public Personne(string prenom, string nom, DateTime naissance, Pays nationalite)
        {
            _prenom = prenom;
            _nom = nom;
            _naissance = naissance;
            _nationalite = nationalite;
        }

        public override string ToString()
        {
            return _prenom + " " + _nom;
        }
    }
}