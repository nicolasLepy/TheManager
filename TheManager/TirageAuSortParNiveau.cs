using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager.Comparators;

namespace TheManager
{
    public class TirageAuSortParNiveau : ITirageAuSort
    {
        private TourPoules _tour;

        public TirageAuSortParNiveau(TourPoules tour)
        {
            _tour = tour;
        }

        public void TirerAuSort()
        {
            List<Club> pot = new List<Club>(_tour.Clubs);
            pot.Sort(new Club_Niveau_Comparator());
            int equipesParPoule = _tour.Clubs.Count / _tour.NombrePoules;
            List<Club>[] pots = new List<Club>[equipesParPoule];
            int ind = 0;
            for (int i = 0; i < equipesParPoule; i++)
            {
                pots[i] = new List<Club>();
                for (int j = 0; j < _tour.NombrePoules; j++)
                {
                    pots[i].Add(pot[ind]);
                    ind++;
                }

            }
            //Pour chaque poule
            for (int i = 0; i < _tour.NombrePoules; i++)
            {
                //Pour chaque pot
                for (int j = 0; j < equipesParPoule; j++)
                {
                    Club c = pots[j][Session.Instance.Random(0, pots[j].Count)];
                    pots[j].Remove(c);
                    _tour.Poules[i].Add(c);
                }
            }
        }
    }
}
