using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class Match_Date_Comparator : IComparer<Match>
    {
        public int Compare(Match x, Match y)
        {
            int res = 1;
            int diff = DateTime.Compare(x.Jour, y.Jour);
            if (diff < 0) res = -1;
            else if(diff == 0)
            {
                int levelDom = 0;
                int levelExt = 0;
                Club_Ville dom = x.Domicile as Club_Ville;
                Club_Ville ext = x.Exterieur as Club_Ville;
                if (dom != null) levelDom += (int)Math.Pow(2, dom.Championnat.Niveau);
                if (ext != null) levelDom += (int)Math.Pow(2, ext.Championnat.Niveau);
                dom = y.Domicile as Club_Ville;
                ext = y.Exterieur as Club_Ville;
                if (dom != null) levelExt += (int)Math.Pow(2, dom.Championnat.Niveau);
                if (ext != null) levelExt += (int)Math.Pow(2, ext.Championnat.Niveau);
                if (levelDom < levelExt) res = -1;
            }
            return res;
            //return DateTime.Compare(x.Jour, y.Jour);
        }
    }
}
