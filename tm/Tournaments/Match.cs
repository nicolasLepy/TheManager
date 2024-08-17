using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using System.Xml.Linq;
using tm.Comparators;
using tm.Tournaments;

namespace tm
{

    [DataContract]
    public struct Substitution : IEquatable<Substitution>
    {
        [DataMember]
        public Player PlayerOut { get; set; }
        [DataMember]
        public Player PlayerIn { get; set; }
        [DataMember]
        public int Minute { get; set; }
        [DataMember]
        public int Period { get; set; }

        public Substitution(Player playerOut, Player playerIn, int minute, int period)
        {
            PlayerOut = playerOut;
            PlayerIn = playerIn;
            Minute = minute;
            Period = period;
        }

        public bool Equals(Substitution other)
        {
            return Minute > other.Minute;
        }
    }

    public struct RetourMatch : IEquatable<RetourMatch>
    {
        public RetourMatchEvenement Evenement { get; set; }
        public object Acteur { get; set; }

        public RetourMatch(RetourMatchEvenement evenement, object acteur)
        {
            Evenement = evenement;
            Acteur = acteur;
        }

        public bool Equals(RetourMatch other)
        {
            throw new NotImplementedException();
        }
    }

    public enum RetourMatchEvenement
    {
        EVENEMENT,
        REMPLACEMENT,
        ACTION,
        FIN_MITEMPS,
        FIN_MATCH
    }

    [DataContract(IsReference =true)]
    public class Statistics
    {
        [DataMember]
        public int RawPossession1 { get; set; }
        [DataMember]
        public int RawPossession2 { get; set; }

        public float HomePossession
        {
            get
            {
                return RawPossession1 + RawPossession2 > 0 ? RawPossession1 / (RawPossession1 + RawPossession2 + 0.0f) : 0;
            }
        }

        public float AwayPossession { get => 1 - HomePossession; }

        [DataMember]
        public int HomeShoots { get; set; }
        [DataMember]
        public int AwayShoots { get; set; }


        public Statistics()
        {
            HomeShoots = 0;
            AwayShoots = 0;
            RawPossession1 = 0;
            RawPossession2 = 0;
        }
    }

    [DataContract(IsReference =true)]
    public class Match
    {
        [DataMember]
        [Key]
        public int id { get; set; }
        [DataMember]
        private Round _round;
        [DataMember]
        private int _minute;
        [DataMember]
        private int _period;
        [DataMember]
        private int _extraTime;
        [DataMember]
        private int _levelDifference;
        [DataMember]
        private float _levelDifferenceRatio;
        [DataMember]
        private List<Player> _compo1Terrain;
        [DataMember]
        private List<Player> _compo2Terrain;

        [DataMember]
        private List<Player> _subs1;
        [DataMember]
        private List<Player> _subs2;
        [DataMember]
        private List<Player> _subs1OnBench;
        [DataMember]
        private List<Player> _subs2OnBench;

        [DataMember]
        private int _score1;
        [DataMember]
        private int _score2;
        [DataMember]
        private List<MatchEvent> _events;
        [DataMember]
        private List<KeyValuePair<string, string>> _actions;
        [DataMember]
        private Statistics _statistics;
        [DataMember]
        private List<Player> _compo1;
        [DataMember]
        private List<Player> _compo2;
        [DataMember]
        private bool _prolongations;
        [DataMember]
        private int _penaltyShootout1;
        [DataMember]
        private int _penaltyShootout2;
        [DataMember]
        private List<bool> _penaltyShoots1;
        [DataMember]
        private List<bool> _penaltyShoots2;
        [DataMember]
        private bool _prolongationsIfDraw;
        [DataMember]
        private Match _firstLeg;
        [DataMember]
        private int _attendance;
        [DataMember]
        private Stadium _stadium;
        [DataMember]
        private List<KeyValuePair<Media,Journalist>> _journalists;
        [DataMember]
        private List<Substitution> _substitutions;
        [DataMember]
        private float _odd1;
        [DataMember]
        private float _oddD;
        [DataMember]
        private float _odd2;
        [DataMember]
        private bool _forfeit;

        
        public int attendance { get => _attendance; }
        [DataMember]
        public DateTime day { get; set; }
        [DataMember]
        public Club home { get; set; }
        [DataMember]
        public Club away { get; set; }
        public int score1 { get => _score1;}
        public int score2 { get => _score2;}
        public List<MatchEvent> events { get => _events; }
        public List<Substitution> substitutions => _substitutions;
        public List<Player> compo1 { get => _compo1; }
        public List<Player> compo2 { get => _compo2; }
        public List<Player> Subs1 => _subs1;
        public List<Player> Subs2 => _subs2;
        public List<Player> Subs1OnBench => _subs1OnBench;
        public List<Player> Subs2OnBench => _subs2OnBench;
        public bool prolongations { get => _prolongations; }
        public int penaltyShootout1 { get => _penaltyShootout1; }
        public int penaltyShootout2 { get => _penaltyShootout2; }
        public List<bool> penaltyShoots1 { get => _penaltyShoots1; }
        public List<bool> penaltyShoots2 { get => _penaltyShoots2; }
        public List<KeyValuePair<Media, Journalist>> medias { get => _journalists; }
        /// <summary>
        /// Game actions description [minutes , actions]
        /// </summary>
        public List<KeyValuePair<string,string>> actions { get => _actions; }
        public Statistics statistics { get => _statistics; }

        public Round Round { get => _round; set => _round = value; }
        public Tournament Tournament => _round.Tournament;
        public float odd1
        {
            get
            {
                if(Played)
                {
                    return _odd1;
                }
                else
                {
                    return GetOdds()[0];
                }
            }
        }
        public float oddD
        {
            get
            {
                if (Played)
                {
                    return _oddD;
                }
                else
                {
                    return GetOdds()[1];
                }
            }
        }
        public float odd2
        {
            get
            {
                if (Played)
                {
                    return _odd2;
                }
                else
                {
                    return GetOdds()[2];
                }
            }
        }
        public bool forfeit => _forfeit;


        [DataMember]
        public bool primeTimeGame { get; set; }

        /*public Tournament Tournament
        {
            get
            {
                Tournament res = null;
                foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
                {
                    foreach(Round t in c.rounds)
                    {
                        foreach(Match m in t.matches)
                        {
                            if (m == this)
                            {
                                res = c;
                            }
                        }
                    }
                }
                //If res is always null, we check in archival tournaments
                if(res == null)
                {
                    foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
                    {
                        foreach(KeyValuePair<int, Tournament> archive in t.previousEditions)
                        {
                            foreach(Round rnd in archive.Value.rounds)
                            {
                                if (rnd.matches.Contains(this))
                                {
                                    res = t;
                                }
                            }
                        }
                    }
                }
                return res;
            }
        }*/

        /*public Round Round
        {
            get
            {
                Round res = null;
                foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
                {
                    foreach (Round t in c.rounds)
                    {
                        foreach (Match m in t.matches)
                        {
                            if (m == this)
                            {
                                res = t;
                            }
                        }
                    }
                }
                //If round is still null, so the round is probably located in archived tournaments
                if(res == null)
                {
                    foreach(Tournament c in Session.Instance.Game.kernel.Competitions)
                    {
                        foreach(KeyValuePair<int, Tournament> oldC in c.previousEditions)
                        {
                            foreach (Round t in oldC.Value.rounds)
                            {
                                foreach (Match m in t.matches)
                                {
                                    if (m == this)
                                    {
                                        res = t;
                                    }
                                }
                            }
                        }
                    }
                }
                return res;
            }
        }*/

        /// <summary>
        /// If the game was played or not
        /// </summary>
        public bool Played
        {
            get
            {
                bool res = _minute > 0 || _period > 1 || forfeit;

                return res;
            }
        }

        public string Time
        {
            get
            {
                int tmp = _minute;
                switch (_period)
                {
                    case 2: 
                        tmp += 45;
                        break;
                    case 3: 
                        tmp += 90;
                        break;
                    case 4 : 
                        tmp += 105;
                        break;
                   default:
                       tmp += 0;
                       break;
                }
                int tmpAdd = _minute - ((_period < 3) ? 45 : 15);
                string time = tmp.ToString();
                if (tmpAdd > 0)
                {
                    time = _period == 1 ? "45" : _period == 2 ? "90" : _period == 3 ? "105" : "120"; 
                    time += "+" + tmpAdd;
                }
                time += "°";
                return time;
            }
        }

        public override string ToString()
        {
            return "[" + day.ToString() + "] " + home.name + "-" + away.name + " (" + Tournament.shortName + ")";
        }

        private int[] GetCumulativeScores()
        {
            int[] res = new int[2];
            int cumulativeScore1 = score1 + _firstLeg.score2;
            int cumulativeScore2 = score2 + _firstLeg.score1;
            if(cumulativeScore1 == cumulativeScore2)
            {
                cumulativeScore1 = score1 + 2 * _firstLeg.score2;
                cumulativeScore2 = 2 * score2 + _firstLeg.score1;
            }

            res[0] = cumulativeScore1;
            res[1] = cumulativeScore2;
            return res;
        }
        
       /// <summary>
       /// Give the winner of the game (useful property for direct knockout rounds)
       /// </summary>
        public Club Winner
        {
            get
            {
                Club c = null;
                if (_firstLeg == null)
                {
                    if (score1 > score2)
                    {
                        c = home;
                    }
                    else if (score1 < score2)
                    {
                        c = away;
                    }
                    else if (prolongations)
                    {
                        if (penaltyShootout1 > penaltyShootout2)
                        {
                            c = home;
                        }
                        else
                        {
                            c = away;
                        }
                    }
                    if (c == null)
                    {
                        c = home;
                    }
                }
                else
                {
                    int[] cumulativeScores = GetCumulativeScores();
                    int cumulativeScore1 = cumulativeScores[0];
                    int cumulativeScore2 = cumulativeScores[1];

                    if (cumulativeScore1 > cumulativeScore2)
                    {
                        c = home;
                    }
                    else if (cumulativeScore2 > cumulativeScore1)
                    {
                        c = away;
                    }
                    else
                    {
                        if (penaltyShootout1 > penaltyShootout2)
                        {
                            c = home;
                        }
                        else
                        {
                            c = away;
                        }
                        if(c == null)
                        {
                            c = home;
                        }
                    }

                }
                if(!Played)
                {
                    c = null;
                }

                return c;
            }
        }

        public Club Looser
        {
            get
            {
                Club c;
                if (Winner == home)
                {
                    c = away;
                }
                else
                {
                    c = home;
                }
                return c;
            }
        }
        
        public int YellowCards
        {
            get
            {
                int res = 0;
                foreach(MatchEvent em in _events)
                {
                    if (em.type == GameEvent.YellowCard)
                    {
                        res++;
                    }
                }
                return res;
            }
        }

        public int RedCards
        {
            get
            {
                int res = 0;
                foreach (MatchEvent em in _events)
                {
                    if (em.type == GameEvent.RedCard)
                    {
                        res++;
                    }
                }
                return res;
            }
        }

        private void DeclareForfeit(Club forfeitTeam)
        {
            _forfeit = true;
            if (forfeitTeam == home)
            {
                _score1 = 0;
                _score2 = 3;
                int pointsSanctions = home.Country().GetSanction(SanctionType.Forfeit).maxPointsDeduction;
                if (pointsSanctions > 0 && Tournament.isChampionship && ((Round as GroupActiveRound) != null))
                {
                    Round.AddPointsDeduction(home, SanctionType.Forfeit, day, pointsSanctions);
                }
            }
            if (forfeitTeam == away)
            {
                _score1 = 3;
                _score2 = 0;
                int pointsSanctions = away.Country().GetSanction(SanctionType.Forfeit).maxPointsDeduction;
                if (pointsSanctions > 0 && Tournament.isChampionship && ((Round as GroupActiveRound) != null))
                {
                    Round.AddPointsDeduction(away, SanctionType.Forfeit, day, pointsSanctions);
                }
            }
            if (forfeitTeam == null && Tournament.isChampionship && ((Round as GroupActiveRound) != null))
            {
                int homePointsSanctions = home.Country().GetSanction(SanctionType.Forfeit).maxPointsDeduction;
                int awayPointsSanctions = away.Country().GetSanction(SanctionType.Forfeit).maxPointsDeduction;
                //Usually homePointsSanctions == awayPointsSanctions
                if (homePointsSanctions > 0)
                {
                    Round.AddPointsDeduction(home, SanctionType.Forfeit, day, homePointsSanctions);
                }
                if(awayPointsSanctions > 0)
                {
                    Round.AddPointsDeduction(away, SanctionType.Forfeit, day, awayPointsSanctions);
                }
            }
        }

        private float CompositionLevel(Club club)
        {
            List<Player> compo = (club == home) ? _compo1Terrain : _compo2Terrain;
            
            float res = 0;
            foreach(Player j in compo)
            {
                res += j.level;
            }
            float teamLevel = res / (11.0f);
            
            int managerLevel;
            if (club.manager != null)
            {
                managerLevel = club.manager.level;
            }
            else
            {
                managerLevel = (int)(teamLevel * 0.8f);
            }

            //We add the level of the manager
            res += 0.175f*(managerLevel - teamLevel);
            return res / (11.0f);
        }

        private Player Card(List<Player> compo)
        {
            List<Player> players = new List<Player>();
            foreach (Player j in compo)
            {
                switch (j.position)
                {
                    case Position.Goalkeeper:
                        for (int i = 0; i < j.level; i++)
                        {
                            players.Add(j);
                        }
                        break;
                    case Position.Defender:
                        for (int i = 0; i < j.level * 2; i++)
                        {
                            players.Add(j);
                        }
                        break;
                    case Position.Midfielder:
                        for (int i = 0; i < j.level; i++)
                        {
                            players.Add(j);
                        }
                        break;
                    case Position.Striker:
                        int k = j.level / 2;
                        for (int i = 0; i < k; i++)
                        {
                            players.Add(j);
                        }
                        break;
                    default:
                        for (int i = 0; i < j.level; i++)
                        {
                            players.Add(j);
                        }
                        break;
                }
            }
            Player res = null;
            if(players.Count > 0)
            {
                res = players[Session.Instance.Random(0, players.Count)];
            }
            return res;
            
        }

        private Player Goalscorer(List<Player> compo)
        {
            List<Player> players = new List<Player>();
            foreach(Player j in compo)
            {
                switch (j.position)
                {
                    case Position.Defender:
                        int k = j.level / 2;
                        for (int i = 0; i < k; i++)
                        {
                            players.Add(j);
                        }
                        break;
                    case Position.Midfielder:
                        for (int i = 0; i < j.level; i++)
                        {
                            players.Add(j);
                        }
                        break;
                    case Position.Striker:
                        for (int i = 0; i < j.level * 2; i++)
                        {
                            players.Add(j);
                        }
                        break;
                    default:
                        for (int i = 0; i < j.level; i++)
                        {
                            players.Add(j);
                        }
                        break;
                }
            }
            Player res = null;
            if (players.Count > 0)
            {
                res = players[Session.Instance.Random(0, players.Count)];
            }
            return res;

        }

        public bool PenaltyShootout
        {
            get
            {
                bool res = false;
                if (_penaltyShootout1 != 0 || _penaltyShootout2 != 0)
                {
                    res = true;
                }
                return res;
            }
        }

        public float HomePossession
        {
            get { return _statistics.HomePossession; }
        }

        public float AwayPossession
        {
            get { return _statistics.AwayPossession; }
        }

        public Stadium stadium
        {
            get { return _stadium != null ? _stadium : home.stadium; }
        }

        public void SetStadium(Stadium stadium)
        {
            _stadium = stadium;
        }

        public int ScoreHalfTime1
        {
            get
            {
                int res = 0;
                foreach(MatchEvent em in _events)
                {
                    if (em.club == home && em.period == 1 &&
                        (em.type == GameEvent.Goal || em.type == GameEvent.AgGoal || em.type == GameEvent.PenaltyGoal))
                    {
                        res++;
                    }
                }
                return res;
            }
        }

        public int ScoreHalfTime2
        {
            get
            {
                int res = 0;
                foreach (MatchEvent em in _events)
                {
                    if (em.club == away && em.period == 1 &&
                        (em.type == GameEvent.Goal || em.type == GameEvent.AgGoal || em.type == GameEvent.PenaltyGoal))
                    {
                        res++;
                    }
                }
                return res;
            }
        }

        private float[] GetOdds()
        {
            return GetOddsFromNormalDistribution();
        }

        private float[] GetOddsFromNormalDistribution()
        {
            float dr = home.elo + 80 - away.elo;

            double pDraw = 2*(1/(Math.Sqrt(2*Math.PI) * (Math.E))) * Math.Exp(-((Math.Pow(dr/100, 2))/(2*Math.Exp(2))));
            double pWin = (1 / (1 + (Math.Pow(10, -dr/400)))) - (0.5 * pDraw);
            double pLose = (1 / (1 + (Math.Pow(10, dr/400)))) - (0.5 * pDraw);
            pWin = 1 / (pWin * 1.1);
            pDraw = 1 / (pDraw * 1.05);
            pLose = 1 / (pLose * 1.1);
            float[] odds = new[] { (float)pWin, (float)pDraw, (float)pLose };

            return odds;
        }

        private void SetOdds()
        {
            float[] odds = GetOdds();
            _odd1 = odds[0];
            _oddD = odds[1];
            _odd2 = odds[2];

        }

        private void SetOddsOld()
        {
            float homeN = home.Level() * 1.05f;
            float awayN = away.Level();
            
            float ratioD = homeN / awayN;
            ratioD *= ratioD * ratioD * ratioD * ratioD;

            float ratioE = awayN / homeN;
            ratioE *= ratioE * ratioE * ratioE * ratioE;

            _odd1 = (float)(1.01f + (1 / Math.Exp((2.5f * ratioD - 3f))));
            _odd2 = (float)(1.01f + (1 / Math.Exp((2.5f * ratioE - 3f))));

            _oddD = (odd1 + odd2) / 2;
        }

        public Match()
        {
            _events = new List<MatchEvent>();
            _compo1 = new List<Player>();
            _compo2 = new List<Player>();
            _compo1Terrain = new List<Player>();
            _compo2Terrain = new List<Player>();
            _journalists = new List<KeyValuePair<Media, Journalist>>();
            _actions = new List<KeyValuePair<string, string>>();
            _subs1 = new List<Player>();
            _subs2 = new List<Player>();
            _subs1OnBench = new List<Player>();
            _subs2OnBench = new List<Player>();
            _substitutions = new List<Substitution>();
            _penaltyShoots1 = new List<bool>();
            _penaltyShoots2 = new List<bool>();
        }

        public Match(Round round, Club homeTeam, Club awayTeam, DateTime matchDay, bool prolongationsIfDraw, Match firstLeg = null)
        {
            _round = round;
            _stadium = null;
            home = homeTeam;
            away = awayTeam;
            day = matchDay;
            _score1 = 0;
            _score2 = 0;
            _penaltyShootout2 = 0;
            _penaltyShootout1 = 0;
            _extraTime = 0;
            _statistics = new Statistics();
            _prolongations = false;
            _events = new List<MatchEvent>();
            _compo1 = new List<Player>();
            _compo2 = new List<Player>();
            _prolongationsIfDraw = prolongationsIfDraw;
            _firstLeg = firstLeg;
            _minute = 0;
            _period = 1;
            _compo1Terrain = new List<Player>();
            _compo2Terrain = new List<Player>();
            _attendance = 0;
            _journalists = new List<KeyValuePair<Media, Journalist>>();
            _actions = new List<KeyValuePair<string, string>>();
            primeTimeGame = false;
            _subs1 = new List<Player>();
            _subs2 = new List<Player>();
            _subs1OnBench = new List<Player>();
            _subs2OnBench = new List<Player>();
            _substitutions = new List<Substitution>();
            _penaltyShoots1 = new List<bool>();
            _penaltyShoots2 = new List<bool>();
        }

        /// <summary>
        /// Reprogram a postponed game at the date the most convenient for both clubs
        /// Offset : start to search a date from the match day + offset
        /// </summary>
        public void Reprogram(int offset)
        {
            bool foundDate = false;
            DateTime dateBase = this.day.AddDays(offset);
            dateBase = dateBase.AddHours(-dateBase.Hour + Session.Instance.Random(18,22));
            while(!foundDate)
            {
                
                if(!home.CloseGame(dateBase,3) && !away.CloseGame(dateBase, 3))
                {
                    foundDate = true;
                    this.day = dateBase;
                }
                else
                {
                    dateBase = dateBase.AddDays(1);
                }
            }
            CheckConflicts();
            medias.Clear();
        }

        /// <summary>
        /// Check match date conflicts with other games and reschedule the game if needed
        /// Rules :
        /// - If the game is in conflict with another game but this game has a higher priority, the other game is rescheduled
        /// - If not, check every free dates between the day of the game and the end of the round
        /// - If no free dates are available but if there is a scheduled game with a lower priority between the date of the game and the end of the round, reschedule this other game.
        /// </summary>
        public void CheckConflicts()
        {
            int gamePriority = Round.programmation.gamesPriority;
            List<Match> homeGames = home.Games;
            homeGames.Sort(new MatchDateComparator());
            List<Match> awayGames = away.Games;
            awayGames.Sort(new MatchDateComparator());

            List<DateTime> homeBestDates = new List<DateTime>();
            List<DateTime> awayBestDates = new List<DateTime>();
            //Store games with lower priority and the date where this game could be played instead
            Dictionary<Match, DateTime> homePossibleDates = new Dictionary<Match, DateTime>();
            Dictionary<Match, DateTime> awayPossibleDates = new Dictionary<Match, DateTime>();

            /*
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Tournament cdf = fr.Cup(1);
            Tournament cdl = fr.Cup(2);
            if (Tournament == cdf || Tournament == cdl)
            {
                Console.WriteLine("[CC " + Tournament.shortName + "]");
                Console.WriteLine("Check conflit of " + home.name + "-" + away.name + " (" + Round.Tournament.name + ", " + Round.name + ", priorité de "+ gamePriority + ") le " + day.ToShortDateString());
            }*/

            DateTime baseDay = day;
            DateTime dt = day;
            DateTime dtEnd = Round.DateEndRound();
            bool findCommonFreeDate = false;
            //Check for free dates and games with lower priority between game date and end of the round
            while (!Utils.CompareDates(dt, dtEnd) && !findCommonFreeDate)
            {
                bool homeFreeDate = true;
                
                foreach(Match hMatch in homeGames)
                {
                    if(hMatch != this)
                    {
                        //If this game is too close
                        if (Utils.DaysNumberBetweenTwoDates(hMatch.day, dt) < 3)
                        {
                            int hMatchPriority = hMatch.Round.programmation.gamesPriority;

                            //If this game is scheduled the same day and his priority is lower, this game will be rescheduled and let the priority to the current game so the date is always free
                            if(!dt.EqualsDate(baseDay) || hMatchPriority >= gamePriority)
                            {
                                homeFreeDate = false;
                            }
                            //Keep this game in memory because it has a lower priority
                            if (hMatchPriority < gamePriority && hMatch.day.EqualsDate(dt))
                            {
                                if (!homePossibleDates.ContainsKey(hMatch))
                                {
                                    homePossibleDates.Add(hMatch, dt);
                                }
                            }

                        }
                    }
                }
                if (homeFreeDate)
                {
                    homeBestDates.Add(dt);
                }
                //Same logic for away games
                bool awayFreeDate = true;
                foreach (Match aMatch in awayGames)
                {
                    if(aMatch != this)
                    {
                        if (Utils.DaysNumberBetweenTwoDates(aMatch.day, dt) < 3)
                        {
                            int aMatchPriority = aMatch.Round.programmation.gamesPriority;
                            if (!dt.EqualsDate(baseDay) || aMatchPriority >= gamePriority)
                            {
                                awayFreeDate = false;
                            }
                            if (aMatchPriority < gamePriority && aMatch.day.EqualsDate(dt))
                            {
                                if (!awayPossibleDates.ContainsKey(aMatch))
                                {
                                    awayPossibleDates.Add(aMatch, dt);
                                }
                            }
                        }
                    }
                }
                if (awayFreeDate)
                {
                    awayBestDates.Add(dt);
                }
                findCommonFreeDate = awayFreeDate && homeFreeDate;

                dt = dt.AddDays(1);
            }

            //Check the first common free date between the two clubs
            DateTime bestDate = new DateTime(2000, 1, 1);
            for(int i = 0; i < homeBestDates.Count && bestDate.Year == 2000; i++)
            {
                if (awayBestDates.Contains(homeBestDates[i]))
                {
                    bestDate = homeBestDates[i];
                }
            }

            //If a common free date is found
            if (bestDate.Year > 2000)
            {
                day = bestDate;
                //In the case where a game with a lower priority was at the same date of this game, this game is rescheduled
                foreach(Match hGame in homeGames)
                {
                    if (hGame != this && Utils.DaysNumberBetweenTwoDates(hGame.day, bestDate) < 3)
                    {
                        hGame.CheckConflicts();
                    }
                }
                //Same logic with away games
                foreach (Match aGame in awayGames)
                {
                    if (aGame != this && Utils.DaysNumberBetweenTwoDates(aGame.day, bestDate) < 3)
                    {
                        aGame.CheckConflicts();
                    }
                }
            }
            else
            {
                //In the case of no common free dates are found, this game take the place of a game with a lower priority
                bool find = false;

                foreach (KeyValuePair<Match, DateTime> kvpHome in homePossibleDates)
                {
                    if (!find && awayBestDates.Contains(kvpHome.Value))
                    {
                        find = true;
                        day = kvpHome.Value;
                        kvpHome.Key.CheckConflicts(); //Reschedule the other game
                    }
                }
                //Same logic for away games if a common game is still not found
                foreach (KeyValuePair<Match, DateTime> kvpAway in awayPossibleDates)
                {
                    if (!find && homeBestDates.Contains(kvpAway.Value))
                    {
                        find = true;
                        day = kvpAway.Value;
                        kvpAway.Key.CheckConflicts();
                    }
                }
            }
            if(!Utils.CompareDates(baseDay, day))
            {
                //Utils.Debug("[" + Tournament.shortName + "] " + home.name + "-" + away.name + " reprogrammé le " + day.ToShortDateString());
            }
        }

        private void SetAttendance()
        {
            //Neutral venue
            if(_stadium != null)
            {
                _attendance = (int)((home.supporters / 2 * (Session.Instance.Random(6, 14) / 10.0f)) + (away.supporters / 2 * (Session.Instance.Random(6, 14) / 10.0f)));
            }
            else
            {
                _attendance = (int)(home.supporters * (Session.Instance.Random(6, 14) / 10.0f));
                float prestigeModifier = (away.Level() / home.Level()) + ((1 - (away.Level() / home.Level())) / 2f);
                _attendance = (int)(_attendance * prestigeModifier);
            }
            if (_attendance > stadium.capacity)
            {
                _attendance = stadium.capacity;
            }
            else if(_attendance < 0)
            {
                _attendance = 0;
            }
            if(home as CityClub != null && _stadium == null)
            {
                (home as CityClub).ModifyBudget(_attendance * home.ticketPrice, BudgetModificationReason.StadiumAttendance);
            }
        }

        public void Replacements(Club c)
        {
            List<Player> initialCompo;
            List<Player> compoOnGroud;
            List<Player> terrain;
            List<Player> subs;
            List<Player> originalSubs;

            if (c == home)
            {
                terrain = new List<Player>(_compo1Terrain);
                subs = new List<Player>(_subs1OnBench);
                compoOnGroud = _compo1Terrain;
                originalSubs = _subs1OnBench;
                initialCompo = _compo1;
            }
            else
            {
                terrain = new List<Player>(_compo2Terrain);
                subs = new List<Player>(_subs2OnBench);
                compoOnGroud = _compo2Terrain;
                originalSubs = _subs2OnBench;
                initialCompo = _compo2;
            }

            terrain.Sort(new PlayerCompositionComparator(Tournament.name == Utils.friendlyTournamentName || (!Tournament.IsInternational() && Tournament.isChampionship) ));
            subs.Sort(new PlayerCompositionComparator(Tournament.name == Utils.friendlyTournamentName || (!Tournament.IsInternational() && Tournament.isChampionship)));
            int nbSubs = 0;
            foreach (Substitution s in _substitutions)
            {
                if (initialCompo.Contains(s.PlayerOut))
                {
                    nbSubs++;
                }
            }

            int i = terrain.Count - 1;

            bool pursue = nbSubs < 3 && i > 0;

            while (pursue)
            {
                Player weakest = terrain[i];
                List<Player> subsForThisPlayer = Utils.PlayersByPosition(subs, weakest.position);
                if(subsForThisPlayer.Count > 0 && subsForThisPlayer[0].effectiveLevel > weakest.effectiveLevel)
                {
                    compoOnGroud.Remove(weakest);
                    compoOnGroud.Add(subsForThisPlayer[0]);
                    originalSubs.Remove(subsForThisPlayer[0]);
                    _substitutions.Add(new Substitution(weakest, subsForThisPlayer[0], _minute, _period));
                    pursue = false;
                }
                i--;
                if(i < 0)
                {
                    pursue = false;
                }
            }
            
        }

        public void SetCompo()
        {
            _compo1 = new List<Player>(home.Composition(this));
            _compo2 = new List<Player>(away.Composition(this));
            _compo1Terrain = new List<Player>(_compo1);
            _compo2Terrain = new List<Player>(_compo2);
            _subs1 = new List<Player>(home.Subs(this, _compo1, 7));
            _subs2 = new List<Player>(away.Subs(this, _compo2, 7));
            _subs1OnBench = new List<Player>(_subs1);
            _subs2OnBench = new List<Player>(_subs2);

            if(_compo1.Count < 8)
            {
                DeclareForfeit(home);
            }
            if (_compo2.Count < 8)
            {
                DeclareForfeit(away);
            }
            if (_compo2.Count < 8 && _compo1.Count < 8)
            {
                DeclareForfeit(null);
            }

        }

        private void UpdatePlayersMatchPlayedStat(Club club, List<Player> compo)
        {
            foreach(Player p in compo)
            {

                if(!p.Statistics.GamesPlayed.Any(x => x.Club == club))
                {
                    p.Statistics.GamesPlayed.Add(new PlayerClubStatistic(club.id, 0));
                }
                p.Statistics.GamesPlayed.Find(x => x.Club == club).Statistic++;
            }
        }

        /// <summary>
        /// Set directly a composition by passing it in parameter
        /// TODO: Remove this function and create one generic for player selected compo and auto compo
        /// </summary>
        /// <param name="compo">Players</param>
        /// <param name="club">Club</param>
        public void SetCompo(List<Player> compo, List<Player> subs, Club club)
        {
            if(club == home)
            {
                _compo1 = new List<Player>(compo);
                _compo1Terrain = new List<Player>(compo);
                _subs1 = new List<Player>(subs);
                if(_compo1.Count < 11)
                {
                    DeclareForfeit(club);
                }
            }
            else if(club == away)
            {
                _compo2 = new List<Player>(compo);
                _compo2Terrain = new List<Player>(compo);
                _subs2 = new List<Player>(subs);
                if (_compo2.Count < 11)
                {
                    DeclareForfeit(club);
                }
            }
        }

        private void EndOfGame()
        {
            SetOdds();
            UpdateRecords();
            UpdateElo();
            if(home as NationalTeam != null)
            {
                (home as NationalTeam).UpdateFifaPointsAfterGame(this);
            }
            if (away as NationalTeam != null)
            {
                (away as NationalTeam).UpdateFifaPointsAfterGame(this);
            }
        }

        private void UpdateElo(Club c, float k, bool noHomeAdjustement)
        {

            Club opponent = c == this.home ? away : home;
            int scoreClub = c == this.home ? score1 : score2;
            int scoreOpponent = c == this.home ? score2 : score1;
            int goalDifference = Math.Abs(scoreClub - scoreOpponent);

            float Ro = c.elo;
            float dr = c.elo - opponent.elo;
            if(!noHomeAdjustement)
            {
                if (c == home)
                {
                    dr = dr + 100;
                }
                else
                {
                    dr = dr - 100;
                }
            }
            float W = this.Winner == c ? 1 : score1 == score2 ? 0.5f : 0;
            float We = 1.0f/((float)Math.Pow(10, -dr/400)+1);
            float kAdjustment = goalDifference == 2 ? 0.5f : goalDifference == 3 ? 0.75f : goalDifference > 3 ? 0.75f + ((goalDifference-3.0f)/8.0f) : 0;
            k = k + (k * kAdjustment);

            float newElo = Ro + k * (W - We);

            c.UpdateElo(newElo);
        }

        private void UpdateElo()
        {
            /*Int. match : 
            60 for World Cup;
            50 for continental championship;
            40 for qualifiers;
            30 for all other tournaments;
            20 for friendly matches.

            Club match :

            50 for international competitions
            40 for domestic competitions
            30 for domestic cups
            20 for friendly matches
             */


            Tournament tournament = this.Tournament;
            bool isInternational = tournament.IsInternational();

            int k = 40;
            if((this.home as NationalTeam) != null)
            {
                if (tournament.name == Utils.friendlyTournamentName) //Hum
                {
                    k = 20;
                }
                else if (isInternational && tournament.level == 3) //World cup
                {
                    k = 60;
                }
                else if(isInternational && tournament.level == 2) //Other international tournament
                {
                    k = 50;
                }
                else if(isInternational && tournament.level == 3) //Qualifiers
                {
                    k = 40;
                }
                else
                {
                    k = 30;
                }
            }
            else
            {
                if(tournament.name == Utils.friendlyTournamentName) //Hum
                {
                    k = 20;
                }
                else if(isInternational)
                {
                    k = 50;
                }
                else if(!isInternational && !tournament.isChampionship)
                {
                    k = 30;
                }
                else
                {
                    k = 40;
                }
            }

            UpdateElo(home, k, Round.rules.Contains(Rule.HostedByOneCountry));
            UpdateElo(away, k, Round.rules.Contains(Rule.HostedByOneCountry));

        }

        private void UpdateRecords()
        {
            home.UpdateRecords(this);
            away.UpdateRecords(this);
        }

        public void CalculateLevelDifference()
        {
            float diffF = Math.Abs(CompositionLevel(home)*1.04f - CompositionLevel(away));

            this._levelDifference = (int)diffF;
            this._levelDifferenceRatio = (CompositionLevel(home) * 1.04f) / CompositionLevel(away);
        }

        public List<RetourMatch> NextMinute()
        {
            List<RetourMatch> lookbacks = new List<RetourMatch>();
            if (!forfeit)
            {
                //At the beginning of the game
                if (_minute == 0 && _period == 1)
                {
                    CalculateLevelDifference();
                    SetAttendance();
                    UpdatePlayersMatchPlayedStat(home, _compo1);
                    UpdatePlayersMatchPlayedStat(away, _compo2);
                }

                Club a = home;
                Club b = away;

                _minute++;
                lookbacks = PlayMinute(a, b);

                int periodDuration = (_period < 3) ? 45 : 15;
                //End of regular time
                if (_minute == periodDuration)
                {
                    _extraTime = Session.Instance.Random(1, 6);
                }

                //End half-time
                if (_minute == periodDuration + _extraTime)
                {
                    _period++;
                    _minute = 0;
                    _extraTime = 0;
                    if (_period == 3)
                    {
                        if ((_prolongationsIfDraw && (_score1 == _score2)) || SecondLegIsDraw())
                        {
                            _prolongations = true;
                        }
                        else
                        {
                            lookbacks.Add(new RetourMatch(RetourMatchEvenement.FIN_MATCH, null));
                            EndOfGame();

                        }
                    }
                    if (_period == 5)
                    {
                        if ((_prolongationsIfDraw && _score1 == _score2) || SecondLegIsDraw())
                        {
                            PlayPenaltyShootout();
                        }
                        lookbacks.Add(new RetourMatch(RetourMatchEvenement.FIN_MATCH, null));
                        EndOfGame();
                    }
                }
            }
            else
            {
                lookbacks.Add(new RetourMatch(RetourMatchEvenement.FIN_MATCH, null));
            }
            return lookbacks;
        }

        public void Play()
        {
            if(!forfeit)
            {
                Club a = home;
                Club b = away;
                CalculateLevelDifference();
                SetAttendance();
                UpdatePlayersMatchPlayedStat(home, _compo1);
                UpdatePlayersMatchPlayedStat(away, _compo2);

                for (_period = 1; _period < 3; _period++)
                {
                    for (_minute = 1; _minute < 50; _minute++)
                    {
                        PlayMinute(a, b);
                    }
                }

                if ((_prolongationsIfDraw && (_score1 == _score2)) || SecondLegIsDraw())
                {
                    _prolongations = true;
                    for (_period = 3; _period < 5; _period++)
                    {
                        for (_minute = 1; _minute < 16; _minute++)
                        {
                            PlayMinute(a, b);
                        }
                    }
                    if ((_prolongationsIfDraw && _score1 == _score2) || SecondLegIsDraw())
                    {
                        PlayPenaltyShootout();
                    }
                }

                if (home == Session.Instance.Game.club || away == Session.Instance.Game.club)
                {
                    string res = ArticleGenerator.Instance.GenerateArticle(this);
                    Article article = new Article(Session.Instance.Game.kernel.NextIdArticle(), res, "", new DateTime(day.Year, day.Month, day.Day), 2);
                    Session.Instance.Game.articles.Add(article);
                }
                EndOfGame();
            }
        }

        private bool SecondLegIsDraw()
        {
            bool res = false;
            if(_firstLeg != null)
            {
                int[] cumulativeScores = GetCumulativeScores();

                if (cumulativeScores[0] == cumulativeScores[1])
                {
                    res = true;
                }
            }
            return res;
        }

        private void PlayPenaltyShootout()
        {
            _penaltyShootout1 = 0;
            _penaltyShootout2 = 0;

            for(int i = 0; i<5; i++)
            {
                if(Math.Abs(_penaltyShootout2-_penaltyShootout1) <= 5-i)
                {
                    if (Session.Instance.Random(1, 4) != 1)
                    {
                        _penaltyShootout1++;
                        _penaltyShoots1.Add(true);
                    }
                    else
                    {
                        _penaltyShoots1.Add(false);
                    }
                }

                if (Math.Abs(_penaltyShootout2 - _penaltyShootout1) <= 5-i)
                {
                    if (Session.Instance.Random(1, 4) != 1)
                    {
                        _penaltyShootout2++;
                        _penaltyShoots2.Add(true);
                    }
                    else
                    {
                        _penaltyShoots2.Add(false);
                    }
                }
            }
            while(_penaltyShootout1 == _penaltyShootout2)
            {
                if (Session.Instance.Random(1, 4) != 1)
                {
                    _penaltyShootout1++;
                    _penaltyShoots1.Add(true);
                }
                else
                {
                    _penaltyShoots1.Add(false);
                }

                if (Session.Instance.Random(1, 4) != 1)
                {
                    _penaltyShootout2++;
                    _penaltyShoots2.Add(true);
                }
                else
                {
                    _penaltyShoots2.Add(false);
                }
            }
        }

        private List<RetourMatch> PlayMinute(Club a, Club b)
        {
            List<RetourMatch> lookbacks = new List<RetourMatch>();

            //Every 10 minutes from second periods, clubs do substitutions
            if (_minute % 10 == 0 && _period > 1)
            {
                Replacements(home);
                Replacements(away);
            }

            foreach (Player j in _compo1)
            {
                if (Session.Instance.Random(2, 7) == 3)
                {
                    j.energy--;
                }
            }
            foreach (Player j in _compo2)
            {
                if (Session.Instance.Random(2, 7) == 3)
                {
                    j.energy--;
                }
            }

            int diff = _levelDifference;
            float diffRatio = _levelDifferenceRatio;

            if(diffRatio > 1)
            {
                diffRatio = 1 / diffRatio;
                Club temp = a;
                a = b;
                b = temp;
            }

            if (diffRatio < 0.05)
            {
                lookbacks = MatchIteration(a, b, 22, 22, 208, 295);
            }
            else if (diffRatio < 0.1)
            {
                lookbacks = MatchIteration(a, b, 22, 22, 208, 280);
            }
            else if (diffRatio >= 0.1 && diffRatio < 0.2)
            {
                lookbacks = MatchIteration(a, b, 22, 22, 208, 256);
            }
            else if (diffRatio >= 0.2 && diffRatio < 0.3)
            {
                lookbacks = MatchIteration(a, b, 22, 22, 208, 246);
            }
            else if (diffRatio >= 0.3 && diffRatio < 0.4)
            {
                lookbacks = MatchIteration(a, b, 22, 22, 208, 240);
            }
            else if (diffRatio >= 0.4 && diffRatio < 0.5)
            {
                lookbacks = MatchIteration(a, b, 22, 23, 208, 235);
            }
            else if (diffRatio >= 0.5 && diffRatio < 0.6)
            {
                lookbacks = MatchIteration(a, b, 22, 23, 208, 230);
            }
            else if (diffRatio >= 0.6 && diffRatio < 0.65)
            {
                lookbacks = MatchIteration(a, b, 22, 23, 208, 224);
            }
            else if (diffRatio >= 0.65 && diffRatio < 0.7)
            {
                lookbacks = MatchIteration(a, b, 21, 23, 208, 222);
            }
            else if (diffRatio >= 0.7 && diffRatio < 0.74)
            {
                lookbacks = MatchIteration(a, b, 21, 23, 208, 221);
            }
            else if (diffRatio >= 0.74 && diffRatio < 0.78)
            {
                lookbacks = MatchIteration(a, b, 21, 23, 208, 220);
            }
            else if (diffRatio >= 0.78 && diffRatio < 0.81)
            {
                lookbacks = MatchIteration(a, b, 21, 24, 208, 220);
            }
            else if (diffRatio >= 0.81 && diffRatio < 0.83)
            {
                lookbacks = MatchIteration(a, b, 21, 24, 208, 219);
            }
            else if (diffRatio >= 0.83 && diffRatio < 0.89)
            {
                lookbacks = MatchIteration(a, b, 21, 24, 208, 218);
            }
            else if (diffRatio >= 0.85 && diffRatio < 0.87)
            {
                lookbacks = MatchIteration(a, b, 21, 25, 208, 218);
            }
            else if (diffRatio >= 0.87 && diffRatio < 0.89)
            {
                lookbacks = MatchIteration(a, b, 21, 25, 208, 217);
            }
            else if (diffRatio >= 0.89 && diffRatio < 0.90)
            {
                lookbacks = MatchIteration(a, b, 21, 25, 208, 216);
            }
            else if (diffRatio >= 0.90 && diffRatio < 0.92)
            {
                lookbacks = MatchIteration(a, b, 21, 25, 208, 215);
            }
            else if (diffRatio >= 0.92 && diffRatio < 0.95)
            {
                lookbacks = MatchIteration(a, b, 21, 25, 208, 214);
            }
            else if (diffRatio >= 0.95 && diffRatio < 0.98)
            {
                lookbacks = MatchIteration(a, b, 21, 26, 208, 214);
            }
            else if (diffRatio >= 0.98 && diffRatio < 1.01)
            {
                lookbacks = MatchIteration(a, b, 21, 26, 208, 213);
            }
            return lookbacks;
        }

        private List<RetourMatch> MatchIteration(Club a, Club b, int aMin, int aMax, int bMin,int bMax)
        {

            List<RetourMatch> res = new List<RetourMatch>();

            int random = Session.Instance.Random(0, 500);

            //Shoots
            if (random >= aMin && random <= aMax + ((aMax + 1 - aMin) * 4))
            {
                if (a == home)
                {
                    _statistics.HomeShoots++;
                    Shot(home);
                    res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                }
                else
                {
                    _statistics.AwayShoots++;
                    Shot(away);
                    res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                }
            }
            else if (random >= bMin && random <= bMax + ((bMax + 1 - bMin) * 4))
            {
                if (a == home)
                {
                    _statistics.AwayShoots++;
                    Shot(away);
                    res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                }
                else
                {
                    _statistics.HomeShoots++;
                    Shot(home);
                    res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                }
            }

            //Goals
            if (random >= aMin && random <= aMax)
            {
                if (a == home)
                {
                    _score1++;
                }
                else
                {
                    _score2++;
                }
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                Goal(a);
            }
            else if (random >= bMin && random <= bMax)
            {
                if (a == home)
                {
                    _score2++;
                }
                else
                {
                    _score1++;
                }
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
                Goal(b);
            }
            //Yellow cards
            else if (random >= 4 && random <= 9)
            {
                YellowCard(a);
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
            }
            else if (random >= 14 && random <= 19)
            {
                YellowCard(b);
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
            }
            //Red cards
            //Second random to reduce chance to have a red card
            else if (random == 2 && Session.Instance.Random(1, 10) < 6)
            {
                RedCard(a);
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
            }
            else if (random == 3 && Session.Instance.Random(1, 10) < 6)
            {
                RedCard(b);
                res.Add(new RetourMatch(RetourMatchEvenement.EVENEMENT, null));
            }
            

            return res;
        }

        private void Goal(Club c)
        {
            Player p = Goalscorer(c == home ? compo1 : compo2);
            if(p != null)
            {
                MatchEvent em = new MatchEvent(GameEvent.Goal, c, p, _minute, _period);
                if (p != null)
                {
                    if (!p.Statistics.Goals.Any(x => x.Club == c))
                    {
                        p.Statistics.Goals.Add(new PlayerClubStatistic(c.id, 0));
                    }
                    p.Statistics.Goals.Find(x => x.Club == c).Statistic++;
                }
                _events.Add(em);
                AddAction(em.MinuteToString, Session.Instance.Game.kernel.Commentary(em));
            }
        }

        private void YellowCard(Club c)
        {
            Player j = Card(c == home ? _compo1Terrain : _compo2Terrain);
            if(j != null)
            {

                bool secondYellowCard = false;
                foreach (MatchEvent ev in _events)
                {
                    if (ev.player == j && ev.type == GameEvent.YellowCard)
                    {
                        secondYellowCard = true;
                    }
                }
                
                MatchEvent em = new MatchEvent(GameEvent.YellowCard, c, j, _minute, _period);
                _events.Add(em);
                AddAction(em.MinuteToString, Session.Instance.Game.kernel.Commentary(em));

                //If it's a second yellow card, a red card is given
                if (secondYellowCard == true)
                {
                    List<Player> compo = c == home ? _compo1Terrain : _compo2Terrain;
                    compo.Remove(j);
                    CalculateLevelDifference();
                    em = new MatchEvent(GameEvent.RedCard, c, j, _minute, _period);
                    _events.Add(em);

                }
            }
        }

        private void RedCard(Club c)
        {
            List<Player> compo = c == home ? _compo1Terrain : _compo2Terrain;
            Player j = Card(compo);
            if(j != null)
            {
                compo.Remove(j);
                CalculateLevelDifference();
                MatchEvent em = new MatchEvent(GameEvent.RedCard, c, j, _minute, _period);
                _events.Add(em);
                AddAction(em.MinuteToString, Session.Instance.Game.kernel.Commentary(em));
            }
        }

        private void Shot(Club c)
        {
            List<Player> compo = c == home ? _compo1Terrain : _compo2Terrain;
            Player j = Goalscorer(compo);
            if(j != null)
            {
                MatchEvent em = new MatchEvent(GameEvent.Shot, c, j, _minute, _period);
                _events.Add(em);
                AddAction(em.MinuteToString, Session.Instance.Game.kernel.Commentary(em));
            }
        }

        /// <summary>
        /// Add action description in the match
        /// </summary>
        /// <param name="minute">Minute of the action</param>
        /// <param name="action">Description of the action</param>
        public void AddAction(string minute, string action)
        {
            _actions.Add(new KeyValuePair<string, string>(minute, action));
        }

        public string PenaltyShootoutToString()
        {
            return String.Format("({0}-{1} t.)", penaltyShootout1, penaltyShootout2);
        }

        public string ScoreToString(bool withTabs, bool withAet, string aet)
        {
            string res = Played ? score1 + " - " + score2 : "";
            if(prolongations && withAet)
            {
                res = res + " " + aet;
            }
            if(PenaltyShootout && withTabs)
            {
                res = res + " (" + penaltyShootout1 + "-" + penaltyShootout2 + " p)";
            }
            if(forfeit)
            {
                res = res + " forf.";
            }
            return res;
        }

        /// <summary>
        /// Used for tests.
        /// Force game score
        /// </summary>
        /// <param name="score1"></param>
        /// <param name="score2"></param>
        public void Force(int score1, int score2)
        {
            _score1 = score1;
            _score2 = score2;
        }
    }
}