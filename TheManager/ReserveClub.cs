using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class ReserveClub : Club
    {
        [DataMember]
        private List<Contract> _players;
        [DataMember]
        private CityClub _fannionClub;

        public List<Contract> Contracts { get => _players; }
        public CityClub FannionClub { get => _fannionClub; }

        public ReserveClub(CityClub fannionClub, string name, string shortName, Manager manager) : base(name,manager,shortName,fannionClub.reputation/2,fannionClub.supporters/30,0,fannionClub.logo,fannionClub.stadium,fannionClub.goalMusic)
        {
            _fannionClub = fannionClub;
            _players = new List<Contract>();
        }

        public override List<Player> Players()
        {
            List<Player> res = new List<Player>();
            foreach (Contract ct in _players)
            {
                res.Add(ct.player);
            }
            return res;
        }

    }
}
