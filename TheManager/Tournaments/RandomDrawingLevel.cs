using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
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
                if (pot[0] as NationalTeam != null)
                {
                    List<NationalTeam> nationalsTeams = new List<NationalTeam>();
                    foreach (Club c in pot)
                    {
                        nationalsTeams.Add(c as NationalTeam);
                    }
                    nationalsTeams.Sort(new NationsFifaRankingComparator(false));
                    pot.Clear();
                    foreach (NationalTeam nt in nationalsTeams)
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

            List<Club>[] baseHats = new List<Club>[minTeamsByGroup];

            //Some groups will get one more team
            if (_round.clubs.Count % _round.groupsCount > 0)
            {
                baseHats = new List<Club>[minTeamsByGroup + 1];
            }
            int ind = 0;
            for (int i = 0; i < minTeamsByGroup; i++)
            {
                baseHats[i] = new List<Club>();
                for (int j = 0; j < _round.groupsCount; j++)
                {
                    baseHats[i].Add(pot[ind]);
                    ind++;
                }

            }
            //Create last hat if there is remaining teams
            if (_round.clubs.Count % _round.groupsCount > 0)
            {
                baseHats[baseHats.Length - 1] = new List<Club>();
                for (int j = _round.groupsCount * minTeamsByGroup; j < _round.clubs.Count; j++)
                {
                    baseHats[baseHats.Length - 1].Add(pot[j]);
                }
            }

            //Shuffle hats because of the case 2
            for (int i = 0; i < baseHats.Count(); i++)
            {
                baseHats[i].Shuffle();
            }

            bool succeed = false;
            while(!succeed)
            {
                succeed = true;
                for(int i = 0; i<_round.groupsCount; i++)
                {
                    _round.groups[i].Clear();
                }
                try
                {
                    List<Club>[] hats = new List<Club>[baseHats.Length];
                    for(int i = 0; i < baseHats.Length; i++)
                    {
                        hats[i] = new List<Club>(baseHats[i]);
                    }

                    bool ruleConcernsContinent = _round.rules.Contains(Rule.OneTeamByContinentInGroup);

                    //Foreach groups
                    for (int i = 0; i < _round.groupsCount; i++)
                    {

                        //Create constraints dictionnary. Specify for each continent or country minimum and maximum numbers of teams on each group to respect rules.
                        //Only used if round include OneTeamByContinentInGroup or OneClubByCountryInGroup rule
                        Dictionary<ILocalisation, List<int>> constraintsContinents = new Dictionary<ILocalisation, List<int>>();
                        Dictionary<ILocalisation, List<int>> constraintsCountry = new Dictionary<ILocalisation, List<int>>();
                        foreach (Continent c in Session.Instance.Game.kernel.world.continents)
                        {
                            int teamsCount = TeamsOfContinent(hats, c);
                            float teamsRatio = teamsCount / (_round.groupsCount - i + 0.0f);
                            constraintsContinents.Add(c, new List<int> { (int)Math.Floor(teamsRatio), (int)Math.Ceiling(teamsRatio) });
                        }
                        List<Country> roundCountries = new List<Country>();

                        foreach (Club c in _round.clubs)
                        {
                            Country cCountry = c.Country();
                            if (!roundCountries.Contains(cCountry))
                            {
                                roundCountries.Add(cCountry);
                            }
                        }
                        foreach (Country c in roundCountries)
                        {
                            int teamsCount = TeamsOfCountry(hats, c);
                            float teamsRatio = teamsCount / (_round.groupsCount - i + 0.0f);
                            constraintsCountry.Add(c, new List<int> { (int)Math.Floor(teamsRatio), (int)Math.Ceiling(teamsRatio) });
                        }

                        //Foreach hats
                        for (int j = 0; j < hats.Length; j++)
                        {
                            if (hats[j].Count > 0)
                            {
                                List<Club> possibleTeams = new List<Club>();

                                foreach (Club hatClub in hats[j])
                                {
                                    if (!_round.rules.Contains(Rule.OneTeamByContinentInGroup) && !_round.rules.Contains(Rule.OneClubByCountryInGroup))
                                    {
                                        possibleTeams.Add(hatClub);
                                    }
                                    else
                                    {
                                        ILocalisation hcLocalisation = null;
                                        if(ruleConcernsContinent)
                                        {
                                            hcLocalisation = hatClub.Country().Continent;
                                        }
                                        else
                                        {
                                            hcLocalisation = hatClub.Country();
                                        }
                                        Dictionary<ILocalisation, List<int>> constraintsLocalisable = ruleConcernsContinent ? constraintsContinents : constraintsCountry;
                                        //Case 1
                                        //Si le nombre d'équipes par groupe + les équipes qui sont obligées d'arriver (car certains chapeaux contiennent uniquement des équipes de tel pays) est inférieur au nombre maximal d'équipes autorisées dans le groupe, alors on peut l'ajouter aux équipes sélectionnables.
                                        if (CountClubsOfLocalisation(_round.groups[i], hcLocalisation, ruleConcernsContinent) - HatsWithOnlyTeamsOfLocalizable(hats, j + 1, hcLocalisation, ruleConcernsContinent) < constraintsLocalisable[hcLocalisation][1])
                                        {
                                            possibleTeams.Add(hatClub);
                                        }
                                        //Case 2
                                        // Si le nombre de chapeaux restants avec des équipes de tel pays est égal au nombre minimum d'équipes nécessaire qui manque dans le groupe, alors on est obligé de prendre cette équipe
                                        if (RemainingHatsWithTeamsOfLocalizable(hats, j, hcLocalisation, ruleConcernsContinent) == constraintsLocalisable[hcLocalisation][0] - CountClubsOfLocalisation(_round.groups[i], hcLocalisation, ruleConcernsContinent))
                                        {
                                            possibleTeams = new List<Club> { hatClub };
                                            break;
                                        }
                                        /*
                                        if (_round.rules.Contains(Rule.OneTeamByContinentInGroup))
                                        {
                                            Continent hcContinent = hatClub.Country().Continent;
                                            //Case 1
                                            if (CountClubsOfContinent(_round.groups[i], hcContinent) - HatsWithOnlyTeamsOfContinent(hats, j + 1, hcContinent) < constraintsContinents[hcContinent][1])
                                            {
                                                possibleTeams.Add(hatClub);
                                            }
                                            //Case 2
                                            if (RemainingHatsWithTeamsOfContinent(hats, j, hcContinent) == constraintsContinents[hcContinent][0] - CountClubsOfContinent(_round.groups[i], hcContinent))
                                            {
                                                possibleTeams = new List<Club> { hatClub };
                                                break;
                                            }
                                        }
                                        else if (_round.rules.Contains(Rule.OneClubByCountryInGroup))
                                        {
                                            Country hcCountry = hatClub.Country();
                                            //Case 1
                                            if (CountClubsOfCountry(_round.groups[i], hcCountry) - HatsWithOnlyTeamsOfCountry(hats, j + 1, hcCountry) < constraintsCountry[hcCountry][1])
                                            {
                                                possibleTeams.Add(hatClub);
                                            }
                                            //Case 2
                                            if (RemainingHatsWithTeamsOfCountry(hats, j, hcCountry) == constraintsCountry[hcCountry][0] - CountClubsOfCountry(_round.groups[i], hcCountry))
                                            {
                                                possibleTeams = new List<Club> { hatClub };
                                                break;
                                            }
                                        }*/
                                    }

                                }

                                Club selectedTeam = possibleTeams[Session.Instance.Random(0, possibleTeams.Count)];
                                _round.groups[i].Add(selectedTeam);
                                hats[j].Remove(selectedTeam);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Utils.Debug(_round.Tournament.name + " (" + _round.name + ") Echec du tirage au sort de ce tour. Nouvelle tentative");
                    succeed = false;
                }
            }

        }
        
        private int CountClubsOfLocalisation(List<Club> clubs, ILocalisation localizable, bool withContinents)
        {
            int res = 0;
            foreach (Club c in clubs)
            {
                ILocalisation comp = null;
                if (withContinents)
                {
                    comp = c.Country().Continent;
                }
                else
                {
                    comp = c.Country();
                }

                if (comp == localizable)
                {
                    res++;
                }
            }
            return res;
        }

        private int TeamsOfCountry(List<Club>[] hats, Country country)
        {
            int res = 0;
            foreach (List<Club> lc in hats)
            {
                foreach (Club c in lc)
                {
                    if (c.Country() == country)
                    {
                        res++;
                    }
                }
            }
            return res;
        }

        private int TeamsOfContinent(List<Club>[] hats, Continent continent)
        {
            int res = 0;
            foreach (List<Club> lc in hats)
            {
                foreach (Club c in lc)
                {
                    if (c.Country().Continent == continent)
                    {
                        res++;
                    }
                }
            }
            return res;
        }

        private int TeamsOfLocalizable(List<Club>[] hats, ILocalisation localizable, bool withContinents)
        {
            int res = 0;
            foreach (List<Club> lc in hats)
            {
                foreach (Club c in lc)
                {
                    ILocalisation comp = null;
                    if (withContinents)
                    {
                        comp = c.Country().Continent;
                    }
                    else
                    {
                        comp = c.Country();
                    }

                    if (comp == localizable)
                    {
                        res++;
                    }
                }
            }
            return res;
        }

        private int RemainingHatsWithTeamsOfLocalizable(List<Club>[] hats, int currentHat, ILocalisation localizable, bool withContinents)
        {
            int res = 0;
            for (int i = currentHat; i < hats.Count(); i++)
            {
                bool teamsOfContinent = false;
                foreach (Club c in hats[i])
                {
                    ILocalisation comp = null;
                    if (withContinents)
                    {
                        comp = c.Country().Continent;
                    }
                    else
                    {
                        comp = c.Country();
                    }

                    if (comp == localizable)
                    {
                        teamsOfContinent = true;
                    }
                }
                if (teamsOfContinent)
                {
                    res++;
                }
            }
            return res;
        }

        private int HatsWithOnlyTeamsOfLocalizable(List<Club>[] hats, int currentHat, ILocalisation localizable, bool continent)
        {
            int res = 0;
            for (int i = currentHat; i < hats.Count(); i++)
            {
                bool onlyTeam = true;
                foreach (Club c in hats[i])
                {
                    ILocalisation comp = null;
                    if(continent)
                    {
                        comp = c.Country().Continent;
                    }
                    else
                    {
                        comp = c.Country();
                    }
                    if (comp != localizable)
                    {
                        onlyTeam = false;
                    }
                }
                if (onlyTeam)
                {
                    res++;
                }
            }
            return res;
        }
    }
}
