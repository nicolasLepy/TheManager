using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tm
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

        public ReserveClub() : base()
        {
            _players = new List<Contract>();
        }

        public ReserveClub(int id, CityClub fannionClub, string name, string shortName, Manager manager) : base(id, name,manager,shortName,fannionClub.elo/2.0f,fannionClub.supporters/30,0,fannionClub.logo,fannionClub.stadium,fannionClub.goalSong, fannionClub.status)
        {
            _fannionClub = fannionClub;
            _players = new List<Contract>();
        }

        public override Country Country()
        {
            return _fannionClub.Country();
        }

        public override GeographicPosition Localisation()
        {
            return _fannionClub.city.Position;
        }

        public override AdministrativeDivision AdministrativeDivision()
        {
            return _fannionClub.AdministrativeDivision();
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

        public override void ChangeStatus(ClubStatus newStatus)
        {
            this._status = newStatus;
        }

    }
}
