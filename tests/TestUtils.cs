using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm;

namespace tests
{
    public class TestUtils
    {

        public static void PrintTournament(Tournament tournament)
        {
            Console.WriteLine("\n Tournament : {0}", tournament.name);
            foreach(Round round in tournament.rounds)
            {
                PrintRound(round);
            }
        }

        public static void PrintRound(Round round)
        {
            GroupsRound gr = round as GroupsRound;
            KnockoutRound kr = round as KnockoutRound;
            if(gr != null)
            {
                PrintRound(gr);
            }
            if(kr != null)
            {
                PrintRound(kr);
            }
        }

        public static void PrintLeagueSystem(Association association)
        {
            foreach (Tournament league in association.Leagues())
            {
                if (league.rounds.Count > 0)
                {
                    GroupsRound gr = league.rounds[0] as GroupsRound;
                    if (gr != null)
                    {
                        PrintRound(gr);
                    }
                }
            }
            foreach (Association a in association.associations)
            {
                PrintLeagueSystem(a);
            }
        }

        public static void PrintRound(KnockoutRound kr)
        {
            Console.WriteLine("\nRound : ----- {0} ----- {1} teams", kr.name, kr.clubs.Count);
            foreach(Match match in kr.matches)
            {
                PrintGame(match);
            }
        }

        public static void PrintGame(Match match)
        {
            string home = string.Format("{0} ({1})", match.home.name, match.home.Championship.shortName);
            string away = string.Format("{0} ({1})", match.away.name, match.away.Championship.shortName);
            string score1 = match.score1.ToString();
            string score2 = match.score2.ToString();
            string extra = "";
            if(match.prolongations)
            {
                extra = "p.";
            }
            if(match.PenaltyShootout)
            {
                extra = String.Format("{0} {1}-{2}p", extra, match.penaltyShootout1, match.penaltyShootout2);
            }
            Console.WriteLine("{0} - {1} {2}-{3} {4}", home.PadRight(40), away.PadRight(40), score1, score2, extra);
        }

        public static void PrintRound(GroupsRound gr)
        {
            Console.WriteLine("\nRanking : ----- {0} ----- {1} teams", gr.Tournament.name, gr.clubs.Count);
            for (int g = 0; g < gr.groupsCount; g++)
            {
                Console.WriteLine(gr.GroupName(g));
                int i = 0;
                List<Qualification> gq = gr.GetGroupQualifications(g);
                foreach (Club club in gr.Ranking(g))
                {
                    Tournament clubChampionship = club.Championship;
                    string clubName = club.extendedName(clubChampionship, Session.Instance.Game.date.Year).PadRight(40);

                    Qualification clubQualification = gq.Where(x => x.ranking == i + 1).Select(x => x).FirstOrDefault();
                    string qualification = "";
                    if (!clubQualification.isNextYear || !clubQualification.target.SameLevel(new QualificationTournament(clubChampionship)))
                    {
                        qualification = string.Format("{0} ({1})", clubQualification.target.Tournament(club).name, clubQualification.roundId);
                        if (!clubQualification.isNextYear)
                        {
                            qualification = string.Format("[{0}]", qualification);
                        }
                    }
                    Console.WriteLine("{0}. {1}{2}", ++i, clubName, qualification);
                }
            }
        }


    }
}
