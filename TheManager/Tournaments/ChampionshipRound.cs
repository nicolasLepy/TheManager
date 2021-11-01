using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;
using TheManager.Tournaments;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class ChampionshipRound : Round
    {

        public ChampionshipRound(string name, Hour hour, List<GameDay> days, bool twoLegs, List<TvOffset> offsets, GameDay initialisation, GameDay end, int lastDaySameDay) : base(name, hour, days, offsets, initialisation,end, twoLegs, lastDaySameDay)
        {
        }

        public override Round Copy()
        {
            Round t = new ChampionshipRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), twoLegs, new List<TvOffset>(programmation.tvScheduling), programmation.initialisation, programmation.end, programmation.lastMatchDaysSameDayNumber);
            foreach (Match m in this.matches)
            {
                t.matches.Add(m);
            }

            foreach (Club c in this.clubs)
            {
                t.clubs.Add(c);
            }

            foreach(Qualification q in this.qualifications)
            {
                t.qualifications.Add(q);
            }
            return t;
        }

        public override void Initialise()
        {
            if(!Tournament.IsInternational())
            {
                AddTeamsToRecover();

                if(this.Tournament.isChampionship)
                {
                    foreach(Club c in clubs)
                    {
                        CityClub cc = c as CityClub;
                        if (cc != null)
                        {
                            int potentialSupporters = GetPotentialSupporters(cc, Tournament.level);
                            c.supporters += (int)((potentialSupporters - c.supporters) * (Session.Instance.Random(5, 12) / 100.0f));
                        }
                    }
                }
            }
            //If it's an international tournament (national teams or continental cup eg), we add all teams to recover for all rounds now because ranking can fluctuate after and the same team could be selected for 2 differents rounds
            else if(Tournament.rounds[0] == this)
            {
                foreach(Round r in Tournament.rounds)
                {
                    r.AddTeamsToRecover();
                }
            }
            _matches = Calendar.GenerateCalendar(this.clubs, this, twoLegs);

        }

        /// <summary>
        /// Get the potential of attendance for a club in function of its level, the size of the city and its championship
        /// </summary>
        /// <param name="club"></param>
        /// <returns></returns>
        public int GetPotentialSupporters(CityClub club, int tournamentLevel)
        {
            //TODO: Take in account situations like for RC Lens (high average attendance for a small city)
            int cityPopulation = club.city.Population;
            float clubLevel = club.Level();

            double lowerSigmoid = 1 / (1 + Math.Exp(-0.0002 * (cityPopulation - 17500)));
            double upperSigmoid = 1 - (1 / (1 + Math.Exp(-0.00000205 * (cityPopulation - 1900000))));
            double populationDivider = upperSigmoid * lowerSigmoid * (7 + (1 * Math.Pow(1.7, 0.000004 * cityPopulation)));
            return (int)((cityPopulation / populationDivider) / tournamentLevel * (clubLevel / 70));
        }

        public override void QualifyClubs()
        {
            List<Club> ranking = Ranking();

            List<Club> qualifies = new List<Club>();
            List<Qualification> adjustedQualifications = new List<Qualification>(qualifications);
            adjustedQualifications.Sort(new QualificationComparator());
            if (rules.Contains(Rule.ReservesAreNotPromoted))
            {
                adjustedQualifications = Utils.AdjustQualificationsToNotPromoteReserves(qualifications, ranking, Tournament);
            }
            foreach (Qualification q in adjustedQualifications)
            {
                Club c = ranking[q.ranking-1];
                if (!q.isNextYear)
                {
                    q.tournament.rounds[q.roundId].clubs.Add(c);
                }
                else
                {
                    q.tournament.AddClubForNextYear(c, q.roundId);
                }
                /*
                if(q.tournament.isChampionship && c.Championship != null)
                {
                    if (q.tournament.level > c.Championship.level)
                    {
                        c.supporters = (int)(c.supporters / 1.8f);
                    }
                    else if (q.tournament.level < c.Championship.level)
                    {
                        c.supporters = (int)(c.supporters * 1.8f);                    
                    }
                }*/
            }
        }

        public List<Club> Ranking(RankingType rankingType = RankingType.General)
        {
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches, rankingType);
            List<Club> ranking = new List<Club>(_clubs);
            ranking.Sort(comparator);
            return ranking;
        }

        public List<Club> RankingWithoutReserves(int group)
        {
            List<Club> res = new List<Club>();
            foreach (Club c in _clubs)
            {
                if ((c as ReserveClub) == null)
                {
                    res.Add(c);
                }
            }
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches);
            res.Sort(comparator);
            return res;
        }

        public override int MatchesDayNumber()
        {
            int nbTeams = clubs.Count;
            int nbMatches = matches.Count;
            if (nbTeams % 2 == 1)
            {
                nbTeams++;
            }
            int nbMatchesDays = nbMatches / nbTeams;
            if (twoLegs)
            {
                nbMatchesDays *= 2;
            }
            return nbMatchesDays ;
        }
        
        public override List<Match> NextMatchesDay()
        {
            List<Match> res;

            int indMatch = -1;
            DateTime earliest = new DateTime(2000, 1, 1);
            int i = 0;
            foreach (Match m in _matches)
            {
                if (!m.Played && (DateTime.Compare(m.day, earliest) < 0 || earliest.Year == 2000))
                {
                    earliest = m.day;
                    indMatch = i;
                }
                i++;
            }

            int total = MatchesByGamesDay();
            int matchesDay = (indMatch / total);
            res = GamesDay(matchesDay + 1); //+1 because matches day going from 0 to n-1
            res.Sort(new MatchDateComparator());

            return res;
        }

        /// <summary>
        /// Gameday of a match
        /// </summary>
        /// <param name="match">The match</param>
        /// <returns>The journey during which the match was played</returns>
        public int GameDay(Match match)
        {
            return (_matches.IndexOf(match) / MatchesByGamesDay()) + 1;
        }

        /// <summary>
        /// Number of matches by journey
        /// </summary>
        /// <returns></returns>
        private int MatchesByGamesDay()
        {
            int total = _clubs.Count;
            total /= 2;
            return total;
        }

        public override List<Match> GamesDay(int journey)
        {
            List<Match> res = new List<Match>();
            int total = MatchesByGamesDay();
            int index = journey - 1;
            for (int i = index * total; i < (index + 1) * total; i++)
            {
                res.Add(_matches[i]);
            }
            return res;
        }

        public override void DistributeGrants()
        {
            List<Club> ranking = new List<Club>(_clubs);
            ranking.Sort(new ClubRankingComparator(this.matches));
            foreach(Prize d in _prizes)
            {
                CityClub cc = ranking[d.Ranking-1] as CityClub;
                if (cc != null)
                {
                    cc.ModifyBudget(d.Amount, BudgetModificationReason.TournamentGrant);
                }
                    
            }
        }

        public override Club Winner()
        {
            return Ranking()[0];
        }
    }
}