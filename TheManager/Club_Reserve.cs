using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Club_Reserve : Club
    {
        [DataMember]
        private List<Contrat> _joueurs;
        [DataMember]
        private Club_Ville _equipePremiere;

        public List<Contrat> Contrats { get => _joueurs; }
        public Club_Ville EquipePremiere { get => _equipePremiere; }

        public Club_Reserve(Club_Ville equipePremiere, string nom, string nomCourt, Entraineur entraineur) : base(nom,entraineur,nomCourt,equipePremiere.reputation/2,equipePremiere.supporters/30,0,equipePremiere.logo,equipePremiere.stadium,equipePremiere.goalMusic)
        {
            _equipePremiere = equipePremiere;
            _joueurs = new List<Contrat>();
        }

        public override List<Joueur> Players()
        {
            List<Joueur> res = new List<Joueur>();
            foreach (Contrat ct in _joueurs) res.Add(ct.Joueur);
            return res;
        }

        public override float Level()
        {
            float res = 0;
            foreach (Contrat ct in _joueurs)
            {
                res += ct.Joueur.Niveau;
            }
            return res / (_joueurs.Count + 0.0f);
        }
    }
}
