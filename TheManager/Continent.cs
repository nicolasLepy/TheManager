using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Continent : IEquipesRecuperables, ILocalisation
    {
        private List<Pays> _pays;
        private List<Competition> _competitions;
        private string _nom;

        public List<Pays> Pays { get { return _pays; } }
        public List<Competition> Competitions()
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
            _competitions = new List<Competition>();
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

        public override string ToString()
        {
            return _nom;
        }
    }
}