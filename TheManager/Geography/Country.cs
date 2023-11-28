using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TheManager.Comparators;
using TheManager.Tournaments;

namespace TheManager
{

    public enum SanctionType
    {
        EnteringAdministration,
        Forfeit,
        IneligiblePlayer,
        FinancialIrregularities
    }

    [DataContract]
    public struct AdministrativeSanction
    {
        [DataMember]
        private SanctionType _type;
        [DataMember]
        private int _minPointsDeduction;
        [DataMember]
        private int _maxPointsDeduction;
        [DataMember]
        private int _minRetrogradation;
        [DataMember]
        private int _maxRetrogradation;

        public SanctionType type => _type;
        public int minPointsDeduction => _minPointsDeduction;
        public int maxPointsDeduction => _maxPointsDeduction;
        public int minRetrogradation => _minRetrogradation;
        public int maxRetrogradation => _maxRetrogradation;

        public AdministrativeSanction(SanctionType type, int minPointsDeduction, int maxPointsDeduction, int minRetrogradation, int maxRetrogradation)
        {
            _type = type;
            _minPointsDeduction = minPointsDeduction;
            _maxPointsDeduction = maxPointsDeduction;
            _minRetrogradation = minRetrogradation;
            _maxRetrogradation = maxRetrogradation;
        }

    }

    [DataContract(IsReference = true)]
    public class Country : ILocalisation
    {
        [DataMember]
        private List<City> _cities;
        [DataMember]
        private List<Stadium> _stadiums;
        [DataMember]
        private Language _language;
        [DataMember]
        private List<Tournament> _tournaments;
        [DataMember]
        private string _dbName;
        [DataMember]
        private string _name;
        [DataMember]
        private int _shapeNumber;
        [DataMember]
        private List<AdministrativeDivision> _administrativeDivisions;
        [DataMember]
        private Dictionary<Club, Tournament> _administrativeRetrogradations;
        [DataMember]
        private List<float[]> _gamesTimesWeekend;
        [DataMember]
        private List<float[]> _gamesTimesWeekdays;
        [DataMember]
        private int _resetWeek;
        [DataMember]
        private List<AdministrativeSanction> _administrativeSanctionsDefinitions;

        private List<Club>[] _cacheAdministrativeRetrogradationsChanges;

        public List<City> cities { get { return _cities; } }
        public List<Stadium> stadiums { get { return _stadiums; } }
        public Language language { get => _language; }

        public List<AdministrativeDivision> administrativeDivisions => _administrativeDivisions;

        public List<float[]> gamesTimesWeekend => _gamesTimesWeekend;
        public List<float[]> gamesTimesWeekdays => _gamesTimesWeekdays;

        public int resetWeek => _resetWeek;

        public string Flag
        {
            get
            {
                string flag = _name;
                flag = flag.ToLower();
                flag = flag.Replace(" ", "");
                flag = flag.Replace("î", "i");
                flag = flag.Replace("é", "e");
                flag = flag.Replace("è", "e");
                flag = flag.Replace("ê", "e");
                flag = flag.Replace("ô", "o");
                flag = flag.Replace("ö", "o");
                flag = flag.Replace("ï", "i");
                flag = flag.Replace("ë", "e");
                flag = flag.Replace("à", "a");
                flag = flag.Replace("ä", "a");
                flag = flag.Replace("-", "");
                return flag;
            }
        }

        public string DbName { get => _dbName; }
        public int ShapeNumber { get => _shapeNumber; }

        public List<NationalTeam> nationalTeams
        {
            get
            {
                List<NationalTeam> res = new List<NationalTeam>();
                foreach (Club c in Session.Instance.Game.kernel.Clubs)
                {
                    NationalTeam nt = c as NationalTeam;
                    if (nt != null && c.Country() == this)
                    {
                        res.Add(nt);
                    }
                }
                return res;
            }
        }

        public AdministrativeSanction GetSanction(SanctionType sanctionType)
        {
            AdministrativeSanction res = default;
            foreach (AdministrativeSanction admS in _administrativeSanctionsDefinitions)
            {
                if (admS.type == sanctionType)
                {
                    res = admS;
                }
            }
            return res;
        }

        public float YearAssociationCoefficient(int nSeason)
        {
            List<Club> clubs = new List<Club>();
            float total = 0;
            for (int i = 1; i < 4; i++)
            {
                Tournament continentalTournament = Continent.GetContinentalClubTournament(i);
                if (continentalTournament != null)
                {
                    int j = continentalTournament.previousEditions.Count - (-nSeason);

                    if (j >= 0)
                    {
                        Tournament yearContinentalTournament = continentalTournament.previousEditions.ToList()[j].Value;
                        foreach (Tournament championship in _tournaments)
                        {
                            if (championship.isChampionship)
                            {
                                foreach (Club c in championship.rounds[0].clubs)
                                {
                                    if (yearContinentalTournament.IsInvolved(c))
                                    {
                                        clubs.Add(c);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (Club c in clubs)
            {
                float clubCoefficient = c.ClubYearCoefficient(nSeason, true);
                total += clubCoefficient;
            }

            return clubs.Count > 0 ? total / clubs.Count : 0;
        }

        public float AssociationCoefficient
        {
            get
            {
                float res = 0;
                for (int i = -5; i < 0; i++)
                {
                    res += YearAssociationCoefficient(i);
                }
                return res;
            }
        }

        public Country(string dbName, string name, Language language, int shapeNumber, int resetWeek, List<AdministrativeSanction> administrativeSanctionsDefinitions)
        {
            _dbName = dbName;
            _name = name;
            _language = language;
            _cities = new List<City>();
            _stadiums = new List<Stadium>();
            _tournaments = new List<Tournament>();
            _shapeNumber = shapeNumber;
            _administrativeDivisions = new List<AdministrativeDivision>();
            _administrativeRetrogradations = new Dictionary<Club, Tournament>();
            _gamesTimesWeekend = new List<float[]>();
            _gamesTimesWeekdays = new List<float[]>();
            _resetWeek = resetWeek;
            _administrativeSanctionsDefinitions = administrativeSanctionsDefinitions;
            _cacheAdministrativeRetrogradationsChanges = null;
        }

        public AdministrativeDivision GetCountryAdministrativeDivision()
        {
            AdministrativeDivision res = null;
            foreach (AdministrativeDivision ad in _administrativeDivisions)
            {
                if (ad.name == this._name)
                {
                    res = ad;
                }
            }

            return res;
        }

        public AdministrativeDivision GetAdministrativeDivision(int id)
        {
            AdministrativeDivision res = null;
            foreach (AdministrativeDivision ad in _administrativeDivisions)
            {
                if (ad.id == id)
                {
                    res = ad;
                }

                AdministrativeDivision resChild = ad.GetAdministrativeDivision(id);
                if (resChild != null)
                {
                    res = resChild;
                }
            }
            return res;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="level">Administrative division level</param>
        /// <returns></returns>
        public List<AdministrativeDivision> GetAdministrativeDivisionsLevel(int level)
        {
            List<AdministrativeDivision> res = new List<AdministrativeDivision>();
            if (level == 1)
            {
                res = _administrativeDivisions;
            }
            else
            {
                foreach (AdministrativeDivision ad in _administrativeDivisions)
                {
                    res.AddRange(ad.GetAdministrativeDivisionsLevel(level - 1));
                }
            }
            return res;
        }

        public int GetLevelOfAdministrativeDivision(AdministrativeDivision administrativeDivision)
        {
            int level = 0;
            AdministrativeDivision admAtLevel = null;
            while (admAtLevel != administrativeDivision)
            {
                admAtLevel = GetAdministrativeDivisionLevel(administrativeDivision, ++level);
            }
            return level;
        }

        public AdministrativeDivision GetAdministrativeDivisionLevel(AdministrativeDivision administrativeDivision, int level)
        {
            AdministrativeDivision res = null;
            List<AdministrativeDivision> levelAdm = GetAdministrativeDivisionsLevel(level);

            foreach (AdministrativeDivision adm in levelAdm)
            {
                if (adm == administrativeDivision || adm.ContainsAdministrativeDivision(administrativeDivision))
                {
                    res = adm;
                }
            }
            return res;
        }

        public List<Tournament> Tournaments()
        {
            return _tournaments;
        }

        private Tournament GetTournamentByLevel(int rank, bool isChampionship)
        {
            Tournament res = null;

            foreach (Tournament t in Tournaments())
            {
                if (t.isChampionship == isChampionship && t.level == rank)
                {
                    res = t;
                }
            }

            return res;

        }

        public List<Tournament> Leagues()
        {
            List<Tournament> res = new List<Tournament>();
            foreach (Tournament t in Tournaments())
            {
                if (t.isChampionship)
                {
                    res.Add(t);
                }
            }
            res.Sort((x, y) => x.level.CompareTo(y.level));
            return res;
        }

        public Tournament League(int leagueRank)
        {
            return GetTournamentByLevel(leagueRank, true);
        }

        public List<Tournament> Cups()
        {
            List<Tournament> res = new List<Tournament>();
            foreach (Tournament t in Tournaments())
            {
                if (!t.isChampionship && t.periodicity == 1)
                {
                    res.Add(t);
                }
            }
            res.Sort(new TournamentComparator());
            /*
            int i = 1;
            Tournament cup = GetTournamentByLevel(i, false);
            while(cup != null)
            {
                res.Add(cup);
                cup = GetTournamentByLevel(++i, false);
            }*/
            return res;

        }

        /**
         * cupRank : for exemple : Coupe de France is level 1 and Coupe de la Ligue is level 2
         */
        public Tournament Cup(int cupRank)
        {
            return GetTournamentByLevel(cupRank, false);
        }

        /**
         * Get last league with a national level, then league is subdivised by groups
         */
        public Tournament GetLastNationalLeague()
        {
            int res = -1;
            foreach (Tournament t in Tournaments())
            {
                GroupsRound gr = t.rounds[0] as GroupsRound;
                if (gr != null && gr.RandomDrawingMethod != RandomDrawingMethod.Administrative && t.level > res)
                {
                    res = t.level;
                }
            }

            return League(res);
        }

        public Tournament GetLastRegionalLeague(int level)
        {
            int res = -1;
            foreach (Tournament t in Tournaments())
            {
                GroupsRound gr = t.rounds[0] as GroupsRound;
                if (gr != null && gr.RandomDrawingMethod == RandomDrawingMethod.Administrative && gr.administrativeLevel == level && t.level > res)
                {
                    res = t.level;
                }
            }

            return League(res);
        }

        public Tournament FirstDivisionChampionship()
        {
            Tournament res = null;
            foreach (Tournament t in _tournaments)
            {
                if (t.isChampionship && t.level == 1)
                {
                    res = t;
                }
            }
            return res;
        }

        public Tournament GetHigherRegionalTournament(int administrativeLevel)
        {
            Tournament higherRegionalTournament = null;
            foreach (Tournament t in Tournaments())
            {
                if (t.isChampionship && (t.rounds[0] as GroupsRound) != null && (t.rounds[0] as GroupsRound).administrativeLevel == administrativeLevel && (higherRegionalTournament == null || t.level < higherRegionalTournament.level))
                {
                    higherRegionalTournament = t;
                }
            }
            return higherRegionalTournament;
        }

        public bool LeagueSystemWithReserves()
        {
            bool res = false;
            for (int i = 0; i < _tournaments.Count && !res; i++)
            {
                Tournament t = _tournaments[i];
                for (int j = 0; j < t.rounds.Count && !res; j++)
                {
                    Round r = t.rounds[j];
                    foreach (Club c in r.clubs)
                    {
                        if ((c as ReserveClub) != null)
                        {
                            res = true;
                        }
                    }
                }
            }
            return res;
        }

        public override string ToString()
        {
            return _name;
        }

        public string Name()
        {
            return _name;
        }

        public int GetLastLeagueLevelWithoutReserves()
        {
            int level = -1;
            foreach (Tournament t in Tournaments())
            {
                if (t.isChampionship && t.rounds[0].rules.Contains(Rule.ReservesAreNotPromoted) && t.level > level)
                {
                    level = t.level;
                }
            }
            return level;
        }


        public Continent Continent
        {
            get
            {
                Continent res = null;
                foreach (Continent c in Session.Instance.Game.kernel.world.continents)
                {
                    foreach (Country cy in c.countries)
                    {
                        if (cy == this)
                        {
                            res = c;
                        }
                    }
                }
                return res;
            }
        }

        public Continent GetContinent()
        {
            return Continent;
        }

        /// <summary>
        /// Get list of available dates on a calendar
        /// </summary>
        /// <param name="withContinentalDates">Remove date taken by continental dates</param>
        /// <param name="removeDateOfCupUntilLevel">Ignore dates of cup below specified level</param>
        /// <returns></returns>
        public List<GameDay> GetAvailableCalendarDates(bool withContinentalDates, int maxCupLevel, List<int> leaguesLevel, bool weekdays, bool weekend)
        {
            List<GameDay> availableDates = new List<GameDay>();
            Continent ct = GetContinent();
            Tournament firstDivision = League(1);
            int startWeek = Utils.Modulo(resetWeek + 2, 52);
            int endWeek = firstDivision != null ? (firstDivision.rounds.Last().programmation.end.WeekNumber + 1) : Utils.Modulo(resetWeek - 15, 52);
            if (endWeek < startWeek)
            {
                endWeek = 52 + endWeek;
            }
            for (int i = startWeek; i < endWeek; i++)
            {
                int week = i % 52;
                availableDates.Add(new GameDay(week, true, 0, 0));
                availableDates.Add(new GameDay(week, false, 0, 0));
            }

            List<Tournament> tournaments = new List<Tournament>();
            if (withContinentalDates)
            {
                foreach (Tournament t in ct.Tournaments())
                {
                    if (!t.isChampionship && t.periodicity == 1)
                    {
                        tournaments.Add(t);
                    }
                }
            }
            foreach (Tournament t in this.Tournaments())
            {
                if (t.periodicity == 1 && ((t.isChampionship && leaguesLevel.Contains(t.level)) || t.level <= maxCupLevel))
                {
                    tournaments.Add(t);
                }
            }
            foreach (Tournament t in tournaments)
            {
                foreach (Round r in t.rounds)
                {
                    foreach (GameDay gd in r.programmation.gamesDays)
                    {
                        if ((gd.MidWeekGame && !weekdays) || (!gd.MidWeekGame && !weekend))
                        {
                            availableDates.RemoveAll(s => s.WeekNumber == gd.WeekNumber && s.MidWeekGame == gd.MidWeekGame);
                        }
                    }
                }
            }

            availableDates.RemoveAll(s => s.WeekNumber == 51);
            availableDates.RemoveAll(s => s.WeekNumber == 52); //A mid-week game on the last year week lead to the next year
            availableDates.RemoveAll(s => s.WeekNumber == 0); //Bug 2027

            return availableDates;
        }

        /// <summary>
        /// Get the bottom league level that a team of an association can reach
        /// </summary>
        /// <param name="division"></param>
        /// <returns>League level (not an index !, start at 1) </returns>
        public int MaxLeagueLevelWithAdministrativeDivision(AdministrativeDivision division)
        {
            List<Tournament> leagues = Leagues();
            int i = GetLastNationalLeague().level;
            bool leagueWithoutTeams = false;
            int associationLevel = 0;
            while(!leagueWithoutTeams && i < leagues.Count)
            {
                Round round = leagues[i].rounds[0];
                GroupsRound groupRound = round as GroupsRound;
                if(groupRound != null)
                {
                    associationLevel = groupRound.administrativeLevel;
                }
                AdministrativeDivision divisionLevel = GetAdministrativeDivisionLevel(division, associationLevel);

                leagueWithoutTeams = true;
                //Division level can be null. Ex : Corse have only 1 association level unlike other associations
                if(divisionLevel != null)
                {
                    foreach (Club c in round.clubs)
                    {
                        if (divisionLevel.ContainsAdministrativeDivision(c.AdministrativeDivision()))
                        {
                            leagueWithoutTeams = false;
                        }
                    }
                }
                if(!leagueWithoutTeams)
                {
                    i++;
                }
            }
            return i;
        }

        /// <summary>
        /// Preprocess administrative retrogradation by appliying changes to reserves due to retrogradation of fannion teams
        /// </summary>
        public Dictionary<Club, Tournament> PreprocessAdministrativeRetrogradation(Club club, Tournament retrogradationTournament, List<Tournament> leagues, List<Club>[] clubsByLeagues, List<int> administrativeLevels, bool bankruptcy)
        {
            KeyValuePair<Club, Tournament> retrogradation = new KeyValuePair<Club, Tournament>(club, retrogradationTournament);
            Tournament target = retrogradationTournament;
            Dictionary<Club, Tournament> newRetrogradations = new Dictionary<Club, Tournament>();
            if(retrogradationTournament != null)
            {
                newRetrogradations.Add(retrogradation.Key, retrogradation.Value);
            }
            else
            {
                target = leagues[GetClubLevelInLeaguesHierarchy(club, clubsByLeagues)];
            }

            int clubLevel = -1;
            int targetIndex = target.level - 1;
            int maxLeagueLevelForThisAssociation = MaxLeagueLevelWithAdministrativeDivision(club.AdministrativeDivision());
            int targetIndexForReserves = Math.Min(targetIndex + 1, maxLeagueLevelForThisAssociation - 1);
            for (int i = 0; i < clubsByLeagues.Length && clubLevel == -1; i++)
            {
                clubLevel = clubsByLeagues[i].Contains(club) ? i : clubLevel;
            }

            //Case bankruptcy : Fannion team is "deleted". B teams become A team (by retrogradation), C teams become B team (by retrogradation) ...
            //Last reserve is sent to bottom division available to the administrative division of the club
            //
            //Case not bankruptcy : Just throw the fannion team at the defined league position. Check to retrograde reserves to avoid having reserves higher than fanion team on the league structure

            CityClub cClub = club as CityClub;
            if(bankruptcy && cClub != null && cClub.reserves.Count > 0)
            {
                for (int i = 0; i < cClub.reserves.Count; i++)
                {
                    ReserveClub reserve = cClub.reserves[i];
                    if (i == cClub.reserves.Count-1)
                    {
                        newRetrogradations.Add(reserve, leagues[maxLeagueLevelForThisAssociation - 1]);
                    }
                    else
                    {
                        int levelReserve = -1;
                        int levelNextReserve = -1;
                        for(int j = 0; j < clubsByLeagues.Length; j++)
                        {
                            levelReserve = clubsByLeagues[j].Contains(cClub.reserves[i]) ? j : levelReserve;
                            levelNextReserve = clubsByLeagues[j].Contains(cClub.reserves[i+1]) ? j : levelNextReserve;
                        }
                        if(levelReserve > -1 && levelNextReserve > -1)
                        {
                            newRetrogradations.Add(reserve, leagues[levelNextReserve]);
                        }
                    }
                }
            }
            else
            {
                List<ReserveClub> processedReserves = new List<ReserveClub>();
                for (int i = clubLevel; i <= targetIndex; i++)
                {
                    foreach (Club c in clubsByLeagues[i])
                    {
                        ReserveClub rc = c as ReserveClub;
                        if (rc != null && rc.FannionClub == club && targetIndexForReserves > i && !processedReserves.Contains(rc))
                        {
                            newRetrogradations.Add(rc, leagues[targetIndexForReserves]);
                            processedReserves.Add(rc);
                            Console.WriteLine("[Rétrograde] " + rc.name + " : " + leagues[i].name + " -> " + leagues[targetIndexForReserves].name);
                            targetIndex = targetIndexForReserves;
                            targetIndexForReserves = Math.Min(targetIndexForReserves + 1, maxLeagueLevelForThisAssociation - 1);
                        }
                    }
                }
            }

            return newRetrogradations;
        }

        public void AddAdministrativeRetrogradation(Club club, Tournament target)
        {
            if(_administrativeRetrogradations.ContainsKey(club))
            {
                _administrativeRetrogradations.Remove(club);
            }
            _administrativeRetrogradations.Add(club, target);
            ClearAdministrativeRetrogradationsCache();
            _cacheAdministrativeRetrogradationsChanges = null;
        }

        public int CountAdministrativeRetrogradations()
        {
            return _administrativeRetrogradations.Count;
        }

        public Dictionary<Club, Tournament> GetRetrogradations()
        {
            return _administrativeRetrogradations;
        }

        public void ClearAdministrativeRetrogradationsCache()
        {
            _cacheAdministrativeRetrogradationsChanges = null;
        }


        /// <summary>
        /// Just before resetting leagues, update NextYearQualified by applying administrative retrogradations
        /// </summary>
        public void ApplyAdministrativeRetrogradations()
        {
            Console.WriteLine("[ApplyAdministrativeRetrogradations] " + this.Name());

            List<Tournament> leagues = Leagues();
            List<Club>[] clubsByLeagues = GetAdministrativeRetrogradations();

            for (int i = 0; i < leagues.Count; i++)
            {
                leagues[i].nextYearQualified[0].Clear();
                leagues[i].nextYearQualified[0].AddRange(clubsByLeagues[i]);
            }

            _administrativeRetrogradations.Clear();
            ClearAdministrativeRetrogradationsCache();
        }

        public List<Club>[] GetAdministrativeRetrogradations()
        {
            if (_cacheAdministrativeRetrogradationsChanges != null)
            {
                return _cacheAdministrativeRetrogradationsChanges;
            }
            List<Tournament> leagues = Leagues();
            List<Club>[] clubsByLeagues = new List<Club>[leagues.Count]; //Each leagues teams
            List<Club> clubsCantBeSaved = new List<Club>(); //Clubs that can't be saved from relegation (bottom teams of each league if this rule is activated)
            List<int> administrativeLevels = new List<int>(); //Each leagues administrative level
            for (int i = 0; i < leagues.Count; i++)
            {
                clubsByLeagues[i] = new List<Club>(leagues[i].nextYearQualified[0]);
                ClubComparator comparator = new ClubComparator(ClubAttribute.CURRENT_RANKING, false);
                clubsByLeagues[i].Sort(comparator);
                Console.WriteLine(clubsByLeagues[i].Count);

                //Replace raw ranking by playoff order
                List<KeyValuePair<Club, int>> promotionPlayOffs = leagues[i].GetTopPlayOffClubs();
                promotionPlayOffs.Reverse();
                promotionPlayOffs.Sort(new ClubPlayoffsComparator(new List<Club>(clubsByLeagues[i])));
                foreach(KeyValuePair<Club, int> kvpC in promotionPlayOffs)
                {
                    Club c = kvpC.Key;
                    if (clubsByLeagues[i].Contains(c))
                    {
                        clubsByLeagues[i].Remove(c);
                        if (c.Championship.level < leagues[i].level)
                        {
                            clubsByLeagues[i].Insert(0, c);
                        }
                        else
                        {
                            int j = 0;
                            while (clubsByLeagues[i][j].Championship.level < leagues[i].level)
                            {
                                j++;
                            }
                            clubsByLeagues[i].Insert(j, c);
                        }
                    }
                }
                

                Console.WriteLine("=====" + leagues[i].name + "===== " + clubsByLeagues[i].Count);
                foreach(KeyValuePair<Club, int> c in promotionPlayOffs)
                {
                    Console.WriteLine("[playoffs] " + c.Key.name);
                }
                foreach (Club c in clubsByLeagues[i])
                {
                    Round clubC = (from Tournament t in Leagues() where t.rounds.Count > 0 && t.rounds[0].clubs.Contains(c) select t.rounds[0]).FirstOrDefault();
                    string adm = (leagues[i].rounds[0] as GroupsRound != null && (leagues[i].rounds[0] as GroupsRound).administrativeLevel > 0) ? "["+GetAdministrativeDivisionLevel(c.AdministrativeDivision(), (leagues[i].rounds[0] as GroupsRound).administrativeLevel).name +"] " : "";
                    Console.WriteLine(adm + c.Championship.name + " - " + comparator.GetRanking(clubC, c) + ". " + c.name);
                }
                int administrativeLevel = 0;
                if (leagues[i].rounds.Count > 0)
                {
                    Round firstRound = leagues[i].rounds[0];
                    if(firstRound as ChampionshipRound != null)
                    {
                        administrativeLevel = 0;
                        if (firstRound.rules.Contains(Rule.BottomTeamNotEligibleForRepechage))
                        {
                            Console.WriteLine("[Last Not Saved] Add " + (firstRound as ChampionshipRound).Ranking().Last());
                            clubsCantBeSaved.Add((firstRound as ChampionshipRound).Ranking().Last());
                        }
                    }
                    else if(firstRound as GroupsRound != null) //KEEP
                    {
                        GroupsRound gFirstRound = firstRound as GroupsRound;
                        administrativeLevel = gFirstRound.administrativeLevel;
                        if (firstRound.rules.Contains(Rule.BottomTeamNotEligibleForRepechage))
                        {
                            for(int g = 0; g < gFirstRound.groupsCount; g++)
                            {
                                List<Club> gRanking = gFirstRound.Ranking(g);
                                if(gRanking.Count > 0)
                                {
                                    Console.WriteLine("[Last Not Saved] Add " + gRanking.Last());
                                    clubsCantBeSaved.Add(gRanking.Last());
                                }
                            }
                        }
                    }
                }
                administrativeLevels.Add(administrativeLevel);
            }

            List<Club> allClubs = new List<Club>();
            foreach(List<Club> allClubsLevel in clubsByLeagues)
            {
                foreach(Club allClubLevel in allClubsLevel)
                {
                    if((allClubLevel as CityClub) != null)
                    {
                        allClubs.Add(allClubLevel);
                    }
                }
            }

            //For each retrogradation, check if this leads to other relegations (reserves). Iterate through these relegations
            foreach(Club currentClub in allClubs)
            //foreach (KeyValuePair<Club, Tournament> mainRetrogradation in _administrativeRetrogradations)
            {
                bool isBankruptcy = false;
                Tournament tournamentRetrogradation = _administrativeRetrogradations.ContainsKey(currentClub) ? _administrativeRetrogradations[currentClub] : null;
                Dictionary<Club, Tournament> clubRetrogradations = PreprocessAdministrativeRetrogradation(currentClub, tournamentRetrogradation, leagues, clubsByLeagues, administrativeLevels, isBankruptcy);

                foreach (KeyValuePair<Club, Tournament> retrogradation in clubRetrogradations)
                {
                    Club club = retrogradation.Key;
                    int clubLevel = -1;
                    for (int i = 0; i < clubsByLeagues.Length && clubLevel == -1; i++)
                    {
                        List<Club> clubs = clubsByLeagues[i];
                        if (clubs.Contains(club))
                        {
                            clubLevel = i;
                        }
                    }

                    int targetLeagueIndex = leagues.IndexOf(retrogradation.Value);
                    clubsByLeagues[targetLeagueIndex].Add(club);
                    clubsByLeagues[clubLevel].Remove(club);
                    Console.WriteLine("[Administrative Retrogradation] " + club.name + " (" + leagues[clubLevel].name + " -> " + leagues[targetLeagueIndex].name + ")");

                    //On remonte un club qui respecte les règles de chaque division
                    for (int i = targetLeagueIndex; i > clubLevel; i--)
                    {
                        Console.WriteLine("_____________________________");
                        Round round = leagues[i].rounds[0];
                        int administrativeLevel = administrativeLevels[i];
                        int j = 0;
                        bool found = false;
                        //Repechage candidates : filtering by association if necessary
                        List<Club> candidates = administrativeLevel == 0 ? clubsByLeagues[i] : FilterAssociation(clubsByLeagues[i], GetAdministrativeDivisionLevel(club.AdministrativeDivision(), administrativeLevel));
                        if (administrativeLevel > 0)
                        {
                            Console.WriteLine("[Remonte un tour régional] " + GetAdministrativeDivisionLevel(club.AdministrativeDivision(), administrativeLevel).name);
                        }
                        while (!found && j < candidates.Count)
                        {
                            Club candidate = candidates[j];
                            ReserveClub candidateAsReserve = candidate as ReserveClub;
                            //TODO: Rules check (doublon ?)
                            if (!clubsCantBeSaved.Contains(candidate) && ((candidateAsReserve == null) || (!round.rules.Contains(Rule.ReservesAreNotPromoted) && !ContainsTeamOfClub(clubsByLeagues[i - 1], candidateAsReserve.FannionClub))))
                            {
                                found = true;
                                clubsByLeagues[i].Remove(candidate);
                                clubsByLeagues[i - 1].Add(candidate);
                                Console.WriteLine("[Repêchage] " + candidate.name + " (" + leagues[i].name + " -> " + leagues[i - 1].name + ")");
                            }
                            else
                            {
                                Console.WriteLine("[Impossible de repêcher] " + candidate.name + "(" + leagues[i].name + ")");
                            }
                            j++;
                        }
                    }
                }
            }
            _cacheAdministrativeRetrogradationsChanges = clubsByLeagues;
            return clubsByLeagues;
        }

        /// <summary>
        /// Return true if a team of a specific club is inside a list
        /// </summary>
        /// <param name="clubs"></param>
        /// <param name="club"></param>
        /// <returns></returns>
        private bool ContainsTeamOfClub(List<Club> clubs, Club club)
        {
            bool res = false;
            foreach(Club c in clubs)
            {
                if(c == club || ((c as ReserveClub != null) && (c as ReserveClub).FannionClub == club))
                {
                    res = true;
                }
            }
            return res;
        }

        /// <summary>
        /// Returns clubs of a particuliar association from a list of clubs
        /// </summary>
        /// <param name="clubs"></param>
        /// <param name="administrativeDivision"></param>
        /// <returns></returns>
        public List<Club> FilterAssociation(List<Club> clubs, AdministrativeDivision administrativeDivision)
        {
            List<Club> clubsAssociation = new List<Club>();
            foreach(Club c in clubs)
            {
                if(administrativeDivision.ContainsAdministrativeDivision(c.AdministrativeDivision()))
                {
                    clubsAssociation.Add(c);
                }
            }
            return clubsAssociation;
        }

        public int GetClubLevelInLeaguesHierarchy(Club club, List<Club>[] leagues)
        {
            int res = -1;
            for(int i = 0; i < leagues.Length && res == -1; i++)
            {
                res = leagues[i].Contains(club) ? i : res;
            }
            return res;
        }

        /// <summary>
        /// Never used
        /// Switch clubs between two leagues before the beginning of the season. Modify NextYearQualified. Used when checking administratives retrogradations.
        /// </summary>
        public void SwitchClubsOfLeagueForNextYear(Club c1, Club c2)
        {
            List<Tournament> leagues = this.Leagues();
            Tournament tournament1 = null;
            Tournament tournament2 = null;
            foreach(Tournament league in leagues)
            {
                if(league.nextYearQualified.Length > 0 && league.nextYearQualified[0].Contains(c1))
                {
                    tournament1 = league;
                }
                if (league.nextYearQualified.Length > 0 && league.nextYearQualified[0].Contains(c2))
                {
                    tournament2 = league;
                }
            }
            tournament1.nextYearQualified[0].Add(c2);
            tournament2.nextYearQualified[0].Add(c1);
            tournament1.nextYearQualified[0].Remove(c2);
            tournament2.nextYearQualified[0].Remove(c1);
        }
    }
}