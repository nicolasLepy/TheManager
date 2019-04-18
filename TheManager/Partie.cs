using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;
using TheManager.Exportation;

namespace TheManager
{
    public class Partie
    {
        private DateTime _date;
        private Gestionnaire _gestionnaire;
        private Options _options;

        public DateTime Date { get { return _date; } }
        public Gestionnaire Gestionnaire { get => _gestionnaire; }
        public Options Options { get => _options; }

        public Partie()
        {
            _date = new DateTime(2018, 07, 01);
            _gestionnaire = new Gestionnaire();
            _options = new Options();
        }

        public void Exportations(Competition c)
        {
            if (Utils.ComparerDatesSansAnnee(c.DebutSaison.AddDays(-7), _date))
            {
                Exporteur.Exporter(c);
            }
        }

        public void Avancer()
        {
            _date = _date.AddDays(1);

            foreach (Media m in _gestionnaire.Medias) m.LibererJournalistes();

            foreach (Competition c in _gestionnaire.Competitions)
            {
                if(c.TourActuel > -1)
                {
                    Tour enCours = c.Tours[c.TourActuel];
                    foreach (Match m in enCours.Matchs)
                    {
                        if (Utils.ComparerDates(m.Jour, _date))
                        {
                            m.Jouer();
                            Club_Ville cv = m.Domicile as Club_Ville;
                            Club_Ville ce = m.Exterieur as Club_Ville;
                            if(cv != null && ce != null && (cv.Championnat != null && cv.Championnat.Niveau <= 2 || ce.Championnat != null && ce.Championnat.Niveau <= 2))
                            {
                                foreach (Media media in _gestionnaire.Medias)
                                {
                                    if (media.Couvre(c, c.TourActuel))
                                    {
                                        List<Journaliste> j = new List<Journaliste>();
                                        foreach (Journaliste j1 in media.Journalistes) if (!j1.EstPris) j.Add(j1);

                                        Journaliste commentateur = null;
                                        if(j.Count > 0)
                                        {
                                            j.Sort(new Journalistes_Comparator(cv.Ville));
                                            
                                            if(Math.Abs(Utils.Distance(j[0].Base,cv.Ville)) < 300)
                                            {
                                                commentateur = j[0];
                                            }
                                        }
                                        if(commentateur == null)
                                        {
                                            Journaliste nouveau = new Journaliste(media.Pays.Langue.ObtenirPrenom(), media.Pays.Langue.ObtenirNom(), Session.Instance.Random(28, 60), cv.Ville, 80);
                                            media.Journalistes.Add(nouveau);
                                            commentateur = nouveau;
                                            //Console.WriteLine("Pas de journalistes disponibles pour " + m.Domicile.Nom + "-" + m.Exterieur.Nom);
                                        }

                                        commentateur.EstPris = true;
                                        m.Journalistes.Add(commentateur);


                                    }
                                }
                            }
                            
                        }
                            
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

                if (Options.Exporter) Exportations(c);
            }
            
            
            //Mise à jour annuelle des clubs (sponsors, centre de formation, contrats)
            if(Date.Day == 1 && Date.Month == 7)
            {

                foreach(Media m in _gestionnaire.Medias)
                {
                    foreach(Journaliste j in m.Journalistes)
                    {
                        j.Age++;
                    }
                }

                //Mise à jour du niveau des joueurs sans clubs
                foreach (Joueur j in _gestionnaire.JoueursLibres) j.MiseAJourNiveau();

                //On balaye tous les clubs
                foreach (Club c in _gestionnaire.Clubs)
                {
                    Club_Ville cv = c as Club_Ville;
                    if (cv != null)
                    {
                        cv.Historique.Elements.Add(new EntreeHistorique(new DateTime(Date.Year, Date.Month, Date.Day), cv.Budget, cv.CentreFormation));
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
                        //Console.WriteLine(c.Nom + " - " + cv.Budget.ToString("F20"));
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