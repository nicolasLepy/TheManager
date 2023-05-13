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

        public ChampionshipRound(string name, Hour hour, List<GameDay> days, bool twoLegs, int phases, List<TvOffset> offsets, GameDay initialisation, GameDay end, int keepRankingFromPreviousRound, int lastDaySameDay, int gamesPriority) : base(name, hour, days, offsets, initialisation,end, twoLegs, phases, lastDaySameDay, keepRankingFromPreviousRound, gamesPriority)
        {
        }

        public override Round Copy()
        {
            Round t = new ChampionshipRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), twoLegs, phases, new List<TvOffset>(programmation.tvScheduling), programmation.initialisation, programmation.end, keepRankingFromPreviousRound, programmation.lastMatchDaysSameDayNumber, programmation.gamesPriority);
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
            foreach (KeyValuePair<Club, List<PointDeduction>> sanctions in this.pointsDeduction)
            {
                t.pointsDeduction.Add(sanctions.Key, sanctions.Value);
            }
            t.rules.AddRange(rules);
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
                            if(cc.baseCityAttendanceMultiplier == 0)
                            {
                                float cityAttendanceMultiplier = 1;
                                if(cc.supporters > 0)
                                {
                                    cityAttendanceMultiplier = cc.supporters / (potentialSupporters+0.0f);
                                }
                                cc.baseCityAttendanceMultiplier = cityAttendanceMultiplier;
                            }
                            c.supporters += (int)(((potentialSupporters - c.supporters) * (Session.Instance.Random(5, 12) / 100.0f))*cc.baseCityAttendanceMultiplier);
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
            CheckConflicts();
        }

        /// <summary>
        /// Get the potential of attendance for a club in function of its level, the size of the city and its championship
        /// </summary>
        /// <param name="club"></param>
        /// <returns></returns>
        public int GetPotentialSupporters(CityClub club, int tournamentLevel)
        {
            int cityPopulation = club.city.Population;
            float clubLevel = club.Level();

            double lowerSigmoid = 1 / (1 + Math.Exp(-0.0002 * (cityPopulation - 17500)));
            double upperSigmoid = 1 - (1 / (1 + Math.Exp(-0.00000205 * (cityPopulation - 1900000))));
            double populationDivider = upperSigmoid * lowerSigmoid * (7 + (1 * Math.Pow(1.7, 0.000004 * cityPopulation)));
            return (int)((cityPopulation / populationDivider) / tournamentLevel * (clubLevel / 70));
        }

        /*
         * Identical logic with GetGroupQualification of GroupRound : merge function on Round class
         */
        public List<Qualification> GetQualifications()
        {
            List<Qualification> adjustedQualifications = new List<Qualification>(qualifications);
            //Adapt qualifications to adapt negative ranking to real ranking in the group

            List<int> allRankings = Enumerable.Range(1, clubs.Count).ToList();

            for (int i = 0; i < adjustedQualifications.Count; i++)
            {
                Qualification q = adjustedQualifications[i];
                if (q.ranking < 0)
                {
                    adjustedQualifications[i] = new Qualification(clubs.Count + q.ranking + 1, q.roundId, q.tournament, q.isNextYear, q.qualifies);
                }

                // This ranking have a qualification to another round, remove it from the list
                allRankings.Remove(adjustedQualifications[i].ranking);
            }

            // Add qualification to every ranking with no qualifications
            if (Tournament.isChampionship)
            {
                foreach (int remainingRanking in allRankings)
                {
                    adjustedQualifications.Add(new Qualification(remainingRanking, 0, Tournament, true, 0));
                }
            }
            return adjustedQualifications;
        }

        //TODO : Gestion des relégations administratives
        /*
        private void RequalifyClubsAdministrativeRetrogradation()
        {
            Tournament tournament = Tournament;
            List<KeyValuePair<Club, Tournament>> qualifications = new List<KeyValuePair<Club, Tournament>>();
            List<Club> ranking = Ranking();
            foreach(Club c in ranking)
            {
                qualifications.Add(new KeyValuePair<Club, Tournament>(c, tournament));
            }

            Country country = Session.Instance.Game.kernel.LocalisationTournament(tournament) as Country;
            for(int i = tournament.level-1; i < tournament.level+2; i++)
            {
                Tournament tournamentI = country.League(i);
                if(tournamentI != null)
                {
                    List<Club> nextYearQualified = tournamentI.nextYearQualified[0];
                    for(int j = 0; j < qualifications.Count; j++)
                    {
                        KeyValuePair<Club, Tournament> kvp = qualifications[j];
                        if (nextYearQualified.Contains(kvp.Key))
                        {
                            qualifications[j] = new KeyValuePair<Club, Tournament>(kvp.Key, tournamentI);
                        }
                    }
                }
            }

            for(int i = 0; i<ranking.Count; i++)
            {
                Club c = ranking[i];
                KeyValuePair<Club, Tournament> qualification = qualifications[i];
                foreach (KeyValuePair<Club, Tournament> retrogradation in country.administrativeRetrogradations)
                {
                    if (retrogradation.Key == c)
                    {
                        List<Qualification> roundQualifications = GetQualifications();
                        //Le club est promu
                        if (qualification.Value.level < tournament.level)
                        {
                            qualification.Value.nextYearQualified[0].Remove(c);

                            //Ajouter le premier club non promu à être promu s'il respecte les règles
                            bool ok = false;
                            for(int j = 0; j < ranking.Count && !ok; j++)
                            {
                                Club candidate = ranking[j];
                                KeyValuePair<Club, Tournament> nextYearCandidate = qualifications[j];
                                if (nextYearCandidate.Value == tournament && Utils.RuleIsRespected(candidate, roundQualifications[j], tournament.level, tournament.rounds[0].rules.Contains(Rule.ReservesAreNotPromoted)) == Utils.RuleStatus.RuleRespected)
                                {
                                    ok = true;
                                    qualification.Value.nextYearQualified[0].Add(candidate);
                                    tournament.nextYearQualified[0].Remove(candidate);

                                }
                            }
                        }
                        if(qualification.Value.level == tournament.level)
                        {
                            //Sauver le premier candidat relegue s'il respecte les règles
                            bool ok = false;
                            for (int j = 0; j < ranking.Count && !ok; j++)
                            {
                                Club candidate = ranking[j];
                                KeyValuePair<Club, Tournament> nextYearCandidate = qualifications[j];
                                if (nextYearCandidate.Value.level > tournament.level && Utils.RuleIsRespected(candidate, roundQualifications[j], tournament.level, tournament.rounds[0].rules.Contains(Rule.ReservesAreNotPromoted)) == Utils.RuleStatus.RuleRespected)
                                {
                                    ok = true;
                                    nextYearCandidate.Value.nextYearQualified[0].Remove(candidate);
                                    tournament.nextYearQualified[0].Add(candidate);

                                }
                            }
                        }
                        retrogradation.Value.AddClubForNextYear(c, 0);
                    }
                }
            }
        }*/

        private List<Qualification> AdjustQualificationsAdministrativeRetrogradation(List<Qualification> qualifications, Country country)
        {
            Tournament tournament = Tournament;
            List<Club> ranking = Ranking();
            List<Qualification> newQualifications = new List<Qualification>(qualifications);
            newQualifications.Sort(new QualificationComparator());
            foreach (Club c in _clubs)
            {
                //Aussi if reserve && reserve >= cible descente
                if(country.administrativeRetrogradations.ContainsKey(c))
                {
                    Qualification q = newQualifications[ranking.IndexOf(c)];
                    if ((q.isNextYear && q.tournament.level < tournament.level) )
                    {

                    }
                    //Si promovable ou barrage, on change
                    //Si relegable, on change rien pour ce groupe
                }
            }

            //Check autres descend voir si ici on est concerné

            return newQualifications;
        }

        public override void QualifyClubs()
        {
            List<Club> ranking = Ranking();

            List<Club> qualifies = new List<Club>();
            List<Qualification> adjustedQualifications = AdaptQualificationsToRanking(new List<Qualification>(qualifications), clubs.Count);
            
            adjustedQualifications.Sort(new QualificationComparator());
            adjustedQualifications = Utils.AdjustQualificationsToNotPromoteReserves(adjustedQualifications, ranking, Tournament, rules.Contains(Rule.ReservesAreNotPromoted));
            
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
            Round previousRoundRanking = this.keepRankingFromPreviousRound > -1 ? this.Tournament.rounds[keepRankingFromPreviousRound] : null;
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches, tiebreakers, pointsDeduction, rankingType, false, false, previousRoundRanking);
            List<Club> ranking = new List<Club>(_clubs);
            ranking.Sort(comparator);
            return ranking;
        }

        public List<Club> RankingWithoutReserves(int group)
        {
            Round previousRoundRanking = this.keepRankingFromPreviousRound > -1 ? this.Tournament.rounds[keepRankingFromPreviousRound] : null;
            List<Club> res = new List<Club>();
            foreach (Club c in _clubs)
            {
                if ((c as ReserveClub) == null)
                {
                    res.Add(c);
                }
            }
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches, tiebreakers, pointsDeduction, RankingType.General, false, false, previousRoundRanking);
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
            List<Club> ranking = this.Ranking();
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