using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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
using static tm.Utils;

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
        /// If False, qualification are defined for each group (1st of each group will qualifie for ...).
        /// Otherwise, qualifications are defined for all group (only 1 team will qualifie for ...).
        /// </summary>
        [DataMember]
        protected bool _qualificationsDefinedForAllGroup;


        public int referenceClubsByGroup => _referenceClubsByGroup;


        public List<Club>[] groups { get => _groups; }

        public int groupsCount
        {
            get => _groupsNumber; set => _groupsNumber = value;
        }

        public int maxClubsInGroup => _clubs.Count % _groupsNumber > 0 ? (_clubs.Count / _groupsNumber) + 1 : _clubs.Count / _groupsNumber;

        public int nonGroupGamesByTeams => _nonGroupGamesByTeams;
        public int nonGroupGamesByGameday => _nonGroupGamesByGameday;
        public bool fusionGroupAndNoGroupGames => _fusionGroupAndNoGroupGames;

        public bool qualificationsDefinedForAllGroup => _qualificationsDefinedForAllGroup;

        public List<GeographicPosition> groupsLocalisation { get => _groupsLocalisation; }

        //TODO: Getter only
        public RandomDrawingMethod RandomDrawingMethod
        {
            get => _randomDrawingMethod; set => _randomDrawingMethod = value;
        }
        
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

        public GroupsRound(int id, string name, Tournament tournament, Hour hour, List<GameDay> dates, List<TvOffset> offsets, int groupsCount, bool qualificationsDefinedForAllGroup, int phases, GameDay initialisation, GameDay end, int keepRankingFromPreviousRound, RandomDrawingMethod randomDrawingMethod, bool fusionGroupAndNoGroupGames, int nonGroupGamesByTeams, int nonGroupGamesByGameday, int gamesPriority, int lastDaysSameDay) : base(id, name, tournament, hour, dates, offsets, initialisation,end, phases, lastDaysSameDay, keepRankingFromPreviousRound, gamesPriority)
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
            _relegationsByAssociations = new Dictionary<Association, int>();
            _nonGroupGamesByTeams = nonGroupGamesByTeams;
            _nonGroupGamesByGameday = nonGroupGamesByGameday;
            _fusionGroupAndNoGroupGames = fusionGroupAndNoGroupGames;
            _cacheRanking = new List<Club>[0];
            _qualificationsDefinedForAllGroup = qualificationsDefinedForAllGroup;
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

        /// <summary>
        /// Returns the number of teams of an association who will be relegated
        /// </summary>
        /// <param name="association"></param>
        /// <returns></returns>
        public List<Club> GetAssociationRelegables(Association association)
        {
            Tournament selfTournament = Tournament;
            List<Club> candidates = new List<Club>();
            for(int group = 0; group < groupsCount; group++)
            {
                List<Club> ranking = Ranking(group);
                List<Qualification> qualifications = GetGroupQualifications(group);
                foreach(Qualification q in qualifications)
                {
                    Club c = ranking[q.ranking-1];
                    if(selfTournament.IsAbove(q.target) && c.Association().IsDirectConnected(association))
                    {
                        candidates.Add(c);
                    }
                }
            }
            return candidates;
        }

        protected abstract List<Club> RankClubs(List<Club> clubs, List<Tiebreaker> tiebreakers, Dictionary<Club, List<PointDeduction>> pointsDeduction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rank">Rank can be negative</param>
        /// <param name="association">If association is None, get Ranking for teams from all groups, else get only teams for the specified association</param>
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
                    //Get mirror qualification where below teams are qualified. If not found, bottomQualification.tournament == null
                    Qualification bottomQualification = baseQualifications.Where(x => x.ranking == baseQualifications[i].ranking && x.qualifies < 0).Select(x => x).FirstOrDefault();
                    bool concernRelegation = bottomQualification.target != null ? from.IsSameLevel(baseQualifications[i].target) : false;
                    int rank = concernRelegation ? baseQualifications[i].ranking - maxRanking - 1 : baseQualifications[i].ranking;
                    if (TeamIsTopRBottom(ranking, rank, baseQualifications[i].qualifies, null))
                    {
                        qualifications.Add(new Qualification(baseQualifications[i].ranking, baseQualifications[i].roundId, baseQualifications[i].target, baseQualifications[i].isNextYear, 0));
                    }
                    else if(bottomQualification.target != null)
                    {
                        qualifications.Add(new Qualification(bottomQualification.ranking, bottomQualification.roundId, bottomQualification.target, bottomQualification.isNextYear, 0));
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
        /// Get the number of teams relegated from above division that will be entering this league for the next edition
        /// </summary>
        /// <param name="selfTournament"></param>
        /// <returns></returns>
        private KeyValuePair<Tournament, int> GetRelegatedFromAbove(Tournament selfTournament)
        {
            int count = 0;
            Association selfAssociation = Session.Instance.Game.kernel.LocalisationTournament(selfTournament);
            Tournament source = selfAssociation.LeagueAbove(selfTournament).Tournament();
            GroupsRound upperRound = source.rounds[0] as GroupsRound;
            if(upperRound != null)
            {
                for(int i = 0; i < upperRound.groupsCount; i++)
                {
                    List<Qualification> qualifications = upperRound.GetGroupQualifications(i);
                    foreach(Qualification q in qualifications)
                    {
                        if(q.target.Tournament(selfAssociation) == selfTournament)
                        {
                            count++;
                        }
                    }
                }
            }
            return new KeyValuePair<Tournament, int>(source, count);
        }

        /// <summary>
        /// WARNING: Doesn't take in account prom/relegation playoffs (direct prom/rel spots)
        /// Get the promotions and relegations spots for the entire round (for all groups)
        /// A dictionary<int, int> is returned
        /// Promotions spots are defined through the key 1
        /// Relegations spots are defined through the key -1
        /// </summary>
        /// <returns></returns>
        private Dictionary<QualificationType, KeyValuePair<QualificationTarget, int>> GetDirectPromotionsRelegationsSpots(List<Qualification> qualifications, Association selfAssociation, Tournament selfTournament)
        {
            int totalPromotions = 0;
            int totalRelegations = 0;
            QualificationTarget targetPromotion = null;
            QualificationTarget targetRelegation = null;

            foreach (Qualification q in qualifications)
            {
                int count = qualificationsDefinedForAllGroup ? 1 : (q.qualifies == 0 ? groups.Length : Math.Abs(q.qualifies));
                if (q.isNextYear && selfTournament.IsBelow(q.target))
                {
                    totalPromotions += count;
                    targetPromotion = q.target;
                }
                if (q.isNextYear && selfTournament.IsAbove(q.target))
                {
                    totalRelegations += count;
                    targetRelegation = q.target;
                }
            }

            //J'ai juste à savoir le nombre d'équipes reléguées de l'association du haut et j'adapte
            int totalRelegationsFromAbove = 0;
            if (selfAssociation.parent != null)
            {
                totalRelegationsFromAbove = selfAssociation.parent.GetExcludedTeamsFromLeagueSystem(selfAssociation);
            }

            Console.WriteLine("[{0}] Association {1} got {2} excluded teams", selfTournament.name, selfAssociation.name, totalRelegationsFromAbove);

            totalRelegations += totalRelegationsFromAbove;

            if(selfAssociation.LeagueBelow(selfTournament) == null)
            {
                totalRelegations = 0;
                targetRelegation = null;
            }
            else if(totalRelegationsFromAbove == totalRelegations) //so targetRelegation is still not defined
            {
                targetRelegation = selfAssociation.LeagueBelow(selfTournament);
            }

            Dictionary<QualificationType, KeyValuePair<QualificationTarget, int>> res = new Dictionary<QualificationType, KeyValuePair<QualificationTarget, int>>
            {
                { QualificationType.DirectPromotion, new KeyValuePair<QualificationTarget, int>(targetPromotion, totalPromotions)},
                { QualificationType.DirectRelegation, new KeyValuePair<QualificationTarget, int>(targetRelegation, totalRelegations)}
            };
            return res;
        }

        /// <summary>
        /// Adjust qualifications according to region and district constraints
        /// </summary>
        /// <param name="baseQualifications">List of initial qualifications</param>
        /// <param name="group">Concerned group</param>
        /// <param name="tournament">Base tournament</param>
        /// <returns>New list of qualifications</returns>
        public List<Qualification> AdjustQualifications(List<Qualification> baseQualifications, int group, Tournament selfTournament)
        {

            List<Qualification> adjustedQualifications = new List<Qualification>(baseQualifications);
            adjustedQualifications.Sort(new QualificationRankingComparator());
            Association selfAssociation = Session.Instance.Game.kernel.LocalisationTournament(selfTournament);
            Console.WriteLine("[{0}][{1}][Groupe {2}]", selfTournament.name, selfAssociation.name, group);

            Tournament upperTournament = selfAssociation.LeagueAbove(selfTournament)?.Tournament();

            //J'ai juste à savoir le nombre d'équipes reléguées de l'association du haut et j'adapte
            Dictionary<QualificationType, KeyValuePair<QualificationTarget, int>> dictionaryQualifications = GetDirectPromotionsRelegationsSpots(qualifications, selfAssociation, selfTournament);

            if (qualificationsDefinedForAllGroup)
            {
                adjustedQualifications = new List<Qualification>();
                int totalPromotion = dictionaryQualifications[QualificationType.DirectPromotion].Value;
                int totalRelegations = dictionaryQualifications[QualificationType.DirectRelegation].Value;

                List<Club> groupRanking = Ranking(group);
                // == Move to a independant method ==
                int promotionsOfGroup = totalPromotion / groupsCount;
                int relegationsOfGroup = totalRelegations / groupsCount;
                int extraPromotions = totalPromotion % groupsCount;
                int extraRelegations = totalRelegations % groupsCount;
                if (extraPromotions != 0)
                {
                    List<Club> rankingRank = RankingByRank(promotionsOfGroup + 1, null);
                    if(rankingRank.IndexOf(groupRanking[promotionsOfGroup+1 - 1]) < extraPromotions)
                    {
                        promotionsOfGroup++;
                    }
                }
                if(extraRelegations != 0)
                {
                    int negativeRanking = -relegationsOfGroup - 1;
                    List<Club> rankingRank = RankingByRank(negativeRanking, null);
                    rankingRank.Reverse();
                    if (rankingRank.IndexOf(groupRanking[groupRanking.Count + negativeRanking]) < extraRelegations)
                    {
                        relegationsOfGroup++;
                    }
                }

                if (promotionsOfGroup + relegationsOfGroup > groupRanking.Count)
                {
                    throw new Exception("Too many promotion and relegation for the group");
                }

                QualificationTarget promotionTarget = new QualificationTournament(upperTournament);
                QualificationTarget relegationTarget = selfAssociation.LeagueBelow(selfTournament);
                QualificationTarget noChangeTarget = new QualificationTournament(selfTournament);
                if(relegationTarget == null)
                {
                    relegationsOfGroup = 0;
                }
                //TODO: Attention, pour les coupes qui définissent des qualifiés à dispatcher pour les groupes (qualifications internationales), il faut récupérer les Qualifications pour les dispatcher et non en recréer comme ça
                for(int i = 0; i < groupRanking.Count; i++)
                {
                    if(i < promotionsOfGroup)
                    {
                        adjustedQualifications.Add(new Qualification(i + 1, 0, promotionTarget, true, 0));
                    }
                    else if(groupRanking.Count - i <= relegationsOfGroup)
                    {
                        adjustedQualifications.Add(new Qualification(i + 1, 0, relegationTarget, true, 0));
                    }
                    else
                    {
                        adjustedQualifications.Add(new Qualification(i + 1, 0, noChangeTarget, true, 0));
                    }
                }
                // == End ==

            }

            return adjustedQualifications;
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
                int countChampionshipQualifications = Utils.CountChampionshipQualifications(qualifications);
                int minTeamsByGroup = clubs.Count / groupsCount;
                int groupsWithExtraTeam = clubs.Count % groupsCount;

                //It's a group with extra team, qualifications needs to be adapted
                if (group < groupsWithExtraTeam /*groups[group].Count == minTeamsByGroup + 1*/ && countChampionshipQualifications == minTeamsByGroup)
                {
                    allQualifications.Sort(new QualificationRankingComparator());
                    int firstRankingToBottom = -1;
                    foreach (Qualification q in allQualifications)
                    {
                        if (tournament.IsAbove(q.target) && firstRankingToBottom == -1)
                        {
                            firstRankingToBottom = q.ranking;
                        }
                    }
                    if (firstRankingToBottom > -1)
                    {
                        for (int i = firstRankingToBottom - 1; i < allQualifications.Count; i++)
                        {
                            allQualifications[i] = new Qualification(allQualifications[i].ranking + 1, allQualifications[i].roundId, allQualifications[i].target, allQualifications[i].isNextYear, allQualifications[i].qualifies);
                        }
                        allQualifications.Add(new Qualification(firstRankingToBottom, allQualifications[firstRankingToBottom - 2].roundId, allQualifications[firstRankingToBottom - 2].target, allQualifications[firstRankingToBottom - 2].isNextYear, allQualifications[firstRankingToBottom - 2].qualifies));
                    }
                    else
                    {
                        Qualification lastQualification = allQualifications[allQualifications.Count - 1];
                        allQualifications.Add(new Qualification(lastQualification.ranking + 1, lastQualification.roundId, lastQualification.target, lastQualification.isNextYear, lastQualification.qualifies));
                    }
                }


                //Adapt qualifications to adapt negative ranking to real ranking in the group
                int totalClubs = _groups[group].Count > 0 ? _groups[group].Count : _clubs.Count / _groupsNumber; //Get theoretical clubs by group if groups were not drawn
                allQualifications = AdaptQualificationsToRanking(allQualifications, totalClubs);

                
                // New implementation
                if (_groups[group].Count > 0)
                {
                    allQualifications = AdjustQualificationsGroup(allQualifications, group, tournament);
                    allQualifications = AdjustQualifications(allQualifications, group, tournament);

                    Association selfAssociation = Session.Instance.Game.kernel.LocalisationTournament(tournament);
                    int groupsCount = groups.Length;
                    Dictionary<QualificationType, KeyValuePair<QualificationTarget, int>> promRelSpots = GetDirectPromotionsRelegationsSpots(qualifications, selfAssociation, tournament);
                    QualificationTarget targetDirectRelegation = promRelSpots[QualificationType.DirectRelegation].Key;
                    QualificationTarget targetDirectPromotion = promRelSpots[QualificationType.DirectPromotion].Key;
                    int totalRelegations = promRelSpots[QualificationType.DirectRelegation].Value;
                    int totalPromotions = promRelSpots[QualificationType.DirectPromotion].Value;
                    if (tournament.isChampionship)
                    {
                        allQualifications = AdjustQualificationsToReserves(allQualifications, Ranking(group), selfAssociation, tournament, _rules.Contains(Rule.ReservesCannotBePromoted), totalRelegations, totalPromotions, targetDirectRelegation, targetDirectPromotion, groupsCount);
                    }

                }
                _storedGroupQualifications[group] = allQualifications;
                return allQualifications;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialQualifications">Qualifications du groupe</param>
        /// <param name="ranking">Classement du groupe</param>
        /// <param name="association">Association concernée</param>
        /// <param name="tournament">Compétition concernée</param>
        /// <param name="round">Tour concerné</param>
        /// <param name="reservesCantBePromoted">Empêche n'importe quelle réserve de monter</param>
        /// <param name="totalRelegations">Nombre total de relégations. Si !qualificationsDefinedForAllGroup, ce paramètre n'est pas utilisé</param>
        /// <param name="totalPromotions">Nombre total de promotions. Si !qualificationsDefinedForAllGroup, ce paramètre n'est pas utilisé</param>
        /// <param name="targetDirectRelegation">Destination des équipes releguées</param>
        /// <param name="targetDirectPromotion">Destination des équipes promues</param>
        /// <param name="groupsCount">Nombre de groupes</param>
        /// <returns>Les nouvelles qualifications</returns>
        /// <exception cref="Exception"></exception>
        private List<Qualification> AdjustQualificationsToReserves(List<Qualification> initialQualifications, List<Club> ranking, Association association, Tournament tournament, bool reservesCantBePromoted, int totalRelegations, int totalPromotions, QualificationTarget targetDirectRelegation, QualificationTarget targetDirectPromotion, int groupsCount)
        {
            Console.WriteLine("[AdjustQualificationsToReserves] {0}, {1} relegations for {2} groups. Association : {3}", tournament.name, totalRelegations, groupsCount, association);

            //Forcer targetDirectPromotion à être la division immédiatement inférieure
            targetDirectPromotion = association.LeagueAbove(tournament);

            //Si les qualifications sont définis groupe par groupe, on se concentre uniquement sur les clubs du groupe pour dispatcher les qualifications
            List<Club> fullInverseRanking = qualificationsDefinedForAllGroup ? GetFullRankingInversed(this, null) : Inverse(ranking);
            List<Club> fullRanking = qualificationsDefinedForAllGroup ? GetFullRanking(this) : new List<Club>(ranking);

            //Ces réserves seront releguées quelque soit leur classement
            List<Club> automaticallyRelegatedReserves = ReservesAutomaticallyRelegated(clubs, null, tournament, reservesCantBePromoted);

            Round relegationBarrageFinalRound = tournament.GetFinalTopPlayOffRound(true);
            List<Round> relegationBarrageRounds = relegationBarrageFinalRound != null && relegationBarrageFinalRound != this ? tournament.GetPlayOffsTree(relegationBarrageFinalRound.Tournament, relegationBarrageFinalRound, new List<Round>()) : new List<Round>();

            Round promotionBarrageFinalRound = tournament.GetFinalTopPlayOffRound(false);
            List<Round> promotionBarrageRounds = promotionBarrageFinalRound != null && promotionBarrageFinalRound != this ? tournament.GetPlayOffsTree(promotionBarrageFinalRound.Tournament, promotionBarrageFinalRound, new List<Round>()) : new List<Round>();

            //Trois phases
            //Phase préliminaire : comptage des promotions/relégations et listage des qualifications spéciales (barrages, plays-off de championnat ...) qui devront être réparties dans le même ordre
            //Note : les places
            //Première phase : Les places de relégations sont dispatchées. Les équipes sont balayées de haut en bas. On attribue d'office une relégation aux réserves qui doivent descendre. Les places de barrages puis de relégation directe sont attribuées aux équipes dans l'ordre
            //Deuxième phase : Les places de promotion sont dispatchées. Même système.
            //Troisième phase : On recrée une liste de qualifications à partir du dictionnaire association à chaque club sa qualification
            //TODO: Problème à gérer : s'il y a trop de réserves releguées d'office : peut mener à plus de relégations que prévue : il faut transférer l'information aux ligues inférieures pour adapter.

            //Note : Les qualifications pour les playsoffs sont propres au groupe.

            Dictionary<QualificationType, List<Qualification>> roundQualifications = ComputeRoundDestinations(tournament, initialQualifications, relegationBarrageRounds, promotionBarrageRounds);

            int promotionsCounter = totalPromotions;
            int relegationsCounter = totalRelegations;

            //Qualifications définies groupe par groupe : le nombre total de promotions/relégation pour ce tour est ignoré
            if(!qualificationsDefinedForAllGroup)
            {
                promotionsCounter = roundQualifications[QualificationType.DirectPromotion].Count;
                relegationsCounter = roundQualifications[QualificationType.DirectRelegation].Count;
            }

            //Qualification.ranking doesn't matter here, it will be recalculated to match club's rank
            Dictionary<Club, Qualification> qualificationsMap = new Dictionary<Club, Qualification>();

            //On commence par les relégations
            if (targetDirectRelegation != null)
            {
                for (int i = 0; i < fullInverseRanking.Count; i++)
                {
                    int remainingClubs = fullInverseRanking.Count - i;
                    Club club = fullInverseRanking[fullInverseRanking.Count - i - 1];
                    bool isRelegated = automaticallyRelegatedReserves.Contains(club);
                    bool atLeastBarragist = relegationsCounter + roundQualifications[QualificationType.PossibleRelegation].Count == remainingClubs;
                    if (isRelegated || atLeastBarragist)
                    {
                        Qualification qualif = new Qualification(0, 0, null, false, 0);
                        if (!isRelegated && (roundQualifications[QualificationType.PossibleRelegation].Count > 0 && ranking.Contains(club)))
                        {
                            qualif = roundQualifications[QualificationType.PossibleRelegation][0];
                            roundQualifications[QualificationType.PossibleRelegation].RemoveAt(0);
                        }
                        else
                        {
                            if (relegationsCounter == 0)
                            {
                                throw new Exception(string.Format("Should relegate {0} but no relegation available", club.name));
                            }
                            qualif = new Qualification(-1, 0, targetDirectRelegation, true, 0);
                            relegationsCounter--;
                        }
                        if(qualif.target != null)
                        {
                            qualificationsMap[club] = qualif;
                        }
                    }
                }
            }

            //Deuxième phase : les promotions et barrages de promotions
            Qualification mockPromotion = new Qualification(-1, 0, targetDirectPromotion, true, 0);
            for (int i = 0; i < fullRanking.Count; i++)
            {
                Club club = fullRanking[i];
                RuleStatus status = targetDirectPromotion != null ? RuleIsRespected(club, mockPromotion, tournament, reservesCantBePromoted) : RuleStatus.RuleRespected;
                if (status.HasFlag(RuleStatus.RuleRespected))
                {
                    if (promotionsCounter > 0)
                    {
                        promotionsCounter--;
                        qualificationsMap[club] = new Qualification(-1, 0, targetDirectPromotion, true, 0);
                    }
                    else if (roundQualifications[QualificationType.PossiblePromotion].Count > 0 && ranking.Contains(club))
                    {
                        Qualification qualif = roundQualifications[QualificationType.PossiblePromotion][0];
                        roundQualifications[QualificationType.PossiblePromotion].RemoveAt(0);
                        qualificationsMap[club] = qualif;
                    }
                }
            }

            //Troisième phase : on créé les qualifications à partir de qualificationsMap
            List<Qualification> newQualifications = new List<Qualification>();
            QualificationTarget defaultTarget = new QualificationTournament(tournament);
            for (int i = 0; i < ranking.Count; i++)
            {
                Club club = ranking[i];
                Qualification q;
                if (qualificationsMap.ContainsKey(club))
                {
                    Qualification qRef = qualificationsMap[club];
                    q = new Qualification(i + 1, qRef.roundId, qRef.target, qRef.isNextYear, qRef.qualifies);
                }
                else
                {
                    q = new Qualification(i + 1, 0, defaultTarget, true, 0);
                }
                newQualifications.Add(q);
            }

            return newQualifications;
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
                qualifications.Sort(new QualificationRankingComparator());

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
                            q.target.Tournament(c).rounds[q.roundId].clubs.Add(c);
                        }
                        else if(q.isNextYear && forNextYear)
                        {
                            Tournament clubNewTournament = q.target.RegisterTeamForNextEdition(c, q.roundId);
                            if (Tournament.level == 1 && !Session.Instance.Game.kernel.LocalisationTournament(Tournament).isStateAssociation)
                            {
                                Console.WriteLine("[" + i + "], " + c.name + " - " + q.ranking + " -> " + clubNewTournament.name + " [" + q.qualifies + ", " + caseQualifieMoreThan0 + ", " + caseQualifieLessThan0 + "]");
                            }
                        }
                        if (q.target.ToChampionshipTournament() && c.Championship != null)
                        {
                            
                            if (c.Championship.IsAbove(q.target))
                            {
                                c.supporters = (int)(c.supporters / 1.8f);
                            }
                            else if (c.Championship.IsBelow(q.target))
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

        protected abstract void SetGroups();

        public override void DistributeGrants()
        {
            for(int i = 0; i < groupsCount; i++)
            {
                List<Club> ranking = Ranking(i);
                foreach(Prize d in _prizes)
                {
                    if(ranking.Count > d.Ranking - 1)
                    {
                        CityClub cv = Ranking(i)[d.Ranking - 1] as CityClub;
                        if (cv != null)
                        {
                            cv.ModifyBudget(d.Amount, BudgetModificationReason.TournamentGrant);
                        }
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