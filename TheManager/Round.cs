using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{


    public enum Rule
    {
        AtHomeIfTwoLevelDifference,
        OnlyFirstTeams,
        ReservesAreNotPromoted
    }

    public enum RecuperationMethod
    {
        Randomly,
        Best,
        Worst
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
            throw new NotImplementedException();
        }
    }

    [DataContract(IsReference =true)]
    public class RoundProgrammation
    {
        [DataMember]
        private Hour _defaultHour;
        [DataMember]
        private List<DateTime> _gamesDays;
        [DataMember]
        private List<TvOffset> _tvScheduling;
        [DataMember]
        private DateTime _initialisation;
        [DataMember]
        private DateTime _end;
        [DataMember]
        private int _lastMatchDaysSameDayNumber;

        public Hour defaultHour { get => _defaultHour; }
        public List<DateTime> gamesDays { get => _gamesDays; }
        public List<TvOffset> tvScheduling { get => _tvScheduling; }
        public DateTime initialisation { get => _initialisation; }
        public DateTime end { get => _end; }
        public int lastMatchDaysSameDayNumber { get => _lastMatchDaysSameDayNumber; }

        public RoundProgrammation(Hour hour, List<DateTime> days, List<TvOffset> tvSchedule, DateTime initialisation, DateTime end, int lastDaySameDay)
        {
            _defaultHour = hour;
            _gamesDays = new List<DateTime>(days);
            _tvScheduling = tvSchedule;
            _initialisation = initialisation;
            _end = end;
            _lastMatchDaysSameDayNumber = lastDaySameDay;
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

        public TvOffset(int daysOffset, Hour hour, int probability, int gameDay)
        {
            DaysOffset = daysOffset;
            Hour = hour;
            Probability = probability;
            GameDay = gameDay;
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

        public Qualification(int Ranking, int IDTour, Tournament Tournament, bool nextYear)
        {
            ranking = Ranking;
            roundId = IDTour;
            tournament = Tournament;
            isNextYear = nextYear;
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

    [DataContract(IsReference =true)]
    [KnownType(typeof(ChampionshipRound))]
    [System.Xml.Serialization.XmlInclude(typeof(ChampionshipRound))]
    [KnownType(typeof(KnockoutRound))]
    [System.Xml.Serialization.XmlInclude(typeof(KnockoutRound))]
    [KnownType(typeof(InactiveRound))]
    [System.Xml.Serialization.XmlInclude(typeof(InactiveRound))]
    [KnownType(typeof(GroupsRound))]
    [System.Xml.Serialization.XmlInclude(typeof(GroupsRound))]
    public abstract class Round : IRecoverableTeams
    {
        /// <summary>
        /// Nom du tour
        /// </summary>
        [DataMember]
        protected string _name;
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
        /// If the round takes place in round-trip matches
        /// </summary>
        [DataMember]
        protected bool _twoLegs;

        /// <summary>
        /// Concern general scheduling of matches of the round (TV, hour, days)
        /// </summary>
        [DataMember]
        protected RoundProgrammation _programmation;

        [DataMember]
        protected List<Qualification> _qualifications;

        /// <summary>
        /// List of teams get from other tournament still in progress
        /// </summary>
        [DataMember]
        protected List<RecoverTeams> _recuperedTeams;

        /// <summary>
        /// Rules concerning the round
        /// Example : the team receiving if she evolves two divisions lower
        /// </summary>
        [DataMember]
        protected List<Rule> _rules;

        /// <summary>
        /// List of prizes given to the clubs at the end of the round
        /// </summary>
        [DataMember]
        protected List<Prize> _prizes;


        public string name { get => _name; }
        public List<Club> clubs { get => _clubs; }
        public List<Match> matches { get => _matches; }
        public bool twoLegs { get => _twoLegs; }
        public RoundProgrammation programmation { get => _programmation; }
        public List<Qualification> qualifications { get => _qualifications; }
        public List<RecoverTeams> recuperedTeams { get => _recuperedTeams; }
        public List<Rule> rules { get => _rules; }
        public List<Prize> prizes { get => _prizes; }

        public Tournament Tournament
        {
            get
            {
                Tournament competition = null;

                foreach(Tournament c in Session.Instance.Game.kernel.Competitions)
                {
                    foreach(Round t in c.rounds)
                    {
                        if(t == this)
                        {
                            competition = c;
                        }
                    }
                }

                return competition;
            }
        }

        public Round(string name, Hour hour, List<DateTime> dates, List<TvOffset> tvOffsets, DateTime initialisation, DateTime end, bool twoLegs, int lastDaysSameDay)
        {
            _name = name;
            _clubs = new List<Club>();
            _matches = new List<Match>();
            _programmation = new RoundProgrammation(hour, dates, tvOffsets, initialisation, end, lastDaysSameDay);
            _twoLegs = twoLegs;
            _qualifications = new List<Qualification>();
            _recuperedTeams = new List<RecoverTeams>();
            _rules = new List<Rule>();
            _prizes = new List<Prize>();
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
                        if (goalscorers.ContainsKey(em.player)) goalscorers[em.player]++;
                        else goalscorers[em.player] = 1;
                    }
                }
            }

            List<KeyValuePair<Player, int>> list = goalscorers.ToList();

            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            return list;
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
            if (_matches.Count == 0) pursue = false;
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
                        res.Add(m);
                    else pursue = false;
                }
                if (i == _matches.Count - 1) pursue = false;
                i++;
            }


            return res;
        }

        public int Points(Club c)
        {
            int points = 0;
            foreach (Match m in _matches)
            {
                if (m.Played)
                {
                    if (m.home == c)
                    {
                        if (m.score1 > m.score2)
                            points += 3;
                        else if (m.score2 == m.score1)
                            points++;
                    }
                    else if (m.away == c)
                    {
                        if (m.score2 > m.score1)
                            points += 3;
                        else if (m.score2 == m.score1)
                            points++;
                    }
                }
            }

            return points;
        }


        public int Played(Club c)
        {
            int played = 0;
            foreach (Match m in _matches)
            {
                if (m.Played && (m.home == c || m.away == c)) played++;
            }
            return played;
        }

        public int Wins(Club c)
        {
            int res = 0;
            foreach (Match m in _matches)
            {
                if (m.home == c)
                {
                    if (m.score1 > m.score2) res++;
                }
                else if (m.away == c)
                {
                    if (m.score2 > m.score1) res++;
                }
            }
            return res;
        }

        public int Draws(Club c)
        {
            int res = 0;
            foreach (Match m in _matches)
            {
                if (m.Played && (m.home == c || m.away == c))
                {
                    if (m.score1 == m.score2) res++;
                }
            }
            return res;
        }

        public int Loses(Club c)
        {
            int res = 0;
            foreach (Match m in _matches)
            {
                if (m.home == c)
                {
                    if (m.score1 < m.score2) res++;
                }
                else if (m.away == c)
                {
                    if (m.score2 < m.score1) res++;
                }
            }
            return res;
        }

        public int GoalsFor(Club c)
        {
            int res = 0;
            foreach (Match m in _matches)
            {
                if (m.home == c)
                {
                    res += m.score1;
                }
                else if (m.away == c)
                {
                    res += m.score2;
                }
            }
            return res;
        }

        public int GoalsAgainst(Club c)
        {
            int res = 0;
            foreach (Match m in _matches)
            {
                if (m.home == c)
                {
                    res += m.score2;
                }
                else if (m.away == c)
                {
                    res += m.score1;
                }
            }
            return res;
        }

        public int Difference(Club c)
        {
            return GoalsFor(c) - GoalsAgainst(c);
        }

        public void Reset()
        {
            _matches = new List<Match>();
            _clubs = new List<Club>();
        }

        /// <summary>
        /// Add clubs to get from other tournaments
        /// Ex : last64 CDF -> L1
        /// Ex : Euro -> European national teams
        /// </summary>
        public void AddTeamsToRecover()
        {
            foreach(RecoverTeams re in _recuperedTeams)
            {
                bool onlyFirstTeams = false;
                if (rules.Contains(Rule.OnlyFirstTeams)) onlyFirstTeams = true;
                foreach (Club c in re.Source.RetrieveTeams(re.Number, re.Method, onlyFirstTeams))
                {
                    _clubs.Add(c);
                }
            }
        }

        /// <summary>
        /// Init the round (random draw, schedule)
        /// </summary>
        public abstract void Initialise();
        /// <summary>
        /// Quality clubs for next rounds
        /// </summary>
        public abstract void QualifyClubs();
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
        


        public List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams)
        {
            List<Club> clubs = new List<Club>(_clubs);
            
            //If we have decided to have only first teams, we delete all reserves teams of the list
            if(onlyFirstTeams)
            {
                List<Club> toDelete = new List<Club>();
                foreach (Club c in clubs)
                {
                    if (c as ReserveClub != null)
                    {
                        toDelete.Add(c);
                    }
                }

                foreach (Club c in toDelete)
                {
                    clubs.Remove(c);
                }
            }


            switch (method)
            {
                case RecuperationMethod.Randomly:
                    clubs = Utils.ShuffleList<Club>(clubs);
                    break;
                case RecuperationMethod.Best:
                    try
                    {
                        clubs.Sort(new ClubLevelComparator());
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Erreur sort Club_Niveau_Comparator pour " + name);
                    }
                    break;
                case RecuperationMethod.Worst:
                    clubs.Sort(new ClubLevelComparator(true));
                    break;
            }
            List<Club> res = new List<Club>();
            for (int i = 0; i < number; i++) res.Add(clubs[i]);
            return res;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}