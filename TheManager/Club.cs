using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Club
    {
        private string _nom;
        private int _reputation;
        private float _supporters;
        private int _centreFormation;
        private Stade _stade;
        private string _logo;
        private string _nomCourt;

        
        private float _budget;
        private Ville _ville;
        private float _sponsor;
        private List<Contrat> _joueurs;

        public Club(string nom, string nomCourt, int repuation, float budget, float supporters, int centreFormation, Ville ville, float sponsor, string logo, Stade stade)
        {
            _nom = nom;
            _nomCourt = nomCourt;
            _reputation = repuation;
            _budget = budget;
            _supporters = supporters;
            _centreFormation = centreFormation;
            _ville = ville;
            _sponsor = sponsor;
            _logo = logo;
            _stade = stade;
            _joueurs = new List<Contrat>();
        }

        //BUDGET TRANSFERT / SALAIRE
        //CHAMPIONNAT PARTICIPATION
    }
}