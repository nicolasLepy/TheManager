using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheManager.Comparators;

namespace TheManager.Exportation
{
    public class Exporteur
    {



        public static void ExporterClubs()
        {
            string output = "<p>Clubs</p>";

            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                CityClub cv = c as CityClub;
                if(cv != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<h2>").Append(cv.name).Append("</h2>");
                    output += "<h2>" + cv.name + "</h2>";
                    foreach(HistoricEntry eh in cv.history.elements)
                    {
                        output += "<p><b>" + eh.date.ToShortDateString() + "</b><br>Budget : " + eh.budget + "<br>Centre de formation : " + eh.formationFacilities + "</p>";
                    }
                }
            }
            File.WriteAllText("Output\\Clubs.html", output);

        }

        public static void Exporter(Tournament c)
        {
            ExporterClubs();
            string dir = "Output\\" + c.name + " " + Session.Instance.Game.date.Year;
            string dir2 = "Output\\" + c.shortName + Session.Instance.Game.date.Year;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

            foreach (Tour t in c.rounds)
            {
                if (!Directory.Exists(dir + "\\" + t.Nom)) Directory.CreateDirectory(dir + "\\" + t.Nom);
                if (!Directory.Exists(dir2 + "\\" + t.Nom)) Directory.CreateDirectory(dir2 + "\\" + t.Nom);

                string output = "<p>" + t.Nom + "</p><p>";
                foreach(Club cl in t.Clubs)
                {
                    CityClub cv = cl as CityClub;
                    if (cv != null)
                        output += "" + cl.name + " - " + cl.formationFacilities + " - " + cv.budget + " €<br>";
                }
                output += "</p>";
                if(t as TourChampionnat != null)
                {
                    TourChampionnat tc = t as TourChampionnat;
                    output += "<table>";
                    
                    foreach (Club club in tc.Classement())
                    {
                        output += "<tr><td>" + club.name + "</td><td>" + tc.Points(club) + "</td><td>" + tc.Joues(club) + "</td><td>" + tc.Gagnes(club) + "</td><td>" + tc.Nuls(club) + "</td><td>" + tc.Perdus(club) + "</td><td>" + tc.ButsPour(club) + "</td><td>" + tc.ButsContre(club) + "</td><td>" + tc.Difference(club) + "</td></tr>";
                    }
                    output += "</table>";
                    int matchsJournee = (tc.Clubs.Count % 2 == 1) ? tc.Clubs.Count/2+1 : tc.Clubs.Count/2;
                    int nbJournees = (tc.Matchs.Count / tc.Clubs.Count) * 2;
                    int k = 0;
                    Exporteurs2.ExporterClassementL(tc, "Output\\" + c.shortName + Session.Instance.Game.date.Year + "\\" + t.Nom + "\\Matchs\\");
                    for (int i = 0; i<nbJournees; i++)
                    {
                        List<Match> journee = new List<Match>();
                        for(int j = 0; j<matchsJournee; j++)
                        {
                            journee.Add(tc.Matchs[i * matchsJournee + j]);
                        }
                        journee.Sort(new MatchDateComparator());


                        Exporteurs2.ExporterL(journee, "Output\\" + c.shortName + Session.Instance.Game.date.Year + "\\" + t.Nom, i + 1);

                        output += "<p>Journée " + (int)(i + 1) + "</p><table>";
                        DateTime last = new DateTime(2000, 1, 1);
                        foreach (Match m in journee)
                        {
                            if (m.day.Date != last.Date)
                            {
                                output += "<tr><td colspan=\"3\">" + m.day.Date.ToShortDateString() + "</td></tr>";
                            }
                            last = m.day;
                            output += "<tr><td>" + m.day.ToShortTimeString() + "</td><td>" + m.home.name + "</td><td><a href=\""+tc.Nom+"\\" + k + ".html\">" + m.score1 + "-" + m.score2 + "</a></td><td>" + m.away.name + "</td></tr>";
                            EcrireMatch(m, dir + "\\" + tc.Nom + "\\" + k + ".html");
                            k++;
                        }
                        output += "</table>";
                    }
                }
                if(t as TourElimination != null)
                {
                    TourElimination te = t as TourElimination;
                    output += "<table>";
                    int k = 0;
                    List<Match> matchs = new List<Match>(te.Matchs);
                    matchs.Sort(new MatchDateComparator());
                    DateTime last = new DateTime(2000, 1, 1);
                    Exporteurs2.ExporterD(matchs, dir + "\\" + te.Nom + "\\");
                    foreach (Match m in matchs)
                    {
                        if(m.day.Date != last.Date)
                        {
                            output += "<tr><td colspan=\"3\">"+m.day.Date.ToShortDateString()+"</td></tr>";
                        }
                        last = m.day;
                        Tournament compDom = m.home.Championship;
                        Tournament compExt = m.away.Championship;
                        string sCompDom = "";
                        string sCompExt = "";
                        if (compDom != null) sCompDom = " (" + compDom.shortName + ")";
                        if (compExt != null) sCompExt = " (" + compExt.shortName + ")";
                        string score = m.score1 + " - " + m.score2;
                        if (m.prolongations) score += " ap";
                        if (m.PenaltyShootout) score += " (" + m.penaltyShootout1 + "-" + m.penaltyShootout2 + " tab)";
                        output += "<tr><td>" + m.day.ToShortTimeString() + "</td><td>" + m.home.name + sCompDom + "</td><td><a href=\"" + te.Nom + "\\" + k + ".html\">" + score + "</a></td><td>" + m.away.name + sCompExt + "</td></tr>";
                        EcrireMatch(m, dir + "\\" + te.Nom + "\\" + k + ".html");
                        k++;
                    }
                }
                if (t as TourInactif != null)
                {
                    TourInactif ti = t as TourInactif;
                    output += "<p><b>Clubs participants</b></p>";
                    List<Club> clubs = new List<Club>(ti.Clubs);                    
                    foreach(Club club in clubs)
                    {
                        output += "<p>" + club.name + "</p>";
                    }
                }
                if (t as TourPoules != null)
                {
                    TourPoules tp = t as TourPoules;
                    int nbEquipesParPoules = 0;
                    foreach (List<Club> poules in tp.Poules)
                    {
                        if (nbEquipesParPoules < poules.Count) nbEquipesParPoules = poules.Count;
                        List<Club> classement = new List<Club>(poules);
                        classement.Sort(new ClubRankingComparator(t.Matchs));
                        output += "<p>Groupe</p><table>";
                        foreach (Club club in classement)
                        {
                            output += "<tr><td>" + club.name + "</td><td>" + t.Points(club) + "</td><td>" + t.Joues(club) + "</td><td>" + t.Gagnes(club) + "</td><td>" + t.Nuls(club) + "</td><td>" + t.Perdus(club) + "</td><td>" + t.ButsPour(club) + "</td><td>" + t.ButsContre(club) + "</td><td>" + t.Difference(club) + "</td></tr>";
                        }
                        output += "</table>";
                    }
                    int nbJournees = nbEquipesParPoules-1;
                    if (t.AllerRetour) nbJournees *= 2;
                    int matchsJournee = t.Matchs.Count / nbJournees;
                    int k = 0;
                    for (int i = 0; i < nbJournees; i++)
                    {
                        List<Match> journee = new List<Match>();
                        for (int j = 0; j < matchsJournee; j++)
                        {
                            journee.Add(t.Matchs[i * matchsJournee + j]);
                        }
                        journee.Sort(new MatchDateComparator());

                        output += "<p>Journée " + (int)(i + 1) + "</p><table>";
                        foreach (Match m in journee)
                        {
                            output += "<tr><td>" + m.day.ToString() + "</td><td>" + m.home.name + "</td><td><a href=\"" + t.Nom + "\\" + k + ".html\">" + m.score1 + "-" + m.score2 + "</a></td><td>" + m.away.name + "</td></tr>";
                            EcrireMatch(m, dir + "\\" + t.Nom + "\\" + k + ".html");
                            k++;
                        }
                        output += "</table>";
                    }
                }
                output += "<p>Moyenne de buts : " + t.MoyenneButs() + "</p><p>Buteurs</p><table>";
                foreach (KeyValuePair<Player, int> j in t.Buteurs())
                {
                    output += "<tr><td>" + j.Key.firstName + " " + j.Key.lastName + "</td><td>" + j.Value + "</td></tr>";
                }
                output += "</table>";

                File.WriteAllText(dir + "\\" + t.Nom + ".html", output);

            }

        }

        public static void EcrireMatch(Match m, string nomFichier)
        {
            string output = "<p>" + m.home.name + " " + m.score1 + "-" + m.score2 + " " + m.away.name + "</p><table>";
            output += "<p>" + m.attendance + " spectateurs.</p>";
            List<MatchEvent> evenements = new List<MatchEvent>();
            List<MatchEvent> cartons = new List<MatchEvent>();
            foreach (MatchEvent em in m.events)
            {
                if (em.type == GameEvent.Goal || em.type == GameEvent.PenaltyGoal || em.type == GameEvent.AgGoal) evenements.Add(em);
                else cartons.Add(em);
                
            }
            evenements.Sort(new GameEventTimeComparator());
            int s1 = 0;
            int s2 = 0;
            foreach (MatchEvent em in evenements)
            {
                if (em.club == m.home) s1++; else s2++;
                output += "<tr><td>" + em.EventMinute + "°</td><td>" + em.player.firstName + " " + em.player.lastName + "</td><td>" + s1 + "-" + s2 + "</td></tr>";
            }
            output += "</table><table>";
            foreach (MatchEvent em in cartons)
            {
                output += "<tr><td>" + em.EventMinute + "°</td><td>" + em.player.firstName + " " + em.player.lastName + "</td><td>" + em.type + "</td><td>" + em.club.name + "</td></tr>";
            }
            output += "</table>";

            output += "<p><b>Compo Domicile</b></p>";
            foreach (Player j in m.compo1) output += "<br>" + j.firstName + " " + j.lastName + "(" + j.position + ")";

            output += "<p><b>Compo Extérieur</b></p>";
            foreach (Player j in m.compo2) output += "<br>" + j.firstName + " " + j.lastName + "(" + j.position + ")";

            output += "<p><b>Médias</b></p>";
            foreach(KeyValuePair<Media,Journalist> j in m.journalists)
            {
                output += "<p>" + j.Value.firstName + " " + j.Value.lastName + " (" + j.Key.name + ")";
            }
            File.WriteAllText(nomFichier,output);
        }
    }
}