using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TheManager.Algorithms;
using TheManager.Comparators;

namespace TheManager
{
    public static class Utils
    {

        public readonly static int beginningYear = 2019;
        public readonly static string imagesFolderName = "images";
        public readonly static string tournamentLogoFolderName = "tournaments";
        public readonly static string clubLogoFolderName = "clubs";
        public readonly static string nationalFlagsFolderName = "flags";
        public readonly static string mediaLogoFolderName = "medias";
        public readonly static string namesSubfolderName = "names";
        public readonly static string dataFolderName = "data";
        public readonly static string musicFolderName = "music";

        public readonly static string friendlyTournamentName = "Matchs amicaux";
        public static int DaysNumberBetweenTwoDates(DateTime a, DateTime b)
        {
            TimeSpan ts = a - b;

            return Math.Abs(ts.Days);
        }

        public static float GetStars(float notation)
        {
            float stars;
            float level = notation;
            if (level < 40)
            {
                stars = 0.5f;
            }
            else if (level < 50)
            {
                stars = 1f;
            }
            else if (level < 57)
            {
                stars = 1.5f;
            }
            else if (level < 62)
            {
                stars = 2f;
            }
            else if (level < 66)
            {
                stars = 2.5f;
            }
            else if (level < 69)
            {
                stars = 3f;
            }
            else if (level < 72)
            {
                stars = 3.5f;
            }
            else if (level < 75)
            {
                stars = 4f;
            }
            else if (level < 79)
            {
                stars = 4.5f;
            }
            else
            {
                stars = 5f;
            }

            return stars;

        }

        public static bool CompareDates(DateTime a, DateTime b)
        {
            bool res = a.Year == b.Year && a.Month == b.Month && a.Day == b.Day;

            return res;
        }
        
        public static bool CompareDatesWithoutYear(DateTime a, DateTime b)
        {
            bool res = a.Month == b.Month && a.Day == b.Day;
            return res;
        }

        public static bool IsBefore(DateTime a, DateTime b)
        {
            bool res = false;

            if (a.Year < b.Year)
            {
                res = true;
            }
            else if (a.Year == b.Year && a.Month < b.Month)
            {
                res = true;
            }
            else if (a.Year == b.Year && a.Month == b.Month && a.Day < b.Day)
            {
                res = true;
            }

            return res;
        }

        public static bool IsBeforeWithoutYear(DateTime a, DateTime b)
        {
            bool res = false;

            if (a.Month < b.Month)
            {
                res = true;
            }
            else if (a.Month == b.Month && a.Day < b.Day)
            {
                res = true;
            }

            return res;
        }

        public static List<Player> PlayersByPosition(List<Player> players, Position p)
        {
            List<Player> res = new List<Player>();

            foreach (Player j in players)
            {
                if (j.position == p)
                {
                    res.Add(j);
                }
            }
        
            return res;
        }

        public static List<E> ShuffleList<E>(List<E> list)
        {
            List<E> res = new List<E>();

            int random = 0;
            while (list.Count > 0)
            {
                random = Session.Instance.Random(0, list.Count);
                res.Add(list[random]);
                list.RemoveAt(random);
            }

            return res;
        }

        public static double Deg2Rad(float deg)
        { 
            return (float)(deg * (Math.PI / 180.0f));
        }

        public static float Distance(City a, City b)
        {
            return Distance(a.Position, b.Position);
        }

        public static float Distance(GeographicPosition a, GeographicPosition b)
        {
            float lat1 = a.Latitude;
            float lon1 = a.Longitude;
            float lat2 = b.Latitude;
            float lon2 = b.Longitude;

            int R = 6371;
            double dLat = Deg2Rad(lat2 - lat1);
            double dLon = Deg2Rad(lon2 - lon1);
            double va = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(va), Math.Sqrt(1 - va));
            double d = R * c;
            return (float)d;
        }

        public static List<Match> MatchesOfClub(List<Match> matches, Club c, RankingType rankingType)
        {
            List<Match> res = new List<Match>();
            foreach(Match m in matches)
            {
                if((m.home == c && rankingType != RankingType.Away) || (m.away == c && rankingType != RankingType.Home))
                {
                    res.Add(m);
                }
            }
            return res;
        }

        public static int Points(List<Match> matchs, Club c, RankingType rankingType = RankingType.General)
        {
            return (3 * Wins(matchs, c, rankingType)) + Draws(matchs, c, rankingType);
        }

        public static int Played(List<Match> matchs, Club c, RankingType rankingType = RankingType.General)
        {
            return Wins(matchs, c, rankingType) + Draws(matchs, c, rankingType) + Loses(matchs, c, rankingType);
        }

        public static int Wins(List<Match> matchs, Club c, RankingType rankingType = RankingType.General)
        {
            int res = 0;
            foreach (Match m in MatchesOfClub(matchs, c, rankingType))
            {
                if (m.Played)
                {
                    if (m.home == c)
                    {
                        if (m.score1 > m.score2)
                        {
                            res++;
                        }
                    }
                    if (m.away == c)
                    {
                        if (m.score1 < m.score2)
                        {
                            res++;
                        }
                    }
                }
            }
            return res;
        }
        public static int Loses(List<Match> matchs, Club c, RankingType rankingType = RankingType.General)
        {
            int res = 0;
            foreach (Match m in MatchesOfClub(matchs, c, rankingType))
            {
                if (m.Played)
                {
                    if (m.home == c)
                    {
                        if (m.score1 < m.score2)
                        {
                            res++;
                        }
                    }

                    if (m.away == c)
                    {
                        if (m.score1 > m.score2)
                        {
                            res++;
                        }
                    }
                }
            }
            return res;
        }
        public static int Draws(List<Match> matchs, Club c, RankingType rankingType = RankingType.General)
        {
            int res = 0;
            foreach (Match m in MatchesOfClub(matchs, c, rankingType))
            {
                if (m.Played)
                {
                    if ((m.home == c || m.away == c) && m.score1 == m.score2)
                    {
                        res++;
                    }
                }
            }
            return res;
        }

        public static int Gf(List<Match> matchs, Club c, RankingType rankingType = RankingType.General)
        {
            int res = 0;
            foreach (Match m in MatchesOfClub(matchs, c, rankingType))
            {
                if (m.home == c)
                {
                    res += m.score1;
                }

                if (m.away == c)
                {
                    res += m.score2;
                }
            }
            return res;
        }

        public static int Ga(List<Match> matchs, Club c, RankingType rankingType = RankingType.General)
        {
            int res = 0;
            foreach (Match m in MatchesOfClub(matchs, c, rankingType))
            {
                if (m.home == c)
                {
                    res += m.score2;
                }

                if (m.away == c)
                {
                    res += m.score1;
                }
            }
            return res;
        }

        public static int Difference(List<Match> games, Club c, RankingType rankingType = RankingType.General)
        {
            return Gf(games, c, rankingType) - Ga(games, c, rankingType);
        }

        public static string MediaLogo(Media m)
        {
            return Environment.CurrentDirectory + "\\" + Utils.imagesFolderName + "\\" + mediaLogoFolderName + "\\" + m.name.Replace(" ","") + ".png";
        }

        public static string Flag(Country c)
        {
            string flag = Environment.CurrentDirectory + "\\" + Utils.imagesFolderName + "\\"+ nationalFlagsFolderName + "\\" + c.Flag + ".png";
            if (!File.Exists(flag))
            {
                flag = System.IO.Directory.GetCurrentDirectory() + "\\" + imagesFolderName + "\\" + clubLogoFolderName + "\\" + "generic.png";
            }
            return flag;
        }

        public static string Logo(Club c)
        {
            string res = "";
            if (c != null)
            {
                string logoString = c.logo;
                if(logoString == "")
                {
                    logoString = "generic";
                }
                string folder = clubLogoFolderName;
                if(c as NationalTeam != null)
                {
                    folder = nationalFlagsFolderName;
                }
                res = System.IO.Directory.GetCurrentDirectory() + "\\" + imagesFolderName + "\\"+folder+"\\" + logoString + ".png";
                if (!File.Exists(res))
                {
                    res = System.IO.Directory.GetCurrentDirectory() + "\\" + imagesFolderName + "\\" + clubLogoFolderName + "\\" + "generic.png";
                }
            }
            else
            {
                Utils.Debug("Try to get a logo of club but club is null");
            }
            return res;
        }

        public static string Image(string imageName)
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\" + imageName;
        }

        public static string LogoTournament(Tournament tournament)
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\"+Utils.tournamentLogoFolderName+"\\" + tournament.logo + ".png";
        }

        public static string PathSong(string song)
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\"+ musicFolderName + "\\" + song + ".wav";
        }

        public static bool RetoursContient(RetourMatchEvenement evenement, List<RetourMatch> retours)
        {
            bool res = false;
            foreach(RetourMatch rm in retours)
            {
                if (rm.Evenement == evenement)
                {
                    res = true;
                }
            }
            return res;
        }

        public static string Rule2String(Rule rule)
        {
            string res = "";
            switch (rule)
            {
                case Rule.AtHomeIfTwoLevelDifference:
                    res = "Le club reçoit s'il évolue au moins deux divisions en dessous";
                    break;
                case Rule.OnlyFirstTeams:
                    res = "Seulement les équipes premières peuvent entrer";
                    break;
                case Rule.ReservesAreNotPromoted:
                    res = "Les réserves ne peuvent pas monter";
                    break; 
                default:
                    break;
            }
            return res;
        }

        public static void Debug(string str)
        {
            Console.WriteLine(str);
        }

        [Flags]
        private enum RuleStatus
        {
            RuleRespected = 1,
            RuleNotRespected = 2,
            RuleRelegation = 4
        }

        private static RuleStatus RuleIsRespected(Club club, Qualification qualification, int baseLevel, bool reservesCantBePromoted)
        {
            //Rule 1 : Reserve can't be promoted
            //Rule 2 : Reserve can't be promoted if another reserve of the club is in the higher division
            //Rule 3 : Reserve is relegated if another reserve of the club in the higher division is relegated
            ReserveClub clubReserve = club as ReserveClub;
            bool resRule2 = true;
            bool resRule3 = false;
            if (clubReserve != null)
            {
                int reserveCount = clubReserve.FannionClub.reserves.IndexOf(clubReserve);
                if(reserveCount > 0)
                {
                    ReserveClub upperReserve = clubReserve.FannionClub.reserves[reserveCount - 1];
                    Tournament up = club.Country().League(baseLevel - 1);
                    foreach(Round r in up.rounds)
                    {
                        if(r.clubs.Contains(upperReserve))
                        {
                            List<Qualification> upperQualifications = null;
                            List<Club> upperRanking = null;
                            ChampionshipRound cr = r as ChampionshipRound;
                            GroupsRound gr = r as GroupsRound;
                            InactiveRound ir = r as InactiveRound;
                            if(cr != null)
                            {
                                upperQualifications = cr.GetQualifications();
                                upperRanking = cr.Ranking();
                            }
                            if(gr != null)
                            {
                                int group = 0;
                                for(int i = 0; i<gr.groupsCount; i++)
                                {
                                    if(gr.groups[i].Contains(upperReserve))
                                    {
                                        group = i;
                                    }
                                }
                                upperQualifications = gr.GetGroupQualifications(group);
                                upperRanking = gr.Ranking(group);
                            }
                            if (ir != null)
                            {
                                upperQualifications = ir.GetQualifications();
                                upperRanking = ir.Ranking();
                            }
                            Qualification reserveQualification = upperQualifications[upperRanking.IndexOf(upperReserve)];
                            if(reserveQualification.isNextYear && reserveQualification.roundId == 0 && reserveQualification.tournament.level == r.Tournament.level && qualification.tournament.level < baseLevel)
                            {
                                resRule2 = false;
                            }
                            if(reserveQualification.isNextYear && reserveQualification.roundId == 0 && reserveQualification.tournament.level > r.Tournament.level)
                            {
                                resRule3 = true;
                            }
                        }
                    }
                }
            }

            bool isReserve = clubReserve != null;
            bool resRule1 = !reservesCantBePromoted || (!isReserve || (isReserve && qualification.tournament.isChampionship && qualification.tournament.level >= baseLevel));
            return ((resRule1 && resRule2) ? RuleStatus.RuleRespected : RuleStatus.RuleNotRespected) | (resRule3 ? RuleStatus.RuleRelegation : 0);
        }

        public static List<Qualification> AdjustQualificationsToNotPromoteReserves(List<Qualification> initialQualifications, List<Club> ranking, Tournament from, bool reservesCantBePromoted)
        {

            List<Qualification> qualifications = new List<Qualification>(initialQualifications);
            List<int> fixedRelegations = new List<int>(); //Contains ranking of teams that can't be saved

            for (int i = 0; i<qualifications.Count; i++)
            {
                qualifications.Sort(new QualificationComparator());
                Qualification q = qualifications[i];
                Club concernedClub = ranking[q.ranking - 1];
                RuleStatus ruleStatus = RuleIsRespected(concernedClub, q, from.level, reservesCantBePromoted);
                bool ok = ruleStatus.HasFlag(RuleStatus.RuleRespected);
                bool toRelegate = ruleStatus.HasFlag(RuleStatus.RuleRelegation);
                if (toRelegate)
                {
                    ok = q.tournament.level >= from.level;
                }
                int j = i;
                if (from.isChampionship)
                {
                    while (!ok)
                    {
                        int tempValue = qualifications[j].ranking;
                        Qualification first = qualifications[j];
                        Qualification second = qualifications[j + 1];
                        first.ranking = qualifications[j + 1].ranking;
                        second.ranking = tempValue;
                        qualifications[j] = first;
                        qualifications[j + 1] = second;
                        ruleStatus = RuleIsRespected(concernedClub, qualifications[j], from.level, reservesCantBePromoted);
                        ok = ruleStatus.HasFlag(RuleStatus.RuleRespected);
                        j++;
                    }
                }
                if (toRelegate && qualifications[j].tournament.level == from.level)
                {
                    int rankingConcernedClub = qualifications[j].ranking;
                    int rankingFirstRelegated = -1;
                    int indexQualificationFirstRelegated = -1;
                    for(int k = 0; k < qualifications.Count && rankingFirstRelegated == -1; k++)
                    {
                        if(qualifications[k].tournament.level > from.level && !fixedRelegations.Contains(qualifications[k].ranking))
                        {
                            rankingFirstRelegated = qualifications[k].ranking;
                            indexQualificationFirstRelegated = k;
                        }
                    }
                    if (rankingFirstRelegated > -1)
                    {
                        fixedRelegations.Add(qualifications[j].ranking);
                        Qualification first = qualifications[j];
                        Qualification second = qualifications[indexQualificationFirstRelegated];
                        first.ranking = qualifications[indexQualificationFirstRelegated].ranking;
                        second.ranking = qualifications[j].ranking;
                        qualifications[j] = first;
                        qualifications[indexQualificationFirstRelegated] = second;
                    }
                }
            }

            /*
            for (int j = 0; j < qualifications.Count; j++)
            {
                Qualification q = qualifications[j];
                //If the two tournaments involved are championship and the level of the destination is higher in league structure than the current league
                if (from.isChampionship && q.tournament.isChampionship && q.tournament.level < from.level)
                {
                    int offset = 0;
                    bool pursue = true;
                    while (pursue && j + offset < qualifications.Count)
                    {
                        //This is a reserve club so it must not be promoted
                        if (ranking[q.ranking - 1 + offset] as ReserveClub != null)
                        {
                            offset++;
                        }
                        else
                        {
                            pursue = false;
                            //If there is an offset, make a swap
                            if (offset > 0)
                            {
                                Qualification first = qualifications[j];
                                Qualification second = qualifications[j + offset];
                                int tempRanking = second.ranking;
                                second.ranking = first.ranking;
                                first.ranking = tempRanking;
                                qualifications[j] = second;
                                qualifications[j + offset] = first;
                            }
                        }
                    }
                }
            }*/

            return qualifications;
        }


        /// <summary>
        /// Split clubs in equal-size geographic clusters using KMeans clustering algorithmn
        /// </summary>
        /// <param name="clubs">List of clubs</param>
        /// <param name="clustersCount">Clusters count</param>
        /// <returns></returns>
        public static List<Club>[] CreateGeographicClusters(List<Club> clubs, int clustersCount)
        {
            KMeansClustering kmeans = new KMeansClustering(clubs, clustersCount);
            return kmeans.CreateClusters();
        }

        /*
        private static void ToStringList<T>(List<T> list)
        {
            foreach (T e in list)
            {
                Console.Write(e.ToString() + "-");
            }
            Console.WriteLine("");
        }*/

        public static string FormatMoney(float money)
        {
            bool negative = money < 0;
            if(money < 0)
            {
                money = -money;
            }
            float i = (float)Math.Pow(10, (int)Math.Max(0, Math.Log10(money) - 2));
            money = money / i * i;

            if (money >= 1000000000)
            {
                return (money / 1000000000D).ToString("0.##") + "B €";
            }
            if (money >= 1000000)
            {
                return (money / 1000000D).ToString("0.##") + "M €";
            }
            if (money >= 1000)
            {
                return (money / 1000D).ToString("0.##") + "K €";
            }

            return negative ? "-" : "" + money.ToString("#,0") + " €";
        }

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        public static int[] GetClustersCapacity(int clubsCount, int clusterCount)
        {
            int[] res = new int[clusterCount];
            int minElementByCluster = clubsCount / clusterCount;
            int clusterWithAdditionnalElement = clubsCount % clusterCount;
            for (int i = 0; i < clusterCount; i++)
            {
                res[i] = minElementByCluster + (i < clusterWithAdditionnalElement ? 1 : 0);
            }
            return res;
        }

    }
}