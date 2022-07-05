using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
        private List<GeographicPosition> _groupsLocalisation;
        [DataMember]
        private List<string> _groupsNames;
        

        public List<Club>[] groups { get => _groups; }

        public int groupsCount
        {
            get => _groupsNumber; set => _groupsNumber = value;
        }

        public int maxClubsInGroup => _clubs.Count % _groupsNumber > 0 ? (_clubs.Count / _groupsNumber) + 1 : _clubs.Count / _groupsNumber;

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

        public GroupsRound(string name, Hour hour, List<GameDay> dates, List<TvOffset> offsets, int groupsCount, bool twoLegs, GameDay initialisation, GameDay end, RandomDrawingMethod randomDrawingMethod) : base(name, hour, dates, offsets, initialisation,end, twoLegs,0)
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
        }

        public override Round Copy()
        {
            GroupsRound t = new GroupsRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), groupsCount, twoLegs, programmation.initialisation, programmation.end, _randomDrawingMethod);
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
                if (_groups[i].Count > 0 && _groups[i][0].AdministrativeDivision() == administrativeDivision)
                {
                    res.Add(i);
                }
            }

            return res;
        }
        
        public List<Qualification> AdjustQualificationAdministrativeDivision(List<Qualification> baseQualifications, int group)
        {
            List<Qualification> adjustedQualifications = new List<Qualification>(baseQualifications);
            Country country = _groups[group][0].Country();
            AdministrativeDivision administrativeDivision = _groups[group][0].AdministrativeDivision();
            int admGroupCount = GetGroupsFromAdministrativeDivision(administrativeDivision).Count;
            Tournament lastNationalLeagueTournament = country.GetLastNationalLeague();
            //TODO: Work only with GroupRound actually
            GroupsRound lnlRound = lastNationalLeagueTournament.rounds[0] as GroupsRound;
            int admRelegables = 0;
            for (int i = 0; i < lnlRound._groupsNumber; i++)
            {
                List<Club> lnlRanking = lnlRound.Ranking(i);
                List<Qualification> lnlQualification = lnlRound.GetGroupQualifications(i);
                foreach (Qualification q in lnlQualification)
                {
                    //TODO : Manage q.qualifies != 0
                    if (q.isNextYear && q.tournament.isChampionship &&
                        q.tournament.level > lastNationalLeagueTournament.level && lnlRanking[q.ranking-1].AdministrativeDivision() == administrativeDivision)
                    {
                        admRelegables++;
                    }
                }
            }

            if (admRelegables > 0)
            {
                int extraRelegablesByGroup = admRelegables / admGroupCount;
                int extraRelegablesExtra = admRelegables % admGroupCount;
                adjustedQualifications.Sort(new QualificationComparator());
                int firstPosition = -1;
                Tournament bottomTournament = null;
                foreach (Qualification q in adjustedQualifications)
                {
                    if (q.isNextYear && q.tournament.isChampionship && q.tournament.level > Tournament.level &&
                        firstPosition == -1)
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
                        for (int j = 0; j < adjustedQualifications.Count; j++)
                        {
                            Qualification q = adjustedQualifications[j];
                            if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == firstPosition)
                            {
                                q.tournament = bottomTournament;
                                adjustedQualifications[j] = new Qualification(q.ranking, q.roundId, bottomTournament,
                                    q.isNextYear, q.qualifies);
                            }
                        }
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
                            for (int j = 0; j < adjustedQualifications.Count; j++)
                            {
                                Qualification q = adjustedQualifications[j];
                                if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == firstPosition)
                                {
                                    q.tournament = bottomTournament;
                                }
                            }
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

        public List<Club> Ranking(int group)
        {
            List<Club> res = new List<Club>(_groups[group]);
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches);
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