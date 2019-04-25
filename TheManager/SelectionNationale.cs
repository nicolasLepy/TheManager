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

        public SelectionNationale(string nom, string nomCourt,int reputation, int supporters, int centreFormation, string logo, Stade stade, float coefficient, Pays pays) : base(nom,nomCourt,reputation,supporters,centreFormation,logo,stade)
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
            List<Joueur> joueursPoste = Utils.JoueursPoste(joueurs,Poste.GARDIEN);
            joueursPoste.Sort(new Joueur_Niveau_Comparator());
            for(int i = 0; i<3; i++)
                if (joueursPoste.Count > i) _selectionnes.Add(joueursPoste[i]);

            joueursPoste = Utils.JoueursPoste(joueurs, Poste.DEFENSEUR);
            joueursPoste.Sort(new Joueur_Niveau_Comparator());
            for (int i = 0; i < 7; i++)
                if (joueursPoste.Count > i) _selectionnes.Add(joueursPoste[i]);

            joueursPoste = Utils.JoueursPoste(joueurs, Poste.MILIEU);
            joueursPoste.Sort(new Joueur_Niveau_Comparator());
            for (int i = 0; i < 7; i++)
                if (joueursPoste.Count > i) _selectionnes.Add(joueursPoste[i]);

            joueursPoste = Utils.JoueursPoste(joueurs, Poste.ATTAQUANT);
            joueursPoste.Sort(new Joueur_Niveau_Comparator());
            for (int i = 0; i < 6; i++)
                if (joueursPoste.Count > i) _selectionnes.Add(joueursPoste[i]);

        }
    }
}