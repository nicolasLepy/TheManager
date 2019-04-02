using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{

    public class Heure
    {
        public int Heures { get; set; }
        public int Minutes { get; set; }
    }

    public class Calendrier
    {
        public static List<Match> GenererCalendrier(List<Club> clubs, ProgrammationTour programmation, bool allerRetour)
        {
            List<Match> res = new List<Match>();
            bool ghost = false;
            int teams = clubs.Count;
            if (teams % 2 == 1)
            {
                ghost = true;
                teams++;
            }

            int totalRound = teams - 1;
            int matchsPerRound = teams / 2;
            string[,] rounds = new string[totalRound, matchsPerRound];

            for (int round = 0; round < totalRound; round++)
            {
                for (int match = 0; match < matchsPerRound; match++)
                {
                    int home = (round + match) % (teams - 1);
                    int away = (teams - 1 - match + round) % (teams - 1);

                    if (match == 0)
                        away = teams - 1;

                    rounds[round, match] = (home + 1) + " v " + (away + 1);

                }
            }

            string[,] interleaved = new string[totalRound, matchsPerRound];
            int evn = 0;
            int odd = (teams / 2);

            for (int i = 0; i < totalRound; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < matchsPerRound; j++)
                    {
                        interleaved[i, j] = rounds[evn, j];
                    }
                    evn++;
                }
                else
                {
                    for (int j = 0; j < matchsPerRound; j++)
                    {
                        interleaved[i, j] = rounds[odd, j];
                    }
                    odd++;
                }
            }

            rounds = interleaved;

            for (int round = 0; round < totalRound; round++)
            {
                if (round % 2 == 1)
                {
                    rounds[round, 0] = flip(rounds[round, 0]);
                }
            }

            for (int i = 0; i < totalRound; i++)
            {
                List<Match> matchs = new List<Match>();
                for (int j = 0; j < matchsPerRound; j++)
                {

                    string[] compenents = rounds[i, j].Split(new string[] { " v " }, StringSplitOptions.None);
                    int a = int.Parse(compenents[0]);
                    int b = int.Parse(compenents[1]);

                    if (!ghost || (a != teams && b != teams))
                    {

                        //-1 car index dans liste de 0 à n-1, et des clubs de 1 à n
                        Club e1 = clubs[a - 1];
                        Club e2 = clubs[b - 1];

                        //Date du match
                        DateTime jour = new DateTime(Session.Instance.Partie.Date.Year, programmation.JoursDeMatchs[i].Month, programmation.JoursDeMatchs[i].Day, programmation.HeureParDefaut.Heures, programmation.HeureParDefaut.Minutes, 0);
                        if (Utils.EstAvantSansAnnee(jour, programmation.Initialisation)) jour = jour.AddYears(1);
                        Match m = new Match(e1, e2, jour, false);
                        res.Add(m);
                        matchs.Add(m);
                    }
                }
                ProgrammeTV(matchs, programmation.DecalagesTV);
            }
            if(allerRetour)
            {
                //Partie 1 : gestion des journées 2-fin
                int nbJourneesAller = res.Count / matchsPerRound;
                List<Match> matchs = new List<Match>();
                for (int i = 1; i< nbJourneesAller; i++)
                {
                    matchs = new List<Match>();
                    for(int j = 0; j< matchsPerRound; j++)
                    {
                        Match mbase = res[matchsPerRound * i + j];
                        DateTime jour = new DateTime(Session.Instance.Partie.Date.Year, programmation.JoursDeMatchs[nbJourneesAller+i-1].Month, programmation.JoursDeMatchs[nbJourneesAller + i-1].Day, programmation.HeureParDefaut.Heures, programmation.HeureParDefaut.Minutes, 0);
                        if (Utils.EstAvantSansAnnee(jour, programmation.Initialisation)) jour = jour.AddYears(1);
                        Match retour = new Match(mbase.Exterieur, mbase.Domicile, jour, false);
                        matchs.Add(retour);
                        res.Add(retour);
                    }
                    ProgrammeTV(matchs, programmation.DecalagesTV);
                }
                //Dernière journée : première journée inversée
                matchs = new List<Match>();
                for (int i = 0; i<matchsPerRound; i++)
                {
                    Match mbase = res[i];
                    DateTime jour = new DateTime(Session.Instance.Partie.Date.Year, programmation.JoursDeMatchs[programmation.JoursDeMatchs.Count-1].Month, programmation.JoursDeMatchs[programmation.JoursDeMatchs.Count - 1].Day, programmation.HeureParDefaut.Heures, programmation.HeureParDefaut.Minutes, 0);
                    if (Utils.EstAvantSansAnnee(jour, programmation.Initialisation)) jour = jour.AddYears(1);
                    Match retour = new Match(mbase.Exterieur, mbase.Domicile, jour, false);
                    matchs.Add(retour);
                    res.Add(retour);
                }
                ProgrammeTV(matchs, programmation.DecalagesTV);
            }
            return res;
        }

        private static string flip(string match)
        {
            string[] components = match.Split(new string[] { " v " }, StringSplitOptions.None);
            return components[1] + " v " + components[0];
        }

        private static void ProgrammeTV(List<Match> matchs, List<DecalagesTV> decalages)
        {
            int indice = 0;
            matchs.Sort(new Match_Niveau_Comparator());
            foreach (DecalagesTV d in decalages)
            {
                matchs[indice].Jour = matchs[indice].Jour.AddDays(d.DecalageJours);
                //Remise des heures à 0
                matchs[indice].Jour = matchs[indice].Jour.AddHours(-matchs[indice].Jour.Hour);
                matchs[indice].Jour = matchs[indice].Jour.AddMinutes(-matchs[indice].Jour.Minute);
                //Affectation de la nouvelle heure
                matchs[indice].Jour = matchs[indice].Jour.AddHours(d.Heure.Heures);
                matchs[indice].Jour = matchs[indice].Jour.AddMinutes(d.Heure.Minutes);
                indice++;
            }
        }
        
        public static List<Match> TirageAuSort(TourElimination tour)
        {
            List<Match> res = new List<Match>();
            List<Club> pot = new List<Club>(tour.Clubs);
            for (int i = 0; i < tour.Clubs.Count / 2; i++)
            {
                Club dom = TirerClub(pot);
                Club ext = TirerClub(pot);
                DateTime jour = new DateTime(Session.Instance.Partie.Date.Year, tour.Programmation.JoursDeMatchs[0].Month, tour.Programmation.JoursDeMatchs[0].Day, tour.Programmation.HeureParDefaut.Heures, tour.Programmation.HeureParDefaut.Minutes, 0);
                if (Utils.EstAvantSansAnnee(jour, tour.Programmation.Initialisation)) jour = jour.AddYears(1);

                res.Add(new Match(dom, ext, jour, !tour.AllerRetour));
            }

            ProgrammeTV(res, tour.Programmation.DecalagesTV);
            if(tour.AllerRetour)
            {
                List<Match> matchs = new List<Match>();
                List<Match> aller = new List<Match>(res);
                foreach (Match m in aller)
                {
                    DateTime jour = new DateTime(Session.Instance.Partie.Date.Year, tour.Programmation.JoursDeMatchs[0].Month, tour.Programmation.JoursDeMatchs[0].Day, tour.Programmation.HeureParDefaut.Heures, tour.Programmation.HeureParDefaut.Minutes, 0);
                    if (Utils.EstAvantSansAnnee(jour, tour.Programmation.Initialisation)) jour = jour.AddYears(1);
                    Match retour = new Match(m.Exterieur, m.Domicile, jour, !tour.AllerRetour, m);
                    matchs.Add(retour);
                    res.Add(retour);
                }
                ProgrammeTV(matchs, tour.Programmation.DecalagesTV);
            }
            return res;
        }
        
        private static Club TirerClub(List<Club> pot)
        {
            Club res = pot[Session.Instance.Random(0, pot.Count)];
            pot.Remove(res);
            return res;
        }
    }
}