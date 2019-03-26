using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class SelectionNationale : Club
    {

        private float _coefficient;
        private List<Joueur> _selectionnes;

        public float Coefficient { get => _coefficient; }

        public SelectionNationale(string nom, string nomCourt,int reputation, int supporters, int centreFormation, string logo, Stade stade, float coefficient) : base(nom,nomCourt,reputation,supporters,centreFormation,logo,stade)
        {
            _coefficient = coefficient;
            _selectionnes = new List<Joueur>();
        }

        public override List<Joueur> Joueurs()
        {
           return new List<Joueur>(_selectionnes);
        }

        public override float Niveau()
        {
            float res = 0;
            foreach(Joueur j in _selectionnes)
            {
                res += j.Niveau;
            }
            return res / (_selectionnes.Count + 0.0f);
        }
    }
}