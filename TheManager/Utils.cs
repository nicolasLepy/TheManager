using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Utils
    {

        public static int DaysNumberBetweenTwoDates(DateTime a, DateTime b)
        {
            TimeSpan ts = a - b;

            return Math.Abs(ts.Days);
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

        public static List<Joueur> PlayersByPoste(List<Joueur> joueurs, Position p)
        {
            List<Joueur> res = new List<Joueur>();

            foreach (Joueur j in joueurs)
            {
                if (j.Poste == p)
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

        public static float Distance(Ville a, Ville b)
        {
            float lat1 = a.Position.Latitude;
            float lon1 = a.Position.Longitude;
            float lat2 = b.Position.Latitude;
            float lon2 = b.Position.Longitude;

            int R = 6371;
            double dLat = Deg2Rad(lat2 - lat1);
            double dLon = Deg2Rad(lon2 - lon1);
            double va = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(va), Math.Sqrt(1 - va));
            double d = R * c;
            return (float)d;
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
                if (m.Domicile == c)
                {
                    if (m.Score1 > m.Score2)
                    {
                        res++;
                    }
                }

                if (m.Exterieur == c)
                {
                    if (m.Score1 < m.Score2)
                    {
                        res++;
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
                if (m.Domicile == c)
                {
                    if (m.Score1 < m.Score2)
                    {
                        res++;
                    }
                }

                if (m.Exterieur == c)
                {
                    if (m.Score1 > m.Score2)
                    {
                        res++;
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
                if ((m.Domicile == c || m.Exterieur == c) && m.Score1 == m.Score2)
                {
                    res++;
                }
            }
            return res;
        }

        public static int Gf(List<Match> matchs, Club c)
        {
            int res = 0;
            foreach (Match m in matchs)
            {
                if (m.Domicile == c)
                {
                    res += m.Score1;
                }

                if (m.Exterieur == c)
                {
                    res += m.Score2;
                }
            }
            return res;
        }

        public static int Ga(List<Match> matchs, Club c)
        {
            int res = 0;
            foreach (Match m in matchs)
            {
                if (m.Domicile == c)
                {
                    res += m.Score2;
                }

                if (m.Exterieur == c)
                {
                    res += m.Score1;
                }
            }
            return res;
        }

        public static int Difference(List<Match> games, Club c)
        {
            return Gf(games, c) - Ga(games, c);
        }

        public static string Logo(Club c)
        {
            string res = "";
            if (c != null)
            {
                res = System.IO.Directory.GetCurrentDirectory() + "\\Output\\Logos\\" + c.logo + ".png";
            }
            else
                Console.WriteLine(c.name + " n'a pas de logo valide");
            return res;
        }

        public static string Image(string image)
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\Images\\" + image;
        }

        public static string LogoTournament(Competition tournament)
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\Images\\Logo\\" + tournament.Logo + ".png";
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
            }
            return res;
        }
    }
}