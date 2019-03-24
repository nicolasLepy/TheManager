using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Match
    {

        private int _score1;
        private int _score2;
        private List<EvenementMatch> _evenements;

        public DateTime Jour { get; set; }
        public Club Domicile { get; set; }
        public Club Exterieur { get; set; }
        public int Score1 { get => _score1; }
        public int Score2 { get => _score2; }
        public List<EvenementMatch> Evenements { get => _evenements; }

        public Match(Club domicile, Club exterieur, DateTime jour)
        {
            Domicile = domicile;
            Exterieur = exterieur;
            Jour = jour;
            _score1 = 0;
            _score2 = 0;
            _evenements = new List<EvenementMatch>();
        }
    }
}