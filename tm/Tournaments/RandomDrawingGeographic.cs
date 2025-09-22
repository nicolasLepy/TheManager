using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using tm.Comparators;

namespace tm
{
    public class RandomDrawingGeographic : IRandomDrawing
    {

        private readonly GroupsRound _round;

        public RandomDrawingGeographic(GroupsRound tour)
        {
            _round = tour;
        }

        public void Draw(List<Club> clubs, int defaultMaxTeamsByGroup)
        {
            List<List<Club>> groups = new List<List<Club>>();

            List<int> groupsCount = Utils.GetGroupSize(clubs.Count, defaultMaxTeamsByGroup);
            List<Club>[] splitClubs = Utils.CreateGeographicClusters(clubs, groupsCount.Count);
            for (int grp = 0; grp < groupsCount.Count; grp++)
            {
                groups.Add(splitClubs[grp]);
            }

            _round.groupsCount = groups.Count;
            _round.InitializeGroups();
            _round.ClearGroupNames();
            int i = 0;
            foreach (List<Club> group in groups)
            {
                _round.groups[i] = group;
                i++;
            }
        }

        public void RandomDrawing()
        {
            List<Club> clubs = new List<Club>(_round.clubs);
            if(_round.groupsLocalisation.Count > 0)
            {
                int[] groupsCapacity = Utils.GetClustersCapacity(_round.clubs.Count, _round.groupsCount);
                for (int i = 0; i < _round.groupsCount; i++)
                {
                    GeographicPosition position = _round.groupsLocalisation[i];
                    clubs.Sort(new ClubLocalisationComparator(position));
                    for (int j = 0; j < groupsCapacity[i]; j++)
                    {
                        _round.groups[i].Add(clubs[0]);
                        clubs.RemoveAt(0);
                    }
                }
            }
            else
            {
                Draw(clubs, _round.maxClubsInGroup);
                /*List<Club>[] splitClubs = Utils.CreateGeographicClusters(clubs, _round.groupsCount);
                for(int i = 0; i<_round.groupsCount; i++)
                {
                    _round.groups[i].AddRange(splitClubs[i]);
                }*/
            }
        }
    }
}
