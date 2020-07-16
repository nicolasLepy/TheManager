using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager.Comparators;

namespace TheManager
{
    public class RandomDrawingLevel : IRandomDrawing
    {
        private readonly GroupsRound _round;

        public RandomDrawingLevel(GroupsRound tour)
        {
            _round = tour;
        }

        public void RandomDrawing()
        {
            List<Club> pot = new List<Club>(_round.clubs);
            try
            {
                pot.Sort(new ClubLevelComparator());
            }
            catch
            {
                Utils.Debug("Le tri pour " + _round.name + "(" + _round.Tournament.name + " de type niveau a echoué");
            }
            int teamsByGroup = _round.clubs.Count / _round.groupsCount;
            List<Club>[] hats = new List<Club>[teamsByGroup];
            int ind = 0;
            for (int i = 0; i < teamsByGroup; i++)
            {
                hats[i] = new List<Club>();
                for (int j = 0; j < _round.groupsCount; j++)
                {
                    hats[i].Add(pot[ind]);
                    ind++;
                }

            }
            //Foreach groups
            for (int i = 0; i < _round.groupsCount; i++)
            {
                //Foreach hats
                for (int j = 0; j < teamsByGroup; j++)
                {
                    Club c = hats[j][Session.Instance.Random(0, hats[j].Count)];
                    hats[j].Remove(c);
                    _round.groups[i].Add(c);
                }
            }
        }
    }
}
