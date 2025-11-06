using FluentNHibernate.Testing.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using tm.Comparators;

namespace tm
{

    public struct AssociationCount
    {
        public Association association;
        public int count;
    }

    public struct GroupComposition
    {
        public int id;
        public int associationOccurences;
        public int groupSize;
    }

    public class AssociationCountComparator : IComparer<AssociationCount>
    {
        public int Compare(AssociationCount x, AssociationCount y)
        {
            return y.count - x.count;
        }
    }

    public class GroupCompositionComparator : IComparer<GroupComposition>
    {
        public int Compare(GroupComposition x, GroupComposition y)
        {
            int res = x.associationOccurences - y.associationOccurences;
            if(res == 0)
            {
                res = x.groupSize - y.groupSize;
            }
            return res;
        }
    }

    public class RandomDrawingLevel : IRandomDrawing
    {
        private readonly GroupsRound _round;
        /// <summary>
        /// Association holding the tournament
        /// </summary>
        private readonly Association _masterAssociation;
        private readonly Dictionary<Club, float> _clubCoefficients;

        private readonly Dictionary<Club, Association> _associationMap;
        private readonly Dictionary<Club, int> _potMap;

        public RandomDrawingLevel(GroupsRound round, List<Club> sortedClubs)
        {
            _round = round;
            _masterAssociation = _round.Tournament.association;
            _clubCoefficients = new Dictionary<Club, float>();
            _associationMap = new Dictionary<Club, Association>();
            _potMap = new Dictionary<Club, int>();
            for (int i = 0; i < sortedClubs.Count; i++)
            {
                _clubCoefficients[sortedClubs[i]] = sortedClubs.Count-i;
            }
        }

        public RandomDrawingLevel(GroupsRound round, Dictionary<Club, float> coefficients)
        {
            _round = round;
            _clubCoefficients = coefficients;
            _masterAssociation = _round.Tournament.association;
            _associationMap = new Dictionary<Club, Association>();
            _potMap = new Dictionary<Club, int>();
        }

        private void InitializeAssociationMap(List<Club> clubs)
        {
            _associationMap.Clear();
            foreach(Club club in clubs)
            {

                Association a;
                if (_round.rules.Contains(Rule.OneTeamByAssociationInGroup))
                {
                    a = club.Association().GetRepresentingAssociation(_masterAssociation);
                }
                else
                {
                    a = _masterAssociation;
                }
                _associationMap[club] = a;
            }
        }

        private void InitializePotMap(List<Club>[] clubs)
        {
            _potMap.Clear();
            int i = 0;
            foreach(List<Club> group in clubs)
            {
                i++;
                foreach(Club club in group)
                {
                    _potMap[club] = i;
                }
            }
        }

        public List<Club>[] SortAndShuffle(List<Club>[] clubs, int potsCount)
        {
            List<Club>[] res = new List<Club>[potsCount];

            for (int i = 0; i < potsCount; i++)
            {
                List<Club> pot = new List<Club>(clubs[i]);
                pot.Shuffle();
                res[i] = pot;
            }

            return res;

        }

        private int CountAssociation(List<Club> group, Association association)
        {
            int res = 0;
            foreach(Club c in group)
            {
                if (_associationMap[c] == association)
                {
                    res++;
                }
            }
            return res;
        }

        private bool ContainsTeamOfPot(List<Club> group, int pot)
        {
            bool res = false;

            foreach(Club c in group)
            {
                if (_potMap[c] == pot)
                {
                    res = true;
                }
            }

            return res;
        }

        private List<GroupComposition> AssociationPresenceByGroup(List<Club>[] groups, Association association)
        {
            List<GroupComposition> res = new List<GroupComposition>();

            for(int i = 0; i< groups.Length; i++)
            {
                int associationOccurences = CountAssociation(groups[i], association);
                GroupComposition gc = new GroupComposition()
                {
                    id = i,
                    associationOccurences = associationOccurences,
                    groupSize = groups[i].Count
                };
                res.Add(gc);
            }

            res.Sort(new GroupCompositionComparator());
            return res;
        }

        private int GetNextGroup(List<Club>[] groups, Club club)
        {
            int res = -1;
            List<int> possibleGroups = new List<int>();
            for(int i = 0; i < groups.Length; i++)
            {
                if (!ContainsTeamOfPot(groups[i], _potMap[club]))
                {
                    possibleGroups.Add(i);
                }
            }
            if(possibleGroups.Count > 0)
            {
                List<int> bestPossibleGroups = new List<int>();

                List<GroupComposition> groupsCompositionAll = AssociationPresenceByGroup(groups, _associationMap[club]);
                List<GroupComposition> groupsComposition = new List<GroupComposition>();
                foreach(GroupComposition gc in groupsCompositionAll)
                {
                    if(possibleGroups.Contains(gc.id))
                    {
                        groupsComposition.Add(gc);
                    }
                }
                int refAssociationOccurences = groupsComposition[0].associationOccurences;
                foreach(GroupComposition gc in groupsComposition)
                {
                    if(possibleGroups.Contains(gc.id) && gc.associationOccurences == refAssociationOccurences)
                    {
                        bestPossibleGroups.Add(gc.id);
                    }
                }
                res = bestPossibleGroups[Session.Instance.Random(bestPossibleGroups.Count)];
            }

            return res;
        }

        private List<Club>[] ReorderGroups(List<Club>[] groups)
        {
            IEnumerable<List<Club>> sorted = groups.OrderByDescending(x => x.Count);
            List<Club>[] newGroups = new List<Club>[groups.Length];
            int i = 0;
            foreach(List<Club> clubs in sorted)
            {
                newGroups[i++] = clubs;
            }

            if(_round.rules.Contains(Rule.HostedByOneAssociation))
            {
                List<Association> hosts = _round.Tournament.Hosts();
                List<int> groupsWithHosts = new List<int>();
                for(int g = 0; g < groups.Length; g++)
                {
                    foreach(Club c in groups[g])
                    {
                        if (hosts.Contains(c.Association().ClosestStateAssociation()))
                        {
                            groupsWithHosts.Add(g);
                        }
                    }
                }
                if(groupsWithHosts.Count == 1)
                {
                    List<Club> temp = newGroups[groupsWithHosts[0]];
                    newGroups[groupsWithHosts[0]] = newGroups[0];
                    newGroups[0] = temp;
                }
            }

            return newGroups;
        }

        private List<Club>[] Draw(List<Club> pool, int groupsCount, List<AssociationCount> associations)
        {
            List<Club>[] groups = new List<Club>[groupsCount];
            for(int i = 0; i < groupsCount; i++)
            {
                groups[i] = new List<Club>();
            }
            foreach(AssociationCount ac in associations)
            {
                List<Club> associationTeams = FilterByAssociation(pool, ac.association);
                foreach(Club club in associationTeams)
                {
                    int nextGroup = GetNextGroup(groups, club);
                    groups[nextGroup].Add(club);
                }
            }
            groups = ReorderGroups(groups);
            return groups;
        }

        private List<Club> FilterByAssociation(List<Club> clubs, Association association)
        {
            List<Club> result = new List<Club>();
            foreach(Club club in clubs)
            {
                if (_associationMap[club] == association)
                {
                    result.Add(club);
                }
            }
            return result;
        }

        private List<AssociationCount> CountAssociations(List<Club> teams)
        {
            Dictionary<Association, int> associations = new Dictionary<Association, int>();
            foreach(Club club in teams)
            {
                if (!associations.ContainsKey(_associationMap[club]))
                {
                    associations[_associationMap[club]] = 0;
                }
                associations[_associationMap[club]]++;
            }

            List<AssociationCount> ac = new List<AssociationCount>();
            foreach(KeyValuePair<Association, int> kvp in associations)
            {
                ac.Add(new AssociationCount() { association = kvp.Key, count = kvp.Value });
            }
            ac.Sort(new AssociationCountComparator());
            return ac;
        }

        private Dictionary<Association, int> AssociationsRepresented()
        {
            Dictionary<Association, int> representations = new Dictionary<Association, int>();
            foreach(Club club in _round.clubs)
            {
                Association ca = club.Association().GetRepresentingAssociation(_masterAssociation);
                if(!representations.ContainsKey(ca))
                {
                    representations[ca] = 0;
                }
                representations[ca]++;
            }
            return representations;
        }

        private List<Club> SortClubsCoefficient(List<Club> clubs, Dictionary<Club, float> coefficients)
        {
            List<Club> pot = new List<Club>(clubs);
            pot.Sort(new CoefficientComparator<Club>(coefficients));
            return pot;
        }

        private List<Club> SortClubs(List<Club> clubs)
        {
            return SortClubsCoefficient(clubs, _clubCoefficients);
        }

        public void RandomDrawing()
        {
            List<Club> pool = SortClubs(_round.clubs);
            List<Club>[] pots = CreatePots(pool);
            InitializeAssociationMap(pool);
            InitializePotMap(pots);
            List<AssociationCount> associations = CountAssociations(pool);
            List<Club>[] groups = Draw(pool, _round.groupsCount, associations);
            for (int i = 0; i < _round.groupsCount; i++)
            {
                _round.groups[i].Clear();
                _round.groups[i].AddRange(groups[i]);
            }
        }
        
        public List<Club>[] CreatePots(List<Club> clubsSorted)
        {
            int minTeamsByGroup = _round.clubs.Count / _round.groupsCount;
            int maxTeamsByGroup = (int)Math.Ceiling(_round.clubs.Count / (_round.groupsCount + 0.0));

            int numberOfPots = _round.clubs.Count / _round.groupsCount;
            if (_round.clubs.Count % _round.groupsCount != 0)
            {
                numberOfPots++;
            }
            int maxTeamsByPot = _round.groupsCount;

            List<Club>[] pots = new List<Club>[numberOfPots];
            for(int i = 0; i < numberOfPots; i++)
            {
                pots[i] = new List<Club>();
            }

            for(int i = 0; i < clubsSorted.Count; i++)
            {
                int potNumber = i / maxTeamsByPot;
                pots[potNumber].Add(clubsSorted[i]);
            }

            return pots;

        }

        /*public void RandomDrawingOld()
        {
            List<Club> pot = SortClubs(_round.clubs, _attribute);
            int minTeamsByGroup = _round.clubs.Count / _round.groupsCount;

            List<Club>[] baseHats = CreatePots(pot);

            //Shuffle hats because of the case 2
            for (int i = 0; i < baseHats.Count(); i++)
            {
                baseHats[i].Shuffle();
            }

            bool succeed = false;
            while(!succeed)
            {
                succeed = true;
                for(int i = 0; i<_round.groupsCount; i++)
                {
                    _round.groups[i].Clear();
                }
                //try
                //{
                    List<Club>[] hats = new List<Club>[baseHats.Length];
                    for(int i = 0; i < baseHats.Length; i++)
                    {
                        hats[i] = new List<Club>(baseHats[i]);
                    }

                    //Foreach groups
                    for (int i = 0; i < _round.groupsCount; i++)
                    {

                        //Create constraints dictionnary. Specify for each association minimum and maximum numbers of teams on each group to respect rules.
                        //Only used if round include OneTeamByAssociation rule
                        Dictionary<Association, List<int>> constraintsAssociations = new Dictionary<Association, List<int>>();

                        if(_round.rules.Contains(Rule.OneTeamByAssociationInGroup))
                        {
                            Dictionary<Association, int> representations = AssociationsRepresented();
                            foreach (KeyValuePair<Association, int> kvp in representations)
                            {
                                int teamsCount = TeamsOfAssociation(hats, kvp.Key);
                                float teamsRatio = teamsCount / (_round.groupsCount - i + 0.0f);
                                constraintsAssociations.Add(kvp.Key, new List<int> { (int)Math.Floor(teamsRatio), (int)Math.Ceiling(teamsRatio) });
                            }

                        }

                    //Foreach hats
                    for (int j = 0; j < hats.Length; j++)
                        {
                            if (hats[j].Count > 0)
                            {
                                List<Club> possibleTeams = new List<Club>();

                                foreach (Club hatClub in hats[j])
                                {
                                    if (!_round.rules.Contains(Rule.OneTeamByAssociationInGroup))
                                    {
                                        possibleTeams.Add(hatClub);
                                    }
                                    else
                                    {
                                        Association hcAssociation = hatClub.Association().GetRepresentingAssociation(_masterAssociation);

                                        //Case 1
                                        //Si le nombre d'équipes par groupe + les équipes qui sont obligées d'arriver (car certains chapeaux contiennent uniquement des équipes de tel pays) est inférieur au nombre maximal d'équipes autorisées dans le groupe, alors on peut l'ajouter aux équipes sélectionnables.
                                        if (CountClubsOfAssociation(_round.groups[i], hcAssociation) - HatsWithOnlyTeamsOfAssociation(hats, j + 1, hcAssociation) < constraintsAssociations[hcAssociation][1])
                                        {
                                            possibleTeams.Add(hatClub);
                                        }
                                        //Case 2
                                        // Si le nombre de chapeaux restants avec des équipes de tel pays est égal au nombre minimum d'équipes nécessaire qui manque dans le groupe, alors on est obligé de prendre cette équipe
                                        if (RemainingHatsWithTeamsOfAssociation(hats, j, hcAssociation) == constraintsAssociations[hcAssociation][0] - CountClubsOfAssociation(_round.groups[i], hcAssociation))
                                        {
                                            possibleTeams = new List<Club> { hatClub };
                                            break;
                                        }
                                    }

                                }

                                Club selectedTeam = possibleTeams[Session.Instance.Random(0, possibleTeams.Count)];
                                _round.groups[i].Add(selectedTeam);
                                hats[j].Remove(selectedTeam);
                            }
                        }
                    }
                //}
                catch(Exception e)
                {
                    Utils.Debug(_round.Tournament.name + " (" + _round.name + ") Echec du tirage au sort de ce tour. Nouvelle tentative");
                    succeed = false;
                }
            }
        }

        private int CountClubsOfAssociation(List<Club> clubs, Association association)
        {
            int res = 0;
            foreach(Club c in clubs)
            {
                Association comp = c.Association().GetRepresentingAssociation(_masterAssociation);
                if(comp == association)
                {
                    res++;
                }
            }
            return res;
        }

        private int TeamsOfAssociation(List<Club>[] hats, Association association)
        {
            int res = 0;
            foreach(List<Club> lc in hats)
            {
                foreach(Club c in lc)
                {
                    if(c.Association().GetRepresentingAssociation(_masterAssociation) == association)
                    {
                        res++;
                    }
                }
            }
            return res;
        }

        private int RemainingHatsWithTeamsOfAssociation(List<Club>[] hats, int currentHat, Association association)
        {
            int res = 0;
            for (int i = currentHat; i < hats.Count(); i++)
            {
                bool hasTeamsOfAssociation = false;
                foreach (Club c in hats[i])
                {
                    Association comp = c.Association().GetRepresentingAssociation(_masterAssociation);
                    if (comp == association)
                    {
                        hasTeamsOfAssociation = true;
                    }
                }
                if (hasTeamsOfAssociation)
                {
                    res++;
                }
            }
            return res;
        }

        private int HatsWithOnlyTeamsOfAssociation(List<Club>[] hats, int currentHat, Association association)
        {
            int res = 0;
            for (int i = currentHat; i < hats.Count(); i++)
            {
                bool onlyTeam = true;
                foreach (Club c in hats[i])
                {
                    Association comp = c.Association().GetRepresentingAssociation(_masterAssociation);
                    onlyTeam = onlyTeam && comp == association;
                }
                if (onlyTeam)
                {
                    res++;
                }
            }
            return res;
        }*/
    }
}
