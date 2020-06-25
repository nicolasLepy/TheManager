using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager.Comparators;

namespace TheManager
{
    public class RandomDrawingGeographic : IRandomDrawing
    {

        private TourPoules _round;

        public RandomDrawingGeographic(TourPoules tour)
        {
            _round = tour;
        }

        public void RandomDrawing()
        {
            int clubsNumberByGroup = _round.Clubs.Count / _round.NombrePoules;
            List<Club> clubs = new List<Club>(_round.Clubs);
            for(int i = 0; i<_round.NombrePoules; i++)
            {
                GeographicPosition position = _round.LocalisationGroupes[i];
                clubs.Sort(new ClubLocalisationComparator(position));
                foreach(Club club in clubs)
                {
                    float dist = Utils.Distance(club.stadium.city.Position, position);
                }
                for(int j = 0;j<clubsNumberByGroup; j++)
                {
                    _round.Poules[i].Add(clubs[0]);
                    clubs.RemoveAt(0);
                }
            }
        }
    }
}
