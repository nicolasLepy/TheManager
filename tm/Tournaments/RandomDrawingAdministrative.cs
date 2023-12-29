using System;
using System.Collections.Generic;
using tm.Comparators;

namespace tm.Tournaments
{
    public class RandomDrawingAdministrative : IRandomDrawing
    {
        private readonly GroupsRound _round;

        public RandomDrawingAdministrative(GroupsRound tour)
        {
            _round = tour;
        }

        private List<int> GetGroupSize(int totalTeams, int defaultMaxTeamsByGroup)
        {
            int groupCount = totalTeams / defaultMaxTeamsByGroup + (totalTeams % defaultMaxTeamsByGroup != 0 ? 1 : 0);
            int ecart = groupCount > 0 ? totalTeams % groupCount : totalTeams;
            List<int> res = new List<int>();
            for (int i = 0; i < groupCount; i++)
            {
                int add = i < ecart ? 1 : 0;
                res.Add(totalTeams/groupCount + add);
            }
            return res;
        }
        
        public void RandomDrawing()
        {
            Console.WriteLine("[Draw " + _round.Tournament.name + "]");
            Country hostCountry = Session.Instance.Game.kernel.LocalisationTournament(_round.Tournament) as Country;
            List<List<Club>> groups = new List<List<Club>>();
            List<string> groupNames = new List<string>();
            Console.WriteLine("Host country = " + hostCountry);
            Console.WriteLine("Reference ClubsByGroup = " + _round.referenceClubsByGroup);
            Console.WriteLine("ClubsByGroup = " + _round.clubs.Count / _round.groupsCount);
            if (hostCountry != null)
            {
                int defaultMaxTeamsByGroup = _round.referenceClubsByGroup == 0 ? _round.clubs.Count / _round.groupsCount : _round.referenceClubsByGroup;
                defaultMaxTeamsByGroup = _round.clubs.Count % _round.groupsCount != 0
                    ? defaultMaxTeamsByGroup + 1
                    : defaultMaxTeamsByGroup;
                defaultMaxTeamsByGroup += 2;
                //If the round is inactive, keep all clubs of a same association into one group
                if((_round as GroupInactiveRound) != null)
                {
                    defaultMaxTeamsByGroup = _round.clubs.Count;
                }
                Console.WriteLine("[MaxTeamsByGroup] " + defaultMaxTeamsByGroup);
                foreach (AdministrativeDivision ad in hostCountry.GetAdministrativeDivisionsLevel(_round.administrativeLevel))
                {
                    int admCounter = 0;
                    List<Club> clubsAdm = _round.GetClubsAdministrativeDivision(ad);
                    Console.WriteLine("[" + ad.name + "], équipes = " + clubsAdm.Count);
                    if (clubsAdm.Count > 0)
                    {

                        List<int> groupsCount = GetGroupSize(clubsAdm.Count, defaultMaxTeamsByGroup);
                        List<Club>[] splitClubs = Utils.CreateGeographicClusters(clubsAdm, groupsCount.Count);
                        for (int grp = 0; grp < groupsCount.Count; grp++)
                        {
                            groups.Add(splitClubs[grp]);
                            groupNames.Add(ad.name + " " + ++admCounter);
                        }
                    }
                }
            }

            _round.groupsCount = groups.Count;
            _round.InitializeGroups();
            _round.ClearGroupNames();
            int i = 0;
            foreach(List<Club> group in groups)
            {
                _round.groups[i] = group;
                _round.AddGroupName(groupNames[i]);
                i++;
            }
        }
        
    }
}