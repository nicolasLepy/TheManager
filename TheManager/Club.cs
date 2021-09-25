using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.ComponentModel;

namespace TheManager
{

    [DataContract]
    public struct ClubRecords : IEquatable<ClubRecords>
    {

        [DataMember]
        public Match BiggestWin { get; set; }
        [DataMember]
        public Match BiggestLose { get; set; }

        public bool Equals(ClubRecords other)
        {
            return false;
        }

    }

    public enum BudgetModificationReason
    {
        [Description("Sponsor")]
        SponsorGrant,
        [Description("Salaires")]
        PayWages,
        [Description("Jour de match")]
        StadiumAttendance,
        [Description("Compétition")]
        TournamentGrant,
        [Description("Centre de formation")]
        UpdateFormationFacilities,
        [Description("Transfert")]
        TransferIndemnity
    }

    public struct BudgetEntry : IEquatable<BudgetEntry>
    {
        public BudgetModificationReason Reason { get; set; }
        public float Amount { get; set; }
        public DateTime Date { get; set; }

        public BudgetEntry(DateTime date, float amount, BudgetModificationReason reason)
        {
            Date = date;
            Amount = amount;
            Reason = reason;
        }

        public bool Equals(BudgetEntry other)
        {
            return Amount > other.Amount;
        }
    }
    
    [DataContract(IsReference =true)]
    [KnownType(typeof(CityClub))]
    [System.Xml.Serialization.XmlInclude(typeof(CityClub))]
    [KnownType(typeof(NationalTeam))]
    [System.Xml.Serialization.XmlInclude(typeof(NationalTeam))]
    [KnownType(typeof(ReserveClub))]
    [System.Xml.Serialization.XmlInclude(typeof(ReserveClub))]
    public abstract class Club
    {
        [DataMember]
        private string _name;
        [DataMember]
        private string _shortName;
        [DataMember]
        private Manager _manager;
        [DataMember]
        private int _reputation;
        [DataMember]
        private int _supporters;
        [DataMember]
        protected int _formationFacilities;
        [DataMember]
        private Stadium _stadium;
        [DataMember]
        private string _logo;
        [DataMember]
        private int _ticketPrice;
        [DataMember]
        private ClubRecords _records = new ClubRecords();
        

        [DataMember]
        private string _goalMusic;

        public string name { get => _name; }
        public Manager manager { get => _manager; set => _manager = value; }
        public int reputation { get => _reputation; }
        public int supporters { get => _supporters; set => _supporters = value; }
        public int formationFacilities { get => _formationFacilities;}
        public Stadium stadium { get => _stadium; }
        public string logo { get => _logo; }
        public string shortName { get => _shortName; }
        public int ticketPrice { get => _ticketPrice; }
        public string goalMusic { get => _goalMusic; }
        public ClubRecords records { get => _records; }

        /// <summary>
        /// List of games played by the club
        /// </summary>
        public List<Match> Games
        {
            get
            {
                List<Match> res = new List<Match>();

                foreach (Match game in Session.Instance.Game.kernel.Matchs)
                {
                    if (game.home == this || game.away == this)
                    {
                        res.Add(game);
                    }
                }
                res.Sort(new MatchDateComparator());
                return res;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="date">From the date</param>
        /// <param name="threshold">Offset days from the date</param>
        /// <returns>True if the club play a game in the date interval in the threshold</returns>
        public bool CloseGame(DateTime date, int threshold)
        {
            bool closeGame = false;
            List<Match> gamesList = Games;
            foreach (Match match in gamesList)
            {
                int diff = Utils.DaysNumberBetweenTwoDates(date.Date, match.day);
                if (diff < threshold)
                {
                    closeGame = true;
                }
            }
            return closeGame;
        }

        public Match NextGame
        {
            get
            {
                Match res = null;
                List<Match> games = new List<Match>(this.Games);
                games.Sort(new MatchDateComparator());
                bool pursue = true;
                int i = 0;
                while (pursue && i < games.Count)
                {
                    Match g = games[i];
                    if(Utils.IsBefore(Session.Instance.Game.date, g.day))
                    {
                        pursue = false;
                        res = g;
                    }
                    i++;
                }
                return res;
            }
        }

        /// <summary>
        /// Donne le nombre de jours entre la date et le match le plus proche joué
        /// </summary>
        /// <param name="date">Regarder par rapport à cette date</param>
        /// <returns></returns>
        /*public int NombreJoursMatchPlusProche(DateTime date)
        {
                int res = -1;
                List<Match> matchs = Matchs;
                foreach(Match m in matchs)
                {
                    if(m.Jour.CompareTo(date) > 0)
                    {
                        //On a dépassé ajd
                        int diffM = Utils.NombreJoursEntreDeuxDates(m.Jour, date); //Match à venir
                        int indexMatch = matchs.IndexOf(m);
                        int diffN = indexMatch > 0 ? Utils.NombreJoursEntreDeuxDates(matchs[indexMatch-1].Jour, date) : -1; //Dernier match du club
                        res = diffM >= diffN ? diffN : diffM;
                    }
                }
                if (res == -1 && matchs.Count > 0)
                    res = Utils.NombreJoursEntreDeuxDates(date, matchs[matchs.Count - 1].Jour);
            if (res == -1) res = 365;
                return res;
            
        }*/

        /// <summary>
        /// Championship where play the club
        /// <returns>null if the club don't play in championship (for example national teams)</returns>
        /// </summary>
        public Tournament Championship
        {
            get
            {
                Tournament res = null;

                foreach(Tournament tournament in Session.Instance.Game.kernel.Competitions)
                {
                    if(tournament.isChampionship)
                    {
                        
                        foreach (Club cl in tournament.rounds[0].clubs)
                        {
                            if (cl == this)
                            {
                                res = tournament;
                            }
                        }
                            
                                
                    }
                }

                return res;
            }
        }

        public abstract List<Player> Players();
        public float Level()
        {
            float level = 0;

            List<Player> players = new List<Player>(Players());
            players.Sort(new PlayerComparator(true, PlayerAttribute.LEVEL));

            int total = 0;
            for (int i = 0; i < 16; i++)
            {
                if (players.Count > i)
                {
                    level += players[i].level;
                    total++;
                }
            }
            return level / (total + 0.0f);
        }
        public float Potential()
        {
            float potential = 0;

            List<Player> players = new List<Player>(Players());
            players.Sort(new PlayerComparator(true, PlayerAttribute.POTENTIAL));

            int total = 0;
            for (int i = 0; i < 16; i++)
            {
                if (players.Count > i)
                {
                    potential += players[i].potential;
                    total++;
                }
            }
            return potential / (total + 0.0f);
        }

        public float Stars
        {
            get
            {
                return Utils.GetStars(Level());
            }
        }

        protected Club(string name, Manager manager, string shortName, int reputation, int supporters, int formationFacilities, string logo, Stadium stadium, string goalMusic)
        {
            _name = name;
            _manager = manager;
            _shortName = shortName;
            _reputation = reputation;
            _supporters = supporters;
            _formationFacilities = formationFacilities;
            _logo = logo;
            _stadium = stadium;
            _goalMusic = goalMusic;
        }

        public List<Player> ListPlayersByPosition(Position position)
        {
            return Utils.PlayersByPosition(Players(), position);
        }

        private List<Player> ListEligiblePlayersByPosition(Position position)
        {
            List<Player> res = new List<Player>();
            foreach (Player j in Players())
            {
                if (j.position == position && !j.suspended)
                {
                    res.Add(j);
                }
            }
            return res;
        }

        /// <summary>
        /// Add players of a position in the composition for a game
        /// </summary>
        /// <param name="position">The position needed</param>
        /// <param name="composition">The current composition to add player</param>
        /// <param name="playersNumberToTake">Number of players of this position to put in the composition</param>
        private void BuildComposition(Position position, List<Player> composition, int playersNumberToTake)
        {
            List<Player> playersOfPosition = ListEligiblePlayersByPosition(position);
            playersOfPosition.Sort(new PlayerCompositionComparator());

            for (int i = 0; i < playersNumberToTake; i++)
            {
                if (playersOfPosition.Count > i)
                {
                    composition.Add(playersOfPosition[i]);
                }

            }
        }

        /// <summary>
        /// Get subs for a game
        /// </summary>
        /// <param name="composition">List of players starting the game on the ground</param>
        public List<Player> Subs(List<Player> composition, int nbSubs)
        {
            List<Player> subs = new List<Player>();
            List<Player> allPlayersAvailable = new List<Player>();
            foreach(Player p in Players())
            {
                if (!composition.Contains(p))
                {
                    allPlayersAvailable.Add(p);
                }
            }

            //Get the sub goalkeeper
            List<Player> goalkeppers = Utils.PlayersByPosition(allPlayersAvailable, Position.Goalkeeper);
            if(goalkeppers.Count > 0)
            {
                subs.Add(goalkeppers[0]);
            }
            foreach(Player p in goalkeppers)
            {
                allPlayersAvailable.Remove(p);
            }
            int remainingPlayers = nbSubs - subs.Count;

            allPlayersAvailable.Sort(new PlayerCompositionComparator());
            for(int i = 0; i < remainingPlayers; i++)
            {
                if(allPlayersAvailable.Count > i)
                {
                    subs.Add(allPlayersAvailable[i]);
                }
            }

            return subs;
        }

        public List<Player> Composition()
        {
            List<Player> res = new List<Player>();

            BuildComposition(Position.Goalkeeper, res, 1);
            BuildComposition(Position.Defender, res, 4);
            BuildComposition(Position.Midfielder, res, 4);
            BuildComposition(Position.Striker, res,2);
            
            return res;
        }

        public void SetTicketPrice()
        {
            int level = (int)Level();
            int price = 0;

            if (level < 20)
            {
                price = 1;
            }
            else if (level < 30)
            {
                price = 2;
            }
            else if (level < 40)
            {
                price = 3;
            }
            else if (level < 50)
            {
                price = 5;
            }
            else if (level < 60)
            {
                price = 10;
            }
            else if (level < 70)
            {
                price = 20;
            }
            else if (level < 80)
            {
                price = 30;
            }
            else
            {
                price = 45;
            }
            _ticketPrice = price;
        }

        /// <summary>
        /// Change the current manager and put the old manager in free managers list
        /// </summary>
        /// <param name="newManager">The new manager of the club</param>
        public void ChangeManager(Manager newManager)
        {
            Session.Instance.Game.kernel.freeManagers.Add(_manager);
            _manager = newManager;
        }

        public void UpdateRecords(Match g)
        {
            if (g.Tournament.name != Utils.friendlyTournamentName)
            {
                if (_records.BiggestWin == null || g.Winner == this && Math.Abs(g.score1 - g.score2) > Math.Abs(_records.BiggestWin.score1 - _records.BiggestWin.score2))
                {
                    _records.BiggestWin = g;
                }
                if (_records.BiggestLose == null || g.Looser == this && Math.Abs(g.score1 - g.score2) > Math.Abs(_records.BiggestLose.score1 - _records.BiggestLose.score2))
                {
                    _records.BiggestLose = g;
                }
            }
        }


        public override string ToString()
        {
            return name;
        }


    }
}