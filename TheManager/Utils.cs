using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Utils
    {

        public readonly static int beginningYear = 2019;
        public readonly static string imagesFolderName = "Images";
        public readonly static string tournamentLogoFolderName = "Logo";
        public readonly static string clubLogoFolderName = "Logos";
        public readonly static string nationalFlagsFolderName = "Drapeaux";

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

        public static List<Player> PlayersByPosition(List<Player> joueurs, Position p)
        {
            List<Player> res = new List<Player>();

            foreach (Player j in joueurs)
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

        public static int Points(List<Match> matchs, Club c)
        {
            return (3 * Wins(matchs, c)) + Draws(matchs, c);
        }

        public static int Played(List<Match> matchs, Club c)
        {
            return Wins(matchs, c) + Draws(matchs, c) + Loses(matchs, c);
        }


        public static int Wins(List<Match> matchs, Club c)
        {
            int res = 0;
            foreach (Match m in matchs)
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
        public static int Loses(List<Match> matchs, Club c)
        {
            int res = 0;
            foreach (Match m in matchs)
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
        public static int Draws(List<Match> matchs, Club c)
        {
            int res = 0;
            foreach (Match m in matchs)
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

        public static int Gf(List<Match> matchs, Club c)
        {
            int res = 0;
            foreach (Match m in matchs)
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

        public static int Ga(List<Match> matchs, Club c)
        {
            int res = 0;
            foreach (Match m in matchs)
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

        public static int Difference(List<Match> games, Club c)
        {
            return Gf(games, c) - Ga(games, c);
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
                Console.WriteLine("Try to get a logo of club but club is null");
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
            return System.IO.Directory.GetCurrentDirectory() + "\\Musiques\\" + song + ".wav";
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

        public static List<Qualification> AdjustQualificationsToNotPromoteReserves(List<Qualification> initialQualifications, List<Club> ranking, Tournament from)
        {
            List<Qualification> qualifications = new List<Qualification>(initialQualifications);

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
            }



            return qualifications;
        }
    }
}