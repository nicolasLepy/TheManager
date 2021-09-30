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
        private float _coefficient;
        [DataMember]
        private List<Player> _selected;
        [DataMember]
        private Country _country;

        public float coefficient { get => _coefficient; }
        public Country country { get => _country; }

        public NationalTeam(string name, Manager manager, string shortName,int reputation, int supporters, int formationFacilities, string logo, Stadium stadium, float coefficient, Country country, string goalMusic) : base(name,manager, shortName,reputation,supporters,formationFacilities,logo,stadium,goalMusic)
        {
            _coefficient = coefficient;
            _selected = new List<Player>();
            _country = country;
        }

        public override Country Country()
        {
            return country;
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

        public void CallInSelection(List<Player> players)
        {
            _selected = new List<Player>();
            SelectPlayersByPosition(Position.Goalkeeper, players, 3);
            SelectPlayersByPosition(Position.Defender, players, 7);
            SelectPlayersByPosition(Position.Midfielder, players, 7);
            SelectPlayersByPosition(Position.Striker, players, 6);
        }
    }
}