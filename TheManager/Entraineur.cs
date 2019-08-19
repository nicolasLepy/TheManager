using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Entraineur : Personne
    {

        [DataMember]
        private int _niveau;

        /// <summary>
        /// Niveau de l'entraîneur.
        /// Correspond au niveau des équipes qu'il est capable d'entraîner
        /// </summary>
        public int Niveau { get => _niveau; }

        public Entraineur(string prenom, string nom, int niveau, DateTime naissance, Pays nationalite) : base(prenom,nom,naissance, nationalite)
        {
            _niveau = niveau;

        }

        /// <summary>
        /// Faire évoluer l'entraîneur
        /// </summary>
        public void Evoluer()
        {
            _niveau += Session.Instance.Random(0, 4);
        }

    }
}