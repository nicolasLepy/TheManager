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


        public ClubComparator(ClubAttribute attribute, bool inverted = false)
        {
            _attribute = attribute;
            _inverted = inverted;
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
                    CityClub xc = x as CityClub;
                    CityClub yc = y as CityClub;
                    res = 0;
                    if(xc != null && yc != null)
                    {
                        Round xChampionship = (from Tournament t in xc.Country().Leagues() where t.previousEditions.Count > 0 && t.LastEdition().rounds.Count > 0 && t.LastEdition().rounds[0].clubs.Contains(xc) select t.LastEdition().rounds[0]).FirstOrDefault();
                        Round yChampionship = (from Tournament t in yc.Country().Leagues() where t.previousEditions.Count > 0 && t.LastEdition().rounds.Count > 0 && t.LastEdition().rounds[0].clubs.Contains(yc) select t.LastEdition().rounds[0]).FirstOrDefault();
                        if(xChampionship != default(Round) && yChampionship != default(Round))
                        {
                            Tournament xTournament = xChampionship.Tournament;
                            Tournament yTournament = yChampionship.Tournament;
                            res = xChampionship.Tournament.level - yChampionship.Tournament.level;
                            if(res == 0)
                            {
                                //TODO: Need a Ranking() method for each round type
                                int xRanking = 0;
                                InactiveRound xIr = xChampionship as InactiveRound;
                                if (xIr != null)
                                {
                                    xRanking = xIr.Ranking().IndexOf(xc);
                                }
                                else
                                {
                                    List<Club> xRankingClubs = new List<Club>(xChampionship.clubs);
                                    xRankingClubs.Sort(new ClubRankingComparator(xChampionship.matches));
                                    xRanking = xRankingClubs.IndexOf(xc);
                                }

                                int yRanking = 0;
                                InactiveRound yIr = yChampionship as InactiveRound;
                                if(yIr != null)
                                {
                                    yRanking = yIr.Ranking().IndexOf(yc);
                                }
                                else
                                {
                                    List<Club> yRankingClubs = new List<Club>(yChampionship.clubs);
                                    yRankingClubs.Sort(new ClubRankingComparator(yChampionship.matches));
                                    yRanking = yRankingClubs.IndexOf(yc);
                                }
                                res = xRanking - yRanking;
                            }
                        }
                        else
                        {
                            res = x.Level() > y.Level() ? -1 : 1;
                        }
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
