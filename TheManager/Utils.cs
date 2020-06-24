using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Utils
    {

        public static int NombreJoursEntreDeuxDates(DateTime a, DateTime b)
        {
            TimeSpan ts = a - b;

            return Math.Abs(ts.Days);
        }

        public static bool ComparerDates(DateTime a, DateTime b)
        {
            bool res = a.Year == b.Year && a.Month == b.Month && a.Day == b.Day;

            return res;
        }
        
        public static bool ComparerDatesSansAnnee(DateTime a, DateTime b)
        {
            bool res = a.Month == b.Month && a.Day == b.Day;
            return res;
        }

        public static bool EstAvantSansAnnee(DateTime a, DateTime b)
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

        public static List<Joueur> JoueursPoste(List<Joueur> joueurs, Poste p)
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

        public static List<E> MelangerListe<E>(List<E> liste)
        {
            List<E> res = new List<E>();

            int random = 0;
            while (liste.Count > 0)
            {
                random = Session.Instance.Random(0, liste.Count);
                res.Add(liste[random]);
                liste.RemoveAt(random);
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

        public static float Distance(Position a, Position b)
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
            return (3 * Gagnes(matchs, c)) + Nuls(matchs, c);
        }

        public static int Joues(List<Match> matchs, Club c)
        {
            return Gagnes(matchs, c) + Nuls(matchs, c) + Perdus(matchs, c);
        }


        public static int Gagnes(List<Match> matchs, Club c)
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
        public static int Perdus(List<Match> matchs, Club c)
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
        public static int Nuls(List<Match> matchs, Club c)
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

        public static int Bp(List<Match> matchs, Club c)
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

        public static int Bc(List<Match> matchs, Club c)
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

        public static int Difference(List<Match> matchs, Club c)
        {
            return Bp(matchs, c) - Bc(matchs, c);
        }

        public static string Logo(Club c)
        {
            string res = "";
            if (c != null)
            {
                res = System.IO.Directory.GetCurrentDirectory() + "\\Output\\Logos\\" + c.Logo + ".png";
            }
            else
                Console.WriteLine(c.Nom + " n'a pas de logo valide");
            return res;
        }

        public static string Image(string image)
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\Images\\" + image;
        }

        public static string LogoCompetition(Competition competition)
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\Images\\Logo\\" + competition.Logo + ".png";
        }

        public static string CheminSon(string son)
        {
            return System.IO.Directory.GetCurrentDirectory() + "\\Musiques\\" + son + ".wav";
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

        public static string Regle2String(Rule regle)
        {
            string res = "";
            switch (regle)
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