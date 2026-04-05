using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Text;
using System.Windows.Media;
using tm.Tournaments;
using tm.Comparators;
using System.Runtime.InteropServices;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using tm.Algorithms;
using System.Diagnostics.Metrics;

/*
 * TODO: Factorisations possibles :
 * club.status, competition.status
 * 
 * competition.reset, (country.reset), continent.reset
 */

namespace tm
{

    public enum TournamentRule
    {
        NoRule,
        OnWinnerQualifiedAdaptClubsQualifications,
        OnWinnerQualifiedAdaptAssociationQualifications
    }

    [DataContract(IsReference =true)]
    public class RecordEntry<T>
    {
        [DataMember]
        public int Statistic { get; set; }
        [DataMember]
        public T Entity { get; set; }

        public RecordEntry()
        {

        }

        public RecordEntry(int statistic, T entity)
        {
            this.Statistic = statistic;
            this.Entity = entity;
        }
    }

    [DataContract(IsReference = true)]
    //[Owned]
    public class TournamentStatistics : IEquatable<TournamentStatistics>
    {
        [DataMember]
        public Match BiggerScore { get; set; }
        [DataMember]
        public Match LargerScore { get; set; }
        [DataMember]
        public RecordEntry<Player> TopGoalscorerOnOneSeason { get; set; }
        [DataMember]
        public RecordEntry<Club> BiggestAttack { get; set; }
        [DataMember]
        public RecordEntry<Club> WeakestAttack { get; set; }
        [DataMember]
        public RecordEntry<Club> BiggestDefense { get; set; }
        [DataMember]
        public RecordEntry<Club> WeakestDefense { get; set; }
        [DataMember]
        public RecordEntry<Club> MostPoints { get; set; }
        [DataMember]
        public RecordEntry<Club> LowestPoints { get; set; }

        public TournamentStatistics()
        {
            BiggerScore = null;
            LargerScore = null;
            TopGoalscorerOnOneSeason = new RecordEntry<Player>(0, null);
            BiggestAttack = new RecordEntry<Club>(0, null);
            WeakestAttack = new RecordEntry<Club>(0, null);
            BiggestDefense = new RecordEntry<Club>(0, null);
            WeakestDefense = new RecordEntry<Club>(0, null);
            LowestPoints = new RecordEntry<Club>(0, null);
            MostPoints = new RecordEntry<Club>(0, null);
        }

        public bool Equals(TournamentStatistics other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract(IsReference = true)]
    public class Tournament
    {

        [DataMember]
        [Key]
        public int Id { get; set; }
        [DataMember]
        private string _name;
        [DataMember]
        private List<Round> _rounds;
        [DataMember]
        private string _logo;
        [DataMember]
        private GameDay _seasonBeginning;
        [DataMember]
        private string _shortName;
        [DataMember]
        private List<Club>[] _nextYearQualified;
        [DataMember]
        private bool _isChampionship;
        [DataMember]
        private int _level;
        [DataMember]
        private TournamentStatistics _statistics;
        [DataMember]
        private Dictionary<int, Tournament> _previousEditions;
        [DataMember]
        private int _periodicity;
        [DataMember]
        private int _remainingYears;
        /// <summary>
        /// Extra rounds added for this year (eg French league cup)
        /// </summary>
        [DataMember]
        private int _extraRounds;
        [DataMember]
        private Color _color;
        [DataMember]
        private List<Stadium> _hostStadiums;
        [DataMember]
        private List<TournamentRule> _rules;
        [DataMember]
        private ClubStatus _status;
        /// <summary>
        /// Case of regional cup tournament (French cup)
        /// </summary>
        [DataMember]
        private Tournament _parent;

        /// <summary>
        /// Keep the overall structure and constraints of the cup in order to build the cup structure.
        /// Can be null
        /// </summary>
        [DataMember]
        private CupStructure _cupStructure;

        [DataMember]
        private Association _association;

        public CupStructure cupStructure { get => _cupStructure; set => _cupStructure = value; }

        public string name { get => _name; }
        public Color color => _color;
        public List<Round> rounds { get => _rounds; }
        public string logo { get => _logo; }
        public ClubStatus status => _status;

        public Association association => _association;
        public List<Club>[] nextYearQualified => _nextYearQualified;

        public List<Stadium> hostStadiums => _hostStadiums;
        public bool isHostedByOneAssociation
        {
            get
            {
                bool isHostedByOneAssociation = false;
                foreach (Round r in _rounds)
                {
                    if (r.rules.Contains(Rule.HostedByOneAssociation))
                    {
                        isHostedByOneAssociation = true;
                    }
                }
                return isHostedByOneAssociation;
            }
        }

        public GameDay seasonBeginning => _seasonBeginning;
        public string shortName => _shortName;
        public Dictionary<int, Tournament> previousEditions => _previousEditions;
        public TournamentStatistics statistics { get => _statistics; set => _statistics = value; }

        public int remainingYears => _remainingYears;
        public int periodicity => _periodicity;

        /// <summary>
        /// Is a championship (L1, L2)
        /// Fixed : if it's a championship, the main round is the round at index 0
        /// </summary>
        public bool isChampionship => _isChampionship;

        /// <summary>
        /// Level in the hierarchy (L1 = 1, L2 = 2 ...)
        /// </summary>
        public int level //TODO: Getter only
        {
            get => _level; set => _level = value;
        }
        
        /// <summary>
        /// General rules applied to the tournament
        /// </summary>
        public List<TournamentRule> rules => _rules;

        public Tournament parent => _parent;

        public Tournament()
        {
            _rounds = new List<Round>();
            _previousEditions = new Dictionary<int, Tournament>();
            _hostStadiums = new List<Stadium>();
            _rules = new List<TournamentRule>();
        }

        public Tournament(int id, string name, string logo, Association association, GameDay seasonBeginning, string shortName, bool isChampionship, int level, int periodicity, int remainingYears, Color color, ClubStatus status, Tournament parent, CupStructure structure)
        {
            Id = id;
            _rounds = new List<Round>();
            _name = name;
            _logo = logo;
            _association = association;
            _seasonBeginning = seasonBeginning;
            _shortName = shortName;
            _isChampionship = isChampionship;
            _level = level;
            _statistics = new TournamentStatistics();
            _previousEditions = new Dictionary<int, Tournament>();
            _periodicity = periodicity;
            _remainingYears = remainingYears;
            _color = color;
            _hostStadiums = new List<Stadium>();
            _extraRounds = 0;
            _rules = new List<TournamentRule>();
            _status = status;
            _parent = parent;
            _cupStructure = structure;
        }

        public void InitializeQualificationsNextYearsLists(int count = -1)
        {
            if (count == -1)
            {
                count = rounds.Count;
            }
            _nextYearQualified = new List<Club>[count];
            for (int i = 0; i < count; i++)
            {
                _nextYearQualified[i] = new List<Club>();
            }
        }

        /// <summary>
        /// Get hosts countries of the tournament
        /// </summary>
        /// <returns>List of hosts countries</returns>
        public List<Association> Hosts()
        {
            List<Association> hosts = new List<Association>();
            foreach(Stadium stadium in _hostStadiums)
            {
                Association country = stadium.city.Country().GetCountryAssociation();
                if(!hosts.Contains(country))
                {
                    hosts.Add(country);
                }
            }
            return hosts;
        }

        /// <summary>
        /// Initialize retained country and stadiums for the tournament
        /// </summary>
        public void InitializeHost()
        {
            _hostStadiums.Clear();
            List<Association> candidates = new List<Association>();
            //Find country
            foreach (Round r in _rounds)
            {
                foreach (Club c in r.clubs)
                {
                    Association candidate = c.Association();
                    if (candidate.stadiums.Count > 7)
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            Association host = candidates.Count > 0 ? candidates[Session.Instance.Random(0, candidates.Count)] : null;
            //Find stadiums
            if (host != null)
            {
                Console.WriteLine("[{0}] {1} chosen to host the tournament.", _name, host.Name());
                List<Stadium> stadiums = new List<Stadium>(host.stadiums);
                stadiums.Sort(new StadiumComparator());
                for (int i = 0; i < 8; i++)
                {
                    _hostStadiums.Add(stadiums[i]);
                    Console.WriteLine(stadiums[i].name + " - " + stadiums[i].capacity + " (" + stadiums[i].city.Country().GetCountryAssociation().name + ")");
                }
                Console.WriteLine("=================================");
            }
        }

        public List<Tournament> GetChildTournaments()
        {
            List<Tournament> childTournaments = new List<Tournament>();
            foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                if(t.parent == this)
                {
                    childTournaments.Add(t);
                }
            }
            return childTournaments;
        }

        public class LeagueCupApparition
        {

            public bool isBestTeams { get => _teams < 0; }

            private int _teams;
            public int teams
            {
                get
                {
                    return Math.Abs(_teams);
                }
                set
                {
                    _teams = value;
                }
            }
            public int apparitionRound { get; set; }
            public Tournament tournament { get; set; }

            public LeagueCupApparition(int teams, int apparitionRound, Tournament tournament)
            {
                _teams = teams;
                this.apparitionRound = apparitionRound;
                this.tournament = tournament;
            }
        }

        public int[] GetTeamsAtEachRound()
        {
            int[] teamsAtEachRound = new int[rounds.Count];
            Association association = this.association.ClosestStateAssociation();
            List<Tournament> otherTournaments = association != null ? association.Leagues() : Session.Instance.Game.kernel.Competitions;
            foreach (Tournament t in otherTournaments)
            {
                List<Round> rounds = t != this ? t.rounds : new List<Round>() { t.rounds[0] };
                int i = 0;
                foreach (Round r in rounds)
                {
                    foreach (Qualification q in r.qualifications)
                    {
                        if (!q.isNextYear && q.target.Tournament() == this)
                        {
                            if ((r as KnockoutRound) == null)
                            {
                                teamsAtEachRound[q.roundId]++;
                            }
                            else
                            {
                                teamsAtEachRound[q.roundId] += t.GetTeamsAtEachRound()[i] / 2;
                            }
                        }
                    }
                    i++;
                }
            }
            return teamsAtEachRound;
        }

        /// <summary>
        /// Get number of teams relegated or promoted after playoffs of the tournament. Assume playoffs are knockout games
        /// TODO: Management of inactive tournaments ...
        /// CONSIDER COUNT OF TEAMS OF THE LEAGUE QUALIFIES AFTER PLAYOFFS ONLY WHEN PLAYOFFS DOESN'T SHUFFLE TWO LEAGUES, OTHERWISE CAN'T MAKE ASSUMPTION OF TEAMS COUNT OF THIS LEAGUE
        /// </summary>
        /// <param name="relegation">Consider relegation if relegation is True, otherwise consider promotion</param>
        /// <returns></returns>
        public int TeamsQualifyAfterPlayOffs(bool relegation)
        {
            int res = 0;
            int[] teamsAtEachRound = GetTeamsAtEachRound();
            for (int i = 1; i < rounds.Count; i++)
            {
                int games = teamsAtEachRound[i] / 2;
                foreach (Qualification q in rounds[i].qualifications)
                {
                    if (q.isNextYear && ((relegation && IsAbove(q.target)) || (!relegation && IsBelow(q.target))))
                    {
                        res += games;
                    }
                    else if (!q.isNextYear && q.target.Tournament() == this)
                    {
                        teamsAtEachRound[q.roundId] += games;
                    }
                }
            }
            return res;
        }

        private int TeamsCountRound(List<RecoverTeams> recoverTeams)
        {
            int res = 0;
            foreach(RecoverTeams rt in recoverTeams)
            {
                res += rt.Number; //TeamsCount(rt);
            }
            return res;
        }

        /// <summary>
        /// Return True if this tournament is above in the league system to the target tournament.
        /// Return False otherwise or if the two tournaments sits at the same level in the league system
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool IsAbove(QualificationTarget to)
        {
            bool isAbove = false;
            Association tAssociation = this.association;
            int hierarchyAssociation = Session.Instance.Game.kernel.worldAssociation.GetLevelOfAssociation(tAssociation, 0);
            if (to.Type == QualificationTargetType.Tournament)
            {
                Tournament t = to.Tournament();
                Association otherAssociation = t.association;
                // if tAssociation is a parent of otherAssociation, so self is above
                if (tAssociation.GetAllChilds().Contains(otherAssociation))
                {
                    isAbove = true;
                }
                // else if tAssociation and otherAssociation are on the same hierarchal level, check the level of the tournaments
                else
                {
                    int hierarchyOther = Session.Instance.Game.kernel.worldAssociation.GetLevelOfAssociation(otherAssociation, 0);
                    if (hierarchyAssociation == hierarchyOther)
                    {
                        isAbove = this.level < t.level;
                    }
                }
            }
            else
            {                
                int qDestinationLevel = to.GetAssociationLevel();
                int selfLevel = hierarchyAssociation;
                if(selfLevel < qDestinationLevel)
                {
                    isAbove = true;
                }
                else if (qDestinationLevel == selfLevel)
                {
                    int selfTLevel = level;
                    int otherTLevel = to.GetTournamentLevel();
                    isAbove = otherTLevel > selfTLevel;
                }
            }
            return isAbove;
        }

        /// <summary>
        /// Return True if this tournament is below in the league system to the target tournament
        /// Return False otherwise or if the two tournaments sits at the same level in the league system
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool IsBelow(QualificationTarget to)
        {
            bool isBelow = false;
            Association tAssociation = this.association;
            int hierarchyAssociation = Session.Instance.Game.kernel.worldAssociation.GetLevelOfAssociation(tAssociation, 0);
            if (to.Type == QualificationTargetType.Tournament)
            {
                Tournament t = to.Tournament();
                Association otherAssociation = t.association;
                // if otherAssociation is a parent of tAssociation, so self is above
                if (otherAssociation.GetAllChilds().Contains(tAssociation))
                {
                    isBelow = true;
                }
                // else if tAssociation and otherAssociation are on the same hierarchal level, check the level of the tournaments
                else
                {
                    int hierarchyOther = Session.Instance.Game.kernel.worldAssociation.GetLevelOfAssociation(otherAssociation, 0);
                    if (hierarchyAssociation == hierarchyOther)
                    {
                        isBelow = this.level > t.level;
                    }
                }
            }
            else
            {
                int qDestinationLevel = to.GetAssociationLevel();
                int selfLevel = hierarchyAssociation;
                if (qDestinationLevel < selfLevel)
                {
                    isBelow = true;
                }
                else if(qDestinationLevel == selfLevel)
                {
                    int selfTLevel = level;
                    int otherTLevel = to.GetTournamentLevel();
                    isBelow = selfTLevel > otherTLevel;
                }
            }
            return isBelow;
        }

        /// <summary>
        /// Return True if this tournament sits at the same level in the league system
        /// Return False otherwise
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool IsSameLevel(QualificationTarget to)
        {
            int otherALevel = to.GetAssociationLevel();
            int otherTLevel = to.GetTournamentLevel();
            Association tAssociation = this.association;
            int selfALevel = Session.Instance.Game.kernel.worldAssociation.GetLevelOfAssociation(tAssociation, 0);
            int selfTLevel = this.level;
            return otherALevel == selfALevel && otherTLevel == selfTLevel;
        }

        /// <summary>
        /// Get "pivot" round where tournament goes from regional to national
        /// </summary>
        private int GetFirstNationalRound()
        {
            int idRoundPivot = -1;
            //Find round where qualified teams depends of regions
            for (int i = 0; i < rounds.Count && idRoundPivot == -1; i++)
            {
                Round r = rounds[i];
                if (r.teamsByAssociation.Count > 0)
                {
                    idRoundPivot = i;
                }
            }
            return idRoundPivot;
        }

        /// <summary>
        /// Split cup between final phase and multiple qualifications tournaments for each region
        /// Example : French Cup before/after 7th round
        /// </summary>
        public void CreateRegionalPathForCup()
        {
            int idRoundPivot = GetFirstNationalRound();
            if(idRoundPivot > -1)
            {
                Round pivotRound = rounds[idRoundPivot];
                List<Tournament> newTournaments = new List<Tournament>();
                foreach (KeyValuePair<Association, int> kvp in pivotRound.teamsByAssociation)
                {
                    //Copie du tournoi est créée
                    Tournament regionalTournament = CopyForArchive(false);
                    regionalTournament._name = string.Format("{0} - {1}", regionalTournament, kvp.Key.name);
                    regionalTournament._level = 1000; //Un niveau exagérément élevé est mis pour éviter d'aller chercher les vainqueurs de ces compétitions pour les places européennes par exemple
                    regionalTournament._parent = this; //Cette compétition dépend du tournoi principal
                    regionalTournament.InitializeQualificationsNextYearsLists();
                    for (int id = rounds.Count-1; id >= idRoundPivot; id--)
                    {
                        regionalTournament.rounds.RemoveAt(id);
                    }
                    foreach(Round r in regionalTournament.rounds)
                    {
                        r.clubs.Clear();
                    }
                    //Le dernier tour doit qualifier à la compétition principale, CopyForArchive ayant reporté les autres qualifications à la nouvelle compétition
                    for (int i = 0; i < regionalTournament.rounds.Last().qualifications.Count; i++)
                    {
                        Qualification qualification = regionalTournament.rounds.Last().qualifications[i];
                        if (qualification.target.Tournament() == regionalTournament && !qualification.isNextYear)
                        {
                            regionalTournament.rounds.Last().qualifications[i] = new Qualification(qualification.ranking, qualification.roundId, new QualificationTournament(this), qualification.isNextYear, qualification.qualifies);
                        }
                    }
                    kvp.Key.tournaments.Add(regionalTournament);
                    newTournaments.Add(regionalTournament);
                }
                //Dans un premier temps pour éviter les rework qualifs et qualifs outre-mer ne pas supprimer les premiers tours mais les équipes qui y rentrent
                for(int i = 0; i < idRoundPivot; i++)
                {
                    rounds[i].recuperedTeams.Clear();
                }
                //Dispatcher les équipes enregistrées en dur dans les compétitions filles
                newTournaments.Shuffle();
                for (int i = 0; i < idRoundPivot; i++)
                {
                    for(int j = 0; j < rounds[i].clubs.Count; j++)
                    {
                        Club c = rounds[i].clubs[j];
                        newTournaments[j % newTournaments.Count].rounds[i].clubs.Add(c);
                    }
                    rounds[i].clubs.Clear();
                }
                UpdateCupQualifications();
                foreach (Tournament regionalTournament in newTournaments)
                {
                    regionalTournament.UpdateCupQualifications();
                }
            }
        }

        private string MakeExtraRoundName(int round, int extraRounds)
        {
            string n = extraRounds == 1 ? "" : (round + 1).ToString();
            return String.Format("Extra preliminary round {0}", n);
        }

        public void WriteCupStrutureResult(CupStructureResult result)
        {
            int extraRounds = result.roundsCount - rounds.Count;
            List<GameDay> availableDates = UtilsTournaments.GetDatesForExtraRound(this, result.leaguesRepresented);
            List<GameDay> extraDates = new List<GameDay>();
            for (int i = 0; i < extraRounds; i++)
            {
                int dateIndex = (availableDates.Count - 2) - (3 * i);
                GameDay gd = availableDates[dateIndex];
                extraDates.Add(gd);
            }

            Round roundModel = rounds[0];
            for(int i = 0; i < extraRounds; i++)
            {
                List<GameDay> extraRoundDates = new List<GameDay>();
                foreach(GameDay dt in roundModel.programmation.gamesDays)
                {
                    extraRoundDates.Add(new GameDay(extraDates[i].WeekNumber, dt.MidWeekGame, dt.YearOffset, dt.DayOffset));
                }
                //Create a new extra preliminary round
                GameDay gdInit = new GameDay(extraDates[i].WeekNumber - 1, true, roundModel.programmation.initialisation.YearOffset, roundModel.programmation.initialisation.DayOffset);
                GameDay gdEnd = new GameDay(extraDates[i].WeekNumber + 1, roundModel.programmation.initialisation.MidWeekGame, roundModel.programmation.initialisation.YearOffset, roundModel.programmation.initialisation.DayOffset);
                KnockoutRound kr = new KnockoutRound(Session.Instance.Game.kernel.NextIdRound(), MakeExtraRoundName(i, extraRounds), this, roundModel.programmation.defaultHour, extraRoundDates, new List<TvOffset>(), roundModel.phases, gdInit, gdEnd, RandomDrawingMethod.Random, false, roundModel.programmation.gamesPriority);
                kr.recuperedTeams.AddRange(result.structure[i]);
                kr.rules.AddRange(roundModel.rules);
                kr.qualifications.Add(new Qualification(1, i + 1, new QualificationTournament(this), false, -1));
                rounds.Insert(i, kr);
            }
            _extraRounds = extraRounds;

            if(extraRounds < 0)
            {
                for(int i = 0; i < Math.Abs(extraRounds); i++)
                {
                    rounds[i].recuperedTeams.Clear();
                }
            }

            for (int i = Math.Abs(extraRounds); i < result.structure.Count; i++)
            {
                rounds[i].recuperedTeams.Clear();
                rounds[i].recuperedTeams.AddRange(result.structure[i]);
                for (int j = 0; j < rounds[i].qualifications.Count; j++)
                {
                    Qualification q = rounds[i].qualifications[j];
                    if (!q.isNextYear && q.target.Tournament() == this)
                    {
                        rounds[i].qualifications[j] = new Qualification(q.ranking, q.roundId + extraRounds, q.target, q.isNextYear, q.qualifies);
                    }
                }
            }
        }

        /// <summary>
        /// Update national cup qualifications due to annual league structure modifications
        /// </summary>
        public void UpdateCupQualifications()
        {
            Utils.Debug(Session.Instance.Game.date.ToShortDateString() + " [UpdateCupQualifications " + name + "] (" + this.association + ")");
            //Sauvegarde en mémoire les qualifications en coupe par défaut, elles pourraient être amenées à changer en cas de modification de la structure de la ligue
            if(!AlreadyStoredRecuperedTeams())
            {
                foreach(Round r in _rounds)
                {
                    r.baseRecuperedTeams.AddRange(new List<RecoverTeams>(r.recuperedTeams));
                }
            }

            // Les qualifications de chaque ligues à chaque tour sont remises par défaut
            foreach (Round r in _rounds)
            {
                r.recuperedTeams.Clear();
                r.recuperedTeams.AddRange(new List<RecoverTeams>(r.baseRecuperedTeams));
            }

            //Pour toutes les compétitions qui qualifient des équipes pour cette compétition sans pour autant faire partie du système de ligue :
            //Si les équipes sont qualifiées durant la phase régionale de la compétition, ces places de qualifications sont dispatchées aléatoirement au sein des régions.
            //Normalement, le UpdateCupQualifications() de la coupe nationale est appelé avant celle des compétitions régionales, donc les régions pourront gérer cette équipe supplémentaire juste après
            int idRoundPivot = GetFirstNationalRound();
            if(idRoundPivot > -1)
            {
                List<Tournament> childTournaments = new List<Tournament>(GetChildTournaments());
                childTournaments.Shuffle();
                int counter = 0;
                foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if (!t.IsInternational() && t != this && t.parent != this)
                    {
                        foreach (Round r in t.rounds)
                        {
                            for (int i = 0; i < r.qualifications.Count; i++)
                            {
                                if (r.qualifications[i].target.Tournament() != null && (r.qualifications[i].target.Tournament().parent == this || (r.qualifications[i].target.Tournament() == this && r.qualifications[i].roundId < idRoundPivot)))
                                {
                                    Tournament hostTournament = childTournaments[(counter++)%childTournaments.Count];
                                    Utils.Debug(string.Format("[Host Cup] {0} send winner of {1} to {2}", t.name, r.name, hostTournament.name));
                                    r.qualifications[i] = new Qualification(r.qualifications[i].ranking, r.qualifications[i].roundId, new QualificationTournament(hostTournament), r.qualifications[i].isNextYear, r.qualifications[i].qualifies);
                                }
                            }
                        }
                    }
                }
            }

            bool leagueCupLike = false;
            foreach(Round r in _rounds)
            {
                foreach(RecoverTeams rt in r.baseRecuperedTeams)
                {
                    bool flagLeagueCupStyle = rt.Flags.HasFlag(RetrieveFlags.QualifiedForInternationalCompetition) || rt.Flags.HasFlag(RetrieveFlags.NotQualifiedForInternationalCompetition);
                    leagueCupLike = leagueCupLike || flagLeagueCupStyle;
                }
            }

            CupCreator creator = new CupCreator();
            CupStructureResult structure = creator.CreateStructure(association, _cupStructure);
            WriteCupStrutureResult(structure);
        }

        private void PrintTournamentResumeShort()
        {
            Console.WriteLine("Résumé de la compétition " + this.name);
            foreach (Round r in _rounds)
            {
                Console.WriteLine("= " + r.name + " =");
                foreach (RecoverTeams rt in r.recuperedTeams)
                {
                    Console.WriteLine(rt.Source.ToString() + " - " + rt.Number + " - " + rt.Flags);
                }
            }
        }

        private void PrintTournamentResume(int[] teamsFromOutsideLeagueSystem)
        {
            Console.WriteLine("Nouveaux tours");
            int cupTeams = 0;
            for (int i = 0; i < rounds.Count; i++)
            {
                List<RecoverTeams> recoverTeams = rounds[i].recuperedTeams;
                cupTeams += TeamsCountRound(recoverTeams);
                cupTeams += i < teamsFromOutsideLeagueSystem.Length ? teamsFromOutsideLeagueSystem[i] : 0;
                Console.WriteLine("_____ Round " + i + " _____ " + rounds[i].name);
                Console.WriteLine("Initialisé le " + rounds[i].programmation.initialisation.WeekNumber + " " + rounds[i].programmation.initialisation.MidWeekGame);
                Console.WriteLine("Se joue le " + rounds[i].programmation.gamesDays[0].WeekNumber + " " + rounds[i].programmation.initialisation.MidWeekGame);
                Console.WriteLine("Cloturé le " + rounds[i].programmation.end.WeekNumber + " " + rounds[i].programmation.end.MidWeekGame);
                foreach (RecoverTeams rt in recoverTeams)
                {
                    int totalAdmTeamsCount = rt.Source.RetrieveTeams(-1, rt.Flags, rounds[i].rules.Contains(Rule.OnlyFirstTeams), this.association).Count;
                    Console.WriteLine("+ " + (rt.Source as Round).Tournament.name + " - " + rt.Number + "/" + totalAdmTeamsCount + " - " + rt.Flags);
                }
                Console.WriteLine(cupTeams + " équipes pour " + (cupTeams / 2) + " matchs");
                cupTeams /= 2;
            }
        }

        private bool AlreadyStoredRecuperedTeams()
        {
            bool alreadyStoredRecuperedTeams = false;
            foreach (Round r in _rounds)
            {
                if (r.baseRecuperedTeams.Count > 0)
                {
                    alreadyStoredRecuperedTeams = true;
                }
            }
            return alreadyStoredRecuperedTeams;
        }

        public Tournament CopyForArchive(bool makeRoundsInactive, string newName = "")
        {
            if(newName.Length == 0)
            {
                newName = _name;
            }
            Tournament copy = new Tournament(Session.Instance.Game.kernel.NextIdTournament(), newName, _logo, _association, _seasonBeginning, _shortName, _isChampionship, _level, _periodicity, _remainingYears, _color, _status, _parent, _cupStructure);
            foreach (Round r in rounds)
            {
                Round roundCopy = r.Copy();
                roundCopy.Tournament = copy;
                roundCopy.recuperedTeams.AddRange(AlreadyStoredRecuperedTeams() ? r.baseRecuperedTeams : r.recuperedTeams);
                if(makeRoundsInactive)
                {
                    bool qualificationsForAllGroups = (roundCopy as GroupsRound != null) ? (roundCopy as GroupsRound).qualificationsDefinedForAllGroup : false;
                    roundCopy = new GroupInactiveRound(Session.Instance.Game.kernel.NextIdRound(), roundCopy.name, this, roundCopy.programmation.defaultHour, new List<GameDay>(), new List<TvOffset>(), 1, qualificationsForAllGroups, 1, roundCopy.programmation.initialisation, roundCopy.programmation.end, -1, RandomDrawingMethod.Geographic, false, 0, 0, 0);
                }
                for (int i = 0; i < roundCopy.qualifications.Count; i++)
                {
                    Qualification q = roundCopy.qualifications[i];
                    if (roundCopy.qualifications[i].target.Tournament() == this)
                    {
                        q = new Qualification(q.ranking, q.roundId, new QualificationTournament(copy), q.isNextYear, q.qualifies);
                        roundCopy.qualifications[i] = q;
                    }
                }
                copy.rounds.Add(roundCopy);
            }
            copy.statistics = statistics;
            copy.hostStadiums.AddRange(hostStadiums);

            return copy;
        }

        public void QualifyClubsNextYear()
        {
            if(_remainingYears == 1)
            {
                foreach (Round r in rounds)
                {
                    r.QualifyClubs(true);
                }
            }
        }

        /// <summary>
        /// End of the season, all rounds are reset and qualified teams for next years are dispatched
        /// </summary>
        public void Reset()
        {
            _remainingYears--;
            if (_remainingYears == 0)
            {
                _remainingYears = _periodicity;
                UpdateRecords();
                Tournament copyForArchives = CopyForArchive(false);

                int gamesCount = 0;
                foreach(Round r in copyForArchives.rounds)
                {
                    gamesCount += r.matches.Count;
                }
                if (_periodicity == 1 || (_periodicity > 1 && gamesCount > 0))
                {
                    int editionYear = copyForArchives.rounds.Last().programmation.end.WeekNumber > this.seasonBeginning.WeekNumber ? Session.Instance.Game.date.Year - 1 : Session.Instance.Game.date.Year;
                    _previousEditions.Add(_periodicity == 1 ? editionYear : editionYear - periodicity, copyForArchives);
                }
                for (int i = 0; i<rounds.Count; i++)
                {

                    //Delete compo if we chose to reduce size of the savegame
                    if (Session.Instance.Game.options.reduceSaveSize)
                    {
                        foreach(Match m in rounds[i].matches)
                        {
                            m.compo1.Clear();
                            m.compo2.Clear();
                        }
                    }

                    rounds[i].Reset();
                    //Ignore first extra rounds created for this edition of the tournament
                    if(i >= _extraRounds)
                    {
                        List<Club> clubs = new List<Club>(_nextYearQualified[i - _extraRounds]);
                        foreach (Club c in clubs)
                        {
                            rounds[i].clubs.Add(c);
                            CityClub cc = c as CityClub;
                            if (cc != null && cc.history.elements.Count > 0)
                            {
                                if (status != ClubStatus.SemiProfessional || cc.status != ClubStatus.Professional || (status == ClubStatus.SemiProfessional && cc.status == ClubStatus.Professional && copyForArchives.rounds[0].clubs.Contains(cc)))
                                {
                                    c.ChangeStatus(status);
                                }
                            }

                        }
                    }
                }
                for(int i = 0; i < _extraRounds; i++)
                {
                    rounds.Remove(rounds.First());
                }
                for (int i = 0; i < rounds.Count; i++)
                {
                    for (int j = 0; j < rounds[i].qualifications.Count; j++)
                    {
                        rounds[i].qualifications[j] = new Qualification(rounds[i].qualifications[j].ranking, rounds[i].qualifications[j].roundId - _extraRounds, rounds[i].qualifications[j].target, rounds[i].qualifications[j].isNextYear, rounds[i].qualifications[j].qualifies);
                    }
                }
                _extraRounds = 0;
                InitializeQualificationsNextYearsLists();
                if(isHostedByOneAssociation)
                {
                    InitializeHost();
                }
            }
            Association localisation = this.association;
            Tournament locTopLeague = localisation.League(1);
            if (!isChampionship && !IsInternational() && (localisation.LeagueSystemWithReserves() || (locTopLeague != null && localisation.LeagueAbove(locTopLeague) != null)))
            {
                UpdateCupQualifications();
            }
        }

        public Tournament LastEdition()
        {
            Tournament res = null;
            if(previousEditions.Count > 0)
            {
                int closestYear = previousEditions.Aggregate((l, r) => l.Key > r.Key ? l : r).Key;
                res = previousEditions[closestYear];
            }
            return res;
        }

        /// <summary>
        /// TODO: Bad pattern. This attribute must not be modified by an external class
        /// </summary>
        public void AddYearToRemainingYears()
        {
            _remainingYears++;
        }

        private void UpdateRecords()
        {
            foreach(Round r in _rounds)
            {
                foreach(Match m in r.matches)
                {
                    if (_statistics.LargerScore == null || Math.Abs(m.score1 - m.score2) >
                        Math.Abs(_statistics.LargerScore.score1 - _statistics.LargerScore.score2))
                    {
                        _statistics.LargerScore = m;
                    }

                    if (_statistics.BiggerScore == null || m.score1 + m.score2 >
                        _statistics.BiggerScore.score1 + _statistics.BiggerScore.score2)
                    {
                        _statistics.BiggerScore = m;
                    }
                }
            }
            if (_isChampionship)
            {
                Round championship = _rounds[0];
                foreach(Club c in championship.clubs)
                {
                    int goalsFor = championship.GoalsFor(c);
                    int goalsAgainst = championship.GoalsAgainst(c);
                    int points = championship.Points(c);
                    if(_statistics.MostPoints.Entity == null || _statistics.MostPoints.Statistic < points)
                    {
                        RecordEntry<Club> newRecord = new RecordEntry<Club>(points, c);
                        _statistics.MostPoints = newRecord;
                    }
                    if (_statistics.LowestPoints.Entity == null || _statistics.LowestPoints.Statistic > points)
                    {
                        RecordEntry<Club> newRecord = new RecordEntry<Club>(points, c);
                        _statistics.LowestPoints = newRecord;
                    }
                    if (_statistics.BiggestAttack.Entity == null || _statistics.BiggestAttack.Statistic < goalsFor)
                    {
                        RecordEntry<Club> newRecord = new RecordEntry<Club>(goalsFor, c);
                        _statistics.BiggestAttack = newRecord;
                    }
                    if (_statistics.WeakestAttack.Entity == null || _statistics.WeakestAttack.Statistic > goalsFor)
                    {
                        RecordEntry<Club> newRecord = new RecordEntry<Club>(goalsFor, c);
                        _statistics.WeakestAttack = newRecord;
                    }
                    if (_statistics.BiggestDefense.Entity == null || _statistics.BiggestDefense.Statistic > goalsAgainst)
                    {
                        RecordEntry<Club> newRecord = new RecordEntry<Club>(goalsAgainst, c);
                        _statistics.BiggestDefense = newRecord;
                    }
                    if (_statistics.WeakestDefense.Entity == null || _statistics.WeakestDefense.Statistic < goalsAgainst)
                    {
                        RecordEntry<Club> newRecord = new RecordEntry<Club>(goalsAgainst, c);
                        _statistics.WeakestDefense = newRecord;
                    }
                }
            }
        }

        /// <summary>
        /// Get final playoffs round leading to accession or win on this tournament.
        /// Final round could be played inside another tournament
        /// (L2 playoffs finishing in L1)
        /// </summary>
        /// <param name="relegation">If true, get final playoffs leading to relegation</param>
        /// <returns></returns>
        public Round GetFinalTopPlayOffRound(bool relegation=false)
        {
            Round res = null;

            List<Round> browsed = new List<Round>();
            List<Round> tas = new List<Round>() { rounds[0] };
            while (tas.Count > 0)
            {
                Round r = tas[0];
                tas.Remove(r);
                browsed.Add(r);
                foreach (Qualification q in r.qualifications)
                {
                    if (q.isNextYear && q.target.ToChampionshipTournament() && ((!relegation && IsBelow(q.target)) || (relegation && IsAbove(q.target))) )
                    {
                        res = r;
                    }
                    if (!q.isNextYear && q.target.ToChampionshipTournament() && !browsed.Contains(q.target.Tournament()?.rounds[q.roundId]))
                    {
                        tas.Add(q.target.Tournament().rounds[q.roundId]);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Get list of rounds that consists of final championship playoffs (for title or promotion)
        /// Can scan rounds outside of the league
        /// </summary>
        /// <param name="tournament">Tournament of round currently scanned</param>
        /// <param name="round">Round currently scanned</param>
        /// <param name="allRounds">List of rounds already scanned</param>
        /// <returns></returns>
        public List<Round> GetPlayOffsTree(Tournament tournament, Round round, List<Round> allRounds)
        {
            allRounds.Add(round);
            int roundIndex = tournament.rounds.IndexOf(round);
            List<Round> res = new List<Round>() { round };
            //Step 1 : Append rounds with losing teams
            foreach (Qualification q in round.qualifications)
            {
                if (!q.isNextYear && q.target.ToChampionshipTournament() && q.ranking > 1)
                {
                    Tournament targetTournament = q.target.Tournament();
                    if(targetTournament != null)
                    {
                        Round targetRound = q.target.Tournament().rounds[q.roundId];
                        if (!allRounds.Contains(targetRound))
                        {
                            res.AddRange(GetPlayOffsTree(targetTournament, targetRound, allRounds));
                        }
                    }
                }
            }
            //Step 2 : Append rounds with winning teams
            foreach (Qualification q in round.qualifications)
            {
                if (!q.isNextYear && q.target.ToChampionshipTournament() && q.ranking == 1)
                {
                    Tournament targetTournament = q.target.Tournament();
                    if(targetTournament != null)
                    {
                        Round targetRound = this.rounds[q.roundId];
                        if (!allRounds.Contains(targetRound))
                        {
                            res.AddRange(GetPlayOffsTree(targetTournament, targetRound, allRounds));
                        }
                    }
                }
            }

            List<Round> otherRounds = new List<Round>(this.rounds);
            foreach(Tournament kT in Session.Instance.Game.kernel.Competitions)
            {
                otherRounds.AddRange(kT.rounds);
            }
            //Step 3 : Insert in first positions rounds where teams come from
            for (int i = 1; i < otherRounds.Count; i++)
            {
                Round ri = otherRounds[i];
                if ((ri as KnockoutRound) != null && !allRounds.Contains(ri))
                {
                    foreach (Qualification q in ri.qualifications)
                    {
                        //Don't know if q.ranking == 1 is mandatory or not. Isn't adequate with some relegation barrages where the qualified team is the looser team
                        if (!q.isNextYear && q.target.Tournament() == tournament && q.roundId == roundIndex /*&& q.ranking == 1*/)
                        {
                            Tournament targetTournament = q.target.Tournament();
                            res.InsertRange(0, GetPlayOffsTree(targetTournament, ri, allRounds));
                        }
                    }
                }

            }
            return res;
        }

        /* Merged with GetPlayOffsTree, could be deleted
         * /// <summary>
        /// Get list of rounds that consists of final championship playoffs
        /// </summary>
        /// <param name="round">Round currently scanned</param>
        /// <param name="allRounds">List of rounds already scanned</param>
        /// <returns></returns>
        public List<Round> GetFinalPhaseTree(Round round, List<Round> allRounds)
        {
            allRounds.Add(round);
            int roundIndex = this.rounds.IndexOf(round);
            List<Round> res = new List<Round>() { round};
            //Step 1 : Append rounds with losing teams
            foreach(Qualification q in round.qualifications)
            {
                if(!q.isNextYear && q.tournament == this && q.ranking > 1)
                {
                    Round targetRound = this.rounds[q.roundId];
                    if(!allRounds.Contains(targetRound))
                    {
                        res.AddRange(GetFinalPhaseTree(targetRound, allRounds));
                    }
                }
            }
            //Step 2 : Append rounds with winning teams
            foreach (Qualification q in round.qualifications)
            {
                if (!q.isNextYear && q.tournament == this && q.ranking == 1)
                {
                    Round targetRound = this.rounds[q.roundId];
                    if (!allRounds.Contains(targetRound))
                    {
                        res.AddRange(GetFinalPhaseTree(targetRound, allRounds));
                    }
                }
            }
            //Step 3 : Insert in first positions rounds where teams come from
            for(int i = 1; i < this.rounds.Count; i++)
            {
                Round ri = this.rounds[i];
                if((ri as KnockoutRound) != null && !allRounds.Contains(ri))
                {
                    foreach(Qualification q in ri.qualifications)
                    {
                        if(!q.isNextYear && q.tournament == this && q.roundId == roundIndex && q.ranking == 1)
                        {
                            res.InsertRange(0, GetFinalPhaseTree(ri, allRounds));
                        }
                    }
                }

            }
            return res;
        }*/

        /// <summary>
        /// Return the last championship round of the tournament
        /// TODO: Incorrect round returned when multiple championship rounds are in the tournament where the tournament is turned inactive
        /// </summary>
        /// <returns></returns>
        public Round GetLastChampionshipRound()
        {
            Round res = null;
            if ((rounds[0] as GroupsRound).groupsCount > 1)
            {
                res = rounds[0];
            }
            for(int i = rounds.Count-1; i>=0 && res == null; i--)
            {
                GroupsRound gr = (rounds[i] as GroupsRound);
                if (gr != null && gr.groupsCount == 1)
                {
                    res = rounds[i];
                }
            }
            return res;
        }

        /// <summary>
        /// Get clubs playing on the tournament
        /// </summary>
        /// <returns></returns>
        public List<Club> Clubs()
        {
            List<Club> tournamentClubs = new List<Club>();
            foreach (Round r in rounds)
            {
                foreach(Club c in r.clubs)
                {
                    if(!tournamentClubs.Contains(c))
                    {
                        tournamentClubs.Add(c);
                    }
                }
            }
            return tournamentClubs;
        }

        /// <summary>
        /// Get Final Phase Clubs ranked from competition winner to first team eliminated
        /// TODO: Final phases with nested group rounds are not managed. Final phases can only be knockout rounds
        /// </summary>
        public List<Club> GetFinalPhasesClubs()
        {
            List<Club> finalClubs = new List<Club>();
            Round finalRound = null;
            Round lastChampionshipRound = GetLastChampionshipRound();
            lastChampionshipRound.qualifications.ForEach(q => finalRound = (!q.isNextYear && q.target.Tournament() == this && q.roundId > 0 && q.ranking == 1) ? _rounds[q.roundId] : finalRound);
            //If the league winner is qualified on another round this year on this tournament then the tournament finish with a final phase
            List<Round> finalRounds = new List<Round>();

            if (finalRound != null)
            {
                finalRounds = GetPlayOffsTree(this, finalRound, new List<Round>()) ;
            }
            List<KeyValuePair<Club, int>> clubsDictionnary = ExtractClubsFromPlayOffs(finalRounds);
            foreach(KeyValuePair<Club, int> kvp in clubsDictionnary)
            {
                finalClubs.Add(kvp.Key);
            }
            return finalClubs;
        }

        private List<KeyValuePair<Club, int>> ExtractClubsFromPlayOffs(List<Round> finalRounds)
        {
            List<KeyValuePair<Club, int>> finalClubs = new List<KeyValuePair<Club, int>>();
            List<Club> clubs = new List<Club>();
            for (int i = finalRounds.Count - 1; i >= 0; i--)
            {
                List<Club> roundClubs = new List<Club>(finalRounds[i].clubs);
                roundClubs.Sort(new ClubRankingComparator(finalRounds[i].matches, finalRounds[i].tiebreakers, finalRounds[i].pointsDeduction, RankingType.General, false, (finalRounds[i] as KnockoutRound) != null));

                foreach (Club c in roundClubs)
                {
                    if (!clubs.Contains(c))
                    {
                        finalClubs.Add(new KeyValuePair<Club, int>(c, i));
                        clubs.Add(c);
                    }
                }
            }
            return finalClubs;
        }

        /// <summary>
        /// Get clubs involved in promotion playoffs, ordered
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<Club, int>> GetTopPlayOffClubs()
        {
            List<KeyValuePair<Club, int>> res = new List<KeyValuePair<Club, int>>();
            Round topPlayOffRound = GetFinalTopPlayOffRound();
            if (topPlayOffRound != null)
            {
                if (topPlayOffRound != rounds[0])
                {
                    List<Round> rounds = GetPlayOffsTree(topPlayOffRound.Tournament, topPlayOffRound, new List<Round>());
                    res = ExtractClubsFromPlayOffs(rounds);
                }
            }
            return res;
        }

        public DateTime BeginDate()
        {
            int offsetYear = _remainingYears % _periodicity;
            return rounds[0].DateInitialisationRound().AddYears(offsetYear);
        }

        public DateTime EndDate()
        {
            int offsetYear = _remainingYears % _periodicity;
            return rounds.Last().DateEndRound().AddYears(offsetYear + rounds.Last().programmation.end.YearOffset);
        }

        public bool IsCurrentlyPlaying()
        {
            DateTime startDate = BeginDate();
            DateTime endDate = EndDate();
            bool isCurrentlyPlaying = Utils.IsBefore(startDate, Session.Instance.Game.date) && Utils.IsBefore(Session.Instance.Game.date, endDate);
            return isCurrentlyPlaying;
        }

        /// <summary>
        /// Qualify a club for a round on the next year edition fo the tournament
        /// </summary>
        /// <param name="c">The club to add</param>
        /// <param name="tourIndex">Index of the round where club is qualified</param>
        public void AddClubForNextYear(Club c, int tourIndex)
        {
            _nextYearQualified[tourIndex].Add(c);
        }

        public override string ToString()
        {
            return _name;
        }

        public int AverageAttendance(Club c)
        {
            int i = 0;
            int attendance = 0;
            foreach(Round t in _rounds)
            {
                foreach(Match m in t.matches)
                {
                    if((m.home == c) && m.Played)
                    {
                        attendance += m.attendance;
                        i++;
                    }
                }
            }
            return i != 0 ? attendance/i : 0;
        }

        public Club Winner()
        {
            if(isChampionship)
            {
                List<Club> clubsFinalPhase = GetFinalPhasesClubs();
                if (clubsFinalPhase.Count > 0)
                {
                    return clubsFinalPhase[0];
                }
                else
                {
                    return _rounds[0].Winner();
                }
            }
            else
            {
                return _rounds[_rounds.Count - 1].Winner();
            }
        }

        public List<KeyValuePair<Player, int>> Goalscorers()
        {
            Dictionary<Player, int> goalscorers = new Dictionary<Player, int>();

            foreach(Round t in _rounds)
            {
                foreach(KeyValuePair<Player,int> kvp in t.GoalScorers())
                {
                    if (goalscorers.ContainsKey(kvp.Key))
                    {
                        goalscorers[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        goalscorers[kvp.Key] = kvp.Value;
                    }
                }
            }

            List<KeyValuePair<Player, int>> list = goalscorers.ToList();

            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            return list;
        }


        private void UpdateRecoverTeamsRound(Round oldRound, Round newRound)
        {
            foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                foreach(Round r in t.rounds)
                {
                    for(int i = 0; i < r.recuperedTeams.Count; i++)
                    {
                        RecoverTeams rt = r.recuperedTeams[i];
                        if (rt.Source == oldRound)
                        {
                            Console.WriteLine("Match entre " + rt.Source.ToString() + " et " + oldRound.ToString());
                            rt.Source = newRound;
                        }
                    }
                }
            }
        }

        public GroupInactiveRound DisableGroupRound(GroupsRound round, int i)
        {
            int groupCount = (round as GroupsRound != null) ? (round as GroupsRound).groupsCount : 1;
            bool qualificationsForAllGroups = (round as GroupsRound != null) ? (round as GroupsRound).qualificationsDefinedForAllGroup : false;
            GroupInactiveRound newRound = new GroupInactiveRound(Session.Instance.Game.kernel.NextIdRound(), round.name, this, round.programmation.defaultHour, new List<GameDay>(), new List<TvOffset>(), groupCount, qualificationsForAllGroups, 1, round.programmation.initialisation, round.programmation.end, -1, RandomDrawingMethod.Geographic, false, 0, 0, 0);
            newRound.rules.AddRange(round.rules);

            List<Qualification> qualificationsToAdd = new List<Qualification>(round.qualifications);

            if (round.groupsCount == 1)
            {
                qualificationsToAdd = round.AdaptQualificationsToRanking(round.qualifications, round.clubs.Count);
            }

            foreach (Qualification q in qualificationsToAdd)
            {
                if(qualificationsForAllGroups)
                {
                    newRound.qualifications.Add(q);
                }
                else
                {
                    for(int g = 0; g < groupCount; g++)
                    {
                        bool valid = q.qualifies == 0 || (q.qualifies < 0 && g < -q.qualifies) || (q.qualifies > 0 && g >= groupCount-q.qualifies);
                        if(valid)
                        {
                            int newRanking;
                            if (q.ranking > 0)
                            {
                                newRanking = ((q.ranking - 1) * groupCount + g) + 1;
                            }
                            else
                            {
                                newRanking = ((q.ranking + 1) * groupCount) - g - 1;
                            }
                            Qualification nq = new Qualification(newRanking, q.roundId, q.target, q.isNextYear, 0);
                            newRound.qualifications.Add(nq);
                        }
                    }
                }
            }
            foreach (RecoverTeams re in round.recuperedTeams)
            {
                newRound.recuperedTeams.Add(re);
            }

            foreach (Club c in round.clubs)
            {
                newRound.clubs.Add(c);
            }

            foreach (Prize d in round.prizes)
            {
                newRound.prizes.Add(d);
            }

            foreach (Rule r in round.rules)
            {
                newRound.rules.Add(r);
            }

            //UpdateRecoverTeamsRound(t, newRound);
            return newRound;
        }

        public void DisableTournament()
        {
            List<Round> newRounds = new List<Round>();
            int i = 0;

            foreach (Round t in _rounds)
            {
                if((t as KnockoutRound) == null)
                {
                    GroupInactiveRound newRound = DisableGroupRound(t as GroupsRound, i);
                    i++;
                    newRounds.Add(newRound);
                }
                else
                {
                    /*
                    int clubCount = round.clubs.Count;
                    foreach (Tournament otherTournaments in Session.Instance.Game.kernel.Competitions)
                    {
                        foreach (Round otherRound in otherTournaments.rounds)
                        {
                            foreach (Qualification otherQualifications in otherRound.qualifications)
                            {
                                if (otherQualifications.target.Tournament() == this && !otherQualifications.isNextYear && otherQualifications.roundId == i)
                                {
                                    clubCount++;
                                }
                            }
                        }
                    }
                    int numberOfGames = clubCount / 2;
                    for (int j = numberOfGames * (q.ranking - 1); j < numberOfGames * q.ranking; j++)
                    {
                        newRound.qualifications.Add(new Qualification(j + 1, q.roundId, q.target, q.isNextYear, q.qualifies));
                    }

                    */
                    newRounds.Add(t);
                }
            }

            foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                if (c != this)
                {
                    foreach (Round t in c.rounds)
                    {
                        //Duplicated code
                        for (int j = 0; j < t.recuperedTeams.Count; j++)
                        {
                            RecoverTeams re = t.recuperedTeams[j];
                            if (_rounds.Contains(re.Source))
                            {
                                Round oldSource = re.Source as Round;
                                int index = _rounds.IndexOf(re.Source as Round);
                                re.Source = newRounds[index];
                            }
                            t.recuperedTeams[j] = re;
                        }
                        for (int j = 0; j < t.baseRecuperedTeams.Count; j++)
                        {
                            RecoverTeams re = t.baseRecuperedTeams[j];
                            if (_rounds.Contains(re.Source))
                            {
                                Round oldSource = re.Source as Round;
                                int index = _rounds.IndexOf(re.Source as Round);
                                re.Source = newRounds[index];
                                if (c.name.Contains("Coupe de France"))
                                {
                                    Console.WriteLine("[" + c.name + "][" + t.name + "] Base Replace " + oldSource.name + "(" + oldSource.GetType() + ") to " + (re.Source as Round).name + " (" + re.Source.GetType() + ")");
                                }
                            }
                            t.baseRecuperedTeams[j] = re;
                        }
                    }
                }
            }

            _rounds.Clear();
            foreach (Round t in newRounds)
            {
                _rounds.Add(t);
            }

        }

        public bool IsInternational()
        {
            Association localisation = this.association;
            return localisation.ClosestStateAssociation() == null;
        }

        public bool IsInvolved(Club c)
        {
            bool res = false;
            foreach(Round r in rounds)
            {
                if(r.clubs.Contains(c))
                {
                    res = true;
                }
            }
            return res;
        }

        private bool ReservesAllowed()
        {
            bool reservesForbidden = false;
            foreach (Round r in _rounds)
            {
                reservesForbidden = reservesForbidden || r.rules.Contains(Rule.OnlyFirstTeams);
            }
            return !reservesForbidden;
        }

        private bool ChildAssociationsLeaguesAllowed()
        {
            Association a = this.association;
            bool res = false;
            foreach(Round r in _rounds)
            {
                foreach(RecoverTeams rt in r.recuperedTeams)
                {
                    Tournament source = rt.Source as Tournament;
                    res = res || (source != null && a.GetAllChilds().Contains(source.association));
                }
            }
            return res;
        }

        public void PrintCupResume()
        {
            Utils.Debug("[" + name + "]");
            foreach (Round r in rounds)
            {
                Utils.Debug(String.Format("-Résumé tour {0} | Initialisé le {1}, cloture le {2}- [{3} clubs, {4} matchs]", r.name, r.DateInitialisationRound().ToShortDateString(), r.DateEndRound().ToShortDateString(), r.clubs.Count, r.matches.Count));
                foreach (Match m in r.matches)
                {
                    Utils.Debug(m.ToString());
                }
            }
        }

        /// <summary>
        /// Check if tournament schedule is valid (especially useful for cups with variable rounds count)
        /// </summary>
        public bool CheckTournamentScheduleIsValid()
        {
            bool isValid = true;
            for(int i = 0; i < rounds.Count-1; i++)
            {
                Round round = rounds[i];
                Round nextRound = rounds[i + 1];
                isValid = isValid && Utils.DaysNumberBetweenTwoDates(round.DateInitialisationRound(), round.DateEndRound()) > 0;
                bool nextRoundNotTooClose = Utils.DaysNumberBetweenTwoDates(round.DateEndRound(), nextRound.DateInitialisationRound()) > 4;
                if(nextRoundNotTooClose)
                {
                    Utils.Debug(String.Format("[{0}] {1} is too close to {2} ({3}-{4})", name, round.name, nextRound.name, round.DateInitialisationRound().ToShortDateString(), nextRound.DateInitialisationRound().ToShortDateString()));
                }
                isValid = isValid && nextRoundNotTooClose;
            }
            return isValid;
        }
    }
}