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

        public InactiveRound(string name, Hour hour, GameDay initialisation, GameDay end) :base(name,hour,new List<GameDay>(),new List<TvOffset>(),initialisation,end,false,0)
        {
            _ranking = null;
        }

        public override Round Copy()
        {
            Round t = new KnockoutRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), twoLegs, programmation.initialisation, programmation.end, RandomDrawingMethod.Random, false);
            foreach (Match m in this.matches)
            {
                t.matches.Add(m);
            }

            foreach (Club c in this.clubs)
            {
                t.clubs.Add(c);
            }
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

        }

        public override List<Match> NextMatchesDay()
        {
            return new List<Match>();
        }

        public override Club Winner()
        {
            return _clubs[0];
        }

        public List<Club> Ranking()
        {
            if(_ranking == null)
            {
                _ranking = new List<Club>(_clubs);
                try
                {
                    _ranking.Sort(new ClubRandomRankingComparator());
                }
                catch
                {
                    Utils.Debug("Le classement aléatoire n'a pu être généré");
                }
            }
            return _ranking;
        }

        public List<Qualification> AdjustQualificationAccordingToAdministrativeDivisions(List<Qualification> baseQualifications, GroupsRound upperGroupRound, List<Club> ranking)
        {
            Country c = _clubs[0].Country();
            List<Qualification> adjustedQualifications = new List<Qualification>(baseQualifications);
            adjustedQualifications.Sort(new QualificationComparator());
            foreach (AdministrativeDivision ad in c.GetAdministrativeDivisionsLevel(upperGroupRound.administrativeLevel))
            {
                List<int> admGroups = upperGroupRound.GetGroupsFromAdministrativeDivision(ad);
                int relegations = 0;

                foreach (Qualification q in upperGroupRound.qualifications)
                {
                    if (q.isNextYear && q.roundId == 0 && q.tournament.isChampionship && q.tournament == Tournament)
                    {
                        relegations++;
                    }
                }

                relegations = (admGroups.Count * relegations) + 1;

                /*
                foreach(int i in admGroups)
                {
                    List<Qualification> down = upperGroupRound.GetGroupQualifications(i);
                    int relegationPlaces = -1;
                    foreach (Qualification q in down)
                    {
                        if (q.tournament.level > upperGroupRound.Tournament.level && q.isNextYear && q.roundId == 0 &&
                            relegationPlaces == -1)
                        {
                            relegationPlaces = down.Count - q.ranking + 1;
                        }
                    }

                    relegations = relegationPlaces > -1 ? relegations + relegationPlaces : relegations;

                }

                relegations = relegations > 0 ? relegations + 1 : relegations;*/
                Console.WriteLine("[Ad " + ad.name + "] -> Rélégués : " + relegations);

                for (int i = 0; i < ranking.Count && relegations > 0; i++)
                {
                    Club club = ranking[i];
                    if (ad.ContainsAdministrativeDivision(club.AdministrativeDivision()))
                    {
                        for (int j = 0; j < adjustedQualifications.Count; j++)
                        {
                            Qualification q = adjustedQualifications[j];
                            if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == i+1)
                            {
                                adjustedQualifications[j] = new Qualification(q.ranking, q.roundId, upperGroupRound.Tournament,
                                    q.isNextYear, 0);
                            }
                        }
                        relegations--;
                    }
                }
            }

            return adjustedQualifications;
        }

        public override void QualifyClubs()
        {

            List<Club> ranking = Ranking();
            //Simuler des gains d'argent de matchs pour les clubs (affluence)

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

            List<Qualification> adjustedQualifications = new List<Qualification>(_qualifications);
            adjustedQualifications.Sort(new QualificationComparator());

            if (_rules.Contains(Rule.ReservesAreNotPromoted))
            {
                adjustedQualifications = Utils.AdjustQualificationsToNotPromoteReserves(_qualifications, ranking, Tournament);
            }

            if (_clubs.Count > 0)
            {
                GroupsRound upperGroupRound = (_clubs[0].Country().League(Tournament.level - 1)?.rounds[0] as GroupsRound);
                if (upperGroupRound != null &&
                    upperGroupRound.RandomDrawingMethod == RandomDrawingMethod.Administrative)
                {
                    adjustedQualifications = AdjustQualificationAccordingToAdministrativeDivisions(adjustedQualifications, upperGroupRound, ranking);
                }
            }
             
            
            foreach (Qualification q in adjustedQualifications)
            {
                Club c = ranking[q.ranking - 1];
                if (Tournament.level == 5)
                {
                    Console.WriteLine("[" + q.ranking + "] " + c.name + " (" + c.AdministrativeDivision().name + ") -> " +
                                      q.tournament.level);
                }
                if (!q.isNextYear)
                {
                    q.tournament.rounds[q.roundId].clubs.Add(c);
                }
                else
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