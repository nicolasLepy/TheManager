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

        public override List<Player> Players()
        {
           return new List<Player>(_selected);
        }

        public override float Level()
        {
            float res = 0;
            foreach(Player j in _selected)
            {
                res += j.level;
            }
            return res / (_selected.Count + 0.0f);
        }

        public void CallInSelection(List<Player> players)
        {
            _selected = new List<Player>();
            List<Player> playersByPosition = Utils.PlayersByPosition(players,Position.Goalkeeper);
            playersByPosition.Sort(new PlayerLevelComparator());
            for(int i = 0; i<3; i++){
                if (playersByPosition.Count > i)
                {
                    _selected.Add(playersByPosition[i]);
                }
            }

            playersByPosition = Utils.PlayersByPosition(players, Position.Defender);
            playersByPosition.Sort(new PlayerLevelComparator());
            for (int i = 0; i < 7; i++){
                if (playersByPosition.Count > i)
                {
                    _selected.Add(playersByPosition[i]);
                }
            }

            playersByPosition = Utils.PlayersByPosition(players, Position.Midfielder);
            playersByPosition.Sort(new PlayerLevelComparator());
            for (int i = 0; i < 7; i++)
            {
                if (playersByPosition.Count > i)
                {
                    _selected.Add(playersByPosition[i]);
                }                
            }

            playersByPosition = Utils.PlayersByPosition(players, Position.Striker);
            playersByPosition.Sort(new PlayerLevelComparator());
            for (int i = 0; i < 6; i++){
                if (playersByPosition.Count > i)
                {
                    _selected.Add(playersByPosition[i]);
                }
            }
        }
    }
}