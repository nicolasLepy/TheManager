﻿using System;
using System.Collections.Generic;
using System.Linq;
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

    [DataContract(IsReference =true)]
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

        public List<City> cities { get { return _cities; } }
        public List<Stadium> stadiums { get { return _stadiums; } }
        public Language language { get => _language; }

        public List<AdministrativeDivision> administrativeDivisions => _administrativeDivisions;
        public Dictionary<Club, Tournament> administrativeRetrogradations => _administrativeRetrogradations;

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
            foreach(AdministrativeSanction admS in _administrativeSanctionsDefinitions)
            {
                if(admS.type == sanctionType)
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
                if(continentalTournament != null)
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
            foreach(Club c in clubs)
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
                for(int i = -5; i < 0; i++)
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
            while(admAtLevel != administrativeDivision)
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

            foreach(Tournament t in Tournaments())
            {
                if(t.isChampionship == isChampionship && t.level == rank)
                {
                    res = t;
                }
            }

            return res;   
            
        }
        
        public List<Tournament> Leagues()
        {
            List<Tournament> res = new List<Tournament>();
            foreach(Tournament t in Tournaments())
            {
                if(t.isChampionship)
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
            foreach(Tournament t in Tournaments())
            {
                if(!t.isChampionship && t.periodicity == 1)
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
            foreach(Tournament t in _tournaments)
            {
                if(t.isChampionship && t.level == 1)
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
            for(int i = 0; i<_tournaments.Count && !res; i++)
            {
                Tournament t = _tournaments[i];
                for(int j = 0; j < t.rounds.Count && !res; j++)
                {
                    Round r = t.rounds[j];
                    foreach(Club c in r.clubs)
                    {
                        if((c as ReserveClub) != null)
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
            foreach(Tournament t in Tournaments())
            {
                if(t.isChampionship && t.rounds[0].rules.Contains(Rule.ReservesAreNotPromoted) && t.level > level)
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
                foreach(Continent c in Session.Instance.Game.kernel.world.continents)
                {
                    foreach(Country cy in c.countries)
                    {
                        if(cy == this)
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
            int endWeek = firstDivision != null ? (firstDivision.rounds.Last().programmation.end.WeekNumber+1) : Utils.Modulo(resetWeek - 15, 52);
            if(endWeek < startWeek)
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
            if(withContinentalDates)
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
                if(t.periodicity == 1 && ( (t.isChampionship && leaguesLevel.Contains(t.level)) || t.level <= maxCupLevel))
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
                        if( (gd.MidWeekGame && !weekdays) || (!gd.MidWeekGame && !weekend))
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

    }
}