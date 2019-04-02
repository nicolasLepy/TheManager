﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Exportation;

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
                    if(Utils.ComparerDatesSansAnnee(t.Programmation.Fin, _date))
                    {
                        t.QualifierClubs();
                    }
                    if (Utils.ComparerDatesSansAnnee (t.Programmation.Initialisation, _date))
                    {
                        c.TourSuivant();
                    }
                }

                if(Utils.ComparerDatesSansAnnee(c.DebutSaison,_date))
                {
                    c.RAZ();
                }

                if(Utils.ComparerDatesSansAnnee(c.DebutSaison.AddDays(-7),_date))
                {
                    Exporteur.Exporter(c);
                }
            }

            //Récupération des joueurs
            foreach(Club c in _gestionnaire.Clubs)
            {
                if(c as Club_Ville != null)
                {
                    foreach (Joueur j in c.Joueurs())
                    {
                        j.Recuperer();
                    }
                }
            }
            foreach(Joueur j in _gestionnaire.JoueursLibres)
            {
                j.Recuperer();
            }
        }
    }
}