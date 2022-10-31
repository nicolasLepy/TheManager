using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Algorithms;
using TheManager.Comparators;
using TheManager.Tournaments;

namespace TheManager
{

    /// <summary>
    /// Represent a generic calendar for tournaments with n games but where no calendar is defined
    /// </summary>
    [DataContract(IsReference =true)]
    public class GenericCalendar
    {
        [DataMember]
        private string _name;
        [DataMember]
        private List<GameDay> _gameDays;
        public List<GameDay> GameDays => _gameDays;

        public int NumberOfGames => _gameDays.Count;

        public string Name => _name;

        public GenericCalendar(string name, List<GameDay> gameDays)
        {
            _name = name;
            _gameDays = gameDays;
        }

    }
    public class Hour
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
    }

    public static class Calendar
    {

        /// <summary>
        /// Generate hour of a game in function of club level
        /// </summary>
        public static void Hour(Match match)
        {
            if (_level2 == null)
            {
                ConstructTables();
            }
            List<int> level = _level2;
            if(match.home.Championship != null)
            {
                switch(match.home.Championship.level)
                {
                    case 1: level = _level2; break;
                    case 2: level = _level2; break;
                    case 3: level = _level2; break;
                    case 4: level = _level2; break;
                    case 5: level = _level2; break;
                    case 6: level = _level6; break;
                    case 7 : level = _level6; break;
                    default: level = _level6;break;
                }
            }
            int hourInt = level[Session.Instance.Random(0, _level2.Count)];
            int dayOffset = hourInt % 10 - 1;
            int hour = hourInt / 1000;
            int minute = (hourInt / 10) % 100;
            //match.day = match.day.AddDays(dayOffset);
            match.day = match.day.AddHours(hour - match.day.Hour);
            match.day = match.day.AddMinutes(minute - match.day.Minute);

        }

        private static List<int> _level2;
        private static List<int> _level6;

        private static void ConstructTables()
        {
            _level2 = new List<int>();
            _level6 = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                _level2.Add(18000);
            }
            for (int i = 0; i < 1; i++)
            {
                _level2.Add(19000);
            }
            for (int i = 0; i < 1; i++)
            {
                _level2.Add(20000);
            }
            for (int i = 0; i < 4; i++)
            {
                _level2.Add(14001);
            }
            for (int i = 0; i < 3; i++)
            {
                _level2.Add(14301);
            }
            for (int i = 0; i < 9; i++)
            {
                _level2.Add(15001);
            }
            for (int i = 0; i < 2; i++)
            {
                _level2.Add(16001);
            }
            for (int i = 0; i < 10; i++)
            {
                _level2.Add(17001);
            }
            for (int i = 0; i < 25; i++)
            {
                _level2.Add(18001);
            }
            for (int i = 0; i < 11; i++)
            {
                _level2.Add(18301);
            }
            for (int i = 0; i < 8; i++)
            {
                _level2.Add(19001);
            }
            for (int i = 0; i < 5; i++)
            {
                _level2.Add(20001);
            }
            for (int i = 0; i < 1; i++)
            {
                _level2.Add(13302);
            }
            for (int i = 0; i < 4; i++)
            {
                _level2.Add(14002);
            }
            for (int i = 0; i < 7; i++)
            {
                _level2.Add(15002);
            }
            for (int i = 0; i < 3; i++)
            {
                _level2.Add(16002);
            }
            for (int i = 0; i < 2; i++)
            {
                _level2.Add(17002);
            }
            for (int i = 0; i < 1; i++)
            {
                _level2.Add(20002);
            }
            for (int i = 0; i < 10; i++)
            {
                _level6.Add(15001);
            }
            for (int i = 0; i < 2; i++)
            {
                _level6.Add(15301);
            }
            for (int i = 0; i < 6; i++)
            {
                _level6.Add(16001);
            }
            for (int i = 0; i < 1; i++)
            {
                _level6.Add(16301);
            }
            for (int i = 0; i < 9; i++)
            {
                _level6.Add(17001);
            }
            for (int i = 0; i < 1; i++)
            {
                _level6.Add(17301);
            }
            for (int i = 0; i < 18; i++)
            {
                _level6.Add(18001);
            }
            for (int i = 0; i < 5; i++)
            {
                _level6.Add(18301);
            }
            for (int i = 0; i < 4; i++)
            {
                _level6.Add(19001);
            }
            for (int i = 0; i < 2; i++)
            {
                _level6.Add(19301);
            }
            for (int i = 0; i < 1; i++)
            {
                _level6.Add(20001);
            }
            for (int i = 0; i < 3; i++)
            {
                _level6.Add(13002);
            }
            for (int i = 0; i < 7; i++)
            {
                _level6.Add(13302);
            }
            for (int i = 0; i < 14; i++)
            {
                _level6.Add(14002);
            }
            for (int i = 0; i < 8; i++)
            {
                _level6.Add(14302);
            }
            for (int i = 0; i < 66; i++)
            {
                _level6.Add(15002);
            }
            for (int i = 0; i < 10; i++)
            {
                _level6.Add(15302);
            }
            for (int i = 0; i < 4; i++)
            {
                _level6.Add(16002);
            }
            for (int i = 0; i < 1; i++)
            {
                _level6.Add(16302);
            }
            for (int i = 0; i < 2; i++)
            {
                _level6.Add(17002);
            }
            for (int i = 0; i < 1; i++)
            {
                _level6.Add(17302);
            }
            for (int i = 0; i < 1; i++)
            {
                _level6.Add(18002);
            }
        }

        /// <summary>
        /// Round robin algorithm to generate games
        /// </summary>
        /// <param name="clubs">List of clubs</param>
        /// <param name="programmation">TV / Federation schedule for games</param>
        /// <param name="twoLegged">One or two games</param>
        /// <returns></returns>
        public static List<Match> GenerateCalendar(List<Club> clubs, Round tournamentRound, bool twoLegged)
        {
            RoundProgrammation programmation = tournamentRound.programmation;
            List<Match> res = new List<Match>();
            bool ghost = false;
            int teams = clubs.Count;
            if (teams % 2 == 1)
            {
                ghost = true;
                teams++;
            }

            int totalRound = teams - 1;
            int gamesPerRound = teams / 2;
            string[,] rounds = new string[totalRound, gamesPerRound];

            for (int round = 0; round < totalRound; round++)
            {
                for (int match = 0; match < gamesPerRound; match++)
                {
                    int home = (round + match) % (teams - 1);
                    int away = (teams - 1 - match + round) % (teams - 1);

                    if (match == 0)
                    {
                        away = teams - 1;
                    }

                    rounds[round, match] = (home + 1) + " v " + (away + 1);

                }
            }

            string[,] interleaved = new string[totalRound, gamesPerRound];
            int evn = 0;
            int odd = (teams / 2);

            for (int i = 0; i < totalRound; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < gamesPerRound; j++)
                    {
                        interleaved[i, j] = rounds[evn, j];
                    }
                    evn++;
                }
                else
                {
                    for (int j = 0; j < gamesPerRound; j++)
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
                for (int j = 0; j < gamesPerRound; j++)
                {

                    string[] compenents = rounds[i, j].Split(new string[] { " v " }, StringSplitOptions.None);
                    int a = int.Parse(compenents[0]);
                    int b = int.Parse(compenents[1]);

                    if (!ghost || (a != teams && b != teams))
                    {
                        //-1 car index dans liste de 0 à n-1, et des clubs de 1 à n
                        Club e1 = clubs[a - 1];
                        Club e2 = clubs[b - 1];

                        //Game day
                        DateTime jour = programmation.gamesDays[i].ConvertToDateTime(Session.Instance.Game.date.Year);
                        if (Utils.IsBeforeWithoutYear(jour, tournamentRound.DateInitialisationRound()))
                        {
                            jour = programmation.gamesDays[i].ConvertToDateTime(Session.Instance.Game.date.Year+1);
                        }
                        jour = jour.AddHours(programmation.defaultHour.Hours);
                        jour = jour.AddMinutes(programmation.defaultHour.Minutes);

                        Match m = new Match(e1, e2, jour, false);
                        res.Add(m);
                        matchs.Add(m);
                    }
                }
                TVSchedule(matchs, programmation.tvScheduling, i+1);
            }
            if(twoLegged)
            {
                if(ghost)
                {
                    gamesPerRound--;
                }
                //Part 1 : manager Journey [2-end]
                int nbGamesFirstRound = res.Count / gamesPerRound;
                List<Match> games;
                for (int i = 1; i< nbGamesFirstRound; i++)
                {
                    games = new List<Match>();
                    for(int j = 0; j< gamesPerRound; j++)
                    {
                        Match mbase = res[gamesPerRound * i + j];

                        DateTime jour = programmation.gamesDays[nbGamesFirstRound + i - 1].ConvertToDateTime(Session.Instance.Game.date.Year);
                        if (Utils.IsBeforeWithoutYear(jour, tournamentRound.DateInitialisationRound()))
                        {
                            jour = programmation.gamesDays[nbGamesFirstRound + i - 1].ConvertToDateTime(Session.Instance.Game.date.Year + 1);
                        }
                        jour = jour.AddHours(programmation.defaultHour.Hours);
                        jour = jour.AddMinutes(programmation.defaultHour.Minutes);

                        Match retour = new Match(mbase.away, mbase.home, jour, false);
                        games.Add(retour);
                        res.Add(retour);
                    }

                    if (nbGamesFirstRound - i >= programmation.lastMatchDaysSameDayNumber)
                    {
                        TVSchedule(games, programmation.tvScheduling, nbGamesFirstRound + i);
                    }
                }
                //Last journey : first journey inverted
                games = new List<Match>();
                for (int i = 0; i<gamesPerRound; i++)
                {
                    Match mbase = res[i];

                    DateTime jour = programmation.gamesDays[programmation.gamesDays.Count - 1].ConvertToDateTime(Session.Instance.Game.date.Year);
                    if (Utils.IsBeforeWithoutYear(jour, tournamentRound.DateInitialisationRound()))
                    {
                        jour = programmation.gamesDays[programmation.gamesDays.Count - 1].ConvertToDateTime(Session.Instance.Game.date.Year + 1);
                    }
                    jour = jour.AddHours(programmation.defaultHour.Hours);
                    jour = jour.AddMinutes(programmation.defaultHour.Minutes);
                    Match retour = new Match(mbase.away, mbase.home, jour, false);
                    games.Add(retour);
                    res.Add(retour);
                }

                if (programmation.lastMatchDaysSameDayNumber < 1)
                {
                    TVSchedule(games, programmation.tvScheduling, nbGamesFirstRound*2);
                }

            }

            return res;
        }

        private static string flip(string match)
        {
            string[] components = match.Split(new string[] { " v " }, StringSplitOptions.None);
            return components[1] + " v " + components[0];
        }

        /// <summary>
        /// Compute TVSchedule for games
        /// </summary>
        /// <param name="games">List of games</param>
        /// <param name="offsets">List of offsets for TV diffusion</param>
        /// <param name="day">Championship day</param>
        private static void TVSchedule(List<Match> games, List<TvOffset> offsets, int day)
        {
            games = new List<Match>(games);
            int indice = 0;
            if (offsets.Count>0)
            {
                try
                {
                    games.Sort(new MatchLevelComparator());
                }
                catch
                {
                    Utils.Debug("Match_Niveau_Comparator exception for TV scheduling");
                }
                foreach (TvOffset d in offsets)
                {
                    bool prisEnCompte = true;
                    if (d.Probability != 1)
                    {
                        prisEnCompte = (Session.Instance.Random(1, d.Probability + 1) == 1);
                    }
                    if (d.GameDay != 0)
                    {
                        prisEnCompte = (d.GameDay == day);
                    }
                    if (indice < games.Count && prisEnCompte)
                    {
                        games[indice].primeTimeGame = d.isPrimeTime;
                        games[indice].day = games[indice].day.AddDays(d.DaysOffset);
                        //Set all hours and minutes at 00
                        games[indice].day = games[indice].day.AddHours(-games[indice].day.Hour);
                        games[indice].day = games[indice].day.AddMinutes(-games[indice].day.Minute);
                        //New hour affectation
                        games[indice].day = games[indice].day.AddHours(d.Hour.Hours);
                        games[indice].day = games[indice].day.AddMinutes(d.Hour.Minutes);
                        indice++;
                    }

                }

            }
            //Other games are randomly programmed according to their level
            while (indice < games.Count)
            {
                Hour(games[indice]);
                indice++;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Session.Instance.Random(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static Club[] SwitchTeams(Club home, Club away)
        {
            Tournament champH = home.Championship;
            Tournament champA = away.Championship;
            if ((champH != null && champA != null) && champA.level - champH.level >= 2)
            {
                Club temp = home;
                home = away;
                away = temp;
            }
            return new Club[] { home, away };
        }

        private static DateTime GetRoundProgrammationDate(Round round, RoundProgrammation programmation)
        {
            //Console.WriteLine("PROGRAMMATION " + round.name + " (" + round.Tournament.name + ")");
            DateTime day = programmation.gamesDays[0].ConvertToDateTime(Session.Instance.Game.date.Year);
            //Console.WriteLine("Date initialisation : " + round.DateInitialisationRound().ToString() + " (" + programmation.initialisation.WeekNumber + ")");
            //Console.WriteLine("Date des matchs : " + day.ToString() + " (" + programmation.gamesDays[0].WeekNumber + ")");
            //Console.WriteLine("Date fin : " + round.DateEndRound().ToString() + " (" + programmation.end.WeekNumber + ")");
            if (Utils.IsBeforeWithoutYear(day, round.DateInitialisationRound()))
            {
                //Console.WriteLine("Ajoute une année");
                day = programmation.gamesDays[0].ConvertToDateTime(Session.Instance.Game.date.Year + 1);
            }
            day.AddHours(programmation.defaultHour.Hours);
            day.AddMinutes(programmation.defaultHour.Minutes);
            //Console.WriteLine("FIN PROGRAMMATION\n");
            return day;
        }

        private static void CreateSecondLegKnockOutRound(List<Match> gamesList, Round round, RoundProgrammation programmation)
        {
            List<Match> games = new List<Match>();
            List<Match> firstRound = new List<Match>(gamesList);
            foreach (Match m in firstRound)
            {

                DateTime day = programmation.gamesDays[1].ConvertToDateTime(Session.Instance.Game.date.Year);
                if (Utils.IsBeforeWithoutYear(day, round.DateInitialisationRound()))
                {
                    day = programmation.gamesDays[1].ConvertToDateTime(Session.Instance.Game.date.Year + 1);
                }
                day.AddHours(programmation.defaultHour.Hours);
                day.AddMinutes(programmation.defaultHour.Minutes);

                Match secondRound = new Match(m.away, m.home, day, !round.twoLegs, m);
                games.Add(secondRound);
                gamesList.Add(secondRound);
            }
            TVSchedule(games, round.programmation.tvScheduling, 0);
        }

        /// <summary>
        /// Get list of fixtures when the precedent round is a group round and no random drawing is performed
        /// TODO: Currently only work when there is two qualified teams by group
        /// </summary>
        /// <param name="round">The round to draw</param>
        /// <param name="previousRound">The previous round</param>
        /// <returns></returns>
        public static List<Match> DrawNoRandomDrawing(KnockoutRound round, GroupsRound previousRound)
        {
            //We assume teams are added in round in the order of ranking and group
            List<Match> res = new List<Match>();

            RoundProgrammation programmation = round.programmation;
            for (int i = 0; i < round.clubs.Count / 2; i++)
            {
                Club home = i < round.clubs.Count / 4 ? round.clubs[i * 4] : round.clubs[((i- (round.clubs.Count / 4)) * 4) + 2];
                Club away = i < round.clubs.Count / 4 ? round.clubs[(i * 4) + 3] : round.clubs[((i - (round.clubs.Count / 4)) * 4) + 1];

                DateTime day = GetRoundProgrammationDate(round, programmation);

                if (round.rules.Contains(Rule.AtHomeIfTwoLevelDifference))
                {
                    Club[] switchedTeams = SwitchTeams(home, away);
                    home = switchedTeams[0];
                    away = switchedTeams[1];
                }
                res.Add(new Match(home, away, day, !round.twoLegs));
            }

            TVSchedule(res, round.programmation.tvScheduling, 0);
            if (round.twoLegs)
            {
                CreateSecondLegKnockOutRound(res, round, programmation);
            }

            return res;
        }

        /// <summary>
        /// Get list of fixtures when the precedent round is a knockout round and no random drawing is performed
        /// </summary>
        /// <param name="round">The round to draw</param>
        /// <param name="previousRound">The previous round</param>
        /// <returns></returns>
        public static List<Match> DrawNoRandomDrawing(KnockoutRound round, KnockoutRound previousRound)
        {
            //We assume teams are added in round in the order of the games
            List<Match> res = new List<Match>();

            List<Club> clubsList = new List<Club>();

            for(int i = previousRound.matches.Count-round.clubs.Count; i < previousRound.matches.Count; i++)
            {
                clubsList.Add(previousRound.matches[i].Winner);
            }

            RoundProgrammation programmation = round.programmation;
            for (int i = 0; i< clubsList.Count/2; i++)
            {
                Club home = clubsList[i*2];
                Club away = clubsList[(i*2) + 1];

                DateTime day = GetRoundProgrammationDate(round, programmation);

                if (round.rules.Contains(Rule.AtHomeIfTwoLevelDifference))
                {
                    Club[] switchedTeams = SwitchTeams(home, away);
                    home = switchedTeams[0];
                    away = switchedTeams[1];
                }
                res.Add(new Match(home, away, day, !round.twoLegs));
            }

            TVSchedule(res, round.programmation.tvScheduling, 0);
            if (round.twoLegs)
            {
                CreateSecondLegKnockOutRound(res, round, programmation);
            }

            return res;
        }

        /// <summary>
        /// The drawing for direct-elimination round
        /// </summary>
        /// <param name="round">The round to draw</param>
        /// <returns>The list of games of this round</returns>
        public static List<Match> Draw(KnockoutRound round)
        {
            Console.WriteLine("Tirage au sort " + round.name + " (" + round.Tournament.name + "), initialisé le " + round.DateInitialisationRound().ToString() + ", du " + Session.Instance.Game.date.ToString());
            if(round.Tournament.rounds.IndexOf(round) > 0 && round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1].matches.Count > 0)
            {
                Console.WriteLine("Précédent tour : " + round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1].name + " initialisé le " + round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1].DateInitialisationRound().ToString());
                Console.WriteLine("Date: " + round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1].matches[0].day.ToString());
            }
            List<Match> res = new List<Match>();


            // Clubs ultramarins dispatchés dans les hats
            // Si deux équipes ultramarines ne peuvent s'affronter et qu'elles s'affrontent dans un hat, alors on recommence le tirage
            /*
            Si SEULEMENT_AWAY:
                Fixe extérieur
            SINON HOME_OU_AWAY:
                Lister toutes les nouvelles équipes dom-tom du tour
                Fixer n/2 home et n/2+n%2 away
                Pour toute équipe dom-tom
                Si équipe dom_tom déjà au tour précédant :
                    Fixe !Domicile-Extérieur tour précédant
                */
            List<Club>[] hats = new List<Club>[] { new List<Club>(), new List<Club>() };

            if(round.randomDrawingMethod == RandomDrawingMethod.Ranking)
            {
                GroupsRound previousRound = round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1] as GroupsRound;
                List<Club>[] clubsByRankPosition = new List<Club>[previousRound.maxClubsInGroup];
                List<Club> allClubs = new List<Club>();

                for (int i = 0; i < clubsByRankPosition.Length; i++)
                {
                    clubsByRankPosition[i] = new List<Club>();
                }

                for (int i = 0; i < previousRound.groupsCount; i++)
                {
                    List<Club> ranking = previousRound.Ranking(i);
                    for (int j = 0; j < ranking.Count; j++)
                    {
                        clubsByRankPosition[j].Add(ranking[j]);
                    }
                }

                for (int i = 0; i < clubsByRankPosition.Length; i++)
                {
                    clubsByRankPosition[i].Sort(new ClubRankingComparator(previousRound.matches));
                    allClubs.AddRange(clubsByRankPosition[i]);
                }

                allClubs = new List<Club>(allClubs.GetRange(0, round.clubs.Count));
                hats[0].AddRange(allClubs.GetRange(0, round.clubs.Count / 2));
                hats[1].AddRange(allClubs.GetRange(round.clubs.Count / 2, round.clubs.Count / 2));
            }
            else if(round.randomDrawingMethod == RandomDrawingMethod.Coefficient)
            {
                List<Club> allClubs = new List<Club>(round.clubs);
                allClubs.Sort(new ClubComparator(ClubAttribute.CONTINENTAL_COEFFICIENT));
                hats[0].AddRange(allClubs.GetRange(0, round.clubs.Count / 2));
                hats[1].AddRange(allClubs.GetRange(round.clubs.Count / 2, round.clubs.Count / 2));
            }
            else if(round.randomDrawingMethod == RandomDrawingMethod.Geographic)
            {
                List<Club> allClubs = new List<Club>(round.clubs);
                int currentClubsByGroups = allClubs.Count;
                int groupsNumber = 1;
                while((currentClubsByGroups / 2) % 2 == 0 && (allClubs.Count / groupsNumber) > 20)
                {
                    currentClubsByGroups /= 2;
                    groupsNumber *= 2;
                }
                KMeansClustering kmeans = new KMeansClustering(allClubs, groupsNumber);
                hats = kmeans.CreateClusters();
            }
            //Random
            else
            {
                List<Club> allClubs = new List<Club>(round.clubs);
                allClubs.Shuffle();
                for(int i = 0; i < allClubs.Count; i++)
                {
                    hats[i < allClubs.Count / 2 ? 0 : 1].Add(allClubs[i]);
                }
            }


            RoundProgrammation programmation = round.programmation;
            int currentGeographicHat = 0;
            for(int i = 0; i < round.clubs.Count / 2; i++)
            {
                Club home;
                Club away;
                if(round.randomDrawingMethod != RandomDrawingMethod.Geographic)
                {
                    int hat = Session.Instance.Random(0, 2);
                    home = DrawClub(hats[hat]);
                    away = DrawClub(hats[hat == 1 ? 0 : 1]);
                }
                else
                {
                    if(hats[currentGeographicHat].Count < 2)
                    {
                        currentGeographicHat++;
                    }
                    home = DrawClub(hats[currentGeographicHat]);
                    away = DrawClub(hats[currentGeographicHat]);
                }

                DateTime day = GetRoundProgrammationDate(round, programmation);

                if(round.rules.Contains(Rule.AtHomeIfTwoLevelDifference))
                {
                    Club[] switchedTeams = SwitchTeams(home, away);
                    home = switchedTeams[0];
                    away = switchedTeams[1];
                }
                res.Add(new Match(home, away, day, !round.twoLegs));
            }

            TVSchedule(res, round.programmation.tvScheduling, 0);
            if(round.twoLegs)
            {
                CreateSecondLegKnockOutRound(res, round, programmation);
            }
            return res;
        }

        /// <summary>
        /// Get a club from a hat and remove it
        /// </summary>
        /// <param name="hat">The hat</param>
        /// <returns></returns>
        private static Club DrawClub(List<Club> hat)
        {
            Club res = hat[Session.Instance.Random(0, hat.Count)];
            hat.Remove(res);
            return res;
        }
    }
}