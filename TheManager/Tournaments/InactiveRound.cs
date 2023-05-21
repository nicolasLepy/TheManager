using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Storage;
using TheManager.Comparators;
using TheManager.Tournaments;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class InactiveRound : Round
    {

        private List<Club> _ranking;

        public InactiveRound(string name, Hour hour, GameDay initialisation, GameDay end) :base(name,hour,new List<GameDay>(),new List<TvOffset>(),initialisation,end,false,1, 0, -1, 0)
        {
            _ranking = null;
        }

        public override Round Copy()
        {
            Round t = new InactiveRound(name, this.programmation.defaultHour, programmation.initialisation, programmation.end);
            //Round t = new KnockoutRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), twoLegs, phases, programmation.initialisation, programmation.end, RandomDrawingMethod.Random, false);
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

        /// <summary>
        /// TODO: Distribute grants in inactive rounds
        /// </summary>
        public override void DistributeGrants()
        {
            // TODO: Not implemented yet
        }

        public override void Initialise()
        {
            if(!Tournament.IsInternational())
            {
                AddTeamsToRecover();
            }
            //If it's an international tournament (national teams or continental cup eg), we add all teams to recover for all rounds now because ranking can fluctuate after and the same team could be selected for 2 differents rounds
            else if (Tournament.rounds[0] == this)
            {
                foreach (Round r in Tournament.rounds)
                {
                    r.AddTeamsToRecover();
                }
            }
            _ranking = null;
            CheckConflicts();
        }

        public override List<Match> NextMatchesDay()
        {
            return new List<Match>();
        }

        public override Club Winner()
        {
            return _clubs[0];
        }

        public void SetRanking(List<Club> ranking)
        {
            _ranking = new List<Club>(ranking);
        }

        public List<Club> Ranking()
        {
            if (_ranking == null)
            {
                _ranking = new List<Club>(_clubs);
                try
                {
                    _ranking.Sort(new ClubRandomRankingComparator(5, -1));
                }
                catch
                {
                    Utils.Debug("Le classement aléatoire n'a pu être généré");
                }
            }
            return new List<Club>(_ranking);
        }

        public List<Qualification> AdjustQualificationAccordingToAdministrativeDivisions(List<Qualification> baseQualifications, GroupsRound upperGroupRound, List<Club> ranking)
        {
            Country c = _clubs[0].Country();
            List<Qualification> adjustedQualifications = new List<Qualification>(baseQualifications);
            adjustedQualifications.Sort(new QualificationComparator());

            int promotionsSlotByAd = 0;
            int relegationsSlotByAd = 0;
            Tournament lowerTournament = null;
            Tournament upperTournament = null;
            for (int i = 0; i < adjustedQualifications.Count; i++)
            {
                Qualification q = adjustedQualifications[i];
                if (q.isNextYear && q.roundId == 0 && q.tournament.level < Tournament.level)
                {
                    promotionsSlotByAd++;
                    upperTournament = q.tournament;
                    //Remove promotion because it will be automatically managed
                    adjustedQualifications[i] = new Qualification(q.ranking, q.roundId, Tournament, q.isNextYear, q.qualifies);
                }
                if (q.isNextYear && q.roundId == 0 && q.tournament.level > Tournament.level)
                {
                    relegationsSlotByAd++;
                    lowerTournament = q.tournament;
                    //Remove relegation because it will be automatically managed
                    adjustedQualifications[i] = new Qualification(q.ranking, q.roundId, Tournament, q.isNextYear, q.qualifies);
                }
            }

            //TODO: Differents relegations count by ADM not taken account
            int upperRegularRelegations = 0;
            foreach (Qualification q in upperGroupRound.qualifications)
            {
                if (q.isNextYear && q.roundId == 0 && q.tournament.level > upperGroupRound.Tournament.level)
                {
                    upperRegularRelegations++;
                }
            }
            foreach (AdministrativeDivision ad in c.GetAdministrativeDivisionsLevel(upperGroupRound.administrativeLevel))
            {
                
                List<int> admGroups = upperGroupRound.GetGroupsFromAdministrativeDivision(ad);
                int upperRelegations = 0;

                foreach (int admGroup in admGroups)
                {
                    foreach (Qualification q in upperGroupRound.GetGroupQualifications(admGroup))
                    {
                        if (q.isNextYear && q.roundId == 0 && q.tournament.isChampionship && q.tournament.level > upperGroupRound.Tournament.level)
                        {
                            upperRelegations++;
                        }
                    }
                }

                int additionnalUpperRelegations = upperRelegations - upperRegularRelegations;
                int relegations = relegationsSlotByAd > 0 ? relegationsSlotByAd + additionnalUpperRelegations : relegationsSlotByAd;
                
                Console.WriteLine("[" + Tournament.name + "][Ad " + ad.name + "] -> Rélégués du haut : " + upperRelegations + "(" + upperRegularRelegations + " normalement). " + relegations + " relegations");

                int promotions = promotionsSlotByAd;
                //Manage promotion
                for (int i = 0; i < ranking.Count && promotions > 0; i++)
                {
                    Club club = ranking[i];
                    if (ad.ContainsAdministrativeDivision(club.AdministrativeDivision()))
                    {
                        for (int j = 0; j < adjustedQualifications.Count; j++)
                        {
                            Qualification q = adjustedQualifications[j];
                            if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == i+1)
                            {
                                adjustedQualifications[j] = new Qualification(q.ranking, q.roundId, upperTournament, q.isNextYear, 0);
                            }
                        }
                        promotions--;
                    }
                }

                //Manage relegation
                //Manage when there is not relegation because there is no ADM team in the lower league
                int lowerRoundTeamsCount = 0;
                if (lowerTournament != null)
                {
                    lowerRoundTeamsCount = lowerTournament.rounds[0].GetClubsAdministrativeDivision(ad).Count;
                    Console.WriteLine("[" + lowerTournament.name + "][Ad " + ad.name + "] " + lowerRoundTeamsCount + " équipes de l'ADM, " + relegations +  " relegations.");
                }
                if (lowerRoundTeamsCount > 0)
                {
                    for (int i = ranking.Count - 1; i >= 0 && relegations > 0; i--)
                    {
                        Club club = ranking[i];
                        if (ad.ContainsAdministrativeDivision(club.AdministrativeDivision()))
                        {
                            for (int j = 0; j < adjustedQualifications.Count; j++)
                            {
                                Qualification q = adjustedQualifications[j];
                                if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == i+1)
                                {
                                    adjustedQualifications[j] = new Qualification(q.ranking, q.roundId, lowerTournament, q.isNextYear, 0);
                                }
                            }
                            relegations--;
                        }
                    }
                }
            }

            return adjustedQualifications;
        }

        public List<Qualification> GetQualifications()
        {
            List<Club> ranking = Ranking();
            List<Qualification> adjustedQualifications = new List<Qualification>(_qualifications);
            adjustedQualifications.Sort(new QualificationComparator());

            adjustedQualifications = AdaptQualificationsToRanking(adjustedQualifications, clubs.Count);

            adjustedQualifications = Utils.AdjustQualificationsToNotPromoteReserves(adjustedQualifications, ranking, Tournament, _rules.Contains(Rule.ReservesAreNotPromoted));

            if (_clubs.Count > 0)
            {
                //Get last non inactive division with administrative random drawing method
                GroupsRound upperGroupRound = null;
                int level = Tournament.level - 1;
                while (upperGroupRound == null && level > 0)
                {
                    upperGroupRound = (_clubs[0].Country().League(level)?.rounds[0] as GroupsRound);
                    if (upperGroupRound != null && upperGroupRound.RandomDrawingMethod != RandomDrawingMethod.Administrative)
                    {
                        upperGroupRound = null;
                    }
                    level--;
                }
                if (upperGroupRound != null)
                {
                    adjustedQualifications = AdjustQualificationAccordingToAdministrativeDivisions(adjustedQualifications, upperGroupRound, ranking);
                }
            }
            return adjustedQualifications;
        }

        public override void QualifyClubs(bool forNextYear)
        {

            List<Club> ranking = Ranking();
            //Estimate incomes dues to matchs (attendance)

            DateTime fin = DateEndRound();
            DateTime dateInitialisationRound = DateInitialisationRound();
            if (fin.Month < dateInitialisationRound.Month)
            {
                fin = fin.AddYears(1);
            }
            else if (fin.Month == dateInitialisationRound.Month && fin.Day < dateInitialisationRound.Day)
            {
                fin = fin.AddYears(1);
            }
            int matchesCount = (int)((fin - dateInitialisationRound).TotalDays) / 14;
            foreach (Club c in ranking)
            {
                CityClub cv = c as CityClub;
                cv?.ModifyBudget(matchesCount * cv.supporters * cv.ticketPrice, BudgetModificationReason.TournamentGrant);
            }

            List<Qualification> adjustedQualifications = GetQualifications();
            
            foreach (Qualification q in adjustedQualifications)
            {
                Club c = ranking[q.ranking - 1];
                if (Tournament.level >= 8)
                {
                    Console.WriteLine("[" + q.ranking + "] " + c.name + " (" + c.AdministrativeDivision().name + ") -> " +
                                      q.tournament.level);
                }
                if (!q.isNextYear && !forNextYear)
                {
                    q.tournament.rounds[q.roundId].clubs.Add(c);
                }
                else if(q.isNextYear && forNextYear)
                {
                    q.tournament.AddClubForNextYear(c, q.roundId);
                }
                if (q.tournament.isChampionship && c.Championship != null)
                {
                    if (q.tournament.level > c.Championship.level)
                    {
                        c.supporters = (int)(c.supporters * 1.4f);                        
                    }
                    else if (q.tournament.level < c.Championship.level)
                    {
                        c.supporters = (int)(c.supporters / 1.4f);
                    }
                }
            }
        }

        public override int MatchesDayNumber()
        {
            return 0;
        }

        public override List<Match> GamesDay(int journey)
        {
            return new List<Match>();
        }
    }
}