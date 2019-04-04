using System;
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
            
            //Mise à jour annuelle des clubs (sponsors, centre de formation, contrats)
            if(Date.Day == 1 && Date.Month == 7)
            {
                //Mise à jour du niveau des joueurs sans clubs
                foreach (Joueur j in _gestionnaire.JoueursLibres) j.MiseAJourNiveau();

                //On balaye tous les clubs
                foreach (Club c in _gestionnaire.Clubs)
                {
                    Club_Ville cv = c as Club_Ville;
                    if (cv != null)
                    {
                        //Prolonger les joueurs
                        List<Contrat> joueursALiberer = new List<Contrat>();
                        foreach (Contrat ct in cv.Contrats)
                        {
                            ct.Joueur.MiseAJourNiveau();
                            if(ct.Fin.Year == Date.Year)
                            {
                                if (!cv.Prolonger(ct)) joueursALiberer.Add(ct);

                            }
                        }
                        //Libérer les joueurs non prolongés
                        foreach(Contrat ct in joueursALiberer)
                        {
                            cv.Contrats.Remove(ct);
                            _gestionnaire.JoueursLibres.Add(ct.Joueur);
                        }

                        cv.ObtenirSponsor();
                        cv.MiseAJourCentreFormation();
                        cv.GenererJeunes();

                        //Affichage budget
                        Console.WriteLine(c.Nom + " - " + cv.Budget.ToString("F20"));
                    }
                }
            }

            //Période des transferts
            if(Date.Month == 7 || Date.Month == 8)
            {

            }

            //Les joueurs libres peuvent partir en retraite
            if(Date.Day == 2 && Date.Month == 7)
            {
                _gestionnaire.RetraiteJoueursLibres();
            }

            //Salaires des clubs && récupération sponsor
            if(Date.Day == 1)
            {
                foreach(Club c in _gestionnaire.Clubs)
                {
                    Club_Ville cv = c as Club_Ville;
                    if(cv != null)
                    {
                        cv.PayerSalaires();
                        cv.SubvensionSponsor();
                    }
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