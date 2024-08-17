using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Text;
using tm.Exportation;
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

    [DataContract(IsReference =true)]
    //[Owned]
    public class ParentTournament
    {
        [DataMember]
        public Association Association { get; set; }
        [DataMember]
        public Tournament Tournament { get; set; }

        public ParentTournament()
        {
            Association = null;
            Tournament = null;
        }

        public ParentTournament(Association association, Tournament tournament)
        {
            Association = association;
            Tournament = tournament;
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
        private ParentTournament _parent;

        public string name { get => _name; }
        public Color color => _color;
        public List<Round> rounds { get => _rounds; }
        public string logo { get => _logo; }
        public ClubStatus status => _status;

        public List<Club>[] nextYearQualified => _nextYearQualified;

        public List<Stadium> hostStadiums => _hostStadiums;
        public bool isHostedByOneCountry
        {
            get
            {
                bool isHostedByOneCountry = false;
                foreach (Round r in _rounds)
                {
                    if (r.rules.Contains(Rule.HostedByOneCountry))
                    {
                        isHostedByOneCountry = true;
                    }
                }
                return isHostedByOneCountry;
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
        public int level => _level;
        
        /// <summary>
        /// General rules applied to the tournament
        /// </summary>
        public List<TournamentRule> rules => _rules;

        public ParentTournament parent => _parent;

        public Tournament()
        {
            _rounds = new List<Round>();
            _previousEditions = new Dictionary<int, Tournament>();
            _hostStadiums = new List<Stadium>();
            _rules = new List<TournamentRule>();
        }

        public Tournament(int id, string name, string logo, GameDay seasonBeginning, string shortName, bool isChampionship, int level, int periodicity, int remainingYears, Color color, ClubStatus status, ParentTournament parent)
        {
            Id = id;
            _rounds = new List<Round>();
            _name = name;
            _logo = logo;
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
        public List<Country> Hosts()
        {
            List<Country> hosts = new List<Country>();
            foreach(Stadium stadium in _hostStadiums)
            {
                Country country = stadium.city.Country();
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
            List<Country> candidates = new List<Country>();
            //Find country
            foreach (Round r in _rounds)
            {
                foreach (Club c in r.clubs)
                {
                    Country candidate = c.Country();
                    if (candidate.stadiums.Count > 7)
                    {
                        candidates.Add(c.Country());
                    }
                }
            }

            Country host = candidates.Count > 0 ? candidates[Session.Instance.Random(0, candidates.Count)] : null;
            //Find stadiums
            if (host != null)
            {
                Console.WriteLine(host.Name() + " chosen to host " + _name);
                List<Stadium> stadiums = new List<Stadium>(host.stadiums);
                stadiums.Sort(new StadiumComparator());
                for (int i = 0; i < 8; i++)
                {
                    _hostStadiums.Add(stadiums[i]);
                    Console.WriteLine(stadiums[i].name + " - " + stadiums[i].capacity);
                }
                Console.WriteLine("=================================");
            }
        }

        public List<Tournament> GetChildTournaments()
        {
            List<Tournament> childTournaments = new List<Tournament>();
            foreach(Tournament t in Session.Instance.Game.kernel.LocalisationTournament(this).Tournaments())
            {
                if(t.parent.Tournament == this)
                {
                    childTournaments.Add(t);
                }
            }
            return childTournaments;
        }

        private int CountAdditionnalTeamsRound(int round, List<LeagueCupApparition> apparitions)
        {
            int res = 0;
            foreach(LeagueCupApparition lca in apparitions)
            {
                if(lca.apparitionRound == round)
                {
                    res += lca.teams;
                }
            }
            return res;
        }

        public RecoverTeams GetOtherRecoverTeamsOfRound(RecoverTeams rt)
        {
            RecoverTeams res = new RecoverTeams(null, 0, RecuperationMethod.Best);
            foreach (Round round in rounds)
            {
                foreach (RecoverTeams recover in round.recuperedTeams)
                {
                    if (recover.Method != rt.Method && recover.Source == rt.Source)
                    {
                        res = recover;
                    }
                }
            }
            return res;
        }


        private class LeagueCupApparition
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

        public RecoverTeams GetRecoverTeams(Tournament league, bool getBestTeamsRecover)
        {
            RecoverTeams res = new RecoverTeams(null, -1, RecuperationMethod.Best);
            foreach (Round r in _rounds)
            {
                for (int k = 0; k < r.recuperedTeams.Count; k++)
                {
                    RecoverTeams rt = r.recuperedTeams[k];
                    bool isBestTeams = r.recuperedTeams[k].Method == RecuperationMethod.Best;
                    if ((rt.Source as Round).Tournament == league && (getBestTeamsRecover == isBestTeams || GetOtherRecoverTeamsOfRound(rt).Source == null))
                    {
                        res = rt;
                    }
                }
            }
            return res;
        }

        public void UpdateRecuperedTeams(int newTeamsCount, Tournament league, bool updateBestTeamsRecover, bool addUp)
        {
            foreach (Round r in _rounds)
            {
                for (int k = 0; k < r.recuperedTeams.Count; k++)
                {
                    RecoverTeams rt = r.recuperedTeams[k];
                    bool isBestTeams = rt.Method == RecuperationMethod.Best;
                    if ((rt.Source as Round).Tournament == league && (updateBestTeamsRecover == isBestTeams || GetOtherRecoverTeamsOfRound(rt).Source == null))
                    {
                        Console.WriteLine(String.Format("UpdateRecuperedTeams with {0} new teams from {1}", addUp ? (rt.Number + newTeamsCount) : newTeamsCount, league.name));
                        rt.Number = addUp ? (rt.Number + newTeamsCount) : newTeamsCount;
                        r.recuperedTeams[k] = rt;
                    }
                }
            }
        }

        public int[] GetTeamsAtEachRound()
        {
            int[] teamsAtEachRound = new int[rounds.Count];
            Country country = Session.Instance.Game.kernel.LocalisationTournament(this) as Country;
            List<Tournament> otherTournaments = country != null ? country.Leagues() : Session.Instance.Game.kernel.Competitions;
            foreach (Tournament t in otherTournaments)
            {
                List<Round> rounds = t != this ? t.rounds : new List<Round>() { t.rounds[0] };
                int i = 0;
                foreach (Round r in rounds)
                {
                    foreach (Qualification q in r.qualifications)
                    {
                        if (!q.isNextYear && q.tournament == this)
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
                    if (q.isNextYear && ((relegation && q.tournament.level > level) || (!relegation && q.tournament.level < level)))
                    {
                        res += games;
                    }
                    else if (!q.isNextYear && q.tournament == this)
                    {
                        teamsAtEachRound[q.roundId] += games;
                    }
                }
            }
            return res;
        }

        private void GetBestTeams(List<RecoverTeams> recoverTeams, int count, bool selectWorstTeams, ref List<RecoverTeams> bestTeams, ref List<RecoverTeams> worstTeams, ref int missingTeams)
        {
            List<RecoverTeams> apps = new List<RecoverTeams>();
            foreach(RecoverTeams rt in recoverTeams)
            {
                apps.Add(new RecoverTeams(rt.Source, rt.Number, rt.Method));
            }
            apps.Sort((a, b) => ( (a.Source as Round).Tournament.level - (b.Source as Round).Tournament.level) * (selectWorstTeams ? -1 : 1));
            List<RecoverTeams> best = new List<RecoverTeams>();
            List<RecoverTeams> worst = new List<RecoverTeams>();
            foreach(RecoverTeams rt in apps)
            {
                worst.Add(rt);
            }
            int currentCount = 0;
            int i = 0;
            while(currentCount < count && i < apps.Count)
            {
                RecuperationMethod appRecuperationMethod = apps[i].Method;
                int appTeamsCount = TeamsCount(apps[i]);
                int teamsToTake = appTeamsCount < (count - currentCount) ? appTeamsCount : (count - currentCount);
                Utils.Debug("Récupère " + teamsToTake + " équipes de Ligue " + (apps[i].Source as Round).Tournament.name + " (actuellement " + currentCount + " sur " + count + ")");

                best.Add(new RecoverTeams(apps[i].Source, teamsToTake, appRecuperationMethod));
                //RecuperationMethod rm = selectWorstTeams ? appRecuperationMethod : (appRecuperationMethod == RecuperationMethod.NotQualifiedForInternationalCompetitionBest ? RecuperationMethod.NotQualifiedForInternationalCompetitionWorst : RecuperationMethod.Worst);
                worst[i] = new RecoverTeams(worst[i].Source, appTeamsCount - teamsToTake, worst[i].Method);
                currentCount += teamsToTake;
                i += 1;
            }
            bestTeams = selectWorstTeams ? worst : best;
            worstTeams = selectWorstTeams ? best : worst;
            missingTeams = count - currentCount;

            for (int j = 0; j < worstTeams.Count; j++)
            {
                foreach (RecoverTeams rt in bestTeams)
                {
                    if (rt.Number > 0 && rt.Source == worstTeams[j].Source)
                    {
                        worstTeams[j] = new RecoverTeams(worstTeams[j].Source, worstTeams[j].Number, rt.Method == RecuperationMethod.NotQualifiedForInternationalCompetitionBest ? RecuperationMethod.NotQualifiedForInternationalCompetitionWorst : RecuperationMethod.Worst);
                    }
                }
            }

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

        private int TeamsCount(RecoverTeams rt)
        {
            int res = 0;
            if (rt.Method == RecuperationMethod.Best || rt.Method == RecuperationMethod.Worst || rt.Method == RecuperationMethod.Randomly)
            {
                res = rt.Number;
                //res = (this.parent.Key == null) ? rt.Number : rt.Source.RetrieveTeams(-1, rt.Method, true, parent.Key).Count; //Take into account teams count variation due to administrative divisions (in remplacement of res = rt.Number);
            }
            else if (rt.Method == RecuperationMethod.QualifiedForInternationalCompetition || rt.Method == RecuperationMethod.NotQualifiedForInternationalCompetitionWorst || rt.Method == RecuperationMethod.NotQualifiedForInternationalCompetitionBest || rt.Method == RecuperationMethod.StatusPro)
            {
                res = rt.Source.RetrieveTeams(-1, rt.Method, false, parent.Association).Count;
            }
            return res;
        }

        /// <summary>
        /// Update league cup qualifications due to clubs qualified for international competitions and professionnals teams count changes
        /// Regional competitions (with parent.key != null) and national league cup competitions (with teams appearing in function of their international qualifications) are incompatible
        /// </summary>
        public void UpdateLeagueCupQualifications()
        {
            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(this);
            //Contains new league apparition through competition for each rounds
            List<List<RecoverTeams>> newRecoverTeams = new List<List<RecoverTeams>>();
            //Contains new extra rounds created if necessary
            List<Round> newRounds = new List<Round>();

            List<int> leagueLevelsRepresented = new List<int>();

            if (parent.Association != null && (localisation as Country) != null)
            {
                int levelsCount = (localisation as Country).Leagues().Count;
                int[] teamsByLevel = new int[levelsCount];
                for(int i = 0; i < levelsCount; i++)
                {
                    teamsByLevel[i] = 0;
                }
                for(int i = 0; i < _rounds.Count; i++)
                {
                    Round round = _rounds[i];
                    for(int j = round.recuperedTeams.Count-1; j >= 0; j--)
                    {
                        RecoverTeams rt = round.recuperedTeams[j];
                        int newTeamsCount = rt.Method == RecuperationMethod.Best ? rt.Source.RetrieveTeams(-1, rt.Method, true, parent.Association).Count : 0;
                        if(newTeamsCount > 0)
                        {
                            round.recuperedTeams[j] = new RecoverTeams(rt.Source, newTeamsCount, rt.Method);
                        }
                        else
                        {
                            round.recuperedTeams.RemoveAt(j);
                        }

                        if(rt.Source as Round != null)
                        {
                            leagueLevelsRepresented.Add((rt.Source as Round).Tournament.level);
                        }
                    }
                }
            }
            
            //First phase : go through tournament qualifications with the new league system to check if adaptations must be made
            List<int> teamsRounds = new List<int>();
            int currentTeams = 1;
            int newTeams = 0;
            for(int i = _rounds.Count-1; i>=0; i--)
            {
                Round round = _rounds[i];
                currentTeams = currentTeams * 2;
                newTeams = 0;
                foreach(RecoverTeams app in round.recuperedTeams)
                {
                    int newTeamsRound = TeamsCount(app);
                    newTeams += newTeamsRound;
                }
                if (currentTeams <= newTeams && i > 0)
                {
                    foreach(RecoverTeams rt in round.recuperedTeams)
                    {
                        rounds[i - 1].recuperedTeams.Add(rt);
                    }
                    round.recuperedTeams.Clear();
                    newTeams = 0;
                }
                newRecoverTeams.Add(new List<RecoverTeams>(round.recuperedTeams));
                teamsRounds.Add(currentTeams);
                currentTeams = currentTeams - newTeams;
            }

            //Replace old RecoverTeams with new RecoverTeams with actual count of clubs (non continental or continental)
            
            //Case national League Cup
            if(parent.Association == null)
            {
                foreach (List<RecoverTeams> lrt in newRecoverTeams)
                {
                    for (int i = 0; i < lrt.Count; i++)
                    {
                        if (lrt[i].Method == RecuperationMethod.NotQualifiedForInternationalCompetitionWorst || lrt[i].Method == RecuperationMethod.NotQualifiedForInternationalCompetitionBest || lrt[i].Method == RecuperationMethod.QualifiedForInternationalCompetition || lrt[i].Method == RecuperationMethod.StatusPro)
                        {
                            lrt[i] = new RecoverTeams(lrt[i].Source, TeamsCount(lrt[i]), lrt[i].Method);
                        }
                    }
                }
            }

            currentTeams = -currentTeams;
            int roundCreated = 0;
            //Trois cas
            //Case 1 : currentTeams == 0
            //Parfait
            //Case 2 : currentTeams < 0
            //Pas assez d'équipes pour compléter les premiers tours, certaines équipes sont remontées au tour suivant.
            //Ex. Coupe de la Ligue : certaines équipes de L2 entrent directement au deuxième tour
            //Case 3 : currentTeams > 0
            //Trop d'équipes pour le nombre de places aux tours suivants : création d'un nouveau tour
            Utils.Debug("[currentTeams] " + currentTeams);
            if (currentTeams == 0)
            {
                Utils.Debug("Parfait !");
            }
            else if(currentTeams < 0)
            {
                Utils.Debug("Pas assez d'équipes pour compléter les premiers tours, certaines équipes sont remontées au tour suivant");
                int teamsToMoveUp = -currentTeams;
                int roundId = newRecoverTeams.Count - 1;
                //Start with first round
                while(teamsToMoveUp > 0)
                {
                    List<RecoverTeams> bestTeams = new List<RecoverTeams>();
                    List<RecoverTeams> worstTeams = new List<RecoverTeams>();
                    int missingTeams = 0;
                    //According to the count of teams to move up, some teams are moved up (bestTeams) and the other remains on the first round (worstTeams)
                    GetBestTeams(newRecoverTeams[roundId], teamsToMoveUp, false, ref bestTeams, ref worstTeams, ref missingTeams);
                    //If additionnal teams need to be promoted to next round but can't because all teams are already moved up
                    //In this case the same operation is made again with round n°1 to round n°2...
                    teamsToMoveUp = missingTeams;
                    foreach(RecoverTeams bt in bestTeams)
                    {
                        newRecoverTeams[roundId - 1].Add(bt);
                    }
                    newRecoverTeams[roundId] = worstTeams;
                    roundId -= 1;
                    teamsToMoveUp = teamsToMoveUp / 2;
                }
            }
            else if(currentTeams > 0)
            {
                int teamsToAdd = currentTeams * 2; //New round : double teams from qualified teams for the "old first round"
                Utils.Debug("Trop d'équipes pour le nombre de places aux tours suivants : création d'un nouveau tour");
                List<GameDay> availableDatesAll = (localisation as Country).GetAvailableCalendarDates(this.parent.Association == null, 2, leagueLevelsRepresented, true, false);
                List<GameDay> availableDates = new List<GameDay>();
                int beginningCompetition = this._seasonBeginning.WeekNumber;
                int beginningRounds = rounds.First().programmation.initialisation.WeekNumber;
                //Filter to get available dates to play the new round
                foreach (GameDay ad in availableDatesAll)
                {
                    //Les dates sont centrées sur le début de la compétition -> pas de problème en cas de passage d'une année à l'autre (semaines 52 puis semaine 02 par ex.)
                    int absoluteAd = Utils.Modulo(ad.WeekNumber - beginningCompetition, 53); // TODO: Des fois 52 ou 53 semaines !
                    int absoluteBeginFirstRound = Utils.Modulo(beginningRounds - beginningCompetition, 53); // TODO: Des fois 52 ou 53 semaines !

                    if (absoluteAd < absoluteBeginFirstRound && absoluteAd > 0)
                    {
                        availableDates.Add(ad);
                    }
                }

                /*Console.WriteLine("availableDates");
                foreach (GameDay dd in availableDates)
                {
                    Console.WriteLine(dd.WeekNumber);
                }*/


                while (teamsToAdd > 0)
                {
                    List<RecoverTeams> bestTeams = new List<RecoverTeams>();
                    List<RecoverTeams> worstTeams = new List<RecoverTeams>();
                    int missingTeams = 0;
                    //According to the teams numbers to add to an extra round, some teams begin tournament at the current round (bestTeams) and the other play the extra round (worstTeams)
                    GetBestTeams(newRecoverTeams.Last(), teamsToAdd, true, ref bestTeams, ref worstTeams, ref missingTeams);
                    //If new rounds can't play all teams, so the missing teams are the new qualified teams from a second extra round etc.
                    teamsToAdd = missingTeams;
                    newRecoverTeams[newRecoverTeams.Count - 1] = bestTeams;
                    newRecoverTeams.Add(worstTeams);
                    Round firstRound = this.rounds[0];
                    List<GameDay> newRoundTimes = new List<GameDay>();
                    int dateIndex = (availableDates.Count - 2) - (3 * roundCreated);
                    foreach (GameDay dt in firstRound.programmation.gamesDays)
                    {
                        newRoundTimes.Add(new GameDay(availableDates[dateIndex].WeekNumber, dt.MidWeekGame, dt.YearOffset, dt.DayOffset));
                    }
                    newRounds.Add(new KnockoutRound(Session.Instance.Game.kernel.NextIdRound(), "Tour préliminaire", this, firstRound.programmation.defaultHour, newRoundTimes, new List<TvOffset>(), firstRound.phases, new GameDay(availableDates[dateIndex].WeekNumber - 1, true, firstRound.programmation.initialisation.YearOffset, firstRound.programmation.initialisation.DayOffset), new GameDay(availableDates[dateIndex].WeekNumber + 1, firstRound.programmation.initialisation.MidWeekGame, firstRound.programmation.initialisation.YearOffset, firstRound.programmation.initialisation.DayOffset), RandomDrawingMethod.Random, false, firstRound.programmation.gamesPriority)); ;
                    newRounds[newRounds.Count - 1].recuperedTeams.AddRange(worstTeams);
                    newRounds[newRounds.Count - 1].rules.AddRange(firstRound.rules);
                    newRounds[newRounds.Count - 1].qualifications.Add(new Qualification(1, 1, this, false, -1));
                    roundCreated++;
                    if (teamsToAdd > 0)
                    {
                        teamsToAdd *= 2;
                    }
                }
            }

            //Set extraRounds to keep trace of extra rounds (needed when reset the tournament)
            _extraRounds = roundCreated;
            foreach (Round newRound in newRounds)
            {
                rounds.Insert(0, newRound);
            }

            //Set newRecoverTeams to be the official league apparition in the tournament now
            bool nonInternationalWorstAppeared = false;
            for (int i = 0; i < rounds.Count; i++)
            {
                rounds[i].recuperedTeams.Clear();
                //TODO: Is this loop really needed ? Use case : CDL with 15 continental teams
                for(int j = 0; j < newRecoverTeams[rounds.Count - i - 1].Count; j++)
                {
                    if(newRecoverTeams[rounds.Count - i - 1][j].Method == RecuperationMethod.NotQualifiedForInternationalCompetitionWorst)
                    {
                        if(!nonInternationalWorstAppeared)
                        {
                            nonInternationalWorstAppeared = true;
                        }
                        else
                        {
                            newRecoverTeams[rounds.Count - i - 1][j] = new RecoverTeams(newRecoverTeams[rounds.Count - i - 1][j].Source, newRecoverTeams[rounds.Count - i - 1][j].Number, RecuperationMethod.NotQualifiedForInternationalCompetitionBest);
                        }
                    }
                }
                rounds[i].recuperedTeams.AddRange(newRecoverTeams[rounds.Count-i-1]);
            }

            // Qualifications to next rounds are recalibrated in case of extra rounds (because old second round in the tournament doesn't refer to the same round now)
            for(int i = 0; i < rounds.Count; i++)
            {
                for(int j = 0; j < rounds[i].qualifications.Count; j++)
                {
                    int newRoundId = i < roundCreated ? (i + 1) : (rounds[i].qualifications[j].roundId + roundCreated);
                    rounds[i].qualifications[j] = new Qualification(rounds[i].qualifications[j].ranking, newRoundId, rounds[i].qualifications[j].tournament, rounds[i].qualifications[j].isNextYear, rounds[i].qualifications[j].qualifies);
                }
            }
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
                foreach (KeyValuePair<Association, int> kvp in pivotRound.teamsByAssociation)
                {
                    //Copie du tournoi est créée
                    Tournament regionalTournament = CopyForArchive(false);
                    regionalTournament._name = string.Format("{0} - {1}", regionalTournament, kvp.Key.name);
                    regionalTournament._level = 1000; //Un niveau exagérément élevé est mis pour éviter d'aller chercher les vainqueurs de ces compétitions pour les places européennes par exemple
                    regionalTournament._parent = new ParentTournament(kvp.Key, this); //Cette compétition dépend du tournoi principal
                    regionalTournament.InitializeQualificationsNextYearsLists();
                    for (int id = rounds.Count-1; id >= idRoundPivot; id--)
                    {
                        regionalTournament.rounds.RemoveAt(id);
                    }
                    //Le dernier tour doit qualifier à la compétition principale, CopyForArchive ayant reporté les autres qualifications à la nouvelle compétition
                    for (int i = 0; i < regionalTournament.rounds.Last().qualifications.Count; i++)
                    {
                        Qualification qualification = regionalTournament.rounds.Last().qualifications[i];
                        if (qualification.tournament == regionalTournament && !qualification.isNextYear)
                        {
                            regionalTournament.rounds.Last().qualifications[i] = new Qualification(qualification.ranking, qualification.roundId, this, qualification.isNextYear, qualification.qualifies);
                        }
                    }
                    Session.Instance.Game.kernel.LocalisationTournament(this).Tournaments().Add(regionalTournament);
                    regionalTournament.UpdateCupQualifications();
                }
                //Dans un premier temps pour éviter les rework qualifs et qualifs outre-mer ne pas supprimer les premiers tours mais les équipes qui y rentrent
                for(int i = 0; i < idRoundPivot; i++)
                {
                    rounds[i].recuperedTeams.Clear();
                }
            }
        }

        /// <summary>
        /// Update national cup qualifications due to annual league structure modifications
        /// </summary>
        public void UpdateCupQualifications()
        {
            Utils.Debug(Session.Instance.Game.date.ToShortDateString() + " [UpdateCupQualifications " + name + "] (" + parent.Association + ")");
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
                List<Tournament> childTournaments = GetChildTournaments();
                foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if (!t.IsInternational() && t != this && t.parent.Tournament != this)
                    {
                        foreach (Round r in t.rounds)
                        {
                            for (int i = 0; i < r.qualifications.Count; i++)
                            {
                                if (r.qualifications[i].tournament.parent.Tournament == this || (r.qualifications[i].tournament == this && r.qualifications[i].roundId < idRoundPivot))
                                {
                                    Tournament hostTournament = childTournaments[Session.Instance.Random(0, childTournaments.Count)];
                                    Utils.Debug(string.Format("[Host Cup] {0} send winner of {1} to {2}", t.name, r.name, hostTournament.name));
                                    r.qualifications[i] = new Qualification(r.qualifications[i].ranking, r.qualifications[i].roundId, hostTournament, r.qualifications[i].isNextYear, r.qualifications[i].qualifies);
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
                    if(rt.Method == RecuperationMethod.QualifiedForInternationalCompetition || rt.Method == RecuperationMethod.NotQualifiedForInternationalCompetitionWorst || rt.Method == RecuperationMethod.NotQualifiedForInternationalCompetitionBest)
                    {
                        leagueCupLike = true;
                    }
                }
            }
            int[] teamsFromOutsideLeagueSystem = new int[_rounds.Count];
            if (leagueCupLike || (this.parent.Tournament == null && this.parent.Association != null)) //Regional cup are updated following league cup algorithm
            {
                UpdateLeagueCupQualifications();
            }
            else
            {
                List<LeagueCupApparition> leagueCupApparitions = new List<LeagueCupApparition>();
                for (int i = 0; i < _rounds.Count; i++)
                {
                    teamsFromOutsideLeagueSystem[i] = 0;
                }

                //Garde en mémoire les équipes qui participent à la compétition sans participer aux ligues (cas des équipes outre-mer en coupe de France) afin de garder leurs places.
                //On ne considère pas ici les chemins régionaux (t.parent.Value != null) car seront comptés après à l'aide des attributs Round.teamsByAssociation pour chaque tour
                foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if (!t.IsInternational() && t != this && t.parent.Tournament != this)
                    {
                        foreach (Round r in t.rounds)
                        {
                            foreach (Qualification q in r.qualifications)
                            {
                                if (q.tournament == this)
                                {
                                    teamsFromOutsideLeagueSystem[q.roundId] += (q.qualifies != 0 ? q.qualifies : 1);
                                }
                            }
                        }
                    }
                }
                foreach(Round r in this.rounds)
                {
                    foreach (KeyValuePair<Association, int> regionalPath in r.teamsByAssociation)
                    {
                        teamsFromOutsideLeagueSystem[rounds.IndexOf(r)] += regionalPath.Value;
                    }
                }

                foreach (int i in teamsFromOutsideLeagueSystem) Console.WriteLine("[Update " + this.name + "] teamsFromOutsideLeagueSystem round " + i);

                //Obtient le nombre d'équipes une fois toutes les ligues entrées dans la compétition (64 en Coupe de France)
                //Dans le cas d'une phase qualificative régionale avant la phase nationale, 
                int teamsAtTheLastRound = 2;
                if(parent.Tournament != null)
                {
                    Association concernedRegion = parent.Association;
                    foreach(Round r in parent.Tournament.rounds)
                    {
                        if(r.teamsByAssociation.ContainsKey(concernedRegion))
                        {
                            teamsAtTheLastRound = r.teamsByAssociation[concernedRegion] * 2;
                        }
                    }
                }

                int teamsKnockout = 0;
                int j = 0;
                for (int i = _rounds.Count - 1; i >= 0 && teamsKnockout == 0; i--)
                {
                    Round r = _rounds[i];
                    if (r.recuperedTeams.Count > 0)
                    {
                        teamsKnockout = teamsAtTheLastRound * (int)Math.Pow(2, j);
                    }
                    j++;
                }

                //Crée la liste qui résume à quel moment chaque division entre dans la compétition et combien d'équipes entrent par division
                foreach (Round r in _rounds)
                {
                    List<RecoverTeams> recovers = new List<RecoverTeams>(r.recuperedTeams);
                    foreach (RecoverTeams rt in recovers)
                    {
                        Round rtRound = rt.Source as Round;
                        if (rtRound != null)
                        {
                            int roundsClubCount = _parent.Association == null ? rtRound.CountWithoutReserves() : rtRound.CountWithoutReserves(_parent.Association);
                            int clubsCount = roundsClubCount;
                            RecoverTeams otherRecoverTeams = GetOtherRecoverTeamsOfRound(rt);
                            if (otherRecoverTeams.Source != null)
                            {
                                clubsCount = otherRecoverTeams.Method == RecuperationMethod.Worst ? -rt.Number : Math.Max(0, clubsCount - otherRecoverTeams.Number);
                            }
                            //Du au changement de structure de ligue d'une année sur l'autre, éviter qu'on demande à une ligue plus d'équipe qu'elle n'en a
                            if(clubsCount > roundsClubCount)
                            {
                                clubsCount = roundsClubCount;
                            }
                            if (clubsCount < -roundsClubCount)
                            {
                                clubsCount = -roundsClubCount;
                            }
                            leagueCupApparitions.Add(new LeagueCupApparition(clubsCount, rounds.IndexOf(r), rtRound.Tournament));
                        }
                    }
                }

                //Trie la liste en fonction du niveau de la compétition (et si jamais une ligue entre dans la compétition en deux fois avec meilleurs/plus mauvaises équipes)
                leagueCupApparitions.Sort((a, b) => (a.tournament.level != b.tournament.level ? a.tournament.level - b.tournament.level : (a.teams > 0 ? -1 : 0)));

                //roundStart désigne le tour où les dernières équipes entrent en compétition
                int roundStart = 0;
                foreach (LeagueCupApparition lca in leagueCupApparitions)
                {
                    Console.WriteLine("[" + name + "]" + lca.tournament.name + ", " + lca.teams + " équipes au tour " + lca.apparitionRound + "(Best ? : " + lca.isBestTeams + ")");
                    roundStart = lca.apparitionRound > roundStart ? lca.apparitionRound : roundStart;
                }
                
                bool noTeamsCongestion = false;
                
                int currentTeamsCount = 0;
                int additionalTeams = 0;

                //Remonte la compétition. Si une fois remonté au premier tour le nombre d'équipes en lice dans la compétition est négatif,
                // c'est qu'il y a trop d'équipes qui sont entrés dans la compétition en haut. On recherche donc en premier lieu des ligues qui entrent deux fois
                // dans la compétition pour ne la faire entrer qu'une fois au tour le plus bas. 
                //TODO: Ajouter une troisième condition à l'étape suivante qui prend en compte ce cas de figure ?
                while (!noTeamsCongestion)
                {
                    currentTeamsCount = teamsKnockout - teamsFromOutsideLeagueSystem[roundStart];
                    additionalTeams = 0;
                    //Remonte la compétition pour récupérer le nombre d'équipes à ajouter au premier tour
                    for (int i = roundStart - 1; i > -1; i--)
                    {
                        //We remove all teams that will be added to the next round
                        additionalTeams = CountAdditionnalTeamsRound(i + 1, leagueCupApparitions);
                        additionalTeams += teamsFromOutsideLeagueSystem[i + 1]; // currentTeamsCount -= teamsFromOutsideLeagueSystem[i];
                        currentTeamsCount = (currentTeamsCount - additionalTeams) * 2;
                        Console.WriteLine("[Round " + i + " (" + _rounds[i].name + ")] Will add " + additionalTeams + " teams. " + currentTeamsCount + " teams (" + currentTeamsCount / 2 + " matchs). Removed " + teamsFromOutsideLeagueSystem[i] + " teams");
                    }

                    //Trop d'équipes sont entrées en compétition, on fait remonter d'un tour les équipes de la plus basse division qui entrent le plus tardivement dans la compétition, tant qu'il n'y a pas de place pour les équipes qui entrent au premier tour
                    if (currentTeamsCount < 0)
                    {
                        Console.WriteLine("[Cas currentTeamsCount < 0]");

                        LeagueCupApparition lca = leagueCupApparitions.OrderByDescending(l => l.apparitionRound).ThenByDescending(l => l.tournament.level).FirstOrDefault();
                        if(lca != default(LeagueCupApparition) && lca.apparitionRound > 0)
                        {
                            //Met à jour la LeagueCupApparition correspondante
                            lca.apparitionRound -= 1;
                            Round r = _rounds[lca.apparitionRound + 1];
                            Round rLca = _rounds[lca.apparitionRound];
                            bool ok = false;
                            RecoverTeams swapped = default(RecoverTeams);
                            for (int i = 0; i < r.recuperedTeams.Count && !ok; i++)
                            {
                                if ((r.recuperedTeams[i].Source as Round).Tournament == lca.tournament)
                                {
                                    ok = true;
                                    swapped = r.recuperedTeams[i];
                                    rLca.recuperedTeams.Add(r.recuperedTeams[i]);
                                    r.recuperedTeams.RemoveAt(i);
                                }
                            }
                            ok = false;
                            //Met à jour les RecoverTeams après la modification
                            for(int i = 0; i < rLca.recuperedTeams.Count && !ok; i++)
                            {
                                RecoverTeams t = rLca.recuperedTeams[i];
                                if (!t.Equals(swapped) && t.Source == swapped.Source)
                                {
                                    ok = true;
                                    int teams = swapped.Number + t.Number;
                                    if (t.Method == RecuperationMethod.Worst)
                                    {
                                        rLca.recuperedTeams[rLca.recuperedTeams.Count - 1] = new RecoverTeams(swapped.Source, teams, swapped.Method);
                                        rLca.recuperedTeams.RemoveAt(i);
                                    }
                                    else if(swapped.Method == RecuperationMethod.Worst)
                                    {
                                        rLca.recuperedTeams[i] = new RecoverTeams(t.Source, teams, t.Method);
                                        rLca.recuperedTeams.RemoveAt(rLca.recuperedTeams.Count - 1);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        noTeamsCongestion = true;
                    }
                }

                additionalTeams = CountAdditionnalTeamsRound(0, leagueCupApparitions);
                Utils.Debug(additionalTeams + " >= " + currentTeamsCount + " ?");
                //Petit fix pour corriger les approximation du ratio. Une équipe est retirée à la plus basse division
                if (additionalTeams - currentTeamsCount == 1)
                {
                    Utils.Debug("[Fix applied]");
                    int minApp = -1;
                    int maxLeagueLevel = -1;
                    int index = 0;
                    int minIndex = 0;
                    foreach(LeagueCupApparition lca in leagueCupApparitions)
                    {
                        bool selectedLca = minApp == -1 || (lca.apparitionRound < minApp || (lca.apparitionRound == minApp && lca.tournament.level > maxLeagueLevel));
                        if(selectedLca)
                        {
                            minIndex = index;
                            minApp = lca.apparitionRound;
                            maxLeagueLevel = lca.tournament.level;
                        }
                        index++;
                    }
                    leagueCupApparitions[minIndex].teams--;
                    //leagueCupApparitions[leagueCupApparitions.Count - 1].teams--;
                }

                //On a suffisament d'équipes dans les divisions qui entrent au premier tour, on sélectionne alors n équipes parmis ces ligues
                if (additionalTeams >= currentTeamsCount)
                {
                    Utils.Debug("Suffisament d'équipes disponibles pour les exigences du tour");

                    //On garde le nombre d'équipes maximales des ligues sans équipes réserves : leur structure ne changera pas avec les années, on garde tout (ex. le N1 au 5ème tour avec les 18 équipes au lieu de 10 équipes calculés avec la méthode du ratio)
                    int lastLevelWithoutReserves = (Session.Instance.Game.kernel.LocalisationTournament(this) as Country).GetLastLeagueLevelWithoutReserves();
                    List<LeagueCupApparition> lcaAddedByAnticipation = new List<LeagueCupApparition>();
                    foreach (LeagueCupApparition lca in leagueCupApparitions)
                    {
                        if (lca.apparitionRound == 0 && lca.tournament.level <= lastLevelWithoutReserves)
                        {
                            currentTeamsCount -= lca.teams;
                            additionalTeams -= lca.teams;
                            UpdateRecuperedTeams(lca.teams, lca.tournament, lca.isBestTeams, false);
                            lcaAddedByAnticipation.Add(lca);
                        }
                    }
                    foreach (LeagueCupApparition lca in lcaAddedByAnticipation)
                    {
                        leagueCupApparitions.Remove(lca);
                    }
                    //Ratio d'équipes à prendre dans chaque ligue pour obtenir le nombre d'équipes ciblé
                    double ratio = additionalTeams / (currentTeamsCount + 0.0);
                    int currentTeamsAdded = 0;

                    for (int i = 0; i < leagueCupApparitions.Count; i++)
                    {
                        int teamsToSelectFromLeague = leagueCupApparitions[i].apparitionRound == 0 ? (int)Math.Round(leagueCupApparitions[i].teams / ratio) : leagueCupApparitions[i].teams;
                        currentTeamsAdded += leagueCupApparitions[i].apparitionRound == 0 ? teamsToSelectFromLeague : 0;
                        Console.WriteLine(String.Format("Must update {0} with {1} teams at round {2}, (is best teams ? {3})", leagueCupApparitions[i].tournament.name, teamsToSelectFromLeague, leagueCupApparitions[i].apparitionRound, leagueCupApparitions[i].isBestTeams));
                        UpdateRecuperedTeams(teamsToSelectFromLeague, leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams, false);
                    }
                    //Dans le cas où le calcul à partir du ratio n'a pas fonctionné (ça ne tombe pas rond), alors on fixe en ajoutant ou retirant manuellement un ou plusieurs équipes pour obtenir le bon compte
                    if (currentTeamsAdded != currentTeamsCount)
                    {
                        int margin = currentTeamsCount - currentTeamsAdded;
                        Console.WriteLine("Marge : " + margin + " avec " + currentTeamsCount + " equipes a atteindres et " + currentTeamsAdded + " equipes ajoutés");
                        for (int i = 0; i < leagueCupApparitions.Count && margin != 0; i++)
                        {
                            int currentlyAddedTeamFromLeague = 0;
                            RecoverTeams rt = GetRecoverTeams(leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams);
                            if (rt.Source != null && leagueCupApparitions[i].apparitionRound == 0)
                            {
                                currentlyAddedTeamFromLeague = rt.Number;
                            }
                            if (leagueCupApparitions[i].apparitionRound == 0 && currentlyAddedTeamFromLeague + margin <= leagueCupApparitions[i].teams)
                            {
                                UpdateRecuperedTeams(margin, leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams, true);
                                margin = 0;
                            }
                        }
                    }
                }
                //On a pas suffisament d'équipes dans les ligues inférieures qui entrent au premier tour, on supprime ce tour pour passer directement au suivant (voir plus loin si on a toujours pas suffisament d'équipes) en simulant n qualifiés parmis les équipes des ligues inférieures
                else
                {
                    Console.WriteLine("Trop peu d'équipes disponibles pour les exigences du tour, obliger de supprimer des tours");
                    int delRoundId = 0;
                    bool ok = false;
                    while (!ok)
                    {
                        Console.WriteLine("Suppression du tour ID " + delRoundId);
                        //On supprime un tour en plus, on calcule le nombre d'équipe censée s'être qualifié à ce tour (à partir du tour précédéant) et le nombre total d'équipe à ce tour (en ajoutant celles qui arrivent à ce tour)
                        delRoundId++;
                        int expectedQualified = currentTeamsCount / 2;
                        int expectedRoundTeams = expectedQualified + CountAdditionnalTeamsRound(delRoundId, leagueCupApparitions);
                        Console.WriteLine("[Tour " + delRoundId + "] On attends " + expectedQualified + " qualifiés + " + CountAdditionnalTeamsRound(delRoundId, leagueCupApparitions) + " nouvelles équipes = " + expectedRoundTeams + " équipes à ce tour");
                        int totalDispoTeams = 0;
                        for (int i = 0; i < delRoundId + 1; i++)
                        {
                            totalDispoTeams += CountAdditionnalTeamsRound(i, leagueCupApparitions);
                        }
                        Console.WriteLine(totalDispoTeams + " équipes disponibles pour le tour " + delRoundId);
                        //Si le nombre d'équipes disponibles (c'est à dire l'intégralité des équipes des tours en lice à ce moment de la compétition) est supérieur au nombre nécessaire, alors c'est bon on débute la compétition à ce tour
                        if (totalDispoTeams > expectedRoundTeams)
                        {
                            ok = true;
                            int totalApp = 0;
                            for (int i = 0; i < delRoundId; i++)
                            {
                                totalApp += CountAdditionnalTeamsRound(i, leagueCupApparitions);
                            }
                            //Créé les qualifications des ligues dans la coupe
                            List<RecoverTeams> newRecoverTeams = new List<RecoverTeams>();

                            /*//On garde le nombre d'équipes maximales des ligues sans équipes réserves : leur structure ne changera pas avec les années, on garde tout (ex. le N1 au 5ème tour avec les 18 équipes au lieu de 10 équipes calculés avec la méthode du ratio)
                            int lastLevelWithoutReserves = (Session.Instance.Game.kernel.LocalisationTournament(this) as Country).GetLastLeagueLevelWithoutReserves();
                            foreach(LeagueCupApparition lca in leagueCupApparitions)
                            {
                                if(lca.tournament.level <= lastLevelWithoutReserves && lca.apparitionRound < delRoundId)
                                {
                                    totalApp -= lca.tournament.rounds[0].clubs.Count;
                                    expectedQualified -= lca.tournament.rounds[0].clubs.Count;
                                    RecoverTeams newRt = new RecoverTeams(lca.tournament.rounds[0], lca.teams, lca.isBestTeams ? RecuperationMethod.Best : RecuperationMethod.Worst);
                                    newRecoverTeams.Add(newRt);
                                    _rounds[delRoundId].recuperedTeams.Add(newRt);
                                    lca.apparitionRound = -1;
                                    Console.WriteLine("Tour " + 0 + ", " + lca.teams + " équipes de D" + (lca.tournament.level) + " directement qualifiés au tour " + delRoundId);
                                }
                            }*/
                            double ratio = expectedQualified / (totalApp + 0.0); //Ratio d'équipes à prendre dans chaque ligue
                            int currentTeamsAdded = 0;
                            for (int i = 0; i < delRoundId; i++)
                            {
                                for (int k = 0; k < leagueCupApparitions.Count; k++)
                                {
                                    int teamsToAddFromLeague = (int)Math.Round(leagueCupApparitions[k].teams * ratio); //Le nombre d'équipes à prendre dans la ligue est calculé en fonction du nombre d'équipes nécessaire au tour
                                    if (leagueCupApparitions[k].apparitionRound == i)
                                    {
                                        RecoverTeams newRt = new RecoverTeams(leagueCupApparitions[k].tournament.rounds[0], teamsToAddFromLeague, leagueCupApparitions[k].isBestTeams ? RecuperationMethod.Best : RecuperationMethod.Worst);
                                        _rounds[delRoundId].recuperedTeams.Add(newRt);
                                        currentTeamsAdded += teamsToAddFromLeague;
                                        newRecoverTeams.Add(newRt);
                                        Console.WriteLine("Tour " + i + ", " + teamsToAddFromLeague + " équipes de D" + (k + 1) + " directement qualifiés au tour " + delRoundId);
                                    }
                                }
                                _rounds[i].recuperedTeams.Clear();
                            }
                            //On oublie pas de mettre à jour les qualifications des autres ligues aux tours plus loin car d'une année à l'autre le nombre d'équipes première dans une ligue a changé
                            for (int i = 0; i < leagueCupApparitions.Count; i++)
                            {
                                RecoverTeams rt = GetRecoverTeams(leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams);
                                if (leagueCupApparitions[i].apparitionRound >= delRoundId && !newRecoverTeams.Contains(rt))
                                {
                                    UpdateRecuperedTeams(leagueCupApparitions[i].teams, leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams, false);
                                }
                            }
                            Console.WriteLine(currentTeamsAdded + " équipes ajoutées sur un total de " + expectedQualified);
                            //Même fixe que l'autre cas si le calcul du ratio n'est pas tombé juste
                            if (currentTeamsAdded != expectedQualified)
                            {
                                int margin = expectedQualified - currentTeamsAdded;
                                Console.WriteLine("Marge : " + margin + " avec " + expectedQualified + " equipes a atteindres et " + currentTeamsAdded + " equipes ajoutés");
                                for (int i = 0; i < newRecoverTeams.Count && margin != 0; i++)
                                {
                                    int currentlyAddedTeamFromLeague = 0;
                                    RecoverTeams rt = newRecoverTeams[i];
                                    LeagueCupApparition lc = null;
                                    foreach (LeagueCupApparition lca in leagueCupApparitions)
                                    {
                                        if (lca.tournament == (rt.Source as Round).Tournament && lca.isBestTeams == (rt.Method == RecuperationMethod.Best))
                                        {
                                            lc = lca;
                                        }
                                    }
                                    if (rt.Source != null)
                                    {
                                        currentlyAddedTeamFromLeague = rt.Number;
                                    }
                                    if (currentlyAddedTeamFromLeague + margin <= lc.teams)
                                    {
                                        UpdateRecuperedTeams(margin, lc.tournament, lc.isBestTeams, true);
                                        margin = 0;
                                    }
                                }
                            }
                        }
                        //Pas assez d'équipe, on passe au tour suivant en mettant à jour le nombre actuel d'équipes dans la compétition
                        currentTeamsCount = expectedRoundTeams;
                    }
                }

                if (this._rounds.Count > 8)
                {
                    Console.WriteLine("Résumé de la compétition " + this.name);
                    foreach (Round r in _rounds)
                    {
                        Console.WriteLine("= " + r.name + " =");
                        foreach (RecoverTeams rt in r.recuperedTeams)
                        {
                            Console.WriteLine(rt.Source.ToString() + " - " + rt.Number + " - " + rt.Method);
                        }
                    }
                }
            }

            //PrintTournamentResume(teamsFromOutsideLeagueSystem);
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
                    int totalAdmTeamsCount = rt.Source.RetrieveTeams(-1, rt.Method, rounds[i].rules.Contains(Rule.OnlyFirstTeams), parent.Association).Count;
                    Console.WriteLine("+ " + (rt.Source as Round).Tournament.name + " - " + rt.Number + "/" + totalAdmTeamsCount + " - " + rt.Method);
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

        public Tournament CopyForArchive(bool makeRoundsInactive)
        {
            Tournament copy = new Tournament(Session.Instance.Game.kernel.NextIdTournament(), _name, _logo, _seasonBeginning, _shortName, _isChampionship, _level, _periodicity, _remainingYears, _color, _status, _parent);
            foreach (Round r in rounds)
            {
                Round roundCopy = r.Copy();
                roundCopy.Tournament = copy;
                roundCopy.recuperedTeams.AddRange(AlreadyStoredRecuperedTeams() ? r.baseRecuperedTeams : r.recuperedTeams);
                if(makeRoundsInactive)
                {
                    int associationLevel = (roundCopy as GroupsRound != null) ? (roundCopy as GroupsRound).administrativeLevel : 0;
                    //roundCopy = new InactiveRound(roundCopy.name, roundCopy.programmation.defaultHour, roundCopy.programmation.initialisation, roundCopy.programmation.end, associationLevel);
                    roundCopy = new GroupInactiveRound(Session.Instance.Game.kernel.NextIdRound(), roundCopy.name, this, roundCopy.programmation.defaultHour, new List<GameDay>(), new List<TvOffset>(), 1, 1, roundCopy.programmation.initialisation, roundCopy.programmation.end, -1, RandomDrawingMethod.Administrative, associationLevel, false, 0, 0, 0);
                }
                for (int i = 0; i < roundCopy.qualifications.Count; i++)
                {
                    Qualification q = roundCopy.qualifications[i];
                    if (roundCopy.qualifications[i].tournament == this)
                    {
                        q.tournament = copy;
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
                        rounds[i].qualifications[j] = new Qualification(rounds[i].qualifications[j].ranking, rounds[i].qualifications[j].roundId - _extraRounds, rounds[i].qualifications[j].tournament, rounds[i].qualifications[j].isNextYear, rounds[i].qualifications[j].qualifies);
                    }
                }
                _extraRounds = 0;
                InitializeQualificationsNextYearsLists();
                if(isHostedByOneCountry)
                {
                    InitializeHost();
                }
            }
            if(!isChampionship && !IsInternational() && (Session.Instance.Game.kernel.LocalisationTournament(this) as Country).LeagueSystemWithReserves())
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
                    if (q.isNextYear && q.tournament.isChampionship && ((!relegation && q.tournament.level < level) || (relegation && q.tournament.level > level)) )
                    {
                        res = r;
                    }
                    if (!q.isNextYear && q.tournament.isChampionship && !browsed.Contains(q.tournament.rounds[q.roundId]))
                    {
                        tas.Add(q.tournament.rounds[q.roundId]);
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
                if (!q.isNextYear && q.tournament.isChampionship && q.ranking > 1)
                {
                    Tournament targetTournament = q.tournament;
                    Round targetRound = q.tournament.rounds[q.roundId];
                    if (!allRounds.Contains(targetRound))
                    {
                        res.AddRange(GetPlayOffsTree(targetTournament, targetRound, allRounds));
                    }
                }
            }
            //Step 2 : Append rounds with winning teams
            foreach (Qualification q in round.qualifications)
            {
                if (!q.isNextYear && q.tournament.isChampionship && q.ranking == 1)
                {
                    Tournament targetTournament = q.tournament;
                    Round targetRound = this.rounds[q.roundId];
                    if (!allRounds.Contains(targetRound))
                    {
                        res.AddRange(GetPlayOffsTree(targetTournament, targetRound, allRounds));
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
                        if (!q.isNextYear && q.tournament == tournament && q.roundId == roundIndex /*&& q.ranking == 1*/)
                        {
                            Tournament targetTournament = q.tournament;
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
            lastChampionshipRound.qualifications.ForEach(q => finalRound = (!q.isNextYear && q.tournament == this && q.roundId > 0 && q.ranking == 1) ? _rounds[q.roundId] : finalRound);
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
                        Console.WriteLine(rt.Source.ToString());
                        if (rt.Source == oldRound)
                        {
                            Console.WriteLine("Match entre " + rt.Source.ToString() + " et " + oldRound.ToString());
                            rt.Source = newRound;
                        }
                    }
                }
            }
        }

        public void DisableTournament()
        {
            List<Round> newRounds = new List<Round>();
            int i = 0;
            foreach(Round t in _rounds)
            {
                if((t as KnockoutRound) == null)
                {
                    int associationLevel = (t as GroupsRound != null) ? (t as GroupsRound).administrativeLevel : 0;
                    int groupCount = (t as GroupsRound != null) ? (t as GroupsRound).groupsCount : 1;
                    GroupInactiveRound newRound = new GroupInactiveRound(Session.Instance.Game.kernel.NextIdRound(), t.name, this, t.programmation.defaultHour, new List<GameDay>(), new List<TvOffset>(), groupCount, 1, t.programmation.initialisation, t.programmation.end, -1, associationLevel == 0 ? RandomDrawingMethod.Random : RandomDrawingMethod.Administrative, associationLevel, false, 0, 0, 0);
                    newRound.rules.AddRange(t.rules);
                    newRounds.Add(newRound);

                    List<Qualification> qualificationsToAdd = new List<Qualification>(t.qualifications);

                    GroupsRound cRound = t as GroupsRound;
                    if (cRound != null && cRound.groupsCount == 1)
                    {
                        qualificationsToAdd = cRound.AdaptQualificationsToRanking(cRound.qualifications, cRound.clubs.Count);
                    }

                    foreach (Qualification q in qualificationsToAdd)
                    {
                        if (t as KnockoutRound != null)
                        {
                            int clubCount = t.clubs.Count;
                            foreach (Tournament otherTournaments in Session.Instance.Game.kernel.Competitions)
                            {
                                foreach (Round otherRound in otherTournaments.rounds)
                                {
                                    foreach (Qualification otherQualifications in otherRound.qualifications)
                                    {
                                        if (otherQualifications.tournament == this && !otherQualifications.isNextYear && otherQualifications.roundId == i)
                                        {
                                            clubCount++;
                                        }
                                    }
                                }
                            }
                            int numberOfGames = clubCount / 2;
                            for (int j = numberOfGames * (q.ranking - 1); j < numberOfGames * q.ranking; j++)
                            {
                                newRound.qualifications.Add(new Qualification(j + 1, q.roundId, q.tournament, q.isNextYear, q.qualifies));
                            }
                        }
                        else if(t as GroupsRound != null)
                        {
                            newRound.qualifications.Add(q);
                        }

                    }
                    foreach (RecoverTeams re in t.recuperedTeams)
                    {
                        newRound.recuperedTeams.Add(re);
                    }

                    foreach (Club c in t.clubs)
                    {
                        newRound.clubs.Add(c);
                    }

                    foreach (Prize d in t.prizes)
                    {
                        newRound.prizes.Add(d);
                    }

                    foreach (Rule r in t.rules)
                    {
                        newRound.rules.Add(r);
                    }

                    //UpdateRecoverTeamsRound(t, newRound);

                    i++;
                }
                else
                {
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
            return Session.Instance.Game.kernel.LocalisationTournament(this) as Continent != null;
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