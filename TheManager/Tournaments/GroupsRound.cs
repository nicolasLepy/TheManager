using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TheManager.Comparators;
using TheManager.Tournaments;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class GroupsRound : Round
    {
        [DataMember]
        private int _groupsNumber;
        [DataMember]
        private List<Club>[] _groups;

        [DataMember]
        private RandomDrawingMethod _randomDrawingMethod;
        [DataMember]
        private int _administrativeLevel;
        [DataMember]
        private List<GeographicPosition> _groupsLocalisation;
        [DataMember]
        private List<string> _groupsNames;
        [DataMember]
        private int _referenceClubsByGroup;

        public int referenceClubsByGroup => _referenceClubsByGroup;


        public List<Club>[] groups { get => _groups; }

        public int groupsCount
        {
            get => _groupsNumber; set => _groupsNumber = value;
        }

        public int maxClubsInGroup => _clubs.Count % _groupsNumber > 0 ? (_clubs.Count / _groupsNumber) + 1 : _clubs.Count / _groupsNumber;

        public int administrativeLevel => _administrativeLevel;

        public List<GeographicPosition> groupsLocalisation { get => _groupsLocalisation; }

        public RandomDrawingMethod RandomDrawingMethod => _randomDrawingMethod;
        
        public string GroupName(int groupId)
        {
            string res = "";
            if (_groupsNames.Count > groupId)
            {
                res = _groupsNames[groupId];
            }
            else
            {
                res = "Groupe " + (groupId + 1);
            }
            return res;
        }

        public void ClearGroupNames()
        {
            _groupsNames.Clear();
        }
        
        public void AddGroupName(string name)
        {
            _groupsNames.Add(name);
        }

        public GroupsRound(string name, Hour hour, List<GameDay> dates, List<TvOffset> offsets, int groupsCount, bool twoLegs, GameDay initialisation, GameDay end, RandomDrawingMethod randomDrawingMethod, int administrativeLevel) : base(name, hour, dates, offsets, initialisation,end, twoLegs,0)
        {
            _groupsNumber = groupsCount;
            _groups = new List<Club>[_groupsNumber];
            _groupsNames = new List<string>();
            for (int i = 0; i < _groupsNumber; i++)
            {
                _groups[i] = new List<Club>();
            }
            _randomDrawingMethod = randomDrawingMethod;
            _groupsLocalisation = new List<GeographicPosition>();
            _referenceClubsByGroup = 0;
            _administrativeLevel = administrativeLevel;
        }

        public override Round Copy()
        {
            GroupsRound t = new GroupsRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), groupsCount, twoLegs, programmation.initialisation, programmation.end, _randomDrawingMethod, _administrativeLevel);
            foreach (Match m in this.matches)
            {
                t.matches.Add(m);
            }
            foreach (Club c in this.clubs)
            {
                t.clubs.Add(c);
            }
            int i = 0;
            foreach (List<Club> c in _groups)
            {
                t._groups[i] = new List<Club>(c);
                i++;
            }
            return t;
        }

        public void InitializeGroups()
        {
            _groups = new List<Club>[_groupsNumber];
            for (int i = 0; i < _groupsNumber; i++)
            {
                _groups[i] = new List<Club>();
            }
        }

        private AdministrativeDivision GetGroupAdministrativeDivision(int group, int admLevel = -1)
        {
            AdministrativeDivision res = null;
            if (_groups[group].Count > 0)
            {
                res = _groups[group][0].AdministrativeDivision();
                if (admLevel != -1)
                {
                    res = _groups[group][0].Country().GetAdministrativeDivisionLevel(res, admLevel);
                }
            }
            return res;
        }

        public List<Club> GetAdministrativeDivisionRelegablesCandidates(AdministrativeDivision administrativeDivision)
        {
            List<Club> candidates = new List<Club>();
            List<int> admGroups = GetGroupsFromAdministrativeDivision(administrativeDivision);
            int maxTeamByGroup = 0;
            foreach (int group in admGroups)
            {
                maxTeamByGroup = groups[group].Count > maxTeamByGroup ? groups[group].Count : maxTeamByGroup;
            }

            for (int i = -1; i > -maxTeamByGroup-1; i--)
            {
                List<Club> rankingI = RankingByRank(i, administrativeDivision);
                rankingI.Reverse();
                foreach(Club c in rankingI)
                {
                    //Pour chaque club, si déjà pas concerné par une relegation, on ajoute
                    candidates.Add(c);
                }
            }
            return candidates;
        }
        
        public List<Club> RankingByRank(int rank, AdministrativeDivision administrativeDivision)
        {
            List<Club> ranking = new List<Club>();
            for (int i = 0; i < _groupsNumber; i++)
            {
                AdministrativeDivision adm = GetGroupAdministrativeDivision(i);
                if (administrativeDivision != null && administrativeDivision.ContainsAdministrativeDivision(adm))
                {
                    List<Club> rankingGroup = Ranking(i);
                    int realRanking = rank > 0 ? rank - 1 : rankingGroup.Count + rank;
                    if (realRanking < rankingGroup.Count)
                    {
                        ranking.Add(rankingGroup[realRanking]);
                    }
                }
            }
            ranking.Sort(new ClubRankingComparator(_matches));
            return ranking;
        }
        
        public override void Initialise()
        {
            InitializeGroups();
            if (!Tournament.IsInternational())
            {
                AddTeamsToRecover();
            }
            //If it's an international tournament (national teams or continental cup eg), we add all teams to recover for all rounds now because ranking can fluctuate after and the same team could be selected for 2 differents rounds
            else if (Tournament.rounds[0] == this)
            {
                foreach (Round r in Tournament.rounds)
                {
                    r.AddTeamsToRecover();
                }
            }


            SetGroups();
            for (int i = 0; i < _groupsNumber; i++)
            {
                _matches.AddRange(Calendar.GenerateCalendar(_groups[i], this, twoLegs));
            }

        }

        public Qualification[] SwapQualifications(Qualification[] qualifications)
        {
            Qualification[] res = new Qualification[2];
            if(qualifications.Length > 1)
            {
                res[0] = qualifications[0];
                res[1] = qualifications[1];
                res[0].ranking = qualifications[1].ranking;
                res[1].ranking = qualifications[0].ranking;
            }
            return res;
        }

        public List<int> GetGroupsFromAdministrativeDivision(AdministrativeDivision administrativeDivision)
        {
            List<int> res = new List<int>();
            for (int i = 0; i < _groupsNumber; i++)
            {
                if (_groups[i].Count > 0)
                {
                    AdministrativeDivision admAtLeagueLevel = _groups[i][0].Country().GetAdministrativeDivisionLevel(_groups[i][0].AdministrativeDivision(), _administrativeLevel);
                    if (admAtLeagueLevel == administrativeDivision)
                    {
                        res.Add(i);
                    }
                }
            }

            return res;
        }

        public void UpdateQualificationTournament(List<Qualification> qualifications, int ranking, Tournament tournament)
        {
            //TODO : Maybe add a check if the ranking is neutral
            for (int j = 0; j < qualifications.Count; j++)
            {
                Qualification q = qualifications[j];
                if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == ranking)
                {
                    qualifications[j] = new Qualification(q.ranking, q.roundId, tournament,
                        q.isNextYear, 0);
                }
            }
        }
        
        public List<Qualification> AdjustQualificationAdministrativeDivision(List<Qualification> baseQualifications, int group)
        {
            Console.WriteLine("== Groupe n°" + group + "==");
            List<Qualification> adjustedQualifications = new List<Qualification>(baseQualifications);
            adjustedQualifications.Sort(new QualificationComparator());
            Country country = _groups[group][0].Country();
            //Get the district of the group (at the good regional level)
            AdministrativeDivision administrativeDivision = country.GetAdministrativeDivisionLevel(_groups[group][0].AdministrativeDivision(), _administrativeLevel);
            int admGroupCount = GetGroupsFromAdministrativeDivision(administrativeDivision).Count;

            //If this league is this first district league, automatically compute promotion slots
            GroupsRound upperGroupRound = country.League(Tournament.level - 1).rounds[0] as GroupsRound;
            bool extraPromotionOnGroup = false;
            int promotionSlots = 0;
            if (upperGroupRound != null && upperGroupRound.administrativeLevel > 0 && _administrativeLevel > upperGroupRound._administrativeLevel)
            {
                AdministrativeDivision upperAdministrativeDivision = country.GetAdministrativeDivisionLevel(administrativeDivision, upperGroupRound.administrativeLevel);
                int admGroupUpper = 0;
                for (int i = 0; i < _groupsNumber; i++)
                {
                    if (groups[i][0].Country().GetAdministrativeDivisionLevel(groups[i][0].AdministrativeDivision(), upperGroupRound._administrativeLevel) == upperAdministrativeDivision)
                    {
                        admGroupUpper++;
                    }
                }
                foreach (Qualification q in upperGroupRound.qualifications)
                {
                    if (q.isNextYear && q.roundId == 0 && q.tournament == Tournament)
                    {
                        promotionSlots++;
                    }
                }

                promotionSlots = promotionSlots + 1; //TODO : +1 or +offset national/regional
                
                int promotionByGroup = promotionSlots / admGroupUpper;
                int extraPromotions = promotionSlots % admGroupUpper;
                for (int i = 1; i <= promotionByGroup; i++)
                {
                    UpdateQualificationTournament(adjustedQualifications, i, upperGroupRound.Tournament);
                }

                if (extraPromotions > 0)
                {
                    List<Club> rankingRank = RankingByRank(promotionByGroup + 1, upperAdministrativeDivision);
                    Club concerned = Ranking(group)[promotionByGroup];
                    if (rankingRank.IndexOf(concerned) < extraPromotions)
                    {
                        UpdateQualificationTournament(adjustedQualifications, promotionByGroup+1, upperGroupRound.Tournament);
                        extraPromotionOnGroup = true;
                    }
                }
            }
            
            //TODO : Round with barrages are not managed, creating barrage not managed
            if (admGroupCount > 1)
            {
                //Multiple groups by adm, split promotion/relegation on groups
                int promotionPlaces = -1;
                int relegationPlaces = -1;
                foreach (Qualification q in adjustedQualifications)
                {
                    if (q.tournament == this.Tournament && q.isNextYear && q.roundId == 0 && promotionPlaces == -1)
                    {
                        promotionPlaces = q.ranking-1;
                    }

                    if (q.tournament.level > this.Tournament.level && q.isNextYear && q.roundId == 0 &&
                        relegationPlaces == -1)
                    {
                        relegationPlaces = adjustedQualifications.Count - q.ranking + 1;
                    }
                }

                if (promotionPlaces > -1)
                {
                    int promotionByGroup = promotionPlaces / admGroupCount;
                    int extraPromotions = promotionPlaces % admGroupCount;
                    int promotionOffset = promotionPlaces - promotionByGroup - (extraPromotions != 0 ? 1 : 0);
                    for (int i = promotionPlaces; i > promotionPlaces-promotionOffset; i--)
                    {
                        UpdateQualificationTournament(adjustedQualifications, i, Tournament);
                    }

                    if (extraPromotions != 0)
                    {
                        int ranking = promotionPlaces - promotionOffset;
                        List<int> groupsAdm = GetGroupsFromAdministrativeDivision(administrativeDivision);
                        Club groupConcerned = Ranking(group)[ranking];
                        List<Club> rankingI = new List<Club>();
                        foreach(int i in groupsAdm)
                        {
                            rankingI.Add(Ranking(i)[ranking]);
                        }

                        rankingI.Sort(new ClubRankingComparator(_matches));
                        if (rankingI.IndexOf(groupConcerned) >= extraPromotions)
                        {
                            UpdateQualificationTournament(adjustedQualifications, ranking, Tournament);
                        }
                    }
                }

                if (relegationPlaces > -1)
                {
                    Console.WriteLine("[" + Tournament.name + "][" + _groupsNames[group] + "][" + administrativeDivision.name + "] " + relegationPlaces + " équipes descendent");
                    int relegationsByGroup = relegationPlaces / admGroupCount;
                    int extraRelegations = relegationPlaces % admGroupCount;
                    int relegationOffset = relegationPlaces - relegationsByGroup - (extraRelegations != 0 ? 1 : 0);
                    Console.WriteLine("[" + Tournament.name + "][" + _groupsNames[group] + "][" + administrativeDivision.name + "] " + relegationsByGroup + " par groupe, " + extraRelegations + "extra");
                    for (int i = _groups[group].Count-relegationPlaces+1; i < _groups[group].Count-relegationPlaces+relegationOffset+1; i++)
                    {
                        UpdateQualificationTournament(adjustedQualifications, i, Tournament);
                    }

                    if (extraRelegations != 0)
                    {
                        int ranking = _groups[group].Count - relegationsByGroup - 1;// + relegationOffset + 1;// - (relegationOffset == 0 ? 1 : 0);
                        int relativeRanking = ranking - _groups[group].Count;
                        /*int ranking = _groups[group].Count - relegationPlaces + relegationOffset + 1;// - (relegationOffset == 0 ? 1 : 0);
                        int relativeRanking = _groups[group].Count - ranking - 1;*/
                        Club groupConcerned = Ranking(group)[ranking];
                        List<Club> rankingI = RankingByRank(relativeRanking, administrativeDivision);
                        int cOff = rankingI.IndexOf(groupConcerned);
                        Console.WriteLine(rankingI.IndexOf(groupConcerned) + " < " + (rankingI.Count-extraRelegations));
                        if (rankingI.IndexOf(groupConcerned) < rankingI.Count-extraRelegations)
                        {
                            Console.WriteLine("[" + Tournament.name + "][" + _groupsNames[group] + "][" + administrativeDivision.name + "] Supprimme une relegation en trop au rang " + (ranking+1));
                            UpdateQualificationTournament(adjustedQualifications, ranking+1, Tournament);
                        }
                    }
                }
            }

            int admRelegables = 0;
            /*
            List<Tournament> pivotTournaments = new List<Tournament>();
            pivotTournaments.Add(country.GetLastNationalLeague());
            for (int i = 1; i < _administrativeLevel; i++)
            {
                pivotTournaments.Add(country.GetLastRegionalLeague(i));
            }
            
            //TODO: Work only with GroupRound actually
            Tournament lastNationalLeagueTournament = country.GetLastNationalLeague();
            GroupsRound lnlRound = lastNationalLeagueTournament.rounds[0] as GroupsRound;
            for (int i = 0; i < lnlRound._groupsNumber; i++)
            {
                List<Club> lnlRanking = lnlRound.Ranking(i);
                List<Qualification> lnlQualification = lnlRound.GetGroupQualifications(i);
                foreach (Qualification q in lnlQualification)
                {
                    //TODO : Manage q.qualifies != 0
                    if (q.isNextYear && q.tournament.isChampionship &&
                        q.tournament.level > lastNationalLeagueTournament.level && country.GetAdministrativeDivisionLevel(lnlRanking[q.ranking-1].AdministrativeDivision(), _administrativeLevel) == administrativeDivision)
                    {
                        admRelegables++;
                    }
                }
            }*/
            
            //Two cases
            //Case 1 : league at top is national or upper adm level
            // -> count team of your adm that are relegated
            //Case 2 : league at top is the same adm level
            // -> count extra relegations
            // A chaque fois, counter pour tous les groupes du bon adm
            if (upperGroupRound != null)
            {
                //Case 1
                if (upperGroupRound.administrativeLevel < administrativeLevel)
                {
                    AdministrativeDivision admAtLevel = upperGroupRound._administrativeLevel > 0 ? country.GetAdministrativeDivisionLevel(administrativeDivision, upperGroupRound._administrativeLevel) : null;
                    for (int i = 0; i < upperGroupRound._groupsNumber; i++)
                    {
                        List<Qualification> groupQualifications = upperGroupRound.GetGroupQualifications(i);
                        List<Club> groupRanking = upperGroupRound.Ranking(i);
                        foreach (Qualification q in groupQualifications)
                        {
                            if (q.roundId == 0 && q.isNextYear && q.tournament == Tournament)
                            {
                                if (country.GetAdministrativeDivisionLevel(groupRanking[q.ranking - 1].AdministrativeDivision(), _administrativeLevel) == administrativeDivision)
                                {
                                    admRelegables++;
                                }
                            }
                        }
                    }
                }
                //Case 2
                else
                {
                    List<Qualification> regularQualifications = upperGroupRound.qualifications;
                    int regularQualificationsCount = 0;
                    foreach (Qualification q in regularQualifications)
                    {
                        if (q.tournament == Tournament && q.isNextYear && q.roundId == 0)
                        {
                            regularQualificationsCount++;
                        }
                    }

                    int totalRelegations = 0;
                    for (int i = 0; i < upperGroupRound._groupsNumber; i++)
                    {
                        if (upperGroupRound.GetGroupAdministrativeDivision(i, _administrativeLevel) ==
                            administrativeDivision)
                        {
                            List<Qualification> groupQualifications = upperGroupRound.GetGroupQualifications(i);
                            foreach (Qualification q in groupQualifications)
                            {
                                if (q.isNextYear && q.roundId == 0 && q.tournament == Tournament)
                                {
                                    totalRelegations++;
                                }
                            }
                        }
                    }

                    admRelegables = totalRelegations - regularQualificationsCount;
                }
            }

            //TODO : Possible d'avoir a gérer ce cas un jour, où il y a moins de relegué que prévu (promu extra ...)
            Console.WriteLine("[" + Tournament.name + "][" + _groupsNames[group] + "][" + administrativeDivision.name + "] " + admRelegables + " relegations en plus que prévues");
            if (admRelegables < 0)
            {
                int savedByGroup = admRelegables / admGroupCount;
                int savedRelegablesExtra = admRelegables % admGroupCount;
                int firstPosition = -1;
                foreach (Qualification q in adjustedQualifications)
                {
                    if (q.isNextYear && q.tournament.isChampionship && q.tournament.level > Tournament.level && firstPosition == -1)
                    {
                        firstPosition = q.ranking;
                    }
                }

                if (firstPosition != -1)
                {
                    for (int i = 0; i < savedByGroup; i++)
                    {
                        Console.WriteLine("[" + Tournament.name + "][" + _groupsNames[group] + "][" + administrativeDivision.name + "] Une équipe sauvée rang " + firstPosition);
                        UpdateQualificationTournament(adjustedQualifications, firstPosition, Tournament);
                        firstPosition++;
                    }

                    List<Club> groupRanking = Ranking(group);
                    if (savedRelegablesExtra > 0)
                    {
                        int negativePosition = firstPosition-_groups[group].Count-1;
                        List<Club> clubsRanking = new List<Club>();
                        for (int i = 0; i < groupsCount; i++)
                        {
                            clubsRanking.Add(Ranking(i)[_groups[i].Count+negativePosition]);
                        }
                        clubsRanking.Sort(new ClubRankingComparator(_matches));
                        if (clubsRanking.IndexOf(groupRanking[firstPosition]) < firstPosition)
                        {
                            UpdateQualificationTournament(adjustedQualifications, firstPosition, Tournament);
                        }
                    }

                    
                }
            }
            if (admRelegables > 0)
            {
                int extraRelegablesByGroup = admRelegables / admGroupCount;
                int extraRelegablesExtra = admRelegables % admGroupCount;
                int firstPosition = -1;
                Tournament bottomTournament = null;
                foreach (Qualification q in adjustedQualifications)
                {
                    if (q.isNextYear && q.tournament.isChampionship && q.tournament.level > Tournament.level && firstPosition == -1)
                    {
                        firstPosition = q.ranking;
                        bottomTournament = q.tournament;
                    }
                }
                
                if (firstPosition != -1)
                {
                    //Incrementally increase relegation places on the ranking
                    for (int i = 0; i < extraRelegablesByGroup; i++)
                    {
                        firstPosition--;
                        //TODO : Maybe add a check if the ranking is neutral
                        UpdateQualificationTournament(adjustedQualifications, firstPosition, bottomTournament);
                        Console.WriteLine("[" + Tournament.name + "][" + _groupsNames[group] + "][" + administrativeDivision.name + "] Une équipe releguée rang " + firstPosition);

                    }

                    List<Club> groupRanking = Ranking(group);
                    if (extraRelegablesExtra > 0)
                    {
                        firstPosition--;
                        int negativePosition = firstPosition-_groups[group].Count-1;
                        List<Club> clubsRanking = new List<Club>();
                        for (int i = 0; i < groupsCount; i++)
                        {
                            clubsRanking.Add(Ranking(i)[_groups[i].Count+negativePosition]);
                        }
                        clubsRanking.Sort(new ClubRankingComparator(_matches, RankingType.General, true));
                        if (clubsRanking.IndexOf(groupRanking[firstPosition]) < extraRelegablesExtra)
                        {
                            //Search the good qualification and set the bottom tournament
                            UpdateQualificationTournament(adjustedQualifications, firstPosition, bottomTournament);
                            Console.WriteLine("[" + Tournament.name + "][" + _groupsNames[group] + "][" + administrativeDivision.name + "] Une équipe supplémentaire dans ce groupe releguée rang " + firstPosition);
                        }
                    }
                    
                }
            }

            if (extraPromotionOnGroup)
            {
                bool ok = false;
                for (int j = 0; j < adjustedQualifications.Count && !ok; j++)
                {
                    if (adjustedQualifications[j].tournament.level > Tournament.level)
                    {
                        UpdateQualificationTournament(adjustedQualifications, adjustedQualifications[j].ranking, Tournament);
                        Console.WriteLine("[" + Tournament.name + "][" + _groupsNames[group] + "][" + administrativeDivision.name + "] Extra promotion, donc l'équipe " + adjustedQualifications[j].ranking + " ne sera pas releguée");
                        ok = true;
                    }
                }
            }
            
            
            //Last check : if in the bottom division there is no club of you're administrative division, remove relegations
            GroupsRound bottomGroupRound = country.League(Tournament.level + 1)?.rounds[0] as GroupsRound;
            if (bottomGroupRound != null)
            {
                bool ok = false;
                for (int i = 0; i < bottomGroupRound.groupsCount; i++)
                {
                    //if (bottomGroupRound.GetGroupAdministrativeDivision(i, _administrativeLevel) == administrativeDivision)
                    if (administrativeDivision.ContainsAdministrativeDivision(bottomGroupRound.GetGroupAdministrativeDivision(i)))
                    {
                        ok = true;
                    }
                }
                
                if (!ok)
                {
                    for (int i = 0; i < adjustedQualifications.Count; i++)
                    {
                        Qualification q = adjustedQualifications[i];
                        if (q.isNextYear && q.tournament.level > Tournament.level && q.roundId == 0)
                        {
                            adjustedQualifications[i] = new Qualification(q.ranking, q.roundId, Tournament,
                                q.isNextYear, q.qualifies);
                        }
                    }
                }
            }

            return adjustedQualifications;
        }

        public List<Qualification> GetGroupQualifications(int group)
        {
            List<Qualification> allQualifications = new List<Qualification>(qualifications);
            int countChampionshipQualifications = 0;
            foreach(Qualification qualification in qualifications)
            {
                if(qualification.tournament.isChampionship)
                {
                    countChampionshipQualifications++;
                }
            }
            int minTeamsByGroup = clubs.Count / groupsCount;
            int groupsWithExtraTeam = clubs.Count % groupsCount;

            //It's a group with extra team, qualifications needs to be adapted
            if (group < groupsWithExtraTeam /*groups[group].Count == minTeamsByGroup + 1*/ && countChampionshipQualifications == minTeamsByGroup)
            {
                allQualifications.Sort(new QualificationComparator());
                int firstRankingToBottom = -1;
                foreach(Qualification q in allQualifications)
                {
                    if(q.tournament.level > Tournament.level && firstRankingToBottom == -1)
                    {
                        firstRankingToBottom = q.ranking;
                    }
                }
                if(firstRankingToBottom > -1)
                {
                    for (int i = firstRankingToBottom - 1; i < allQualifications.Count; i++)
                    {
                        allQualifications[i] = new Qualification(allQualifications[i].ranking + 1, allQualifications[i].roundId, allQualifications[i].tournament, allQualifications[i].isNextYear, allQualifications[i].qualifies);
                    }
                    allQualifications.Add(new Qualification(firstRankingToBottom, allQualifications[firstRankingToBottom - 2].roundId, allQualifications[firstRankingToBottom - 2].tournament, allQualifications[firstRankingToBottom - 2].isNextYear, allQualifications[firstRankingToBottom - 2].qualifies));
                }
                else
                {
                    Qualification lastQualification = allQualifications[allQualifications.Count - 1];
                    allQualifications.Add(new Qualification(lastQualification.ranking+1, lastQualification.roundId, lastQualification.tournament, lastQualification.isNextYear, lastQualification.qualifies));
                }
            }
            
            
            //Adapt qualifications to adapt negative ranking to real ranking in the group
            int totalClubs = _groups[group].Count > 0 ? _groups[group].Count : _clubs.Count/_groupsNumber; //Get theoretical clubs by group if groups were not drawn
            allQualifications = AdaptQualificationsToRanking(allQualifications, totalClubs);
            if (_randomDrawingMethod == RandomDrawingMethod.Administrative && _groups[group].Count > 0)
            {
                allQualifications = AdjustQualificationAdministrativeDivision(allQualifications, group);
            }
            
            return allQualifications;

        }

        public override void QualifyClubs()
        {
            int maxClubsInGroup = _clubs.Count % _groupsNumber > 0 ? (_clubs.Count / _groupsNumber) + 1 : _clubs.Count / _groupsNumber; //Theorical formula
            if (_referenceClubsByGroup != 0)
            {
                maxClubsInGroup = _referenceClubsByGroup;
            }
            for (int i = 0; i < this.groups.Length; i++)
            {
                maxClubsInGroup = this.groups[i].Count > maxClubsInGroup ? this.groups[i].Count : maxClubsInGroup;
            }
            List<Club>[] clubsByRanking = new List<Club>[maxClubsInGroup];
            List<Club>[] clubsByRankingDescending = new List<Club>[maxClubsInGroup];
            for(int i = 0; i<maxClubsInGroup; i++)
            {
                clubsByRanking[i] = new List<Club>();
                clubsByRankingDescending[i] = new List<Club>();
            }

            List<Club>[] groups = new List<Club>[_groupsNumber];
            for (int i = 0; i < _groupsNumber; i++)
            {
                groups[i] = new List<Club>(Ranking(i));

                for (int j = 0; j < groups[i].Count; j++)
                {
                    clubsByRanking[j].Add(groups[i][j]);
                    clubsByRankingDescending[j].Add(groups[i][groups[i].Count-1-j]);
                }
            }

            for (int i = 0; i < maxClubsInGroup; i++)
            {
                clubsByRanking[i].Sort(new ClubRankingComparator(_matches));
                clubsByRankingDescending[i].Sort(new ClubRankingComparator(_matches));
            }

            for (int i = 0; i < _groupsNumber; i++)
            {
                List<Qualification> qualifications = GetGroupQualifications(i);// new List<Qualification>(_qualifications);
                qualifications.Sort(new QualificationComparator());

                if (_rules.Contains(Rule.ReservesAreNotPromoted))
                {
                    qualifications = Utils.AdjustQualificationsToNotPromoteReserves(qualifications, groups[i], Tournament);
                }

                foreach (Qualification q in qualifications)
                {
                    Club c = groups[i][q.ranking - 1];

                    // If this qualification has a "negative ranking" (get team from bottom of ranking), get the base qualification to know if there is condition on team (n best team, n worst team eg)
                    Qualification baseNegativeQualification = new Qualification();
                    bool baseNegativeQualificationExist = false;
                    foreach (Qualification qu in _qualifications)
                    {
                        if (qu.ranking < 0 && q.ranking == groups[i].Count + qu.ranking + 1)
                        {
                            baseNegativeQualification = qu;
                            baseNegativeQualificationExist = true;
                        }
                    }

                    //Move club according to 3 cases
                    //q.qualifies == 0 : all clubs
                    //q.qualifies > 0 : from best nth clubs
                    //q.qualifies < 0 : from worst nth clubs
                    bool caseQualifieMoreThan0 = (q.qualifies > 0 && clubsByRanking[q.ranking - 1].IndexOf(c) < q.qualifies);
                    bool caseQualifieLessThan0 = (q.qualifies < 0 && clubsByRanking[q.ranking - 1].IndexOf(c) >= (clubsByRanking.Length + q.qualifies));
                    if (baseNegativeQualificationExist)
                    {
                        caseQualifieMoreThan0 = (q.qualifies > 0 && clubsByRankingDescending[Math.Abs(baseNegativeQualification.ranking)-1].IndexOf(c) < q.qualifies);
                        caseQualifieLessThan0 = (q.qualifies < 0 && clubsByRankingDescending[Math.Abs(baseNegativeQualification.ranking)-1].IndexOf(c) >= (groups.Length + q.qualifies));
                    }

                    if (Tournament.level == 4)
                    {
                        Console.WriteLine("[" + i + "], " + c.name + " - " + q.ranking + " -> " + q.tournament.name + " [" + q.qualifies + ", " + caseQualifieMoreThan0 + ", " + caseQualifieLessThan0 + "]");
                    }
                    if(q.qualifies == 0 || caseQualifieMoreThan0 || caseQualifieLessThan0)
                    {
                        if (!q.isNextYear)
                        {
                            q.tournament.rounds[q.roundId].clubs.Add(c);
                        }
                        else
                        {
                            q.tournament.AddClubForNextYear(c, q.roundId);
                        }
                        if (q.tournament.isChampionship && c.Championship != null)
                        {
                            if (q.tournament.level > c.Championship.level)
                            {
                                c.supporters = (int)(c.supporters / 1.8f);
                            }
                            else if (q.tournament.level < c.Championship.level)
                            {
                                c.supporters = (int)(c.supporters * 1.8f);
                            }
                        }
                    }

                }
            }
        }

        public override  List<Match> NextMatchesDay()
        {
            List<Match> res = new List<Match>();
            return NextMatches();

        }

        public override List<Match> GamesDay(int journey)
        {
            List<Match> res = new List<Match>();
            if(_matches.Count > 0)
            {
                int matchesPerGroups = GroupMatchesPerGamesDay() * ((_clubs.Count / _groupsNumber) - 1);
                if (twoLegs)
                {
                    matchesPerGroups *= 2;
                }
                for (int i = 0; i < _groupsNumber; i++)
                {
                    int baseIndex = (matchesPerGroups * i) + (GroupMatchesPerGamesDay() * (journey - 1));
                    for (int j = 0; j < GroupMatchesPerGamesDay(); j++)
                    {
                        res.Add(_matches[j + baseIndex]);
                    }

                }
            }

            return res;
        }

        public override int MatchesDayNumber()
        {
            return _matches.Count / _groupsNumber / GroupMatchesPerGamesDay();
        }

        public int GroupMatchesPerGamesDay()
        {
            return (_clubs.Count / _groupsNumber) / 2;
        }

        public List<Club> Ranking(int group, bool inverse=false)
        {
            List<Club> res = new List<Club>(_groups[group]);
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches, RankingType.General, inverse);
            res.Sort(comparator);
            return res;
        }

        public List<Club> RankingWithoutReserves(int ranking)
        {
            List<Club> res = new List<Club>();
            foreach(Club c in _groups[ranking])
            {
                if ((c as ReserveClub) == null)
                {
                    res.Add(c);
                }
            }
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches);
            res.Sort(comparator);
            return res;
        }

        private void SetGroups()
        {
            IRandomDrawing randomDrawing;
            switch (_randomDrawingMethod)
            {
                case RandomDrawingMethod.Coefficient:
                    randomDrawing = new RandomDrawingLevel(this, _clubs[0] as NationalTeam == null ? ClubAttribute.CONTINENTAL_COEFFICIENT : ClubAttribute.LEVEL);
                    break;
                case RandomDrawingMethod.Geographic:
                    randomDrawing = new RandomDrawingGeographic(this);
                    break;
                case RandomDrawingMethod.Administrative:
                    randomDrawing = new RandomDrawingAdministrative(this);
                    break;
                case RandomDrawingMethod.Level : default:
                    randomDrawing = new RandomDrawingLevel(this, ClubAttribute.LEVEL);
                    break;
            }
            randomDrawing.RandomDrawing();

            if (_referenceClubsByGroup == 0)
            {
                _referenceClubsByGroup = (_clubs.Count / groupsCount); // + 1;
            }
        }

        public override void DistributeGrants()
        {
            foreach(Prize d in _prizes)
            {
                for(int i = 0;i<groupsCount; i++)
                {
                    CityClub cv = Ranking(i)[d.Ranking - 1] as CityClub;
                    if (cv != null)
                    {
                        cv.ModifyBudget(d.Amount, BudgetModificationReason.TournamentGrant);
                    }
                }

            }
        }

        /// <summary>
        /// No sense, there is no competition finishing with groups round
        /// </summary>
        /// <returns></returns>
        public override Club Winner()
        {
            return null;
        }
    }
}