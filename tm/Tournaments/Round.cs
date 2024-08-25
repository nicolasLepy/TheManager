using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using tm.Comparators;
using tm.Tournaments;

namespace tm
{

    public enum Rule
    {
        AtHomeIfTwoLevelDifference,
        OnlyFirstTeams,
        ReservesAreNotPromoted,
        OneClubByCountryInGroup,
        OneTeamByContinentInGroup,
        HostedByOneCountry,
        UltramarineTeamsPlayHomeOrAway,
        UltramarineTeamsPlayAway,
        UltramarineTeamsCantCompeteAgainst,
        BottomTeamNotEligibleForRepechage
    }

    public enum RankingType
    {
        General,
        Home,
        Away
        
    }

    public enum RecuperationMethod
    {
        Randomly,
        Best,
        Worst,
        QualifiedForInternationalCompetition,
        NotQualifiedForInternationalCompetitionWorst,
        NotQualifiedForInternationalCompetitionBest,
        StatusPro
    }

    public enum Tiebreaker
    {
        GoalDifference,
        GoalFor,
        GoalAgainst,
        HeadToHead,
        Discipline
    }

    [DataContract]
    public struct RecoverTeams : IEquatable<RecoverTeams>
    {
        [DataMember]
        public IRecoverableTeams Source { get; set; }
        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public RecuperationMethod Method { get; set; }
        public RecoverTeams(IRecoverableTeams source, int number, RecuperationMethod method)
        {
            Source = source;
            Number = number;
            Method = method;
        }

        public bool Equals(RecoverTeams other)
        {
            return this.Source == other.Source && this.Number == other.Number && this.Method == other.Method;
        }
    }

    [DataContract(IsReference =true)]
    public class RoundProgrammation
    {
        [DataMember]
        private Hour _defaultHour;
        [DataMember]
        private List<GameDay> _gamesDays;
        [DataMember]
        private List<TvOffset> _tvScheduling;
        [DataMember]
        private GameDay _initialisation;
        [DataMember]
        private GameDay _end;
        [DataMember]
        private int _lastMatchDaysSameDayNumber;
        [DataMember]
        private int _gamesPriority;

        public Hour defaultHour { get => _defaultHour; }
        public List<GameDay> gamesDays { get => _gamesDays; }
        public List<TvOffset> tvScheduling { get => _tvScheduling; }
        public GameDay initialisation { get => _initialisation; }
        public GameDay end { get => _end; }
        public int lastMatchDaysSameDayNumber { get => _lastMatchDaysSameDayNumber; }
        public int gamesPriority => _gamesPriority;

        public RoundProgrammation()
        {
            _gamesDays = new List<GameDay>();
        }

        public RoundProgrammation(Hour hour, List<GameDay> days, List<TvOffset> tvSchedule, GameDay initialisation, GameDay end, int lastDaySameDay, int gamesPriority)
        {
            _defaultHour = hour;
            _gamesDays = new List<GameDay>(days);
            _tvScheduling = tvSchedule;
            _initialisation = initialisation;
            _end = end;
            _lastMatchDaysSameDayNumber = lastDaySameDay;
            _gamesPriority = gamesPriority;
        }
    }

    [DataContract]
    public struct TvOffset : IEquatable<TvOffset>
    {
        [DataMember]
        public int DaysOffset { get; set; }
        [DataMember]
        public Hour Hour { get; set; }
        [DataMember]
        public int Probability { get; set; }
        /// <summary>
        /// "La journée"
        /// </summary>
        [DataMember]
        public int GameDay { get; set; }

        [DataMember]
        public bool isPrimeTime { get; set; }

        public TvOffset(int daysOffset, Hour hour, int probability, int gameDay, bool primeTime)
        {
            DaysOffset = daysOffset;
            Hour = hour;
            Probability = probability;
            GameDay = gameDay;
            isPrimeTime = primeTime;
        }

        public bool Equals(TvOffset other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract]
    public struct Qualification : IEquatable<Qualification>
    {
        [DataMember]
        public int ranking { get; set; }
        [DataMember]
        public int roundId { get; set; }
        [DataMember]
        public Tournament tournament { get; set; }
        [DataMember]
        public bool isNextYear { get; set; }
        [DataMember]
        public int qualifies { get; set; }

        public Qualification(int Ranking, int RoundId, Tournament Tournament, bool nextYear, int Qualifies)
        {
            ranking = Ranking;
            roundId = RoundId;
            tournament = Tournament;
            isNextYear = nextYear;
            qualifies = Qualifies;
        }

        public bool Equals(Qualification other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract]
    public struct Prize : IEquatable<Prize>
    {
        [DataMember]
        public int Ranking { get; set; }
        [DataMember]
        public int Amount { get; set; }

        public Prize(int ranking, int amount)
        {
            Ranking = ranking;
            Amount = amount;
        }

        public bool Equals(Prize other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract]
    public struct PointDeduction
    {
        [DataMember]
        private SanctionType _sanctionType;

        [DataMember]
        private DateTime _date;

        [DataMember]
        private int _points;

        public SanctionType sanctionType => _sanctionType;

        public DateTime date => _date;

        public int points => _points;

        public PointDeduction(SanctionType sanction, DateTime date, int points)
        {
            _sanctionType = sanction;
            _date = date;
            _points = points;
        }
    }

    [DataContract(IsReference =true)]
    [KnownType(typeof(ChampionshipRound))]
    [System.Xml.Serialization.XmlInclude(typeof(ChampionshipRound))]
    [KnownType(typeof(KnockoutRound))]
    [System.Xml.Serialization.XmlInclude(typeof(KnockoutRound))]
    [KnownType(typeof(GroupsRound))]
    [System.Xml.Serialization.XmlInclude(typeof(GroupsRound))]
    public abstract class Round : IRecoverableTeams
    {
        /// <summary>
        /// Round ID
        /// </summary>
        [DataMember]
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Nom du tour
        /// </summary>
        [DataMember]
        protected string _name;
        /// <summary>
        /// Compétition à laquelle ce tour appartient
        /// </summary>
        [DataMember]
        protected Tournament _tournament;

        /// <summary>
        /// Liste des clubs participant à ce tour
        /// </summary>
        [DataMember]
        protected List<Club> _clubs;
        /// <summary>
        /// Liste des matchs du tour
        /// </summary>
        [DataMember]
        protected List<Match> _matches;
        /// <summary>
        /// Legs count during round
        /// </summary>
        [DataMember]
        protected int _phases;

        /// <summary>
        /// Concern general scheduling of matches of the round (TV, hour, days)
        /// </summary>
        [DataMember]
        protected RoundProgrammation _programmation;

        [DataMember]
        protected List<Qualification> _qualifications;

        /// <summary>
        /// List of teams got from other tournament still in progress
        /// </summary>
        [DataMember]
        protected List<RecoverTeams> _recuperedTeams;

        /// <summary>
        /// Initial list of teams get from other tournament still in progress. Stored for references for national cups
        /// </summary>
        [DataMember]
        protected List<RecoverTeams> _baseRecuperedTeams;

        /// <summary>
        /// Rules concerning the round
        /// Example : the team receiving if she evolves two divisions lower
        /// </summary>
        [DataMember]
        protected List<Rule> _rules;

        /// <summary>
        /// Rules to decide how to rank teams with the same number of points
        /// </summary>
        [DataMember]
        protected List<Tiebreaker> _tiebreakers;

        /// <summary>
        /// List of prizes given to the clubs at the end of the round
        /// </summary>
        [DataMember]
        protected List<Prize> _prizes;

        [DataMember]
        protected int _keepRankingFromPreviousRound;

        /// <summary>
        /// Qualified
        /// Used for tournaments like French Cup where every regions sent a defined number of teams at the seventh round.
        /// </summary>
        [DataMember]
        protected Dictionary<Association, int> _teamsByAssociation;

        /// <summary>
        /// Store every points deductions for clubs
        /// </summary>
        [DataMember]
        protected Dictionary<Club, List<PointDeduction>> _pointsDeduction;

        public string name { get => _name; }
        public List<Club> clubs { get => _clubs; }
        public List<Match> matches { get => _matches; }
        public int phases { get => _phases; }
        public int keepRankingFromPreviousRound { get => _keepRankingFromPreviousRound; }
        public RoundProgrammation programmation { get => _programmation; }
        public List<Qualification> qualifications { get => _qualifications; }
        public List<RecoverTeams> recuperedTeams { get => _recuperedTeams; }
        public List<RecoverTeams> baseRecuperedTeams { get => _baseRecuperedTeams; }
        public List<Rule> rules { get => _rules; }
        public List<Tiebreaker> tiebreakers { get => _tiebreakers; }
        public List<Prize> prizes { get => _prizes; }
        public Dictionary<Association, int> teamsByAssociation => _teamsByAssociation;
        public Dictionary<Club, List<PointDeduction>> pointsDeduction => _pointsDeduction;

        public Tournament Tournament { get => _tournament; set => _tournament = value; }
        /*{
            get
            {
                Tournament tournament = null;

                foreach(Tournament c in Session.Instance.Game.kernel.Competitions)
                {
                    foreach(Round t in c.rounds)
                    {
                        if(t == this)
                        {
                            tournament = c;
                        }
                    }
                }

                //If tournament is null we search in the archives
                if(tournament == null)
                {
                    foreach(Tournament c in Session.Instance.Game.kernel.Competitions)
                    {
                        foreach(KeyValuePair<int,Tournament> t in c.previousEditions)
                        {
                            if (t.Value.rounds.Contains(this))
                            {
                                tournament = t.Value;
                            }
                        }
                    }
                }

                return tournament;
            }
        }*/

        protected Round()
        {
            _clubs = new List<Club>();
            _matches = new List<Match>();
            _qualifications = new List<Qualification>();
            _recuperedTeams = new List<RecoverTeams>();
            _baseRecuperedTeams = new List<RecoverTeams>();
            _rules = new List<Rule>();
            _tiebreakers = new List<Tiebreaker>();
            _prizes = new List<Prize>();
            _teamsByAssociation = new Dictionary<Association, int>();
            _pointsDeduction = new Dictionary<Club, List<PointDeduction>>();
        }

        protected Round(int id, string name, Tournament tournament, Hour hour, List<GameDay> dates, List<TvOffset> tvOffsets, GameDay initialisation, GameDay end, int phases, int lastDaysSameDay, int keepRankingFromPreviousRound, int gamesPriority)
        {
            Id = id;
            _name = name;
            _tournament = tournament;
            _clubs = new List<Club>();
            _matches = new List<Match>();
            _programmation = new RoundProgrammation(hour, dates, tvOffsets, initialisation, end, lastDaysSameDay, gamesPriority);
            _qualifications = new List<Qualification>();
            _recuperedTeams = new List<RecoverTeams>();
            _baseRecuperedTeams = new List<RecoverTeams>();
            _rules = new List<Rule>();
            _tiebreakers = new List<Tiebreaker>();
            _prizes = new List<Prize>();
            _phases = phases;
            _keepRankingFromPreviousRound = keepRankingFromPreviousRound;
            _teamsByAssociation = new Dictionary<Association, int>();
            _pointsDeduction = new Dictionary<Club, List<PointDeduction>>();
        }

        public void AddPointsDeduction(Club c, SanctionType sanction, DateTime date, int points)
        {
            if(!_pointsDeduction.ContainsKey(c))
            {
                _pointsDeduction.Add(c, new List<PointDeduction>());
            }
            _pointsDeduction[c].Add(new PointDeduction(sanction, date, points));
        }

        public int GetPointsDeduction(Club c)
        {
            int points = 0;
            if(_pointsDeduction.ContainsKey(c))
            {
                foreach(PointDeduction entry in _pointsDeduction[c])
                {
                    points += entry.points;
                }
            }
            if (_keepRankingFromPreviousRound > -1)
            {
                points += Tournament.rounds[_keepRankingFromPreviousRound].GetPointsDeduction(c);
            }
            return points;
        }

        public DateTime DateInitialisationRound()
        {
            int year = Session.Instance.Game.date.Year;
            if (_programmation.initialisation.WeekNumber < Tournament.seasonBeginning.WeekNumber)
            {
                year = year + 1;
            }
            if(Utils.IsBeforeWithoutYear(Session.Instance.Game.date, Tournament.seasonBeginning.ConvertToDateTime()))
            {
                year = year - 1;
            }
            //Yearoffset : see DateEndRound()
            return _programmation.initialisation.ConvertToDateTime(year - _programmation.initialisation.YearOffset).AddDays(-1);
        }

        public DateTime DateEndRound()
        {
            int year = Session.Instance.Game.date.Year;
            if (_programmation.end.WeekNumber < Tournament.seasonBeginning.WeekNumber)
            {
                year = year + 1;
            }
            if (Utils.IsBeforeWithoutYear(Session.Instance.Game.date, Tournament.seasonBeginning.ConvertToDateTime()))
            {
                year = year - 1;
            }
            //Year offset is retranched because it is taken in account on ConvertToDateTime, to have a date corresponding to this year and not to the year + yearOffset
            //yearOffset is ignored because DateEndRound() is called to end a round whe a check taking in account yearOffset is already performed (so we want the date of this year and not year+yearOffset).
            return _programmation.end.ConvertToDateTime(year - _programmation.end.YearOffset).AddDays(1);
        }

        /// <summary>
        /// Give the number of first team in the round
        /// </summary>
        /// <returns></returns>
        public int CountWithoutReserves()
        {
            int total = 0;
            foreach(Club c in _clubs)
            {
                if(c as ReserveClub == null)
                {
                    total++;
                }
            }
            return total;
        }

        /// <summary>
        /// Give the number of first team in the round of a specific association
        /// </summary>
        /// <returns></returns>
        public int CountWithoutReserves(Association association)
        {
            int total = 0;
            foreach (Club c in _clubs)
            {
                if (c as ReserveClub == null && association.ContainsAssociation(c.Association()))
                {
                    total++;
                }
            }
            return total;
        }

        public List<Club> GetClubsAssociation(Association adm)
        {
            List<Club> res = new List<Club>();
            foreach(Club c in clubs)
            {
                if (c.Association() == adm || adm.ContainsAssociation(c.Association()))
                {
                    res.Add(c);
                }
            }
            return res;
        }
        
        protected void CheckConflicts()
        {
            foreach(Match match in _matches)
            {
                match.CheckConflicts();
            }
        }
        public float GoalsAverage()
        {
            float res = 0;
            foreach(Match m in _matches)
            {
                res += m.score1 + m.score2;
            }
            return res / ( _matches.Count+0.0f);
        }

        /// <summary>
        /// List of goalscorer by decreasing order
        /// </summary>
        /// <returns>List of KeyValuePair with player in key and his goals in value</returns>
        public List<KeyValuePair<Player, int>> GoalScorers()
        {
            Dictionary<Player, int> goalscorers = new Dictionary<Player, int>();
            foreach(Match m in _matches)
            {
                foreach(MatchEvent em in m.events)
                {
                    if(em.type == GameEvent.Goal || em.type == GameEvent.PenaltyGoal)
                    {
                        if (goalscorers.ContainsKey(em.player))
                        {
                            goalscorers[em.player]++;
                        }
                        else
                        {
                            goalscorers[em.player] = 1;
                        }
                    }
                }
            }

            List<KeyValuePair<Player, int>> list = goalscorers.ToList();

            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            return list;
        }

        /// <summary>
        /// Get a list of matches with a specific date
        /// </summary>
        /// <param name="date">The date of matches to get</param>
        /// <returns>A list of matches</returns>
        public List<Match> GetMatchesByDate(DateTime date)
        {
            List<Match> res = new List<Match>();
            foreach(Match m in _matches)
            {
                if(Utils.CompareDates(date, m.day))
                {
                    res.Add(m);
                }
            }
            return res;
        }

        /// <summary>
        /// Return the date of the next matches day
        /// If all the matches were played, the date of the last match is returned
        /// Return 01/01/2000 if there is no matches planned for the round
        /// </summary>
        /// <returns></returns>
        public DateTime NextMatchesDate()
        {
            DateTime res = new DateTime(2000,1,1);
            List<Match> nextMatches = NextMatches();
            if (nextMatches.Count > 0)
            {
                res = nextMatches[0].day;
            }
            else if (matches.Count > 0)
            {
                res = matches[matches.Count - 1].day;
            }
            return res;
        }

        /// <summary>
        /// WARNING : Performance
        /// </summary>
        /// <returns></returns>
        public int NextMatchesGameDay()
        {
            int res = -1;
            List<Match> nextMatches = NextMatches();
            if (nextMatches.Count > 0)
            {
                res = -1;
                int i = 1;
                int matchDayNumber = MatchesDayNumber();
                while (res == -1 && i <= matchDayNumber)
                {
                    if (GamesDay(i).Contains(nextMatches[0]))
                    {
                        res = i;
                    }
                    i++;
                }
            }
            else if (matches.Count > 0)
            {
                res = MatchesDayNumber();
            }
            return res;
        }

        /// <summary>
        /// Return list to next matches to be played according to the date
        /// </summary>
        /// <returns></returns>
        public List<Match> NextMatches()
        {
            List<Match> res = new List<Match>();
            bool pursue = true;
            DateTime date = new DateTime(2000, 1, 1);
            int i = 0;
            if (_matches.Count == 0)
            {
                pursue = false;
            }
            while (pursue)
            {
                Match m = _matches[i];
                if (!m.Played)
                {
                    if (date.Year == 2000)
                    {
                        date = m.day;
                        res.Add(m);
                    }
                    else if (date.Date == m.day.Date)
                    {
                        res.Add(m);
                    }
                    else
                    {
                        pursue = false;
                    }
                }

                if (i == _matches.Count - 1)
                {
                    pursue = false;
                }
                i++;
            }


            return res;
        }

        private List<Match> Matches(Club c, RankingType matchType)
        {
            List<Match> res = new List<Match>();
            foreach(Match m in _matches)
            {
                if( (m.home == c && matchType != RankingType.Away) || (m.away == c && matchType != RankingType.Home))
                {
                    res.Add(m);
                }
            }
            return res;
        }

        private List<Match> MatchesRanking()
        {
            List<Match> matchs = new List<Match>(_matches);
            if (this.keepRankingFromPreviousRound > -1)
            {
                matchs.AddRange(this.Tournament.rounds[this.keepRankingFromPreviousRound].matches);
            }
            return matchs;
        }

        public int Points(Club c, RankingType rankingType = RankingType.General)
        {
            return Utils.Points(MatchesRanking(), c, rankingType) - (rankingType == RankingType.General ? GetPointsDeduction(c) : 0);
        }

        public int Played(Club c, RankingType rankingType = RankingType.General)
        {
            return Utils.Played(MatchesRanking(), c, rankingType);
        }

        public int Wins(Club c, RankingType rankingType = RankingType.General)
        {
            return Utils.Wins(MatchesRanking(), c, rankingType);
        }

        public int Draws(Club c, RankingType rankingType = RankingType.General)
        {
            return Utils.Draws(MatchesRanking(), c, rankingType);
        }

        public int Loses(Club c, RankingType rankingType = RankingType.General)
        {
            return Utils.Loses(MatchesRanking(), c, rankingType);
        }

        public int GoalsFor(Club c, RankingType rankingType = RankingType.General)
        {
            return Utils.Gf(MatchesRanking(), c, rankingType);
        }

        public int GoalsAgainst(Club c, RankingType rankingType = RankingType.General)
        {
            return Utils.Ga(MatchesRanking(), c, rankingType);
        }

        public int Difference(Club c, RankingType rankingType = RankingType.General)
        {
            return Utils.Difference(MatchesRanking(), c, rankingType);
        }

        public void Reset()
        {
            _matches = new List<Match>();
            _clubs = new List<Club>();
            _pointsDeduction = new Dictionary<Club, List<PointDeduction>>();
        }

        /// <summary>
        /// Add clubs to get from other tournaments
        /// Ex : last64 CDF -> L1
        /// Ex : Euro -> European national teams
        /// </summary>
        public void AddTeamsToRecover()
        {
            for (int i = 0; i< _recuperedTeams.Count; i++)
            {
                RecoverTeams re = _recuperedTeams[i];
                int teamsToGrab = re.Number;
                int regularCount = 0;
                int currentCount = _clubs.Count;
                for (int j = 0; j<i; j++)
                {
                    regularCount += _recuperedTeams[j].Number;
                }
                int diff = regularCount - currentCount;
                if(diff > 0)
                {
                    teamsToGrab += diff;
                }
                if (teamsToGrab > re.Source.CountWithoutReserves() && rules.Contains(Rule.OnlyFirstTeams))
                {
                    teamsToGrab = re.Source.CountWithoutReserves();
                }
                foreach (Club c in re.Source.RetrieveTeams(teamsToGrab, re.Method, rules.Contains(Rule.OnlyFirstTeams), Tournament.parent.Association))
                {
                    _clubs.Add(c);
                }
            }
        }

        /// <summary>
        /// Affect stadiums to games in case of round is hosted by one country
        /// </summary>
        public void AffectHostStadiumsToGames()
        {
            int i = 0;
            foreach(Match match in _matches)
            {
                match.SetStadium(Tournament.hostStadiums[i % Tournament.hostStadiums.Count]);
                i++;
            }
        }

        public void Setup()
        {
            Initialise();
            if (rules.Contains(Rule.HostedByOneCountry))
            {
                AffectHostStadiumsToGames();
            }
        }

        /// <summary>
        /// Init the round (random draw, schedule)
        /// </summary>
        public abstract void Initialise();
        /// <summary>
        /// Qualify clubs for next rounds
        /// <paramref name="forNextYear">Qualify clubs to tournaments competing next year</paramref>
        /// </summary>
        public abstract void QualifyClubs(bool forNextYear);
        public abstract Round Copy();

        public abstract void DistributeGrants();
        /// <summary>
        /// Winner of the round
        /// </summary>
        /// <returns></returns>
        public abstract Club Winner();

        /// <summary>
        /// Next matches day of the round
        /// </summary>
        /// <returns></returns>
        public abstract List<Match> NextMatchesDay();

        public abstract int MatchesDayNumber();

        public abstract bool IsKnockOutRound();

        /// <summary>
        /// List the matches of the games day
        /// </summary>
        /// <param name="journey">The games day</param>
        /// <returns>Matches of game day j</returns>
        public abstract List<Match> GamesDay(int journey);

        public List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams, Association associationFilter)
        {
            List<Club> roundClubs = new List<Club>(_clubs);

            //If we have decided to have only first teams, we delete all reserves teams of the list
            if (onlyFirstTeams)
            {
                List<Club> toDelete = new List<Club>();
                foreach (Club c in roundClubs)
                {
                    if (c as ReserveClub != null)
                    {
                        toDelete.Add(c);
                    }
                }

                foreach (Club c in toDelete)
                {
                    roundClubs.Remove(c);
                }
            }
            //If a association filter is defined, keep only clubs from this association
            if(associationFilter != null)
            {
                List<Club> toDelete = new List<Club>();
                foreach (Club c in roundClubs)
                {
                    if (!associationFilter.ContainsAssociation(c.Association()))
                    {
                        toDelete.Add(c);
                    }
                }

                foreach (Club c in toDelete)
                {
                    roundClubs.Remove(c);
                }

            }

            switch (method)
            {
                case RecuperationMethod.Randomly:
                    roundClubs = Utils.ShuffleList<Club>(roundClubs);
                    break;
                case RecuperationMethod.Best:
                    try
                    {
                        roundClubs.Sort(new ClubComparator(ClubAttribute.PAST_RANKING));
                    }
                    catch (Exception e)
                    {
                        Utils.Debug("Erreur sort Club_Niveau_Comparator pour " + name);
                    }
                    break;
                case RecuperationMethod.Worst :
                    try
                    {
                        roundClubs.Sort(new ClubComparator(ClubAttribute.PAST_RANKING, true));
                    }
                    catch(Exception e)
                    {
                        Utils.Debug("Erreur sort Club_Niveau_Comparator pour " + name);
                    }
                    break;
                case RecuperationMethod.QualifiedForInternationalCompetition:
                    roundClubs = Session.Instance.Game.kernel.LocalisationTournament(this.Tournament).GetContinentalAssociation().GetContinentalClubs(roundClubs);
                    roundClubs.Sort(new ClubComparator(ClubAttribute.PAST_RANKING));
                    break;
                case RecuperationMethod.NotQualifiedForInternationalCompetitionBest:
                case RecuperationMethod.NotQualifiedForInternationalCompetitionWorst:
                    List<Club> internationalClubs = Session.Instance.Game.kernel.LocalisationTournament(this.Tournament).GetContinentalAssociation().GetContinentalClubs(roundClubs);
                    foreach (Club c in internationalClubs)
                    {
                        roundClubs.Remove(c);
                    }
                    roundClubs.Sort(new ClubComparator(ClubAttribute.PAST_RANKING, method == RecuperationMethod.NotQualifiedForInternationalCompetitionWorst));
                    break;
                case RecuperationMethod.StatusPro:
                    List<Club> pro = new List<Club>();
                    foreach(Club c in roundClubs)
                    {
                        if(c.status == ClubStatus.Professional)
                        {
                            pro.Add(c);
                        }
                    }
                    roundClubs.Clear();
                    roundClubs.AddRange(pro);
                    break;
                default:
                    roundClubs.Sort(new ClubComparator(ClubAttribute.LEVEL));
                    break;

            }
            if (number == -1)
            {
                number = roundClubs.Count;
            }
            List<Club> res = new List<Club>();

            for (int i = 0; i < number; i++)
            {
               res.Add(roundClubs[i]);
            }
            return res;
        }


        /// <summary>
        /// Adjust qualification of championship (or group for RoundGroup) by replacing negative ranking by "true" position on ranking, and filling hole by qualifications for next year on the same championship (if the tournament is a championship)
        /// </summary>
        /// <param name="qualificationsList">List of qualifications (can be specific to round or a group)</param>
        /// <param name="totalClubsInRanking">Clubs count on the table (round club count for ChampionshipRound, group club count for specific group)</param>
        /// <returns></returns>
        public List<Qualification> AdaptQualificationsToRanking(List<Qualification> qualificationsList, int totalClubsInRanking)
        {
            //Adapt qualifications to adapt negative ranking to real ranking in the group
            List<int> allRankings = Enumerable.Range(1, totalClubsInRanking).ToList();

            for (int i = 0; i < qualificationsList.Count; i++)
            {
                Qualification q = qualificationsList[i];
                if (q.ranking < 0)
                {
                    qualificationsList[i] = new Qualification(totalClubsInRanking + q.ranking + 1, q.roundId, q.tournament, q.isNextYear, q.qualifies);
                }

                // This ranking have a qualification to another round, remove it from the list
                allRankings.Remove(qualificationsList[i].ranking);
            }

            // Add qualification to every ranking with no qualifications
            if (Tournament.isChampionship)
            {
                foreach (int remainingRanking in allRankings)
                {
                    qualificationsList.Add(new Qualification(remainingRanking, 0, Tournament, true, 0));
                }
            }
            return qualificationsList;
        }
        
        public override string ToString()
        {
            return _name;
        }
    }
}