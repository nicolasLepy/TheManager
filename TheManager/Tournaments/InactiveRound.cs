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
    public class InactiveRound : Round
    {


        private List<Club> _ranking;

        public InactiveRound(string name, Hour hour, GameDay initialisation, GameDay end) :base(name,hour,new List<GameDay>(),new List<TvOffset>(),initialisation,end,false,0)
        {
            _ranking = null;
        }

        public override Round Copy()
        {
            Round t = new KnockoutRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), twoLegs, programmation.initialisation, programmation.end, RandomDrawingMethod.Random);
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
                if(cv != null)
                {
                    cv.ModifyBudget(matchesCount * cv.supporters * cv.ticketPrice, BudgetModificationReason.TournamentGrant);
                }
            }

            List<Qualification> adjustedQualifications = new List<Qualification>(_qualifications);
            adjustedQualifications.Sort(new QualificationComparator());

            if (_rules.Contains(Rule.ReservesAreNotPromoted))
            {
                adjustedQualifications = Utils.AdjustQualificationsToNotPromoteReserves(_qualifications, ranking, Tournament);
            }

            foreach (Qualification q in adjustedQualifications)
            {
                Club c = ranking[q.ranking - 1];
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