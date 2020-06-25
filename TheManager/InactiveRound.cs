using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class InactiveRound : Round
    {

        public InactiveRound(string name, Hour hour, DateTime initialisation, DateTime end) :base(name,hour,new List<DateTime>(),new List<TvOffset>(),initialisation,end,false,0)
        { }

        public override Round Copy()
        {
            Round t = new KnockoutRound(name, this.programmation.defaultHour, new List<DateTime>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), twoLegs, programmation.initialisation, programmation.end);
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

        public override void DistributeGrants()
        {
        }

        public override void Initialise()
        {
            
        }

        public override List<Match> NextMatchesDay()
        {
            return new List<Match>();
        }

        public override Club Winner()
        {
            return _clubs[0];
        }

        public override void QualifyClubs()
        {
            List<Club> ranking = new List<Club>(_clubs);
            try
            {
                ranking.Sort(new ClubRandomRankingComparator());
            }
            catch
            {
                Console.WriteLine("Le classement aléatoire n'a pu être généré");
            }

            //Simuler des gains d'argent de matchs pour les clubs (affluence)


            DateTime fin = new DateTime(programmation.end.Year, programmation.end.Month, programmation.end.Day);
            if (fin.Month < programmation.initialisation.Month)
                fin = fin.AddYears(1);
            else if (fin.Month == programmation.initialisation.Month && fin.Day < programmation.initialisation.Day)
                fin = fin.AddYears(1);
            int nbMatchs = (int)((fin - programmation.initialisation).TotalDays) / 14;
            foreach (Club c in ranking)
            {
                CityClub cv = c as CityClub;
                if(cv != null)
                {
                    cv.ModifyBudget(nbMatchs * cv.supporters * cv.ticketPrice);
                }
            }
            foreach(Qualification q in _qualifications)
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
    }
}