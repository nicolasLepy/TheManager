using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{

    public enum Evenement
    {
        BUT,
        BUT_PENALTY,
        BUT_CSC,
        CARTON_JAUNE,
        CARTON_ROUGE
    }

    public class EvenementMatch
    {

        private Evenement _type;
        private int _minute;
        private int _miTemps;
        private Joueur _joueur;
        private Club _club;

        public EvenementMatch(Evenement type, Club club, Joueur joueur, int minute, int mitemps)
        {
            _type = type;
            _club = club;
            _joueur = joueur;
            _minute = minute;
            _miTemps = mitemps;
        }
    }
}