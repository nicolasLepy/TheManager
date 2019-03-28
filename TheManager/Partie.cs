using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Partie
    {
        private DateTime _date;
        private Gestionnaire _gestionnaire;

        public DateTime Date { get { return _date; } }
        public Gestionnaire Gestionnaire { get => _gestionnaire; }

        public Partie()
        {
            _date = new DateTime(2018, 07, 01);
            _gestionnaire = new Gestionnaire();
        }

        public void Avancer()
        {
            _date = _date.AddDays(1);
            foreach(Competition c in _gestionnaire.Competitions)
            {
                if(c.TourActuel > -1)
                {
                    Tour enCours = c.Tours[c.TourActuel];
                    foreach (Match m in enCours.Matchs)
                    {
                        if (Utils.ComparerDates(m.Jour, _date))
                            m.Jouer();
                    }
                }
                foreach(Tour t in c.Tours)
                {
                    if (Utils.ComparerDatesSansAnnee (t.Programmation.Initialisation, _date))
                    {
                        c.TourSuivant();
                    }
                }
            }
        }
    }
}