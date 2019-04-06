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

            foreach(Club c in Session.Instance.Partie.Gestionnaire.Clubs)
            {
                Club_Ville cv = c as Club_Ville;
                if(cv != null)
                {
                    output += "<h2>" + cv.Nom + "</h2>";
                    foreach(EntreeHistorique eh in cv.Historique.Elements)
                    {
                        output += "<p><b>" + eh.Date.ToShortDateString() + "</b><br>Budget : " + eh.Budget + "<br>Centre de formation : " + eh.CentreFormation + "</p>";
                    }
                }
            }
            File.WriteAllText("Output\\Clubs.html", output);

        }

        public static void Exporter(Competition c)
        {
            ExporterClubs();
            string dir = "Output\\" + c.Nom + " " + Session.Instance.Partie.Date.Year;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            foreach(Tour t in c.Tours)
            {
                if (!Directory.Exists(dir + "\\" + t.Nom))
                {
                    Directory.CreateDirectory(dir + "\\" + t.Nom);
                }
                string output = "<p>" + t.Nom + "</p><p>";
                foreach(Club cl in t.Clubs)
                {
                    Club_Ville cv = cl as Club_Ville;
                    if (cv != null)
                        output += "" + cl.Nom + " - " + cl.CentreFormation + " - " + cv.Budget + " €<br>";
                }
                output += "</p>";
                if(t as TourChampionnat != null)
                {
                    TourChampionnat tc = t as TourChampionnat;
                    output += "<table>";
                    
                    foreach (Club club in tc.Classement())
                    {
                        output += "<tr><td>" + club.Nom + "</td><td>" + tc.Points(club) + "</td><td>" + tc.Joues(club) + "</td><td>" + tc.Gagnes(club) + "</td><td>" + tc.Nuls(club) + "</td><td>" + tc.Perdus(club) + "</td><td>" + tc.ButsPour(club) + "</td><td>" + tc.ButsContre(club) + "</td><td>" + tc.Difference(club) + "</td></tr>";
                    }
                    output += "</table>";
                    int matchsJournee = (tc.Clubs.Count % 2 == 1) ? tc.Clubs.Count/2+1 : tc.Clubs.Count/2;
                    int nbJournees = (tc.Matchs.Count / tc.Clubs.Count) * 2;
                    int k = 0;
                    for (int i = 0; i<nbJournees; i++)
                    {
                        List<Match> journee = new List<Match>();
                        for(int j = 0; j<matchsJournee; j++)
                        {
                            journee.Add(tc.Matchs[i * matchsJournee + j]);
                        }
                        journee.Sort(new Match_Date_Comparator());
                        
                        output += "<p>Journée " + (int)(i + 1) + "</p><table>";
                        DateTime last = new DateTime(2000, 1, 1);
                        foreach (Match m in journee)
                        {
                            if (m.Jour.Date != last.Date)
                            {
                                output += "<tr><td colspan=\"3\">" + m.Jour.Date.ToShortDateString() + "</td></tr>";
                            }
                            last = m.Jour;
                            output += "<tr><td>" + m.Jour.ToShortTimeString() + "</td><td>" + m.Domicile.Nom + "</td><td><a href=\""+tc.Nom+"\\" + k + ".html\">" + m.Score1 + "-" + m.Score2 + "</a></td><td>" + m.Exterieur.Nom + "</td></tr>";
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
                    matchs.Sort(new Match_Date_Comparator());
                    DateTime last = new DateTime(2000, 1, 1);
                    Exporteurs2.ExporterD(matchs, dir + "\\" + te.Nom + "\\");
                    foreach (Match m in matchs)
                    {
                        if(m.Jour.Date != last.Date)
                        {
                            output += "<tr><td colspan=\"3\">"+m.Jour.Date.ToShortDateString()+"</td></tr>";
                        }
                        last = m.Jour;
                        Competition compDom = m.Domicile.Championnat;
                        Competition compExt = m.Exterieur.Championnat;
                        string sCompDom = "";
                        string sCompExt = "";
                        if (compDom != null) sCompDom = " (" + compDom.NomCourt + ")";
                        if (compExt != null) sCompExt = " (" + compExt.NomCourt + ")";
                        string score = m.Score1 + " - " + m.Score2;
                        if (m.Prolongations) score += " ap";
                        if (m.TAB) score += " (" + m.Tab1 + "-" + m.Tab2 + " tab)";
                        output += "<tr><td>" + m.Jour.ToShortTimeString() + "</td><td>" + m.Domicile.Nom + sCompDom + "</td><td><a href=\"" + te.Nom + "\\" + k + ".html\">" + score + "</a></td><td>" + m.Exterieur.Nom + sCompExt + "</td></tr>";
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
                        output += "<p>" + club.Nom + "</p>";
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
                        classement.Sort(new Club_Classement_Comparator(t));
                        output += "<p>Groupe</p><table>";
                        foreach (Club club in classement)
                        {
                            output += "<tr><td>" + club.Nom + "</td><td>" + t.Points(club) + "</td><td>" + t.Joues(club) + "</td><td>" + t.Gagnes(club) + "</td><td>" + t.Nuls(club) + "</td><td>" + t.Perdus(club) + "</td><td>" + t.ButsPour(club) + "</td><td>" + t.ButsContre(club) + "</td><td>" + t.Difference(club) + "</td></tr>";
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
                        journee.Sort(new Match_Date_Comparator());

                        output += "<p>Journée " + (int)(i + 1) + "</p><table>";
                        foreach (Match m in journee)
                        {
                            output += "<tr><td>" + m.Jour.ToString() + "</td><td>" + m.Domicile.Nom + "</td><td><a href=\"" + t.Nom + "\\" + k + ".html\">" + m.Score1 + "-" + m.Score2 + "</a></td><td>" + m.Exterieur.Nom + "</td></tr>";
                            EcrireMatch(m, dir + "\\" + t.Nom + "\\" + k + ".html");
                            k++;
                        }
                        output += "</table>";
                    }
                }
                output += "<p>Moyenne de buts : " + t.MoyenneButs() + "</p><p>Buteurs</p><table>";
                foreach (KeyValuePair<Joueur, int> j in t.Buteurs())
                {
                    output += "<tr><td>" + j.Key.Prenom + " " + j.Key.Nom + "</td><td>" + j.Value + "</td></tr>";
                }
                output += "</table>";

                File.WriteAllText(dir + "\\" + t.Nom + ".html", output);

            }

        }

        public static void EcrireMatch(Match m, string nomFichier)
        {
            string output = "<p>" + m.Domicile.Nom + " " + m.Score1 + "-" + m.Score2 + " " + m.Exterieur.Nom + "</p><table>";
            output += "<p>" + m.Affluence + " spectateurs.</p>";
            List<EvenementMatch> evenements = new List<EvenementMatch>();
            List<EvenementMatch> cartons = new List<EvenementMatch>();
            foreach (EvenementMatch em in m.Evenements)
            {
                if (em.Type == Evenement.BUT || em.Type == Evenement.BUT_PENALTY || em.Type == Evenement.BUT_CSC) evenements.Add(em);
                else cartons.Add(em);
                
            }
            evenements.Sort(new EvenementMatch_Temps_Comparator());
            int s1 = 0;
            int s2 = 0;
            foreach (EvenementMatch em in evenements)
            {
                if (em.Club == m.Domicile) s1++; else s2++;
                output += "<tr><td>" + em.MinuteEv + "°</td><td>" + em.Joueur.Prenom + " " + em.Joueur.Nom + "</td><td>" + s1 + "-" + s2 + "</td></tr>";
            }
            output += "</table><table>";
            foreach (EvenementMatch em in cartons)
            {
                output += "<tr><td>" + em.MinuteEv + "°</td><td>" + em.Joueur.Prenom + " " + em.Joueur.Nom + "</td><td>" + em.Type + "</td><td>" + em.Club.Nom + "</td></tr>";
            }
            output += "</table>";

            output += "<p><b>Compo Domicile</b></p>";
            foreach (Joueur j in m.Compo1) output += "<br>" + j.Prenom + " " + j.Nom + "(" + j.Poste + ")";

            output += "<p><b>Compo Extérieur</b></p>";
            foreach (Joueur j in m.Compo2) output += "<br>" + j.Prenom + " " + j.Nom + "(" + j.Poste + ")";

            output += "<p><b>Médias</b></p>";
            foreach(Journaliste j in m.Journalistes)
            {
                output += "<p>" + j.Prenom + " " + j.Nom + " (" + j.Media.Nom + ")";
            }
            File.WriteAllText(nomFichier,output);
        }
    }
}