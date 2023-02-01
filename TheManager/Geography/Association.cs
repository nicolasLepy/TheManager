using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference = true)]
    public class Association : IRecoverableTeams
    {

        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public ILocalisation localization { get; set; }

        [DataMember]
        private string _logo;

        [DataMember]
        private List<Association> _associations;

        [DataMember]
        private List<Tournament> _tournaments;

        /**
         * Represent qualification in continental clubs competitions in function of the country place in the coefficient ranking
         */
        [DataMember]
        private List<Qualification> _associationQualifications;
        [DataMember]
        private List<Association> _associationRanking;
        [DataMember]
        private List<List<Association>> _archivalAssociationRanking;

        [DataMember]
        private Dictionary<Club, Tournament> _administrativeRetrogradations;

        [DataMember]
        private Club _associationTeam;

        public List<Association> associations => _associations;
        public string logo => _logo;
        public List<Qualification> associationQualifications => _associationQualifications;
        public List<List<Association>> archivalAssociationRanking => _archivalAssociationRanking;
        public Club associationTeam => _associationTeam;
        public Dictionary<Club, Tournament> administrativeRetrogradations => _administrativeRetrogradations;

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

        // Continentals methods

        public List<NationalTeam> GetNationalTeams()
        {
            List<NationalTeam> res = new List<NationalTeam>();
            
            foreach(Association association in _associations)
            {
                if(association.associationTeam != null)
                {
                    res.Add(association.associationTeam as NationalTeam);
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
        public List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams)
        {
            List<NationalTeam> nationalsTeams = GetNationalTeams();
            foreach (Association association in _associations)
            {
                nationalsTeams.Add(association.associationTeam as NationalTeam);
            }
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

            List<Club> res = new List<Club>();
            for (int i = 0; i < number; i++)
            {
                res.Add(nationalsTeams[i]);
            }
            return res;
        }

        /// <summary>
        /// Get all tournaments including tournaments from sub associations
        /// </summary>
        /// <returns></returns>
        public List<Tournament> AllTournaments()
        {
            List<Tournament> res = new List<Tournament>(_tournaments);

            foreach(Association a in _associations)
            {
                res.AddRange(a.AllTournaments());
            }

            return res;
        }

        /// <summary>
        /// Get all associations including associations from sub associations
        /// </summary>
        /// <returns></returns>
        public List<Association> AllAssociations()
        {
            List<Association> res = new List<Association>(_associations);

            foreach (Association a in _associations)
            {
                res.AddRange(a.AllAssociations());
            }

            return res;
        }

        public int AssociationTournamentsCount
        {
            get
            {
                int res = 0;
                foreach (Tournament t in _tournaments)
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

        /// <summary>
        /// For Europe : level 1 : CL, level 2 : EL, level 3 : ECL
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public Tournament GetAssociationClubTournament(int level)
        {
            Tournament res = null;
            foreach (Tournament t in _tournaments)
            {
                if (t.periodicity == 1 && t.level == level && t.name != Utils.friendlyTournamentName)
                {
                    res = t;
                }
            }
            return res;
        }

        public void UpdateStoredAssociationRanking()
        {
            if (_associationRanking != null && _associationRanking.Count > 0)
            {
                _archivalAssociationRanking.Add(new List<Association>(_associationRanking));
            }
            _associationRanking = new List<Association>();
            foreach (Association a in _associations)
            {
                if (a.tournaments.Count > 0)
                {
                    _associationRanking.Add(a);
                }
            }

            _associationRanking.Sort(new AssociationComparator(AssociationAttribute.CONTINENTAL_COEFFICIENT));

        }

        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Association countryAssociation, int year)
        {
            Dictionary<Club, Qualification> res = new Dictionary<Club, Qualification>();

            int level = 1;
            Tournament tournament = GetAssociationClubTournament(level);
            while (tournament != null)
            {
                Tournament archive = year == Session.Instance.Game.CurrentSeason ? tournament : tournament.previousEditions[year];
                foreach (Round r in archive.rounds)
                {
                    Console.WriteLine(r.clubs.Count);
                    foreach (Club club in r.clubs)
                    {
                        if (club.Association() == countryAssociation && (!res.ContainsKey(club) || Utils.IsBefore(r.DateInitialisationRound(), res[club].tournament.rounds[res[club].roundId].DateInitialisationRound())))
                        {
                            res.Add(club, new Qualification(1, archive.rounds.IndexOf(r), tournament, true, 0));
                        }
                    }
                }
                level++;
                tournament = GetAssociationClubTournament(level);
            }

            return res;
        }


        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Association a)
        {
            Dictionary<Club, Qualification> res = new Dictionary<Club, Qualification>();

            List<Association> associationsRanking = new List<Association>(associationRanking);
            int index = associationsRanking.IndexOf(a);

            List<Club> registeredClubs = new List<Club>();
            List<Club> leagueClubs = new List<Club>();
            Tournament firstDivisionChampionship = a.FirstDivisionChampionship();
            Round championshipRound = firstDivisionChampionship.GetLastChampionshipRound();
            List<Tournament> cups = a.Cups();
            List<Club> cupWinners = new List<Club>();
            foreach (Tournament cup in cups)
            {
                if (cup.currentRound == (cup.rounds.Count - 1) && cup.rounds.Last().matches[0].Played)
                {
                    cupWinners.Add(cup.Winner());
                }
                else
                {
                    cupWinners.Add(null);
                }
            }

            if (championshipRound as ChampionshipRound != null)
            {
                leagueClubs = (championshipRound as ChampionshipRound).Ranking();
            }
            if (championshipRound as InactiveRound != null)
            {
                leagueClubs = (championshipRound as InactiveRound).Ranking();
            }
            if (championshipRound as GroupsRound != null) //TODO: No sense to do this, no tournament finish on a group round (maybe if one day Top and Bottom championships are merged in a group round phase)
            {
                leagueClubs = new List<Club>(championshipRound.clubs);
                leagueClubs.Sort(new ClubRankingComparator(championshipRound.matches));
            }

            List<Club> finalPhasesClubs = firstDivisionChampionship.GetFinalPhasesClubs();
            if (finalPhasesClubs.Count > 0)
            {
                for (int j = finalPhasesClubs.Count - 1; j >= 0; j--)
                {
                    leagueClubs.Remove(finalPhasesClubs[j]);
                    leagueClubs.Insert(0, finalPhasesClubs[j]);
                }
            }
            int rank = index + 1;
            int currentLevel = 0;
            int cupRank = 0;
            foreach (Qualification q in _associationQualifications)
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
            List<Association> associationsRanking = new List<Association>(associationRanking);

            for (int i = 0; i < associationsRanking.Count; i++)
            {
                List<Club> registeredClubs = new List<Club>();
                List<Club> clubs = new List<Club>();
                Tournament firstDivisionChampionship = associationsRanking[i].FirstDivisionChampionship();
                Round championshipRound = firstDivisionChampionship.GetLastChampionshipRound(); //firstDivisionChampionship.rounds[0];
                List<Tournament> cups = associationsRanking[i].Cups();
                List<Club> cupWinners = new List<Club>();
                cups.ForEach(t => cupWinners.Add(t.Winner()));

                if (championshipRound as ChampionshipRound != null)
                {
                    clubs = (championshipRound as ChampionshipRound).Ranking();
                }
                if (championshipRound as InactiveRound != null)
                {
                    clubs = (championshipRound as InactiveRound).Ranking();
                }
                if (championshipRound as GroupsRound != null) //TODO: No sense to do this, no tournament finish on a group round (maybe if one day Top and Bottom championships are merged in a group round phase)
                {
                    clubs = new List<Club>(championshipRound.clubs);
                    clubs.Sort(new ClubRankingComparator(championshipRound.matches));
                }

                List<Club> finalPhasesClubs = firstDivisionChampionship.GetFinalPhasesClubs();
                if (finalPhasesClubs.Count > 0)
                {
                    for (int j = finalPhasesClubs.Count - 1; j >= 0; j--)
                    {
                        clubs.Remove(finalPhasesClubs[j]);
                        clubs.Insert(0, finalPhasesClubs[j]);
                    }
                }
                int rank = i + 1;
                int currentLevel = 0;
                int cupRank = 0;
                foreach (Qualification q in _associationQualifications)
                {
                    if (q.ranking == rank)
                    {
                        Club currentCupWinner = cupRank < cupWinners.Count ? cupWinners[cupRank] : null;
                        //isNextYear is used as "cup winner" here instead of league qualification
                        if ((!q.isNextYear || registeredClubs.Contains(currentCupWinner)) || currentCupWinner == null)
                        {
                            for (int j = 0; j < q.qualifies; j++)
                            {
                                Club qualifiedClub = clubs[currentLevel];
                                //If we get the cup winner and is already qualified, then we move to the next candidate team to avoid the cup winner entering two times in continental tournament
                                while (registeredClubs.Contains(qualifiedClub))
                                {
                                    currentLevel++;
                                    qualifiedClub = clubs[currentLevel];
                                }
                                currentLevel++;
                                q.tournament.AddClubForNextYear(qualifiedClub, q.roundId);
                                registeredClubs.Add(qualifiedClub);
                                Console.WriteLine("Qualifie " + qualifiedClub.name + " pour la " + q.tournament.name + " (tour " + q.roundId + ")");
                            }

                        }
                        else
                        {
                            q.tournament.AddClubForNextYear(currentCupWinner, q.roundId);
                            registeredClubs.Add(currentCupWinner);
                            Console.WriteLine("Qualifie " + currentCupWinner.name + " pour la " + q.tournament.name + " (tour " + q.roundId + ")");
                        }
                        if (q.isNextYear)
                        {
                            cupRank++;
                        }
                    }
                }
            }
        }


        //----

        public List<Tournament> tournaments => _tournaments;

        public Association(int id, string name, ILocalisation localization, string logo, Club associationTeam)
        {
            _associations = new List<Association>();
            _tournaments = new List<Tournament>();
            this.id = id;
            this.name = name;
            this.localization = localization;
            _logo = logo;
            _associationTeam = associationTeam;
        }

        public bool ContainsAssociation(Association association)
        {
            bool res = this == association;

            if (!res)
            {
                foreach (Association a in _associations)
                {
                    if (a.ContainsAssociation(association))
                    {
                        res = true;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Get sub-associations at the requested level.
        /// From FFF, Bourgogne-Franche-Comté is level 1 and Côte d'Or is level 2
        /// From Bourgogne-Franche-Comté, Côte d'Or is level 1
        /// </summary>
        /// <param name="level">Administrative division level</param>
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
                foreach (Association a in _associations)
                {
                    res.AddRange(a.GetAssociationsLevel(level - 1));
                }
            }
            return res;
        }

        // From Country -----

        public float YearAssociationCoefficient(int nSeason)
        {
            List<Club> clubs = new List<Club>();
            float total = 0;
            for (int i = 1; i < 4; i++)
            {
                Tournament continentalTournament = this.Parent.GetAssociationClubTournament(i);
                if (continentalTournament != null)
                {
                    int j = continentalTournament.previousEditions.Count - (-nSeason);

                    if (j >= 0)
                    {
                        Tournament yearContinentalTournament = continentalTournament.previousEditions.ToList()[j].Value;
                        foreach (Tournament championship in _tournaments)
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

        public Association GetCountryGenericAssociation()
        {
            Association res = null;
            foreach (Association a in _associations)
            {
                if (a.name == this.name)
                {
                    res = a;
                }
            }

            return res;
        }

        public ILocalisation String2Localisation(string name)
        {
            ILocalisation res = null;
            if(this.localization?.Name() == name)
            {
                res = this.localization;
            }
            else
            {
                foreach(Association a in _associations)
                {
                    if(res == null)
                    {
                        res = a.String2Localisation(name);
                    }
                }
            }
            return res;
        }

        public Association GetAssociationOfTournament(Tournament tournament)
        {
            Association res = null;
            foreach(Tournament t in tournaments)
            {
                foreach(KeyValuePair<int, Tournament> kvp in t.previousEditions)
                {
                    if (tournament == kvp.Value)
                    {
                        res = this;
                    }
                }
            }
            if(_tournaments.Contains(tournament))
            {
                res = this;
            }
            else
            {
                foreach(Association a in _associations)
                {
                    if(res == null)
                    {
                        res = a.GetAssociationOfTournament(tournament);
                    }
                }
            }
            return res;
        }

        public Association GetAssociation(int id)
        {
            Association res = null;
            foreach (Association a in _associations)
            {
                if (a.id == id)
                {
                    res = a;
                }

                Association resChild = a.GetAssociation(id);
                if (resChild != null)
                {
                    res = resChild;
                }
            }
            return res;
        }

  
        /// <summary>
        /// Get a parent association of the association at the defined level (level 0 is the current association).
        /// From FFF, request Côte d'Or at level 1 return Bourgogne-Franche-Comté
        /// </summary>
        /// <param name="administrativeDivision"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public Association GetAssociationAtLevelFromAssociation(Association association, int level)
        {
            Association res = null;
            List<Association> levelAssociations = GetAssociationsLevel(level);

            foreach (Association a in levelAssociations)
            {
                if (a == association || a.ContainsAssociation(association))
                {
                    res = a;
                }
            }
            return res;
        }


        //----------------

        public Association Parent => GetParent(Session.Instance.Game.kernel.worldAssociation);

        public Association GetParent(Association from)
        {
            Association parent = null;
            if(from.associations.Contains(this))
            {
                parent = from;
            }
            else
            {
                foreach(Association a in from.associations)
                {
                    parent = parent == null ? GetParent(a) : parent;
                }
            }
            return parent;
        }


        public int GetLastLeagueLevelWithoutReserves()
        {
            int level = -1;
            foreach (Tournament t in tournaments)
            {
                if (t.isChampionship && t.rounds[0].rules.Contains(Rule.ReservesAreNotPromoted) && t.level > level)
                {
                    level = t.level;
                }
            }
            return level;
        }

        public bool LeagueSystemWithReserves()
        {
            bool res = false;
            for (int i = 0; i < tournaments.Count && !res; i++)
            {
                Tournament t = tournaments[i];
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

        private Tournament GetTournamentByLevel(int rank, bool isChampionship)
        {
            Tournament res = null;

            foreach (Tournament t in tournaments)
            {
                if (t.isChampionship == isChampionship && t.level == rank)
                {
                    res = t;
                }
            }

            return res;

        }

        public Tournament League(int leagueRank)
        {
            return GetTournamentByLevel(leagueRank, true);
        }

        public List<Tournament> Cups()
        {
            int i = 1;
            List<Tournament> res = new List<Tournament>();
            Tournament cup = GetTournamentByLevel(i, false);
            while (cup != null)
            {
                res.Add(cup);
                cup = GetTournamentByLevel(++i, false);
            }
            return res;

        }

        /**
         * cupRank : for exemple : Coupe de France is level 1 and Coupe de la Ligue is level 2
         */
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
            foreach (Tournament t in tournaments)
            {
                GroupsRound gr = t.rounds[0] as GroupsRound;
                if (gr != null && gr.RandomDrawingMethod != RandomDrawingMethod.Administrative && t.level > res)
                {
                    res = t.level;
                }
            }

            return League(res);
        }


        public Tournament GetLastRegionalLeague(int level)
        {
            int res = -1;
            foreach (Tournament t in tournaments)
            {
                GroupsRound gr = t.rounds[0] as GroupsRound;
                if (gr != null && gr.RandomDrawingMethod == RandomDrawingMethod.Administrative && gr.administrativeLevel == level && t.level > res)
                {
                    res = t.level;
                }
            }

            return League(res);
        }

        public Tournament GetHigherRegionalTournament(int administrativeLevel)
        {
            Tournament higherRegionalTournament = null;
            foreach (Tournament t in tournaments)
            {
                if (t.isChampionship && (t.rounds[0] as GroupsRound) != null && (t.rounds[0] as GroupsRound).administrativeLevel == administrativeLevel && (higherRegionalTournament == null || t.level < higherRegionalTournament.level))
                {
                    higherRegionalTournament = t;
                }
            }
            return higherRegionalTournament;
        }

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

        public int CountWithoutReserves()
        {
            return GetNationalTeams().Count;
        }

        public Association String2Association(String str)
        {
            Association res = null;
            if (this.name == str)
            {
                res = this;
            }
            else
            {
                foreach (Association a in _associations)
                {
                    res = res == null ? a.String2Association(str) : res;
                }
            }
            return res;
        }

        public Association Club2AssociationTeam(Club c)
        {
            Association res = null;
            if (this.associationTeam == c)
            {
                res = this;
            }
            else
            {
                foreach(Association a in _associations)
                {
                    res = res == null ? a.Club2AssociationTeam(c) : res;
                }
            }
            return res;
        }

        public Country City2Country(City city)
        {
            Country res = null;
            Country c = this.localization as Country;
            if(c != null && c.cities.Contains(city))
            {
                res = c;
            }
            else
            {
                foreach(Association a in _associations)
                {
                    res = res == null ? a.City2Country(city) : res;
                }
            }

            return res;
        }

        public Association GetAssociationOfLocalizable(ILocalisation localizable)
        {
            Association res = null;
            if(this.localization == localization)
            {
                res = this;
            }
            else
            {
                foreach(Association a in _associations)
                {
                    res = res == null ? a.GetAssociationOfLocalizable(localizable) : res;
                }
            }
            return res;
        }

        public List<Association> GetAssociationsOfHierarchyLevel(int level)
        {
            List<Association> res = new List<Association>();
            if(level == 1)
            {
                res = new List<Association>(this.associations);
            }
            else
            {
                foreach(Association a in _associations)
                {
                    res.AddRange(a.GetAssociationsOfHierarchyLevel(level - 1));
                }
            }
            return res;
        }
    }
}

//Relegation promotion divisions adminisitratives fonctionnent toujours
//Réglage coupe nationale fonctionne toujours
//Qualifications européennes
//Coefficients européens
//Coupes intercontinentales de sélections nationales
//Coupe du Monde et finales hosté par une seule sous-association

//A compléter selon les fonctions qui seront modifiées