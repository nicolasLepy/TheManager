using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Ville
    {
        [DataMember]
        public string Nom { get; set; }
        [DataMember]
        public int Population { get; set; }
        [DataMember]
        public Position Position { get; set; }

        public Ville(string nom, int population, float latitude, float longitude)
        {
            Nom = nom;
            Population = population;
            Position = new Position(latitude, longitude);
        }

        public Pays Pays()
        {
            Pays res = null;
            foreach(Continent c in Session.Instance.Partie.Gestionnaire.Continents)
            {
                foreach(Pays p in c.Pays)
                {
                    foreach(Ville v in p.Villes)
                    {
                        if (v == this) res = p;
                    }
                }
            }
            return res;
        }
    }
}