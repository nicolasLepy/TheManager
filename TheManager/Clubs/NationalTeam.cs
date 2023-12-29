using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class NationalTeam : Club
    {
        [DataMember]
        private List<Player> _selected;
        [DataMember]
        private Country _country;
        [DataMember]
        private double _fifaPoints;
        [DataMember]
        private List<double> _archivalFifaPoints;

        public Country country { get => _country; }
        public double officialFifaPoints { get => _archivalFifaPoints[_archivalFifaPoints.Count-1]; }
        public List<double> archivalFifaPoints { get => _archivalFifaPoints; }

        public NationalTeam(int id, string name, Manager manager, string shortName,int reputation, int supporters, int formationFacilities, string logo, Stadium stadium, Country country, string goalSong, double fifaPoints) : base(id, name,manager, shortName,reputation,supporters,formationFacilities,logo,stadium, goalSong, ClubStatus.Professional)
        {
            _selected = new List<Player>();
            _country = country;
            _fifaPoints = fifaPoints;
            _archivalFifaPoints = new List<double>();
            _archivalFifaPoints.Add(_fifaPoints);
        }

        public override Country Country()
        {
            return country;
        }

        public override GeographicPosition Localisation()
        {
            //TOOD
            return null;
        }

        public override AdministrativeDivision AdministrativeDivision()
        {
            return null;
        }

        public int Ranking()
        {
            return Session.Instance.Game.kernel.FifaRanking().IndexOf(this) + 1;
        }

        public void UpdateFifaPoints()
        {
            _archivalFifaPoints.Add(_fifaPoints);
        }

        public void UpdateFifaPointsAfterGame(Match match)
        {
            NationalTeam adv = (match.home == this ? match.away : match.home) as NationalTeam;
            //Qualif
            double I = 5;
            if (match.Tournament.level == 1)
            {
                I = 25;
            }
            //Continental cup
            else if(match.Tournament.level == 2)
            {
                //From Quarter Finals to the end, game is more important
                if(match.Tournament.rounds.Count - match.Tournament.rounds.IndexOf(match.Round) <= 3)
                {
                    I = 40;
                }
                else
                {
                    I = 35;
                }
            }
            //World cup
            else if(match.Tournament.level == 3)
            {
                //From Quarter Finals to the end, game is more important
                if (match.Tournament.rounds.Count - match.Tournament.rounds.IndexOf(match.Round) <= 3)
                {
                    I = 60;
                }
                else
                {
                    I = 50;
                }
            }
            double Dv = officialFifaPoints - adv.officialFifaPoints;
            double Ra = 1/(Math.Pow(10, -Dv/600)+1);
            double R = 0;
            if (match.PenaltyShootout && match.Winner == this)
            {
                R = 0.75;
            }
            if (match.score1 == match.score2)
            {
                R = 0.5;
            }
            if( (match.score1 > match.score2 && match.home == this) || match.score1 < match.score2 && match.away == this)
            {
                R = 1;
            }

            _fifaPoints = _fifaPoints + (I * (R - Ra));
        }

        public override List<Player> Players()
        {
           return new List<Player>(_selected);
        }

        private void SelectPlayersByPosition(Position position, List<Player> players, int playersNumberToTake)
        {
            List<Player> playersByPosition = Utils.PlayersByPosition(players, position);
            playersByPosition.Sort(new PlayerComparator(true, PlayerAttribute.LEVEL));
            for (int i = 0; i < playersNumberToTake; i++)
            {
                if (playersByPosition.Count > i)
                {
                    _selected.Add(playersByPosition[i]);
                }
            }
        }

        public void JoinPlayers()
        {
            foreach(Player player in _selected)
            {
                player.inSelection = true;
            }
        }

        public void ReleasePlayers()
        {
            foreach(Player player in _selected)
            {
                player.inSelection = false;
            }
        }

        public void CallPlayers(List<Player> players)
        {
            _selected.Clear();
            SelectPlayersByPosition(Position.Goalkeeper, players, 3);
            SelectPlayersByPosition(Position.Defender, players, 7);
            SelectPlayersByPosition(Position.Midfielder, players, 7);
            SelectPlayersByPosition(Position.Striker, players, 6);
        }

        public override void ChangeStatus(ClubStatus newStatus)
        {
            this._status = newStatus;
        }

    }
}