using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Continent : IEquipesRecuperables, ILocalisation
    {
        [DataMember]
        private List<Pays> _pays;
        [DataMember]
        private List<Tournament> _competitions;
        [DataMember]
        private string _nom;

        public List<Pays> Pays { get { return _pays; } }
        public List<Tournament> Competitions()
        {
            return _competitions;
        }

        public string Nom()
        {
            return _nom;
        }
        

        public Continent(string nom)
        {
            _nom = nom;
            _pays = new List<Pays>();
            _competitions = new List<Tournament>();
        }

        /// <summary>
        /// Le paramètre "equipesPremieresUniquement" est ignoré vu que les sélections nationales sont toutes des équipes premières
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="methode"></param>
        /// <param name="equipesPremieresUniquement"></param>
        /// <returns></returns>
        public List<Club> RecupererEquipes(int nombre, MethodeRecuperation methode, bool equipesPremieresUniquement)
        {
            List<Club> clubs = new List<Club>();
            foreach(Club c in Session.Instance.Partie.Gestionnaire.Clubs)
            {
                SelectionNationale sn = c as SelectionNationale;
                if(sn != null)
                {
                    if (_pays.Contains(sn.Pays)) clubs.Add(sn);
                }
            }
            List<Club> res = new List<Club>();
            if (methode == MethodeRecuperation.MEILLEURS)
                clubs.Sort(new Club_Niveau_Comparator());
            else if (methode == MethodeRecuperation.PIRES)
                clubs.Sort(new Club_Niveau_Comparator(true));
            else if (methode == MethodeRecuperation.ALEATOIRE)
                clubs = Utils.ShuffleList<Club>(clubs);

            for (int i = 0; i < nombre; i++) res.Add(clubs[i]);
            return res;
        }

        public override string ToString()
        {
            return _nom;
        }
    }
}