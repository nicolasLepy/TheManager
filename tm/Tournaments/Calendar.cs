using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using tm.Algorithms;
using tm.Comparators;
using tm.Tournaments;

namespace tm
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

        public GenericCalendar()
        {

        }

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
            Tournament matchChampionship = match.home.Championship;
            bool weekendGame = match.day.DayOfWeek == DayOfWeek.Saturday;
            if (matchChampionship != null)
            {
                List<float[]> gamesTimesList = weekendGame ? match.home.Country().gamesTimesWeekend : match.home.Country().gamesTimesWeekdays;
                if(gamesTimesList.Count > 0)
                {
                    float[] gamesTimes = gamesTimesList.Count > (matchChampionship.level - 1) ? gamesTimesList[matchChampionship.level - 1] : gamesTimesList[gamesTimesList.Count - 1];
                    int hourIndex = Session.Instance.Random(1, CountHours(gamesTimes));
                    int dayIndex = 0;
                    float halfTimeProbability = 0f;
                    int counter = 0;
                    int hour = 0;
                    bool find = false;
                    for (int i = 0; i < gamesTimes.Length && !find; i++)
                    {
                        counter += (int)Math.Floor(gamesTimes[i]);
                        if (counter >= hourIndex)
                        {
                            find = true;
                            halfTimeProbability = (float)(gamesTimes[i] - Math.Truncate(gamesTimes[i]));
                            dayIndex = i/Utils.gamesTimesHoursCount;
                            hour = i % Utils.gamesTimesHoursCount;
                        }
                    }
                    int dayOffset = dayIndex - (weekendGame ? 1 : 2);
                    int minute = Session.Instance.Random() < halfTimeProbability ? 30 : 0;
                    match.day = match.day.AddDays(dayOffset);
                    match.day = match.day.AddHours(hour - match.day.Hour);
                    match.day = match.day.AddMinutes(minute - match.day.Minute);
                }
            }
        }

        private static int CountHours(float[] hoursList)
        {
            int res = 0;
            for(int i = 0; i < hoursList.Length; i++)
            {
                res += (int)Math.Floor(hoursList[i]);
            }
            return res;
        }

        /// <summary>
        /// Only suitable for two-conferences league
        /// </summary>
        /// <param name="n">Matrix size (number of teams)</param>
        /// <returns></returns>
        private static int[,] GenerateNonConferencesGamesMatrix(int n, int phases)
        {
            int[,] matrix = new int[n, n*phases];
            for(int phase = 0; phase < phases; phase++)
            {
                List<int> firstRow = GenerateRandomSample(n);
                List<int> permutes = GenerateRandomSample(n);
                int row = 0;
                foreach (int i in permutes)
                {
                    List<int> step = firstRow.GetRange(i, firstRow.Count - i);
                    step.AddRange(firstRow.GetRange(0, i));
                    int[] line = step.ToArray();
                    for (int j = 0; j < line.Length; j++)
                    {
                        matrix[row, (phase*n)+j] = line[j];
                    }
                    row++;
                }
            }
            return matrix;
        }

        private static List<int> GenerateRandomSample(int n)
        {
            List<int> res = new List<int>();
            for (int i = 0; i < n; i++)
            {
                res.Add(i);
            }
            res.Shuffle();
            return res;
        }

        private static int[] GetEvenlyDistributedNValuesFromM(int n, int m)
        {
            int[] res = new int[n];
            double step = (m - 1.0) / (n - 1.0);
            for(int i = 0; i<n; i++)
            {
                res[i] = (int)Math.Round(step * i);
            }
            return res;
        }

        public static List<Match> GenerateNonConferenceGames(GroupsRound tournamentRound)
        {
            List<Match> res = new List<Match>();
            int maxTeamsByGroup = 0;
            foreach(List<Club> c in tournamentRound.groups)
            {
                maxTeamsByGroup = c.Count > maxTeamsByGroup ? c.Count : maxTeamsByGroup;
            }
            List<Club>[] groups = new List<Club>[tournamentRound.groupsCount];
            for(int i = 0; i<tournamentRound.groups.Length; i++)
            {
                groups[i] = new List<Club>(tournamentRound.groups[i]);
                groups[i].Shuffle();
            }
            int nonConferenceGamesPhases = 1 + (tournamentRound.nonGroupGamesByTeams/maxTeamsByGroup);
            int totalRound = (maxTeamsByGroup - 1) * nonConferenceGamesPhases;
            int programmationDaysCountByPhase = (tournamentRound.programmation.gamesDays.Count / tournamentRound.phases);
            int[] baseCalendarIndices = GetEvenlyDistributedNValuesFromM(maxTeamsByGroup - 1, programmationDaysCountByPhase);

            List<int> nonConferenceGamesCalendarIndices = new List<int>();
            for(int i = 0; i< tournamentRound.programmation.gamesDays.Count; i++)
            {
                bool ok = true;
                foreach(int index in baseCalendarIndices)
                {
                    if(i% programmationDaysCountByPhase == index)
                    {
                        ok = false;
                    }
                }
                if(ok)
                {
                    nonConferenceGamesCalendarIndices.Add(i);
                }
            }
            
            int gamesByDays = tournamentRound.nonGroupGamesByGameday > 0 ? tournamentRound.nonGroupGamesByGameday : tournamentRound.clubs.Count / 2;
            int totalNonConferencesGames = tournamentRound.clubs.Count * tournamentRound.nonGroupGamesByTeams / 2;
            int daysCounts = totalNonConferencesGames / gamesByDays;
            int[] calendarIndices = GetEvenlyDistributedNValuesFromM(daysCounts, nonConferenceGamesCalendarIndices.Count);
            int[,] gamesMatrix = GenerateNonConferencesGamesMatrix(maxTeamsByGroup, nonConferenceGamesPhases);

            List<Club[]> games = new List<Club[]>();

            for (int col = 0; col<tournamentRound.nonGroupGamesByTeams; col++)
            {
                for(int row = 0; row<maxTeamsByGroup; row++)
                {
                    if (row < groups[0].Count && gamesMatrix[row, col] < tournamentRound.groups[1].Count)
                    {
                        Club first = col % 2 == 0 ? groups[0][row] : groups[1][gamesMatrix[row, col]];
                        Club second = col % 2 == 0 ? groups[1][gamesMatrix[row, col]] : groups[0][row];
                        games.Add(new Club[] { first, second });
                    }
                }
            }
            for(int i = 0; i<games.Count; i++)
            {
                if (games[i].Contains(null))
                {
                    games.RemoveAt(i);
                    i--;
                }
            }

            int calendarIndex = 0;
            for (int i = 0; i < games.Count; i++) //games.Count and not totalNonConferencesGames because due to algo some games are removed because of ghost team
            {
                Club club1 = games[i][0];
                Club club2 = games[i][1];

                if(i%gamesByDays == 0 && i > 0)
                {
                    calendarIndex++;
                }
                int programmationIndice = nonConferenceGamesCalendarIndices[calendarIndices[calendarIndex]];
                DateTime jour = tournamentRound.programmation.gamesDays[programmationIndice].ConvertToDateTime(Session.Instance.Game.date.Year);
                if (Utils.IsBeforeWithoutYear(jour, tournamentRound.DateInitialisationRound()))
                {
                    jour = tournamentRound.programmation.gamesDays[programmationIndice].ConvertToDateTime(Session.Instance.Game.date.Year + 1);
                }
                jour = jour.AddHours(tournamentRound.programmation.defaultHour.Hours - jour.Hour);
                jour = jour.AddMinutes(tournamentRound.programmation.defaultHour.Minutes - jour.Minute);

                Match game = new Match(tournamentRound, club1, club2, jour, false);
                res.Add(game);

            }
            return res;
        }

        /// <summary>
        /// Round robin algorithm to generate games
        /// </summary>
        /// <param name="clubs">List of clubs</param>
        /// <param name="programmation">TV / Federation schedule for games</param>
        /// <param name="twoLegged">One or two games</param>
        /// <returns></returns>
        public static List<Match> GenerateCalendar(List<Club> clubsBase, Round tournamentRound)
        {
            RoundProgrammation programmation = tournamentRound.programmation;
            List<Match> res = new List<Match>();
            bool ghost = false;
            List<Club> clubs = new List<Club>(clubsBase);
            clubs.Shuffle();
            int teams = clubs.Count;
            if (teams % 2 == 1)
            {
                ghost = true;
                teams++;
            }

            int totalRound = teams - 1;
            int gamesPerRound = teams / 2;
            int[,,] rounds = new int[totalRound, gamesPerRound, 2];

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

                    rounds[round, match, 0] = (home + 1);
                    rounds[round, match, 1] = (away + 1);

                }
            }

            int[,,] interleaved = new int[totalRound, gamesPerRound, 2];
            int evn = 0;
            int odd = (teams / 2);

            for (int i = 0; i < totalRound; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < gamesPerRound; j++)
                    {
                        interleaved[i, j, 0] = rounds[evn, j, 0];
                        interleaved[i, j, 1] = rounds[evn, j, 1];
                    }
                    evn++;
                }
                else
                {
                    for (int j = 0; j < gamesPerRound; j++)
                    {
                        interleaved[i, j, 0] = rounds[odd, j, 0];
                        interleaved[i, j, 1] = rounds[odd, j, 1];
                    }
                    odd++;
                }
            }

            rounds = interleaved;

            for (int round = 0; round < totalRound; round++)
            {
                if (round % 2 == 1)
                {
                    int[] flipped = new [] { rounds[round, 0, 1], rounds[round, 0, 0] };
                    rounds[round, 0, 0] = flipped[0];
                    rounds[round, 0, 1] = flipped[1];
                }
            }

            //Remove games with ghost team
            if(ghost)
            {
                int[,,] newRounds = new int[totalRound, gamesPerRound - 1, 2];
                for(int round = 0; round < totalRound; round++)
                {
                    int baseRoundGame = 0;
                    for(int game = 0; game < gamesPerRound-1; game++, baseRoundGame++)
                    {
                        if (rounds[round, baseRoundGame, 0] > clubs.Count || rounds[round, baseRoundGame, 1] > clubs.Count)
                        {
                            baseRoundGame++;
                        }
                        newRounds[round, game, 0] = rounds[round, baseRoundGame, 0];
                        newRounds[round, game, 1] = rounds[round, baseRoundGame, 1];
                    }
                }
                rounds = newRounds;
                gamesPerRound--;
            }

            int[] calendarIndices = GetEvenlyDistributedNValuesFromM(totalRound, programmation.gamesDays.Count/tournamentRound.phases);
            for (int i = 0; i < totalRound; i++)
            {
                List<Match> matchs = new List<Match>();
                for (int j = 0; j < gamesPerRound; j++)
                {
                    int a = rounds[i, j, 0];
                    int b = rounds[i, j, 1];

                    if (!ghost || (a != teams && b != teams))
                    {
                        //-1 car index dans liste de 0 à n-1, et des clubs de 1 à n
                        Club e1 = clubs[a - 1];
                        Club e2 = clubs[b - 1];

                        //Game day
                        DateTime jour = programmation.gamesDays[calendarIndices[i]].ConvertToDateTime(Session.Instance.Game.date.Year);
                        if (Utils.IsBeforeWithoutYear(jour, tournamentRound.DateInitialisationRound()))
                        {
                            jour = programmation.gamesDays[calendarIndices[i]].ConvertToDateTime(Session.Instance.Game.date.Year+1);
                        }
                        jour = jour.AddHours(programmation.defaultHour.Hours);
                        jour = jour.AddMinutes(programmation.defaultHour.Minutes);

                        Match m = new Match(tournamentRound, e1, e2, jour, false);
                        res.Add(m);
                        matchs.Add(m);
                    }
                }
                TVSchedule(matchs, programmation.tvScheduling, i+1);
            }
            if(tournamentRound.phases > 1)
            {
                int nbGamesByLeg = res.Count / gamesPerRound;
                for (uint leg = 2; leg <= tournamentRound.phases; leg++)
                {
                    bool firstLeg = leg % 2 == 1;
                    //New phase, redraw games
                    if (firstLeg)
                    {
                        clubs.Shuffle();
                    }
                    //Part 1 : manager Journey [2-end]
                    int calendarBaseIndex = (programmation.gamesDays.Count / tournamentRound.phases) * ((int)leg-1);
                    List<Match> games;
                    for (int i = firstLeg ? 0 : 1; i < nbGamesByLeg; i++)
                    {
                        games = new List<Match>();
                        for (int j = 0; j < gamesPerRound; j++)
                        {
                            int calendarIndex = firstLeg ? calendarIndices[i] : calendarIndices[i - 1];
                            DateTime jour = programmation.gamesDays[calendarBaseIndex + calendarIndex].ConvertToDateTime(Session.Instance.Game.date.Year);
                            if (Utils.IsBeforeWithoutYear(jour, tournamentRound.DateInitialisationRound()))
                            {
                                jour = programmation.gamesDays[calendarBaseIndex + calendarIndex].ConvertToDateTime(Session.Instance.Game.date.Year + 1);
                            }
                            jour = jour.AddHours(programmation.defaultHour.Hours);
                            jour = jour.AddMinutes(programmation.defaultHour.Minutes);

                            Match retour = new Match(tournamentRound, clubs[rounds[i, j, 1]-1], clubs[rounds[i, j, 0]-1], jour, false);
                            games.Add(retour);
                            res.Add(retour);
                        }
                        if(tournamentRound.Tournament.name == "Ligue 1")
                        {
                            Console.WriteLine("[nbGamesByLeg] " + nbGamesByLeg + " check " + (nbGamesByLeg - i) + ">=" + programmation.lastMatchDaysSameDayNumber);
                        }
                        if (nbGamesByLeg - i >= programmation.lastMatchDaysSameDayNumber)
                        {
                            TVSchedule(games, programmation.tvScheduling, nbGamesByLeg + i);
                        }
                    }
                    if(!firstLeg)
                    {
                        //Last journey : first journey inverted
                        games = new List<Match>();
                        int lastLegcalendarIndex = ((programmation.gamesDays.Count / tournamentRound.phases) * (int)leg) - 1;
                        for (int i = 0; i < gamesPerRound; i++)
                        {
                            DateTime jour = programmation.gamesDays[lastLegcalendarIndex].ConvertToDateTime(Session.Instance.Game.date.Year);
                            if (Utils.IsBeforeWithoutYear(jour, tournamentRound.DateInitialisationRound()))
                            {
                                jour = programmation.gamesDays[lastLegcalendarIndex].ConvertToDateTime(Session.Instance.Game.date.Year + 1);
                            }
                            jour = jour.AddHours(programmation.defaultHour.Hours);
                            jour = jour.AddMinutes(programmation.defaultHour.Minutes);
                            Match retour = new Match(tournamentRound, clubs[rounds[0, i, 1] - 1], clubs[rounds[0, i, 0] - 1], jour, false);
                            games.Add(retour);
                            res.Add(retour);
                        }

                        if (programmation.lastMatchDaysSameDayNumber < 1)
                        {
                            TVSchedule(games, programmation.tvScheduling, nbGamesByLeg * 2);
                        }
                    }
                }

            }

            return res;
        }

        private static int[] flip(int[] match)
        {
            return new int[] { match[0], match[1] };
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

        private static Club[] SwitchTeamsTwoLevelsGap(Club home, Club away)
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
            day = day.AddHours(programmation.defaultHour.Hours);
            day = day.AddMinutes(programmation.defaultHour.Minutes);
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
                day = day.AddHours(programmation.defaultHour.Hours);
                day = day.AddMinutes(programmation.defaultHour.Minutes);

                Match secondRound = new Match(round, m.away, m.home, day, !(round.phases == 2), m);
                games.Add(secondRound);
                gamesList.Add(secondRound);
            }
            TVSchedule(games, round.programmation.tvScheduling, 0);
        }

        /// <summary>
        /// Get list of fixtures when the precedent round is a championship round and no random drawing is performed
        /// The best team receive the worst, the second best the second worst ...
        /// </summary>
        /// <param name="round"></param>
        /// <returns></returns>
        public static List<Match> DrawNoRandomDrawing(KnockoutRound round)
        {
            RoundProgrammation programmation = round.programmation;
            DateTime day = GetRoundProgrammationDate(round, programmation);

            List<Match> res = new List<Match>();
            List<Club> teams = new List<Club>(round.clubs);
            teams.Sort(new ClubComparator(ClubAttribute.CURRENT_RANKING));
            for (int i = 0; i < round.clubs.Count / 2; i++)
            {
                Club clubA = teams[i];
                Club clubB = teams[teams.Count - i - 1];
                res.Add(new Match(round, round.phases % 2 == 1 ? clubA : clubB, round.phases % 2 == 1 ? clubB : clubA, day, round.phases == 1));
            }

            TVSchedule(res, round.programmation.tvScheduling, 0);
            for(int i = 1; i < round.phases; i++)
            {
                CreateSecondLegKnockOutRound(res, round, programmation);
            }

            return res;
        }

        /// <summary>
        /// Get list of fixtures when the precedent round is a group round and no random drawing is performed
        /// TODO: Currently only work when there is two qualified teams by group
        /// TODO: Maybe DrawNoRandomDrawing for GroupsRound can be merged with DrawNoRandomDrawing of ChampionshipRound
        /// </summary>
        /// <param name="round">The round to draw</param>
        /// <param name="previousRound">The previous round</param>
        /// <returns></returns>
        public static List<Match> DrawNoRandomDrawing(KnockoutRound round, GroupsRound previousRound)
        {
            //We assume teams are added in round in the order of ranking and group
            List<Match> res = new List<Match>();

            RoundProgrammation programmation = round.programmation;
            //Hacky way to manage round with only 2 clubs
            if(round.clubs.Count == 2)
            {
                Club home = round.clubs[0];
                Club away = round.clubs[1];
                DateTime day = GetRoundProgrammationDate(round, programmation);
                res.Add(new Match(round, home, away, day, round.phases == 1));
            }
            else
            {
                for (int i = 0; i < round.clubs.Count / 2; i++)
                {
                    Club home = i < round.clubs.Count / 4 ? round.clubs[i * 4] : round.clubs[((i - (round.clubs.Count / 4)) * 4) + 2];
                    Club away = i < round.clubs.Count / 4 ? round.clubs[(i * 4) + 3] : round.clubs[((i - (round.clubs.Count / 4)) * 4) + 1];
                    DateTime day = GetRoundProgrammationDate(round, programmation);
                    res.Add(new Match(round, home, away, day, round.phases == 1));
                }
            }
            if (round.rules.Contains(Rule.AtHomeIfTwoLevelDifference))
            {
                foreach (Match m in res)
                {
                    Club[] switchedTeams = SwitchTeamsTwoLevelsGap(m.home, m.away);
                    m.home = switchedTeams[0];
                    m.away = switchedTeams[1];
                }
            }

            TVSchedule(res, round.programmation.tvScheduling, 0);
            for (int i = 1; i < round.phases; i++)
            {
                CreateSecondLegKnockOutRound(res, round, programmation); //TODO: So the best team must receive for the second leg
            }

            return res;
        }

        /// <summary>
        /// Get list of fixtures when the precedent round is a knockout round and no random drawing is performed
        /// </summary>
        /// <param name="round">The round to draw</param>
        /// <param name="previousRound">The previous round</param>
        /// <returns></returns>
        public static List<Match> DrawNoRandomDrawingFixed(KnockoutRound round, KnockoutRound previousRound)
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
                    Club[] switchedTeams = SwitchTeamsTwoLevelsGap(home, away);
                    home = switchedTeams[0];
                    away = switchedTeams[1];
                }
                res.Add(new Match(round, home, away, day, round.phases == 1));
            }

            TVSchedule(res, round.programmation.tvScheduling, 0);
            for (int i = 1; i < round.phases; i++)
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
        public static List<Match> DrawNoRandomDrawingRanking(KnockoutRound round, KnockoutRound previousRound)
        {
            List<Match> res = new List<Match>();

            //We assume teams are added in round in the order of the games
            List<Club> clubsFromPreviousRound = new List<Club>(round.clubs);
            List<Club> newClubs = new List<Club>();
            List<Club> clubs = new List<Club>();
            foreach(Club c in round.clubs)
            {
                if(!previousRound.clubs.Contains(c))
                {
                    newClubs.Add(c);
                    clubsFromPreviousRound.Remove(c);
                }
            }
            clubs = newClubs.Concat(clubsFromPreviousRound).ToList();

            RoundProgrammation programmation = round.programmation;
            DateTime day = GetRoundProgrammationDate(round, programmation);

            for (int i = 0; i < clubs.Count/2; i++)
            {
                Club clubA = clubs[i];
                Club clubB = clubs[clubs.Count - 1 - i];
                res.Add(new Match(round, round.phases % 2 == 1 ? clubA : clubB, round.phases % 2 == 1 ? clubB : clubA, day, round.phases == 1));
            }

            TVSchedule(res, round.programmation.tvScheduling, 0);
            for (int i = 1; i < round.phases; i++)
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
            Console.WriteLine("Tirage au sort " + round.name + " (" + round.Tournament.name + "), initialisé le " + round.DateInitialisationRound().ToString() + ", au " + round.DateEndRound().ToString());
            if(round.Tournament.rounds.IndexOf(round) > 0 && round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1].matches.Count > 0)
            {
                Console.WriteLine("Précédent tour : " + round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1].name + " initialisé le " + round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1].DateInitialisationRound().ToString());
                Console.WriteLine("Date: " + round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1].matches[0].day.ToString());
            }
            List<Match> res = new List<Match>();
            ILocalisation localisationTournament = Session.Instance.Game.kernel.LocalisationTournament(round.Tournament);

            List<Club>[] hats = new List<Club>[] { new List<Club>(), new List<Club>() };

            //First step, fix if some teams will play home or away according to round rules
            Dictionary<Club, bool> fixedHomeOrAwayTeams = new Dictionary<Club, bool>();
            if(round.rules.Contains(Rule.UltramarineTeamsPlayAway))
            {
                foreach(Club c in round.clubs)
                {
                    if(c.Country() != localisationTournament)
                    {
                        fixedHomeOrAwayTeams.Add(c, false);
                    }
                }
            }
            if(round.rules.Contains(Rule.UltramarineTeamsPlayHomeOrAway))
            {
                List<Club> newUltramarineTeams = new List<Club>();
                Round previousRound = round.Tournament.rounds.IndexOf(round) > 0 ? round.Tournament.rounds[round.Tournament.rounds.IndexOf(round) - 1] : null;
                foreach (Club c in round.clubs)
                {
                    //New ultramarine team entering this round
                    if (c.Country() != localisationTournament && (previousRound != null && !previousRound.clubs.Contains(c)))
                    {
                        newUltramarineTeams.Add(c);
                    }
                    //Ultramarine already in tournament
                    else if(c.Country() != localisationTournament && previousRound != null)
                    {
                        foreach(Match m in previousRound.matches)
                        {
                            if(m.home == c)
                            {
                                fixedHomeOrAwayTeams.Add(c, false);
                            }
                            else if(m.away == c)
                            {
                                fixedHomeOrAwayTeams.Add(c, true);
                            }
                        }
                    }
                }
                int willPlayHome = newUltramarineTeams.Count / 2;
                newUltramarineTeams = Utils.ShuffleList<Club>(newUltramarineTeams);
                for(int i = 0; i<willPlayHome; i++)
                {
                    fixedHomeOrAwayTeams.Add(newUltramarineTeams[i], true);
                }
                for(int i = willPlayHome; i<newUltramarineTeams.Count; i++)
                {
                    fixedHomeOrAwayTeams.Add(newUltramarineTeams[i], false);
                }
            }

            if (round.randomDrawingMethod == RandomDrawingMethod.Ranking)
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
                    clubsByRankPosition[i].Sort(new ClubRankingComparator(previousRound.matches, previousRound.tiebreakers, previousRound.pointsDeduction));
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
                List<Club> clubsToDispatch = new List<Club>();
                
                int currentClubsByGroups = allClubs.Count;
                int groupsNumber = 1;
                while((currentClubsByGroups / 2) % 2 == 0 && (allClubs.Count / groupsNumber) > 20)
                {
                    currentClubsByGroups /= 2;
                    groupsNumber *= 2;
                }

                if (round.rules.Contains(Rule.UltramarineTeamsPlayAway) || round.rules.Contains(Rule.UltramarineTeamsPlayHomeOrAway))
                {
                    foreach (Club c in allClubs)
                    {
                        if (c.Country() != localisationTournament)
                        {
                            clubsToDispatch.Add(c);
                        }
                    }
                    foreach (Club c in clubsToDispatch)
                    {
                        allClubs.Remove(c);
                    }
                }

                if(allClubs.Count > 0)
                {
                    KMeansClustering kmeans = new KMeansClustering(allClubs, groupsNumber, clubsToDispatch);
                    hats = kmeans.CreateClusters();
                }
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
            for (int i = 0; i < round.clubs.Count / 2; i++)
            {
                Club home = null;
                Club away = null;
                if (round.randomDrawingMethod != RandomDrawingMethod.Geographic)
                {
                    int hat = Session.Instance.Random(0, 2);
                    home = DrawClub(hats[hat]);
                    away = DrawClub(hats[hat == 1 ? 0 : 1]);
                }
                else
                {
                    if (hats[currentGeographicHat].Count < 2)
                    {
                        currentGeographicHat++;
                    }

                    //If Ultramarines teams can't compete against each other, start to draw them
                    if (round.rules.Contains(Rule.UltramarineTeamsCantCompeteAgainst))
                    {
                        for (int j = 0; j < hats[currentGeographicHat].Count && home == null; j++)
                        {
                            if (hats[currentGeographicHat][j].Country() != localisationTournament)
                            {
                                home = hats[currentGeographicHat][j];
                                hats[currentGeographicHat].Remove(hats[currentGeographicHat][j]);
                            }
                        }
                    }
                    if (home == null)
                    {
                        home = DrawClub(hats[currentGeographicHat]);
                    }
                    away = DrawClub(hats[currentGeographicHat]);
                }

                DateTime day = GetRoundProgrammationDate(round, programmation);

                if(round.rules.Contains(Rule.AtHomeIfTwoLevelDifference))
                {
                    Club[] switchedTeams = SwitchTeamsTwoLevelsGap(home, away);
                    home = switchedTeams[0];
                    away = switchedTeams[1];
                }
                if((fixedHomeOrAwayTeams.ContainsKey(home) && !fixedHomeOrAwayTeams[home]) || (fixedHomeOrAwayTeams.ContainsKey(away) && fixedHomeOrAwayTeams[away]))
                {
                    Club temp = home;
                    home = away;
                    away = temp;
                }
                res.Add(new Match(round, home, away, day, round.phases == 1));
            }

            TVSchedule(res, round.programmation.tvScheduling, 0);
            for (int i = 1; i < round.phases; i++)
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