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
    public class SelectionNationale : Club
    {

        [DataMember]
        private float _coefficient;
        [DataMember]
        private List<Joueur> _selectionnes;
        [DataMember]
        private Pays _pays;

        public float Coefficient { get => _coefficient; }
        public Pays Pays { get => _pays; }

        public SelectionNationale(string nom, Entraineur entraineur, string nomCourt,int reputation, int supporters, int centreFormation, string logo, Stade stade, float coefficient, Pays pays, string musiqueBut) : base(nom,entraineur, nomCourt,reputation,supporters,centreFormation,logo,stade,musiqueBut)
        {
            _coefficient = coefficient;
            _selectionnes = new List<Joueur>();
            _pays = pays;
        }

        public override List<Joueur> Joueurs()
        {
           return new List<Joueur>(_selectionnes);
        }

        public override float Niveau()
        {
            float res = 0;
            foreach(Joueur j in _selectionnes)
            {
                res += j.Niveau;
            }
            return res / (_selectionnes.Count + 0.0f);
        }

        public void AppelSelection(List<Joueur> joueurs)
        {
            _selectionnes = new List<Joueur>();
            List<Joueur> joueursPoste = Utils.JoueursPoste(joueurs,Position.Goalkeeper);
            joueursPoste.Sort(new Joueur_Niveau_Comparator());
            for(int i = 0; i<3; i++)
                if (joueursPoste.Count > i) _selectionnes.Add(joueursPoste[i]);

            joueursPoste = Utils.JoueursPoste(joueurs, Position.Defender);
            joueursPoste.Sort(new Joueur_Niveau_Comparator());
            for (int i = 0; i < 7; i++)
                if (joueursPoste.Count > i) _selectionnes.Add(joueursPoste[i]);

            joueursPoste = Utils.JoueursPoste(joueurs, Position.Midfielder);
            joueursPoste.Sort(new Joueur_Niveau_Comparator());
            for (int i = 0; i < 7; i++)
                if (joueursPoste.Count > i) _selectionnes.Add(joueursPoste[i]);

            joueursPoste = Utils.JoueursPoste(joueurs, Position.Striker);
            joueursPoste.Sort(new Joueur_Niveau_Comparator());
            for (int i = 0; i < 6; i++)
                if (joueursPoste.Count > i) _selectionnes.Add(joueursPoste[i]);

        }
    }
}