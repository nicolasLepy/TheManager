using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public abstract class Club
    {
        private string _nom;
        private int _reputation;
        private float _supporters;
        private int _centreFormation;
        private Stade _stade;
        private string _logo;
        private string _nomCourt;

        public abstract List<Joueur> Joueurs();
        public abstract float Niveau();

        public Club(string nom, string nomCourt, int reputation, float supporters, int centreFormation, string logo, Stade stade)
        {
            _nom = nom;
            _nomCourt = nomCourt;
            _reputation = reputation;
            _supporters = supporters;
            _centreFormation = centreFormation;
            _logo = logo;
            _stade = stade;
        }

        //BUDGET TRANSFERT / SALAIRE
        //CHAMPIONNAT PARTICIPATION
    }
}