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

        /// <summary>
        /// Génère l'heure d'un match en fonction du niveau d'un club
        /// </summary>
        /// <param name="niveau">Niveau du championnat du club</param>
        public static void Heure(Match match)
        {
            if (_niveau2 == null) ConstruireTables();
            List<int> niveau = _niveau2;
            if(match.Domicile.Championnat != null)
            {
                switch(match.Domicile.Championnat.Niveau)
                {
                    case 1: niveau = _niveau2; break;
                    case 2: niveau = _niveau2; break;
                    case 3: niveau = _niveau2; break;
                    case 4: niveau = _niveau2; break;
                    case 5: niveau = _niveau2; break;
                    case 6: niveau = _niveau6; break;
                    case 7 : niveau = _niveau6; break;
                    default: niveau = _niveau6;break;
                }
            }
            int heureInt = niveau[Session.Instance.Random(0, _niveau2.Count)];
            int jourDecalage = heureInt % 10 - 1;
            int heure = heureInt / 1000;
            int minute = (heureInt / 10) % 100;
            match.Jour = match.Jour.AddDays(jourDecalage);
            match.Jour = match.Jour.AddHours(heure - match.Jour.Hour);
            match.Jour = match.Jour.AddMinutes(minute - match.Jour.Minute);

        }

        private static List<int> _niveau2;
        private static List<int> _niveau3;
        private static List<int> _niveau4;
        private static List<int> _niveau5;
        private static List<int> _niveau6;
        private static List<int> _niveau7;

        private static void ConstruireTables()
        {
            _niveau2 = new List<int>();
            _niveau3 = new List<int>();
            _niveau4 = new List<int>();
            _niveau5 = new List<int>();
            _niveau6 = new List<int>();
            _niveau7 = new List<int>();
            for (int i = 0; i < 2; i++) _niveau2.Add(18000);
            for (int i = 0; i < 1; i++) _niveau2.Add(19000);
            for (int i = 0; i < 1; i++) _niveau2.Add(20000);
            for (int i = 0; i < 4; i++) _niveau2.Add(14001);
            for (int i = 0; i < 3; i++) _niveau2.Add(14301);
            for (int i = 0; i < 9; i++) _niveau2.Add(15001);
            for (int i = 0; i < 2; i++) _niveau2.Add(16001);
            for (int i = 0; i < 10; i++) _niveau2.Add(17001);
            for (int i = 0; i < 25; i++) _niveau2.Add(18001);
            for (int i = 0; i < 11; i++) _niveau2.Add(18301);
            for (int i = 0; i < 8; i++) _niveau2.Add(19001);
            for (int i = 0; i < 5; i++) _niveau2.Add(20001);
            for (int i = 0; i < 1; i++) _niveau2.Add(13302);
            for (int i = 0; i < 4; i++) _niveau2.Add(14002);
            for (int i = 0; i < 7; i++) _niveau2.Add(15002);
            for (int i = 0; i < 3; i++) _niveau2.Add(16002);
            for (int i = 0; i < 2; i++) _niveau2.Add(17002);
            for (int i = 0; i < 1; i++) _niveau2.Add(20002);
            for (int i = 0; i < 1; i++) _niveau2.Add(20452);
            for (int i = 0; i < 1; i++) _niveau2.Add(20303);

            for (int i = 0; i < 10; i++) _niveau6.Add(15001);
            for (int i = 0; i < 2; i++) _niveau6.Add(15301);
            for (int i = 0; i < 6; i++) _niveau6.Add(16001);
            for (int i = 0; i < 1; i++) _niveau6.Add(16301);
            for (int i = 0; i < 9; i++) _niveau6.Add(17001);
            for (int i = 0; i < 1; i++) _niveau6.Add(17301);
            for (int i = 0; i < 18; i++) _niveau6.Add(18001);
            for (int i = 0; i < 5; i++) _niveau6.Add(18301);
            for (int i = 0; i < 4; i++) _niveau6.Add(19001);
            for (int i = 0; i < 2; i++) _niveau6.Add(19301);
            for (int i = 0; i < 1; i++) _niveau6.Add(20001);
            for (int i = 0; i < 3; i++) _niveau6.Add(13002);
            for (int i = 0; i < 7; i++) _niveau6.Add(13302);
            for (int i = 0; i < 14; i++) _niveau6.Add(14002);
            for (int i = 0; i < 8; i++) _niveau6.Add(14302);
            for (int i = 0; i < 66; i++) _niveau6.Add(15002);
            for (int i = 0; i < 10; i++) _niveau6.Add(15302);
            for (int i = 0; i < 4; i++) _niveau6.Add(16002);
            for (int i = 0; i < 1; i++) _niveau6.Add(16302);
            for (int i = 0; i < 2; i++) _niveau6.Add(17002);
            for (int i = 0; i < 1; i++) _niveau6.Add(17302);
            for (int i = 0; i < 1; i++) _niveau6.Add(18002);

        }

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
                ProgrammeTV(matchs, programmation.DecalagesTV, i+1);
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
                    if(nbJourneesAller-i >= programmation.DernieresJourneesMemeJour)
                        ProgrammeTV(matchs, programmation.DecalagesTV, nbJourneesAller + i);
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
                if(programmation.DernieresJourneesMemeJour < 1)
                    ProgrammeTV(matchs, programmation.DecalagesTV, nbJourneesAller*2);
            }
            return res;
        }

        private static string flip(string match)
        {
            string[] components = match.Split(new string[] { " v " }, StringSplitOptions.None);
            return components[1] + " v " + components[0];
        }

        private static void ProgrammeTV(List<Match> matchs, List<DecalagesTV> decalages, int journee)
        {
            int indice = 0;
            matchs.Sort(new Match_Niveau_Comparator());
            foreach (DecalagesTV d in decalages)
            {
                bool prisEnCompte = true;
                if (d.Probabilite != 1) prisEnCompte = (Session.Instance.Random(1, d.Probabilite + 1) == 1) ? true : false;
                if (d.Journee != 0) prisEnCompte = (d.Journee == journee) ? true : false;
                if(indice < matchs.Count && prisEnCompte)
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
            //Les autres matchs sont programmés plus randomly
            while(indice < matchs.Count)
            {
                Heure(matchs[indice]);
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

                if(tour.Regles.Contains(Regle.RECOIT_SI_DEUX_DIVISION_ECART))
                {
                    Competition champD = dom.Championnat;
                    Competition champE = ext.Championnat;
                    if((champD != null && champE != null) && champE.Niveau - champD.Niveau >= 2)
                    {
                        Club temp = dom;
                        dom = ext;
                        ext = temp;
                    }
                }

                res.Add(new Match(dom, ext, jour, !tour.AllerRetour));
            }

            ProgrammeTV(res, tour.Programmation.DecalagesTV,0);
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
                ProgrammeTV(matchs, tour.Programmation.DecalagesTV, 0);
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