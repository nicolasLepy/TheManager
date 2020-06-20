using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TheManager
{
    public class GenerateurArticle
    {

        private static GenerateurArticle _instance = null;

        public static GenerateurArticle Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GenerateurArticle();
                return _instance;
            }
        }

        private GenerateurArticle()
        {
            ChargerArticles();
        }

        private List<string> _gl_f = new List<string>();
        private List<string> _gl_e = new List<string>();
        private List<string> _gl_o = new List<string>();
        private List<string> _gs_f = new List<string>();
        private List<string> _gs_e = new List<string>();
        private List<string> _gs_o = new List<string>();
        private List<string> _n_f = new List<string>();
        private List<string> _n_e = new List<string>();
        private List<string> _n_o = new List<string>();
        private List<string> _nl_f = new List<string>();
        private List<string> _nl_e = new List<string>();
        private List<string> _nl_o = new List<string>();



        public string GenererArticle(Match match)
        {
            string res = "";
            Club equ1 = match.Vainqueur;
            Club equ2 = match.Perdant;
            float niv1 = equ1.Niveau();
            float niv2 = equ2.Niveau();
            int score1 = match.Score1;
            int score2 = match.Score2;
            
            //Vainqueur est favori
            if(niv1-niv2 > 8)
            {
                //Large victoire
                if ((equ1 == match.Domicile && score1 - score2 > 2) || (equ1 == match.Exterieur && score2 - score1 > 2))
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
                if ((equ1 == match.Domicile && score1 - score2 > 2) || (equ1 == match.Exterieur && score2 - score1 > 2))
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
                if ((equ1 == match.Domicile && score1 - score2 > 2) || (equ1 == match.Exterieur && score2 - score1 > 2))
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

            res = res.Replace("VAINQUEUR", equ1.NomCourt);
            res = res.Replace("VAINCU", equ2.NomCourt);
            res = res.Replace("SCORE", score1 + "-" + score2);
            return res;
        }

        private void ChargerArticles()
        {
            XDocument doc = XDocument.Load("Donnees/articles.xml");
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
                        case "NL_O": _nl_o.Add(article); 
                            break;
                    }
                }
            }
        }


    }
}
