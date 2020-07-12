using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TheManager.Comparators;
using TheManager.Exportation;
using MathNet.Numerics.Distributions;

namespace TheManager
{

    public struct Transfer : IEquatable<Transfer>
    {
        private readonly CityClub _from;
        private readonly CityClub _to;
        private readonly DateTime _date;

        public CityClub From => _from;
        public CityClub To => _to;
        public DateTime Date => _date;

        public Transfer(CityClub from, CityClub to, DateTime date)
        {
            _from = from;
            _to = to;
            _date = date;
        }

        public bool Equals(Transfer other)
        {
            return false;
        }
    }

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
                            if (chanceToLeave < 1)
                            {
                                chanceToLeave = 1;
                            }
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
                    int totalGames = 0;
                    int totalAttendance = 0;
                    foreach(Match m in cv.Games)
                    {
                        if (m.home == cv)
                        {
                            totalGames++;
                            totalAttendance += m.attendance;
                        }
                    }
                    if(totalGames > 0)
                    {
                        totalAttendance /= totalGames;
                    }
                    cv.history.elements.Add(new HistoricEntry(new DateTime(date.Year, date.Month, date.Day), cv.budget, cv.formationFacilities, totalAttendance));
                    //Prolong the players
                    List<Contract> playersToFree = new List<Contract>();
                    foreach (Contract ct in cv.contracts)
                    {
                        ct.player.UpdateLevel();
                        if (ct.end.Year == date.Year)
                        {
                            if (!cv.Prolong(ct))
                            {
                                playersToFree.Add(ct);
                            }

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

                    //If savegame size is alleged, we delete all the budget change entry history
                    if (Session.Instance.Game.options.reduceSaveSize)
                    {
                        cv.budgetHistory.Clear();
                    }

                }
            }

        }

        public void Transfers()
        {
            //First day of the transfers market, clubs create a list of targets
            if (date.Month == 7 && date.Day == 2)
            {
                foreach (Club c in kernel.Clubs)
                {
                    if (c as CityClub != null)
                    {
                        (c as CityClub).SearchFreePlayers();
                    }
                }
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
                        Round t = c.rounds[c.currentRound];
                        int nbMatchsParJournee = t.clubs.Count/2;
                        int nbJournees = t.matches.Count / nbMatchsParJournee;
                        int j = (t.matches.IndexOf(gamesList[0]) / nbMatchsParJournee) + 1;


                        Normal n = new Normal(3, 1);
                        numberOfGamesToFollow = (int)Math.Round(n.Sample());
                        if (numberOfGamesToFollow < 0)
                        {
                            numberOfGamesToFollow = 0;
                        }

                        if (numberOfGamesToFollow > games.Count)
                        {
                            numberOfGamesToFollow = games.Count;
                        }

                        if (nbJournees-j == 1)
                        {
                            games.Sort(new MatchRankingComparator(t as ChampionshipRound));
                        }
                        else if (nbJournees - j == 0)
                        {
                            games.Sort(new MatchRankingComparator(t as ChampionshipRound));
                        }
                        else if(j<3)
                        {
                            games.Sort(new MatchLevelComparator());
                        }
                        else if(t as ChampionshipRound != null) 
                        {
                            games.Sort(new MatchRankingComparator(t as ChampionshipRound));
                        }
                        else
                        {
                            games.Sort(new MatchLevelComparator());
                        }
                    }

                    for(int i= 0;i<numberOfGamesToFollow;i++)
                    {
                        Match m = games[i];

                        bool followGame = true;
                        CityClub homeCityClub = m.home as CityClub;
                        CityClub awayCityClub = m.away as CityClub;
                        if (cc.MinimumLevel != -1 && homeCityClub != null && homeCityClub.Championship != null && homeCityClub.Championship.level > cc.MinimumLevel
                            && awayCityClub != null && awayCityClub.Championship != null && awayCityClub.Championship.level > cc.MinimumLevel)
                        {
                            followGame = false;
                        }
                        
                        if (followGame)
                        {
                            City city = null;
                            if (m.home as CityClub != null)
                            {
                                city = (m.home as CityClub).city;
                            }

                            if (m.home as ReserveClub != null)
                            {
                                city = (m.home as ReserveClub).FannionClub.city;
                            }
                            List<Journalist> j = new List<Journalist>();
                            foreach (Journalist j1 in media.journalists)
                            {
                                if (!j1.isTaken)
                                {
                                    j.Add(j1);
                                }
                            }
                            Journalist journalist = null;
                            if (j.Count > 0)
                            {
                                j.Sort(new JournalistsComparator(city));

                                if (Math.Abs(Utils.Distance(j[0].baseCity, city)) < 300)
                                {
                                    journalist = j[0];
                                }
                            }
                            if (journalist == null)
                            {
                                Journalist newJournalist = new Journalist(media.country.language.GetFirstName(), media.country.language.GetLastName(), Session.Instance.Random(28, 60), city, 100);
                                media.journalists.Add(newJournalist);
                                journalist = newJournalist;
                            }
                            journalist.isTaken = true;
                            KeyValuePair<Media, Journalist> employment = new KeyValuePair<Media, Journalist>(journalist.Media, journalist);
                            m.journalists.Add(employment);
                        }
                    }
                }
            }
        }

        public List<Match> NextDay()
        {
            _date = _date.AddDays(1);

            List<Match> toPlay = new List<Match>();
            List<Match> clubMatchs = new List<Match>();

            foreach (Media m in _kernel.medias)
            {
                m.FreeJournalists();
            }

            
            foreach (Tournament c in _kernel.Competitions)
            {
                List<Match> todayGames = new List<Match>();
                if(c.currentRound > -1)
                {
                    Round currentRound = c.rounds[c.currentRound];
                    foreach (Match m in currentRound.matches)
                    {
                        if (Utils.CompareDates(m.day, _date))
                        {
                            todayGames.Add(m);
                            m.SetCompo();
                            if ((m.home == club || m.away == club) && !options.simulateGames)
                            {
                                clubMatchs.Add(m);
                            }
                            else
                            {
                                if (c.isChampionship && (date.Month == 1 || date.Month == 12) &&
                                    Session.Instance.Random(1, 26) == 2)
                                {
                                    m.Reprogram(3);
                                }
                                else
                                {
                                    toPlay.Add(m);
                                }
                            }
                            
                        }
                    }
                    SetUpMediasForTournaments(todayGames, c);
                }
                foreach(Round t in c.rounds)
                {
                    if(Utils.CompareDatesWithoutYear(t.programmation.end, _date))
                    {
                        t.QualifyClubs();
                    }
                    if (Utils.CompareDatesWithoutYear(t.programmation.initialisation, _date))
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
                if (clubPlayedHaveAMatch && m.Tournament == clubMatchs[0].Tournament && m.day.ToShortTimeString() == clubMatchs[0].day.ToShortTimeString()){                
                    clubMatchs.Add(m);
                }
                else{
                    m.Play();
                }
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
                foreach (Club c in kernel.Clubs)
                {
                    c.SetTicketPrice();
                }
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
                    if(cv !=  null){
                        cv.DispatchPlayersInReserveTeams();
                    }
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