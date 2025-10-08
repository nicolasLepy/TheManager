using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static tm.Tournament;

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

        /// <summary>
        /// Sample n teams from leagues described in  leagueCupApparitions
        /// </summary>
        /// <param name="leagueCupApparitions">Lists of data object referencing tournaments and teams available. The list is assumed to be sorted (best in top)</param>
        /// <param name="teamsToSamples">Number of teams to sample</param>
        /// <returns></returns>
        public static List<LeagueCupApparition> SampleTeams(List<LeagueCupApparition> leagueCupApparitions, int teamsToSamples)
        {
            List<LeagueCupApparition> res = new List<LeagueCupApparition>();
            double chance = 0.9;
            int remainingTeams = teamsToSamples;
            //Sample aléatoirement les équipes jusqu'à ce que le compte soit bon.
            //La première division représentée à un taux de sélection de 0.9, la seconde 0.9*2/3, ...
            for(int i = 0; i < leagueCupApparitions.Count; i++)
            {
                int teams = (int)Math.Floor(leagueCupApparitions[i].teams * chance);
                if(remainingTeams - teams < 0)
                {
                    teams = remainingTeams;
                }
                remainingTeams = remainingTeams - teams;
                res.Add(new LeagueCupApparition(teams, leagueCupApparitions[i].apparitionRound, leagueCupApparitions[i].tournament));
                chance *= (0.8-(i*0.05));
                chance = Math.Max(chance, 0);
            }
            //Si le compte n'est pas bon, on complète avec les équipes qui n'ont pas été sélectionnées dans l'ordre des divisions
            if(remainingTeams > 0)
            {
                for(int i = 0; i < leagueCupApparitions.Count && remainingTeams > 0; i++)
                {
                    int extraTeams = leagueCupApparitions[i].teams - res[i].teams;
                    if(remainingTeams - extraTeams < 0)
                    {
                        extraTeams = remainingTeams;
                    }
                    res[i].teams = res[i].teams + extraTeams;
                    remainingTeams = remainingTeams - extraTeams;
                }
            }
            if(remainingTeams != 0)
            {
                throw new Exception(String.Format("Could not sample {0} teams.", teamsToSamples));
            }
            return res;
        }

    }
}
