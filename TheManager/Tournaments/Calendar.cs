using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{

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
            match.day = match.day.AddDays(dayOffset);
            match.day = match.day.AddHours(hour - match.day.Hour);
            match.day = match.day.AddMinutes(minute - match.day.Minute);

        }

        private static List<int> _level2;
        private static List<int> _level3;
        private static List<int> _level4;
        private static List<int> _level5;
        private static List<int> _level6;
        private static List<int> _level7;

        private static void ConstructTables()
        {
            _level2 = new List<int>();
            _level3 = new List<int>();
            _level4 = new List<int>();
            _level5 = new List<int>();
            _level6 = new List<int>();
            _level7 = new List<int>();
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
        public static List<Match> GenerateCalendar(List<Club> clubs, RoundProgrammation programmation, bool twoLegged)
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

                        //Date du match
                        DateTime jour = new DateTime(Session.Instance.Game.date.Year, programmation.gamesDays[i].Month, programmation.gamesDays[i].Day, programmation.defaultHour.Hours, programmation.defaultHour.Minutes, 0);
                        if (Utils.IsBeforeWithoutYear(jour, programmation.initialisation))
                        {
                            jour = jour.AddYears(1);
                        }
                        Match m = new Match(e1, e2, jour, false);
                        res.Add(m);
                        matchs.Add(m);
                    }
                }
                TVSchedule(matchs, programmation.tvScheduling, i+1);
            }
            if(twoLegged)
            {
                //Part 1 : manager Journey [2-end]
                int nbGamesFirstRound = res.Count / gamesPerRound;
                List<Match> games = new List<Match>();
                for (int i = 1; i< nbGamesFirstRound; i++)
                {
                    games = new List<Match>();
                    for(int j = 0; j< gamesPerRound; j++)
                    {
                        Match mbase = res[gamesPerRound * i + j];
                        DateTime jour = new DateTime(Session.Instance.Game.date.Year, programmation.gamesDays[nbGamesFirstRound+i-1].Month, programmation.gamesDays[nbGamesFirstRound + i-1].Day, programmation.defaultHour.Hours, programmation.defaultHour.Minutes, 0);
                        if (Utils.IsBeforeWithoutYear(jour, programmation.initialisation))
                        {
                            jour = jour.AddYears(1);
                        }
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
                    DateTime jour = new DateTime(Session.Instance.Game.date.Year, programmation.gamesDays[programmation.gamesDays.Count-1].Month, programmation.gamesDays[programmation.gamesDays.Count - 1].Day, programmation.defaultHour.Hours, programmation.defaultHour.Minutes, 0);
                    if (Utils.IsBeforeWithoutYear(jour, programmation.initialisation))
                    {
                        jour = jour.AddYears(1);
                    }
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
            int indice = 0;
            if (offsets.Count>0)
            {
                try
                {
                    games.Sort(new MatchLevelComparator());
                }
                catch
                {
                    Utils.Debug("Match_Niveau_Comparator exception pour programme TV");
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
        
        /// <summary>
        /// The drawing for direct-elimination round
        /// </summary>
        /// <param name="round">The round to draw</param>
        /// <returns>The list of games of this round</returns>
        public static List<Match> Draw(KnockoutRound round)
        {
            List<Match> res = new List<Match>();
            List<Club> hat = new List<Club>(round.clubs);
            for (int i = 0; i < round.clubs.Count / 2; i++)
            {
                Club home = DrawClub(hat);
                Club away = DrawClub(hat);
                DateTime day = new DateTime(Session.Instance.Game.date.Year, round.programmation.gamesDays[0].Month, round.programmation.gamesDays[0].Day, round.programmation.defaultHour.Hours, round.programmation.defaultHour.Minutes, 0);
                if (Utils.IsBeforeWithoutYear(day, round.programmation.initialisation))
                {
                    day = day.AddYears(1);
                }

                if(round.rules.Contains(Rule.AtHomeIfTwoLevelDifference))
                {
                    Tournament champH = home.Championship;
                    Tournament champA = away.Championship;
                    if((champH != null && champA != null) && champA.level - champH.level >= 2)
                    {
                        Club temp = home;
                        home = away;
                        away = temp;
                    }
                }

                res.Add(new Match(home, away, day, !round.twoLegs));
            }

            TVSchedule(res, round.programmation.tvScheduling,0);
            if(round.twoLegs)
            {
                List<Match> games = new List<Match>();
                List<Match> firstRound = new List<Match>(res);
                foreach (Match m in firstRound)
                {
                    DateTime day = new DateTime(Session.Instance.Game.date.Year, round.programmation.gamesDays[1].Month, round.programmation.gamesDays[1].Day, round.programmation.defaultHour.Hours, round.programmation.defaultHour.Minutes, 0);
                    if (Utils.IsBeforeWithoutYear(day, round.programmation.initialisation))
                    {
                        day = day.AddYears(1);
                    }
                    Match secondRound = new Match(m.away, m.home, day, !round.twoLegs, m);
                    games.Add(secondRound);
                    res.Add(secondRound);
                }
                TVSchedule(games, round.programmation.tvScheduling, 0);
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