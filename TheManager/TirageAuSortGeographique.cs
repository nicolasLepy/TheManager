using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager.Comparators;

namespace TheManager
{
    public class TirageAuSortGeographique : ITirageAuSort
    {

        private TourPoules _tour;

        public TirageAuSortGeographique(TourPoules tour)
        {
            _tour = tour;
        }

        public void TirerAuSort()
        {
            int nbClubsParPoule = _tour.Clubs.Count / _tour.NombrePoules;
            List<Club> clubs = new List<Club>(_tour.Clubs);
            for(int i = 0; i<_tour.NombrePoules; i++)
            {
                GeographicPosition position = _tour.LocalisationGroupes[i];
                clubs.Sort(new Club_Localisation_Comparator(position));
                foreach(Club club in clubs)
                {
                    float dist = Utils.Distance(club.Stade.Ville.Position, position);
                }
                for(int j = 0;j<nbClubsParPoule; j++)
                {
                    _tour.Poules[i].Add(clubs[0]);
                    clubs.RemoveAt(0);
                }
            }
        }
    }
}
