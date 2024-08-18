using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using tm.Comparators;
using tm.Tournaments;

namespace tm
{

    [DataContract(IsReference =true)]
    public class MatchEventCommentary
    {
        [DataMember]
        private GameEvent _event;
        [DataMember]
        private List<string> _commentaries;

        public List<string> commentaries { get => _commentaries; }
        public GameEvent gameEvent { get => _event; }

        public MatchEventCommentary()
        {
            _commentaries = new List<string>();
        }

        public MatchEventCommentary(GameEvent gameEvent)
        {
            _event = gameEvent;
            _commentaries = new List<string>();
        }

        public string Commentary(MatchEvent em)
        {
            string brutCommentary = commentaries[Session.Instance.Random(0, commentaries.Count - 1)];
            brutCommentary = brutCommentary.Replace(" CLUB ", " " + em.club.shortName + " ");
            brutCommentary = brutCommentary.Replace(" JOUEUR ", " " + em.player.lastName + " ");
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
        private int _retiredPlayersCount;
        [DataMember]
        private List<Manager> _freeManagers;
        [DataMember]
        private Continent _world;
        [DataMember]
        private Association _worldAssociation;
        [DataMember]
        private List<Language> _languages;
        [DataMember]
        private List<GenericCalendar> _genericCalendars;
        [DataMember]
        private List<AudioSource> _audioSources;
        [DataMember]
        private List<Media> _medias;
        [DataMember]
        private List<MatchEventCommentary> _matchCommentaries;
        [DataMember]
        private List<Journalist> _freeJournalists;

        [DataMember]
        private int _nextIdStadium = 0;
        [DataMember]
        private int _nextIdPerson = 0;
        [DataMember]
        private int _nextIdTournament = 0;
        [DataMember]
        private int _nextIdRound = 0;
        [DataMember]
        private int _nextIdArticle = 0;
        [DataMember]
        private int _nextIdMedia = 0;
        [DataMember]
        private int _nextIdCity = 0;
        [DataMember]
        private int _nextIdCountry = 0;
        [DataMember]
        private int _nextIdContinent = 0;
        [DataMember]
        private int _nextIdContract = 0;
        [DataMember]
        private int _nextIdAudio = 0;
        [DataMember]
        private int _nextIdClub = 0;

        public int NextIdStadium() => ++_nextIdStadium;
        public int NextIdPerson() => ++_nextIdPerson;
        public int NextIdTournament() => ++_nextIdTournament;
        public int NextIdRound() => ++_nextIdRound;
        public int NextIdArticle() => ++_nextIdArticle;
        public int NextIdMedia() => ++_nextIdMedia;
        public int NextIdCity() => ++_nextIdCity;
        public int NextIdCountry() => ++_nextIdCountry;
        public int NextIdContinent() => ++_nextIdContinent;
        public int NextIdContract() => ++_nextIdContract;
        public int NextIdAudio() => ++_nextIdAudio;
        public int NextIdClub() => ++_nextIdClub;

        public void SetClubIdIterator(int id)
        {
            _nextIdClub = id;
        }


        public List<Club> Clubs { get => _clubs; }


        public List<Association> GetAllAssociations()
        {
            List<Association> res = new List<Association>();
            res.Add(_worldAssociation);
            res.AddRange(_worldAssociation.GetAllChilds());
            return res;
        }


        [NotMapped]
        public List<Tournament> Competitions
        {
            get
            {
                List<Tournament> tournaments = new List<Tournament>(_world.GetAllTournaments());
                tournaments.AddRange(_worldAssociation.GetAllTournaments());
                return tournaments;
            }
        }
        public List<Player> freePlayers { get => _freePlayers; }
        public List<Manager> freeManagers { get => _freeManagers; }
        public Continent world { get => _world; set => _world = value; }
        public Association worldAssociation { get => _worldAssociation; set => _worldAssociation = value; }
        public List<Language> languages { get => _languages; }
        public List<Media> medias { get => _medias; }
        public List<MatchEventCommentary> matchCommentaries { get => _matchCommentaries; }
        public List<Journalist> freeJournalists { get => _freeJournalists; }
        public List<AudioSource> audioSources => _audioSources;
        public int retiredPlayersCount => _retiredPlayersCount;

        public Kernel()
        {
            _retiredPlayersCount = 0;
            _clubs = new List<Club>();
            _freePlayers = new List<Player>();
            _languages = new List<Language>();
            _medias = new List<Media>();
            _freeManagers = new List<Manager>();
            _matchCommentaries = new List<MatchEventCommentary>();
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.Goal));
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.PenaltyGoal));
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.YellowCard));
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.RedCard));
            _matchCommentaries.Add(new MatchEventCommentary(GameEvent.Shot));
            _freeJournalists = new List<Journalist>();
            _genericCalendars = new List<GenericCalendar>();
            _audioSources = new List<AudioSource>();
        }

        public GenericCalendar GetGenericCalendar(string name)
        {
            GenericCalendar res = null;

            foreach(GenericCalendar gc in _genericCalendars)
            {
                if (gc.Name == name)
                {
                    res = gc;
                }
            }

            return res;
        }

        public City String2City(string name)
        {
            City res = null;
            foreach(Continent c in _world.continents)
            {
                foreach(Country p in c.countries)
                {
                    foreach(City v in p.cities)
                    {
                        if (v.Name == name)
                        {
                            res = v;
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Get the list of all games from all current tournaments
        /// </summary>
        [NotMapped]
        public List<Match> Matchs
        {
            get
            {
                List<Match> res = new List<Match>();
                foreach(Tournament c in Competitions)
                {
                    foreach(Round t in c.rounds)
                    {
                        foreach (Match m in t.matches)
                        {
                            res.Add(m);
                        }
                    }
                }

                return res;
            }
        }

        [NotMapped]
        public List<Player> Players
        {
            get
            {
                List<Player> res = new List<Player>(freePlayers);
                foreach(Club club in this.Clubs)
                {
                    res.AddRange(club.Players());
                }
                return res.Distinct().ToList();
            }
        }


        public List<Match> MatchsOfDate(DateTime date)
        {
            List<Match> res = new List<Match>();
            foreach (Tournament c in Competitions)
            {
                foreach (Round t in c.rounds)
                {
                    foreach (Match m in t.matches)
                    {
                        if(Utils.CompareDates(m.day, date))
                        {
                            res.Add(m);
                        }
                    }
                }
            }

            return res;
        }

        public List<Player> TransferList(int clubChampionshipLevel, Country country)
        {
            List<Player> players = new List<Player>();

            foreach(Tournament c in Competitions)
            {
                if (c.isChampionship && c.level <= clubChampionshipLevel)
                {
                    if(c.level == 1 || (LocalisationTournament(c).Name() == country.Name() && c.level <= 2))
                    {
                        players.AddRange(TransferList(c));
                    }
                }
            }
            return players;
        }

        /// <summary>
        /// List of all transferable players of a tournament
        /// </summary>
        /// <param name="c">The tournament</param>
        /// <returns></returns>
        public List<Player> TransferList(Tournament c)
        {
            List<Player> players = new List<Player>();
            Round tournamentRound = c.rounds[0];

            foreach(Club club in tournamentRound.clubs)
            {
                CityClub cc = club as CityClub;
                if(cc != null)
                {
                    foreach (Contract ct in cc.contracts)
                    {
                        if (ct.isTransferable)
                        {
                            players.Add(ct.player);
                        }
                    }
                }
            }

            return players;
        }

        public Country String2Country(string country, bool idName = false)
        {
            Country res = null;

            foreach(Continent c in _world.continents)
            {
                foreach(Country p in c.countries)
                {
                    if (!idName && p.Name() == country)
                    {
                        res = p;
                    }
                    if (idName && p.DbName == country)
                    {
                        res = p;
                    }
                }
            }
            return res;
        }

        public Country DbCountryName2Country(string country)
        {
            Country res = null;

            foreach (Continent c in _world.continents)
            {
                foreach (Country p in c.countries)
                {
                    if (p.DbName == country)
                    {
                        res = p;
                    }
                }
            }
            return res;
        }

        public Stadium String2Stadium(string stadium)
        {
            Stadium res = null;

            foreach (Continent c in _world.continents)
            {
                foreach(Country p in c.countries)
                {
                    foreach(Stadium s in p.stadiums)
                    {
                        if (s.name == stadium)
                        {
                            res = s;
                        }
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
                if (tournament.name == name)
                {
                    res = tournament;
                }
            }
            return res;
        }

        public Club GetClubById(int id)
        {
            Club res = null;
            foreach(Club club in _clubs)
            {
                if(club.id == id)
                {
                    res = club;
                }
            }
            return res;
        }

        public Club String2Club(string name)
        {
            Club res = null;
            foreach(Club c in _clubs)
            {
                if (c.name == name)
                {
                    res = c;
                }
            }

            return res;
        }

        public Language String2Language(string name)
        {
            Language res = null;

            foreach (Language l in _languages)
            {
                if (l.name == name)
                {
                    res = l;
                }
            }

            return res;
        }

        public Continent String2Continent(string name)
        {
            return _world.String2Continent(name);
        }

        public Association String2Association(string name)
        {
            return _worldAssociation.String2Association(name);
        }

        public int PlayersCount()
        {
            int res = _freePlayers.Count;
            foreach (Club c in _clubs)
            {
                if (c as CityClub != null)
                {
                    res += c.Players().Count;
                }
            }
            return res;
        }

        public int NumberPlayersOfCountry(Country p)
        {
            int res = 0;
            foreach(Player j in _freePlayers)
            {
                if (j.nationality == p)
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
                        if (j.nationality == p)
                        {
                            res++;
                        }
                    }
                }
            }
            return res;
        }

        public List<Player> GetPlayersByCountry(Country country)
        {
            List<Player> res = new List<Player>();
            foreach (Player j in _freePlayers)
            {
                if (j.nationality == country)
                {
                    res.Add(j);
                }
            }
            foreach (Club c in _clubs)
            {
                if (c as CityClub != null)
                {
                    foreach (Player j in c.Players())
                    {
                        if (j.nationality == country)
                        {
                            res.Add(j);
                        }
                    }
                }
            }
            return res;
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

            _retiredPlayersCount += retiredPlayers.Count;
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
            foreach(Continent c in _world.GetAllContinents())
            {
                if (c.Name() == name)
                {
                    res = c;
                }
                foreach(Country p in c.countries)
                {
                    if (p.Name() == name)
                    {
                        res = p;
                    }
                }
            }
            return res;
        }

        public Association GetAssociation(int idAssociation)
        {
            Association res = null;
            foreach (Continent c in _world.continents)
            {
                foreach (Country p in c.countries)
                {
                    if (res == null)
                    {
                        res = p.GetAssociation(idAssociation);
                    }
                }
            }

            return res;
        }

        public Association GetAssociation(ILocalisation localisation)
        {
            Association res = null;
            foreach(Association a in GetAllAssociations())
            {
                if(a.localisation == localisation)
                {
                    res = a;
                    break;
                }
            }
            return res;
        }

        public Continent ContinentTournament(Tournament tournament)
        {
            Continent res = null;

            foreach(Continent c in _world.GetAllContinents())
            {
                foreach(Tournament t in c.Tournaments())
                {
                    if(t == tournament)
                    {
                        res = c;
                    }
                }
                foreach(Country cy in c.countries)
                {
                    foreach(Tournament t in cy.Tournaments())
                    {
                        if(t == tournament)
                        {
                            res = c;
                        }
                    }
                }
            }

            return res;
        }

        public ILocalisation LocalisationTournament(Tournament tournament)
        {
            ILocalisation res = null;
            foreach(Continent c in _world.GetAllContinents())
            {
                if (c.Tournaments().Contains(tournament))
                {
                    res = c;
                }

                foreach (Country p in c.countries)
                {
                    if (p.Tournaments().Contains(tournament))
                    {
                        res = p;
                    }
                }
            }
            foreach (Association association in GetAllAssociations())
            {
                if (association.tournaments.Contains(tournament))
                {
                    res = association;
                }
            }
            if (res == null)
            {
                foreach(Tournament t in Competitions)
                {
                    foreach (KeyValuePair<int, Tournament> tt in t.previousEditions)
                    {
                        if (tt.Value == tournament)
                        {
                            res = LocalisationTournament(t);
                        }
                    }
                }
                /*foreach (Continent c in _world.continents)
                {
                    foreach(Tournament t in c.Tournaments())
                    {
                        foreach(KeyValuePair<int,Tournament> tt in t.previousEditions)
                        {
                            if(tt.Value == tournament)
                            {
                                res = c;
                            }
                        }
                    }

                    foreach (Country p in c.countries)
                    {
                        foreach (Tournament t in p.Tournaments())
                        {
                            foreach (KeyValuePair<int, Tournament> tt in t.previousEditions)
                            {
                                if (tt.Value == tournament)
                                {
                                    res = p;
                                }
                            }
                        }

                    }
                }*/
            }
            return res;
        }


        public Match AddFriendlyGame(Club home, Club away, DateTime gameDate)
        {
            Tournament amc = String2Tournament("Matchs amicaux");
            Match game = new Match(amc.rounds[0], home, away, gameDate, false);
            Calendar.Hour(game);
            amc.rounds[0].matches.Add(game);
            return game;
        }

        public void CancelFriendlyGame(Match m)
        {
            Tournament amc = String2Tournament("Matchs amicaux");
            amc.rounds[0].matches.Remove(m);
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

        public void AddAudioSource(AudioSource audioSource)
        {
            _audioSources.Add(audioSource);
        }

        public void AddGenericCalendar(GenericCalendar calendar)
        {
            _genericCalendars.Add(calendar);
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

        public List<NationalTeam> FifaRanking()
        {
            List<NationalTeam> nationalsTeams = new List<NationalTeam>();
            foreach(Club c in _clubs)
            {
                NationalTeam nt = c as NationalTeam;
                if (nt != null)
                {
                    nationalsTeams.Add(nt);
                }
            }

            nationalsTeams.Sort(new NationsFifaRankingComparator());

            return nationalsTeams;
        }

        public void Resume()
        {
            int historyEntries = 0;
            foreach(Player player in Players)
            {
                historyEntries += player.history.Count;
            }
            Console.WriteLine(String.Format("[Players] {0} ({1} entrées historiques)", Players.Count, historyEntries));
            Console.WriteLine(String.Format("[Free Players] {0}", freePlayers.Count));
            Console.WriteLine(String.Format("[Clubs] {0}", Clubs.Count));
            Console.WriteLine(String.Format("[retired players] {0}", retiredPlayersCount));

        }

    }
}
