using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using tm.Comparators;

namespace tm
{
    public class RandomDrawingLevel : IRandomDrawing
    {
        private readonly GroupsRound _round;
        private readonly ClubAttribute _attribute;
        /// <summary>
        /// Association holding the tournament
        /// </summary>
        private readonly Association _masterAssociation;

        public RandomDrawingLevel(GroupsRound tour, ClubAttribute attribute)
        {
            _round = tour;
            _attribute = attribute;
            _masterAssociation = Session.Instance.Game.kernel.LocalisationTournament(_round.Tournament) as Association;
        }

        private Dictionary<Association, int> AssociationsRepresented()
        {
            Dictionary<Association, int> representations = new Dictionary<Association, int>();
            foreach(Club club in _round.clubs)
            {
                Association ca = club.Association().GetRepresentingAssociation(_masterAssociation);
                if(!representations.ContainsKey(ca))
                {
                    representations[ca] = 0;
                }
                representations[ca]++;
            }
            return representations;
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
                //try
                //{
                    List<Club>[] hats = new List<Club>[baseHats.Length];
                    for(int i = 0; i < baseHats.Length; i++)
                    {
                        hats[i] = new List<Club>(baseHats[i]);
                    }

                    //Foreach groups
                    for (int i = 0; i < _round.groupsCount; i++)
                    {

                        //Create constraints dictionnary. Specify for each continent or country minimum and maximum numbers of teams on each group to respect rules.
                        //Only used if round include OneTeamByAssociation rule
                        Dictionary<ILocalisation, List<int>> constraintsContinents = new Dictionary<ILocalisation, List<int>>();
                        Dictionary<ILocalisation, List<int>> constraintsCountry = new Dictionary<ILocalisation, List<int>>();
                        Dictionary<Association, List<int>> constraintsAssociations = new Dictionary<Association, List<int>>();

                        if(_round.rules.Contains(Rule.OneTeamByAssociationInGroup))
                        {
                            Dictionary<Association, int> representations = AssociationsRepresented();
                            foreach (KeyValuePair<Association, int> kvp in representations)
                            {
                                int teamsCount = TeamsOfAssociation(hats, kvp.Key);
                                float teamsRatio = teamsCount / (_round.groupsCount - i + 0.0f);
                                constraintsAssociations.Add(kvp.Key, new List<int> { (int)Math.Floor(teamsRatio), (int)Math.Ceiling(teamsRatio) });
                            }

                        }

                    //Foreach hats
                    for (int j = 0; j < hats.Length; j++)
                        {
                            if (hats[j].Count > 0)
                            {
                                List<Club> possibleTeams = new List<Club>();

                                foreach (Club hatClub in hats[j])
                                {
                                    if (!_round.rules.Contains(Rule.OneTeamByAssociationInGroup))
                                    {
                                        possibleTeams.Add(hatClub);
                                    }
                                    else
                                    {
                                        Association hcAssociation = hatClub.Association().GetRepresentingAssociation(_masterAssociation);

                                        //Case 1
                                        //Si le nombre d'équipes par groupe + les équipes qui sont obligées d'arriver (car certains chapeaux contiennent uniquement des équipes de tel pays) est inférieur au nombre maximal d'équipes autorisées dans le groupe, alors on peut l'ajouter aux équipes sélectionnables.
                                        if (CountClubsOfAssociation(_round.groups[i], hcAssociation) - HatsWithOnlyTeamsOfAssociation(hats, j + 1, hcAssociation) < constraintsAssociations[hcAssociation][1])
                                        {
                                            possibleTeams.Add(hatClub);
                                        }
                                        //Case 2
                                        // Si le nombre de chapeaux restants avec des équipes de tel pays est égal au nombre minimum d'équipes nécessaire qui manque dans le groupe, alors on est obligé de prendre cette équipe
                                        if (RemainingHatsWithTeamsOfAssociation(hats, j, hcAssociation) == constraintsAssociations[hcAssociation][0] - CountClubsOfAssociation(_round.groups[i], hcAssociation))
                                        {
                                            possibleTeams = new List<Club> { hatClub };
                                            break;
                                        }
                                    }

                                }

                                Club selectedTeam = possibleTeams[Session.Instance.Random(0, possibleTeams.Count)];
                                _round.groups[i].Add(selectedTeam);
                                hats[j].Remove(selectedTeam);
                            }
                        }
                    }
                //}
                /*catch(Exception e)
                {
                    Utils.Debug(_round.Tournament.name + " (" + _round.name + ") Echec du tirage au sort de ce tour. Nouvelle tentative");
                    succeed = false;
                }*/
            }
        }

        private int CountClubsOfAssociation(List<Club> clubs, Association association)
        {
            int res = 0;
            foreach(Club c in clubs)
            {
                Association comp = c.Association().GetRepresentingAssociation(_masterAssociation);
                if(comp == association)
                {
                    res++;
                }
            }
            return res;
        }

        private int TeamsOfAssociation(List<Club>[] hats, Association association)
        {
            int res = 0;
            foreach(List<Club> lc in hats)
            {
                foreach(Club c in lc)
                {
                    if(c.Association().GetRepresentingAssociation(_masterAssociation) == association)
                    {
                        res++;
                    }
                }
            }
            return res;
        }

        private int RemainingHatsWithTeamsOfAssociation(List<Club>[] hats, int currentHat, Association association)
        {
            int res = 0;
            for (int i = currentHat; i < hats.Count(); i++)
            {
                bool hasTeamsOfAssociation = false;
                foreach (Club c in hats[i])
                {
                    Association comp = c.Association().GetRepresentingAssociation(_masterAssociation);
                    if (comp == association)
                    {
                        hasTeamsOfAssociation = true;
                    }
                }
                if (hasTeamsOfAssociation)
                {
                    res++;
                }
            }
            return res;
        }

        private int HatsWithOnlyTeamsOfAssociation(List<Club>[] hats, int currentHat, Association association)
        {
            int res = 0;
            for (int i = currentHat; i < hats.Count(); i++)
            {
                bool onlyTeam = true;
                foreach (Club c in hats[i])
                {
                    Association comp = c.Association().GetRepresentingAssociation(_masterAssociation);
                    onlyTeam = onlyTeam && comp == association;
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
