using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class ClubRandomRankingComparator : IComparer<Club>
    {

        private Dictionary<Club, int> clubsArtificalLevel;

        private readonly int _strength;
        private readonly int _makeClubsOfAtLeastChampionshipLevelOnTop;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strength"></param>
        /// <param name="championship">Clubs that are at or above the level of this championship on league system are automatically at the top of the ranking (-1 to not consider this parameter)</param>
        public ClubRandomRankingComparator(int strength, int championship)
        {
            clubsArtificalLevel = new Dictionary<Club, int>();
            _strength = strength;
            _makeClubsOfAtLeastChampionshipLevelOnTop = championship;
        }

        public int Compare(Club x, Club y)
        {
            if(!clubsArtificalLevel.ContainsKey(x))
            {
                clubsArtificalLevel[x] = (int)((100 * x.Level()) * Session.Instance.Random(10-_strength, 10+_strength) / 10.0f) + ((_makeClubsOfAtLeastChampionshipLevelOnTop > -1 && x.Championship != null && x.Championship.level <= _makeClubsOfAtLeastChampionshipLevelOnTop) ? 100000 : 0);
            }
            if (!clubsArtificalLevel.ContainsKey(y))
            {
                clubsArtificalLevel[y] = (int)((100 * y.Level()) * Session.Instance.Random(10 - _strength, 10 + _strength) / 10.0f) + ((_makeClubsOfAtLeastChampionshipLevelOnTop > -1 && y.Championship != null && y.Championship.level <= _makeClubsOfAtLeastChampionshipLevelOnTop) ? 100000 : 0);
            }

            return clubsArtificalLevel[y]-clubsArtificalLevel[x];
        }
    }
}