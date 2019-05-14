using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Contrat
    {
        [DataMember]
        private DateTime _debut;
        [DataMember]
        private int _salaire;
        [DataMember]
        private DateTime _fin;
        [DataMember]
        private Joueur _joueur;

        public int Salaire { get { return _salaire; } }
        public DateTime Fin { get => _fin; }
        [DataMember]
        public bool Transferable { get; set; }
        public Joueur Joueur { get => _joueur; }
        public DateTime Debut { get => _debut; }

        public Contrat(Joueur joueur, int salaire, DateTime fin, DateTime debut)
        {
            _joueur = joueur;
            _salaire = salaire;
            _fin = fin;
            Transferable = false;
            _debut = debut;
        }

        public void MettreAJour(int salaire, DateTime fin)
        {
            _salaire = salaire;
            _fin = fin;
        }
    }
}