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


            //Count teams by countries and continent. Used if round contains OneClubByCountryInGroup or OneTeamByContinentInGroup rules.
            Dictionary<Continent, int> teamsByContinent = _round.clubs.GroupBy(x => x.Country().Continent).ToDictionary(g => g.Key, g => g.Count());
            Dictionary<Country, int> teamsByCountry = _round.clubs.GroupBy(x => x.Country()).ToDictionary(g => g.Key, g => g.Count());

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
                        bool ruleFixedCountry = true;
                        bool ruleFixedContinent = true;

                        do
                        {
                            securityLoop++;
                            c = hats[j][Session.Instance.Random(0, hats[j].Count)];
                            drawnClub = c;
                            //The rule cannot be respected, switch current club with another club in the same hat but already placed
                            bool ruleOneClubByCountryCantBeRespected = _round.rules.Contains(Rule.OneClubByCountryInGroup) && ContainsCountry(_round.groups[i], c.Country()) && ContainsOnlyTeamsWithCountry(hats[j], c.Country());
                            bool ruleOneTeamByContinentCantBeRespected = _round.rules.Contains(Rule.OneTeamByContinentInGroup) && ContainsContinent(_round.groups[i], c.Country().Continent) && ContainsOnlyTeamsWithContinent(hats[j], c.Country().Continent);
                            if (ruleOneClubByCountryCantBeRespected || ruleOneTeamByContinentCantBeRespected)
                            {
                                //For k from 0 to i, see if the k-j switch unlock the situation
                                int k = 0;
                                bool blocked = true;
                                while (blocked)
                                {
                                    Club switchClub = _round.groups[k][j];
                                    bool checkCountry = !_round.rules.Contains(Rule.OneClubByCountryInGroup) || (!ContainsCountry(_round.groups[i], switchClub.Country()) && !ContainsCountry(_round.groups[k], c.Country()));
                                    bool checkContinent = !_round.rules.Contains(Rule.OneTeamByContinentInGroup) || (!ContainsContinent(_round.groups[i], switchClub.Country().Continent) && !ContainsContinent(_round.groups[k], c.Country().Continent));
                                    if ((k + 1) == i || (checkContinent && checkCountry))
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
                            ruleFixedCountry = _round.rules.Contains(Rule.OneClubByCountryInGroup) && (ContainsCountry(_round.groups[i], c.Country()) && !ContainsOnlyTeamsWithCountry(hats[j], c.Country()));
                            ruleFixedContinent = _round.rules.Contains(Rule.OneTeamByContinentInGroup) && (ContainsContinent(_round.groups[i], c.Country().Continent) && !ContainsOnlyTeamsWithContinent(hats[j], c.Country().Continent));

                            //Console.WriteLine(_round.Tournament + " - " + _round.rules.Contains(Rule.OneClubByCountryInGroup) + " - " + ContainsCountry(_round.groups[i], c.Country()) + " - " + !ContainsOnlyCountry(hats[j], c.Country()));
                        }
                        while (securityLoop < 500 && (ruleFixedCountry || ruleFixedContinent));

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

        private bool ContainsOnlyTeamsWithCountry(List<Club> clubs, Country country)
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

        private bool ContainsOnlyTeamsWithContinent(List<Club> clubs, Continent continent)
        {
            bool res = true;

            foreach (Club c in clubs)
            {
                if (c.Country().Continent != continent)
                {
                    res = false;
                }
            }

            return res;
        }

        private int CountClubsOfCountry(List<Club> clubs, Country country)
        {
            int res = 0;
            foreach(Club c in clubs)
            {
                if(c.Country() == country)
                {
                    res++;
                }
            }
            return res;
        }
        private int CountClubsOfContinent(List<Club> clubs, Continent continent)
        {
            int res = 0;
            foreach (Club c in clubs)
            {
                if (c.Country().Continent == continent)
                {
                    res++;
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

        private bool ContainsContinent(List<Club> clubs, Continent continent)
        {
            bool res = false;

            foreach (Club c in clubs)
            {
                if (c.Country().Continent == continent)
                {
                    res = true;
                }
            }
            return res;
        }
    }
}
