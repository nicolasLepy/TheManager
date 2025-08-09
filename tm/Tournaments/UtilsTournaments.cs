using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.Tournaments
{
    public static class UtilsTournaments
    {

        /// <summary>
        /// Returns clubs of a particuliar association from a list of clubs
        /// </summary>
        /// <param name="clubs"></param>
        /// <param name="association"></param>
        /// <returns></returns>
        public static List<Club> FilterAssociation(List<Club> clubs, Association association)
        {
            List<Club> clubsAssociation = new List<Club>();
            foreach (Club c in clubs)
            {
                if (association.ContainsAssociation(c.Association()))
                {
                    clubsAssociation.Add(c);
                }
            }
            return clubsAssociation;
        }


        /// <summary>
        /// Return true if a team of a specific club is inside a list
        /// </summary>
        /// <param name="clubs"></param>
        /// <param name="club"></param>
        /// <returns></returns>
        public static bool ContainsTeamOfClub(List<Club> clubs, Club club)
        {
            bool res = false;
            foreach (Club c in clubs)
            {
                if (c == club || ((c as ReserveClub != null) && (c as ReserveClub).FannionClub == club))
                {
                    res = true;
                }
            }
            return res;
        }

        public static int GetClubLevelInLeaguesHierarchy(Club club, List<Club>[] leagues)
        {
            int res = -1;
            for (int i = 0; i < leagues.Length && res == -1; i++)
            {
                res = leagues[i].Contains(club) ? i : res;
            }
            return res;
        }

    }
}
