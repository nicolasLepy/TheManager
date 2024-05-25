using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace tm
{

    public enum GameEvent
    {
        Goal,
        PenaltyGoal,
        AgGoal,
        YellowCard,
        RedCard,
        Shot
    }

    [DataContract(IsReference =true)]
    public class MatchEvent
    {
        [DataMember]
        [Key]
        public int Id { get; set; }
        [DataMember]
        private GameEvent _type;
        [DataMember]
        private int _minute;
        [DataMember]
        private int _period;
        [DataMember]
        private Player _player;
        [DataMember]
        private Club _club;

        public GameEvent type => _type;
        public int minute => _minute;
        public int EventMinute
        {
            get
            {
                int minutes = _minute;
                if (_period == 2)
                {
                    minutes = minutes + 45;
                }
                else if (_period == 3)
                {
                    minutes = minutes + 90;
                }
                else if (_period == 4)
                {
                    minutes = minutes + 105;
                }

                return minutes;
            }
        }

        public string MinuteToString
        {
            get
            {
                int tAdd = (_minute - 45 > 0) ? _minute - 45 : 0;
                int minutes = _minute;
                if (_period == 2)
                {
                    minutes += 45;
                }
                else if (_period == 3)
                {
                    minutes += 90;
                }
                else if (_period == 4)
                {
                    minutes += 105;
                }

                string res = (minutes - tAdd).ToString();
                if (tAdd > 0)
                {
                    res += "+" + tAdd;
                }
                res += "'";
                return res;
            }
        }
        public int period { get => _period; }
        public Player player { get => _player; }
        public Club club { get => _club; }

        public MatchEvent()
        {

        }

        public MatchEvent(GameEvent type, Club club, Player player, int minute, int period)
        {
            _type = type;
            _club = club;
            _player = player;
            _minute = minute;
            _period = period;
        }
    }
}