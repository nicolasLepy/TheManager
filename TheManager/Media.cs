﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{

    [DataContract]
    public struct CouvertureCompetition : IEquatable<CouvertureCompetition>
    {
        [DataMember]
        public Tournament Competition { get; set; }
        [DataMember]
        public int IndexDebut { get; set; }
        [DataMember]
        public int NombreMatchsMiniMultiplex { get; set; }
        [DataMember]
        public int NombreMatchsParMultiplex { get; set; }

        public CouvertureCompetition(Tournament competition, int indexDebut, int nombreMatchsMiniMultiplex, int nombreMatchsParMultiplex)
        {
            Competition = competition;
            IndexDebut = indexDebut;
            NombreMatchsMiniMultiplex = nombreMatchsMiniMultiplex;
            NombreMatchsParMultiplex = nombreMatchsParMultiplex;

        }

        public bool Equals(CouvertureCompetition other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract(IsReference =true)]
    public class Media
    {
        [DataMember]
        private string _nom;
        [DataMember]
        private List<Journaliste> _journalistes;
        [DataMember]
        private List<CouvertureCompetition> _couvertures;
        [DataMember]
        private Pays _pays;

        public string Nom { get => _nom; }
        public List<Journaliste> Journalistes { get => _journalistes; }
        public List<CouvertureCompetition> Couvertures { get => _couvertures; }
        public Pays Pays { get => _pays; }

        public Media(string nom, Pays pays)
        {
            _nom = nom;
            _journalistes = new List<Journaliste>();
            _couvertures = new List<CouvertureCompetition>();
            _pays = pays;
        }

        public bool Couvre(Tournament c, int indexTour)
        {
            bool res = false;
            foreach(CouvertureCompetition cc in Couvertures)
            {
                if (cc.Competition == c && cc.IndexDebut <= indexTour) res = true;
            }
            return res;
        }

        public CouvertureCompetition GetCouverture(Tournament competition)
        {
            CouvertureCompetition res = _couvertures[0];

            foreach(CouvertureCompetition c in _couvertures)
            {
                if (c.Competition == competition) res = c;
            }

            return res;
        }

        public void LibererJournalistes()
        {
            foreach (Journaliste j in _journalistes) j.EstPris = false;
        }

    }
}