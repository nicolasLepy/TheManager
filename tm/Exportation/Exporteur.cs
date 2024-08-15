using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using tm.Comparators;
using tm.Tournaments;

namespace tm.Exportation
{
    public class Exporteur
    {
        
        public static void ExporterClubs()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<p>Clubs</p>");

            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                CityClub cv = c as CityClub;
                if(cv != null)
                {
                    sb.Append("<h2>").Append(cv.name).Append("</h2>");
                    foreach(HistoricEntry eh in cv.history.elements)
                    {
                        sb.Append("<p><b>").Append(eh.date.ToShortDateString()).Append("</b><br>Budget : ")
                            .Append(eh.budget).Append("<br>Centre de formation : ").Append(eh.formationFacilities)
                            .Append("</p>");
                    }
                }
            }
            File.WriteAllText("Output\\Clubs.html", sb.ToString());

        }

        public static void Exporter(Tournament c)
        {
            ExporterClubs();
            StringBuilder dir = new StringBuilder();
            dir.Append("Output\\").Append(c.name).Append(" ").Append(Session.Instance.Game.date.Year);
            StringBuilder dir2 = new StringBuilder();
            dir2.Append("Output\\").Append(c.shortName).Append(" ").Append(Session.Instance.Game.date.Year);
            if (!Directory.Exists(dir.ToString()))
            {
                Directory.CreateDirectory(dir.ToString());
            }

            if (!Directory.Exists(dir2.ToString()))
            {
                Directory.CreateDirectory(dir2.ToString());
            }

            foreach (Round t in c.rounds)
            {
                if (!Directory.Exists(dir + "\\" + t.name))
                {
                    Directory.CreateDirectory(dir + "\\" + t.name);
                }

                if (!Directory.Exists(dir2 + "\\" + t.name))
                {
                    Directory.CreateDirectory(dir2 + "\\" + t.name);
                }

                StringBuilder output = new StringBuilder();
                output.Append("<p>").Append(t.name).Append("</p><p>");
                foreach(Club cl in t.clubs)
                {
                    CityClub cv = cl as CityClub;
                    if (cv != null)
                    {
                        output.Append("").Append(cl.name).Append(" - ").Append(cl.formationFacilities).Append(" - ")
                            .Append(cv.budget).Append(" €<br>");
                    }
                }
                output.Append("</p>");
                if(t as KnockoutRound != null)
                {
                    KnockoutRound te = t as KnockoutRound;
                    output.Append("<table>");
                    int k = 0;
                    List<Match> matchs = new List<Match>(te.matches);
                    matchs.Sort(new MatchDateComparator());
                    DateTime last = new DateTime(2000, 1, 1);
                    Exporteurs2.ExporterD(matchs, dir + "\\" + te.name + "\\");
                    foreach (Match m in matchs)
                    {
                        if(m.day.Date != last.Date)
                        {
                            output.Append("<tr><td colspan=\"3\">").Append(m.day.Date.ToShortDateString())
                                .Append("</td></tr>");
                        }
                        last = m.day;
                        Tournament compDom = m.home.Championship;
                        Tournament compExt = m.away.Championship;
                        string sCompDom = "";
                        string sCompExt = "";
                        if (compDom != null)
                        {
                            sCompDom = " (" + compDom.shortName + ")";
                        }

                        if (compExt != null)
                        {
                            sCompExt = " (" + compExt.shortName + ")";
                        }
                        string score = m.score1 + " - " + m.score2;
                        if (m.prolongations)
                        {
                            score += " ap";
                        }

                        if (m.PenaltyShootout)
                        {
                            score += " (" + m.penaltyShootout1 + "-" + m.penaltyShootout2 + " tab)";
                        }

                        output.Append("<tr><td>").Append(m.day.ToShortTimeString()).Append("</td><td>")
                            .Append(m.home.name).Append(sCompDom).Append("</td><td><a href=\"").Append(te.name)
                            .Append("\\").Append(k).Append(".html\">").Append(score).Append("</a></td><td>")
                            .Append(m.away.name).Append(sCompExt).Append("</td></tr>");
                        EcrireMatch(m, dir + "\\" + te.name + "\\" + k + ".html");
                        k++;
                    }
                }
                if (t as GroupInactiveRound != null)
                {
                    GroupInactiveRound ti = t as GroupInactiveRound;
                    output.Append("<p><b>Clubs participants</b></p>");
                    List<Club> clubs = new List<Club>(ti.clubs);                    
                    foreach(Club club in clubs)
                    {
                        output.Append("<p>").Append(club.name).Append("</p>");
                    }
                }
                if (t as GroupsRound != null)
                {
                    GroupsRound tp = t as GroupsRound;
                    int nbEquipesParPoules = 0;
                    foreach (List<Club> poules in tp.groups)
                    {
                        if (nbEquipesParPoules < poules.Count)
                        {
                            nbEquipesParPoules = poules.Count;
                        }
                        List<Club> classement = new List<Club>(poules);
                        classement.Sort(new ClubRankingComparator(t.matches, t.tiebreakers, t.pointsDeduction));
                        output.Append("<p>Groupe</p><table>");
                        foreach (Club club in classement)
                        {
                            output.Append("<tr><td>").Append(club.name).Append("</td><td>").Append(t.Points(club))
                                .Append("</td><td>").Append(t.Played(club)).Append("</td><td>").Append(t.Wins(club))
                                .Append("</td><td>").Append(t.Draws(club)).Append("</td><td>").Append(t.Loses(club))
                                .Append("</td><td>").Append(t.GoalsFor(club)).Append("</td><td>")
                                .Append(t.GoalsAgainst(club)).Append("</td><td>").Append(t.Difference(club))
                                .Append("</td></tr>");
                        }

                        output.Append("</table>");
                    }
                    int nbJournees = (nbEquipesParPoules-1) * t.phases;
                    int matchsJournee = t.matches.Count / nbJournees;
                    int k = 0;
                    for (int i = 0; i < nbJournees; i++)
                    {
                        List<Match> journee = new List<Match>();
                        for (int j = 0; j < matchsJournee; j++)
                        {
                            journee.Add(t.matches[i * matchsJournee + j]);
                        }
                        journee.Sort(new MatchDateComparator());

                        output.Append("<p>Journée ").Append((int) (i + 1)).Append("</p><table>");
                        foreach (Match m in journee)
                        {
                            output.Append("<tr><td>").Append(m.day.ToString()).Append("</td><td>").Append(m.home.name)
                                .Append("</td><td><a href=\"").Append(t.name).Append("\\").Append(k).Append(".html\">")
                                .Append(m.score1).Append("-").Append(m.score2).Append("</a></td><td>")
                                .Append(m.away.name).Append("</td></tr>");
                            EcrireMatch(m, dir + "\\" + t.name + "\\" + k + ".html");
                            k++;
                        }

                        output.Append("</table>");
                    }
                }

                output.Append("<p>Moyenne de buts : ").Append(t.GoalsAverage()).Append("</p><p>Buteurs</p><table>");
                foreach (KeyValuePair<Player, int> j in t.GoalScorers())
                {
                    output.Append("<tr><td>").Append(j.Key.firstName).Append(" ").Append(j.Key.lastName)
                        .Append("</td><td>").Append(j.Value).Append("</td></tr>");
                }

                output.Append("</table>");

                File.WriteAllText(dir + "\\" + t.name + ".html", output.ToString());

            }

        }

        public static void EcrireMatch(Match m, string nomFichier)
        {
            StringBuilder output = new StringBuilder();
            output.Append("<p>").Append(m.home.name).Append(" ").Append(m.score1).Append("-").Append(m.score2).Append(" ").Append(m.away.name).Append("</p><table>");
            output.Append("<p>").Append(m.attendance).Append(" spectateurs.</p>");
            List<MatchEvent> evenements = new List<MatchEvent>();
            List<MatchEvent> cartons = new List<MatchEvent>();
            foreach (MatchEvent em in m.events)
            {
                if (em.type == GameEvent.Goal || em.type == GameEvent.PenaltyGoal || em.type == GameEvent.AgGoal)
                {
                    evenements.Add(em);
                }
                else
                {
                    cartons.Add(em);
                }
                
            }
            evenements.Sort(new GameEventTimeComparator());
            int s1 = 0;
            int s2 = 0;
            foreach (MatchEvent em in evenements)
            {
                if (em.club == m.home)
                {
                    s1++;
                }
                else
                {
                    s2++;
                }
                output.Append("<tr><td>").Append(em.EventMinute).Append("°</td><td>").Append(em.player.firstName).Append(" ").Append(em.player.lastName).Append("</td><td>").Append(s1).Append("-").Append(s2).Append("</td></tr>");
            }
            output.Append("</table><table>");
            foreach (MatchEvent em in cartons)
            {
                output.Append("<tr><td>").Append(em.EventMinute).Append("°</td><td>").Append(em.player.firstName).Append(" ").Append(em.player.lastName).Append("</td><td>").Append(em.type).Append("</td><td>").Append(em.club.name).Append("</td></tr>");
            }
            output.Append("</table>");

            output.Append("<p><b>Compo Domicile</b></p>");
            foreach (Player j in m.compo1)
            {
                output.Append("<br>").Append(j.firstName).Append(" ").Append(j.lastName).Append("(").Append(j.position).Append(")");
            }

            output.Append("<p><b>Compo Extérieur</b></p>");
            foreach (Player j in m.compo2)
            {
                output.Append("<br>").Append(j.firstName).Append(" ").Append(j.lastName).Append("(").Append(j.position).Append(")");
            }

            output.Append("<p><b>Médias</b></p>");
            foreach(KeyValuePair<Media,Journalist> j in m.medias)
            {
                output.Append("<p>").Append(j.Value.firstName).Append(" ").Append(j.Value.lastName).Append(" (").Append(j.Key.name).Append(")");
            }
            File.WriteAllText(nomFichier,output.ToString());
        }
    }
}