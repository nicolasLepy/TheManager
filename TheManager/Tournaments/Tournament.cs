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

namespace TheManager
{

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

    [DataContract(IsReference =true)]
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
        private Dictionary<int,Tournament> _previousEditions;
        [DataMember]
        private int _periodicity;
        [DataMember]
        private int _remainingYears;
        [DataMember]
        private Color _color;


        public string name { get => _name; }
        public Color color => _color;
        public List<Round> rounds { get => _rounds; }
        public string logo { get => _logo; }
        [DataMember]
        public int currentRound { get; set; }

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

        public Tournament(string name, string logo, GameDay seasonBeginning, string shortName, bool isChampionship, int level, int periodicity, int remainingYears, Color color)
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
        }

        public void InitializeQualificationsNextYearsLists()
        {
            _nextYearQualified = new List<Club>[rounds.Count];
            for(int i =0;i < rounds.Count; i++)
            {
                _nextYearQualified[i] = new List<Club>();
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
                Tournament copyForArchives = new Tournament(_name, _logo, _seasonBeginning, _shortName, _isChampionship, _level, _periodicity, _remainingYears, _color);

                int gamesCount = 0;
                foreach (Round r in rounds)
                {
                    copyForArchives.rounds.Add(r.Copy());
                    gamesCount += r.matches.Count;
                }
                copyForArchives.statistics = statistics;
                if(_periodicity == 1 || (_periodicity > 1 && gamesCount > 0))
                {
                    _previousEditions.Add(_periodicity == 1 ? Session.Instance.Game.date.Year : Session.Instance.Game.date.Year - periodicity, copyForArchives);
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
                    List<Club> clubs = new List<Club>(_nextYearQualified[i]);
                    foreach (Club c in clubs)
                    {
                        rounds[i].clubs.Add(c);
                    }
                }
                InitializeQualificationsNextYearsLists();
                currentRound = -1;
                
            }
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
            }

            //Tour 0, championnat -> génère match amicaux
            if(currentRound == 0)
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
                return _rounds[0].Winner();
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
                newRounds.Add(newRound);
                foreach (Qualification q in t.qualifications)
                {
                    //TODO: Non optimal architecture
                    if(t as GroupsRound != null)
                    {
                        GroupsRound tGroup = t as GroupsRound;
                        for(int j = tGroup.groupsCount*(q.ranking-1); j< tGroup.groupsCount * q.ranking; j++)
                        {
                            newRound.qualifications.Add(new Qualification(j+1, q.roundId, q.tournament, q.isNextYear));
                        }
                    }
                    else if(t as ChampionshipRound != null)
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
                            newRound.qualifications.Add(new Qualification(j+1, q.roundId, q.tournament, q.isNextYear));
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

    }
}