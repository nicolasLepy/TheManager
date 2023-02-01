using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TheManager.Comparators;
using TheManager.Tournaments;

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
            brutCommentary = brutCommentary.Replace(" JOUEUR ", " " + em.player.lastName + " ");
            return brutCommentary;
        }
    }
    

    [DataContract(IsReference =true)]
    [KnownType(typeof(Continent))]
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
        private Association _worldAssociation;
        [DataMember]
        private List<Language> _languages;
        [DataMember]
        private List<GenericCalendar> _genericCalendars;
        [DataMember]
        private List<Media> _medias;
        [DataMember]
        private List<MatchEventCommentary> _matchCommentaries;
        [DataMember]
        private List<Journalist> _freeJournalists;

        

        public List<Club> Clubs { get => _clubs; }

        public List<Tournament> Competitions
        {
            get
            {
                return _worldAssociation.AllTournaments();
            }
        }
        public List<Player> freePlayers { get => _freePlayers; }
        public List<Manager> freeManagers { get => _freeManagers; }
        public Association worldAssociation { get => _worldAssociation; }
        public List<Language> languages { get => _languages; }
        public List<Media> medias { get => _medias; }
        public List<MatchEventCommentary> matchCommentaries { get => _matchCommentaries; }
        public List<Journalist> freeJournalists { get => _freeJournalists; }

        public Kernel()
        {
            _clubs = new List<Club>();
            _freePlayers = new List<Player>();
            _continents = new List<Continent>();
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

        public Association String2Association(string name)
        {
            return _worldAssociation.String2Association(name);
        }

        public City String2City(string name)
        {
            City res = null;
            foreach(Continent c in _continents)
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

            foreach(Continent c in _continents)
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

            foreach (Continent c in _continents)
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

            foreach (Continent c in _continents)
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

        public void NationalTeamsCall()
        {
            foreach(Club c in _clubs)
            {
                NationalTeam sn = c as NationalTeam;
                if (sn != null)
                {
                    sn.CallInSelection(GetPlayersByCountry(sn.country));
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
            return _worldAssociation.String2Localisation(name);
        }

        public Association GetAssociation(int idAssociation)
        {
            return _worldAssociation.GetAssociation(idAssociation);
        }

        public ILocalisation LocalisationTournament(Tournament tournament)
        {
            ILocalisation res = _worldAssociation.GetAssociationOfTournament(tournament)?.localization;
            if(res == null)
            {
                foreach(Tournament t in _worldAssociation.AllTournaments())
                {
                    foreach (KeyValuePair<int, Tournament> tt in t.previousEditions)
                    {
                        if (tt.Value == tournament)
                        {
                            res = _worldAssociation.GetAssociationOfTournament(tt.Value).localization;
                        }
                    }
                }
            }
            return res;
        }


        public void AddFriendlyGame(Match m)
        {
            Tournament amc = String2Tournament("Matchs amicaux");
            amc.rounds[0].matches.Add(m);
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

        public List<Association> AllAssociations()
        {
            List<Association> res = new List<Association>() { _worldAssociation };
            _worldAssociation.AllAssociations();
            return res;
        }

        public Country City2Country(City city)
        {
            return _worldAssociation.City2Country(city);
        }

        /// <summary>
        /// Get list of associations of the nth level in the hierarchy.
        /// 1 : continents
        /// 2 : countries
        /// 3 : districts
        /// 4 : sub-districts
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<Association> GetAssociationsOfHierarchyLevel(int level)
        {
            List<Association> res = new List<Association>();
            res = worldAssociation.GetAssociationsOfHierarchyLevel(level);
            return res;
        }

    }
}
