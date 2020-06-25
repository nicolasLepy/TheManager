using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{

    [DataContract]
    public class MatchEventCommentary
    {
        [DataMember]
        private GameEvent _event;
        [DataMember]
        private List<string> _commentaries;

        public List<string> commentaries { get => _commentaries; }
        public GameEvent gameEvent { get => _event; }

        public MatchEventCommentary(GameEvent gameEvent)
        {
            _event = gameEvent;
            _commentaries = new List<string>();
        }

        public string Commentary(MatchEvent em)
        {
            string brutCommentary = commentaries[Session.Instance.Random(0, commentaries.Count - 1)];
            brutCommentary = brutCommentary.Replace(" CLUB ", " " + em.club.shortName + " ");
            brutCommentary = brutCommentary.Replace(" JOUEUR ", " " + em.player.Nom + " ");
            return brutCommentary;
        }
    }
    

    [DataContract(IsReference =true)]
    public class Kernel
    {

        

        [DataMember]
        private List<Club> _clubs;
        [DataMember]
        private List<Player> _freePlayers;
        [DataMember]
        private List<Manager> _freeManagers;
        [DataMember]
        private List<Continent> _continents;
        [DataMember]
        private List<Langue> _languages;
        [DataMember]
        private List<Media> _medias;
        [DataMember]
        private List<MatchEventCommentary> _matchCommentaries;
        [DataMember]
        private List<Journaliste> _freeJournalists;

        public List<Club> Clubs { get => _clubs; }

        public List<Tournament> Competitions
        {
            get
            {
                List<Tournament> res = new List<Tournament>();
                foreach(Continent c in _continents)
                {
                    foreach (Tournament cp in c.Tournaments()) res.Add(cp);
                    foreach (Pays p in c.countries) foreach (Tournament cp in p.Tournaments()) res.Add(cp);
                }
                return res;
            }
        }
        public List<Player> freePlayers { get => _freePlayers; }
        public List<Manager> freeManagers { get => _freeManagers; }
        public List<Continent> continents { get => _continents; }
        public List<Langue> languages { get => _languages; }
        public List<Media> medias { get => _medias; }
        public List<MatchEventCommentary> matchCommentaries { get => _matchCommentaries; }
        public List<Journaliste> freeJournalists { get => _freeJournalists; }

        public Kernel()
        {
            _clubs = new List<Club>();
            _freePlayers = new List<Player>();
            _continents = new List<Continent>();
            _languages = new List<Langue>();
            _medias = new List<Media>();
            _freeManagers = new List<Manager>();
            _matchCommentaries = new List<MatchEventCommentary>();
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.Goal));
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.PenaltyGoal));
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.YellowCard));
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.RedCard));
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.Shot));
            _freeJournalists = new List<Journaliste>();
        }

        public Ville String2City(string name)
        {
            Ville res = null;
            foreach(Continent c in _continents)
            {
                foreach(Pays p in c.countries)
                {
                    foreach(Ville v in p.Villes)
                    {
                        if (v.Nom == name) res = v;
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Get the list of all games from all current tournaments
        /// </summary>
        public List<Match> Matchs
        {
            get
            {
                List<Match> res = new List<Match>();
                foreach(Tournament c in Competitions)
                {
                    foreach(Tour t in c.rounds)
                    {
                        foreach (Match m in t.Matchs) res.Add(m);
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// List of all transferable players of a tournament
        /// </summary>
        /// <param name="c">The tournament</param>
        /// <returns></returns>
        public List<Player> TransfertList(Tournament c)
        {
            List<Player> players = new List<Player>();
            Tour tournamentRound = c.rounds[0];

            foreach(Club club in tournamentRound.Clubs)
            {
                CityClub cc = club as CityClub;
                if(cc != null)
                {
                    foreach (Contract ct in cc.contracts)
                    {
                        if (ct.isTransferable) players.Add(ct.player);
                    }
                }
            }

            return players;
        }

        public Pays String2Country(string country)
        {
            Pays res = null;

            foreach(Continent c in _continents)
            {
                foreach(Pays p in c.countries)
                {
                    if (p.Name() == country) res = p;
                }
            }
            return res;
        }

        public Stade String2Stadium(string stadium)
        {
            Stade res = null;

            foreach (Continent c in _continents)
            {
                foreach(Pays p in c.countries)
                {
                    foreach(Stade s in p.Stades)
                    {
                        if (s.Nom == stadium) res = s;
                    }
                }
            }

            return res;
        }

        public Tournament String2Tournament(string name)
        {
            Tournament res = null;
            foreach(Tournament tournament in Competitions)
            {
                if (tournament.name == name) res = tournament;
            }
            return res;
        }

        public Club String2Club(string name)
        {
            Club res = null;
            foreach(Club c in _clubs)
            {
                if (c.name == name) res = c;
            }

            return res;
        }

        public Langue String2Language(string name)
        {
            Langue res = null;

            foreach (Langue l in _languages)
            {
                if (l.Nom == name) res = l;
            }

            return res;
        }

        public Continent String2Continent(string name)
        {
            Continent res = null;
            foreach (Continent c in _continents)
            {
                if (c.Name() == name) res = c;
            }
            return res;
        }

        public int NumberPlayersOfCountry(Pays p)
        {
            int res = 0;
            foreach(Player j in _freePlayers)
            {
                if (j.Nationalite == p)
                {
                    res++;
                }
            }
            foreach(Club c in _clubs)
            {
                if (c as CityClub != null)
                {
                    foreach (Player j in c.Players())
                    {
                        if (j.Nationalite == p)
                        {
                            res++;
                        }
                    }
                }
            }
            return res;
        }

        public List<Player> GetPlayersByCountry(Pays country)
        {
            List<Player> res = new List<Player>();
            foreach (Player j in _freePlayers)
            {
                if (j.Nationalite == country) res.Add(j); ;
            }
            foreach (Club c in _clubs)
            {
                if (c as CityClub != null)
                {
                    foreach (Player j in c.Players())
                    {
                        if (j.Nationalite == country) res.Add(j);
                    }
                }
            }
            return res;
        }

        public void NationalTeamsCall()
        {
            foreach(Club c in _clubs)
            {
                SelectionNationale sn = c as SelectionNationale;
                if (sn != null)
                {
                    sn.AppelSelection(GetPlayersByCountry(sn.Pays));
                }
            }
        }

        /// <summary>
        ///  Retires free players who are too old
        /// </summary>
        public void RetirementOfFreePlayers()
        {
            List<Player> retiredPlayers = new List<Player>();
            foreach(Player player in _freePlayers)
            {
                if (player.Age > 33)
                {
                    if (Session.Instance.Random(1, 3) == 1)
                    {
                        retiredPlayers.Add(player);
                    }
                }
            }

            foreach (Player j in retiredPlayers)
            {
                _freePlayers.Remove(j);
            }
        }

        /// <summary>
        /// Get a localisation (continent or country) from a name
        /// </summary>
        /// <returns></returns>
        public ILocalisation String2Localisation(string name)
        {
            ILocalisation res = null;
            foreach(Continent c in _continents)
            {
                if (c.Name() == name) res = c;
                foreach(Pays p in c.countries)
                {
                    if (p.Name() == name)
                    {
                        res = p;
                    }
                }
            }
            return res;
        }

        public ILocalisation LocalisationTournament(Tournament tournament)
        {
            ILocalisation res = null;
            foreach(Continent c in _continents)
            {
                if (c.Tournaments().Contains(tournament))
                {
                    res = c;
                }

                foreach (Pays p in c.countries)
                {
                    if (p.Tournaments().Contains(tournament)) res = p;
                }
            }
            return res;
        }

        public void AddFriendlyGame(Match m)
        {
            Tournament amc = String2Tournament("Matchs amicaux");
            amc.rounds[0].Matchs.Add(m);
        }

        public void AddMatchCommentary(GameEvent gameEvent, string commentary)
        {
            foreach(MatchEventCommentary cem in _matchCommentaries)
            {
                if (cem.gameEvent == gameEvent)
                {
                    cem.commentaries.Add(commentary);
                }
            }
        }

        public string Commentary(MatchEvent matchEvent)
        {
            string res = "";
            foreach(MatchEventCommentary cem in _matchCommentaries)
            {
                if (cem.gameEvent == matchEvent.type)
                {
                    res = cem.Commentary(matchEvent);
                }
            }

            return res;
        }

    }
}
