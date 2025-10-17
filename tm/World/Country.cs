using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using tm.Comparators;
using tm.Tournaments;

namespace tm
{

    [DataContract(IsReference = true)]
    public class Country : ILocalisation
    {
        [DataMember]
        [Key]
        public int Id { get; set; }
        [DataMember]
        private List<City> _cities;
        [DataMember]
        private List<Stadium> _stadiums;
        [DataMember]
        private Language _language;
        [DataMember]
        private List<Tournament> _tournaments;
        [DataMember]
        private string _dbName;
        [DataMember]
        private string _name;
        [DataMember]
        private int _shapeNumber;
        [DataMember]
        private List<Association> _associations;
        //TODELETE
        [DataMember]
        private int _resetWeek;

        public List<City> cities => _cities;
        public List<Stadium> stadiums => _stadiums;
        public Language language => _language;

        public List<Association> associations => _associations;

        public int resetWeek => _resetWeek;

        public string Flag
        {
            get
            {
                return Utils.NormalizeFilename(_name);
            }
        }

        public string DbName { get => _dbName; }
        public int ShapeNumber { get => _shapeNumber; }

        public Country()
        {
            _cities = new List<City>();
            _stadiums = new List<Stadium>();
            _tournaments = new List<Tournament>();
            _associations = new List<Association>();
        }

        public Country(int id, string dbName, string name, Language language, int shapeNumber, int resetWeek, List<AdministrativeSanction> administrativeSanctionsDefinitions)
        {
            Id = id;
            _dbName = dbName;
            _name = name;
            _language = language;
            _cities = new List<City>();
            _stadiums = new List<Stadium>();
            _tournaments = new List<Tournament>();
            _shapeNumber = shapeNumber;
            _associations = new List<Association>();
            _resetWeek = resetWeek;
        }

        public Association GetCountryAssociation()
        {
            Association res = null;
            foreach (Association ad in _associations)
            {
                if (ad.name == this._name)
                {
                    res = ad;
                }
            }

            return res;
        }

        public Association GetAssociation(int id)
        {
            Association res = null;
            foreach (Association ad in _associations)
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
        ///
        /// </summary>
        /// <param name="level">Administrative division level</param>
        /// <returns></returns>
        public List<Association> GetAssociationsLevel(int level)
        {
            List<Association> res = new List<Association>();
            if (level == 1)
            {
                res = GetCountryAssociation().associations;
            }
            else
            {
                foreach (Association ad in GetCountryAssociation().associations)
                {
                    res.AddRange(ad.GetAssociationsLevel(level - 1));
                }
            }
            return res;
        }

        public int GetLevelOfAssociation(Association association)
        {
            int level = 0;
            Association admAtLevel = null;
            while (admAtLevel != association)
            {
                admAtLevel = GetAssociationLevel(association, ++level);
            }
            return level;
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

        public List<Tournament> Tournaments()
        {
            return _tournaments;
        }

        // Done
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

        //Done
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

        //Done
        public Tournament League(int leagueRank)
        {
            return GetTournamentByLevel(leagueRank, true);
        }

        //Done
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
            res.Sort((x, y) => x.level.CompareTo(y.level));
            return res;

        }

        /**
         * cupRank : for exemple : Coupe de France is level 1 and Coupe de la Ligue is level 2
         */
        //Done
        public Tournament Cup(int cupRank)
        {
            return GetTournamentByLevel(cupRank, false);
        }

        /**
         * Get last league with a national level, then league is subdivised by groups
         */
        //Done
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

        public override string ToString()
        {
            return _name;
        }

        public string Name()
        {
            return _name;
        }

        public Continent Continent
        {
            get
            {
                Continent res = null;
                foreach (Continent c in Session.Instance.Game.kernel.world.continents)
                {
                    foreach (Country cy in c.countries)
                    {
                        if (cy == this)
                        {
                            res = c;
                        }
                    }
                }
                return res;
            }
        }

        public Association GetContinentalAssociation()
        {
            return GetCountryAssociation().parent;
        }


        /*/// <summary>
        /// Preprocess administrative retrogradation by appliying changes to reserves due to retrogradation of fannion teams
        /// </summary>
        public Dictionary<Club, Tournament> PreprocessAdministrativeRetrogradation(Club club, Tournament retrogradationTournament, List<Tournament> leagues, List<Club>[] clubsByLeagues, bool bankruptcy)
        {
            KeyValuePair<Club, Tournament> retrogradation = new KeyValuePair<Club, Tournament>(club, retrogradationTournament);
            Tournament target = retrogradationTournament;
            Dictionary<Club, Tournament> newRetrogradations = new Dictionary<Club, Tournament>();
            if(retrogradationTournament != null)
            {
                newRetrogradations.Add(retrogradation.Key, retrogradation.Value);
            }
            else
            {
                target = leagues[UtilsTournaments.GetClubLevelInLeaguesHierarchy(club, clubsByLeagues)];
            }

            int clubLevel = -1;
            int targetIndex = target.level - 1;
            int maxLeagueLevelForThisAssociation = MaxLeagueLevelWithAssociation(club.Association());
            int targetIndexForReserves = Math.Min(targetIndex + 1, maxLeagueLevelForThisAssociation - 1);
            for (int i = 0; i < clubsByLeagues.Length && clubLevel == -1; i++)
            {
                clubLevel = clubsByLeagues[i].Contains(club) ? i : clubLevel;
            }

            //Case bankruptcy : Fannion team is "deleted". B teams become A team (by retrogradation), C teams become B team (by retrogradation) ...
            //Last reserve is sent to bottom division available to the administrative division of the club
            //
            //Case not bankruptcy : Just throw the fannion team at the defined league position. Check to retrograde reserves to avoid having reserves higher than fanion team on the league structure

            CityClub cClub = club as CityClub;
            if(bankruptcy && cClub != null && cClub.reserves.Count > 0)
            {
                for (int i = 0; i < cClub.reserves.Count; i++)
                {
                    ReserveClub reserve = cClub.reserves[i];
                    if (i == cClub.reserves.Count-1)
                    {
                        newRetrogradations.Add(reserve, leagues[maxLeagueLevelForThisAssociation - 1]);
                    }
                    else
                    {
                        int levelReserve = -1;
                        int levelNextReserve = -1;
                        for(int j = 0; j < clubsByLeagues.Length; j++)
                        {
                            levelReserve = clubsByLeagues[j].Contains(cClub.reserves[i]) ? j : levelReserve;
                            levelNextReserve = clubsByLeagues[j].Contains(cClub.reserves[i+1]) ? j : levelNextReserve;
                        }
                        if(levelReserve > -1 && levelNextReserve > -1)
                        {
                            newRetrogradations.Add(reserve, leagues[levelNextReserve]);
                        }
                    }
                }
            }
            else
            {
                List<ReserveClub> processedReserves = new List<ReserveClub>();
                for (int i = clubLevel; i <= targetIndex; i++)
                {
                    foreach (Club c in clubsByLeagues[i])
                    {
                        ReserveClub rc = c as ReserveClub;
                        if (rc != null && rc.FannionClub == club && targetIndexForReserves > i && !processedReserves.Contains(rc))
                        {
                            newRetrogradations.Add(rc, leagues[targetIndexForReserves]);
                            processedReserves.Add(rc);
                            Console.WriteLine("[Rétrograde] " + rc.name + " : " + leagues[i].name + " -> " + leagues[targetIndexForReserves].name);
                            targetIndex = targetIndexForReserves;
                            targetIndexForReserves = Math.Min(targetIndexForReserves + 1, maxLeagueLevelForThisAssociation - 1);
                        }
                    }
                }
            }

            return newRetrogradations;
        }*/

        /*public List<Club>[] GetAdministrativeRetrogradations()
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


                Console.WriteLine("=====" + leagues[i].name + "===== " + clubsByLeagues[i].Count);
                foreach (KeyValuePair<Club, int> c in promotionPlayOffs)
                {
                    Console.WriteLine("[playoffs] " + c.Key.name);
                }
                foreach (Club c in clubsByLeagues[i])
                {
                    Round clubC = (from Tournament t in Leagues() where t.rounds.Count > 0 && t.rounds[0].clubs.Contains(c) select t.rounds[0]).FirstOrDefault();
                    string adm = (leagues[i].rounds[0] as GroupsRound != null && (leagues[i].rounds[0] as GroupsRound).administrativeLevel > 0) ? "[" + GetAssociationLevel(c.Association(), (leagues[i].rounds[0] as GroupsRound).administrativeLevel).name + "] " : "";
                    Console.WriteLine(adm + c.Championship.name + " - " + comparator.GetRanking(clubC, c) + ". " + c.name);
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

            //For each club (and its retrogradation if concerned), check if this leads to other relegations (reserves). Iterate through these relegations
            foreach (Club currentClub in allClubs)
            //foreach (KeyValuePair<Club, Tournament> mainRetrogradation in _administrativeRetrogradations)
            {
                bool isBankruptcy = false;
                Tournament tournamentRetrogradation = _administrativeRetrogradations.ContainsKey(currentClub) ? _administrativeRetrogradations[currentClub] : null;
                Dictionary<Club, Tournament> clubRetrogradations = PreprocessAdministrativeRetrogradation(currentClub, tournamentRetrogradation, leagues, clubsByLeagues, administrativeLevels, isBankruptcy);

                foreach (KeyValuePair<Club, Tournament> retrogradation in clubRetrogradations)
                {
                    Club club = retrogradation.Key;
                    int clubLevel = -1;
                    for (int i = 0; i < clubsByLeagues.Length && clubLevel == -1; i++)
                    {
                        List<Club> clubs = clubsByLeagues[i];
                        if (clubs.Contains(club))
                        {
                            clubLevel = i;
                        }
                    }

                    int targetLeagueIndex = leagues.IndexOf(retrogradation.Value);
                    clubsByLeagues[targetLeagueIndex].Add(club);
                    clubsByLeagues[clubLevel].Remove(club);
                    Console.WriteLine("[Administrative Retrogradation] " + club.name + " (" + leagues[clubLevel].name + " -> " + leagues[targetLeagueIndex].name + ")");

                    //On remonte un club qui respecte les règles de chaque division
                    for (int i = targetLeagueIndex; i > clubLevel; i--)
                    {
                        Console.WriteLine("_____________________________");
                        Round round = leagues[i].rounds[0];
                        int administrativeLevel = administrativeLevels[i];
                        //Repechage candidates : filtering by association if necessary
                        List<Club> candidates = administrativeLevel == 0 ? clubsByLeagues[i] : UtilsTournaments.FilterAssociation(clubsByLeagues[i], GetAssociationLevel(club.Association(), administrativeLevel));
                        if (administrativeLevel > 0)
                        {
                            Console.WriteLine("[Remonte un tour régional] " + GetAssociationLevel(club.Association(), administrativeLevel).name);
                        }
                        RescrueTeam(clubsByLeagues, candidates, i, round, clubsCantBeSaved);
                        
                        //int j = 0;
                        //bool found = false;
                        //while (!found && j < candidates.Count)
                        //{
                        //    Club candidate = candidates[j];
                        //    ReserveClub candidateAsReserve = candidate as ReserveClub;
                        //    //TODO: Rules check (doublon ?)
                        //    if (!clubsCantBeSaved.Contains(candidate) && ((candidateAsReserve == null) || (!round.rules.Contains(Rule.ReservesCannotBePromoted) && !ContainsTeamOfClub(clubsByLeagues[i - 1], candidateAsReserve.FannionClub))))
                        //    {
                        //        found = true;
                        //        clubsByLeagues[i].Remove(candidate);
                        //        clubsByLeagues[i - 1].Add(candidate);
                        //        Console.WriteLine("[Repêchage] " + candidate.name + " (" + leagues[i].name + " -> " + leagues[i - 1].name + ")");
                        //    }
                        //    else
                        //    {
                        //        Console.WriteLine("[Impossible de repêcher] " + candidate.name + "(" + leagues[i].name + ")");
                        //    }
                        //    j++;
                        //}
                    }
                }
            }

            // !!! Special case
            // Rare case
            // Much simpler to manage it here
            // For each penultimate league of each association, sometime they can't find team for repechage from the bottom league so the league lose one team.
            // We go through candidates for repechage and save the number of missing teams who match conditions to play this league.
            List<Club>[] currentLeagueSystem = GetCurrentLeagueSystem();
            int totalTeams = currentLeagueSystem.SelectMany(list => list).Distinct().Count();
            int totalTeamsQualified = clubsByLeagues.SelectMany(list => list).Distinct().Count();
            //Only apply the fix if all leagues are finished and closed
            if(totalTeams == totalTeamsQualified)
            {
                for (int i = 0; i < leagues.Count; i++)
                {
                    GroupsRound round = leagues[i].rounds[0] as GroupsRound;
                    if(round != null)
                    {
                        foreach (Association association in GetAssociationsLevel(round.administrativeLevel))
                        {
                            int maxPossibleIndex = MaxLeagueLevelWithAssociation(association) - 1;
                            //Check only the penultimate league
                            if(maxPossibleIndex - i == 1)
                            {
                                int missingTeam = UtilsTournaments.FilterAssociation(currentLeagueSystem[i], association).Count - UtilsTournaments.FilterAssociation(clubsByLeagues[i], association).Count;
                                Console.WriteLine(association.name + " => missing " + missingTeam + " teams (" + (UtilsTournaments.FilterAssociation(currentLeagueSystem[i], association).Count) + " - " + (UtilsTournaments.FilterAssociation(clubsByLeagues[i], association).Count));
                                for (int t = 0; t < missingTeam; t++)
                                {
                                    //Big duplicate
                                    int indexLevelRepechage = i + 1;
                                    List <Club> candidates = round.administrativeLevel == 0 ? clubsByLeagues[indexLevelRepechage] : UtilsTournaments.FilterAssociation(clubsByLeagues[indexLevelRepechage], association);
                                    RescrueTeam(clubsByLeagues, candidates, indexLevelRepechage, leagues[indexLevelRepechage].rounds[0], clubsCantBeSaved);
                                    Console.WriteLine(candidates.Count + " candidates");
                                    //bool found = false;
                                    //int j = 0;
                                    //while (!found && j < candidates.Count)
                                    //{
                                    //    Club candidate = candidates[j];
                                    //    ReserveClub candidateAsReserve = candidate as ReserveClub;
                                    //    //TODO: Rules check (doublon ?)
                                    //    if (!clubsCantBeSaved.Contains(candidate) && ((candidateAsReserve == null) || (!round.rules.Contains(Rule.ReservesCannotBePromoted) && !ContainsTeamOfClub(clubsByLeagues[indexLevelRepechage - 1], candidateAsReserve.FannionClub))))
                                    //    {
                                    //        found = true;
                                    //        clubsByLeagues[indexLevelRepechage].Remove(candidate);
                                    //        clubsByLeagues[indexLevelRepechage - 1].Add(candidate);
                                    //        Console.WriteLine("[Repêchage] " + candidate.name + " (" + leagues[indexLevelRepechage].name + " -> " + leagues[indexLevelRepechage - 1].name + ")");
                                    //    }
                                    //   else
                                    //    {
                                    //        Console.WriteLine("[Impossible de repêcher] " + candidate.name + "(" + leagues[indexLevelRepechage].name + ")");
                                    //    }
                                    //    j++;
                                    //}
                                }
                            }
                        }
                    }
                }
            }

            _cacheAdministrativeRetrogradationsChanges = clubsByLeagues;
            return clubsByLeagues;
        }*/

    }
}