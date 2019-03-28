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
        public static void Exporter(Competition c)
        {
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
                string output = "";
                if(t as TourChampionnat != null)
                {
                    TourChampionnat tc = t as TourChampionnat;
                    output = "<p>" + tc.Nom + "</p><table>";
                    
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
                        foreach(Match m in journee)
                        {
                            output += "<tr><td>" + m.Jour.ToString() + "</td><td>" + m.Domicile.Nom + "</td><td><a href=\""+tc.Nom+"\\" + k + ".html\">" + m.Score1 + "-" + m.Score2 + "</a></td><td>" + m.Exterieur.Nom + "</td></tr>";
                            EcrireMatch(m, dir + "\\" + tc.Nom + "\\" + k + ".html");
                            k++;
                        }
                        output += "</table>";
                        output += "<p>Moyenne de buts : " + tc.MoyenneButs() + "</p>";
                    }
                }
                File.WriteAllText(dir + "\\" + t.Nom + ".html", output);
            }
        }

        public static void EcrireMatch(Match m, string nomFichier)
        {
            string output = "<p>" + m.Domicile.Nom + " " + m.Score1 + "-" + m.Score2 + " " + m.Exterieur.Nom + "</p><table>";
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
            File.WriteAllText(nomFichier,output);
        }
    }
}