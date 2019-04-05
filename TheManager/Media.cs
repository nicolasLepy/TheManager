using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{

    public struct CouvertureCompetition
    {
        public Competition Competition { get; set; }
        public int IndexDebut { get; set; }

        public CouvertureCompetition(Competition competition, int indexDebut)
        {
            Competition = competition;
            IndexDebut = indexDebut;
        }
    }

    public class Media
    {
        private string _nom;
        private List<Journaliste> _journalistes;
        private List<CouvertureCompetition> _couvertures;
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

        public bool Couvre(Competition c, int indexTour)
        {
            bool res = false;
            foreach(CouvertureCompetition cc in Couvertures)
            {
                if (cc.Competition == c && cc.IndexDebut <= indexTour) res = true;
            }
            return res;
        }

        public void LibererJournalistes()
        {
            foreach (Journaliste j in _journalistes) j.EstPris = false;
        }

    }
}