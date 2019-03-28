﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class Match_Niveau_Comparator : IComparer<Match>
    {
        public int Compare(Match x, Match y)
        {
            return (int)((x.Domicile.Niveau() + x.Exterieur.Niveau()) - ((y.Domicile.Niveau() + y.Exterieur.Niveau())));
        }
    }
}