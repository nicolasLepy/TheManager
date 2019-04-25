using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TheManager.Comparators;
using TheManager.Exportation;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Partie
    {
        [DataMember]
        private DateTime _date;
        [DataMember]
        private Gestionnaire _gestionnaire;
        [DataMember]
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
            if (Utils.ComparerDatesSansAnnee(c.DebutSaison.AddDays(-7), _date) && Options.CompetitionsAExporter.Contains(c))
            {
                Exporteur.Exporter(c);
            }
        }

        public void Sauvegarder(string chemin)
        {
            using (FileStream writer = new FileStream(chemin, FileMode.Create, FileAccess.Write))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(Partie));
                ser.WriteObject(writer, this);
            }
            
        }

        public void Charger(string chemin)
        {
            Partie loadObj;
            using (FileStream reader = new FileStream(chemin,FileMode.Open, FileAccess.Read))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(Partie));
                loadObj = (Partie)ser.ReadObject(reader);
                _options= loadObj.Options;
                this._gestionnaire = loadObj.Gestionnaire;
                this._date = loadObj.Date;
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
                                            Journaliste nouveau = new Journaliste(media.Pays.Langue.ObtenirPrenom(), media.Pays.Langue.ObtenirNom(), Session.Instance.Random(28, 60), cv.Ville, 100);
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
                    List<Journaliste> toDelete = new List<Journaliste>();
                    foreach(Journaliste j in m.Journalistes)
                    {
                        j.Age++;
                        if (j.Age > 65) if (Session.Instance.Random(1, 8) == 1) toDelete.Add(j);
                    }
                    foreach (Journaliste j in toDelete) m.Journalistes.Remove(j);
                }

                //Mise à jour du niveau des joueurs sans clubs
                foreach (Joueur j in _gestionnaire.JoueursLibres)
                {
                    j.MiseAJourNiveau();
                }

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

                    }
                }
            }

            //Période des transferts
            if(Date.Month == 7 || Date.Month == 8)
            {
                //Joueurs checks leurs offres
                foreach (Club c in Gestionnaire.Clubs) if ((c as Club_Ville) != null) foreach (Joueur j in c.Joueurs()) j.ConsidererOffres();
                List<Joueur> aRetirer = new List<Joueur>();
                foreach (Joueur j in Gestionnaire.JoueursLibres)
                {
                    j.ConsidererOffres();
                    if (j.Club != null) aRetirer.Add(j);
                }
                foreach (Joueur j in aRetirer) Gestionnaire.JoueursLibres.Remove(j);

                //Clubs recherchent des joueurs libres
                foreach (Club c in Gestionnaire.Clubs) if (c as Club_Ville != null)
                    {
                        //Le club part en recherche en moyenne un peu moins d'un fois par semaine
                        if(Session.Instance.Random(1,6) == 1)
                            (c as Club_Ville).RechercherJoueursLibres();
                    } 
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