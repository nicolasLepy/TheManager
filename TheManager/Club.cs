using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;
using System.Runtime.Serialization;

namespace TheManager
{
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
                    if (game.home == this || game.away == this) res.Add(game);
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
                if (diff < threshold) closeGame = true;
            }
            return closeGame;
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
                        
                        foreach (Club cl in tournament.rounds[0].Clubs)
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
        public abstract float Level();

        public float Stars
        {
            get
            {
                float stars = 0.5f;
                float level = Level();
                if (level < 40)
                    stars = 0.5f;
                else if (level < 50)
                    stars = 1f;
                else if (level < 57)
                    stars = 1.5f;
                else if (level < 62)
                    stars = 2f;
                else if (level < 66)
                    stars = 2.5f;
                else if (level < 69)
                    stars = 3f;
                else if (level < 72)
                    stars = 3.5f;
                else if (level < 75)
                    stars = 4f;
                else if (level < 79)
                    stars = 4.5f;
                else stars = 5f;
                
                return stars;
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

        public List<Player> Composition()
        {
            List<Player> res = new List<Player>();

            List<Player> joueursPosition = ListEligiblePlayersByPosition(Position.Goalkeeper);
            joueursPosition.Sort(new PlayerCompositionComparator());
            if (joueursPosition.Count >= 1)
                res.Add(joueursPosition[0]);

            joueursPosition = ListEligiblePlayersByPosition(Position.Defender);
            joueursPosition.Sort(new PlayerCompositionComparator());
            
            if (joueursPosition.Count > 0) res.Add(joueursPosition[0]);
            if (joueursPosition.Count > 1) res.Add(joueursPosition[1]);
            if (joueursPosition.Count > 2) res.Add(joueursPosition[2]);
            if (joueursPosition.Count > 3) res.Add(joueursPosition[3]);
            

            joueursPosition = ListEligiblePlayersByPosition(Position.Midfielder);
            joueursPosition.Sort(new PlayerCompositionComparator());

            if (joueursPosition.Count > 0) res.Add(joueursPosition[0]);
            if (joueursPosition.Count > 1) res.Add(joueursPosition[1]);
            if (joueursPosition.Count > 2) res.Add(joueursPosition[2]);
            if (joueursPosition.Count > 3) res.Add(joueursPosition[3]);


            joueursPosition = ListEligiblePlayersByPosition(Position.Striker);
            joueursPosition.Sort(new PlayerCompositionComparator());
            if (joueursPosition.Count > 0) res.Add(joueursPosition[0]);
            if (joueursPosition.Count > 1) res.Add(joueursPosition[1]);


            return res;
        }

        public void SetTicketPrice()
        {
            int level = (int)Level();
            int price = 0;

            if (level < 20) price = 1;
            else if (level < 30) price = 2;
            else if (level < 40) price = 3;
            else if (level < 50) price = 5;
            else if (level < 60) price = 10;
            else if (level < 70) price = 20;
            else if (level < 80) price = 30;
            else price = 45;
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


        public override string ToString()
        {
            return name;
        }


    }
}