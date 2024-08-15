using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using tm.Comparators;
using tm.Tournaments;

namespace tm
{

    public enum ClubAttribute
    {
        STADIUM,
        LEVEL,
        POTENTIEL,
        BUDGET,
        SPONSOR,
        CONTINENTAL_COEFFICIENT,
        ELO,
        PAST_RANKING,
        CURRENT_RANKING,
        NAME,
        CITY_NAME
    }

    public class ClubComparator : IComparer<Club>
    {

        private readonly ClubAttribute _attribute;
        private readonly bool _inverted;
        private Dictionary<Round, List<Club>[]> _rankings;
        private Dictionary<Round, List<Club>> _fullRankings;

        public ClubComparator(ClubAttribute attribute, bool inverted = false)
        {
            _attribute = attribute;
            _inverted = inverted;
            _rankings = new Dictionary<Round, List<Club>[]>();
            _fullRankings = new Dictionary<Round, List<Club>>();
        }

        private int CompareRanking(GroupsRound round, Club x, Club y)
        {
            if(!_fullRankings.ContainsKey(round))
            {
                List<Club> roundClubs = new List<Club>(round.clubs);
                roundClubs.Sort(new ClubRankingComparator(round.matches, round.tiebreakers, round.pointsDeduction));
                _fullRankings.Add(round, roundClubs);
            }
            List<Club> clubs = _fullRankings[round];
            return clubs.IndexOf(x) - clubs.IndexOf(y);
        }

        public int GetRanking(Round round, Club c)
        {
            if(!_rankings.ContainsKey(round))
            {
                GroupsRound gr = round as GroupsRound;
                List<Club> roundRanking;
                //TODO: Need a Ranking() method for each round type
                if(gr != null)
                {
                    List<Club>[] rankings = new List<Club>[gr.groups.Length];
                    for(int i = 0; i< gr.groups.Length; i++)
                    {
                        rankings[i] = gr.Ranking(i);
                    }
                    _rankings.Add(round, rankings);
                }
            }
            int ranking = -1;
            foreach(List<Club> clubs in _rankings[round])
            {
                if(clubs.Contains(c))
                {
                    ranking = clubs.IndexOf(c);
                }
            }
            return ranking;
        }

        private int Inverted()
        {
            return _inverted ? -1 : 1;
        }

        public int Compare(Club x, Club y)
        {
            int res;
            switch (_attribute)
            {
                case ClubAttribute.NAME:
                    res = y.name.CompareTo(x.name);
                    break;
                case ClubAttribute.CITY_NAME:
                    CityClub xCity = x as CityClub;
                    CityClub yCity = y as CityClub;
                    string xName = xCity != null ? xCity.city.Name : x.name;
                    string yName = yCity != null ? yCity.city.Name : y.name;
                    res = xName.CompareTo(yName);
                    break;
                case ClubAttribute.STADIUM:
                    res = y.stadium.capacity - x.stadium.capacity;
                    break;
                case ClubAttribute.LEVEL:
                    res = x.Level() > y.Level() ? -1 : 1;
                    break;
                case ClubAttribute.POTENTIEL:
                    res = x.Potential() > y.Potential() ? -1 : 1;
                    break;
                case ClubAttribute.CONTINENTAL_COEFFICIENT:
                    res = x.ClubCoefficient() > y.ClubCoefficient() ? -1 : 1;
                    break;
                case ClubAttribute.BUDGET:
                    if(x as CityClub == null && y as CityClub == null)
                    {
                        res = 0;
                    }
                    else if (x as CityClub != null && y as CityClub == null)
                    {
                        res = -1;
                    }
                    else if (x as CityClub == null && y as CityClub != null)
                    {
                        res = 1;
                    }
                    else
                    {
                        res = (y as CityClub).budget - (x as CityClub).budget;
                    }
                    break;
                case ClubAttribute.SPONSOR:
                    if(x as CityClub != null)
                    {
                        res = (x as CityClub).sponsor > (y as CityClub).sponsor ? -1 : 1;
                    }
                    else
                    {
                        res = 0;
                    }
                    break;
                case ClubAttribute.ELO:
                    res = x.elo > y.elo ? -1 : 1;
                    break;
                case ClubAttribute.PAST_RANKING:
                case ClubAttribute.CURRENT_RANKING:
                    res = 0;
                    Round xChampionship;
                    Round yChampionship;
                    if(_attribute == ClubAttribute.PAST_RANKING)
                    {
                        xChampionship = (from Tournament t in x.Country().Leagues() where t.previousEditions.Count > 0 && t.LastEdition().rounds.Count > 0 && t.LastEdition().rounds[0].clubs.Contains(x) select t.LastEdition().rounds[0]).FirstOrDefault();
                        yChampionship = (from Tournament t in y.Country().Leagues() where t.previousEditions.Count > 0 && t.LastEdition().rounds.Count > 0 && t.LastEdition().rounds[0].clubs.Contains(y) select t.LastEdition().rounds[0]).FirstOrDefault();
                    }
                    else
                    {
                        xChampionship = (from Tournament t in x.Country().Leagues() where t.rounds.Count > 0 && t.rounds[0].clubs.Contains(x) select t.rounds[0]).FirstOrDefault();
                        yChampionship = (from Tournament t in y.Country().Leagues() where t.rounds.Count > 0 && t.rounds[0].clubs.Contains(y) select t.rounds[0]).FirstOrDefault();
                    }
                    if (xChampionship != default(Round) && yChampionship != default(Round))
                    {
                        Tournament xTournament = xChampionship.Tournament;
                        Tournament yTournament = yChampionship.Tournament;
                        res = xChampionship.Tournament.level - yChampionship.Tournament.level;
                        if(res == 0)
                        {
                            int xRanking = GetRanking(xChampionship, x);
                            int yRanking = GetRanking(yChampionship, y);
                            //Console.WriteLine("[Ranking " + x.name + " : "+xRanking+ "][Ranking " + y.name + " : "+yRanking+"]");
                            res = xRanking - yRanking;
                        }
                        GroupsRound xGroup = xTournament.rounds[0] as GroupsRound;
                        if (res == 0 && xGroup != null) //If the ranking of the clubs are the same, they are playing in the groups league
                        {
                            res = CompareRanking(xGroup, x, y);
                        }
                    }
                    else
                    {
                        res = x.Level() > y.Level() ? -1 : 1;
                    }
                    break;
                default:
                    res = x.Level() > y.Level() ? -1 : 1;
                    break;
            }
            return res * Inverted();
        }
    }
}
