using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace tm.Comparators
{
    public class ClubRandomRankingComparator : IComparer<Club>
    {

        private Dictionary<Club, int> clubsArtificalLevel;

        private readonly int _strength;
        private readonly Tournament _makeClubsOfAtLeastChampionshipLevelOnTop;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strength"></param>
        /// <param name="championship">Clubs that are at or above this championship on league system are automatically at the top of the ranking (null to not consider this parameter)</param>
        public ClubRandomRankingComparator(int strength, Tournament championship)
        {
            clubsArtificalLevel = new Dictionary<Club, int>();
            _strength = strength;
            _makeClubsOfAtLeastChampionshipLevelOnTop = championship;
        }

        public int Compare(Club x, Club y)
        {
            if(!clubsArtificalLevel.ContainsKey(x))
            {
                clubsArtificalLevel[x] = (int)((100 * x.Level()) * Session.Instance.Random(10-_strength, 10+_strength) / 10.0f) + ((_makeClubsOfAtLeastChampionshipLevelOnTop != null && x.Championship != null && !x.Championship.IsBelow(new QualificationTournament(_makeClubsOfAtLeastChampionshipLevelOnTop))) ? 100000 : 0);
            }
            if (!clubsArtificalLevel.ContainsKey(y))
            {
                clubsArtificalLevel[y] = (int)((100 * y.Level()) * Session.Instance.Random(10 - _strength, 10 + _strength) / 10.0f) + ((_makeClubsOfAtLeastChampionshipLevelOnTop != null && y.Championship != null && !x.Championship.IsBelow(new QualificationTournament(_makeClubsOfAtLeastChampionshipLevelOnTop))) ? 100000 : 0);
            }

            return clubsArtificalLevel[y]-clubsArtificalLevel[x];
        }
    }
}