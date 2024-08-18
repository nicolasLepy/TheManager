using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Runtime.Serialization;
using tm.Comparators;
using tm.Tournaments;
using AssociationAttribute = tm.Comparators.AssociationAttribute;

namespace tm
{
    [DataContract(IsReference =true)]
    public class Association : ILocalisation
    {
        [DataMember]
        [Key]
        public int Id { get; set; }
        [DataMember]
        private List<Association> _divisions;
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
        private int _resetWeek;
        [DataMember]
        private bool _enableInternationalClubsCompetitions;


        public List<Tournament> tournaments => _tournaments;


        public List<Association> divisions => _divisions;
        public string name => _name;
        public string logo => _logo;
        public ILocalisation localisation => _localisation;
        public Association parent { get => _parent; set => _parent = value; }
        public int resetWeek => _resetWeek;
        public bool enabledInternationalClubsCompetitions => _enableInternationalClubsCompetitions;

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


        public Association()
        {
            _divisions = new List<Association>();
            _tournaments = new List<Tournament>();
            _continentalQualifications = new List<Qualification>();
            _associationRanking = new List<Association>();
            _archivalAssociationRanking = new List<List<Association>>();
            _internationalDates = new List<InternationalDates>();
        }

        public Association(int id, string name, string logo, ILocalisation localisation, Association parent, int resetWeek, bool enableInternationalClubsCompetitions)
        {
            Id = id;
            _name = name;
            _logo = logo;
            _divisions = new List<Association>();
            _tournaments = new List<Tournament>();
            _continentalQualifications = new List<Qualification>();
            _associationRanking = new List<Association>();
            _archivalAssociationRanking = new List<List<Association>>();
            _internationalDates = new List<InternationalDates>();
            _localisation = localisation;
            _parent = parent;
            _resetWeek = resetWeek;
            _enableInternationalClubsCompetitions = enableInternationalClubsCompetitions;
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
                foreach (Association a in divisions)
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
                foreach(Association ad in _divisions)
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
                res = _divisions;
            }
            else
            {
                foreach (Association ad in _divisions)
                {
                    res.AddRange(ad.GetAssociationsLevel(level - 1));
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
                foreach (Association adm in _divisions)
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

            foreach (Association ad in divisions)
            {
                if (ad.Id == id)
                {
                    res = ad;
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
            res.AddRange(divisions);
            foreach(Association a in divisions)
            {
                res.AddRange(a.GetAllChilds());
            }
            return res;
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

        public List<Tournament> GetAllTournaments()
        {
            List<Tournament> res = new List<Tournament>();
            res.AddRange(tournaments);
            foreach(Association a in divisions)
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
            foreach (Association a in divisions)
            {
                //a.localisation.Tournaments().Count > 0 : Because countries still hold domestics tournaments
                if (a.tournaments.Count > 0 || a.localisation.Tournaments().Count > 0)
                {
                    _associationRanking.Add(a);
                }
            }

            _associationRanking.Sort(new AssociationComparator(AssociationAttribute.CONTINENTAL_COEFFICIENT));

        }



        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Country c, int year)
        {
            // Special case when getting clubs of the finished edition (Ireland 2022 for example) for an international tournament not started yet (CL 2023-2024 for example)
            if (year == Session.Instance.Game.CurrentSeason + 1)
            {
                return GetClubsQualifiedForInternationalCompetitions(c, false);
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
                        if (club.Country() == c && (!res.ContainsKey(club) || Utils.IsBefore(r.DateInitialisationRound(), res[club].tournament.rounds[res[club].roundId].DateInitialisationRound())))
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

            List<Association> countriesRanking = new List<Association>(associationRanking);
            int index = countriesRanking.IndexOf(c.GetCountryAssociation());
            int rank = index + 1;
            List<Qualification> associationQualifications = (from q in _continentalQualifications where q.ranking == rank select q).ToList();

            List<Club> registeredClubs = new List<Club>();
            List<Club> leagueClubs = new List<Club>();

            int leagueLevel = 1;
            Tournament leagueDivisionChampionship = c.League(leagueLevel);
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

                leagueDivisionChampionship = c.League(++leagueLevel);
            }

            List<Tournament> cups = c.Cups();
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
                Dictionary<Club, Qualification> qualifiedClubs = GetClubsQualifiedForInternationalCompetitions(countriesRanking[i].localisation as Country, false);
                foreach (KeyValuePair<Club, Qualification> kvp in qualifiedClubs)
                {
                    Utils.Debug("[IC][preprocess][international qualfication][" + kvp.Value.tournament.shortName + "][" + kvp.Value.roundId + "][" + kvp.Key.Country().Name() + "] " + kvp.Key.name);
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
                                if (t.nextYearQualified[j][k].Country() == countriesRanking[i].localisation)
                                {
                                    t.nextYearQualified[j].Remove(t.nextYearQualified[j][k]);
                                }
                            }
                        }
                    }
                }
                foreach (KeyValuePair<Club, Qualification> kvp in qualifiedClubs)
                {
                    Utils.Debug("[IC][international qualfication][" + kvp.Value.tournament.shortName + "][" + kvp.Value.roundId + "][" + kvp.Key.Country().Name() + "] " + kvp.Key.name);
                    kvp.Value.tournament.AddClubForNextYear(kvp.Key, kvp.Value.roundId);
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

        public List<Tournament> Tournaments()
        {
            return tournaments;
        }

        public string Name()
        {
            return _name;
        }

        public Continent GetContinent()
        {
            throw new NotImplementedException();
        }

        ///
        /// Domestics tournaments related methods
        ///

        //RetrieveTeams() (don't forget Continent implementation)
        //CountWithoutReserves() (don't forget Continent implementation)

    }
}