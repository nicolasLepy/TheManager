using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TheManager.Comparators;

namespace TheManager
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
        PAST_RANKING
    }

    public class ClubComparator : IComparer<Club>
    {

        private readonly ClubAttribute _attribute;
        private readonly bool _inverted;
        private Dictionary<Round, List<Club>> _rankings;

        public ClubComparator(ClubAttribute attribute, bool inverted = false)
        {
            _attribute = attribute;
            _inverted = inverted;
            _rankings = new Dictionary<Round, List<Club>>();
        }

        private List<Club> GetRanking(Round round)
        {
            if(!_rankings.ContainsKey(round))
            {
                InactiveRound ir = round as InactiveRound;
                List<Club> roundRanking;
                //TODO: Need a Ranking() method for each round type
                if (ir != null)
                {
                    roundRanking = ir.Ranking();
                }
                else
                {
                    roundRanking = new List<Club>(round.clubs);
                    roundRanking.Sort(new ClubRankingComparator(round.matches));
                }
                _rankings.Add(round, roundRanking);
            }
            return _rankings[round];
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
                    res = 0;
                    Round xChampionship = (from Tournament t in x.Country().Leagues() where t.previousEditions.Count > 0 && t.LastEdition().rounds.Count > 0 && t.LastEdition().rounds[0].clubs.Contains(x) select t.LastEdition().rounds[0]).FirstOrDefault();
                    Round yChampionship = (from Tournament t in y.Country().Leagues() where t.previousEditions.Count > 0 && t.LastEdition().rounds.Count > 0 && t.LastEdition().rounds[0].clubs.Contains(y) select t.LastEdition().rounds[0]).FirstOrDefault();
                    if(xChampionship != default(Round) && yChampionship != default(Round))
                    {
                        Tournament xTournament = xChampionship.Tournament;
                        Tournament yTournament = yChampionship.Tournament;
                        res = xChampionship.Tournament.level - yChampionship.Tournament.level;
                        if(res == 0)
                        {
                            int xRanking = GetRanking(xChampionship).IndexOf(x);
                            int yRanking = GetRanking(yChampionship).IndexOf(y);
                            res = xRanking - yRanking;
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
