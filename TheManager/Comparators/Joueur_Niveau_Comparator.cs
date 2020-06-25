using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class Joueur_Niveau_Comparator : IComparer<Player>
    {
        public int Compare(Player x, Player y)
        {
            return y.level - x.level;
        }
    }
}