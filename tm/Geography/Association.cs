using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Runtime.Serialization;
using tm.Comparators;
using tm.Tournaments;
using AssociationAttribute = tm.Comparators.AssociationAttribute;

namespace tm
{

    [DataContract(IsReference =true)]
    public class Association : ILocalisation, IRecoverableTeams
    {
        [DataMember]
        [Key]
        public int Id { get; set; }
        [DataMember]
        private List<Association> _associations;
        [DataMember]
        private string _name;
        [DataMember]
        private string _logo;
        [DataMember]
        private ILocalisation _localisation;
        [DataMember]
        private Association _parent;

        [DataMember]
        private List<Tournament> _tournaments;
        [DataMember]
        private List<NationalTeam> _nationalTeams;

        [DataMember]
        private int _resetWeek;
        [DataMember]
        private bool _enableInternationalClubsCompetitions;


        [DataMember]
        private Dictionary<Club, Tournament> _administrativeRetrogradations;
        [DataMember]
        private List<float[]> _gamesTimesWeekend;
        [DataMember]
        private List<float[]> _gamesTimesWeekdays;
        [DataMember]
        private List<AdministrativeSanction> _administrativeSanctionsDefinitions;

        private List<Club>[] _cacheAdministrativeRetrogradationsChanges;



        public List<Tournament> tournaments => _tournaments;


        public List<Association> associations => _associations;
        public string name => _name;
        public string logo => _logo;
        public ILocalisation localisation => _localisation;
        public Association parent { get => _parent; set => _parent = value; }
        public int resetWeek => _resetWeek;

        public List<float[]> gamesTimesWeekend => _gamesTimesWeekend;
        public List<float[]> gamesTimesWeekdays => _gamesTimesWeekdays;

        public AdministrativeSanction GetSanction(SanctionType sanctionType)
        {
            AdministrativeSanction res = default;
            bool found = false;
            foreach (AdministrativeSanction admS in _administrativeSanctionsDefinitions)
            {
                if (admS.type == sanctionType)
                {
                    res = admS;
                    found = true;
                }
            }
            if(!found && parent != null)
            {
                res = parent.GetSanction(sanctionType);
            }
            return res;
        }



        public bool enabledInternationalClubsCompetitions => _enableInternationalClubsCompetitions;

        public List<NationalTeam> nationalTeams => new List<NationalTeam>(_nationalTeams);

        /**
         * Represent qualification in continental clubs competitions in function of the country place in the coefficient ranking
         */
        [DataMember]
        private List<Qualification> _continentalQualifications;
        [DataMember]
        private List<Association> _associationRanking;
        [DataMember]
        private List<List<Association>> _archivalAssociationRanking;
        [DataMember]
        private List<InternationalDates> _internationalDates;
        [DataMember]
        private bool _stateAssociation;

        public bool isStateAssociation => _stateAssociation;

        /// <summary>
        /// As association ranking can be long to be computed (and change only at the end of the season), ranking is stored here to be reused without computing all ranking
        /// </summary>
        public List<Association> associationRanking
        {
            get
            {
                if (_associationRanking == null)
                {
                    UpdateStoredAssociationRanking();
                }
                return _associationRanking;
            }
        }
        public List<List<Association>> archivalAssociationRanking => _archivalAssociationRanking;
        public List<Qualification> continentalQualifications => _continentalQualifications;
        public List<InternationalDates> internationalDates => _internationalDates;

        /// <summary>
        /// The country associated to this association
        /// TODO: Change country to a generic geographic unit
        /// FIXME: Seems not working currently
        /// </summary>
        /*public Country country
        {
            get
            {
                Country r = null;
                foreach(Country c in Session.Instance.Game.kernel.world.GetAllCountries())
                {
                    if(c.GetCountryAssociation() == this)
                    {
                        r = c;
                    }
                }
                return r;
            }
        }*/

        public List<Stadium> stadiums
        {
            get
            {
                return (localisation as Country).stadiums;
            }
        }

        public List<float[]> GamesTimesWeekend()
        {
            List<float[]> res = gamesTimesWeekend;
            if (res.Count == 0 && parent != null)
            {
                res = parent.GamesTimesWeekend();
            }
            return res;
        }

        public List<float[]> GamesTimesWeekdays()
        {
            List<float[]> res = gamesTimesWeekdays;
            if (res.Count == 0 && parent != null)
            {
                res = parent.GamesTimesWeekdays();
            }
            return res;
        }

        /// <summary>
        /// Get the tournament definition just below. Can represent a tournament if the tournament below in managed by the same association or an exclusion (relegation to regional league)
        /// </summary>
        /// <returns>A QualificationTarget object</returns>
        public QualificationTarget LeagueBelow(Tournament tournament)
        {
            QualificationTarget below = null;
            if(!_tournaments.Contains(tournament))
            {
                throw new Exception(String.Format("The tournament {0} is not holded by the association {1}", tournament.name, name));
            }
            Tournament belowInAssociation = this.League(tournament.level + 1);
            if(belowInAssociation != null)
            {
                below = new QualificationTournament(belowInAssociation);
            }
            else
            {
                //If a child association define a league system, so we define a "ExcludeFromLeagueSystem" qualification
                foreach(Association a in _associations)
                {
                    if(a.League(1) != null)
                    {
                        below = new QualificationExcludeLeagueSystem(this);
                    }
                }
            }
            return below;
        }

        /// <summary>
        /// Get the tournament definition just above. Can represent a tournament if the tournament below in managed by the same association or an exclusion (relegation to regional league)
        /// </summary>
        /// <returns>A QualificationTarget object</returns>
        public QualificationTarget LeagueAbove(Tournament tournament)
        {
            QualificationTarget above = null;
            if (!_tournaments.Contains(tournament))
            {
                throw new Exception(String.Format("The tournament {0} is not holded by the association {1}", tournament.name, name));
            }
            Tournament aboveInAssociation = this.League(tournament.level - 1);
            if (aboveInAssociation != null)
            {
                above = new QualificationTournament(aboveInAssociation);
            }
            else
            {
                if(parent != null)
                {
                    Tournament t = parent.Leagues().LastOrDefault();
                    if(t != null)
                    {
                        above = new QualificationTournament(t);
                    }
                }
            }
            return above;
        }

        public Association ClosestStateAssociation()
        {
            Association res = null;
            if (isStateAssociation)
            {
                res = this;
            }
            else if(parent != null)
            {
                res = parent.ClosestStateAssociation();
            }
            return res;
        }

        /// <summary>
        /// Get the association who is the direct children of an association
        /// For exemple : When association=Europe, District of Côte d'Or will return France
        /// For exemple : When association=World, Spain will return Europe
        /// </summary>
        /// <param name="association"></param>
        /// <returns></returns>
        public Association GetRepresentingAssociation(Association association)
        {
            Association res = null;
            if(association.associations.Contains(this))
            {
                res = this;
            }
            else if(parent != null)
            {
                res = parent.GetRepresentingAssociation(association);
            }
            return res;
        }

        public Association()
        {
            _associations = new List<Association>();
            _tournaments = new List<Tournament>();
            _continentalQualifications = new List<Qualification>();
            _associationRanking = new List<Association>();
            _archivalAssociationRanking = new List<List<Association>>();
            _internationalDates = new List<InternationalDates>();
            _nationalTeams = new List<NationalTeam>();
            _cacheAdministrativeRetrogradationsChanges = null;
            _gamesTimesWeekend = new List<float[]>();
            _gamesTimesWeekdays = new List<float[]>();
            _administrativeSanctionsDefinitions = new List<AdministrativeSanction>();
            _stateAssociation = false;
            _administrativeRetrogradations = new Dictionary<Club, Tournament>();

        }

        public Association(int id, string name, string logo, ILocalisation localisation, Association parent, int resetWeek, bool enableInternationalClubsCompetitions, List<AdministrativeSanction> sanctionsDefinitions, bool isStateAssociation)
        {
            Id = id;
            _name = name;
            _logo = logo;
            _associations = new List<Association>();
            _tournaments = new List<Tournament>();
            _continentalQualifications = new List<Qualification>();
            _associationRanking = new List<Association>();
            _archivalAssociationRanking = new List<List<Association>>();
            _internationalDates = new List<InternationalDates>();
            _nationalTeams = new List<NationalTeam>();
            _localisation = localisation;
            _parent = parent;
            _resetWeek = resetWeek;
            _enableInternationalClubsCompetitions = enableInternationalClubsCompetitions;
            _cacheAdministrativeRetrogradationsChanges = null;
            _gamesTimesWeekend = new List<float[]>();
            _gamesTimesWeekdays = new List<float[]>();
            _administrativeSanctionsDefinitions = sanctionsDefinitions;
            _stateAssociation = isStateAssociation;
            _administrativeRetrogradations = new Dictionary<Club, Tournament>();

        }

        public void RegisterNationalTeam(NationalTeam nt)
        {
            this._nationalTeams.Add(nt);
        }

        public Association String2Association(string name)
        {
            Association res = null;
            if (name == Name())
            {
                res = this;
            }
            else
            {
                foreach (Association a in associations)
                {
                    res = res == null ? a.String2Association(name) : res;
                }
            }
            return res;
        }

        ///
        /// Hierarchical association querying
        ///

        
        /// <summary>
        /// Return True if this association is a child or the same as the association passed as an argument
        /// Warning: returns False if 'association' is a child of self
        /// </summary>
        public bool IsDirectConnected(Association association)
        {
            return association == this || (parent != null && parent.IsDirectConnected(association));
        }

        /// <summary>
        /// Get the hierachical level of the association
        /// </summary>
        /// <param name="association">Association</param>
        /// <param name="currentLevel"></param>
        /// <returns></returns>
        public int GetLevelOfAssociation(Association association, int currentLevel)
        {
            if(association == this)
            {
                return currentLevel;
            }
            else
            {
                int newLevel = -1;
                foreach(Association ad in _associations)
                {
                    int adLevel = ad.GetLevelOfAssociation(association, currentLevel + 1);
                    if(adLevel != -1)
                    {
                        newLevel = adLevel;
                    }
                }
                return newLevel;
            }
        }

        /// <summary>
        /// Get child association of the specified level, relative to the current association
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<Association> GetAssociationsLevel(int level)
        {
            List<Association> res = new List<Association>();
            if (level == 1)
            {
                res = _associations;
            }
            else
            {
                foreach (Association ad in _associations)
                {
                    res.AddRange(ad.GetAssociationsLevel(level - 1));
                }
            }
            return res;
        }

        public Association GetAssociationLevel(Association association, int level)
        {
            Association res = null;
            List<Association> levelAdm = GetAssociationsLevel(level);

            foreach (Association adm in levelAdm)
            {
                if (adm == association || adm.ContainsAssociation(association))
                {
                    res = adm;
                }
            }
            return res;
        }

        /// <summary>
        /// Return True if the association is contained in this association, or if the association and this association are the same.
        /// </summary>
        /// <param name="association"></param>
        /// <returns></returns>
        public bool ContainsAssociation(Association association)
        {
            bool res = this == association;

            if(!res)
            {
                foreach (Association adm in _associations)
                {
                    if (adm.ContainsAssociation(association))
                    {
                        res = true;
                    }
                }
            }
            
            return res;
        }
        
        /// <summary>
        /// Get child association by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Association GetAssociation(int id)
        {
            Association res = null;

            foreach (Association ad in associations)
            {
                if (ad.Id == id)
                {
                    res = ad;
                }

                Association resChild = ad.GetAssociation(id);
                if (resChild != null)
                {
                    res = resChild;
                }
            }
            
            return res;
        }

        /// <summary>
        /// Return all childs
        /// </summary>
        /// <returns></returns>
        public List<Association> GetAllChilds()
        {
            List<Association> res = new List<Association>();
            res.AddRange(associations);
            foreach(Association a in associations)
            {
                res.AddRange(a.GetAllChilds());
            }
            return res;
        }

        /// <summary>
        /// Return all childs at a specific level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<Association> GetAllChilds(int level)
        {
            List<Association> childs = new List<Association>();
            if(level == 1)
            {
                childs = new List<Association>(associations);
            }
            if(level > 1)
            {
                foreach(Association ad in associations)
                {
                    childs.AddRange(ad.GetAllChilds(level - 1));
                }
            }
            return childs;
        }

        ///
        /// International tournaments related methods
        ///

        public float YearAssociationCoefficient(int nSeason)
        {
            List<Club> clubs = new List<Club>();
            float total = 0;
            for (int i = 1; i < 4; i++)
            {
                Tournament continentalTournament = parent.GetContinentalClubTournament(i);
                if (continentalTournament != null)
                {
                    int j = continentalTournament.previousEditions.Count - (-nSeason);

                    if (j >= 0)
                    {
                        Tournament yearContinentalTournament = continentalTournament.previousEditions.ToList()[j].Value;

                        List<Tournament> tournamentsParse = new List<Tournament>(_tournaments);
                        if(localisation as Country != null)
                        {
                            tournamentsParse.AddRange((localisation as Country).Tournaments());
                        }
                        foreach (Tournament championship in tournamentsParse)
                        {
                            if (championship.isChampionship)
                            {
                                foreach (Club c in championship.rounds[0].clubs)
                                {
                                    if (yearContinentalTournament.IsInvolved(c))
                                    {
                                        clubs.Add(c);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (Club c in clubs)
            {
                float clubCoefficient = c.ClubYearCoefficient(nSeason, true);
                total += clubCoefficient;
            }

            return clubs.Count > 0 ? total / clubs.Count : 0;
        }

        public float AssociationCoefficient
        {
            get
            {
                float res = 0;
                for (int i = -5; i < 0; i++)
                {
                    res += YearAssociationCoefficient(i);
                }
                return res;
            }
        }

        public override string ToString()
        {
            return String.Format("tm.Association {0}", _name);
        }

        /// <summary>
        /// Returns the number of teams of an association who will be relegated
        /// </summary>
        /// <param name="association"></param>
        /// <returns></returns>
        public int GetExcludedTeamsFromLeagueSystem(Association association)
        {
            int res = 0;
            if(parent != null)
            {
                res += parent.GetExcludedTeamsFromLeagueSystem(this);
            }
            Tournament lastLevel = Leagues().LastOrDefault();
            List<Club> clubs = new List<Club>();
            if (lastLevel != null)
            {
                foreach (Round r in lastLevel.rounds)
                {
                    GroupsRound gr = r as GroupsRound;
                    if (gr != null)
                    {
                        List<Club> relegablesCandidates = gr.GetAssociationRelegables(association);
                        clubs.AddRange(relegablesCandidates);
                    }
                }
            }
            res += clubs.Count;
            return res;
        }

        public List<Tournament> GetAllTournaments()
        {
            List<Tournament> res = new List<Tournament>();
            res.AddRange(tournaments);
            foreach(Association a in associations)
            {
                res.AddRange(a.GetAllTournaments());
            }
            return res;
        }


        public void UpdateStoredAssociationRanking()
        {
            Console.WriteLine("[UpdateStoredAssociationRanking] " + name);
            if (_associationRanking != null && _associationRanking.Count > 0)
            {
                _archivalAssociationRanking.Add(new List<Association>(_associationRanking));
            }
            _associationRanking = new List<Association>();
            foreach (Association a in associations)
            {
                //a.localisation.Tournaments().Count > 0 : Because countries still hold domestics tournaments
                if (a.tournaments.Count > 0 || a.localisation.Tournaments().Count > 0)
                {
                    _associationRanking.Add(a);
                }
            }

            _associationRanking.Sort(new AssociationComparator(AssociationAttribute.CONTINENTAL_COEFFICIENT));

        }



        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Association a, int year)
        {
            // Special case when getting clubs of the finished edition (Ireland 2022 for example) for an international tournament not started yet (CL 2023-2024 for example)
            if (year == Session.Instance.Game.CurrentSeason + 1)
            {
                return GetClubsQualifiedForInternationalCompetitions(a, false);
            }
            Dictionary<Club, Qualification> res = new Dictionary<Club, Qualification>();

            int level = 1;
            Tournament tournament = GetContinentalClubTournament(level);
            while (tournament != null)
            {
                Tournament archive = year == Session.Instance.Game.CurrentSeason ? tournament : tournament.previousEditions[year];
                foreach (Round r in archive.rounds)
                {
                    foreach (Club club in r.clubs)
                    {
                        if (club.Association().IsDirectConnected(a) && (!res.ContainsKey(club) || Utils.IsBefore(r.DateInitialisationRound(), res[club].target.Tournament(club).rounds[res[club].roundId].DateInitialisationRound())))
                        {
                            res.Add(club, new Qualification(1, archive.rounds.IndexOf(r), new QualificationTournament(tournament), true, 0));
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
        /// <param name="a">Association</param>
        /// <param name="onlyCurrentLeagueEdition">
        /// Change only behavior for country that use a different calendar of continental association (Ireland for exemple).
        /// Qualified clubs of these country are taken from an older league edition and not from the current running division. A new edition is started at this time.
        /// When this argument is true, show teams that will be qualified for the CL edition when this edition will be finished.
        /// This argument is ignored when league and continent use the same calendar because the current running league is always the league where teams will be taken by continental association for the next CL edition.
        /// [TODO] Possible refactor to avoid this parameter     [(round = (last_round).finished ? last_round : previous_edition.rounds.last) could avoid date comparaison but not this parameter]
        /// </param>
        /// <returns></returns>
        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Association a, bool onlyCurrentLeagueEdition)
        {
            Dictionary<Club, Qualification> res = new Dictionary<Club, Qualification>();

            List<Association> countriesRanking = new List<Association>(associationRanking);
            int index = countriesRanking.IndexOf(a);
            int rank = index + 1;
            List<Qualification> associationQualifications = (from q in _continentalQualifications where q.ranking == rank select q).ToList();

            List<Club> registeredClubs = new List<Club>();
            List<Club> leagueClubs = new List<Club>();

            int leagueLevel = 1;
            Tournament leagueDivisionChampionship = a.League(leagueLevel);
            while (leagueDivisionChampionship != null)
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

                Round championshipRound = leagueDivisionChampionship.GetLastChampionshipRound(); //TODO: Bottom league is not registered here (czech)

                if (championshipRound as GroupInactiveRound != null)
                {
                    leagueClubs.AddRange((championshipRound as GroupInactiveRound).FullRanking());
                }
                if (championshipRound as GroupActiveRound != null)
                {
                    List<Club> roundClubs = new List<Club>(championshipRound.clubs);
                    if (leagueLevel == 1 || roundClubs.Count < 60) //TODO: Other way to check if it's a very lower league. //TODO: USE_RANKING_METHOD
                    {
                        roundClubs.Sort(new ClubRankingComparator(championshipRound.matches, championshipRound.tiebreakers, championshipRound.pointsDeduction));
                    }
                    leagueClubs.AddRange(roundClubs);
                }

                //Get final phase clubs tree from first league in case of
                if (leagueLevel == 1)
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

                //Cas spécial championnat coupé en deux : les équipes qui sont envoyée dans le championnat du bas de tableau sont oubliées.
                //TODO: On perds le classement des équipes du bas de tableau ici (pas grave sauf exception)
                if (championshipRound != leagueDivisionChampionship.rounds[0])
                {
                    foreach (Club club in leagueDivisionChampionship.rounds[0].clubs)
                    {
                        if (!championshipRound.clubs.Contains(club))
                        {
                            leagueClubs.Add(club);
                        }
                    }
                }

                leagueDivisionChampionship = a.League(++leagueLevel);
            }

            List<Tournament> cups = a.Cups();
            List<Club> cupWinners = new List<Club>();
            for (int i = 0; i < cups.Count; i++)
            {
                Tournament cup = cups[i];
                //Same way to manage association where calendar is not the same as the continent calendar
                if (!onlyCurrentLeagueEdition && !Utils.IsBefore(cup.rounds.Last().DateEndRound(), new GameDay(resetWeek, false, 0, 0).ConvertToDateTime()) && cup.previousEditions.Count > 0)
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
                else if (cup.parent.Association == null) //This cup is not the regional path of a bigger cup
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
                for (int i = leagueClubs.Count - 1; i >= 0; i--)
                {
                    Club club = leagueClubs[i];
                    KeyValuePair<Tournament, int> cdq = new KeyValuePair<Tournament, int>(null, 0);
                    foreach (Tournament t in continentalTournaments)
                    {
                        Club tWinner = t.rounds.Last().Winner();
                        List<Qualification> tQualifications = t.rounds.Last().qualifications;
                        if (tWinner == club && tQualifications.Count > 0 && tQualifications[0].isNextYear && tQualifications[0].ranking == 1)
                        {
                            cdq = new KeyValuePair<Tournament, int>(tQualifications[0].target.Tournament(), tQualifications[0].roundId);
                        }
                    }

                    bool clubQualifiedAsWinner = cdq.Key != null;
                    if (clubQualifiedAsWinner)
                    {
                        //Add a new qualification corresponding to the place reserved to the international cup winner
                        Qualification qualificationCupWinner = new Qualification(rank, cdq.Value, new QualificationTournament(cdq.Key), true, 1);
                        associationQualifications.Add(qualificationCupWinner);
                        //Sort to put the new qualification at the right place


                        associationQualifications.Sort(new QualificationTournamentComparator());

                        int indexQ = -1;
                        List<Qualification> cupQualifications = (from aq in associationQualifications where aq.isNextYear select aq).ToList();
                        for (int q = 0; q < cupQualifications.Count; q++)
                        {
                            //Reminder isNextYear is used for isCupWinner
                            indexQ = (indexQ == -1 && cupQualifications[q].roundId == cdq.Value && cupQualifications[q].target.Tournament() == cdq.Key) ? q : indexQ;
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
            foreach (Qualification q in associationQualifications)
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
                    else if (!cupWinnerNotDefinedYet)
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
            List<Association> countriesRanking = new List<Association>(associationRanking);
            for (int i = 0; i < countriesRanking.Count; i++)
            {
                Dictionary<Club, Qualification> qualifiedClubs = GetClubsQualifiedForInternationalCompetitions(countriesRanking[i], false);
                foreach (KeyValuePair<Club, Qualification> kvp in qualifiedClubs)
                {
                    Utils.Debug("[IC][preprocess][international qualfication][" + kvp.Value.target.Tournament().shortName + "][" + kvp.Value.roundId + "][" + kvp.Key.Country().Name() + "] " + kvp.Key.name);
                }
                bool ruleR1 = GetContinentalClubTournament(1) != null ? GetContinentalClubTournament(1).rules.Contains(TournamentRule.OnWinnerQualifiedAdaptClubsQualifications) : false;
                if (ruleR1)
                {
                    foreach (Tournament t in GetContinentalClubTournaments())
                    {
                        for (int j = 0; j < t.nextYearQualified.Length; j++)
                        {
                            for (int k = t.nextYearQualified[j].Count - 1; k >= 0; k--)
                            {
                                if (t.nextYearQualified[j][k].Association().IsDirectConnected(countriesRanking[i]))
                                {
                                    t.nextYearQualified[j].Remove(t.nextYearQualified[j][k]);
                                }
                            }
                        }
                    }
                }
                foreach (KeyValuePair<Club, Qualification> kvp in qualifiedClubs)
                {
                    Utils.Debug("[IC][international qualfication][" + kvp.Value.target.Tournament().shortName + "][" + kvp.Value.roundId + "][" + kvp.Key.Country().Name() + "] " + kvp.Key.name);
                    kvp.Value.target.RegisterTeamForNextEdition(kvp.Key, kvp.Value.roundId);
                }
            }
        }


        public int ContinentalTournamentsCount
        {
            get
            {
                int res = 0;
                foreach (Tournament t in tournaments)
                {
                    //TODO: Move friendly tournaments in a "World" category
                    if (t.periodicity == 1 && t.name != Utils.friendlyTournamentName)
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
            while (t != null)
            {
                res.Add(t);
                t = GetContinentalClubTournament(++i);
            }
            return res;
        }


        public Tournament GetContinentalClubTournament(int level)
        {
            Tournament res = null;
            foreach (Tournament t in tournaments)
            {
                if (t.periodicity == 1 && t.level == level && t.name != Utils.friendlyTournamentName)
                {
                    res = t;
                }
            }
            return res;
        }

        /// <summary>
        /// Returns list of tournaments organized by associations above it
        /// </summary>
        /// <returns></returns>
        public List<Tournament> TournamentsAbove(bool onlyYearPeriodicity)
        {
            List<Tournament> res = new List<Tournament>();
            if(parent != null)
            {
                foreach(Tournament t in parent.tournaments)
                {
                    if((t.periodicity == 1 || !onlyYearPeriodicity) && t.name != Utils.friendlyTournamentName)
                    {
                        res.Add(t);
                    }
                }
                res.AddRange(parent.TournamentsAbove(onlyYearPeriodicity));
            }
            return res;
        }

        /// <summary>
        /// Get clubs involved in tournaments organized by associations above it
        /// </summary>
        /// <param name="clubs"></param>
        /// <returns></returns>
        public List<Club> GetContinentalClubs(List<Club> clubs)
        {
            List<Club> internationalClubs = new List<Club>();

            foreach(Tournament it in TournamentsAbove(true))
            {
                foreach (Round r in it.rounds)
                {
                    foreach (Club club in clubs)
                    {
                        if (r.clubs.Contains(club) && !internationalClubs.Contains(club))
                        {
                            internationalClubs.Add(club);
                        }
                    }
                }
            }

            /*
            level = 1;
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
            }*/
            return internationalClubs;
        }

        public List<Tournament> Tournaments()
        {
            return tournaments;
        }

        public string Name()
        {
            return _name;
        }

        public Association GetContinentalAssociation()
        {
            return this;
        }

        private List<NationalTeam> GetNationalTeams()
        {
            List<ILocalisation> countries = new List<ILocalisation>();
            foreach(Association a in associations)
            {
                countries.Add(a.localisation);
            }
            List<NationalTeam> res = new List<NationalTeam>();
            foreach(Club club in Session.Instance.Game.kernel.Clubs)
            {
                NationalTeam nt = club as NationalTeam;
                if(nt != null && countries.Contains(nt.country))
                {
                    res.Add(nt);
                }
            }
            return res;
        }

        /// <summary>
        /// The parameter "onlyFirstTeams" is ignored because national teams are always first teams
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="methode"></param>
        /// <param name="onlyFirstTeams"></param>
        /// <returns></returns>
        public List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams, Association associationFilter)
        {
            List<NationalTeam> nationalsTeams = GetNationalTeams();
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
            return GetNationalTeams().Count;
        }

        ///
        /// ===========================================
        ///
        /// Domestics tournaments related methods
        ///
        /// ===========================================
        ///

        //RetrieveTeams() (don't forget Continent implementation)
        //CountWithoutReserves() (don't forget Continent implementation)

        private Tournament GetTournamentByLevel(int rank, bool isChampionship)
        {
            Tournament res = null;

            foreach (Tournament t in Tournaments())
            {
                if (t.isChampionship == isChampionship && t.level == rank)
                {
                    res = t;
                }
            }

            return res;
        }

        //OK
        public List<Tournament> Leagues()
        {
            List<Tournament> res = new List<Tournament>();
            foreach (Tournament t in Tournaments())
            {
                if (t.isChampionship)
                {
                    res.Add(t);
                }
            }
            res.Sort((x, y) => x.level.CompareTo(y.level));
            return res;
        }

        //OK
        public Tournament League(int leagueRank)
        {
            return GetTournamentByLevel(leagueRank, true);
        }

        //OK
        public List<Tournament> Cups()
        {
            List<Tournament> res = new List<Tournament>();
            foreach (Tournament t in Tournaments())
            {
                if (!t.isChampionship && t.periodicity == 1)
                {
                    res.Add(t);
                }
            }
            res.Sort(new TournamentComparator());
            return res;

        }

        /**
         * cupRank : for exemple : Coupe de France is level 1 and Coupe de la Ligue is level 2
         */
        //OK
        public Tournament Cup(int cupRank)
        {
            return GetTournamentByLevel(cupRank, false);
        }

        /**
         * Get last league with a national level, then league is subdivised by groups
         */
        public Tournament GetLastNationalLeague()
        {
            int res = -1;
            foreach (Tournament t in Tournaments())
            {
                GroupsRound gr = t.rounds[0] as GroupsRound;
                if (((gr != null && gr.RandomDrawingMethod != RandomDrawingMethod.Administrative)) && t.level > res)
                {
                    res = t.level;
                }
            }

            return League(res);
        }

        /*public Tournament GetLastRegionalLeague(int level)
        {
            int res = -1;
            foreach (Tournament t in Tournaments())
            {
                GroupsRound gr = t.rounds[0] as GroupsRound;
                if (gr != null && gr.RandomDrawingMethod == RandomDrawingMethod.Administrative && gr.administrativeLevel == level && t.level > res)
                {
                    res = t.level;
                }
            }

            return League(res);
        }*/

        public Tournament FirstDivisionChampionship()
        {
            Tournament res = null;
            foreach (Tournament t in _tournaments)
            {
                if (t.isChampionship && t.level == 1)
                {
                    res = t;
                }
            }
            return res;
        }

        public Tournament GetHigherRegionalTournament(int administrativeLevel)
        {
            Tournament higherRegionalTournament = null;
            foreach (Tournament t in Tournaments())
            {
                if (t.isChampionship && (t.rounds[0] as GroupsRound) != null && (t.rounds[0] as GroupsRound).administrativeLevel == administrativeLevel && (higherRegionalTournament == null || t.level < higherRegionalTournament.level))
                {
                    higherRegionalTournament = t;
                }
            }
            return higherRegionalTournament;
        }

        public bool LeagueSystemWithReserves()
        {
            bool res = false;
            for (int i = 0; i < _tournaments.Count && !res; i++)
            {
                Tournament t = _tournaments[i];
                for (int j = 0; j < t.rounds.Count && !res; j++)
                {
                    Round r = t.rounds[j];
                    foreach (Club c in r.clubs)
                    {
                        if ((c as ReserveClub) != null)
                        {
                            res = true;
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Rescrue the best possible club from a league to the upper league.
        /// </summary>
        /// <param name="leagueSystem">League system</param>
        /// <param name="candidates">List of club candidates to be rescrued</param>
        /// <param name="indexLevelRepechage">League index where a club is rescrued</param>
        /// <param name="round">Source round where club is rescrued</param>
        /// <param name="clubsCantBeSaved">List of club who can't be rescrued</param>
        private void RescrueTeam(List<Club>[] leagueSystem, List<Club> candidates, int indexLevelRepechage, Round round, List<Club> clubsCantBeSaved)
        {
            bool found = false;
            int j = 0;
            Console.WriteLine(candidates.Count + " candidates");
            while (!found && j < candidates.Count)
            {
                Club candidate = candidates[j];
                ReserveClub candidateAsReserve = candidate as ReserveClub;
                //TODO: Rules check (doublon ?)
                if (!clubsCantBeSaved.Contains(candidate) && ((candidateAsReserve == null) || (!round.rules.Contains(Rule.ReservesAreNotPromoted) && !UtilsTournaments.ContainsTeamOfClub(leagueSystem[indexLevelRepechage - 1], candidateAsReserve.FannionClub))))
                {
                    found = true;
                    leagueSystem[indexLevelRepechage].Remove(candidate);
                    leagueSystem[indexLevelRepechage - 1].Add(candidate);
                    Console.WriteLine("[Repêchage] " + candidate.name + " (" + Leagues()[indexLevelRepechage].name + " -> " + Leagues()[indexLevelRepechage - 1].name + ")");
                }
                else
                {
                    Console.WriteLine("[Impossible de repêcher] " + candidate.name + "(" + Leagues()[indexLevelRepechage].name + ")");
                }
                j++;
            }
        }

        public int GetLastLeagueLevelWithoutReserves()
        {
            int level = -1;
            foreach (Tournament t in Tournaments())
            {
                if (t.isChampionship && t.rounds[0].rules.Contains(Rule.ReservesAreNotPromoted) && t.level > level)
                {
                    level = t.level;
                }
            }
            return level;
        }

        public void ClearAdministrativeRetrogradationsCache()
        {
            _cacheAdministrativeRetrogradationsChanges = null;
        }

        /// <summary>
        /// Check if a league system is conform compared to current league system
        /// </summary>
        /// <param name="clubsByLeagues">The league system</param>
        /// <returns>True it the league system is conform, False otherwise</returns>
        public bool CheckLeagueConformity(List<Club>[] clubsByLeagues)
        {
            bool res = true;
            List<Tournament> leagues = Leagues();
            for (int i = 0; i < leagues.Count; i++)
            {
                List<Club> thisYear = leagues[i].rounds[0].clubs;
                List<Club> nextYear = clubsByLeagues[i];
                bool leagueTeamsCanVary = LeagueBelow(leagues[i]) == null && parent.Leagues().Count > 0;
                if (thisYear.Count != nextYear.Count && !leagueTeamsCanVary)
                {
                    Console.WriteLine("[CheckLeagueConformity] Error : " + leagues[i].name + " have a different number of teams");
                    res = false;
                }
            }
            return res;
        }

        /// <summary>
        /// Get the bottom league level that a team of an association can reach
        /// </summary>
        /// <param name="division"></param>
        /// <returns>League level (not an index !, start at 1) </returns>
        public int MaxLeagueLevelWithAssociation(Association division)
        {
            List<Tournament> leagues = Leagues();
            int i = GetLastNationalLeague().level;
            bool leagueWithoutTeams = false;
            int associationLevel = 0;
            while (!leagueWithoutTeams && i < leagues.Count)
            {
                Round round = leagues[i].rounds[0];
                GroupsRound groupRound = round as GroupsRound;
                if (groupRound != null)
                {
                    associationLevel = groupRound.administrativeLevel;
                }
                Association divisionLevel = GetAssociationLevel(division, associationLevel);

                leagueWithoutTeams = true;
                //Division level can be null. Ex : Corse have only 1 association level unlike other associations
                if (divisionLevel != null)
                {
                    foreach (Club c in round.clubs)
                    {
                        if (divisionLevel.ContainsAssociation(c.Association()))
                        {
                            leagueWithoutTeams = false;
                        }
                    }
                }
                if (!leagueWithoutTeams)
                {
                    i++;
                }
            }
            return i;
        }

        /// <summary>
        /// Just before resetting leagues, update NextYearQualified by applying administrative retrogradations
        /// </summary>
        public void ApplyAdministrativeRetrogradations()
        {
            Console.WriteLine("[ApplyAdministrativeRetrogradations] " + this.Name());

            List<Tournament> leagues = Leagues();
            List<Club>[] clubsByLeagues = GetAdministrativeRetrogradations();

            for (int i = 0; i < leagues.Count; i++)
            {
                leagues[i].nextYearQualified[0].Clear();
                leagues[i].nextYearQualified[0].AddRange(clubsByLeagues[i]);
            }

            _administrativeRetrogradations.Clear();
            ClearAdministrativeRetrogradationsCache();
        }

        public List<Club>[] GetAdministrativeRetrogradations()
        {
            if (_cacheAdministrativeRetrogradationsChanges != null)
            {
                return _cacheAdministrativeRetrogradationsChanges;
            }
            List<Tournament> leagues = Leagues();
            List<Club>[] clubsByLeagues = new List<Club>[leagues.Count]; //Each leagues teams
            List<Club> clubsCantBeSaved = new List<Club>(); //Clubs that can't be saved from relegation (bottom teams of each league if this rule is activated)
            List<int> administrativeLevels = new List<int>(); //Each leagues administrative level
            for (int i = 0; i < leagues.Count; i++)
            {
                clubsByLeagues[i] = new List<Club>(leagues[i].nextYearQualified[0]);
                ClubComparator comparator = new ClubComparator(ClubAttribute.CURRENT_RANKING, false);
                clubsByLeagues[i].Sort(comparator);
                Console.WriteLine(clubsByLeagues[i].Count);

                //Replace raw ranking by playoff order
                List<KeyValuePair<Club, int>> promotionPlayOffs = leagues[i].GetTopPlayOffClubs();
                promotionPlayOffs.Reverse();
                promotionPlayOffs.Sort(new ClubPlayoffsComparator(new List<Club>(clubsByLeagues[i])));
                foreach (KeyValuePair<Club, int> kvpC in promotionPlayOffs)
                {
                    Club c = kvpC.Key;
                    if (clubsByLeagues[i].Contains(c))
                    {
                        clubsByLeagues[i].Remove(c);
                        if (c.Championship.level < leagues[i].level)
                        {
                            clubsByLeagues[i].Insert(0, c);
                        }
                        else
                        {
                            int j = 0;
                            while (clubsByLeagues[i][j].Championship.level < leagues[i].level)
                            {
                                j++;
                            }
                            clubsByLeagues[i].Insert(j, c);
                        }
                    }
                }

                if(name == "France" || parent?.name == "France")
                {
                    Console.WriteLine("=====" + leagues[i].name + "===== " + clubsByLeagues[i].Count);
                    foreach (KeyValuePair<Club, int> c in promotionPlayOffs)
                    {
                        Console.WriteLine("[playoffs] " + c.Key.name);
                    }
                    foreach (Club c in clubsByLeagues[i])
                    {
                        Round clubC = c.Championship?.rounds[0];
                        //Round clubC = (from Tournament t in Leagues() where t.rounds.Count > 0 && t.rounds[0].clubs.Contains(c) select t.rounds[0]).FirstOrDefault();
                        string adm = (leagues[i].rounds[0] as GroupsRound != null && (leagues[i].rounds[0] as GroupsRound).administrativeLevel > 0) ? "[" + GetAssociationLevel(c.Association(), (leagues[i].rounds[0] as GroupsRound).administrativeLevel).name + "] " : "";
                        Console.WriteLine(adm + c.Championship.name + " - " + comparator.GetRanking(clubC, c) + ". " + c.name);
                    }
                }


                int administrativeLevel = 0;
                if (leagues[i].rounds.Count > 0)
                {
                    Round firstRound = leagues[i].rounds[0];
                    if (firstRound as GroupsRound != null)
                    {
                        GroupsRound gFirstRound = firstRound as GroupsRound;
                        administrativeLevel = gFirstRound.administrativeLevel;
                        if (firstRound.rules.Contains(Rule.BottomTeamNotEligibleForRepechage))
                        {
                            for (int g = 0; g < gFirstRound.groupsCount; g++)
                            {
                                List<Club> gRanking = gFirstRound.Ranking(g);
                                if (gRanking.Count > 0)
                                {
                                    clubsCantBeSaved.Add(gRanking.Last());
                                }
                            }
                        }
                    }
                }
                administrativeLevels.Add(administrativeLevel);
            }

            List<Club> allClubs = new List<Club>();
            foreach (List<Club> allClubsLevel in clubsByLeagues)
            {
                foreach (Club allClubLevel in allClubsLevel)
                {
                    if ((allClubLevel as CityClub) != null)
                    {
                        allClubs.Add(allClubLevel);
                    }
                }
            }
            _cacheAdministrativeRetrogradationsChanges = clubsByLeagues;
            return clubsByLeagues;
        }
    }
}
