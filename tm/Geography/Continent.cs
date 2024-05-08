using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using tm.Comparators;
using tm.Tournaments;

namespace tm
{

    [DataContract(IsReference = true)]
    public class InternationalDates : IEquatable<InternationalDates>
    {
        [DataMember]
        private GameDay _start;
        [DataMember]
        private GameDay _end;
        [DataMember]
        private Tournament _tournament;
        [DataMember]
        private bool _currentlyCalled;

        public GameDay start => _start;
        public GameDay end => _end;

        public bool currentlyCalled { get => _currentlyCalled; set => _currentlyCalled = value; }

        public Tournament tournament => _tournament;

        public bool IsValid()
        {
            return tournament == null || tournament.IsCurrentlyPlaying(); // ((tournament.currentRound > -1) && (tournament.currentRound < tournament.rounds.Count - 1));
        }

        public bool IsEquals(InternationalDates obj)
        {
            return start == obj.start && end == obj.end && _tournament == tournament;
        }

        public int StartYear(int currentWeekNumber)
        {
            bool startIsNextYear = start.WeekNumber < (currentWeekNumber);
            return Session.Instance.Game.date.Year + (startIsNextYear ? 1 : 0);
        }

        public int EndYear(int currentWeekNumber)
        {
            bool endIsNextYear = end.WeekNumber < (currentWeekNumber-2); //-1 because players are release 2 days after the _end week definition so probably the next week, to avoiding getting a day one year after
            return Session.Instance.Game.date.Year + (endIsNextYear? 1 : 0);
        }

        public bool Equals(InternationalDates other)
        {
            throw new NotImplementedException();
        }

        public InternationalDates(GameDay start, GameDay end, Tournament tournament, bool currentlyCalled)
        {
            _start = start;
            _end = end;
            _tournament = tournament;
            _currentlyCalled = currentlyCalled;
            if(tournament != null)
            {
                _start = tournament.rounds.First().programmation.gamesDays.First();
                _end = tournament.rounds.Last().programmation.gamesDays.Last();
            }
        }
    }

    [DataContract(IsReference = true)]
    public class Continent : IRecoverableTeams, ILocalisation
    {
        [DataMember]
        private List<Country> _countries;
        [DataMember]
        private List<Tournament> _tournaments;
        [DataMember]
        [Key]
        public int Id { get; set; }
        [DataMember]
        private string _name;
        [DataMember]
        private string _logo;
        [DataMember]
        private int _resetWeek;

        /**
         * Represent qualification in continental clubs competitions in function of the country place in the coefficient ranking
         */
        [DataMember]
        private List<Qualification> _continentalQualifications;
        [DataMember]
        private List<Country> _associationRanking;
        [DataMember]
        private List<List<Country>> _archivalAssociationRanking;
        [DataMember]
        private List<Continent> _continents;
        [DataMember]
        private List<InternationalDates> _internationalDates;

        /// <summary>
        /// As association ranking can be long to be computed (and change only at the end of the season), ranking is stored here to be reused without computing all ranking
        /// </summary>
        public List<Country> associationRanking
        {
            get
            {
                if(_associationRanking == null)
                {
                    UpdateStoredAssociationRanking();
                }
                return _associationRanking;
            }
        }

        public List<List<Country>> archivalAssociationRanking => _archivalAssociationRanking;

        public List<Country> countries => _countries;
        public List<Continent> continents => _continents;
        public List<Qualification> continentalQualifications => _continentalQualifications;
        public int resetWeek => _resetWeek;
        public List<InternationalDates> internationalDates => _internationalDates;
        public List<Tournament> Tournaments()
        {
            return _tournaments;
        }

        public string Name()
        {
            return _name;
        }

        public string Logo()
        {
            return _logo;
        }

        public Continent String2Continent(string name)
        {
            Continent res = null;
            if(name == Name())
            {
                res = this;
            }
            else
            {
                foreach(Continent c in continents)
                {
                    res = res == null ? c.String2Continent(name) : res;
                }
            }
            return res;
        }
        
        public List<Continent> GetAllContinents()
        {
            List<Continent> res = new List<Continent>();
            res.Add(this);
            foreach(Continent c in continents)
            {
                res.AddRange(c.GetAllContinents());
            }

            return res;
        }

        public List<Country> GetAllCountries()
        {
            List<Country> res = new List<Country>();
            res.AddRange(countries);
            foreach(Continent c in continents)
            {
                res.AddRange(c.GetAllCountries());
            }
            return res;
        }

        public List<Tournament> GetAllTournaments()
        {
            List<Tournament> res = new List<Tournament>();
            res.AddRange(Tournaments());
            foreach (Country p in countries)
            {
                res.AddRange(p.Tournaments());
            }
            foreach(Continent c in this.continents)
            {
                res.AddRange(c.GetAllTournaments());
            }
            return res;
        }

        public Continent()
        {
            _countries = new List<Country>();
            _tournaments = new List<Tournament>();
            _continentalQualifications = new List<Qualification>();
            _associationRanking = new List<Country>();
            _archivalAssociationRanking = new List<List<Country>>();
            _continents = new List<Continent>();
            _internationalDates = new List<InternationalDates>();
        }

        public Continent(int id, string name, string logo, int resetWeek)
        {
            Id = id;
            _name = name;
            _logo = logo;
            _countries = new List<Country>();
            _tournaments = new List<Tournament>();
            _continentalQualifications = new List<Qualification>();
            _associationRanking = new List<Country>();
            _archivalAssociationRanking = new List<List<Country>>();
            _continents = new List<Continent>();
            _resetWeek = resetWeek;
            _internationalDates = new List<InternationalDates>();
        }

        /// <summary>
        /// The parameter "onlyFirstTeams" is ignored because national teams are always first teams
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="methode"></param>
        /// <param name="onlyFirstTeams"></param>
        /// <returns></returns>
        public List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams, AdministrativeDivision associationFilter)
        {
            List<NationalTeam> nationalsTeams = new List<NationalTeam>();
            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                NationalTeam sn = c as NationalTeam;
                if(sn != null)
                {
                    if (_countries.Contains(sn.country))
                    {
                        nationalsTeams.Add(sn);
                    }
                }
            }
            List<Club> res = new List<Club>();
            if (method == RecuperationMethod.Best)
            {
                nationalsTeams.Sort(new NationsFifaRankingComparator());
            }
            else if (method == RecuperationMethod.Worst)
            {
                nationalsTeams.Sort(new NationsFifaRankingComparator(true));
            }
            else if (method == RecuperationMethod.Randomly)
            {
                nationalsTeams = Utils.ShuffleList<NationalTeam>(nationalsTeams);
            }

            for (int i = 0; i < number; i++)
            {
                res.Add(nationalsTeams[i]);
            }
            return res;
        }

        public int CountWithoutReserves()
        {
            return _countries.Count;
        }

        public int ContinentalTournamentsCount
        {
            get
            {
                int res = 0;
                foreach(Tournament t in Tournaments())
                {
                    //TODO: Move friendly tournaments in a "World" category
                    if(t.periodicity == 1 && t.name != Utils.friendlyTournamentName)
                    {
                        res++;
                    }
                }
                return res;
            }
        }

        public List<Tournament> GetContinentalClubTournaments()
        {
            List<Tournament> res = new List<Tournament>();
            int i = 1;
            Tournament t = GetContinentalClubTournament(i);
            while(t != null)
            {
                res.Add(t);
                t = GetContinentalClubTournament(++i);
            }
            return res;
        }


        public Tournament GetContinentalClubTournament(int level)
        {
            Tournament res = null;
            foreach (Tournament t in Tournaments())
            {
                if(t.periodicity == 1 && t.level == level && t.name != Utils.friendlyTournamentName)
                {
                    res = t;
                }
            }
            return res;
        }

        public void UpdateStoredAssociationRanking()
        {
            Console.WriteLine("[UpdateStoredAssociationRanking] " + Name());
            if(_associationRanking != null && _associationRanking.Count > 0)
            {
                _archivalAssociationRanking.Add(new List<Country>(_associationRanking));
            }
            _associationRanking = new List<Country>();
            foreach (Country c in _countries)
            {
                if (c.Tournaments().Count > 0)
                {
                    _associationRanking.Add(c);
                }
            }

            _associationRanking.Sort(new CountryComparator(CountryAttribute.CONTINENTAL_COEFFICIENT));

        }

        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Country c, int year)
        {
            // Special case when getting clubs of the finished edition (Ireland 2022 for example) for an international tournament not started yet (CL 2023-2024 for example)
            if(year == Session.Instance.Game.CurrentSeason + 1)
            {
                return GetClubsQualifiedForInternationalCompetitions(c, false);
            }
            Dictionary<Club, Qualification> res = new Dictionary<Club, Qualification>();

            int level = 1;
            Tournament tournament = GetContinentalClubTournament(level);
            while(tournament != null)
            {
                Tournament archive = year == Session.Instance.Game.CurrentSeason ? tournament : tournament.previousEditions[year];
                foreach(Round r in archive.rounds)
                {
                    foreach(Club club in r.clubs)
                    {
                        if(club.Country() == c && (!res.ContainsKey(club) || Utils.IsBefore(r.DateInitialisationRound(), res[club].tournament.rounds[res[club].roundId].DateInitialisationRound())))
                        {
                            res.Add(club, new Qualification(1, archive.rounds.IndexOf(r), tournament, true, 0));
                        }
                    }
                }
                level++;
                tournament = GetContinentalClubTournament(level);
            }

            return res;
        }


        /// <summary>
        /// Get at this date the qualified clubs for a country for international tournaments
        /// </summary>
        /// <param name="c">Country</param>
        /// <param name="onlyCurrentLeagueEdition">
        /// Change only behavior for country that use a different calendar of continental association (Ireland for exemple).
        /// Qualified clubs of these country are taken from an older league edition and not from the current running division. A new edition is started at this time.
        /// When this argument is true, show teams that will be qualified for the CL edition when this edition will be finished.
        /// This argument is ignored when league and continent use the same calendar because the current running league is always the league where teams will be taken by continental association for the next CL edition.
        /// [TODO] Possible refactor to avoid this parameter     [(round = (last_round).finished ? last_round : previous_edition.rounds.last) could avoid date comparaison but not this parameter]
        /// </param>
        /// <returns></returns>
        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Country c, bool onlyCurrentLeagueEdition)
        {
            Dictionary<Club, Qualification> res = new Dictionary<Club, Qualification>();

            List<Country> countriesRanking = new List<Country>(associationRanking);
            int index = countriesRanking.IndexOf(c);
            int rank = index + 1;
            List<Qualification> associationQualifications = (from q in _continentalQualifications where q.ranking == rank select q).ToList();

            List<Club> registeredClubs = new List<Club>();
            List<Club> leagueClubs = new List<Club>();

            int leagueLevel = 1;
            Tournament leagueDivisionChampionship = c.League(leagueLevel);
            while(leagueDivisionChampionship != null)
            {
                //Manage association where calendar is not the same as the continent calendar
                //Eg. August-May calendar for Europe and Febuary-November calendar for Ireland
                //If the current first division championship of a country is not finished the day of the continental reset, we take teams from the previous league edition
                //For the CL 2022-2023, teams are picked from the 2021 Airtriciy League.
                //TODO: Quid of league finishing in Febuary (if any)
                if (!onlyCurrentLeagueEdition && !Utils.IsBefore(leagueDivisionChampionship.rounds.Last().DateEndRound(), new GameDay(resetWeek, false, 0, 0).ConvertToDateTime()) && leagueDivisionChampionship.previousEditions.Count > 0)
                {
                    var maxValueKey = leagueDivisionChampionship.previousEditions.Aggregate((x, y) => x.Key > y.Key ? x : y).Key;
                    leagueDivisionChampionship = leagueDivisionChampionship.previousEditions[maxValueKey];
                }

                Round championshipRound = leagueDivisionChampionship.GetLastChampionshipRound();
                if (championshipRound as ChampionshipRound != null)
                {
                    leagueClubs.AddRange((championshipRound as ChampionshipRound).Ranking());
                }
                if (championshipRound as GroupInactiveRound != null)
                {
                    leagueClubs.AddRange((championshipRound as GroupInactiveRound).FullRanking());
                }
                if (championshipRound as GroupActiveRound != null) //Only to store clubs from lower division (uncommon to have a league playing as groups without final phase)
                {
                    List<Club> roundClubs = new List<Club>(championshipRound.clubs);
                    if (leagueLevel == 1 || roundClubs.Count < 60)
                    {
                        roundClubs.Sort(new ClubRankingComparator(championshipRound.matches, championshipRound.tiebreakers, championshipRound.pointsDeduction));
                    }
                    leagueClubs.AddRange(roundClubs);
                }

                //Get final phase clubs tree from first league in case of
                if(leagueLevel == 1)
                {
                    List<Club> finalPhasesClubs = leagueDivisionChampionship.GetFinalPhasesClubs();
                    if (finalPhasesClubs.Count > 0)
                    {
                        for (int j = finalPhasesClubs.Count - 1; j >= 0; j--)
                        {
                            leagueClubs.Remove(finalPhasesClubs[j]);
                            leagueClubs.Insert(0, finalPhasesClubs[j]);
                        }
                    }
                }

                leagueDivisionChampionship = c.League(++leagueLevel);
            }

            List<Tournament> cups = c.Cups();
            List<Club> cupWinners = new List<Club>();
            for(int i = 0; i < cups.Count; i++)
            {
                Tournament cup = cups[i];
                //Same way to manage association where calendar is not the same as the continent calendar
                if(!onlyCurrentLeagueEdition && !Utils.IsBefore(cup.rounds.Last().DateEndRound(), new GameDay(resetWeek, false, 0, 0).ConvertToDateTime()) && cup.previousEditions.Count > 0)
                {
                    var maxValueKey = cup.previousEditions.Aggregate((x, y) => x.Key > y.Key ? x : y).Key;
                    cup = cup.previousEditions[maxValueKey];
                }
                //If the cup is finished
                //cup.PrintCupResume();
                if (cup.rounds.Last().matches.Count == 1 && cup.rounds.Last().matches[0].Played)
                {
                    cupWinners.Add(cup.Winner());
                }
                else if(cup.parent.Association == null) //This cup is not the regional path of a bigger cup
                {
                    cupWinners.Add(null); //Placeholder to tell this cup expect a winner but is not finished
                }
            }

            //Rule R1 : The association of the winner of a continental tournament get one additionnal place because the winner is automatically qualified
            bool ruleR1 = GetContinentalClubTournament(1) != null ? GetContinentalClubTournament(1).rules.Contains(TournamentRule.OnWinnerQualifiedAdaptClubsQualifications) : false;
            //On rajoute le clubs aux vainqueurs de coupe, on rajoute la qualification au bon endroit. Si le club est déjà enregistré et à une meilleure position
            if (ruleR1)
            {
                List<Tournament> continentalTournaments = GetContinentalClubTournaments();
                for(int i = leagueClubs.Count-1; i >=0; i--)
                {
                    Club club = leagueClubs[i];
                    KeyValuePair<Tournament, int> cdq = new KeyValuePair<Tournament, int>(null, 0);
                    foreach (Tournament t in continentalTournaments)
                    {
                        Club tWinner = t.rounds.Last().Winner();
                        List<Qualification> tQualifications = t.rounds.Last().qualifications;
                        if (tWinner == club && tQualifications.Count > 0 && tQualifications[0].isNextYear && tQualifications[0].ranking == 1)
                        {
                            cdq = new KeyValuePair<Tournament, int>(tQualifications[0].tournament, tQualifications[0].roundId);
                        }
                    }

                    bool clubQualifiedAsWinner = cdq.Key != null;
                    if (clubQualifiedAsWinner)
                    {
                        //Add a new qualification corresponding to the place reserved to the international cup winner
                        Qualification qualificationCupWinner = new Qualification(rank, cdq.Value, cdq.Key, true, 1);
                        associationQualifications.Add(qualificationCupWinner);
                        //Sort to put the new qualification at the right place
                        associationQualifications.Sort((x, y) => x.tournament.level != y.tournament.level ? x.tournament.level - y.tournament.level : y.roundId - x.roundId);
                        int indexQ = -1;
                        List<Qualification> cupQualifications = (from a in associationQualifications where a.isNextYear select a).ToList();
                        for (int q = 0; q < cupQualifications.Count; q++)
                        {
                            //Reminder isNextYear is used for isCupWinner
                            indexQ = (indexQ == -1 && cupQualifications[q].roundId == cdq.Value && cupQualifications[q].tournament == cdq.Key) ? q : indexQ;
                        }
                        //Resort cup winners to match added qualification
                        if (cupWinners.IndexOf(club) > -1 && cupWinners.IndexOf(club) < indexQ)
                        {
                            indexQ--;
                        }
                        
                        cupWinners.Remove(club);
                        cupWinners.Insert(indexQ, club);
                    }

                }
            }
            int currentLevel = 0;
            int cupRank = 0;
            foreach(Qualification q in associationQualifications)
            {
                if (q.ranking == rank)
                {
                    Club currentCupWinner = cupRank < cupWinners.Count ? cupWinners[cupRank] : null;
                    bool cupWinnerNotDefinedYet = cupRank < cupWinners.Count && cupWinners[cupRank] == null;
                    //isNextYear is used as "cup winner" here instead of league qualification
                    if ((!q.isNextYear || registeredClubs.Contains(currentCupWinner)) || (currentCupWinner == null && !cupWinnerNotDefinedYet))
                    {
                        for (int j = 0; j < q.qualifies; j++)
                        {
                            Club qualifiedClub = leagueClubs[currentLevel];
                            //If we get the cup winner and is already qualified, then we move to the next candidate team to avoid the cup winner entering two times in continental tournament
                            while (registeredClubs.Contains(qualifiedClub))
                            {
                                currentLevel++;
                                qualifiedClub = leagueClubs[currentLevel];
                            }
                            currentLevel++;
                            res.Add(qualifiedClub, q);
                            registeredClubs.Add(qualifiedClub);
                        }
                    }
                    else if(!cupWinnerNotDefinedYet)
                    {
                        res.Add(currentCupWinner, q);
                        registeredClubs.Add(currentCupWinner);
                    }
                    if (q.isNextYear)
                    {
                        cupRank++;
                    }
                }
            }
            return res;
        }

        public void QualifiesClubForContinentalCompetitionNextYear()
        {
            List<Country> countriesRanking = new List<Country>(associationRanking);
            for(int i = 0; i< countriesRanking.Count; i++)
            {
                Dictionary<Club, Qualification> qualifiedClubs = GetClubsQualifiedForInternationalCompetitions(countriesRanking[i], false);
                bool ruleR1 = GetContinentalClubTournament(1) != null ? GetContinentalClubTournament(1).rules.Contains(TournamentRule.OnWinnerQualifiedAdaptClubsQualifications) : false;
                if(ruleR1)
                {
                    foreach (Tournament t in GetContinentalClubTournaments())
                    {
                        for (int j = 0; j < t.nextYearQualified.Length; j++)
                        {
                            for (int k = t.nextYearQualified[j].Count - 1; k >= 0; k--)
                            {
                                if (t.nextYearQualified[j][k].Country() == countriesRanking[i])
                                {
                                    t.nextYearQualified[j].Remove(t.nextYearQualified[j][k]);
                                }
                            }
                        }
                    }
                }
                foreach (KeyValuePair<Club, Qualification> kvp in qualifiedClubs)
                {
                    Utils.Debug("Qualifie " + kvp.Key.name + " pour la " + kvp.Value.tournament.name + " (tour " + kvp.Value.roundId + ")");
                    kvp.Value.tournament.AddClubForNextYear(kvp.Key, kvp.Value.roundId);
                }
            }
        }


        public List<Club> GetContinentalClubs(List<Club> clubs)
        {
            List<Club> internationalClubs = new List<Club>();
            int level = 1;
            Tournament internationalTournament = GetContinentalClubTournament(level);
            while (internationalTournament != null)
            {
                foreach (Round r in internationalTournament.rounds)
                {
                    foreach (Club club in clubs)
                    {
                        if (r.clubs.Contains(club) && !internationalClubs.Contains(club))
                        {
                            internationalClubs.Add(club);
                        }
                    }
                }
                internationalTournament = GetContinentalClubTournament(++level);
            }
            return internationalClubs;
        }

        public override string ToString()
        {
            return _name;
        }

        public Continent GetContinent()
        {
            return this;
        }
    }
}