using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class Joueur_Composition_Comparator : IComparer<Player>
    {
        public int Compare(Player x, Player y)
        {
            return (int)(y.level * ((y.energy/200.0f)+0.5f) - x.level * ((x.energy/200.0f)+0.5f));
        }
    }
}