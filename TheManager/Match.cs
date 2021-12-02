using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using LiveCharts.Wpf;
using TheManager.Comparators;

namespace TheManager
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

        public int attendance { get => _attendance; }
        [DataMember]
        public DateTime day { get; set; }
        [DataMember]
        public Club home { get; set; }
        [DataMember]
        public Club away { get; set; }
        public int score1 { get => _score1; }
        public int score2 { get => _score2; }
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
        public List<KeyValuePair<Media, Journalist>> journalists { get => _journalists; }
        /// <summary>
        /// Game actions description [minutes , actions]
        /// </summary>
        public List<KeyValuePair<string,string>> actions { get => _actions; }
        public Statistics statistics { get => _statistics; }
        [DataMember]
        public float odd1 { get; set; }
        [DataMember]
        public float oddD { get; set; }
        [DataMember]
        public float odd2 { get; set; }

        [DataMember]
        public bool primeTimeGame { get; set; }


        public Tournament Tournament
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
        }

        public Round Round
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
        }

        /// <summary>
        /// If the game was played or not
        /// </summary>
        public bool Played
        {
            get
            {
                bool res = _minute > 0 || _period > 1;

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

        public Stadium Stadium
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

        private void SetOdds()
        {
            float homeN = home.Level() * 1.05f;
            float awayN = away.Level();
            
            float ratioD = homeN / awayN;
            ratioD *= ratioD * ratioD * ratioD * ratioD;

            float ratioE = awayN / homeN;
            ratioE *= ratioE * ratioE * ratioE * ratioE;

            odd1 = (float)(1.01f + (1 / Math.Exp((2.5f * ratioD - 3f))));
            odd2 = (float)(1.01f + (1 / Math.Exp((2.5f * ratioE - 3f))));
            
            oddD = (odd1 + odd2) / 2;
        }

        public Match(Club homeTeam, Club awayTeam, DateTime matchDay, bool prolongationsIfDraw, Match firstLeg = null)
        {
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
            SetOdds();
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
        }

        private void SetAttendance()
        {
            _attendance = (int)(home.supporters * (Session.Instance.Random(6, 14) / 10.0f));
            float prestigeModifier = (away.Level() / home.Level()) + ((1-(away.Level() / home.Level()))/2f);
            _attendance = (int)(_attendance * prestigeModifier);
            if (_attendance > home.stadium.capacity)
            {
                _attendance = home.stadium.capacity;
            }
            else if(_attendance < 0)
            {
                _attendance = 0;
            }
            if(home as CityClub != null)
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

            UpdatePlayersMatchPlayedStat(_compo1);
            UpdatePlayersMatchPlayedStat(_compo2);

        }

        private void UpdatePlayersMatchPlayedStat(List<Player> compo)
        {
            foreach(Player p in compo)
            {
                p.playedGames++;
            }
        }

        /// <summary>
        /// Set directly a composition by passing it in parameter
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
            }
            else if(club == away)
            {
                _compo2 = new List<Player>(compo);
                _compo2Terrain = new List<Player>(compo);
                _subs2 = new List<Player>(subs);
            }
            UpdatePlayersMatchPlayedStat(compo);
        }

        private void EndOfGame()
        {
            UpdateRecords();
            if(home as NationalTeam != null)
            {
                (home as NationalTeam).UpdateFifaPointsAfterGame(this);
            }
            if (away as NationalTeam != null)
            {
                (away as NationalTeam).UpdateFifaPointsAfterGame(this);
            }
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
            //At the beginning of the game
            if(_minute == 0 && _period == 1)
            {
                CalculateLevelDifference();
                SetAttendance();
            }

            Club a = home;
            Club b = away;

            _minute++;
            
            List<RetourMatch> lookbacks = PlayMinute(a, b);

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
                if(_period == 3)
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
                if(_period == 5)
                {
                    if ((_prolongationsIfDraw && _score1 == _score2) || SecondLegIsDraw())
                    {
                        PlayPenaltyShootout();
                    }
                    lookbacks.Add(new RetourMatch(RetourMatchEvenement.FIN_MATCH, null));
                    EndOfGame();
                }
            }

            
            return lookbacks;

        }

        public void Play()
        {
            Club a = home;
            Club b = away;
            CalculateLevelDifference();
            SetAttendance();

            for (_period = 1; _period < 3; _period++)
            {
                for (_minute = 1; _minute < 50; _minute++)
                {
                    PlayMinute(a, b);
                }
            }

            if((_prolongationsIfDraw && (_score1 == _score2)) || SecondLegIsDraw())
            {
                _prolongations = true;
                for(_period = 3; _period<5;_period++)
                {
                    for(_minute = 1; _minute<16; _minute++)
                    {
                        PlayMinute(a, b);
                    }
                }
                if((_prolongationsIfDraw && _score1 == _score2) || SecondLegIsDraw())
                {
                    PlayPenaltyShootout();
                }
            }

            if(home == Session.Instance.Game.club || away == Session.Instance.Game.club)
            {
                string res = ArticleGenerator.Instance.GenerateArticle(this);
                Article article = new Article(res, "", new DateTime(day.Year, day.Month, day.Day), 2);
                Session.Instance.Game.articles.Add(article);
            }
            EndOfGame();
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
            /*if (diff < 1) IterationMatch(a, b, 1, 6, 8, 13);
            if (diff >= 1 && diff <= 2) IterationMatch(a, b, 1, 7, 8, 13);
            if (diff >= 3 && diff <= 4) IterationMatch(a, b, 1, 8, 9, 14);
            if (diff >= 5 && diff <= 7) IterationMatch(a, b, 1, 9, 10, 14);
            if (diff >= 8 && diff <= 10) IterationMatch(a, b, 1, 10, 12, 16);
            if (diff >= 11 && diff <= 14) IterationMatch(a, b, 1, 11, 15, 19);
            if (diff >= 15 && diff <= 18) IterationMatch(a, b, 1, 12, 17, 21);
            if (diff >= 19 && diff <= 23) IterationMatch(a, b, 1, 13, 20, 22);
            if (diff >= 24 && diff <= 28) IterationMatch(a, b, 1, 14, 23, 25);
            if (diff >= 29 && diff <= 33) IterationMatch(a, b, 1, 16, 23, 25);
            if (diff >= 34 && diff <= 40) IterationMatch(a, b, 1, 18, 23, 24);
            if (diff >= 40 && diff <= 47) IterationMatch(a, b, 1, 20, 23, 24);
            if (diff >= 48 && diff <= 55) IterationMatch(a, b, 1, 22, 23, 24);
            if (diff >= 55 && diff <= 63) IterationMatch(a, b, 1, 24, 25, 26);
            if (diff >= 64 && diff <= 70) IterationMatch(a, b, 1, 26, 40, 40);
            if (diff >= 71 && diff <= 79) IterationMatch(a, b, 1, 36, 40, 40);
            if (diff >= 80 && diff <= 89) IterationMatch(a, b, 1, 39, 40, 40);
            if (diff >= 90 && diff <= 100) IterationMatch(a, b, 1, 43, 44, 44);*/

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
            Player j = Goalscorer(c == home ? compo1 : compo2);
            if(j != null)
            {
                MatchEvent em = new MatchEvent(GameEvent.Goal, c, j, _minute, _period);
                if (j != null)
                {
                    j.goalsScored++;
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

        public string ScoreToString()
        {
            string res = Played ? score1 + " - " + score2 : "";
            if(prolongations)
            {
                res = res + " ap";
            }
            if(PenaltyShootout)
            {
                res = res + " (" + penaltyShootout1 + "-" + penaltyShootout2 + " t.)";
            }
            return res;
        }
    }
}