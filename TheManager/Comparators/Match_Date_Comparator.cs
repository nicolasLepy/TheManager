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
                int X = 0;
                int Y = 0;
                CityClub dom = x.Domicile as CityClub;
                CityClub ext = x.Exterieur as CityClub;
                if (dom != null && dom.Championship != null) X += (int)Math.Pow(2, 10 - dom.Championship.Niveau);
                if (ext != null && ext.Championship != null) X += (int)Math.Pow(2, 10 - ext.Championship.Niveau);
                dom = y.Domicile as CityClub;
                ext = y.Exterieur as CityClub;
                if (dom != null && dom.Championship != null) Y += (int)Math.Pow(2, 10 - dom.Championship.Niveau);
                if (ext != null && ext.Championship != null) Y += (int)Math.Pow(2, 10 - ext.Championship.Niveau);
                if (X > Y) res = -1;
            }
            return res;
        }
    }
}
