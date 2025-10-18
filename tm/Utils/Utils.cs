using Newtonsoft.Json.Linq;
using NHibernate.Id;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using tm.Algorithms;
using tm.Comparators;

namespace tm
{

    public enum QualificationType
    {
        DirectPromotion,
        PossiblePromotion,
        PossibleRelegation,
        DirectRelegation
    }

    public static class Utils
    {

        public readonly static bool DISABLE_FINANCIAL_SANCTIONS = true;
        public readonly static bool DISABLE_ADMINISTRATIVE_RETROGRADATIONS = true;
        public readonly static bool DISABLE_POSTPONE_GAMES = true;

        private static bool providerRegistered = false;

        public readonly static string imagesFolderName = "images";
        public readonly static string tournamentLogoFolderName = "tournaments";
        public readonly static string clubLogoFolderName = "clubs";
        public readonly static string nationalFlagsFolderName = "flags";
        public readonly static string universeLogoFolderName = "world";
        public readonly static string mediaLogoFolderName = "medias";
        public readonly static string namesSubfolderName = "names";
        private static string _dataFolderName = "data";
        public static string dataFolderName { get => _dataFolderName; set => _dataFolderName = value; }
        public readonly static string musicFolderName = "music";

        public readonly static string friendlyTournamentName = "Matchs amicaux";

        public readonly static int gamesTimesHoursCount = 24;
        public readonly static int gamesTimesDaysCount = 4;

        public readonly static int tournamentMaxId = 1000;

        /// <summary>
        /// Transform a name to a complient logo file name
        /// Removes accents, space and hyphens
        /// </summary>
        /// <param name="value">Name</param>
        public static string NormalizeFilename(string value)
        {
            if(!providerRegistered)
            {
                System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
                Encoding.RegisterProvider(provider);
                providerRegistered = true;
            }

            string flag = value.ToLower().Replace(" ", "").Replace("-", "");
            byte[] bytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(flag);
            return System.Text.Encoding.UTF8.GetString(bytes);

        }

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

        public static int Modulo(int k, int n) 
        { 
            return ((k %= n) < 0) ? k + n : k;
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

        public static int CountEvent(GameEvent gameEvent, List<Match> matchs, Club c, RankingType rankingType = RankingType.General)
        {
            int res = 0;
            foreach(Match m in MatchesOfClub(matchs, c, rankingType))
            {
                foreach(MatchEvent me in m.events)
                {
                    if(me.type == gameEvent && me.club == c)
                    {
                        res++;
                    }
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

        public static string Logo(Continent c)
        {
            string flag = Environment.CurrentDirectory + "\\" + Utils.imagesFolderName + "\\" + universeLogoFolderName + "\\" + c.Logo() + ".png";
            if (!File.Exists(flag))
            {
                flag = System.IO.Directory.GetCurrentDirectory() + "\\" + imagesFolderName + "\\" + clubLogoFolderName + "\\" + "generic.png";
            }
            return flag;
        }

        public static string Logo(Association a)
        {
            string flag = Environment.CurrentDirectory + "\\" + Utils.imagesFolderName + "\\" + universeLogoFolderName + "\\" + a.logo + ".png";
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
            string path = System.IO.Directory.GetCurrentDirectory() + "\\" + imagesFolderName + "\\" + tournamentLogoFolderName + "\\" + tournament.logo + ".png";
            if(!File.Exists(path))
            {
                path = System.IO.Directory.GetCurrentDirectory() + "\\" + imagesFolderName + "\\" + tournamentLogoFolderName + "\\" + "generic.png";
            }
            return path;
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
                case Rule.ReservesCannotBePromoted:
                    res = "Les réserves ne peuvent pas monter";
                    break; 
                default:
                    break;
            }
            return res;
        }

        public static void Debug(string str, params object[] args)
        {
            Console.WriteLine(str, args);
        }

        [Flags]
        public enum RuleStatus
        {
            RuleRespected = 1,
            RuleNotRespected = 2,
            RuleRelegation = 4
        }

        /// <summary>
        /// Allows to know if a club entering a specific round can access promotion or relegation
        /// </summary>
        /// <param name="originQualification">Club original qualification to next round</param>
        /// <param name="originTournament">Club's tournament</param>
        /// <param name="promotion">If true, consider promotion. Otherwise, consider relegation</param>
        /// <returns>True if the round can lead to [promotion/relegation], False otherwise.</returns>
        private static bool QualificationCanLeadToNewLeague(Qualification originQualification, Tournament originTournament, bool promotion)
        {
            bool res = false;
            if(originQualification.target.Type == QualificationTargetType.ExcludeFromLeagueSystem)
            {
                res = true;
            }
            else
            {
                if (originQualification.isNextYear)
                {
                    if (promotion)
                    {
                        return originTournament.IsBelow(originQualification.target);
                    }
                    if (!promotion)
                    {
                        return originTournament.IsAbove(originQualification.target);
                    }
                }
                Round r = originQualification.target.Tournament().rounds[originQualification.roundId];
                foreach (Qualification q in r.qualifications)
                {
                    if (q.isNextYear && q.target.Tournament().isChampionship && ((promotion && originTournament.IsBelow(q.target)) || (!promotion && originTournament.IsAbove(q.target))))
                    {
                        res = true;
                    }
                }
                if (!res)
                {
                    foreach (Qualification q in r.qualifications)
                    {
                        //Qualifie pour un tour de barrage qui peut mener à une promotion ou à une relegation
                        if (q.target.Type == QualificationTargetType.Tournament && !q.isNextYear && originTournament.IsSameLevel(q.target) && !q.target.Tournament().IsInternational() && q.target.Tournament().isChampionship)
                        {
                            res = QualificationCanLeadToNewLeague(q, originTournament, promotion);
                        }
                    }
                }
            }
            

            return res;
        }


        private static int GetGroup(GroupsRound groupRound, Club club)
        {
            int group = -1;
            for (int i = 0; i < groupRound.groupsCount; i++)
            {
                if (groupRound.groups[i].Contains(club))
                {
                    group = i;
                }
            }
            return group;
        }


        /// <summary>
        /// Return True if the rules are respected
        /// </summary>
        /// <param name="club">Club to check</param>
        /// <param name="qualification">Qualification to test</param>
        /// <param name="baseTournament">Tournament of the club</param>
        /// <param name="reservesCantBePromoted">Prevent reserves teams to be promoted</param>
        /// <returns></returns>
        public static RuleStatus RuleIsRespected(Club club, Qualification qualification, Tournament baseTournament, bool reservesCantBePromoted)
        {
            //Rule 1 : Reserve can't be promoted
            //Rule 2 : Reserve can't be promoted if another reserve of the club (or the fannion club) is in the higher division
            //Rule 3 : Reserve is relegated if another reserve of the club (or the fannion club) in the division above is relegated
            ReserveClub clubReserve = club as ReserveClub;
            bool isReserve = clubReserve != null;
            bool inPromotionSpot = ((qualification.target.ToChampionshipTournament()) && baseTournament.IsBelow(qualification.target)) || (!qualification.isNextYear && QualificationCanLeadToNewLeague(qualification, baseTournament, true));
            bool relegable = !qualification.isNextYear && QualificationCanLeadToNewLeague(qualification, baseTournament, false);

            bool breakRule2 = isReserve && inPromotionSpot;
            bool breakRule3 = false;


            if (clubReserve != null)
            {
                Club upperTeam = clubReserve.GetTeamAbove();
                //TODO: Two reserves in the same league
                Club upperReserve = upperTeam;// clubReserve.FannionClub.reserves[reserveCount - 1];
                Association baseAssociation = Session.Instance.Game.kernel.LocalisationTournament(baseTournament);
                Tournament tournamentAbove = baseAssociation.LeagueAbove(baseTournament).Tournament();

                Tournament up = upperReserve.Championship;
                Round upRound = up.rounds[0];
                if(up.IsAbove(new QualificationTournament(baseTournament)) && upRound.clubs.Contains(upperReserve))
                {
                    List<Qualification> upperQualifications = null;
                    List<Club> upperRanking = null;
                    GroupsRound upGr = upRound as GroupsRound;
                    if(upGr != null)
                    {
                        int group = GetGroup(upGr, upperReserve);
                        if(group == -1)
                        {
                            throw new Exception(String.Format("{0} was not found on {1}", upperReserve.name, upGr.Tournament.name));
                        }
                        upperQualifications = upGr.GetGroupQualifications(group);
                        upperRanking = upGr.Ranking(group);
                    }
                    Qualification upperReserveQualification = upperQualifications[upperRanking.IndexOf(upperReserve)];

                    /*bool upperReserveCanBePromoted = QualificationCanLeadToNewLeague(reserveQualification, baseLevel-1, true);
                    bool upperReserveCanBeReleguated = QualificationCanLeadToNewLeague(reserveQualification, baseLevel - 1, false);*/ //TODO: Handle consequence of reserves promotions/non promotion by playoffs with administrative relegations ?

                    bool aboveIsBelowReserve = tournamentAbove.IsBelow(upperReserveQualification.target);
                    if (upperReserveQualification.isNextYear && upperReserveQualification.roundId == 0 && aboveIsBelowReserve && inPromotionSpot)
                    {
                        breakRule2 = false;
                    }
                    bool debug = inPromotionSpot && baseTournament.level == 5 && false;
                    Console.WriteLine("[RuleIsRespected][Check Rule 2][{0}][{1} ({2}->{3})] inPromSpot : {4} // With team {5} ({6}->{7}, iny: {8}, ri: {9}) UpIsBelow : {10}     = breakRule2: {11}", baseTournament.name, club.name, club.Championship.shortName, qualification.target.Tournament(club).shortName, inPromotionSpot, upperReserve.name, upperReserve.Championship.shortName, upperReserveQualification.target.Tournament(upperReserve).shortName, upperReserveQualification.isNextYear, upperReserveQualification.roundId, aboveIsBelowReserve, breakRule2);

                    breakRule3 = upperReserveQualification.isNextYear && upperReserveQualification.roundId == 0 && baseTournament.IsSameLevel(upperReserveQualification.target);
                    Console.WriteLine("[RuleIsRespected][Check Rule 3][{0}][{1} ({2}->{3})] // With team {4} ({5}->{6}, iny: {7}, ri: {8})     = breakRule3: {9}", baseTournament.name, club.name, club.Championship.shortName, qualification.target.Tournament(club).shortName, upperReserve.name, upperReserve.Championship.shortName, upperReserveQualification.target.Tournament(upperReserve).shortName, upperReserveQualification.isNextYear, upperReserveQualification.roundId, breakRule3);

                }
            }

            //Console.WriteLine("QualificationCanLeadToNewLeague ? " + QualificationCanLeadToNewLeague(qualification, baseLevel, true) + " from " + qualification.tournament.rounds[qualification.roundId]);

            //bool breakRule1 = !reservesCantBePromoted || (!isReserve || (isReserve && !inPromotionSpot));
            bool breakRule1 = reservesCantBePromoted && isReserve && inPromotionSpot;
            RuleStatus res = ((breakRule1 || breakRule2) ? RuleStatus.RuleNotRespected : RuleStatus.RuleRespected) | (breakRule3 ? RuleStatus.RuleRelegation : 0);
            Console.WriteLine("[RuleIsRespected] {0} : {1} ", club.name, res);
            return res;
        }

        public static List<Club> ReservesAutomaticallyRelegated(List<Club> ranking, Association association, Tournament from, bool reservesCantBePromoted)
        {
            List<Club> reservesAutomaticallyRelegated = new List<Club>();
            Qualification qMock = new Qualification(1, 0, new QualificationTournament(from), true, 0);
            foreach(Club concernedClub in ranking)
            {
                if (RuleIsRespected(concernedClub, qMock, from, reservesCantBePromoted).HasFlag(RuleStatus.RuleRelegation) && (association == null || association.ContainsAssociation(concernedClub.Association())))
                {
                    reservesAutomaticallyRelegated.Add(concernedClub);
                }
            }
            return reservesAutomaticallyRelegated;
        }

        public static List<Club> GetFullRankingInversed(GroupsRound round, Association association)
        {
            List<Club> ranking = new List<Club>();
            for (int i = -1; i > -round.maxClubsInGroup - 1; i--)
            {
                List<Club> rankingI = round.RankingByRank(i, association);
                rankingI.Reverse();
                ranking.AddRange(rankingI);
            }
            return ranking;
        }

        public static List<Club> GetFullRanking(GroupsRound round)
        {
            List<Club> ranking = new List<Club>();
            for(int i = 0; i < round.maxClubsInGroup; i++)
            {
                List<Club> rankingI = round.RankingByRank(i+1, null);
                ranking.AddRange(rankingI);
            }
            return ranking;
        }


        /// <summary>
        /// Clear all relegation places and replace them by qualifications to current tournament. Qualifications to play-offs are not affected
        /// Assume no relegations playoffs, because not managed ! (except for Championship Round)
        /// </summary>
        /// <param name="initialQualifications">Qualifications to changes</param>
        /// <param name="baseTournament">Base tournament</param>
        /// <returns>New list of qualifications where relegations were removed</returns>
        public static List<Qualification> ClearRelegations(List<Qualification> initialQualifications, Tournament baseTournament)
        {
            List<Qualification> qualifications = new List<Qualification>();
            foreach(Qualification q in initialQualifications)
            {
                if(q.isNextYear && baseTournament.IsAbove(q.target) && q.qualifies <= 0)
                {
                    qualifications.Add(new Qualification(q.ranking, q.roundId, new QualificationTournament(baseTournament), q.isNextYear, 0));
                }

                else if (q.isNextYear || baseTournament.IsBelow(q.target) || (q.target.Tournament() == baseTournament && q.qualifies <= 0))
                {
                    qualifications.Add(q);
                }
                else if(!q.isNextYear && baseTournament.IsAbove(q.target))
                {
                    qualifications.Add(new Qualification(q.ranking, q.roundId, new QualificationTournament(baseTournament), q.isNextYear, q.qualifies));
                }
            }
            return qualifications;
        }

        /*/// <summary>
        /// Update a specific qualification in a list of qualifications
        /// </summary>
        /// <param name="qualifications">List of qualifications</param>
        /// <param name="ranking">Update qualification at this rank</param>
        /// <param name="tournament">New qualification tournament</param>
        /// <param name="qualifies">New number of qualified for this qualification</param>
        /// <param name="eraseQualificationSameYear">Update qualification even if its a same year qualification</param>
        /// <param name="eraseRoundId">New qualification round id (default leave)</param>
        public static void UpdateQualificationTournament(List<Qualification> qualifications, int ranking, Tournament tournament, int qualifies=0, bool eraseQualificationSameYear=false, int eraseRoundId=-1)
        {
            //TODO : Maybe add a check if the ranking is neutral
            for (int j = 0; j < qualifications.Count; j++)
            {
                Qualification q = qualifications[j];
                if ((q.isNextYear || eraseQualificationSameYear) && q.target.ToChampionshipTournament() && (q.roundId == 0 || eraseQualificationSameYear ) && q.ranking == ranking)
                {
                    qualifications[j] = new Qualification(q.ranking, eraseRoundId > -1 ? eraseRoundId : q.roundId, new QualificationTournament(tournament), q.isNextYear, qualifies);
                }
            }
        }*/

        /// <summary>
        /// Update a specific qualification in a list of qualifications
        /// </summary>
        /// <param name="qualifications">List of qualifications</param>
        /// <param name="ranking">Update qualification at this rank</param>
        /// <param name="target">New qualification tournament</param>
        /// <param name="qualifies">New number of qualified for this qualification</param>
        /// <param name="eraseQualificationSameYear">Update qualification even if its a same year qualification</param>
        /// <param name="eraseRoundId">New qualification round id (default leave)</param>

        public static void UpdateQualificationTournament(List<Qualification> qualifications, int ranking, QualificationTarget target, int qualifies = 0, bool eraseQualificationSameYear = false, int eraseRoundId = -1)
        {
            //TODO : Maybe add a check if the ranking is neutral
            for (int j = 0; j < qualifications.Count; j++)
            {
                Qualification q = qualifications[j];
                if ((q.isNextYear || eraseQualificationSameYear) && q.target.ToChampionshipTournament() && (q.roundId == 0 || eraseQualificationSameYear) && q.ranking == ranking)
                {
                    qualifications[j] = new Qualification(q.ranking, eraseRoundId > -1 ? eraseRoundId : q.roundId, target, q.isNextYear, qualifies);
                }
            }
        }

        public static bool RankLeadingToRelegationBarrage(List<Qualification> qualifications, int rank, List<Round> relegationBarrageRounds)
        {
            bool res = false;
            foreach(Qualification q in qualifications)
            {
                if(q.ranking == rank && !q.isNextYear && relegationBarrageRounds.Contains(q.target.Tournament()?.rounds[q.roundId]))
                {
                    res = true;
                }
            }
            return res;
        }

        public static Dictionary<QualificationType, List<Qualification>> ComputeRoundDestinations(Tournament tournament, List<Qualification> initialQualifications, List<Round> relegationBarrageRounds, List<Round> promotionBarrageRounds)
        {
            //== Déplacable dans une autre fonction
            Dictionary<QualificationType, List<Qualification>> roundQualifications = new Dictionary<QualificationType, List<Qualification>>
            {
                { QualificationType.PossiblePromotion, new List<Qualification>() },
                { QualificationType.PossibleRelegation, new List<Qualification>() },
                { QualificationType.DirectPromotion, new List<Qualification>() },
                { QualificationType.DirectRelegation, new List<Qualification>() },
            };
            initialQualifications = new List<Qualification>(initialQualifications);
            initialQualifications.Sort(new QualificationRankingComparator());
            foreach (Qualification q in initialQualifications)
            {
                if (!q.isNextYear && relegationBarrageRounds.Contains(q.target.Tournament()?.rounds[q.roundId]))
                {
                    roundQualifications[QualificationType.PossibleRelegation].Add(q);
                }
                if (!q.isNextYear && promotionBarrageRounds.Contains(q.target.Tournament()?.rounds[q.roundId]))
                {
                    roundQualifications[QualificationType.PossiblePromotion].Add(q);
                }
                if(q.isNextYear && tournament.IsBelow(q.target))
                {
                    roundQualifications[QualificationType.DirectPromotion].Add(q);
                }
                if(q.isNextYear && tournament.IsAbove(q.target))
                {
                    roundQualifications[QualificationType.DirectRelegation].Add(q);
                }
            }
            // Cas un peu spécial où des qualifications vers une autre phase de ligue qui n'implique pas de promotion/relégation. On les ajoute à la liste des promotions car elles seront ajoutées dans l'ordre de classement aux clubs
            foreach (Qualification q in initialQualifications)
            {
                if (!q.isNextYear && (!relegationBarrageRounds.Contains(q.target.Tournament()?.rounds[q.roundId])) && (!promotionBarrageRounds.Contains(q.target.Tournament()?.rounds[q.roundId])))
                {
                    roundQualifications[QualificationType.PossiblePromotion].Add(q);
                }
            }

            // == Security check
            int specialQualifications = 0;
            foreach (Qualification q in initialQualifications)
            {
                if (!q.isNextYear || q.roundId > 0)
                {
                    specialQualifications++;
                }
            }
            if (specialQualifications != roundQualifications[QualificationType.PossiblePromotion].Count + roundQualifications[QualificationType.PossibleRelegation].Count)
            {
                throw new Exception("Some qualifications were not taken into account");
            }
            // == End of security check

            return roundQualifications;


        }

        [Obsolete("Please use AdjustQualificationsToReserves instead")]
        public static List<Qualification> AdjustQualificationsToNotPromoteReservesOld(List<Qualification> initialQualifications, List<Club> ranking, Association association, Tournament from, Round round, bool reservesCantBePromoted, int totalRelegations, int totalPromotions, int groupsCount)
        {
            
            List<Qualification> qualifications = new List<Qualification>(initialQualifications);
            List<int> fixedRelegations = new List<int>(); // Contains ranking of teams that can't be saved

            // Get new relegation zone taking account of retrograded reserves
            Console.WriteLine("[AdjustQualificationsToNotPromoteReserves] " + from.name + ", " + totalRelegations + " relegations for " + groupsCount + " groups. Association : " + association);
            List<Club> roundClubs = round.clubs;
            List<Club> automaticallyRelegatedReserves = ReservesAutomaticallyRelegated(roundClubs, null, from, reservesCantBePromoted);
            List<Club> fullInverseRanking = GetFullRankingInversed(round as GroupsRound, null);
            GroupsRound gRound = round as GroupsRound;
            //int groupsCount = (gRound != null) ? (association != null ? gRound.GetGroupsFromAssociation(association).Count : gRound.groups.Length) : 1;
            int regularRelegationPlaces = totalRelegations - automaticallyRelegatedReserves.Count;
            bool limitReached = false;
            Console.WriteLine("automaticallyRelegatedReserves : " + automaticallyRelegatedReserves.Count);
            Console.WriteLine("regularRelegationPlaces : " + totalRelegations + "-" + automaticallyRelegatedReserves.Count + "=" + regularRelegationPlaces);
            int index = 0;
            int regularRelegationCount = 0;

            QualificationTarget bottomTournament = Session.Instance.Game.kernel.LocalisationTournament(from).LeagueBelow(from);
            if (automaticallyRelegatedReserves.Count > 0 && regularRelegationPlaces > 0 && bottomTournament != null)
            {
                while (!limitReached)
                {
                    Club currentClub = fullInverseRanking[index];
                    bool isAutoRelegated = automaticallyRelegatedReserves.Contains(currentClub);
                    if (!isAutoRelegated)
                    {
                        regularRelegationCount++;
                    }
                    if (regularRelegationCount == regularRelegationPlaces)
                    {
                        limitReached = true;
                    }
                    //Should not happens
                    else if (index > fullInverseRanking.Count)
                    {
                        limitReached = true;
                        throw new Exception("Should not happen");
                    }
                    index++;
                }
                int newRelegationPlaces = index / groupsCount; // 12 / 5 -> 2 last places leading to relegation
                int additionalRelegationPlaces = index % groupsCount; // 12 % 5 -> 2 additional relegation slot
            
                Console.WriteLine("new relegations places : {0} (+{1})", newRelegationPlaces, additionalRelegationPlaces);
                if(bottomTournament != null)
                {
                    
                    int rankingRelegationLimit = qualifications.Where(x => bottomTournament.SameLevel(x.target)).Min(x => x.ranking);
                    int maxRanking = qualifications.Max(x => x.ranking);

                    // ================================================
                    //Special feature for championship round :
                    //If relegation barrage, move them just up direct relegations places so there is no offset between barrage places and direct relegations places
                    List<Qualification> relegationsBarrages = new List<Qualification>();
                    //Get all barrages qualifications places
                    Round relegationBarrageFinalRound = from.GetFinalTopPlayOffRound(true);
                    List<Round> relegationBarrageRounds = from.GetPlayOffsTree(relegationBarrageFinalRound.Tournament, relegationBarrageFinalRound, new List<Round>());
                    rankingRelegationLimit--;
                    while (RankLeadingToRelegationBarrage(qualifications, rankingRelegationLimit, relegationBarrageRounds))
                    {
                        foreach(Qualification q in qualifications)
                        {
                            if(q.ranking == rankingRelegationLimit)
                            {
                                relegationsBarrages.Add(q);
                            }
                        }
                        rankingRelegationLimit--;
                    }
                    // ================================================

                    qualifications = ClearRelegations(qualifications, from);
                    for (int i = maxRanking; i > maxRanking - newRelegationPlaces; i--)
                    {
                        Console.WriteLine("=> Update Ranking {0} to {1}", i, bottomTournament.ToString());
                        UpdateQualificationTournament(qualifications, i, bottomTournament);
                    }

                    //NOT TESTED.
                    //For championship rounds, relegation barrages are allowed. If ever there are reserves automatically relegated (probably very rare),
                    //so we move relegation barrages according to match with new relegations places
                    int barrageIndex = maxRanking - newRelegationPlaces;
                    foreach (Qualification q in relegationsBarrages)
                    {
                        Console.WriteLine("=> Update barrage Ranking {0} to {1} ", q.ranking, q.target.ToString());
                        UpdateQualificationTournament(qualifications, barrageIndex, q.target, q.qualifies, true, q.roundId);
                        barrageIndex--;
                    }
                    
                    if (groupsCount > 1 && additionalRelegationPlaces != 0)
                    {
                        bool teamOfGroupIsUp = gRound.TeamIsTopRBottom(ranking, maxRanking - newRelegationPlaces, groupsCount - additionalRelegationPlaces, null);
                        QualificationTarget targetTournament = teamOfGroupIsUp ? new QualificationTournament(from) : bottomTournament;
                        UpdateQualificationTournament(qualifications, maxRanking - newRelegationPlaces, targetTournament);
                        Console.WriteLine("=> Update Ranking {0} to {1}, (line separation {2})", (maxRanking - newRelegationPlaces), targetTournament.ToString(), (-additionalRelegationPlaces));
                    }
                }
            }

            // == Step 2 == Readapt qualifications

            for (int i = 0; i<qualifications.Count && ranking.Count > 0; i++)
            {
                qualifications.Sort(new QualificationRankingComparator());
                Qualification q = qualifications[i];
                Club concernedClub = ranking[q.ranking - 1];
                RuleStatus ruleStatus = RuleIsRespected(concernedClub, q, from, reservesCantBePromoted);
                bool ok = ruleStatus.HasFlag(RuleStatus.RuleRespected);
                bool ruleRelegation = ruleStatus.HasFlag(RuleStatus.RuleRelegation);
                if (ruleRelegation)
                {
                    ok = q.isNextYear && (!from.IsBelow(q.target));
                }
                int j = i;
                if (from.isChampionship)
                {
                    while (!ok)
                    {
                        int tempValue = qualifications[j].ranking;
                        Qualification first = qualifications[j];
                        Qualification second = qualifications[j + 1];
                        Console.WriteLine("Swap {0}. =>{1} and {2} . =>{3} ", (j+1), qualifications[j].target.Tournament(concernedClub).shortName, (j+2), qualifications[j+1].target.Tournament(concernedClub).shortName);
                        first.ranking = qualifications[j + 1].ranking;
                        second.ranking = tempValue;
                        qualifications[j] = first;
                        qualifications[j + 1] = second;
                        ruleStatus = RuleIsRespected(concernedClub, qualifications[j], from, reservesCantBePromoted);

                        ok = !ruleRelegation ? ruleStatus.HasFlag(RuleStatus.RuleRespected) : qualifications[j].isNextYear && (from.IsAbove(qualifications[j].target) || from.IsSameLevel(qualifications[j].target));
                        Console.WriteLine("[RuleIsRespected] ? {0} pour accéder à {1} (iny: {2}, ri: {3}) : {4}", concernedClub.name, qualifications[j].target.Tournament(concernedClub).name, qualifications[j].isNextYear, qualifications[j].roundId, ok);
                        j++;
                    }
                    if(ruleRelegation && bottomTournament != null && regularRelegationPlaces > 0)
                    {
                        UpdateQualificationTournament(qualifications, qualifications[j].ranking, bottomTournament, 0, true, 0);
                    }
                }
            }

            // == Step 3 == Tests
            bool playsOff = false;
            int newTotalRelegations = index + automaticallyRelegatedReserves.Count;
            int newTotalPromotions = 0;
            foreach(Qualification q in qualifications)
            {
                playsOff = playsOff || !q.isNextYear;
                /*if(from.IsBelow(q.target))
                {
                    newTotalPromotions++;
                }
                if (from.IsAbove(q.target))
                {
                    newTotalRelegations++;
                }*/
            }
            if (bottomTournament != null && automaticallyRelegatedReserves.Count > 0 && !playsOff && totalRelegations != newTotalRelegations)
            {
                Console.WriteLine("Danger. Total relegations slots computed ({0}) differ from original relegation slots count {1}) ({2}, {3})", newTotalRelegations, totalRelegations, from.name, association.name);
                //throw new Exception("Total relegations slots computed differ from original relegation slots count");
            }
            /*if (!playsOff && totalPromotions != newTotalPromotions)
            {
                throw new Exception("Total promotions slots computed differ from original promotions slots count");
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
            KMeansClustering kmeans = new KMeansClustering(clubs, clustersCount, new List<Club>());
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

        public static string ClubStatus2ResourceString(ClubStatus status)
        {
            string res = "";
            switch (status)
            {
                case ClubStatus.Professional:
                    res = "str_pro";
                    break;
                case ClubStatus.SemiProfessional:
                    res = "str_semipro";
                    break;
                case ClubStatus.Amateur:
                    res = "str_amateur";
                    break;
                default:
                    res = "";
                    break;
            }
            return res;
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

        public static bool EqualsDate(this DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day;
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

        /// <summary>
        /// Util function for debugging purpose.
        /// Check if a team is entered more than one time in a cup
        /// </summary>
        /// <param name="c"></param>
        public static void CheckDuplicates(Association a)
        {
            Console.WriteLine("[Search for duplicates]");
            foreach(Tournament t in a.Cups())
            {
                foreach(Round r in t.rounds)
                {
                    List<Club> clubs = new List<Club>();
                    List<Club> duplicates = new List<Club>();
                    foreach(Match m in r.matches)
                    {
                        List<Club> matchClubs = new List<Club>() { m.home, m.away };
                        foreach(Club mc in matchClubs)
                        {
                            if (clubs.Contains(mc))
                            {
                                duplicates.Add(mc);
                            }
                            else
                            {
                                clubs.Add(mc);
                            }
                        }
                    }
                    Console.WriteLine(string.Format("[{0}, {1}] Duplicates : {2}", t.name, r.name, duplicates.Count));
                    foreach(Club dc in duplicates)
                    {
                        Console.WriteLine(dc.name);
                    }
                }
            }
            Console.WriteLine("[Search for duplicates finished]");
        }

        public static int GroupCount(int totalTeams, int defaultMaxTeamsByGroup)
        {
            int groupCount = totalTeams / defaultMaxTeamsByGroup + (totalTeams % defaultMaxTeamsByGroup != 0 ? 1 : 0);
            return groupCount;
        }

        public static List<int> GetGroupSize(int totalTeams, int groupCount)
        {
            int ecart = groupCount > 0 ? totalTeams % groupCount : totalTeams;
            List<int> res = new List<int>();
            for (int i = 0; i < groupCount; i++)
            {
                int add = i < ecart ? 1 : 0;
                res.Add(totalTeams / groupCount + add);
            }
            return res;
        }

        public static int CountChampionshipQualifications(List<Qualification> qualifications)
        {
            int countChampionshipQualifications = 0;
            foreach (Qualification qualification in qualifications)
            {
                if (qualification.target.ToChampionshipTournament())
                {
                    countChampionshipQualifications++;
                }
            }
            return countChampionshipQualifications;
        }

        public static List<T> Inverse<T>(List<T> clubs)
        {
            List<T> newList = new List<T>(clubs);
            newList.Reverse();
            return newList;
        }

        public static int CompareQualificationTargets(QualificationTarget x, QualificationTarget y)
        {
            int xALevel = x.GetAssociationLevel();
            int yALevel = y.GetAssociationLevel();
            int res = xALevel - yALevel;
            if (res == 0)
            {
                int xTLevel = x.GetTournamentLevel();
                int yTLevel = y.GetTournamentLevel();
                res = xTLevel - yTLevel;
            }
            return res;
        }

        public static List<Club> SortClubsAttribute(List<Club> clubs, ClubAttribute attribute)
        {
            List<Club> pot = new List<Club>(clubs);
            pot.Sort(new ClubComparator(attribute, false));
            if (pot[0] as NationalTeam != null)
            {
                List<NationalTeam> nationalsTeams = new List<NationalTeam>();
                foreach (Club c in pot)
                {
                    nationalsTeams.Add(c as NationalTeam);
                }
                nationalsTeams.Sort(new NationsFifaRankingComparator(false));
                pot.Clear();
                foreach (NationalTeam nt in nationalsTeams)
                {
                    pot.Add(nt);
                }
            }
            return pot;
        }
    }
}