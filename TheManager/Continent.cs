using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Continent : IEquipesRecuperables
    {
        private List<Pays> _pays;

        public string Nom { get; set; }
        public List<Pays> Pays { get { return _pays; } }
        
        public Continent(string nom)
        {
            Nom = nom;
            _pays = new List<Pays>();
        }

        public List<Club> RecupererEquipes(int nombre, MethodeRecuperation methode)
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
            for (int i = 0; i < nombre; i++) res.Add(clubs[i]);
            return res;
        }
    }
}