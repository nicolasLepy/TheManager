using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class PlayerCompositionComparator : IComparer<Player>
    {
        private readonly bool _lessImportantGame = true;

        public PlayerCompositionComparator(bool lessImportantGame)
        {
            _lessImportantGame = lessImportantGame;
        }
        public int Compare(Player x, Player y)
        {
            float energyImportance = 200f;
            if(_lessImportantGame)
            {
                energyImportance = 100f;
            }

            return (int)(y.level * ((y.energy/ energyImportance) +0.5f) - x.level * ((x.energy/ energyImportance) +0.5f));
        }
    }
}