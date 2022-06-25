using System.Collections.Generic;
using TheManager.Comparators;

namespace TheManager.Tournaments
{
    public class RandomDrawingAdministrative : IRandomDrawing
    {
        private readonly GroupsRound _round;

        public RandomDrawingAdministrative(GroupsRound tour)
        {
            _round = tour;
        }

        public void RandomDrawing()
        {
            RandomDrawingGeographic rdg = new RandomDrawingGeographic(_round);
            rdg.RandomDrawing();
        }
        
    }
}