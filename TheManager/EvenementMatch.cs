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

        public Evenement Type { get => _type; }
        public int Minute { get => _minute; }
        public int MinuteEv
        {
            get
            {
                int minutes = _minute;
                if (_miTemps == 2) minutes = minutes + 45;
                return minutes;
            }
        }
        public int MiTemps { get => _miTemps; }
        public Joueur Joueur { get => _joueur; }
        public Club Club { get => _club; }

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