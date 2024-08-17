using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Animation;
using tm.Comparators;
using tm.Tournaments;

namespace tm
{
    [DataContract(IsReference =true)]
    [KnownType(typeof(GroupActiveRound))]
    [System.Xml.Serialization.XmlInclude(typeof(GroupActiveRound))]
    [KnownType(typeof(GroupInactiveRound))]
    [System.Xml.Serialization.XmlInclude(typeof(GroupInactiveRound))]

    public abstract class GroupsRound : Round
    {
        [DataMember]
        protected int _groupsNumber;
        [DataMember]
        protected List<Club>[] _groups;

        [DataMember]
        protected RandomDrawingMethod _randomDrawingMethod;
        /// <summary>
        /// Administrative level of the round.
        /// 0 : National round
        /// 1 : Regional round
        /// 2 : Departemental/District round
        /// </summary>
        [DataMember]
        protected int _administrativeLevel;
        [DataMember]
        protected List<GeographicPosition> _groupsLocalisation;
        [DataMember]
        protected List<string> _groupsNames;
        [DataMember]
        protected int _referenceClubsByGroup;
        [DataMember]
        protected int _nonGroupGamesByTeams;
        [DataMember]
        protected int _nonGroupGamesByGameday;
        [DataMember]
        protected bool _fusionGroupAndNoGroupGames;

        [DataMember]
        protected Dictionary<Association, int> _relegationsByAssociations;
        /// <summary>
        /// For computation time optimization, save computed group qualifications
        /// </summary>
        [DataMember]
        protected List<Qualification>[] _storedGroupQualifications = null;

        /// <summary>
        /// Contains ranking cache
        /// Currently cleared every day. Should be cleared when a game is finished
        /// </summary>
        protected List<Club>[] _cacheRanking;

        /// <summary>
        /// Used to share information between qualifications computation phases
        /// </summary>
        protected int __computationRelegationPlaces;

        public int referenceClubsByGroup => _referenceClubsByGroup;


        public List<Club>[] groups { get => _groups; }

        public int groupsCount
        {
            get => _groupsNumber; set => _groupsNumber = value;
        }

        public int maxClubsInGroup => _clubs.Count % _groupsNumber > 0 ? (_clubs.Count / _groupsNumber) + 1 : _clubs.Count / _groupsNumber;

        public int administrativeLevel => _administrativeLevel;

        public int nonGroupGamesByTeams => _nonGroupGamesByTeams;
        public int nonGroupGamesByGameday => _nonGroupGamesByGameday;
        public bool fusionGroupAndNoGroupGames => _fusionGroupAndNoGroupGames;

        public List<GeographicPosition> groupsLocalisation { get => _groupsLocalisation; }

        public RandomDrawingMethod RandomDrawingMethod => _randomDrawingMethod;
        
        public Dictionary<Association, int> relegationsByAssociations => _relegationsByAssociations;

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

        public void InitStoredGroupQualifications()
        {
            _storedGroupQualifications = new List<Qualification>[_groups.Length];
        }

        public GroupsRound() : base()
        {
            _groups = new List<Club>[0];
            _groupsLocalisation = new List<GeographicPosition>();
            _relegationsByAssociations = new Dictionary<Association, int>();
            _cacheRanking = new List<Club>[0];
        }

        public GroupsRound(int id, string name, Tournament tournament, Hour hour, List<GameDay> dates, List<TvOffset> offsets, int groupsCount, int phases, GameDay initialisation, GameDay end, int keepRankingFromPreviousRound, RandomDrawingMethod randomDrawingMethod, int administrativeLevel, bool fusionGroupAndNoGroupGames, int nonGroupGamesByTeams, int nonGroupGamesByGameday, int gamesPriority, int lastDaysSameDay) : base(id, name, tournament, hour, dates, offsets, initialisation,end, phases, lastDaysSameDay, keepRankingFromPreviousRound, gamesPriority)
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
            _relegationsByAssociations = new Dictionary<Association, int>();
            _nonGroupGamesByTeams = nonGroupGamesByTeams;
            _nonGroupGamesByGameday = nonGroupGamesByGameday;
            _fusionGroupAndNoGroupGames = fusionGroupAndNoGroupGames;
            _cacheRanking = new List<Club>[0];
            __computationRelegationPlaces = 0;
        }

        public void ClearCache()
        {
            ClearRankingCache();
            InitStoredGroupQualifications();
        }

        public void ClearRankingCache()
        {
            _cacheRanking = new List<Club>[_groups.Length];
            for (int i = 0; i < _groups.Length; i++)
            {
                _cacheRanking[i] = new List<Club>();
            }
        }

        protected abstract GroupsRound Clone();

        public override Round Copy()
        {
            GroupsRound t = Clone();
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
            foreach(KeyValuePair<Association, int> kvp in relegationsByAssociations)
            {
                t.relegationsByAssociations.Add(kvp.Key, kvp.Value);
            }
            foreach (KeyValuePair<Club, List<PointDeduction>> sanctions in this.pointsDeduction)
            {
                t.pointsDeduction.Add(sanctions.Key, sanctions.Value);
            }
            foreach (Qualification q in this.qualifications)
            {
                t.qualifications.Add(q);
            }
            t.rules.AddRange(rules);
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

        private Association GetGroupAssociation(int group, int admLevel = -1)
        {
            Association res = null;
            if (_groups[group].Count > 0)
            {
                res = _groups[group][0].Association();
                if (admLevel != -1)
                {
                    res = _groups[group][0].Country().GetAssociationLevel(res, admLevel);
                }
            }
            return res;
        }

        public List<Club> GetAssociationRelegablesCandidates(Association association)
        {
            List<Club> candidates = new List<Club>();
            List<int> admGroups = GetGroupsFromAssociation(association);
            int maxTeamByGroup = 0;
            foreach (int group in admGroups)
            {
                maxTeamByGroup = groups[group].Count > maxTeamByGroup ? groups[group].Count : maxTeamByGroup;
            }

            for (int i = -1; i > -maxTeamByGroup-1; i--)
            {
                List<Club> rankingI = RankingByRank(i, association);
                rankingI.Reverse();
                foreach(Club c in rankingI)
                {
                    //Pour chaque club, si déjà pas concerné par une relegation, on ajoute
                    candidates.Add(c);
                }
            }
            return candidates;
        }

        protected abstract List<Club> RankClubs(List<Club> clubs, List<Tiebreaker> tiebreakers, Dictionary<Club, List<PointDeduction>> pointsDeduction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rank">Rank can be negative</param>
        /// <param name="association">If association is None, get Ranking for teams from all groups, else get only teams for the specified administrative division</param>
        /// <returns></returns>
        public List<Club> RankingByRank(int rank, Association association)
        {
            List<Club> ranking = new List<Club>();
            for (int i = 0; i < _groupsNumber; i++)
            {
                Association adm = GetGroupAssociation(i);
                if ((association != null && association.ContainsAssociation(adm)) || association == null)
                {
                    List<Club> rankingGroup = Ranking(i);
                    int realRanking = rank > 0 ? rank - 1 : rankingGroup.Count + rank;
                    if (realRanking < rankingGroup.Count && realRanking >= 0)
                    {
                        ranking.Add(rankingGroup[realRanking]);
                    }
                }
            }
            ranking = RankClubs(ranking, _tiebreakers, pointsDeduction);
            //ranking.Sort(new ClubRankingComparator(_matches, _tiebreakers, pointsDeduction));
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

            SpecialInitialize();
            CheckConflicts();
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

        public int GetTeamsCountSentToHigherRegionalLevel(Association association, Country c)
        {
            int res = 0;
            if(this.RandomDrawingMethod == RandomDrawingMethod.Administrative)
            {
                Tournament higherRegionalTournament = c.GetHigherRegionalTournament(_administrativeLevel);
                GroupsRound gr = higherRegionalTournament.rounds[0] as GroupsRound;
                if(gr != null)
                {
                    List<int> groups = gr.GetGroupsFromAssociation(association);
                    foreach(int group in groups)
                    {
                        List<Qualification> qualifications = gr.GetGroupQualifications(group);
                        foreach(Qualification q in qualifications)
                        {
                            if(q.tournament.level < higherRegionalTournament.level)
                            {
                                res++;
                            }
                        }
                    }
                }
            }
            return res;
        }

        public List<int> GetGroupsFromAssociation(Association association)
        {
            List<int> res = new List<int>();
            for (int i = 0; i < _groupsNumber; i++)
            {
                if (_groups[i].Count > 0)
                {
                    Association admAtLeagueLevel = _groups[i][0].Country().GetAssociationLevel(_groups[i][0].Association(), _administrativeLevel);
                    if (admAtLeagueLevel == association)
                    {
                        res.Add(i);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Check if the team of a group of a particular rank is on top or on bottom of teams ranked at the same place in other groups according to a "top/bottom line" defined as a parameter
        /// </summary>
        /// <param name="ranking">Group ranking</param>
        /// <param name="rank">Rank of interest</param>
        /// <param name="line">Limit between top and bottom teams</param>
        /// <param name="association">If limit rankings to one association</param>
        /// <returns>True if the concerned team of the group is over the line, else otherwise</returns>
        public bool TeamIsTopRBottom(List<Club> ranking, int rank, int topBottomSeparation, Association association)
        {
            List<Club> rankingNth = RankingByRank(rank, association);
            int indexRank = rank > 0 ? rank - 1 : ranking.Count + rank;
            int groupClubRanking = rankingNth.IndexOf(ranking[indexRank]);
            Console.WriteLine("[TeamIsTopRBottom] " + groupClubRanking + " < " + topBottomSeparation);
            return groupClubRanking < topBottomSeparation;

        }

        /// <summary>
        /// Called if national group round.
        /// Decide slot status for this group where slot status (saved/relegated) depend of results of there teams at the same position (worst 11nth are relegated, ...)
        /// </summary>
        /// <param name="baseQualifications">Qualifications</param>
        /// <param name="group">Concerned group</param>
        /// <param name="from">Round' tournament</param>
        /// <returns></returns>
        public List<Qualification> AdjustQualificationsGroup(List<Qualification> baseQualifications, int group, Tournament from)
        {
            List<Club> ranking = Ranking(group);
            int maxRanking = baseQualifications.Max(x => x.ranking);
            List<Qualification> qualifications = new List<Qualification>();
            for(int i = 0; i < baseQualifications.Count; i++)
            {
                if (baseQualifications[i].qualifies > 0)
                {
                    //Get mirror qualification where below teams are qualified. If not find, bottomQualification.tournament == null
                    Qualification bottomQualification = baseQualifications.Where(x => x.ranking == baseQualifications[i].ranking && x.qualifies < 0).Select(x => x).FirstOrDefault();
                    bool concernRelegation = bottomQualification.tournament != null ? baseQualifications[i].tournament.level == from.level : false;
                    int rank = concernRelegation ? baseQualifications[i].ranking - maxRanking - 1 : baseQualifications[i].ranking;
                    if (TeamIsTopRBottom(ranking, rank, baseQualifications[i].qualifies, null))
                    {
                        qualifications.Add(new Qualification(baseQualifications[i].ranking, baseQualifications[i].roundId, baseQualifications[i].tournament, baseQualifications[i].isNextYear, 0));
                    }
                    else if(bottomQualification.tournament != null)
                    {
                        qualifications.Add(new Qualification(bottomQualification.ranking, bottomQualification.roundId, bottomQualification.tournament, bottomQualification.isNextYear, 0));
                    }
                }
                else if (baseQualifications[i].qualifies == 0)
                {
                    qualifications.Add(baseQualifications[i]);
                }
            }
            return qualifications;
        }

        /// <summary>
        /// Adjust qualifications according to region and district constraints
        /// </summary>
        /// <param name="baseQualifications">List of initial qualifications</param>
        /// <param name="group">Concerned group</param>
        /// <returns>New list of qualifications</returns>
        public List<Qualification> AdjustQualificationAssociation(List<Qualification> baseQualifications, int group)
        {
            Tournament tournament = Tournament;
            List<Qualification> adjustedQualifications = new List<Qualification>(baseQualifications);
            adjustedQualifications.Sort(new QualificationComparator());
            Country country = _groups[group][0].Country();
            //Get the district of the group (at the good regional level)
            Association association = country.GetAssociationLevel(_groups[group][0].Association(), _administrativeLevel);
            Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "]");
            bool isHigherRegionalLevel = country.GetHigherRegionalTournament(administrativeLevel) == tournament;
            //int teamsSentToHigherRegionalLevel = !isHigherRegionalLevel ? GetTeamsCountSentToHigherRegionalLevel(association, country) : 0;
            int teamsSentToHigherRegionalLevel = 0;
            int admGroupCount = GetGroupsFromAssociation(association).Count;
            int extraPromotions = 0;

            List<Club> admRelegationsCandidates = GetAssociationRelegablesCandidates(association);
            List<Club> groupRanking = Ranking(group);
            //Get tournament target of relegation places
            Tournament bottomTournament = null;
            foreach (Qualification q in adjustedQualifications)
            {
                if (q.isNextYear && q.tournament.isChampionship && q.tournament.level > tournament.level)
                {
                    bottomTournament = q.tournament;
                }
            }

            if (bottomTournament == null)
            {
                bottomTournament = tournament;
            }

            teamsSentToHigherRegionalLevel = 1;

            //If this league is this first district league, automatically compute promotion slots
            GroupsRound upperGroupRound = country.League(tournament.level - 1).rounds[0] as GroupsRound;
            bool extraPromotionOnGroup = false;
            int promotionSlots = 0;
            if (upperGroupRound != null && upperGroupRound.administrativeLevel > 0 && _administrativeLevel > upperGroupRound._administrativeLevel)
            {
                Association upperAssociation = country.GetAssociationLevel(association, upperGroupRound.administrativeLevel);
                int admGroupUpper = 0;
                for (int i = 0; i < _groupsNumber; i++)
                {
                    if (groups[i][0].Country().GetAssociationLevel(groups[i][0].Association(), upperGroupRound._administrativeLevel) == upperAssociation)
                    {
                        admGroupUpper++;
                    }
                }
                foreach (Qualification q in upperGroupRound.qualifications)
                {
                    if (q.isNextYear && q.roundId == 0 && q.tournament == tournament)
                    {
                        promotionSlots++;
                    }
                }
                //Si le nombre de relegations de la division du haut est défini par région, alors on sélectionne automatiquement ce nombre de relegués
                if(upperGroupRound.relegationsByAssociations.ContainsKey(upperAssociation))
                {
                    promotionSlots = upperGroupRound.relegationsByAssociations[upperAssociation];
                    Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Promotions slots calculés automatiquement : " + (promotionSlots+1));
                }

                promotionSlots = promotionSlots + 1; // 1; //TODO : +1 or +offset national/regional
                
                //Dispatche les places de promotion attribué au groupe si la région administrative possède plusieurs groupes dans la division
                int promotionForAdm = promotionSlots / admGroupUpper;
                teamsSentToHigherRegionalLevel = promotionForAdm;
                extraPromotions = promotionSlots % admGroupUpper;
                Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] " + admGroupUpper + " groupes administratifs : " + promotionForAdm + " promotions (avec " + extraPromotions + " extra slot)");
                for (int i = 1; i <= promotionForAdm; i++)
                {
                    Utils.UpdateQualificationTournament(adjustedQualifications, i, upperGroupRound.Tournament);
                }

                if (extraPromotions > 0)
                {
                    List<Club> rankingRank = RankingByRank(promotionForAdm + 1, upperAssociation);
                    Club concerned = groupRanking[promotionForAdm];
                    if (rankingRank.IndexOf(concerned) < extraPromotions)
                    {
                        Utils.UpdateQualificationTournament(adjustedQualifications, promotionForAdm + 1, upperGroupRound.Tournament);
                        extraPromotionOnGroup = true;
                        teamsSentToHigherRegionalLevel++;
                    }
                }
            }

            int relegationPlaces = 0;
            //TODO : Round with barrages are not managed, creating barrage not managed
            //Dispatche les places de promotion attribué au groupe si la région administrative possède plusieurs groupes dans la division
            if (admGroupCount > 1)
            {
                int promotionPlaces = -1;
                //Multiple groups by adm, split promotion/relegation on groups
                relegationPlaces = -1;
                foreach (Qualification q in adjustedQualifications)
                {
                    if (q.tournament == tournament && q.isNextYear && q.roundId == 0 && promotionPlaces == -1)
                    {
                        promotionPlaces = q.ranking-1;
                    }

                    if (q.tournament.level > tournament.level && q.isNextYear && q.roundId == 0 &&
                        relegationPlaces == -1)
                    {
                        relegationPlaces = adjustedQualifications.Count - q.ranking + 1;
                    }
                }
                Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Etape 0 : " + relegationPlaces + " places de rélégations");
                Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Etape 0 : " + promotionPlaces + " places de promotions");
                if (promotionPlaces > 0)
                {
                    int promotionByGroup = promotionPlaces / admGroupCount;
                    extraPromotions = promotionPlaces % admGroupCount;
                    int promotionOffset = promotionPlaces - promotionByGroup - (extraPromotions != 0 ? 1 : 0);
                    for (int i = promotionPlaces; i > promotionPlaces-promotionOffset; i--)
                    {
                        Utils.UpdateQualificationTournament(adjustedQualifications, i, tournament);
                    }

                    if (extraPromotions != 0)
                    {
                        int ranking = promotionPlaces - promotionOffset;
                        List<int> groupsAdm = GetGroupsFromAssociation(association);
                        Club groupConcerned = groupRanking[ranking];
                        List<Club> rankingI = new List<Club>();
                        foreach(int i in groupsAdm)
                        {
                            rankingI.Add(Ranking(i)[ranking]);
                        }

                        rankingI = RankClubs(rankingI, _tiebreakers, pointsDeduction);
                        //rankingI.Sort(new ClubRankingComparator(_matches, _tiebreakers, pointsDeduction));
                        if (rankingI.IndexOf(groupConcerned) >= extraPromotions)
                        {
                            Utils.UpdateQualificationTournament(adjustedQualifications, ranking, tournament);
                        }
                    }
                }

                //Ignore qualifications relegations but take predefined relegations number of administrative division
                if (relegationsByAssociations.ContainsKey(association))
                {
                    relegationPlaces = relegationsByAssociations[association];
                }
                Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Promotions : " + promotionPlaces + ", relegations : " + relegationPlaces);
                if (relegationPlaces > -1)
                {
                    int relegationsByGroup = relegationPlaces / admGroupCount;
                    int extraRelegations = relegationPlaces % admGroupCount;
                    int relegationOffset = relegationPlaces - relegationsByGroup - (extraRelegations != 0 ? 1 : 0);
                    for (int i = _groups[group].Count-relegationPlaces+1; i < _groups[group].Count-relegationPlaces+relegationOffset+1; i++)
                    {
                        Utils.UpdateQualificationTournament(adjustedQualifications, i, tournament);
                    }

                    if (extraRelegations != 0)
                    {
                        int ranking = _groups[group].Count - relegationsByGroup - 1;// + relegationOffset + 1;// - (relegationOffset == 0 ? 1 : 0);
                        int relativeRanking = ranking - _groups[group].Count;
                        //int ranking = _groups[group].Count - relegationPlaces + relegationOffset + 1;// - (relegationOffset == 0 ? 1 : 0);
                        //int relativeRanking = _groups[group].Count - ranking - 1;
                        Club groupConcerned = groupRanking[ranking];
                        List<Club> rankingI = RankingByRank(relativeRanking, association);
                        int cOff = rankingI.IndexOf(groupConcerned);
                        Console.WriteLine(rankingI.IndexOf(groupConcerned) + " < " + (rankingI.Count-extraRelegations));
                        if (rankingI.IndexOf(groupConcerned) < rankingI.Count-extraRelegations)
                        {
                            Utils.UpdateQualificationTournament(adjustedQualifications, ranking+1, tournament);
                        }
                    }
                }
            }
            else
            {
                //Sinon le nombre de relégations dans le groupe est le nombre de relégations défini dans la liste des qualifications
                foreach (Qualification q in adjustedQualifications)
                {
                    if (q.isNextYear && q.roundId == 0 && q.tournament.level > tournament.level)
                    {
                        relegationPlaces++;
                    }
                }
                //Ignore qualifications relegations but take predefined relegations number of administrative division if defined
                if (relegationsByAssociations.ContainsKey(association))
                {
                    for (int i = adjustedQualifications.Count; i > adjustedQualifications.Count-relegationPlaces; i--)
                    {
                        Utils.UpdateQualificationTournament(adjustedQualifications, i, tournament);
                    }
                    int fixedRelegations = relegationsByAssociations[association];
                    for (int i = adjustedQualifications.Count; i > adjustedQualifications.Count-fixedRelegations; i--)
                    {
                        Utils.UpdateQualificationTournament(adjustedQualifications, i, bottomTournament);
                    }
                    relegationPlaces = fixedRelegations;
                }
            }

            if (relegationPlaces == -1)
            {
                relegationPlaces = 0;
            }

            int promotionsCount = 0;
            foreach(Qualification q in adjustedQualifications)
            {
                if(q.tournament.level < tournament.level)
                {
                    promotionsCount++;
                }
            }
            /*if(isHigherRegionalLevel)
            {
                teamsSentToHigherRegionalLevel = promotionsCount;
            }*/
            //Système un peu brut. On admet qu'une région envoie une équipe au niveau régional au dessus. On gère donc seulement si on est dans le cas où on envoie plus d'une équipe au niveau régional au dessus.
            //On applique cette modification uniquement si on est à l'échelon régional le plus élevé car les échelons du dessous s'appuierons sur le nombre de relegués à ce niveau pour gérer leur propre nombre de relégué (donc pour éviter d'appliquer cette réduction du nombre de relegués plusieurs fois)
            if (teamsSentToHigherRegionalLevel != 1 && isHigherRegionalLevel)
            {
                relegationPlaces -= (teamsSentToHigherRegionalLevel - 1);
            }
            Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Etape 1 : " + relegationPlaces + " rel. places (higher level ? "+ isHigherRegionalLevel + " : teams sent to higher regional level : " + teamsSentToHigherRegionalLevel + ")");

            int admRelegables = 0;

            //Two cases to count extra relegations
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
                    Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Case 1");
                    //It's probably to create a function in GroupsRound that return relegables teams
                    //Et dangereux de faire comme ça car compatibilité entre relegations au milieu de classement (réserve qui tombe, rétrogradation adm) sont pas forcément compatible avec le classement des meilleurs nième
                    Association admAtLevel = upperGroupRound.administrativeLevel > 0 ? country.GetAssociationLevel(association, upperGroupRound._administrativeLevel) : null;
                    for (int i = 0; i < upperGroupRound._groupsNumber; i++)
                    {
                        List<Qualification> groupQualifications = upperGroupRound.GetGroupQualifications(i);
                        int upperGroupRoundClubsCount = upperGroupRound.groups[i].Count();
                        List<Club> upperGroupRanking = upperGroupRound.Ranking(i);
                        foreach (Qualification q in groupQualifications)
                        {
                            Club concernedClub = upperGroupRanking[q.ranking - 1];
                            if(country.GetAssociationLevel(concernedClub.Association(), _administrativeLevel) == association)
                            {
                                if (q.roundId == 0 && q.isNextYear && q.tournament == tournament && q.qualifies == 0)
                                {
                                    admRelegables++;
                                }
                                else if (q.qualifies != 0 && q.tournament == tournament)
                                {
                                    List<Club> rankingOfRank = upperGroupRound.RankingByRank(q.ranking - upperGroupRoundClubsCount - 1, null);
                                    int threshold = q.qualifies > 0 ? q.qualifies : q.qualifies + upperGroupRound._groupsNumber;
                                    if (rankingOfRank.IndexOf(concernedClub) >= threshold)
                                    {
                                        admRelegables++;
                                    }
                                }
                            }
                        }
                    }
                }
                //Case 2
                else
                {
                    Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Case 2");
                    List<Qualification> regularQualifications = upperGroupRound.qualifications;
                    int regularQualificationsCount = 0;
                    Association upperAssociation = country.GetAssociationLevel(association, upperGroupRound.administrativeLevel);
                    //Ignore qualifications relegations but take predefined relegations number of administrative divsiion
                    if (upperGroupRound.relegationsByAssociations.ContainsKey(upperAssociation))
                    {
                        regularQualificationsCount = upperGroupRound.relegationsByAssociations[upperAssociation];
                    }
                    else
                    {
                        foreach (Qualification q in regularQualifications)
                        {
                            if (q.tournament == tournament && q.isNextYear && q.roundId == 0)
                            {
                                regularQualificationsCount++;
                            }
                        }
                    }

                    int totalRelegations = 0;
                    for (int i = 0; i < upperGroupRound._groupsNumber; i++)
                    {
                        if (upperGroupRound.GetGroupAssociation(i, _administrativeLevel) ==
                            association)
                        {
                            List<Qualification> groupQualifications = upperGroupRound.GetGroupQualifications(i);
                            foreach (Qualification q in groupQualifications)
                            {
                                if (q.isNextYear && q.roundId == 0 && q.tournament == tournament)
                                {
                                    totalRelegations++;
                                }
                            }
                        }
                    }

                    admRelegables = totalRelegations - regularQualificationsCount;
                }
            }

            Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Relegations places " + relegationPlaces);
            relegationPlaces += admRelegables;
            __computationRelegationPlaces = relegationPlaces;
            //Dans le cas par exemple avec deux groupes et trois montées, un groupe aura une montée additionelle. On enlève alors une relégation à ce groupe pour maintenir le même nombre d'équipes dans le groupe
            /*if (extraPromotionOnGroup)
            {
                relegationPlaces--;
            }*/

            //Remove all relegation places
            for (int i = 0; i < adjustedQualifications.Count; i++)
            {
                if (adjustedQualifications[i].tournament.level > tournament.level)
                {
                    Utils.UpdateQualificationTournament(adjustedQualifications, adjustedQualifications[i].ranking, tournament);
                }
            }

            int relegationOnGroup = 0;
            for (int i = 0; i < relegationPlaces; i++)
            {
                Club c = admRelegationsCandidates[i];
                if (groupRanking.Contains(c))
                {
                    Utils.UpdateQualificationTournament(adjustedQualifications, groupRanking.IndexOf(c)+1, bottomTournament);
                    relegationOnGroup++;
                }
            }
            Console.WriteLine("[" + tournament.name + "][" + association.name + "][Groupe " + group + "] Promotions : " + promotionsCount + "; AdmRelegables : " + admRelegables + " -> total : " + relegationPlaces + "rélégations, " + relegationOnGroup + " pour ce groupe");

            //Last check : if in the bottom division there is no club of you're administrative division, remove relegations
            //Check if bottom round is a group round or an inactive round (could be factorized)
            bool ok = false;
            GroupsRound bottomGroupRound = country.League(tournament.level + 1)?.rounds[0] as GroupsRound;
            if (bottomGroupRound != null)
            {
                for (int i = 0; i < bottomGroupRound.groupsCount; i++)
                {
                    if (association.ContainsAssociation(bottomGroupRound.GetGroupAssociation(i)))
                    {
                        ok = true;
                    }
                }
            }
            if (!ok)
            {
                for (int i = 0; i < adjustedQualifications.Count; i++)
                {
                    Qualification q = adjustedQualifications[i];
                    if (q.isNextYear && q.tournament.level > tournament.level && q.roundId == 0)
                    {
                        adjustedQualifications[i] = new Qualification(q.ranking, q.roundId, tournament,
                            q.isNextYear, q.qualifies);
                    }
                }
            }

            return adjustedQualifications;
        }

        /// <summary>
        /// WARNING: Doesn't take in account relegation playoffs
        /// Get direct relegation slots
        /// </summary>
        /// <param name="association"></param>
        /// <returns></returns>
        private int GetRelegations(Association association)
        {
            Tournament tournament = Tournament;
            int res = 0;
            if(association == null)
            {
                foreach(Qualification q in qualifications)
                {
                    if(q.isNextYear && q.tournament.level > tournament.level && q.qualifies == 0)
                    {
                        res += groups.Length;
                    }
                    else if(q.isNextYear && q.tournament.level > tournament.level && q.qualifies < 0)
                    {
                        res += -q.qualifies;
                    }
                }
            }
            else
            {
                res = __computationRelegationPlaces;
            }
            return res;
        }

        public List<Qualification> GetGroupQualifications(int group)
        {
            if (_storedGroupQualifications == null)
            {
                _storedGroupQualifications = new List<Qualification>[_groups.Length];
            }
            if(_storedGroupQualifications[group] != null)
            {
                return _storedGroupQualifications[group];
            }
            else
            {
                ClearRankingCache();
                Tournament tournament = Tournament;
                List<Qualification> allQualifications = new List<Qualification>(qualifications);
                int countChampionshipQualifications = 0;
                foreach (Qualification qualification in qualifications)
                {
                    if (qualification.tournament.isChampionship)
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
                    foreach (Qualification q in allQualifications)
                    {
                        if (q.tournament.level > tournament.level && firstRankingToBottom == -1)
                        {
                            firstRankingToBottom = q.ranking;
                        }
                    }
                    if (firstRankingToBottom > -1)
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
                        allQualifications.Add(new Qualification(lastQualification.ranking + 1, lastQualification.roundId, lastQualification.tournament, lastQualification.isNextYear, lastQualification.qualifies));
                    }
                }


                //Adapt qualifications to adapt negative ranking to real ranking in the group
                int totalClubs = _groups[group].Count > 0 ? _groups[group].Count : _clubs.Count / _groupsNumber; //Get theoretical clubs by group if groups were not drawn
                allQualifications = AdaptQualificationsToRanking(allQualifications, totalClubs);
                if (_randomDrawingMethod == RandomDrawingMethod.Administrative && _groups[group].Count > 0)
                {
                    allQualifications = AdjustQualificationAssociation(allQualifications, group);
                }
                else if (_groups[group].Count > 0)
                {
                    allQualifications = AdjustQualificationsGroup(allQualifications, group, tournament);
                }

                if (groups[group].Count > 0)
                {
                    Country country = _groups[group][0].Country();
                    Association association = _randomDrawingMethod == RandomDrawingMethod.Administrative ? country.GetAssociationLevel(_groups[group][0].Association(), _administrativeLevel) : null;
                    int groupsCount = association != null ? GetGroupsFromAssociation(association).Count : groups.Length;
                    int totalRelegations = GetRelegations(association);
                    allQualifications = Utils.AdjustQualificationsToNotPromoteReserves(allQualifications, Ranking(group), association, tournament, this, _rules.Contains(Rule.ReservesAreNotPromoted), totalRelegations, groupsCount);
                }
                _storedGroupQualifications[group] = allQualifications;
                return allQualifications;
            }

        }

        public override void QualifyClubs(bool forNextYear)
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
                clubsByRanking[i] = RankClubs(clubsByRanking[i], _tiebreakers, pointsDeduction);
                clubsByRankingDescending[i] = RankClubs(clubsByRankingDescending[i], _tiebreakers, pointsDeduction);
                //clubsByRanking[i].Sort(new ClubRankingComparator(_matches, _tiebreakers, pointsDeduction));
                //clubsByRankingDescending[i].Sort(new ClubRankingComparator(_matches, _tiebreakers, pointsDeduction));
            }

            for (int i = 0; i < _groupsNumber; i++)
            {
                List<Qualification> qualifications = GetGroupQualifications(i);// new List<Qualification>(_qualifications);
                qualifications.Sort(new QualificationComparator());

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
                        if (!q.isNextYear && !forNextYear)
                        {
                            q.tournament.rounds[q.roundId].clubs.Add(c);
                        }
                        else if(q.isNextYear && forNextYear)
                        {
                            q.tournament.AddClubForNextYear(c, q.roundId);
                            if (Tournament.level == 5)
                            {
                                Console.WriteLine("[" + i + "], " + c.name + " - " + q.ranking + " -> " + q.tournament.name + " [" + q.qualifies + ", " + caseQualifieMoreThan0 + ", " + caseQualifieLessThan0 + "]");
                            }
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

        public override bool IsKnockOutRound()
        {
            return false;
        }

        private List<Match> GetGames(int group)
        {
            List<Club> clubs = this.groups[group];
            List<Match> res = new List<Match>();
            foreach(Match m in _matches)
            {
                if(clubs.Contains(m.home))
                {
                    res.Add(m);
                }
            }
            return res;
        }

        public override List<Match> GamesDay(int journey)
        {
            List<Match> res = new List<Match>();
            if(_matches.Count > 0)
            {
                for (int i = 0; i < groupsCount; i++)
                {
                    int matchPerGames = groups[i].Count / 2;
                    List<Match> games = GetGames(i);
                    int indexGameDay = journey - 1;
                    for (int j = matchPerGames * indexGameDay; j < matchPerGames * (indexGameDay + 1); j++)
                    {
                        if(j < games.Count)
                        {
                            res.Add(games[j]);
                        }
                    }
                }
            }
            return res;
        }

        public override int MatchesDayNumber()
        {
            int groupMatchsPerGamesDay = GroupMatchesPerGamesDay();
            return groupMatchsPerGamesDay == 0 || _groupsNumber == 0 ? 0 : _matches.Count / _groupsNumber / groupMatchsPerGamesDay;
        }

        public int GroupMatchesPerGamesDay()
        {
            return (_clubs.Count / _groupsNumber) / 2;
        }

        protected abstract void SpecialInitialize();

        public abstract List<Club> Ranking(int group, bool inverse = false);

        /// <summary>
        /// Get ranking in range [0, n[ for a club on its group
        /// </summary>
        /// <param name="club"></param>
        /// <returns></returns>
        public int Ranking(Club club)
        {
            int group = GroupOfClub(club);
            return Ranking(group, false).IndexOf(club);
        }

        public int GroupOfClub(Club club)
        {
            int res = 0;
            int i = 0;
            foreach(List<Club> group in this.groups)
            {
                if(group.Contains(club))
                {
                    res = i;
                }
                i++;
            }
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
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches, tiebreakers, pointsDeduction);
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
            if(this.groupsCount == 1 && this.groups[0].Count > 0)
            {
                return Ranking(0)[0];
            }
            else
            {
                return null;
            }
        }
    }
}