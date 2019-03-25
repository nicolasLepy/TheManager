using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Club_Ville : Club
    {

        private float _budget;
        private Ville _ville;
        private float _sponsor;
        private List<Contrat> _joueurs;

        public float Budget { get => _budget; }
        public Ville Ville { get => _ville; }
        public float Sponsor { get => _sponsor; }
        public List<Contrat> Contrats { get => _joueurs; }

        public Club_Ville(string nom, string nomCourt, int reputation, float budget, float supporters, int centreFormation, Ville ville, float sponsor, string logo, Stade stade) : base(nom,nomCourt,reputation,supporters,centreFormation,logo,stade)
        {
            _budget = budget;
            _ville = ville;
            _sponsor = sponsor;
            _joueurs = new List<Contrat>();
        }

        public override List<Joueur> Joueurs()
        {
            List<Joueur> joueurs = new List<Joueur>();
            foreach (Contrat c in _joueurs)
                joueurs.Add(c.Joueur);
            return joueurs;
        }

        public override float Niveau()
        {
            float niveau = 0;
            foreach(Contrat c in _joueurs)
            {
                niveau += c.Joueur.Niveau;
            }
            return niveau / (_joueurs.Count+0.0f);
        }
    }
}