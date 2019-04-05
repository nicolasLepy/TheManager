using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Utils
    {

        public static bool ComparerDates(DateTime a, DateTime b)
        {
            bool res = false;

            if (a.Year == b.Year && a.Month == b.Month && a.Day == b.Day) res = true;
            return res;
        }
        
        public static bool ComparerDatesSansAnnee(DateTime a, DateTime b)
        {
            bool res = false;

            if (a.Month == b.Month && a.Day == b.Day) res = true;
            return res;
        }

        public static bool EstAvantSansAnnee(DateTime a, DateTime b)
        {
            bool res = false;

            if (a.Month < b.Month) res = true;
            else if (a.Month == b.Month && a.Day < b.Day) res = true;

            return res;
        }

        public static List<Joueur> JoueursPoste(List<Joueur> joueurs, Poste p)
        {
            List<Joueur> res = new List<Joueur>();

            foreach (Joueur j in joueurs) if (j.Poste == p) res.Add(j);
        
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
    }
}