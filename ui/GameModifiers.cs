using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager;
using TheManager.Comparators;

namespace TheManager_GUI
{
    public class GameModifiers
    {

        public void CheckPlayoffTrees()
        {
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Console.WriteLine("Final Phases Clubs");
            foreach (Club c in fr.Leagues()[0].GetFinalPhasesClubs())
            {
                Console.WriteLine(". " + c.name);
            }
            List<Tournament> leagues = fr.Leagues();
            foreach (Tournament league in leagues)
            {
                Console.WriteLine("PLAYOFFS " + league.name);
                Round topPlayOffRound = league.GetFinalTopPlayOffRound();
                if (topPlayOffRound != null)
                {
                    Console.WriteLine("topPlayOffRound " + topPlayOffRound.Tournament.name + ", " + topPlayOffRound.name);
                    if (topPlayOffRound != league.rounds[0])
                    {
                        List<Round> rounds = new List<Round>();
                        rounds = league.GetPlayOffsTree(topPlayOffRound.Tournament, topPlayOffRound, new List<Round>());
                        foreach (Round r in rounds)
                        {
                            Console.WriteLine("- " + r.Tournament.name + ", " + r.name);
                        }
                    }
                }
                Console.WriteLine("========================");
            }
            int i = 0;
            ClubComparator comparator = new ClubComparator(ClubAttribute.CURRENT_RANKING, false);
            foreach (List<Club> clubs in fr.GetAdministrativeRetrogradations())
            {
                Console.WriteLine("============" + leagues[i].name + "==========");
                foreach (Club c in clubs)
                {
                    Round clubC = (from Tournament t in leagues where t.rounds.Count > 0 && t.rounds[0].clubs.Contains(c) select t.rounds[0]).FirstOrDefault();
                    string adm = (leagues[i].rounds[0] as GroupsRound != null && (leagues[i].rounds[0] as GroupsRound).administrativeLevel > 0) ? "[" + fr.GetAdministrativeDivisionLevel(c.AdministrativeDivision(), (leagues[i].rounds[0] as GroupsRound).administrativeLevel).name + "] " : "";
                    Console.WriteLine(adm + c.Championship.name + " - " + comparator.GetRanking(clubC, c) + ". " + c.name);
                }
                i++;
            }

        }

        public void RemovingPoints()
        {
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Round r = fr.League(4).rounds[0];
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("Stade de Reims B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("Olympique de Marseille B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("FC Nantes B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("FC Metz B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("AJ Auxerre B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("Paris Saint-Germain B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
        }

        public void SetFrenchCupFinalScore()
        {
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            fr.Cups()[0].rounds.Last().matches[0].score2 = 5;
        }

        public void SetAdministrativeRetrogradationsFr()
        {
            Country fr = Session.Instance.Game.kernel.String2Country("France");

            foreach (Tournament t in fr.Leagues())
            {
                foreach (Round r in t.rounds)
                {
                    if ((r as ChampionshipRound) != null || (r as GroupsRound) != null)
                    {
                        r.rules.Add(Rule.BottomTeamNotEligibleForRepechage);
                    }
                }
            }

            Dictionary<string, int> retrogradations = new Dictionary<string, int>()
            {
                ["AJ Auxerre"] = 6,
                ["Le Mans FC"] = 7,
                ["FC Villefranche"] = 7,
                ["FC Annecy"] = 7,
                ["Jura Lacs F"] = 7,
                ["CA DE Pontarlier"] = 7,
                ["AS ST Apollinaire"] = 7,
                ["Paron F C"] = 7,
                ["FC Grandvillars"] = 7
            };


            foreach (Club c in Session.Instance.Game.kernel.Clubs)
            {
                if (retrogradations.ContainsKey(c.name))
                {
                    Console.WriteLine("Relegue " + c.name);
                    fr.AddAdministrativeRetrogradation(c, fr.League(retrogradations[c.name]));
                }
            }
        }

        public void CheckDuplicates()
        {
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("France"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Martinique"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Guadeloupe"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Réunion"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Guyane"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Mayotte"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Tahiti"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Nouvelle-Calédonie"));

        }

        public void PrintGames()
        {
            foreach (Club c in Session.Instance.Game.kernel.Clubs)
            {
                Country fr = Session.Instance.Game.kernel.String2Country("France");
                if (c.Country() == fr)
                {
                    Console.WriteLine("PRINT GAMES FOR " + c.name);
                    List<Match> games = c.Games;
                    games.Sort(new MatchDateComparator());
                    DateTime currentDate = new DateTime(2000, 1, 1);
                    foreach (Match m in games)
                    {
                        int daysDiff = Utils.DaysNumberBetweenTwoDates(currentDate, m.day);
                        string alert = "";
                        switch (daysDiff)
                        {
                            case 0:
                                alert = "||| ";
                                break;
                            case 1:
                                alert = "|| ";
                                break;
                            case 2:
                                alert = "| ";
                                break;
                            default:
                                alert = "";
                                break;
                        }
                        Console.WriteLine(alert + m.day.ToShortDateString() + " [" + m.Tournament + "]" + m.home.name + " - " + m.away.name);
                        currentDate = m.day;
                    }
                }
            }
        }

        public void PrintAdministrativeRetrogradations()
        {
            /*Country fr = Session.Instance.Game.kernel.String2Country("France");
            Console.WriteLine(fr.Name());
            foreach(KeyValuePair<Club, Tournament> ra in fr.administrativeRetrogradations)
            {
                Console.WriteLine(ra.Key.name + " -> " + ra.Value.name);
            }*/
        }

        /*
        public void SetAdministrativeRetrogradationsAz()
        {
            Country az = Session.Instance.Game.kernel.String2Country("Azerbaïdjan");

            foreach (Tournament t in az.Leagues())
            {
                foreach (Round r in t.rounds)
                {
                    if ((r as ChampionshipRound) != null || (r as GroupsRound) != null)
                    {
                        r.rules.Add(Rule.BottomTeamNotEligibleForRepechage);
                    }
                }
            }

            Dictionary<string, int> retrogradations = new Dictionary<string, int>()
            {
                ["Club Nord N1 2"] = 7,
                ["Club Nord N1 3"] = 7,
                ["Club Sud9"] = 5,
                ["Club Sud8"] = 5,
                ["Club Nord N1 8"] = 8,
                ["Club Nord N2 6"] = 8,
                ["Club Nord N1 14"] = 8,
                ["Club Nord N1 9"] = 8,
                ["Club Nord N2 6"] = 8,
                ["Club Ouest O1 7"] = 8,
                ["Club Est E2 6"] = 6
            };


            foreach (Club c in Session.Instance.Game.kernel.Clubs)
            {
                if (retrogradations.ContainsKey(c.name))
                {
                    Console.WriteLine("Relegue " + c.name);
                    az.AddAdministrativeRetrogradation(c, az.League(retrogradations[c.name]));
                }
            }
        }*/


    }
}
