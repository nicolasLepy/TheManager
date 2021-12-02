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
                        Club c;
                        Club drawnClub;

                        //TODO: Change the algorithm to avoid use of securityLoop variable
                        int securityLoop = 0;

                        do
                        {
                            securityLoop++;
                            c = hats[j][Session.Instance.Random(0, hats[j].Count)];
                            drawnClub = c;
                            //The rule cannot be respected, switch current club with another club in the same hat but already placed
                            if (_round.rules.Contains(Rule.OneClubByCountryInGroup) && ContainsCountry(_round.groups[i], c.Country()) && ContainsOnlyCountry(hats[j], c.Country()))
                            {
                                //For k from 0 to i, see if the k-j switch unlock the situation
                                int k = 0;
                                bool blocked = true;
                                while (blocked)
                                {
                                    Club switchClub = _round.groups[k][j];
                                    if ((k + 1) == i || (!ContainsCountry(_round.groups[i], switchClub.Country()) && !ContainsCountry(_round.groups[k], c.Country())))
                                    {
                                        blocked = false;
                                        if ((k + 1) != i)
                                        {
                                            _round.groups[k][j] = c;
                                            c = switchClub;
                                        }
                                    }
                                    k++;
                                }
                            }
                            //Console.WriteLine(_round.Tournament + " - " + _round.rules.Contains(Rule.OneClubByCountryInGroup) + " - " + ContainsCountry(_round.groups[i], c.Country()) + " - " + !ContainsOnlyCountry(hats[j], c.Country()));
                        }
                        while(securityLoop < 500 && _round.rules.Contains(Rule.OneClubByCountryInGroup) && (ContainsCountry(_round.groups[i], c.Country()) && !ContainsOnlyCountry(hats[j], c.Country())));

                        if(securityLoop == 500)
                        {
                            Console.WriteLine("Security break used for " + _round.name + " (" + _round.Tournament.name + ")");
                        }

                        hats[j].Remove(drawnClub);
                        _round.groups[i].Add(c);
                    }
                }
            }
        }

        private bool ContainsOnlyCountry(List<Club> clubs, Country country)
        {
            bool res = true;

            foreach(Club c in clubs)
            {
                if(c.Country() != country)
                {
                    res = false;
                }
            }

            return res;
        }

        private bool ContainsCountry(List<Club> clubs, Country country)
        {
            bool res = false;

            foreach(Club c in clubs)
            {
                if(c.Country() == country)
                {
                    res = true;
                }
            }
            return res;
        }

    }
}
