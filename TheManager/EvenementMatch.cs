using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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

    [DataContract(IsReference =true)]
    public class EvenementMatch
    {

        [DataMember]
        private Evenement _type;
        [DataMember]
        private int _minute;
        [DataMember]
        private int _miTemps;
        [DataMember]
        private Joueur _joueur;
        [DataMember]
        private Club _club;

        public Evenement Type { get => _type; }
        public int Minute { get => _minute; }
        public int MinuteEv
        {
            get
            {
                int minutes = _minute;
                if (_miTemps == 2) minutes = minutes + 45;
                else if (_miTemps == 3) minutes = minutes + 90;
                else if (_miTemps == 4) minutes = minutes + 105;

                return minutes;
            }
        }

        public string MinuteStr
        {
            get
            {
                int tpAdd = (_minute - 45 > 0) ? _minute - 45 : 0;
                int minutes = _minute;
                if (_miTemps == 2) minutes = minutes + 45;
                else if (_miTemps == 3) minutes = minutes + 90;
                else if (_miTemps == 4) minutes = minutes + 105;

                string res = (minutes - tpAdd).ToString();
                if (tpAdd > 0)
                    res += "+" + tpAdd;
                res += "'";
                return res;
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