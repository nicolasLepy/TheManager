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
    public class Game
    {
        [DataMember]
        private DateTime _date;
        [DataMember]
        private Kernel _kernel;
        [DataMember]
        private Options _options;
        [DataMember]
        private CityClub _club;
        [DataMember]
        private List<Article> _articles;

        /// <summary>
        /// Date of the day
        /// </summary>
        public DateTime date => _date;
        public Kernel kernel { get => _kernel; }
        public Options options { get => _options; }
        /// <summary>
        /// Club controllé par le joueur
        /// </summary>
        public CityClub club { get => _club; set => _club = value; }
        /// <summary>
        /// Manager representing the player
        /// </summary>
        public Manager manager { get => club.manager; }

        public List<Article> articles { get => _articles; }

        public Game()
        {
            _articles = new List<Article>();
            _date = new DateTime(2018, 07, 01);
            _kernel = new Kernel();
            _options = new Options();
            _club = null;
        }

        public void Exports(Tournament t)
        {
            if (Utils.CompareDatesWithoutYear(t.seasonBeginning.AddDays(-7), _date) && options.tournamentsToExport.Contains(t))
            {
                Exporteur.Exporter(t);
            }
        }

        public void Save(string path)
        {
            using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(Game));
                ser.WriteObject(writer, this);
            }
            
        }

        public void Load(string path)
        {
            Game loadObj;
            using (FileStream reader = new FileStream(path,FileMode.Open, FileAccess.Read))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(Game));
                loadObj = (Game)ser.ReadObject(reader);
                _options= loadObj.options;
                this._kernel = loadObj.kernel;
                this._date = loadObj.date;
                this._club = loadObj.club;
            }
        }

        /// <summary>
        /// Manage departures of journalists from a year to the next year
        /// </summary>
        public void UpdateJournalists()
        {
            foreach(Media m in _kernel.medias)
            {
                for(int i = 0;i<m.journalists.Count; i++)
                {
                    Journalist j = m.journalists[i];
                    j.age++;
                    if (j.age > 65)
                    {
                        if (Session.Instance.Random(1, 4) == 1)
                        {
                            m.journalists.Remove(j);
                            i--;
                        }
                    }
                    else
                    {
                        int commentedGamesNumber = j.NumberOfCommentedGames;
                        if(commentedGamesNumber < 10)
                        {
                            int chanceToLeave = commentedGamesNumber - 1;
                            if (chanceToLeave < 1) chanceToLeave = 1;
                            if(Session.Instance.Random(0,chanceToLeave) == 0)
                            {
                                if(m.journalists.Remove(j))
                                {
                                    _kernel.freeJournalists.Add(j);
                                    i--;
                                }
                            }
                        }
                    }

                }
            }
        }

        public void UpdateClubs()
        {

            //Update free players level
            foreach (Player j in _kernel.freePlayers)
            {
                j.UpdateLevel();
            }

            //We check all clubs
            foreach (Club c in _kernel.Clubs)
            {
                CityClub cv = c as CityClub;
                if (cv != null)
                {
                    cv.history.elements.Add(new HistoricEntry(new DateTime(date.Year, date.Month, date.Day), cv.budget, cv.formationFacilities));
                    //Prolong the players
                    List<Contract> playersToFree = new List<Contract>();
                    foreach (Contract ct in cv.contracts)
                    {
                        ct.player.UpdateLevel();
                        if (ct.end.Year == date.Year)
                        {
                            if (!cv.Prolong(ct)) playersToFree.Add(ct);

                        }
                    }
                    //Free not prolonged player
                    foreach (Contract ct in playersToFree)
                    {
                        cv.contracts.Remove(ct);
                        _kernel.freePlayers.Add(ct.player);
                    }

                    cv.GetSponsor();
                    cv.UpdateFormationFacilities();
                    cv.GenerateJuniors();
                    //Put undesirable players on transfers list
                    cv.UpdateTransfertList();

                }
            }
        }

        public void Transfers()
        {
            //First day of the transfers market, clubs create a list of targets
            if (date.Month == 7 && date.Day == 2)
            {
                foreach (Club c in kernel.Clubs)if (c as CityClub != null)
                        (c as CityClub).SearchFreePlayers();
            }
            if (date.Month == 7 || date.Month == 8)
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
                //Clubs search for free players
                foreach (Club c in kernel.Clubs)
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

        private void SetUpMediasForTournaments(List<Match> gamesList, Tournament c)
        {
            List<Match> games = new List<Match>(gamesList);
            foreach(Media media in _kernel.medias)
            {
                if(media.Cover(c,c.currentRound))
                {
                    int numberOfGamesToFollow = games.Count;
                    TournamentCoverage cc = media.GetCoverage(c);
                    if (cc.MinimumGamesNumberOfMultiplex != -1 && games.Count >= cc.MinimumGamesNumberOfMultiplex)
                    {
                        Tour t = c.rounds[c.currentRound];
                        int nbMatchsParJournee = t.Clubs.Count/2;
                        int nbJournees = t.Matchs.Count / nbMatchsParJournee;
                        int j = (t.Matchs.IndexOf(gamesList[0]) / nbMatchsParJournee) + 1;


                        numberOfGamesToFollow = cc.GamesNumberByMultiplex;
                        Normal n = new Normal(3, 1);
                        numberOfGamesToFollow = (int)Math.Round(n.Sample());
                        if (numberOfGamesToFollow < 0)numberOfGamesToFollow = 0;
                        if (numberOfGamesToFollow > games.Count) numberOfGamesToFollow = games.Count;

                        if (nbJournees-j == 1)
                        {
                            games.Sort(new MatchRankingComparator(t as TourChampionnat));
                        }
                        else if (nbJournees - j == 0)
                        {
                            games.Sort(new MatchRankingComparator(t as TourChampionnat));
                        }
                        else if(j<3)
                        {
                            games.Sort(new MatchLevelComparator());
                        }
                        else if(t as TourChampionnat != null) 
                        {
                            games.Sort(new MatchRankingComparator(t as TourChampionnat));
                        }
                        else
                        {
                            games.Sort(new MatchLevelComparator());
                        }
                    }

                    for(int i= 0;i<numberOfGamesToFollow;i++)
                    {
                        Match m = games[i];
                        Ville city = null;
                        if (m.Domicile as CityClub != null) city = (m.Domicile as CityClub).city;
                        if (m.Domicile as ReserveClub != null) city = (m.Domicile as ReserveClub).FannionClub.city;
                        List<Journalist> j = new List<Journalist>();
                        foreach (Journalist j1 in media.journalists) if (!j1.isTaken) j.Add(j1);
                        Journalist journalist = null;
                        if(j.Count > 0)
                        {
                            j.Sort(new JournalistsComparator(city));

                            if(Math.Abs(Utils.Distance(j[0].baseCity, city))< 300)
                            {
                                journalist = j[0];
                            }
                        }
                        if(journalist == null)
                        {
                            Journalist newJournalist = new Journalist(media.country.Langue.GetFirstName(), media.country.Langue.GetLastName(), Session.Instance.Random(28, 60), city, 100);
                            media.journalists.Add(newJournalist);
                            journalist = newJournalist;
                        }
                        journalist.isTaken = true;
                        KeyValuePair<Media, Journalist> employment = new KeyValuePair<Media, Journalist>(journalist.Media, journalist);
                        m.Journalistes.Add(employment);
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

        public List<Match> NextDay()
        {
            _date = _date.AddDays(1);

            List<Match> toPlay = new List<Match>();
            List<Match> clubMatchs = new List<Match>();

            foreach (Media m in _kernel.medias) m.FreeJournalists();

            
            foreach (Tournament c in _kernel.Competitions)
            {
                List<Match> todayGames = new List<Match>();
                if(c.currentRound > -1)
                {
                    Tour currentRound = c.rounds[c.currentRound];
                    foreach (Match m in currentRound.Matchs)
                    {
                        if (Utils.CompareDates(m.Jour, _date))
                        {
                            todayGames.Add(m);
                            m.DefinirCompo();
                            if ((m.Domicile == club || m.Exterieur == club) && !options.simulateGames)
                            {
                                clubMatchs.Add(m);
                            }
                            else
                            {
                                //while (!Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, m.MinuteSuivante())) ;
                                //m.Jouer();
                                if (c.isChampionship && (date.Month == 1 || date.Month == 12) && Session.Instance.Random(1, 26) == 2)
                                    m.Reprogrammer(3);
                                else
                                {
                                    toPlay.Add(m);
                                    //EtablirMediasPourMatch(m, c);
                                }
                            }
                            
                        }       
                    }
                    SetUpMediasForTournaments(todayGames, c);
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

                if (options.ExportEnabled)
                {
                    Exports(c);
                }
            }

            bool clubPlayedHaveAMatch = (clubMatchs.Count > 0) ? true : false;
            foreach(Match m in toPlay)
            {
                if (clubPlayedHaveAMatch && m.Competition == clubMatchs[0].Competition && m.Jour.ToShortTimeString() == clubMatchs[0].Jour.ToShortTimeString())
                    clubMatchs.Add(m);
                else
                    m.Jouer();
            }


            if (date.Month == 6 && date.Day == 15)
            {
                UpdateJournalists();
            }

            //Yearly update of clubs (sponsors, formation facilities, contracts)
            if (date.Day == 1 && date.Month == 7)
            {
                UpdateClubs();
            }

            //Transfers market
            if(options.transfersEnabled)
            {
                Transfers();
            }

            //Free players can be retired
            if (date.Day == 2 && date.Month == 7)
            {
                _kernel.RetirementOfFreePlayers();
            }

            //July 20th => teams set up tickets price
            if(date.Day == 20 && date.Month == 7)
            {
                foreach (Club c in kernel.Clubs) c.SetTicketPrice();
            }

            //Teams are completed at the end of the transfers market if they are not enough players
            if(date.Day == 1 && date.Month == 9)
            {
                foreach (Club c in _kernel.Clubs)
                {
                    CityClub cc = c as CityClub;
                    if (cc != null)
                    {
                        cc.CompleteSquad();
                    }
                }
            }

            //Club pay wages && get sponsor grant
            if(date.Day == 1)
            {
                foreach(Club c in _kernel.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if(cv != null)
                    {
                        cv.PayWages();
                        cv.SponsorGrant();
                    }
                }
            }

            //Building reserves teams the 5th of the month
            if(date.Day == 5)
            {
                foreach(Club c in _kernel.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if(cv !=  null)
                        cv.DispatchPlayersInReserveTeams();
                }
            }

            //Players recover energy
            foreach(Club c in _kernel.Clubs)
            {
                if(c as CityClub != null)
                {
                    foreach (Player j in c.Players())
                    {
                        j.Recover();
                    }
                }
            }
            foreach(Player j in _kernel.freePlayers)
            {
                j.Recover();
            }

            return clubMatchs;
        }
    }
}