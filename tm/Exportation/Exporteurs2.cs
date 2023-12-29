using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using tm.Comparators;

namespace tm.Exportation
{
    public class Exporteurs2
    {

        public static void ExporterMatchDeuxZero(Match match, string filename)
        {
            CityClub dom = match.home as CityClub;
            CityClub ext = match.away as CityClub;
            string output = "";
            string ap = "";
            output += "<td class=\"Chapitre2\" colspan=\"9\" height=\"18\">&nbsp;" + match.day.ToShortDateString() + " " + match.day.ToShortTimeString() + "</td>\n</tr>\n<tr>\n";
            output += "<td class=\"TraitGris\" colspan=\"9\"><img src=\"..\\..\\dzmatch\\Pts.gif\"></td>\n</tr>\n<tr>\n";
            output += "<td class=\"Txt10\" width=\"5%\" height=\"10\">&nbsp;</td>\n<td class=\"Txt10\" width=\"5%\">&nbsp;</td>\n<td class=\"Txt10\" width=\"4%\">&nbsp;</td>\n<td class=\"Txt10\" width=\"29%\">&nbsp;</td>\n<td class=\"Txt10\" width=\"14%\">&nbsp;</td>\n<td class=\"Txt10\" width=\"29%\">&nbsp;</td>\n<td class=\"Txt10\" width=\"4%\">&nbsp;</td>\n<td class=\"Txt10\" width=\"5%\">&nbsp;</td>\n<td class=\"Txt10\" width=\"5%\">&nbsp;</td>\n</tr>\n<tr>\n";
            output += "<td class=\"TraitBlanc\" colspan=\"9\"><img src=\"..\\..\\dzmatch/Pts.gif\"></td></tr>\n";
            output += "<tr><td class=\"Txt11Centre\" colspan=\"4\" height=\"100\"><img src=\"..\\..\\Logos\\" + match.home.logo + ".png width=\"80\" height=\"80\"></td><td>";

            output += "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">";
            if (match.prolongations) ap = "a.p.";
            output += "<tbody><tr><td class=\"Score\" width=\"12%\" height=\"40\">" + match.score1 + " - " + match.score2 + " " + ap + "</td></tr>";
            //Add tirs au buts ici
            output += "<tr><td class=\"ScoreMiTemps\" height=\"18\">(" + match.score1 + " - " + match.score2 + ")</td></tr></tbody></table></td>";

            output += "<td class=\"Txt11Centre\" colspan=\"4\"><img src=\"..\\..\\Logos\\" + match.away.logo + ".png width=\"80\" height=\"80\"></td></tr>";

            string c1 = "";
            string c2 = "";

            c1 = match.home.name;
            if (dom != null) c1 += " (" + dom.Championship.shortName + ")";

            c2 = match.away.name;
            if (ext != null) c2 += " (" + ext.Championship.shortName + ")";

            output += "<tr><td class=\"Txt12GrasCentre\" colspan=\"4\" height=\"20\">" + c1 + "</td><td class=\"Txt12GrasCentre\" width=\"12%\">&nbsp;</td><td class=\"Txt12GrasCentre\" colspan=\"4\">" + c2 + "</td></tr>";

            c1 = match.home.name;
            c2 = match.away.name;

            output += "<tr><td class=\"Txt10CentreGris\" colspan=\"4\" height=\"16\" valign=\"top\">(" + c1 + ")</td><td class=\"Txt11Centre\" valign=\"top\">&nbsp;</td><td class=\"Txt10CentreGris\" colspan=\"4\" valign=\"top\">(" + c2 + ")</td></tr>\n";

            output += "<tr><td class=\"Txt11Gras\" colspan=\"9\" height=\"10\">&nbsp;</td></tr><tr><td class=\"TraitGrisClair\" colspan=\"9\"><img src=\"..\\..\\dzmatch/Pts.gif\"></td></tr><tr><td class=\"Txt11Gras\" colspan=\"9\" height=\"18\">&nbsp;Buts&nbsp;:</td></tr>\n";

            int ic = 0;
            string bPen = "";
            foreach (MatchEvent em in match.events)
            {
                if (em.type == GameEvent.Goal || em.type == GameEvent.AgGoal || em.type == GameEvent.PenaltyGoal)
                {
                    int nTemps = 0;
                    ic++;
                    if (em.type == GameEvent.PenaltyGoal) bPen = " sp";
                    if (em.club == match.away)
                    {
                        output += "<tr><td class=\"TraitBlanc\" colspan=\"9\"><img src=\"..\\..\\dzmatch\\Pts.gif\"></td></tr>\n";
                        output += "<tr>\n";
                        output += "<td class=\"Txt11Droit\" height=\"18\"></td>\n";
                        output += "<td class=\"Txt11Centre\"></td>\n";
                        output += "<td class=\"Txt11Centre\"></td>\n";
                        output += "<td class=\"Txt11Droit\" colspan=\"3\">" + em.player.firstName + " " + em.player.lastName.ToUpper() + "</td>\n";
                        output += "<td class=\"Txt11Centre\"><img src=\"..\\..\\dzmatch/Icone_ButSurAction.png\"></td>\n";
                        if (em.minute > 45)
                        {
                            int tempsMax = 45;
                            if (em.period == 2) tempsMax = 90;
                            if (em.period == 3) tempsMax = 105;
                            if (em.period == 4) tempsMax = 120;
                            nTemps = em.EventMinute - tempsMax;
                            output += "<td class=\"Txt11Droit\">" + tempsMax + "+" + nTemps + bPen + "'</td>";
                        }
                        else
                        {
                            output += "<td class=\"Txt11Droit\">" + em.EventMinute + bPen + "'</td>\n";
                        }
                        output += "<td class=\"Txt11Centre\"></td>\n</tr>\n";


                    }
                    else
                    {
                        output += "<tr><td class=\"TraitBlanc\" colspan=\"9\"><img src=\"..\\..\\dzmatch/Pts.gif\"></td></tr>\n";

                        output += "<tr>\n";

                        if (em.minute > 45)
                        {
                            int tempsMax = 45;
                            if (em.period == 2) tempsMax = 90;
                            if (em.period == 3) tempsMax = 105;
                            if (em.period == 4) tempsMax = 120;
                            nTemps = em.EventMinute - tempsMax;
                            output += "<td class=\"Txt11Droit\" height=\"18\">" + tempsMax + "+" + nTemps + bPen + "'</td>";
                        }
                        else
                        {
                            output += "<td class=\"Txt11Droit\" height=\"18\">" + em.EventMinute + bPen + "'</td>\n";
                        }
                        output += "<td class=\"Txt11Centre\"></td>\n";
                        output += "<td class=\"Txt11Centre\"><img src=\"..\\..\\dzmatch/Icone_ButSurAction.png\"></td>\n";
                        output += "<td class=\"Txt11\" colspan=\"3\">" + em.player.firstName + " " + em.player.lastName.ToUpper() + "</td>\n";
                        output += "<td class=\"Txt11Centre\"></td>\n";
                        output += "<td class=\"Txt11Droit\"></td>\n";
                        output += "<td class=\"Txt11Centre\"></td>\n";
                        output += "</tr>\n";
                    }
                }
            }

            output += "<tr>\n<td class=\"TraitGrisClair\" colspan=\"9\"><img src=\"..\\..\\dzmatch/Pts.gif\"></td>\n</tr>";
            output += "<tr>\n<td class=\"Txt11\" colspan=\"9\" height=\"18\"><b>&nbsp;Stade&nbsp;:&nbsp;</b>" + match.home.stadium.name + "</td></tr>";
            output += "<tr>\n<td class=\"TraitGrisClair\" colspan=\"9\"><img src=\"..\\..\\dzmatch/Pts.gif\"></td>\n</tr>";
            output += "<tr><td class=\"Txt11\" colspan=\"9\" height=\"18\"><b>&nbsp;Spectateurs&nbsp;:&nbsp;</b>" + match.attendance + "</td></tr>";
            output += "<tr>\n<td class=\"TraitGrisClair\" colspan=\"9\"><img src=\"..\\..\\dzmatch/Pts.gif\"></td></tr>";

            foreach (KeyValuePair<Media,Journalist> j in match.medias)
            {
                output += "<tr><td colspan=\"2\" class=\"Txt11\">" + j.Key.name + " : </td>";
                output += "<td colspan=\"3\" class=\"Txt11\">" + j.Value.firstName + " " + j.Value.lastName + "</td>";
                output += "</tr>";
            }

            string slab = "";

            string ndz1 = File.ReadAllText("..\\Debug\\Output\\deux-zero.com\\fm1.txt", Encoding.Default);

            string ndz2 = File.ReadAllText("..\\Debug\\Output\\deux-zero.com\\fm2.txt", Encoding.Default);
            output = ndz1 + output + ndz2;

            File.WriteAllText(filename, output, Encoding.Default);

            /***
	
	SI Match.match_rmc=Vrai ALORS
		sChain=sChain+"<tr><img src=""logo/rmc.png""/></tr>" + RC
	FIN
	SI Match.match_europe1=Vrai ALORS
		sChain=sChain+"<tr><img src=""logo/europe1.png""/></tr>" + RC
	FIN
	SI Match.match_rtl=Vrai ALORS
		sChain=sChain+"<tr><img src=""logo/rtl.jpg""/></tr>" + RC
	FIN
	
	SI Match.match_tva=Vrai ALORS
		sChain=sChain+"<tr><img src=""logo/tva.png""/></tr>" + RC
	FIN
	
	SI Match.match_rsa=Vrai ALORS
		sChain=sChain+"<tr><img src=""logo/rsa.png""/></tr>" + RC
	FIN
	SI Match.correspondant_rmc>0 ALORS
		HLitRecherche(correspondant,IDcorrespondant,Match.correspondant_rmc)
		sChain=sChain+"<tr>"
		sChain=sChain+"<td colspan=""2"" class=""Txt11"">RMC : </td>"
		sChain=sChain+"<td colspan=""3"" class=""Txt11"">" + correspondant.nom + "</td>"
		sChain=sChain+"</tr>"
	FIN
	SI Match.correspondant_europe1>0 ALORS
		HLitRecherche(correspondant,IDcorrespondant,Match.correspondant_europe1)
		sChain=sChain+"<tr>"
		sChain=sChain+"<td colspan=""2"" class=""Txt11"">Europe 1 : </td>"
		sChain=sChain+"<td colspan=""3"" class=""Txt11"">" + correspondant.nom + "</td>"
		sChain=sChain+"</tr>"	
	FIN
	SI Match.correspondant_rtl>0 ALORS
		HLitRecherche(correspondant,IDcorrespondant,Match.correspondant_rtl)
		sChain=sChain+"<tr>"
		sChain=sChain+"<td colspan=""2"" class=""Txt11"">RTL : </td>"
		sChain=sChain+"<td colspan=""3"" class=""Txt11"">" + correspondant.nom + "</td>"
		sChain=sChain+"</tr>"	
	FIN
	SI Match.correspondant_rsa>0 ALORS
		HLitRecherche(correspondant,IDcorrespondant,Match.correspondant_rsa)
		sChain=sChain+"<tr>"
		sChain=sChain+"<td colspan=""2"" class=""Txt11"">Radio Sport Agzag : </td>"
		sChain=sChain+"<td colspan=""3"" class=""Txt11"">" + correspondant.nom + "</td>"
		sChain=sChain+"</tr>"	
	FIN

             * 
             * 
             */
        }


        public static void ExporterD(List<Match> matchs, string dir)
        {
            string output = "";
            bool bcgjour = false;

            DateTime precedente = new DateTime(2000, 7, 1);
            int index = -1;
            foreach (Match match in matchs)
            {
                index++;
                string nomDomicile = match.home.name;
                string nomExterieur = match.away.name;
                CityClub dom = match.home as CityClub;
                CityClub ext = match.away as CityClub;
                if (dom != null) nomDomicile += " (" + dom.Championship.shortName + ")";
                if (ext != null) nomExterieur += " (" + ext.Championship.shortName + ")";
                string ap = "";
                bcgjour = false;
                if (match.day.Date != precedente)
                {
                    output += "<tr><td class=\"TraitGris\" colspan=\"8\"><img src=\"..\\..\\deux-zero_files\\Pts.gif\"></td></tr>\n";
                    output += "<tr><td class=\"Chapitre2\" colspan=\"8\">&nbsp;" + match.day.ToString("Dddd dd MMMM yyyy") + "</td></tr>";
                    output += "<tr><td class=\"TraitGris\" colspan=\"8\"><img src=\"..\\..\\deux-zero_files\\Pts.gif\"></td></tr>";
                }
                precedente = match.day.Date;

                if (match.prolongations) ap = " a.p.";

                output += "<tr>\n<td class=\"TraitGrisClair\" colspan=\"8\"><img src=\"..\\..\\deux-zero_files\\Pts.gif\" alt=\"\"></td>\n</tr><tr class=\"LigneMatch\" onmouseover=\"this.className=&#39;LigneMatchOn&#39;;\" onmouseout=\"this.className=&#39;LigneMatch&#39;;\" onclick=\"document.location.href=&#39;index.php?Code=CF&amp;Item=Fiche&amp;Edition=2013-2014&amp;MatchId=18598&#39;;\">  \n<td class=\"Txt11\" style=\"cursor:hand\" width=\"2%\">&nbsp;</td>\n<td class=\"Txt11\" style=\"cursor:hand\" width=\"14%\">" + match.day.ToShortTimeString() + "</td>\n";

                output += "<td class=\"Txt11Centre\" style=\"cursor:hand\" width=\"5%\"><img src=\"..\\..\\Logos\\" + match.home.logo + ".png\" title=\"A\"  width=18 height=18></td>\n";

                if (match.score1 > match.score2)
                    output += "<td class=\"Txt11GrasDroit\" style=\"cursor:hand\" width=\"25%\">" + nomDomicile + "</td>\n";
                else
                    output += "<td class=\"Txt11Droit\" style=\"cursor:hand\" width=\"25%\">" + nomDomicile + "</td>\n";

                if (!match.Played)
                {
                    output += "<td class=\"FlagResultat1\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat1\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\"> <img border=\"0\" src=\"deux-zero_files/IconeDuelInactif.png\"></a><br><font style=\"font-size:10px\"></font></td>\n";
                }

                if (match.Played && !match.PenaltyShootout && dom != null && ext != null && dom.Championship.level == ext.Championship.level)
                {
                    output += "<td class=\"FlagResultat1\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat1\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\"> " + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\"></font></td>\n";
                }

                if (match.score1 > match.score2 && !match.PenaltyShootout && dom != null && ext != null && dom.Championship.level < ext.Championship.level)
                {

                    output += "<td class=\"FlagResultat1\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat1\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\">" + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\"></font></td>\n";
                }

                if (match.score1 > match.score2 && !match.PenaltyShootout && dom != null && ext != null && dom.Championship.level > ext.Championship.level)
                {
                    output += "<td class=\"FlagResultat2\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat2\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\">" + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\"></font></td>\n";
                }

                if (match.score1 < match.score2 && dom != null && ext != null && dom.Championship.level < ext.Championship.level)
                {
                    output += "<td class=\"FlagResultat2\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat2\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\">" + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\"></font></td>\n";
                }

                if (match.score1 < match.score2 && dom != null && ext != null && dom.Championship.level > ext.Championship.level)
                {
                    output += "<td class=\"FlagResultat1\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat1\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\">" + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\"></font></td>\n";
                }


                if (match.penaltyShootout1 > match.penaltyShootout2 && dom != null && ext != null && dom.Championship.level > ext.Championship.level)
                {
                    output += "<td class=\"FlagResultat2B\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat2\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\">" + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\">(" + match.penaltyShootout1 + "-" + match.penaltyShootout2 + " tab)</font></td>\n";
                }


                if (match.penaltyShootout1 > match.penaltyShootout2 && dom != null && ext != null && dom.Championship.level <= ext.Championship.level)
                {
                    output += "<td class=\"FlagResultat1B\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat1\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\">" + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\">(" + match.penaltyShootout1 + "-" + match.penaltyShootout2 + " tab)</font></td>\n";
                }

                if (match.penaltyShootout1 < match.penaltyShootout2 && dom != null && ext != null && dom.Championship.level >= ext.Championship.level)
                {
                    output += "<td class=\"FlagResultat1B\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat1\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\">" + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\">(" + match.penaltyShootout1 + "-" + match.penaltyShootout2 + " tab)</font></td>\n";
                }

                if (match.penaltyShootout1 < match.penaltyShootout2 && dom != null && ext != null && dom.Championship.level < ext.Championship.level)
                {
                    output += "<td class=\"FlagResultat2B\" style=\"cursor:hand\" width=\"10%\"><a class=\"FlagResultat2\" href=\"match_" + index + "mdz.html\" style=\"text-decoration: none\">" + match.score1 + "-" + match.score2 + ap + "</a><br><font style=\"font-size:10px\">(" + match.penaltyShootout1 + "-" + match.penaltyShootout2 + " tab)</font></td>\n";
                }


                /*if(match.Score1 > match.Score2)
                {
                    output += "<td class=\"Txt11\" style=\"cursor:hand\" width=\"25%\">" + nomExterieur + "</td>\n";
                }

                if(match.TAB && match.Score1+match.Score2>0)
                {
                    output += "<td class=\"Txt11\" style=\"cursor:hand\" width=\"25%\">" + nomExterieur + "</td>\n";
                }*/

                if (match.score1 > match.score2)
                {
                    output += "<td class=\"Txt11\" style=\"cursor:hand\" width=\"25%\">" + nomExterieur + "</td>\n";
                }
                else if (match.score1 + match.score2 > 0 || match.penaltyShootout1 + match.penaltyShootout2 > 0)
                {
                    output += "<td class=\"Txt11Gras\" style=\"cursor:hand\" width=\"25%\">" + nomExterieur + "</td>\n";
                }
                else
                {
                    output += "<td class=\"Txt11\" style=\"cursor:hand\" width=\"25%\">" + nomExterieur + "</td>\n";
                }


                output += "<td class=\"Txt11Centre\" style=\"cursor:hand\" width=\"5%\"><img src=\"..\\..\\Logos\\" + match.away.logo + ".png\" title=\"A\" width=18 height=18></td>\n";
                ExporterMatchDeuxZero(match, dir + "match_" + index + "mdz.html");
                /*    SI Match.tele = 1 ALORS
                          schaine_b = schaine_b + "<td class=""Txt11"" style="""" width=""14%""><img src=logo/france3.png title=""France 3 Régions"">&nbsp;</td>" + RC + "</tr>" + RC


                    SINON SI Match.tele = 2

                        schaine_b = schaine_b + "<td class=""Txt11"" style="""" width=""14%""><img src=logo/eurosport.png title=""Eurosport"">&nbsp;</td>" + RC + "</tr>" + RC

                    SINON
                        schaine_b = schaine_b + "<td class=""Txt11"" style=""cursor:hand"" width=""14%"">&nbsp;</td>" + RC + "</tr>" + RC

                    FIN*/
            }

            string ndz1 = File.ReadAllText("..\\Debug\\Output\\deux-zero.com\\ndz1.txt", Encoding.UTF8);

            string ndz2 = File.ReadAllText("..\\Debug\\Output\\deux-zero.com\\ndz2.txt", Encoding.UTF8);

            output = ndz1 + output + ndz2;
            File.WriteAllText(dir + "ndz.html", output, Encoding.UTF8);

        }

        public static void ExporterMatchL(Match m, string chemin)
        {
            string temps = "";
            string output = "";
            string equ1 = "";
            string equ2 = "";
            int ordre = 0;

            output += "<h1 class=\"confrontation\">Feuille de Match <span>" + m.home.name + " - " + m.away.name + "</span></h1>\n";

            output += "<p>Saison 2013/2014 - <A href=\"\">37ème journée</A></p>\n";

            output += "<p>Samedi 10 mai 2014 - " + m.home.stadium.name + "</p>";

            output += "<p>\n";

            output += m.attendance + " spectateurs </p>\n";

            foreach(KeyValuePair<Media, Journalist> j in m.medias)
            {
                output += "<p>" + j.Key.name + " : " + j.Value.firstName + " " + j.Value.lastName + "</p>";
            }

            output += "<input type=\"hidden\" id=\"match_id_hidden\" value=\"77876\">\n";
            output += "<Input type=\"hidden\" id=\"dom_id_hidden\" value=\"157\">\n";
            output += "<Input type=\"hidden\" id=\"ext_id_hidden\" value=\"68\">\n";
            output += "<Input type=\"hidden\" id=\"live_hidden\" value=\"0\">\n";
            output += "<Input type=\"hidden\" id=\"dom_nom_club_hidden\" value=\"EA+Guingamp\">\n";
            output += "<Input type=\"hidden\" id=\"ext_nom_club_hidden\" value=\"Toulouse+FC\">\n";
            output += "<div class=\"score\">\n";

            output += "<div style=\"background-image: url(../../Logos/" + m.home.logo + ".png);background-size:80px 80px;\" class=\"club_dom\">\n";
            output += "<span class=\"club\">" + m.home.name + "</span>\n";
            output += "<span class=\"buts\">" + m.score1 + "</span>\n";
            output += "</div>\n";
            output += "<div style=\"background-image: url(../../Logos/" + m.away.logo + ".png);background-size:80px 80px;\" class=\"club_ext\">\n";
            output += "<span class=\"club\">" + m.away.name + "</span>\n";
            output += "<span class=\"buts\">" + m.score2 + "</span>\n";
            output += "</div>\n";
            output += "<div class=\"video\"><span class=\"icon icon_video\"></span><A href=\"a\" rel=\"popup_77876\" class=\"poplight\">Résumé vidéo</A></div>";
            output += "<div class=\"clear\"></div>\n";
            output += "<p class=\"periode\">";

            output += "(" + m.ScoreHalfTime1 + "-" + m.ScoreHalfTime2 + ")\n";
            output += "</p>\n";
            output += "<div class=\"clear\"></div><div style=\"text-align:center;padding:10px;\">\n";
            output += "<iframe id=\"twitter-widget-0\" Scrolling=\"no\" frameborder=\"0\" allowtransparency=\"true\" src=\"./LFPm.fr/tweet_button.a8867d06719745ebf3f9c8b2ae835b17.fr.html\" class=\"twitter-hashtag-button twitter-tweet-button twitter-hashtag-button twitter-count-none\" data-twttr-rendered=\"true\" style=\"width: 150px; height: 28px;\"></iframe>\n";
            output += "</div>\n";
            output += "<div class=\"clear\"></div>\n";
            output += "<br>\n";
            output += "<div id=\"buts\">\n";
            output += "<ul class=\"club_dom\">\n";

            //But pour l'équipe 1
            foreach(MatchEvent em in m.events)
            {
                string extra = "";
                string minute = "";
                if(em.club == m.home && (em.type == GameEvent.Goal || em.type == GameEvent.PenaltyGoal || em.type == GameEvent.AgGoal))
                {
                    if (em.type == GameEvent.PenaltyGoal) extra = " (Pen)";
                    else if (em.type == GameEvent.AgGoal) extra = " (csc)";

                    if (em.minute > 45)
                    {
                        int tempsMax = 45;
                        if (em.period == 2) tempsMax = 90;
                        if (em.period == 3) tempsMax = 105;
                        if (em.period == 4) tempsMax = 120;
                        int tempsAdd = em.EventMinute - tempsMax;
                        minute = tempsMax + "+" + tempsAdd;
                    }
                    else minute = em.EventMinute.ToString();
                    output += "<li><img src=\"..\\..\\..\\LFP\\images\\but.png\"/><span class=\"icon icon_but\">Buts</span> " + minute + "' : <A href=\"b\">" + em.player.firstName + " " + em.player.lastName.ToUpper() + extra + "</A></li>\n";
                }
            }

            output += "</ul>\n";
            output += "<ul class=\"club_ext\">\n";

            //But pour l'équipe 2
            foreach (MatchEvent em in m.events)
            {
                string extra = "";
                string minute = "";
                if (em.club == m.away && (em.type == GameEvent.Goal || em.type == GameEvent.PenaltyGoal || em.type == GameEvent.AgGoal))
                {
                    if (em.type == GameEvent.PenaltyGoal) extra = " (Pen)";
                    else if (em.type == GameEvent.AgGoal) extra = " (csc)";

                    if (em.minute > 45)
                    {
                        int tempsMax = 45;
                        if (em.period == 2) tempsMax = 90;
                        if (em.period == 3) tempsMax = 105;
                        if (em.period == 4) tempsMax = 120;
                        int tempsAdd = em.EventMinute - tempsMax;
                        minute = tempsMax + "+" + tempsAdd;
                    }
                    else minute = em.EventMinute.ToString();
                    output += "<li><img src=\"..\\..\\..\\LFP\\images\\but.png\"/><span class=\"icon icon_but\">Buts</span> " + minute + "' : <A href=\"b\">" + em.player.firstName + " " + em.player.lastName.ToUpper() + extra + "</A></li>\n";
                }
            }

            output += "</ul>\n";

            output += "</div>\n";


            //Cartons jaune pour l'équipe à domicile
            output += "<div id=\"cartons\">\n<ul class=\"club_dom clear\">";

            foreach(MatchEvent em in m.events)
            {
                if(em.club == m.home && (em.type == GameEvent.YellowCard || em.type == GameEvent.RedCard))
                {
                    string image = em.type == GameEvent.YellowCard ? "jaune" : "rouge";
                    string minute = "";
                    if (em.minute > 45)
                    {
                        int tempsMax = 45;
                        if (em.period == 2) tempsMax = 90;
                        if (em.period == 3) tempsMax = 105;
                        if (em.period == 4) tempsMax = 120;
                        int tempsAdd = em.EventMinute - tempsMax;
                        minute = tempsMax + "+" + tempsAdd;
                    }
                    else minute = em.EventMinute.ToString();
                    output += "<li><img src=\"..\\..\\..\\LFP\\images/" + image + ".png\"/><span class=\"icon icon_but\">Carton jaune</span> " + minute + "' : <A href=\"b\">" + em.player.firstName + " " + em.player.lastName.ToUpper() + "</A></li>\n";
                }
            }

            output += "</ul>\n";

            output += "<ul class=\"club_ext\">";

            foreach (MatchEvent em in m.events)
            {
                if (em.club == m.away && (em.type == GameEvent.YellowCard || em.type == GameEvent.RedCard))
                {
                    string image = em.type == GameEvent.YellowCard ? "jaune" : "rouge";
                    string minute = "";
                    if (em.minute > 45)
                    {
                        int tempsMax = 45;
                        if (em.period == 2) tempsMax = 90;
                        if (em.period == 3) tempsMax = 105;
                        if (em.period == 4) tempsMax = 120;
                        int tempsAdd = em.EventMinute - tempsMax;
                        minute = tempsMax + "+" + tempsAdd;
                    }
                    else minute = em.EventMinute.ToString();
                    output += "<li><img src=\"..\\..\\..\\LFP\\images/" + image + ".png\"/><span class=\"icon icon_but\">Carton jaune</span> " + minute + "' : <A href=\"b\">" + em.player.firstName + " " + em.player.lastName.ToUpper() + "</A></li>\n";
                }
            }

            output += "</ul>\n";

            output += "</div>\n";

            string lfp1 = File.ReadAllText("..\\Debug\\Output\\LFP\\lfp1.txt", Encoding.UTF8);

            string lfp2 = File.ReadAllText("..\\Debug\\Output\\LFP\\lfp2.txt", Encoding.UTF8);

            output = lfp1 + output + lfp2;
            File.WriteAllText(chemin, output, Encoding.UTF8);

        }

        public static void ExporterClassementL(ChampionshipRound tc, string dossier)
        {
            if (!Directory.Exists(dossier)) Directory.CreateDirectory(dossier);

            List<Club> classement = tc.Ranking();
            List<Match> matchs = new List<Match>();
            int nbMatchsParJournees = classement.Count / 2;
            int nbJournees = tc.matches.Count / nbMatchsParJournees;

            string class1 = File.ReadAllText("..\\Debug\\Output\\LFP\\class1.txt", Encoding.UTF8);

            string class2 = File.ReadAllText("..\\Debug\\Output\\LFP\\class2.txt", Encoding.UTF8);

            for (int i = 0; i<nbJournees; i++)
            {
                for (int j = 0; j < nbMatchsParJournees; j++) matchs.Add(tc.matches[i * nbMatchsParJournees + j]);
                
                bool odd = true;
                string output = "";
                int compteur = 0;

                List<Club> classementJourneeJ = new List<Club>(classement);
                classementJourneeJ.Sort(new ClubRankingComparator(matchs, tc.tiebreakers, tc.pointsDeduction));
                foreach (Club c in classementJourneeJ)
                {
                    compteur++;
                    odd = !odd;
                    if (odd) output += "<tr class=\"even\">\n";
                    else output += "<tr class=\"odd\">\n";

                    //En dur pour l'instant
                    if (compteur > 17) output += "<td class=\"position position_relegation\">" + compteur + "</td>\n";
                    else if (compteur < 3) output += "<td class=\"position position_europe\">" + compteur + "</td>\n";
                    else output += "<td class=\"position\">" + compteur + "</td>\n";
                    output += "<td class=\"prog\"></td><td class=\"club\"><A href=\"a\"><img src=\"..\\..\\..\\Logos\\" + c.logo + ".png\" width=\"18\" height=\"18\">" + c.name + "</A></td>\n";

                    output += "<td class=\"points\">" + Utils.Points(matchs, c) + "</td>"; //TODO: Points deduction not taken account

                    output += "<td class=\"chiffres\">" + Utils.Played(matchs,c) + "</td>\n";
                    output += "<td class=\"chiffres\">" + Utils.Wins(matchs,c) + "</td>\n";
                    output += "<td class=\"chiffres\">" + Utils.Draws(matchs,c) + "</td>\n";
                    output += "<td class=\"chiffres\">" + Utils.Loses(matchs,c) + "</td>\n";
                    output += "<td class=\"chiffres\">" + Utils.Gf(matchs,c) + "</td>\n";
                    output += "<td class=\"chiffres\">" + Utils.Ga(matchs,c) + "</td>\n";
                    output += "<td class=\"chiffres\">" + Utils.Difference(matchs,c) + "</td>\n";
                    
                }
                output = class1 + output + class2;
                File.WriteAllText(dossier + "classement_" + (i + 1) + ".html", output, Encoding.UTF8);

            }
            
        }

        public static void ExporterL(List<Match> matchs, string dir, int journee)
        {

            string dirMatchs = dir + "\\Matchs\\";
            if (!Directory.Exists(dirMatchs)) Directory.CreateDirectory(dirMatchs);
            string output = "";
            bool alternate = false;
            int b = 0;
            DateTime last = new DateTime(2000, 1, 1);
            foreach(Match m in matchs)
            {
                alternate = !alternate;
                if(last.Date != m.day.Date)
                {
                    output += "</tbody></table>\n<h4 style=\"\"color: black;\"\">" + m.day.ToString("dddd dd MMMM yyyy") + "</h4>\n<table>\n<tbody>\n\n";
                }
                last = m.day;

                if (alternate) output += "<tr class=\"odd\">\n";
                else output += "<tr>\n";

                string score = m.score1 + " - " + m.score2;

                output += "<td class=\"horaire \"><a href=\"dd\">" + m.day.ToShortTimeString() + "</a></td>\n";
                output += "<td class=\"domicile\"><a href=\"bb\">" + m.home.name + "</a></td>\n";
                output += "<td class=\"logo\"><A href=\"bb\"><img src=\"..\\..\\..\\Logos\\" + m.home.logo + ".png\" width = \"18\" height = \"18\" ></ a ></ td > \n";
                output += "<td class=\"stats\"><A href=\"Matchs/match_" + journee + "_" + b + ".html\"> " + score + "</A></td>\n";
                output += "<td class=\"logo\"><A href=\"bb\"><img src=\"..\\..\\..\\Logos\\" + m.away.logo + ".png\" width = \"18\" height = \"18\" alt = \"b\" ></ a ></ td >\n";
                output += "<td class=\"exterieur\"><a href=\"bb\">" + m.away.name + "</a></td>\n";
                output += "<td class=\"video\"></td></tr>\n\n";

                ExporterMatchL(m, dirMatchs + "match_" + journee + "_" + b + ".html");
                b++;
            }

            output += "</tbody></table>";
            output += "<a href=\"" + (journee+1) + ".html\">Journée Suivante</a>\n";

            string lfp1 = File.ReadAllText("..\\Debug\\Output\\LFP\\pre.txt", Encoding.UTF8);
            string lfp2 = File.ReadAllText("..\\Debug\\Output\\LFP\\apres.txt", Encoding.UTF8);

            output = lfp1 + output + lfp2;
            File.WriteAllText(dir + "\\"+journee+".html", output, Encoding.UTF8);
        }
    }
}