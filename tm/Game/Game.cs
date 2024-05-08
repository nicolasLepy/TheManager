using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using tm.Comparators;
using tm.Exportation;
using MathNet.Numerics.Distributions;
using tm.Tournaments;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;
using System.Linq;
using MathNet.Numerics;
using tm.persistance.datacontract;
using tm.persistance.sqlite;
using tm.persistance.nhibernate;
using tm.persistance;

namespace tm
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
        private readonly List<float> _averageFormationInGame;
        [DataMember]
        private readonly List<int> _playersInGame;
        [DataMember]
        private readonly List<float> _averageGoals;
        [DataMember]
        private readonly List<float> _rateIndebtesClubs;

        public List<int> TotalBudgetInGame => _totalBudgetInGame;

        public List<float> AveragePlayerLevelInGame => _averagePlayerLevelInGame;

        public List<float> AverageClubLevelInGame => _averageClubLevelInGame;
        public List<float> AverageFormationInGame => _averageFormationInGame;

        public List<int> PlayersInGame => _playersInGame;

        public List<float> AverageGoals => _averageGoals;

        public List<float> RateIndebtesClubs => _rateIndebtesClubs;

        public void AddInfo(int totalBudgetInGame, float averagePlayerLevelInGame, int playersInGame, float averageClubLevelInGame, float averageFormationInGame, float averageGoals, float rateIndebtesClubs)
        {
            _totalBudgetInGame.Add(totalBudgetInGame);
            _averagePlayerLevelInGame.Add(averagePlayerLevelInGame);
            _playersInGame.Add(playersInGame);
            _averageClubLevelInGame.Add(averageClubLevelInGame);
            _averageFormationInGame.Add(averageFormationInGame);
            _averageGoals.Add(averageGoals);
            _rateIndebtesClubs.Add(rateIndebtesClubs);
        }

        public GameWorld()
        {
            _totalBudgetInGame = new List<int>();
            _averagePlayerLevelInGame = new List<float>();
            _playersInGame = new List<int>();
            _averageClubLevelInGame = new List<float>();
            _averageFormationInGame = new List<float>();
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
    [KnownType(typeof(Continent))]
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

        public void AttachKernel(Kernel kernel)
        {
            _kernel = kernel;
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
                    t.rounds[0].Setup();
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
            if (Utils.CompareDatesWithoutYear(t.seasonBeginning.ConvertToDateTime().AddDays(-7), _date))
            {
                Exporteur.Exporter(t);
            }
        }

        public void Save(string path)
        {

            //ObjectGraphValidator ogv = new ObjectGraphValidator();
            //ogv.ValidateObjectGraph(this);

            var timeDataContract = System.Diagnostics.Stopwatch.StartNew();
            DataContractProvider provider = new DataContractProvider(path);
            provider.Save(this);
            timeDataContract.Stop();
            Console.WriteLine(String.Format("[Save] DataContract : {0} ms", timeDataContract.ElapsedMilliseconds));

        }

        public void Load(string path)
        {
            var timeDataContract = System.Diagnostics.Stopwatch.StartNew();
            DataContractProvider provider = new DataContractProvider(path);
            Game loadObj = provider.Load();
            this._options = loadObj.options;
            this._kernel = loadObj.kernel;
            this._date = loadObj.date;
            this._club = loadObj.club;
            this._gameUniverse = loadObj.gameUniverse;
            this._articles = loadObj.articles;
            Console.WriteLine(String.Format("[Load] DataContract : {0} ms", timeDataContract.ElapsedMilliseconds));

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
            float averageFormationInGame = 0;
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
                    averageFormationInGame += c.formationFacilities;
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
            averageFormationInGame = averageFormationInGame / (clubsCount + 0.0f);
            averagePlayerLevelInGame = averagePlayerLevelInGame / (playersCount+0.0f);


            _gameUniverse.AddInfo(totalBudgetInGame, averagePlayerLevelInGame, playersInGame, averageClubLevelInGame, averageFormationInGame, averageGoals, (indebtedClubs+0.0f)/(clubsCount+0.0f));
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
                    cv.GenerateFriendlyGamesCalendar();
                    //Put undesirable players on transfers list
                    //cv.UpdateTransfertList();

                    //If savegame size is alleged, we delete all the budget change entry history
                    if (options.reduceSaveSize)
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

        private void SetUpMediasForTournaments(List<Match> gamesList, Tournament c, Round r)
        {
            List<Match> games = new List<Match>(gamesList);
            foreach(Media media in _kernel.medias)
            {
                if(media.Cover(c,c.rounds.IndexOf(r)))
                {
                    int numberOfGamesToFollow = games.Count;
                    TournamentCoverage cc = media.GetCoverage(c);
                    if (cc.MinimumGamesNumberOfMultiplex != -1 && games.Count >= cc.MinimumGamesNumberOfMultiplex)
                    {
                        int nbMatchsParJournee = r.clubs.Count/2;
                        int nbJournees = r.matches.Count / nbMatchsParJournee;
                        int j = (r.matches.IndexOf(gamesList[0]) / nbMatchsParJournee) + 1;


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
                            games.Sort(new MatchRankingComparator(r as ChampionshipRound));
                        }
                        else if (nbJournees - j == 0)
                        {
                            games.Sort(new MatchRankingComparator(r as ChampionshipRound));
                        }
                        else if(j<3)
                        {
                            games.Sort(new MatchLevelComparator());
                        }
                        else if(r as ChampionshipRound != null) 
                        {
                            games.Sort(new MatchRankingComparator(r as ChampionshipRound));
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
                                m.medias.Add(nationalEmployment);
                            }

                            KeyValuePair<Media, Journalist> employment = new KeyValuePair<Media, Journalist>(journalist.Media, journalist);
                            m.medias.Add(employment);
                        }
                    }
                }
            }
        }

        public void AutoSave()
        {
            this.Save(String.Format("save_{0}.csave", this.date.Year));
        }

        public bool UpdateTournaments()
        {
            bool valid = true;
            foreach (Tournament c in _kernel.Competitions)
            {
                int i = 0;
                Country tc = (_kernel.LocalisationTournament(c) as Country);
                foreach (Round t in c.rounds)
                {
                    //First game day of the season
                    if (c.isChampionship && c.rounds.IndexOf(t) == 0 && t.programmation.gamesDays.Count > 0 && Utils.CompareDates(t.programmation.gamesDays[0].ConvertToDateTime(), _date))
                    {
                        foreach(Club cl in t.clubs)
                        {
                            CityClub cc = cl as CityClub;
                            if(cc != null)
                            {
                                if (cc.budget < 0)
                                {
                                    //Ratio perte gains
                                    float totalIncome = cc.GetTotalIncomeOnYear(_date) + cc.GetTransfersResultOnYear(_date);
                                    float totalExpenses = cc.GetTotalExpensesOnYear(_date);
                                    float ratio = -totalExpenses / (totalIncome + 0.0f);
                                    ratio = ratio < 1 ? 1 : ratio;
                                    ratio = ratio > 2 ? 2 : ratio;
                                    ratio--;
                                    int maxPoints = cl.Country().GetSanction(SanctionType.FinancialIrregularities).maxPointsDeduction;
                                    int minPoints = cl.Country().GetSanction(SanctionType.FinancialIrregularities).minPointsDeduction;
                                    int pointsDeduction = (int)Math.Floor((maxPoints - minPoints) * ratio) + minPoints;
                                    t.AddPointsDeduction(cl, SanctionType.FinancialIrregularities, _date, pointsDeduction);
                                }
                            }
                        }
                    }

                    //End round (every year when c.periodicity == 1, else every c.periodicity years). Take care of YearOffset based on c.remainingYears who is based on tournament reset date. Additional check to avoid updating a non started tournament
                    if (c.remainingYears == (c.periodicity - t.programmation.end.YearOffset) && (this.CurrentSeason - Utils.beginningYear) >= t.programmation.end.YearOffset)
                    {
                        if (Utils.CompareDates(t.DateEndRound(), _date))
                        {
                            t.DistributeGrants();
                            t.QualifyClubs(false);
                            t.QualifyClubs(true);
                            if (!c.isChampionship)
                            {
                                //t.QualifyClubs(true);
                            }
                            else if(tc != null)
                            {
                                tc.ClearAdministrativeRetrogradationsCache();
                            }
                        }
                    }
                    if (c.remainingYears == (c.periodicity - t.programmation.initialisation.YearOffset) && (this.CurrentSeason - Utils.beginningYear) >= t.programmation.initialisation.YearOffset)
                    {
                        if (Utils.CompareDates(t.DateInitialisationRound(), _date) && c.name != Utils.friendlyTournamentName)
                        {
                            t.Setup();
                        }
                    }
                    i++;
                }

                //Case of leagues playing on more than 1 year ?
                //Not managed for now
                /*if(c.isChampionship && Utils.CompareDates(c.seasonBeginning.ConvertToDateTime().AddDays(-1), _date))
                {
                    if(c.currentRound > -1)
                    {
                        //c.QualifyClubsNextYear();
                    }
                }*/

                if (c.level == 1 && c.isChampionship && Utils.CompareDatesWithoutYear(c.seasonBeginning.ConvertToDateTime().AddDays(-1), _date))
                {
                    Country ctry = kernel.LocalisationTournament(c) as Country;
                    if (ctry != null && c.remainingYears == 1)// && ctry.CountAdministrativeRetrogradations() > 0)
                    {
                        List<Tournament> leagues = ctry.Leagues();
                        List<Club>[] clubsByLeagues = new List<Club>[leagues.Count]; //Each leagues teams
                        for (int ii = 0; ii < leagues.Count; ii++)
                        {
                            clubsByLeagues[ii] = new List<Club>(leagues[ii].nextYearQualified[0]);
                        }
                        bool isConformCurrent = ctry.CheckLeagueConformity(clubsByLeagues);
                        bool isConformWithRetrogradations = ctry.CheckLeagueConformity(ctry.GetAdministrativeRetrogradations());
                        if(!isConformCurrent)
                        {
                            Console.WriteLine("-- Leagues are not conform --");
                            valid = false;
                        }
                        if(!isConformWithRetrogradations)
                        {
                            Console.WriteLine("-- Leagues are not conform after retrogradations --");
                            valid = false;
                        }
                    }
                }

                //if (c.level == 1 && c.isChampionship && Utils.CompareDatesWithoutYear(c.seasonBeginning.ConvertToDateTime().AddDays(-2), _date))
                if (c.level == 1 && c.isChampionship && Utils.CompareDatesWithoutYear(c.seasonBeginning.ConvertToDateTime(), _date))
                {
                    Country ctry = kernel.LocalisationTournament(c) as Country;
                    if(ctry != null && c.remainingYears == 1)// && ctry.CountAdministrativeRetrogradations() > 0)
                    {
                        ctry.ApplyAdministrativeRetrogradations();
                    }
                }

                if (Utils.CompareDates(c.seasonBeginning.ConvertToDateTime(), _date))
                {
                    c.Reset();
                }

                if (options.tournamentsToExport.Contains(c))
                {
                    Exports(c);
                }
            }
            return valid;
        }

        public List<Match> NextDay()
        {
            _date = _date.AddDays(1);
            //Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("France"));

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
                Dictionary<Round, List<Match>> games = new Dictionary<Round, List<Match>>();
                foreach (Round r in c.rounds)
                {
                    foreach (Match m in r.matches)
                    {
                        if (Utils.CompareDates(m.day, _date))
                        {
                            if (!games.ContainsKey(r))
                            {
                                games.Add(r, new List<Match>());
                            }
                            games[r].Add(m);
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
                }

                foreach (KeyValuePair<Round, List<Match>> gamesByRound in games)
                {
                    SetUpMediasForTournaments(gamesByRound.Value, c, gamesByRound.Key);
                }

                foreach (Round round in c.rounds)
                {
                    GroupsRound gRound = round as GroupsRound;
                    if (gRound != null)
                    {
                        gRound.ClearCache();
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
                    foreach (Club countryClub in kernel.Clubs)
                    {
                        if(countryClub.Country() == c)
                        {
                            countryClub.SetTicketPrice();
                        }
                    }
                }

            }

            List<NationalTeam> nationalTeams = kernel.world.GetAllCountries().SelectMany(s => s.nationalTeams).ToList();
            //Check every international window
            for (int i = 0; i < kernel.world.internationalDates.Count; i++)
            {
                InternationalDates intDate = kernel.world.internationalDates[i];
                //Valid if no tournament is associated to the window or if it's a year with the tournament playing
                if (intDate.IsValid())
                {
                    List<Club> specialTournamentDate = new List<Club>();
                    DateTime startDate = intDate.start.ConvertToDateTime(intDate.StartYear(weekNumber));
                    DateTime endDate = intDate.end.ConvertToDateTime(intDate.EndYear(weekNumber));
                    //Get teams engaged with an international tournament playing at the same time of this window
                    if (intDate.tournament == null)
                    {
                        foreach (InternationalDates iDate in kernel.world.internationalDates)
                        {
                            if (!iDate.IsEquals(intDate) && iDate.tournament != null && iDate.IsValid())
                            {
                                DateTime otherStartDate = iDate.start.ConvertToDateTime(iDate.StartYear(weekNumber));
                                DateTime otherEndDate = iDate.end.ConvertToDateTime(iDate.EndYear(weekNumber));
                                if ((Utils.IsBefore(otherStartDate, startDate) && Utils.IsBefore(startDate, otherEndDate)) || (Utils.IsBefore(otherStartDate, endDate) && Utils.IsBefore(endDate, otherEndDate)))
                                {
                                    foreach (Round r in iDate.tournament.rounds)
                                    {
                                        specialTournamentDate.AddRange(r.clubs);
                                    }
                                }
                            }
                        }
                    }

                    //Players are called two weeks before int. games for classic windows. Players are called 1 month before for a specific tournament
                    int daysBefore = intDate.tournament != null ? -31 : -14;
                    if (Utils.CompareDates(intDate.start.ConvertToDateTime(intDate.StartYear(weekNumber)).AddDays(daysBefore), _date))
                    {
                        foreach (NationalTeam nt in nationalTeams)
                        {
                            Match nextGame = nt.NextGame;
                            //Call players if a game of the team is scheduled and not for a specific tournament (specific tournament has priority)
                            if (!specialTournamentDate.Contains(nt) && nextGame != null && Utils.IsBefore(nextGame.day, intDate.end.ConvertToDateTime(intDate.EndYear(weekNumber))))
                            {
                                Console.WriteLine(_date.ToShortDateString() + " [Appel] " + nt.name + (intDate.tournament != null ? " pour " + intDate.tournament.name + ", " + intDate.tournament.BeginDate().ToShortDateString() + "-" + intDate.tournament.EndDate().ToShortDateString() : ""));
                                nt.CallPlayers(kernel.GetPlayersByCountry(nt.country));
                                kernel.world.internationalDates[i] = new InternationalDates(intDate.start, intDate.end, intDate.tournament, true);
                            }
                        }
                    }
                }
                //Players join selection 2 days before the beginning of the window (or 10 days for a specific tournament like World Cup)
                int callPlayersDaysBefore = intDate.tournament != null ? -10 : -2;
                if (intDate.currentlyCalled && Utils.CompareDates(intDate.start.ConvertToDateTime(intDate.StartYear(weekNumber)).AddDays(callPlayersDaysBefore), _date))
                {
                    List<Club> tournamentClubs = intDate.tournament != null ? intDate.tournament.Clubs() : new List<Club>();
                    foreach (NationalTeam nt in nationalTeams)
                    {
                        if (intDate.tournament == null || tournamentClubs.Contains(nt))
                        {
                            nt.JoinPlayers();
                        }
                    }
                }

                //Players are freed 2 days after the end of the tournament or the window
                if (intDate.currentlyCalled && Utils.CompareDates(intDate.end.ConvertToDateTime(intDate.EndYear(weekNumber)).AddDays(2), _date))
                {
                    List<Club> tournamentClubs = intDate.tournament != null ? intDate.tournament.Clubs() : new List<Club>();
                    foreach (NationalTeam nt in nationalTeams)
                    {
                        if (intDate.tournament == null || tournamentClubs.Contains(nt))
                        {
                            nt.ReleasePlayers();
                            kernel.world.internationalDates[i] = new InternationalDates(intDate.start, intDate.end, intDate.tournament, false);
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

            // PrintClubElo();
            
            return clubMatchs;
        }

        private void PrintClubElo()
        {
            Console.WriteLine("[Clubs elo]");
            List<Club> totalClubs = new List<Club>(this.kernel.Clubs);
            totalClubs.Sort(new ClubComparator(ClubAttribute.ELO, false));
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(totalClubs[i].name + " - " + totalClubs[i].elo);
            }
            Console.WriteLine("...");
            for (int i = totalClubs.Count - 1; i > totalClubs.Count - 10; i--)
            {
                Console.WriteLine(totalClubs[i].name + " - " + totalClubs[i].elo);
            }
            float totalElo = 0;
            foreach (Club c in totalClubs)
            {
                totalElo += c.elo;
            }
            Console.WriteLine("Elo Sum = " + totalElo);
        }
    }
}