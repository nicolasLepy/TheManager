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
        private List<Contrat> _players;
        [DataMember]
        private CityClub _fannionClub;

        public List<Contrat> Contracts { get => _players; }
        public CityClub FannionClub { get => _fannionClub; }

        public ReserveClub(CityClub fannionClub, string name, string shortName, Entraineur manager) : base(name,manager,shortName,fannionClub.reputation/2,fannionClub.supporters/30,0,fannionClub.logo,fannionClub.stadium,fannionClub.goalMusic)
        {
            _fannionClub = fannionClub;
            _players = new List<Contrat>();
        }

        public override List<Joueur> Players()
        {
            List<Joueur> res = new List<Joueur>();
            foreach (Contrat ct in _players) res.Add(ct.Joueur);
            return res;
        }

        public override float Level()
        {
            float res = 0;
            foreach (Contrat ct in _players)
            {
                res += ct.Joueur.Niveau;
            }
            return res / (_players.Count + 0.0f);
        }
    }
}
