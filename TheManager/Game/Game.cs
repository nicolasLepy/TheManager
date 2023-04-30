using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TheManager.Comparators;
using TheManager.Exportation;
using MathNet.Numerics.Distributions;
using TheManager.Tournaments;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using LiveCharts;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;

namespace TheManager
{

    public enum SerializationMethod
    {
        DataContractSerializer,
        NewtonsoftJsonSerializer
    }

    /// <summary>
    /// Store data about game universe to verify its long-term stability (budgets, players ...)
    /// </summary>
    [DataContract(IsReference = true)]
    public class GameWorld
    {
        [DataMember]
        private readonly List<int> _totalBudgetInGame;
        [DataMember]
        private readonly List<float> _averagePlayerLevelInGame;
        [DataMember]
        private readonly List<float> _averageClubLevelInGame;
        [DataMember]
        private readonly List<int> _playersInGame;
        [DataMember]
        private readonly List<float> _averageGoals;
        [DataMember]
        private readonly List<float> _rateIndebtesClubs;

        public List<int> TotalBudgetInGame => _totalBudgetInGame;

        public List<float> AveragePlayerLevelInGame => _averagePlayerLevelInGame;

        public List<float> AverageClubLevelInGame => _averageClubLevelInGame;

        public List<int> PlayersInGame => _playersInGame;

        public List<float> AverageGoals => _averageGoals;

        public List<float> RateIndebtesClubs => _rateIndebtesClubs;

        public void AddInfo(int totalBudgetInGame, float averagePlayerLevelInGame, int playersInGame, float averageClubLevelInGame, float averageGoals, float rateIndebtesClubs)
        {
            _totalBudgetInGame.Add(totalBudgetInGame);
            _averagePlayerLevelInGame.Add(averagePlayerLevelInGame);
            _playersInGame.Add(playersInGame);
            _averageClubLevelInGame.Add(averageClubLevelInGame);
            _averageGoals.Add(averageGoals);
            _rateIndebtesClubs.Add(rateIndebtesClubs);
        }

        public GameWorld()
        {
            _totalBudgetInGame = new List<int>();
            _averagePlayerLevelInGame = new List<float>();
            _playersInGame = new List<int>();
            _averageClubLevelInGame = new List<float>();
            _averageGoals = new List<float>();
            _rateIndebtesClubs = new List<float>();
        }
    }

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
        /// <summary>
        /// Current date of the game
        /// </summary>
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
        [DataMember]
        private GameWorld _gameUniverse;

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

        public GameWorld gameUniverse => _gameUniverse;



        public Game()
        {
            _articles = new List<Article>();
            GameDay beginSeasons = new GameDay(Utils.defaultStartWeek, true, 0, 0);
            _date = beginSeasons.ConvertToDateTime(Utils.beginningYear);
            _kernel = new Kernel();
            _options = new Options();
            _club = null;
            _gameUniverse = new GameWorld();
        }

        public DateTime GetBeginDate(Country c)
        {
            GameDay begin = new GameDay(c.resetWeek, true, 0, 0);
            DateTime res = begin.ConvertToDateTime(begin.WeekNumber > Utils.defaultStartWeek ? Utils.beginningYear - 1 : Utils.beginningYear);
            return res;
        }

        /// <summary>
        /// Change the begin date of the game according to the selected club.
        /// A club selected from Ireland for example move the beginning of the game back to january.
        /// For games starting in June, simulate tournament playing between january and june (Ireland league, Copa Libertadores...)
        /// </summary>
        /// <param name="begin"></param>
        public void SetBeginDate(DateTime begin)
        {
            DateTime defaultStart = _date;

            DateTime kernelStart = _date;
            foreach (Continent ct in _kernel.world.continents)
            {
                foreach (Country co in ct.countries)
                {
                    if (co.Tournaments().Count > 0)
                    {
                        DateTime beginCountry = GetBeginDate(co);
                        if (Utils.IsBefore(beginCountry, kernelStart))
                        {
                            kernelStart = beginCountry;
                        }
                    }
                }
            }

            _date = kernelStart;

            Utils.Debug("[Kernel start date] " + _date.ToShortDateString());
            Utils.Debug("[Game start date] " + begin.ToShortDateString());

            foreach (Tournament t in _kernel.world.GetAllTournaments())
            {
                DateTime tBegin = t.seasonBeginning.ConvertToDateTime(Utils.beginningYear);
                if(Utils.IsBefore(_date, tBegin) && Utils.IsBefore(tBegin, defaultStart))
                {
                    Utils.Debug("[AddYearToRemainingYears] " + t.name + " (" + defaultStart.ToShortDateString() + ", " + tBegin.ToShortDateString() + ", " + _date.ToShortDateString());
                    t.AddYearToRemainingYears();
                }
                if(t.name == Utils.friendlyTournamentName)
                {
                    t.NextRound();
                }
            }

            options.simulateGames = true;
            while (!Utils.CompareDates(date, begin))
            {
                Utils.Debug("[Date] " + date.ToShortDateString());
                this.NextDay();
                this.UpdateTournaments();
            }
            options.simulateGames = false;
        }

        /// <summary>
        /// Get current season year (for eg. 2021-2022 => 2022)
        /// TODO: Better to have a season by association and not a global season calendar
        /// </summary>
        public int CurrentSeason
        {
            get
            {
                return Utils.IsBeforeWithoutYear(date, new DateTime(2000, 6, 16)) ? _date.Year : _date.Year+1;
            }
        }

        public void Exports(Tournament t)
        {
            if (Utils.CompareDatesWithoutYear(t.seasonBeginning.ConvertToDateTime().AddDays(-7), _date) && options.tournamentsToExport.Contains(t))
            {
                Exporteur.Exporter(t);
            }
        }

        public void Save(string path)
        {
            SerializationMethod serializationMethod = SerializationMethod.DataContractSerializer;

            path = path.Replace(".csave", ".save");
            if (serializationMethod == SerializationMethod.DataContractSerializer)
            {
                using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    DataContractSerializerSettings dcss = new DataContractSerializerSettings() { MaxItemsInObjectGraph = int.MaxValue };
                    DataContractSerializer ser = new DataContractSerializer(typeof(Game), dcss);
                    ser.WriteObject(writer, this);
                }
            }
            if (serializationMethod == SerializationMethod.NewtonsoftJsonSerializer)
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                string output = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings() 
                { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto });
                using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var sr = new StreamWriter(writer);
                    sr.Write(output);
                }
            }

            //ObjectGraphValidator ogv = new ObjectGraphValidator();
            //ogv.ValidateObjectGraph(this);

            /*FileStream fs = new FileStream(path.Replace(".csave", ".bin"), FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }*/


            if (File.Exists(path.Split('.')[0] + ".csave"))
            {
                File.Delete(path.Split('.')[0] + ".csave");
            }

            using (ZipArchive zip = ZipFile.Open(path.Split('.')[0] + ".csave", ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(path, "save.save");
                File.Delete(path);
                zip.Dispose();
            }

        }

        public void Load(string path)
        {

            SerializationMethod serializationMethod = SerializationMethod.DataContractSerializer;

            Game loadObj = null;
            if (serializationMethod == SerializationMethod.DataContractSerializer)
            {
                using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry cFile = zip.GetEntry("save.save");
                    DataContractSerializer ser = new DataContractSerializer(typeof(Game));
                    loadObj = (Game)ser.ReadObject(cFile.Open());
                    zip.Dispose();
                }
            }
            if(serializationMethod == SerializationMethod.NewtonsoftJsonSerializer)
            {
                using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry cFile = zip.GetEntry("save.save");
                    StreamReader osr = new StreamReader(cFile.Open(), Encoding.Default);
                    string content = osr.ReadToEnd();
                    loadObj = JsonConvert.DeserializeObject<Game>(content, new JsonSerializerSettings()
                    { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto });
                    zip.Dispose();
                }
            }
            this._options = loadObj.options;
            this._kernel = loadObj.kernel;
            this._date = loadObj.date;
            this._club = loadObj.club;
            this._gameUniverse = loadObj.gameUniverse;
            this._articles = loadObj.articles;

        }

        /// <summary>
        /// Manage departures of journalists from a year to the next year
        /// </summary>
        public void UpdateJournalists(Country country)
        {
            foreach(Media m in _kernel.medias)
            {
                if(m.country == country)
                {
                    for (int i = 0; i < m.journalists.Count; i++)
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
                            if (commentedGamesNumber < 10)
                            {
                                int chanceToLeave = commentedGamesNumber - 1;
                                if (chanceToLeave < 1)
                                {
                                    chanceToLeave = 1;
                                }
                                if (Session.Instance.Random(0, chanceToLeave) == 0)
                                {
                                    if (m.journalists.Remove(j))
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
        }

        public void UpdateGameUniverseData()
        {
            int totalBudgetInGame = 0;
            float averagePlayerLevelInGame = 0;
            int playersInGame = 0;
            float averageClubLevelInGame = 0;
            float averageGoals = 0;
            int indebtedClubs = 0;
            
            int clubsCount = 0;
            int playersCount = 0;

            foreach (Club c in _kernel.Clubs)
            {
                if (c is CityClub)
                {
                    clubsCount++;
                    totalBudgetInGame += (c as CityClub).budget;
                    playersCount += c.Players().Count;
                    playersInGame += c.Players().Count;
                    foreach (Player p in c.Players())
                    {
                        averagePlayerLevelInGame += p.level;
                    }
                    
                    averageClubLevelInGame += (c as CityClub).Level();
                    if((c as CityClub).budget < 0)
                    {
                        indebtedClubs++;
                    }
                }
            }

            foreach (Match m in _kernel.Matchs)
            {
                averageGoals += m.score1 + m.score2;
            }
            averageGoals /= _kernel.Matchs.Count;

            averageClubLevelInGame = averageClubLevelInGame / (clubsCount+0.0f);
            averagePlayerLevelInGame = averagePlayerLevelInGame / (playersCount+0.0f);
            

            _gameUniverse.AddInfo(totalBudgetInGame, averagePlayerLevelInGame, playersInGame, averageClubLevelInGame, averageGoals, (indebtedClubs+0.0f)/(clubsCount+0.0f));
        }

        public void UpdateClubs(Country country)
        {
            //Update free players level
            foreach (Player j in _kernel.freePlayers)
            {
                if(j.nationality == country)
                {
                    j.UpdateLevel();
                }
            }

            //We check all clubs
            foreach (Club c in _kernel.Clubs)
            {
                CityClub cv = c as CityClub;
                if (cv != null && cv.Country() == country)
                {
                    Console.WriteLine("[UpdateClub]" + cv.name);
                    DNCGPassage(cv);
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
                    cv.history.elements.Add(new HistoricEntry(new DateTime(date.Year, date.Month, date.Day), cv.budget, cv.formationFacilities, totalAttendance, cv.status));
                    //Prolong the players
                    List<Contract> playersToFree = new List<Contract>();
                    foreach (Contract ct in cv.allContracts)
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
                        cv.RemovePlayer(ct.player);
                        //cv.contracts.Remove(ct);
                        _kernel.freePlayers.Add(ct.player);
                    }

                    cv.GetSponsor();
                    cv.UpdateFormationFacilities();
                    cv.GenerateJuniors();
                    //Put undesirable players on transfers list
                    //cv.UpdateTransfertList();

                    //If savegame size is alleged, we delete all the budget change entry history
                    if (Session.Instance.Game.options.reduceSaveSize)
                    {
                        cv.budgetHistory.Clear();
                    }

                }
            }

        }

        /// <summary>
        /// All clubs finances are examined. If the situation is critical, DNCG can prevent clubs to recruit during the transfers market
        /// </summary>
        public void DNCGPassage(CityClub cc)
        {
            bool noRecruiting = false;
            if((cc.budget < 0) && cc.history.elements.Count > 0 && cc.budget + (cc.budget - cc.history.elements[cc.history.elements.Count-1].budget) < 0)
            {
                noRecruiting = true;
                Utils.Debug(cc.name + " est interdit de recrutement (budget : " + cc.budget + ")");
            }
            cc.isForbiddenToRecruit = noRecruiting;
                
        }

        public void Transfers()
        {
            //First day of the transfers market, clubs create a list of targets
            if (date.Month == 7 && date.Day == 2)
            {
                //Put undesirable players on transfers list
                foreach (Club c in kernel.Clubs)
                {
                    CityClub cc = c as CityClub;
                    if (cc != null && !cc.isForbiddenToRecruit)
                    {
                        cc.UpdateTransfertList();
                    }
                }


                foreach (Club c in kernel.Clubs)
                {
                    CityClub cc = c as CityClub;
                    if (cc != null && !cc.isForbiddenToRecruit)
                    {
                        cc.SearchFreePlayers();
                        cc.SearchInTransferList();
                    }
                }
            }

            if (date.Month == 7 || date.Month == 8)
            {
                //Clubs search for free players
                foreach (Club c in kernel.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if (cv != null)
                    {
                        cv.ConsiderateSendOffers();
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
                            Journalist journalist = media.GetJournalist(city);

                            if (m.primeTimeGame)
                            {
                                Journalist second = media.GetNationalJournalist();
                                if(second == null)
                                {
                                    second = media.GetJournalist(city);
                                }
                                KeyValuePair<Media, Journalist> nationalEmployment = new KeyValuePair<Media, Journalist>(journalist.Media, second);
                                second.isTaken = true;
                                m.journalists.Add(nationalEmployment);
                            }

                            KeyValuePair<Media, Journalist> employment = new KeyValuePair<Media, Journalist>(journalist.Media, journalist);
                            m.journalists.Add(employment);
                        }
                    }
                }
            }
        }

        public void UpdateTournaments()
        {
            foreach (Tournament c in _kernel.Competitions)
            {
                int i = 0;
                foreach (Round t in c.rounds)
                {
                    if (c.remainingYears == c.periodicity)
                    {

                        if (Utils.CompareDates(t.DateEndRound(), _date))
                        {
                            t.QualifyClubs();
                        }
                        if (Utils.CompareDates(t.DateInitialisationRound(), _date) && c.currentRound + 1 == i)
                        {
                            c.NextRound();
                        }
                    }
                    i++;
                }

                if (Utils.CompareDates(c.seasonBeginning.ConvertToDateTime(), _date))
                {
                    c.Reset();
                }

                if (options.ExportEnabled)
                {
                    Exports(c);
                }
            }
        }

        public List<Match> NextDay()
        {
            _date = _date.AddDays(1);

            int weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            /*foreach(Tournament t in _kernel.Competitions)
            {
                
                if(t.IsInternational())
                {
                    Console.WriteLine("=======================");
                    Console.WriteLine(t.name);
                    Console.WriteLine("Initialisation : " + t.seasonBeginning.ConvertToDateTime().ToString());
                    foreach (Round r in t.rounds)
                    {
                        Console.WriteLine(r.name);
                        Console.WriteLine("Begin = " + r.DateInitialisationRound().ToString());
                        Console.WriteLine("End = " + r.DateEndRound().ToString());
                    }
                }
            }*/

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
                    List<Match> matchs = new List<Match>();
                    foreach(Round r in c.rounds)
                    {
                        matchs.AddRange(r.matches);
                    }
                    Round currentRound = c.rounds[c.currentRound];
                    //matchs = currentRound.matches;
                    foreach (Match m in matchs)
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

                foreach(Round round in c.rounds)
                {
                    GroupsRound gRound = round as GroupsRound;
                    if (gRound != null)
                    {
                        gRound.InitStoredGroupQualifications();
                    }
                }
            }

            bool clubPlayedHaveAMatch = (clubMatchs.Count > 0) ? true : false;
            foreach (Match m in toPlay)
            {
                if (clubPlayedHaveAMatch && m.Tournament == clubMatchs[0].Tournament && m.day.ToShortTimeString() == clubMatchs[0].day.ToShortTimeString())
                {
                    clubMatchs.Add(m);
                }
                else
                {
                    m.Play();
                }
            }

            foreach (Country c in kernel.world.GetAllCountries())
            {
                //Update clubs before resetting league. Don't update clubs if the game date is before the start of the season of this country.
                if(Utils.Modulo(c.resetWeek-1, 52) == weekNumber && date.DayOfWeek == DayOfWeek.Wednesday && Utils.IsBefore(new GameDay(c.resetWeek, true, 0, 0).ConvertToDateTime(Utils.beginningYear), _date))
                {
                    UpdateClubs(c);
                    UpdateJournalists(c);
                }

                if (Utils.Modulo(c.resetWeek+5, 52) == weekNumber && date.DayOfWeek == DayOfWeek.Wednesday)
                {
                    c.administrativeRetrogradations.Clear();
                    foreach (Club countryClub in kernel.Clubs)
                    {
                        if(countryClub.Country() == c)
                        {
                            countryClub.SetTicketPrice();
                        }
                    }
                }

            }

            //Yearly update of clubs (sponsors, formation facilities, contracts)
            if (weekNumber == kernel.world.resetWeek && date.DayOfWeek == DayOfWeek.Wednesday)
            {
                UpdateGameUniverseData();
            }

            foreach (Continent c in kernel.world.continents)
            {
                if(weekNumber == c.resetWeek && date.DayOfWeek == DayOfWeek.Wednesday) //In DB tournaments are reset at 25. Game starts this day
                {
                    c.QualifiesClubForContinentalCompetitionNextYear();
                }
                if(weekNumber == (c.resetWeek + 2)%52 && date.DayOfWeek == DayOfWeek.Wednesday)
                {
                    c.UpdateStoredAssociationRanking();
                }
            }

            //Transfers market
            if (options.transfersEnabled)
            {
                Transfers();
            }

            //Free players can be retired
            if (date.Day == 17 && date.Month == 6)
            {
                _kernel.RetirementOfFreePlayers();
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
                    NationalTeam nt = c as NationalTeam;
                    if(nt != null)
                    {
                        nt.UpdateFifaPoints();
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

            /*Console.WriteLine("[Clubs elo]");
            List<Club> totalClubs = new List<Club>(this.kernel.Clubs);
            totalClubs.Sort(new ClubComparator(ClubAttribute.ELO, false));
            for(int i = 0; i < 10; i++)
            {
                Console.WriteLine(totalClubs[i].name + " - " + totalClubs[i].elo);
            }
            Console.WriteLine("...");
            for (int i = totalClubs.Count-1; i > totalClubs.Count-10; i--)
            {
                Console.WriteLine(totalClubs[i].name + " - " + totalClubs[i].elo);
            }
            float totalElo = 0;
            foreach(Club c in totalClubs)
            {
                totalElo += c.elo;
            }
            Console.WriteLine("Elo Sum = " + totalElo);*/
            
            return clubMatchs;
        }
    }
}