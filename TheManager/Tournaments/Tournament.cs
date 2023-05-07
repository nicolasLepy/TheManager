using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Text;
using TheManager.Exportation;
using System.Windows.Media;
using TheManager.Tournaments;
using TheManager.Comparators;
using System.Runtime.InteropServices;
using System.Data;
using System.Diagnostics;

/*
 * TODO: Factorisations possibles :
 * club.status, competition.status
 * 
 * competition.reset, (country.reset), continent.reset
 */

namespace TheManager
{

    public enum TournamentRule
    {
        NoRule,
        OnWinnerQualifiedAdaptClubsQualifications,
        OnWinnerQualifiedAdaptAssociationQualifications
    }

    [DataContract]
    public struct TournamentStatistics : IEquatable<TournamentStatistics>
    {
        [DataMember]
        public Match BiggerScore { get; set; }
        [DataMember]
        public Match LargerScore { get; set; }
        [DataMember]
        public KeyValuePair<int, Player> TopGoalscorerOnOneSeason { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> BiggestAttack { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> WeakestAttack { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> BiggestDefense { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> WeakestDefense { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> MostPoints { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> LowestPoints { get; set; }



        public TournamentStatistics(int i)
        {
            BiggerScore = null;
            LargerScore = null;
            TopGoalscorerOnOneSeason = new KeyValuePair<int, Player>(0, null);
            BiggestAttack = new KeyValuePair<int, Club>(0, null);
            WeakestAttack = new KeyValuePair<int, Club>(0, null);
            BiggestDefense = new KeyValuePair<int, Club>(0, null);
            WeakestDefense = new KeyValuePair<int, Club>(0, null);
            LowestPoints = new KeyValuePair<int, Club>(0, null);
            MostPoints = new KeyValuePair<int, Club>(0, null);
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
        private KeyValuePair<AdministrativeDivision, Tournament> _parent;

        public string name { get => _name; }
        public Color color => _color;
        public List<Round> rounds { get => _rounds; }
        public string logo { get => _logo; }
        public ClubStatus status => _status;

        [DataMember]
        public int currentRound { get; set; }

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

        public KeyValuePair<AdministrativeDivision, Tournament> parent => _parent;

        public Tournament(string name, string logo, GameDay seasonBeginning, string shortName, bool isChampionship, int level, int periodicity, int remainingYears, Color color, ClubStatus status, KeyValuePair<AdministrativeDivision, Tournament> parent)
        {
            _rounds = new List<Round>();
            _name = name;
            _logo = logo;
            _seasonBeginning = seasonBeginning;
            currentRound = -1;
            _shortName = shortName;
            _isChampionship = isChampionship;
            _level = level;
            _statistics = new TournamentStatistics(0);
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
                if(t.parent.Value == this)
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
                res = rt.Source.RetrieveTeams(-1, rt.Method, false, parent.Key).Count;
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

            if (parent.Key != null && (localisation as Country) != null)
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
                        int newTeamsCount = rt.Method == RecuperationMethod.Best ? rt.Source.RetrieveTeams(-1, rt.Method, true, parent.Key).Count : 0;
                        if(newTeamsCount > 0)
                        {
                            round.recuperedTeams[j] = new RecoverTeams(rt.Source, newTeamsCount, rt.Method);
                        }
                        else
                        {
                            round.recuperedTeams.RemoveAt(j);
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
            if(parent.Key == null)
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
                List<int> availableDatesAll = (localisation as Country).GetAvailableCalendarDates(false, 2);
                List<int> availableDates = new List<int>();
                int beginningCompetition = this._seasonBeginning.WeekNumber;
                int beginningRounds = rounds.First().programmation.initialisation.WeekNumber;
                //Filter to get available dates to play the new round
                foreach(int ad in availableDatesAll)
                {
                    //Les dates sont centrées sur le début de la compétition -> pas de problème en cas de passage d'une année à l'autre (semaines 52 puis semaine 02 par ex.)
                    int absoluteAd = Utils.Modulo(ad - beginningCompetition, 53); // TODO: Des fois 52 ou 53 semaines !
                    int absoluteBeginFirstRound = Utils.Modulo(beginningRounds - beginningCompetition, 53); // TODO: Des fois 52 ou 53 semaines !

                    if (absoluteAd < absoluteBeginFirstRound && absoluteAd > 0)
                    {
                        availableDates.Add(ad);
                    }
                }

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
                    foreach (GameDay dt in firstRound.programmation.gamesDays)
                    {
                        int dateIndex = (availableDates.Count - 2) - (2 * roundCreated);
                        newRoundTimes.Add(new GameDay(availableDates[dateIndex], dt.MidWeekGame, dt.YearOffset, dt.DayOffset));
                    }
                    newRounds.Add(new KnockoutRound("Tour préliminaire", firstRound.programmation.defaultHour, newRoundTimes, new List<TvOffset>(), firstRound.twoLegs, firstRound.phases, new GameDay(availableDates[(availableDates.Count - 2) - (2 * roundCreated)]-1, true, firstRound.programmation.initialisation.YearOffset, firstRound.programmation.initialisation.DayOffset), new GameDay(availableDates[(availableDates.Count - 2) - (2 * roundCreated)]+1, firstRound.programmation.initialisation.MidWeekGame, firstRound.programmation.initialisation.YearOffset, firstRound.programmation.initialisation.DayOffset), RandomDrawingMethod.Random, false, firstRound.programmation.gamesPriority));
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
                if (r.teamsByAdministrativeDivision.Count > 0)
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
                foreach (KeyValuePair<AdministrativeDivision, int> kvp in pivotRound.teamsByAdministrativeDivision)
                {
                    //Copie du tournoi est créée
                    Tournament regionalTournament = CopyForArchive(false);
                    regionalTournament.currentRound = -1;
                    regionalTournament._name = string.Format("{0} - {1}", regionalTournament, kvp.Key.name);
                    regionalTournament._level = 1000; //Un niveau exagérément élevé est mis pour éviter d'aller chercher les vainqueurs de ces compétitions pour les places européennes par exemple
                    regionalTournament._parent = new KeyValuePair<AdministrativeDivision, Tournament>(kvp.Key, this); //Cette compétition dépend du tournoi principal
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
            Utils.Debug("[UpdateCupQualifications " + name + "] (" + parent.Key + ")");
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
                    if (!t.IsInternational() && t != this && t.parent.Value != this)
                    {
                        foreach (Round r in t.rounds)
                        {
                            for (int i = 0; i < r.qualifications.Count; i++)
                            {
                                if (r.qualifications[i].tournament.parent.Value == this || (r.qualifications[i].tournament == this && r.qualifications[i].roundId < idRoundPivot))
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
            if (leagueCupLike || (this.parent.Value == null && this.parent.Key != null)) //Regional cup are updated following league cup algorithm
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
                //On ne considère pas ici les chemins régionaux (t.parent.Value != null) car seront comptés après à l'aide des attributs Round.teamsByAdministrativeDivision pour chaque tour
                foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if (!t.IsInternational() && t != this && t.parent.Value != this)
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
                    foreach (KeyValuePair<AdministrativeDivision, int> regionalPath in r.teamsByAdministrativeDivision)
                    {
                        teamsFromOutsideLeagueSystem[rounds.IndexOf(r)] += regionalPath.Value;
                    }
                }

                foreach (int i in teamsFromOutsideLeagueSystem) Console.WriteLine("[Update " + this.name + "] teamsFromOutsideLeagueSystem round " + i);

                //Obtient le nombre d'équipes une fois toutes les ligues entrées dans la compétition (64 en Coupe de France)
                //Dans le cas d'une phase qualificative régionale avant la phase nationale, 
                int teamsAtTheLastRound = 2;
                if(parent.Value != null)
                {
                    AdministrativeDivision concernedRegion = parent.Key;
                    foreach(Round r in parent.Value.rounds)
                    {
                        if(r.teamsByAdministrativeDivision.ContainsKey(concernedRegion))
                        {
                            teamsAtTheLastRound = r.teamsByAdministrativeDivision[concernedRegion] * 2;
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
                            int roundsClubCount = _parent.Key == null ? rtRound.CountWithoutReserves() : rtRound.CountWithoutReserves(_parent.Key);
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

            PrintTournamentResume(teamsFromOutsideLeagueSystem);
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
                    int totalAdmTeamsCount = rt.Source.RetrieveTeams(-1, rt.Method, rounds[i].rules.Contains(Rule.OnlyFirstTeams), parent.Key).Count; //TeamsCount(rt)
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
            Tournament copy = new Tournament(_name, _logo, _seasonBeginning, _shortName, _isChampionship, _level, _periodicity, _remainingYears, _color, _status, _parent);
            foreach (Round r in rounds)
            {
                Round roundCopy = r.Copy();

                roundCopy.recuperedTeams.AddRange(AlreadyStoredRecuperedTeams() ? r.baseRecuperedTeams : r.recuperedTeams);
                if(makeRoundsInactive)
                {
                    roundCopy = new InactiveRound(roundCopy.name, roundCopy.programmation.defaultHour, roundCopy.programmation.initialisation, roundCopy.programmation.end);
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
            copy.currentRound = copy.rounds.Count - 1;

            return copy;
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
                currentRound = -1;
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
                    if(_statistics.MostPoints.Value == null || _statistics.MostPoints.Key < points)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(points, c);
                        _statistics.MostPoints = newRecord;
                    }
                    if (_statistics.LowestPoints.Value == null || _statistics.LowestPoints.Key > points)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(points, c);
                        _statistics.LowestPoints = newRecord;
                    }
                    if (_statistics.BiggestAttack.Value == null || _statistics.BiggestAttack.Key < goalsFor)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(goalsFor, c);
                        _statistics.BiggestAttack = newRecord;
                    }
                    if (_statistics.WeakestAttack.Value == null || _statistics.WeakestAttack.Key > goalsFor)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(goalsFor, c);
                        _statistics.WeakestAttack = newRecord;
                    }
                    if (_statistics.BiggestDefense.Value == null || _statistics.BiggestDefense.Key > goalsAgainst)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(goalsAgainst, c);
                        _statistics.BiggestDefense = newRecord;
                    }
                    if (_statistics.WeakestDefense.Value == null || _statistics.WeakestDefense.Key < goalsAgainst)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(goalsAgainst, c);
                        _statistics.WeakestDefense = newRecord;
                    }
                }
            }
        }

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
        }

        /// <summary>
        /// Return the last championship round of the tournament
        /// TODO: Incorrect round returned when multiple championship rounds are in the tournament where the tournament is turned inactive
        /// </summary>
        /// <returns></returns>
        public Round GetLastChampionshipRound()
        {
            Round res = null;
            if ((rounds[0] as InactiveRound) != null || (rounds[0] as GroupsRound) != null)
            {
                res = rounds[0]; 
            }
            for(int i = rounds.Count-1; i>=0 && res == null; i--)
            {
                if ((rounds[i] as ChampionshipRound) != null)
                {
                    res = rounds[i];
                }
            }
            return res;
        }

        /// <summary>
        /// Get Final Phase Clubs ranked from competition winner to first team eliminated
        /// TODO: Final phases with nested group rounds are not managed. Final phases can only be knockout rounds
        /// </summary>
        public List<Club> GetFinalPhasesClubs()
        {
            List<Club> finalClubs = new List<Club>();
            Round finalRound = null;
            Round lastChampionshipRound = GetLastChampionshipRound(); //_rounds[0]
            lastChampionshipRound.qualifications.ForEach(q => finalRound = (!q.isNextYear && q.tournament == this && q.roundId > 0 && q.ranking == 1) ? _rounds[q.roundId] : finalRound);
            //If the league winner is qualified on another round this year on this tournament then the tournament finish with a final phase
            List<Round> finalRounds = new List<Round>();

            if (finalRound != null)
            {
                finalRounds = GetFinalPhaseTree(finalRound, new List<Round>()) ;
            }
            for (int i = finalRounds.Count-1; i>=0; i--)
            {
                List<Club> roundClubs = new List<Club>(finalRounds[i].clubs);
                roundClubs.Sort(new ClubRankingComparator(finalRounds[i].matches, finalRounds[i].tiebreakers, RankingType.General, false, (finalRounds[i] as KnockoutRound) != null));

                foreach (Club c in roundClubs)
                {
                    if(!finalClubs.Contains(c))
                    {
                        finalClubs.Add(c);
                    }
                }
            }
            return finalClubs;
        }

        public void NextRound()
        {
            if (currentRound > -1 && currentRound < _rounds.Count)
            {
                _rounds[currentRound].DistributeGrants();
            }
            if (_rounds.Count > currentRound + 1)
            {
                currentRound++;
                _rounds[currentRound].Initialise();
                if(_rounds[currentRound].rules.Contains(Rule.HostedByOneCountry))
                {
                    _rounds[currentRound].AffectHostStadiumsToGames();
                }
            }

            //Tour 0, championnat -> génère match amicaux
            if (currentRound == 0)
            {
                if (isChampionship)
                {
                    foreach (Club c in rounds[0].clubs)
                    {
                        CityClub cv = c as CityClub;
                        if (cv != null)
                        {
                            cv.GenerateFriendlyGamesCalendar();
                        }
                    }
                }
            }
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

        /// <summary>
        /// Make the tournament inactive (when used decide to disable tournament)
        /// </summary>
        public void DisableTournament()
        {
            List<Round> newRounds = new List<Round>();
            int i = 0;
            foreach(Round t in _rounds)
            {
                InactiveRound newRound = new InactiveRound(t.name, t.programmation.defaultHour, t.programmation.initialisation, t.programmation.end);
                newRound.rules.AddRange(t.rules);
                newRounds.Add(newRound);

                GroupsRound tGroup = t as GroupsRound;
                if(tGroup != null)
                {
                    if(tGroup.RandomDrawingMethod != RandomDrawingMethod.Administrative)
                    {
                        for (int j = 0; j < tGroup.groupsCount; j++)
                        {
                            List<Qualification> groupQualifications = tGroup.GetGroupQualifications(j);
                            groupQualifications.Sort(new QualificationComparator());
                            foreach (Qualification q in groupQualifications)
                            {
                                if (q.qualifies == 0 || (q.qualifies < 0 && (-q.qualifies > j - 1)) || (q.qualifies > 0 && (j < q.qualifies)))
                                {
                                    newRound.qualifications.Add(new Qualification((j + 1) + ((q.ranking - 1) * tGroup.groupsCount), q.roundId, q.tournament, q.isNextYear, q.qualifies));
                                }
                            }
                        }

                        //Groups with extra teams request additional qualification to the next year same tournament
                        int extraTeams = tGroup.clubs.Count % tGroup.groupsCount;
                        for (int j = tGroup.clubs.Count - extraTeams; j < tGroup.clubs.Count; j++)
                        {
                            newRound.qualifications.Add(new Qualification(j + 1, 0, tGroup.Tournament, true, 0));
                        }
                    }
                    else
                    {
                        /*
                        Country c = Session.Instance.Game.kernel.LocalisationTournament(this) as Country;
                        GroupsRound lowerNonDeactivatedDivisionGroups = null;
                        for(int level = _level - 1; level > 0 && lowerNonDeactivatedDivisionGroups == null; level--)
                        {
                            lowerNonDeactivatedDivisionGroups = c.League(level).rounds[0] as GroupsRound;
                        }
                        
                        //List administrative divisions of clubs in the round
                        List<AdministrativeDivision> roundAdministrativeDivisions = new List<AdministrativeDivision>();
                        List<AdministrativeDivision> roundAdministrativeDivisionsLowerDivisionLevel = new List<AdministrativeDivision>();
                        foreach (Club tClub in t.clubs)
                        {
                            AdministrativeDivision adClub = tClub.Country().GetAdministrativeDivisionLevel(tClub.AdministrativeDivision(), tGroup.administrativeLevel);
                            AdministrativeDivision adClubCompLevel = lowerNonDeactivatedDivisionGroups != null ? tClub.Country().GetAdministrativeDivisionLevel(tClub.AdministrativeDivision(), lowerNonDeactivatedDivisionGroups.administrativeLevel) : null;
                            if (!roundAdministrativeDivisions.Contains(adClub))
                            {
                                roundAdministrativeDivisions.Add(adClub);
                            }
                            if (adClubCompLevel != null && !roundAdministrativeDivisionsLowerDivisionLevel.Contains(adClubCompLevel))
                            {
                                roundAdministrativeDivisionsLowerDivisionLevel.Add(adClubCompLevel);
                            }
                        }

                        int relegationsCount = 0;
                        int promotionsCount = 0;
                        Tournament upperTournament = null;
                        Tournament lowerTournament = null;
                        int baseRelegations = 0;
                        int basePromotions = 0;
                        foreach (Qualification q in t.qualifications)
                        {
                            if (q.isNextYear && q.roundId == 0 && q.tournament.level > level)
                            {
                                baseRelegations++;
                                lowerTournament = q.tournament;
                            }
                            if (q.isNextYear && q.roundId == 0 && q.tournament.level < level)
                            {
                                basePromotions++;
                                upperTournament = q.tournament;
                            }
                        }

                        if(true || (lowerNonDeactivatedDivisionGroups != null && lowerNonDeactivatedDivisionGroups.RandomDrawingMethod != RandomDrawingMethod.Administrative))
                        {
                            relegationsCount = roundAdministrativeDivisions.Count * baseRelegations;
                            promotionsCount = roundAdministrativeDivisions.Count * basePromotions;

                        }
                        else
                        {
                            //relegationsCount = int((roundAdministrativeDivisionsLowerDivisionLevel.Count / roundAdministrativeDivisionsLowerDivisionLevel.Count)) * baseRelegations;
                            promotionsCount = 0;
                                //si group_level.adm > lowerNonDeactivatedDivision.adm_level:
			                    //    relegation = int ((nb adm group_level / nb adm lowerNonDeactivatedDivision.adm_level) * relegation)
			                    //    promotions = int ((nb adm group_level / nb adm lowerNonDeactivatedDivision.adm_level) * relegation) //Assurer algo cohérent
                        }
                        //Apply relegations and promotions
                        for (int p = 0; p < promotionsCount; p++)
                        {
                            newRound.qualifications.Add(new Qualification(p + 1, 0, upperTournament, true, 0));
                        }
                        for (int r = t.clubs.Count; r > t.clubs.Count - relegationsCount; r--)
                        {
                            newRound.qualifications.Add(new Qualification(r, 0, lowerTournament, true, 0));
                        }
                        */
                    }

                }
                
                List<Qualification> qualificationsToAdd = new List<Qualification>(t.qualifications);
                
                ChampionshipRound cRound = t as ChampionshipRound;
                if (cRound != null)
                {
                    qualificationsToAdd = cRound.GetQualifications();
                }

                InactiveRound iRound = t as InactiveRound;
                //If this round has less promotions than upper inactive round, then
                if(iRound != null && _level > 1)
                {
                    InactiveRound upperIRound = (Session.Instance.Game.kernel.LocalisationTournament(this) as Country).League(_level - 1).rounds[0] as InactiveRound;
                    if(upperIRound != null)
                    {
                        int totalRelegationsUpper = 0;
                        foreach(Qualification q in upperIRound.qualifications)
                        {
                            if(q.isNextYear && q.roundId == 0 && q.tournament.level == _level)
                            {
                                totalRelegationsUpper++;
                            }
                        }
                        int currentPromotions = 0;
                        int currentRelegations = 0;
                        Tournament lowerTournament = null;
                        Tournament upperTournament = null;
                        foreach(Qualification q in iRound.qualifications)
                        {
                            if (q.isNextYear && q.roundId == 0 && q.tournament.level < _level)
                            {
                                currentPromotions++;
                                upperTournament = q.tournament;
                            }
                            if (q.isNextYear && q.roundId == 0 && q.tournament.level > _level)
                            {
                                currentRelegations++;
                                lowerTournament = q.tournament;
                            }
                        }

                        if(totalRelegationsUpper > currentPromotions && currentPromotions > 0)
                        {
                            int ratio = totalRelegationsUpper / currentPromotions;
                            //Adapt prom/rel to ratio
                            int newPromotions = totalRelegationsUpper;
                            int newRelegations = currentRelegations * ratio;
                            for(int p = 0; p < newPromotions; p++)
                            {
                                for (int j = 0; j < iRound.qualifications.Count; j++)
                                {
                                    Qualification q = iRound.qualifications[j];
                                    if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == p+1)
                                    {
                                        iRound.qualifications[j] = new Qualification(q.ranking, q.roundId, upperTournament, q.isNextYear, 0);
                                    }
                                }
                            }
                            for(int r = iRound.clubs.Count; r > iRound.clubs.Count-newRelegations; r++)
                            {
                                for (int j = 0; j < iRound.qualifications.Count; j++)
                                {
                                    Qualification q = iRound.qualifications[j];
                                    if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == r)
                                    {
                                        iRound.qualifications[j] = new Qualification(q.ranking, q.roundId, lowerTournament, q.isNextYear, 0);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Qualification q in qualificationsToAdd)
                {
                    //TODO: Non optimal architecture
                    /*if(t as GroupsRound != null)
                    {
                        List<Qualification>[] qualifications = new List<Qualification>[tGroup.groupsCount];
                        for(int j = 0; j<tGroup.groupsCount; j++)
                        {
                            qualifications[i] = tGroup.GetGroupQualifications(j);
                        }
                        int clubsCount = tGroup.clubs.Count;
                        Console.WriteLine("Deactivate " + t.name + " from " + this.name);
                        for(int j = tGroup.groupsCount*(q.ranking-1); j< tGroup.groupsCount * q.ranking; j++)
                        {
                            newRound.qualifications.Add(new Qualification(j+1, q.roundId, q.tournament, q.isNextYear, q.qualifies));
                        }
                    }*/
                    if(t as ChampionshipRound != null)
                    {
                        newRound.qualifications.Add(q);
                    }
                    else if(t as KnockoutRound != null)
                    {
                        int clubCount = t.clubs.Count ;
                        foreach(Tournament otherTournaments in Session.Instance.Game.kernel.Competitions)
                        {
                            foreach(Round otherRound in otherTournaments.rounds)
                            {
                                foreach(Qualification otherQualifications in otherRound.qualifications)
                                {
                                    if(otherQualifications.tournament == this && !otherQualifications.isNextYear && otherQualifications.roundId == i)
                                    {
                                        clubCount++;
                                    }
                                }
                            }
                        }
                        int numberOfGames = clubCount / 2;
                        for(int j = numberOfGames*(q.ranking-1); j< numberOfGames * q.ranking; j++)
                        {
                            newRound.qualifications.Add(new Qualification(j+1, q.roundId, q.tournament, q.isNextYear, q.qualifies));
                        }

                    }
                    else if(t as InactiveRound != null)
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

            foreach(Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                if(c != this)
                {
                    foreach (Round t in c.rounds)
                    {
                        for(int j = 0; j<t.recuperedTeams.Count; j++)
                        {
                            RecoverTeams re = t.recuperedTeams[j];
                            if (_rounds.Contains(re.Source))
                            {
                                int index = _rounds.IndexOf(re.Source as Round);
                                re.Source = newRounds[index];
                            }
                            //Affectation to the list because re is a struct (get a copy, not a ref)
                            t.recuperedTeams[j] = re;
                        }
                        
                    }

                }
            }

            _rounds.Clear();
            foreach (Round t in newRounds)
            {
                _rounds.Add(t);
            }

            /*
            Tour premierTour = _tours[0];
            Tour dernierTour = _tours[_tours.Count - 1];
            TourInactif ti = new TourInactif("Tour", new Heure() { Heures = 18, Minutes = 0 }, premierTour.Programmation.Initialisation.AddDays(3), dernierTour.Programmation.Fin.AddDays(-3));

            List<IEquipesRecuperables> tours = new List<IEquipesRecuperables>(_tours);

            //qualifications

            //Pour toutes les autres compétitions, quand il faut récupérer des équipes de cette compétition on les récupère depuis le tour que l'on est en train de créer
            //Pour toutes les qualitifcations des autres compétitions vers ce tour, elle se transforment au tour 0 si c'est le tour 0 qui est visé et année suivante, sinon on abandonnne
            foreach (Competition c in Session.Instance.Partie.Gestionnaire.Competitions)
            {
                if(c != this)
                {
                    foreach(Tour t in _tours)
                    {
                        for(int i = 0; i<t.RecuperationEquipes.Count; i++)
                        {
                            RecuperationEquipes re = t.RecuperationEquipes[i];
                            if (tours.Contains(re.Source)) re.Source = ti;
                        }
                        for(int i = 0; i<t.Qualifications.Count; i++)
                        {
                            Qualification q = t.Qualifications[i];
                            if(!(q.Competition == this && q.IDTour == 0 && q.AnneeSuivante == true))
                            {
                                t.Qualifications.Remove(q);
                                i--;
                            }
                        }

                    }
                }
            }


            _tours.Clear();
            _tours.Add(ti);*/

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
    }
}