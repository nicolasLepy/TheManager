using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TheManager.Comparators;
using TheManager.Exportation;
using MathNet.Numerics.Distributions;

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
        [DataMember]
        private CityClub _club;
        [DataMember]
        private List<Article> _articles;

        /// <summary>
        /// Date du jeur
        /// </summary>
        public DateTime Date { get { return _date; } }
        public Gestionnaire Gestionnaire { get => _gestionnaire; }
        public Options Options { get => _options; }
        /// <summary>
        /// Club controllé par le joueur
        /// </summary>
        public CityClub Club { get => _club; set => _club = value; }
        /// <summary>
        /// Entraîneur représentant le joueur
        /// </summary>
        public Entraineur Entraineur { get => Club.manager; }

        public List<Article> Articles { get => _articles; }

        public Partie()
        {
            _articles = new List<Article>();
            _date = new DateTime(2018, 07, 01);
            _gestionnaire = new Gestionnaire();
            _options = new Options();
            _club = null;
        }

        public void Exportations(Tournament c)
        {
            if (Utils.CompareDatesWithoutYear(c.seasonBeginning.AddDays(-7), _date) && Options.CompetitionsAExporter.Contains(c))
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
                this._club = loadObj.Club;
            }
        }

        /// <summary>
        /// Gère les départs des journalistes d'une année à l'autre
        /// </summary>
        public void MiseAJourJournalistes()
        {
            foreach(Media m in _gestionnaire.Medias)
            {
                for(int i = 0;i<m.Journalistes.Count; i++)
                {
                    Journaliste j = m.Journalistes[i];
                    j.Age++;
                    if (j.Age > 65)
                    {
                        if (Session.Instance.Random(1, 4) == 1)
                        {
                            m.Journalistes.Remove(j);
                            i--;
                        }
                    }
                    else
                    {
                        int nbMatchsCommentes = j.NombreMatchsCommentes;
                        if(nbMatchsCommentes < 10)
                        {
                            int chanceDePartir = nbMatchsCommentes - 1;
                            if (chanceDePartir < 1) chanceDePartir = 1;
                            if(Session.Instance.Random(0,chanceDePartir) == 0)
                            {
                                if(m.Journalistes.Remove(j))
                                {
                                    _gestionnaire.JournalistesLibres.Add(j);
                                    i--;
                                }
                            }
                        }
                    }

                }
            }
        }

        public void MiseAJourDesClubs()
        {

            //Mise à jour du niveau des joueurs sans clubs
            foreach (Joueur j in _gestionnaire.JoueursLibres)
            {
                j.MiseAJourNiveau();
            }

            //On balaye tous les clubs
            foreach (Club c in _gestionnaire.Clubs)
            {
                CityClub cv = c as CityClub;
                if (cv != null)
                {
                    cv.history.Elements.Add(new EntreeHistorique(new DateTime(Date.Year, Date.Month, Date.Day), cv.budget, cv.formationFacilities));
                    //Prolonger les joueurs
                    List<Contrat> joueursALiberer = new List<Contrat>();
                    foreach (Contrat ct in cv.contracts)
                    {
                        ct.Joueur.MiseAJourNiveau();
                        if (ct.Fin.Year == Date.Year)
                        {
                            if (!cv.Prolong(ct)) joueursALiberer.Add(ct);

                        }
                    }
                    //Libérer les joueurs non prolongés
                    foreach (Contrat ct in joueursALiberer)
                    {
                        cv.contracts.Remove(ct);
                        _gestionnaire.JoueursLibres.Add(ct.Joueur);
                    }

                    cv.GetSponsor();
                    cv.UpdateFormationFacilities();
                    cv.GenerateJuniors();
                    //Mettre les joueurs les plus indésirables sur la liste des transferts
                    cv.UpdateTransfertList();

                }
            }
        }

        public void Transferts()
        {
            //Premier jour du mercato, le club identifie une liste de joueurs à recruter
            if (Date.Month == 7 && Date.Day == 2)
            {
                foreach (Club c in Gestionnaire.Clubs) if (c as CityClub != null)
                        (c as CityClub).SearchFreePlayers();
            }
            if (Date.Month == 7 || Date.Month == 8)
            {

                //Joueurs checks leurs offres
                /*foreach (Club c in Gestionnaire.Clubs) if ((c as Club_Ville) != null) foreach (Joueur j in c.Joueurs()) j.ConsidererOffres();
                List<Joueur> aRetirer = new List<Joueur>();
                foreach (Joueur j in Gestionnaire.JoueursLibres)
                {
                    j.ConsidererOffres();
                    if (j.Club != null) aRetirer.Add(j);
                }
                foreach (Joueur j in aRetirer) Gestionnaire.JoueursLibres.Remove(j);
                */
                //Clubs recherchent des joueurs libres
                foreach (Club c in Gestionnaire.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if (cv != null)
                    {
                        cv.ConsiderateOffers();
                        cv.SendOfferToPlayers();
                    }
                }
            }
        }

        private void EtablrMediasPourCompetition(List<Match> listeMatchs, Tournament c)
        {
            List<Match> matchs = new List<Match>(listeMatchs);
            foreach(Media media in _gestionnaire.Medias)
            {
                if(media.Couvre(c,c.currentRound))
                {
                    int nbMatchsASuivre = matchs.Count;
                    CouvertureCompetition cc = media.GetCouverture(c);
                    if (cc.NombreMatchsMiniMultiplex != -1 && matchs.Count >= cc.NombreMatchsMiniMultiplex)
                    {
                        Tour t = c.rounds[c.currentRound];
                        int nbMatchsParJournee = t.Clubs.Count/2;
                        int nbJournees = t.Matchs.Count / nbMatchsParJournee;
                        int j = (t.Matchs.IndexOf(listeMatchs[0]) / nbMatchsParJournee) + 1;


                        nbMatchsASuivre = cc.NombreMatchsParMultiplex;
                        Normal n = new Normal(3, 1);
                        nbMatchsASuivre = (int)Math.Round(n.Sample());
                        if (nbMatchsASuivre < 0) nbMatchsASuivre = 0;
                        if (nbMatchsASuivre > matchs.Count) nbMatchsASuivre = matchs.Count;

                        if (nbJournees-j == 1)
                        {
                            matchs.Sort(new Match_Classement_Comparator(t as TourChampionnat));
                        }
                        else if (nbJournees - j == 0)
                        {
                            matchs.Sort(new Match_Classement_Comparator(t as TourChampionnat));
                        }
                        else if(j<3)
                        {
                            matchs.Sort(new Match_Niveau_Comparator());
                        }
                        else if(t as TourChampionnat != null) 
                        {
                            matchs.Sort(new Match_Classement_Comparator(t as TourChampionnat));
                        }
                        else
                        {
                            matchs.Sort(new Match_Niveau_Comparator());
                        }
                    }

                    for(int i= 0;i<nbMatchsASuivre;i++)
                    {
                        Match m = matchs[i];
                        Ville ville = null;
                        if (m.Domicile as CityClub != null) ville = (m.Domicile as CityClub).city;
                        if (m.Domicile as ReserveClub != null) ville = (m.Domicile as ReserveClub).FannionClub.city;
                        List<Journaliste> j = new List<Journaliste>();
                        foreach (Journaliste j1 in media.Journalistes) if (!j1.EstPris) j.Add(j1);
                        Journaliste commentateur = null;
                        if(j.Count > 0)
                        {
                            j.Sort(new Journalistes_Comparator(ville));

                            if(Math.Abs(Utils.Distance(j[0].Base, ville))< 300)
                            {
                                commentateur = j[0];
                            }
                        }
                        if(commentateur == null)
                        {
                            Journaliste nouveau = new Journaliste(media.Pays.Langue.ObtenirPrenom(), media.Pays.Langue.ObtenirNom(), Session.Instance.Random(28, 60), ville, 100);
                            media.Journalistes.Add(nouveau);
                            commentateur = nouveau;
                        }
                        commentateur.EstPris = true;
                        KeyValuePair<Media, Journaliste> poste = new KeyValuePair<Media, Journaliste>(commentateur.Media, commentateur);
                        m.Journalistes.Add(poste);
                    }
                }
            }
        }

       /*
        private void EtablirMediasPourMatch(Match m, Competition c)
        {
            Club_Ville cv = m.Domicile as Club_Ville;
            Club_Ville ce = m.Exterieur as Club_Ville;
            if (cv != null && ce != null && (cv.Championnat != null && cv.Championnat.Niveau <= 2 || ce.Championnat != null && ce.Championnat.Niveau <= 2))
            {
                foreach (Media media in _gestionnaire.Medias)
                {
                    if (media.Couvre(c, c.TourActuel))
                    {
                        List<Journaliste> j = new List<Journaliste>();
                        foreach (Journaliste j1 in media.Journalistes) if (!j1.EstPris) j.Add(j1);

                        Journaliste commentateur = null;
                        if (j.Count > 0)
                        {
                            j.Sort(new Journalistes_Comparator(cv.Ville));

                            if (Math.Abs(Utils.Distance(j[0].Base, cv.Ville)) < 300)
                            {
                                commentateur = j[0];
                            }
                        }
                        if (commentateur == null)
                        {
                            Journaliste nouveau = new Journaliste(media.Pays.Langue.ObtenirPrenom(), media.Pays.Langue.ObtenirNom(), Session.Instance.Random(28, 60), cv.Ville, 100);
                            media.Journalistes.Add(nouveau);
                            commentateur = nouveau;
                        }

                        commentateur.EstPris = true;
                        m.Journalistes.Add(commentateur);


                    }
                }
            }
        }*/

        public List<Match> Avancer()
        {
            _date = _date.AddDays(1);

            List<Match> aJouer = new List<Match>();
            List<Match> matchClub = new List<Match>();

            foreach (Media m in _gestionnaire.Medias) m.LibererJournalistes();

            
            foreach (Tournament c in _gestionnaire.Competitions)
            {
                List<Match> matchsDuJour = new List<Match>();
                if(c.currentRound > -1)
                {
                    Tour enCours = c.rounds[c.currentRound];
                    foreach (Match m in enCours.Matchs)
                    {
                        if (Utils.CompareDates(m.Jour, _date))
                        {
                            matchsDuJour.Add(m);
                            m.DefinirCompo();
                            if ((m.Domicile == Club || m.Exterieur == Club) && !Options.SimulerMatchs)
                            {
                                matchClub.Add(m);
                            }
                            else
                            {
                                //while (!Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, m.MinuteSuivante())) ;
                                //m.Jouer();
                                if (c.isChampionship && (Date.Month == 1 || Date.Month == 12) && Session.Instance.Random(1, 26) == 2)
                                    m.Reprogrammer(3);
                                else
                                {
                                    aJouer.Add(m);
                                    //EtablirMediasPourMatch(m, c);
                                }
                            }
                            
                        }       
                    }
                    EtablrMediasPourCompetition(matchsDuJour, c);
                }
                foreach(Tour t in c.rounds)
                {
                    if(Utils.CompareDatesWithoutYear(t.Programmation.Fin, _date))
                    {
                        t.QualifierClubs();
                    }
                    if (Utils.CompareDatesWithoutYear (t.Programmation.Initialisation, _date))
                    {
                        c.NextRound();
                    }
                }

                if(Utils.CompareDatesWithoutYear(c.seasonBeginning,_date))
                {
                    c.Reset();
                }

                if (Options.Exporter) Exportations(c);
            }

            bool clubAMatch = (matchClub.Count > 0) ? true : false;
            foreach(Match m in aJouer)
            {
                if (clubAMatch && m.Competition == matchClub[0].Competition && m.Jour.ToShortTimeString() == matchClub[0].Jour.ToShortTimeString())
                    matchClub.Add(m);
                else
                    m.Jouer();
            }


            if (Date.Month == 6 && Date.Day == 15)
            {
                MiseAJourJournalistes();
            }

            //Mise à jour annuelle des clubs (sponsors, centre de formation, contrats)
            if (Date.Day == 1 && Date.Month == 7)
            {
                MiseAJourDesClubs();
            }

            //Période des transferts
            if(Options.Transferts)
            {
                Transferts();
            }

            //Les joueurs libres peuvent partir en retraite
            if (Date.Day == 2 && Date.Month == 7)
            {
                _gestionnaire.RetraiteJoueursLibres();
            }

            //20 juillet => les équipes mettent en place le prix des billets
            if(Date.Day == 20 && Date.Month == 7)
            {
                foreach (Club c in Gestionnaire.Clubs) c.SetTicketPrice();
            }

            //Les équipes sont complétées à la fin de la période de transfert si elles n'ont pas assez de joueurs
            if(Date.Day == 1 && Date.Month == 9)
            {
                foreach (Club c in _gestionnaire.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if (cv != null)
                    {
                        cv.CompleteSquad();
                    }
                }
            }

            //Salaires des clubs && récupération sponsor
            if(Date.Day == 1)
            {
                foreach(Club c in _gestionnaire.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if(cv != null)
                    {
                        cv.PayWages();
                        cv.SponsorGrant();
                    }
                }
            }

            //Construction des équipes réserves le 5 du mois
            if(Date.Day == 5)
            {
                foreach(Club c in _gestionnaire.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if(cv !=  null)
                        cv.DispatchPlayersInReserveTeams();
                }
            }

            //Récupération des joueurs
            foreach(Club c in _gestionnaire.Clubs)
            {
                if(c as CityClub != null)
                {
                    foreach (Joueur j in c.Players())
                    {
                        j.Recuperer();
                    }
                }
            }
            foreach(Joueur j in _gestionnaire.JoueursLibres)
            {
                j.Recuperer();
            }

            return matchClub;
        }
    }
}