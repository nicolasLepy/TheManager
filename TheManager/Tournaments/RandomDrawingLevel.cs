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
        private readonly ClubAttribute _attribute;

        public RandomDrawingLevel(GroupsRound tour, ClubAttribute attribute)
        {
            _round = tour;
            _attribute = attribute;
        }

        public void RandomDrawing()
        {
            List<Club> pot = new List<Club>(_round.clubs);
            try
            {
                pot.Sort(new ClubComparator(_attribute, false));
                if(pot[0] as NationalTeam != null)
                {
                    List<NationalTeam> nationalsTeams = new List<NationalTeam>();
                    foreach(Club c in pot)
                    {
                        nationalsTeams.Add(c as NationalTeam);
                    }
                    nationalsTeams.Sort(new NationsFifaRankingComparator(false));
                    pot.Clear();
                    foreach(NationalTeam nt in nationalsTeams)
                    {
                        pot.Add(nt);
                    }
                }
            }
            catch
            {
                Utils.Debug("Le tri pour " + _round.name + "(" + _round.Tournament.name + " de type niveau a echoué");
            }
            int minTeamsByGroup = _round.clubs.Count / _round.groupsCount;

            List<Club>[] hats = new List<Club>[minTeamsByGroup];
            
            //Some groups will get one more team
            if(_round.clubs.Count % _round.groupsCount > 0)
            {
                hats = new List<Club>[minTeamsByGroup + 1];
            }
            int ind = 0;
            for (int i = 0; i < minTeamsByGroup; i++)
            {
                hats[i] = new List<Club>();
                for (int j = 0; j < _round.groupsCount; j++)
                {
                    hats[i].Add(pot[ind]);
                    ind++;
                }

            }
            //Create last hat if there is remaining teams
            if (_round.clubs.Count % _round.groupsCount > 0)
            {
                hats[hats.Length - 1] = new List<Club>();
                for (int j = _round.groupsCount*minTeamsByGroup; j<_round.clubs.Count; j++)
                {
                    hats[hats.Length - 1].Add(pot[j]);
                }
            }


            //Foreach groups
            for (int i = 0; i < _round.groupsCount; i++)
            {
                //Foreach hats
                for (int j = 0; j < hats.Length; j++)
                {
                    if (hats[j].Count > 0)
                    {
                        Club c = hats[j][Session.Instance.Random(0, hats[j].Count)];
                        hats[j].Remove(c);
                        _round.groups[i].Add(c);
                    }
                }
            }
        }
    }
}
