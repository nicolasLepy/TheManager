﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LiveCharts.Wpf;

namespace TheManager
{
    public class ArticleGenerator
    {

        private static ArticleGenerator _instance;

        public static ArticleGenerator Instance
        {
            get
            {
                if (_instance == null){
                    _instance = new ArticleGenerator();
                }
                return _instance;
            }
        }

        private ArticleGenerator()
        {
            LoadArticles();
        }

        private readonly List<string> _gl_f = new List<string>();
        private readonly List<string> _gl_e = new List<string>();
        private readonly List<string> _gl_o = new List<string>();
        private readonly List<string> _gs_f = new List<string>();
        private readonly List<string> _gs_e = new List<string>();
        private readonly List<string> _gs_o = new List<string>();
        private readonly List<string> _n_f = new List<string>();
        private readonly List<string> _n_e = new List<string>();
        private readonly List<string> _n_o = new List<string>();
        private readonly List<string> _nl_f = new List<string>();
        private readonly List<string> _nl_e = new List<string>();
        private readonly List<string> _nl_o = new List<string>();


        public string GenerateArticle(ContractOffer co, CityClub to)
        {
            string res = "";
            string priceStr = "";

            if(co.TransferIndemnity > 0)
            {
                priceStr = "Pour " + co.TransferIndemnity + " €";
            }

            switch (co.Result)
            {
                case ContractOfferResult.Successful:
                    res = co.Player.lastName + " rejoint " + to.shortName + " " + priceStr + ".";
                    if(co.Origin == null)
                    {
                        res += " Le joueur était libre.";
                    }
                    break;
                case ContractOfferResult.NoAgreementWithPlayer:
                default:
                    res = to.shortName + " n'arrive pas à s'entendre avec " + co.Player.lastName + ".";
                    break;

            }
            return res;

        }

        public string GenerateArticle(Match match)
        {
            string res = "";
            Club team1 = match.Winner;
            Club team2 = match.Looser;
            float niv1 = team1.Level();
            float niv2 = team2.Level();
            int score1 = match.score1;
            int score2 = match.score2;
            
            //Winner is favorite
            if(niv1-niv2 > 8)
            {
                //Large victoire
                if ((team1 == match.home && score1 - score2 > 2) || (team1 == match.away && score2 - score1 > 2))
                {
                    res = _gl_f[Session.Instance.Random(0, _gl_f.Count)];
                }
                //Match nul
                else if (score1 - score2 == 0)
                {
                    res = _n_f[Session.Instance.Random(0, _n_f.Count)];
                }
                //Petite victoire
                else
                {
                    res = _gs_f[Session.Instance.Random(0, _gs_f.Count)];
                }
            }
            
            //Vainqueur est l'outsider
            else if(niv1-niv2 < -8)
            {
                //Large victoire
                if ((team1 == match.home && score1 - score2 > 2) || (team1 == match.away && score2 - score1 > 2))
                {
                    res = _gl_o[Session.Instance.Random(0, _gl_o.Count)];
                }
                //Match nul
                else if (score1 - score2 == 0)
                {
                    res = _n_o[Session.Instance.Random(0, _n_o.Count)];
                }
                //Petite victoire
                else
                {
                    res = _gs_o[Session.Instance.Random(0, _gs_o.Count)];
                }
            }
            
            //Equilibré
            else
            {
                //Large victoire
                if ((team1 == match.home && score1 - score2 > 2) || (team1 == match.away && score2 - score1 > 2))
                {
                    res = _gl_e[Session.Instance.Random(0, _gl_e.Count)];
                }
                //Match nul
                else if (score1 - score2 == 0)
                {
                    res = _n_e[Session.Instance.Random(0, _n_e.Count)];
                }
                //Petite victoire
                else
                {
                    res = _gs_e[Session.Instance.Random(0, _gs_e.Count)];
                }
            }

            res = res.Replace("VAINQUEUR", team1.shortName);
            res = res.Replace("VAINCU", team2.shortName);
            res = res.Replace("SCORE", score1 + "-" + score2);
            return res;
        }

        private void LoadArticles()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/articles.xml");
            foreach (XElement e in doc.Descendants("Articles"))
            {
                foreach (XElement e2 in e.Descendants("Article"))
                {
                    string type = e2.Attribute("type").Value;
                    string article = e2.Value;

                    switch(type)
                    {
                        case "GL_F": _gl_f.Add(article); 
                            break;
                        case "GL_E": _gl_e.Add(article); 
                            break;
                        case "GL_O": _gl_o.Add(article); 
                            break;
                        case "GS_F": _gs_f.Add(article); 
                            break;
                        case "GS_E": _gs_e.Add(article); 
                            break;
                        case "GS_O": _gs_o.Add(article); 
                            break;
                        case "N_F": _n_f.Add(article); 
                            break;
                        case "N_E": _n_e.Add(article); 
                            break;
                        case "N_O": _n_o.Add(article); 
                            break;
                        case "NL_F": _nl_f.Add(article); 
                            break;
                        case "NL_E": _nl_e.Add(article); 
                            break;
                        case "NL_O": default : _nl_o.Add(article);
                            break;
                    }
                }
            }
        }


    }
}
