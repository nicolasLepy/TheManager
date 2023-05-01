using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.TextFormatting;

namespace TheManager.Comparators
{
    public class ClubRankingComparator : IComparer<Club>
    {
        private readonly List<Match> _round;
        private readonly RankingType _rankingType;
        private readonly List<Tiebreaker> _tiebreakers;
        private readonly bool _inverted;
        private readonly bool _knockoutRound;

        public ClubRankingComparator(List<Match> games, List<Tiebreaker> tiebreakers, RankingType rankingType = RankingType.General, bool inverted = false, bool knockoutRound = false, Round addRankingFromThisRound = null)
        {
            _round = new List<Match>(games);
            _rankingType = rankingType;
            _inverted = inverted;
            _knockoutRound = knockoutRound;
            _tiebreakers = tiebreakers;
            if (addRankingFromThisRound != null)
            {
                _round.AddRange(addRankingFromThisRound.matches);
            }
        }

        public int CompareKnockoutRound(Club x, Club y)
        {
            int res = 0;
            bool gameFound = false;
            for(int j = this._round.Count-1; j >= 0 && !gameFound; j--)
            {
                Match game = this._round[j];
                if((game.home == x && game.away == y) || (game.home == y && game.away == x))
                {
                    res = 1;
                    if(game.Winner == x)
                    {
                        res = -1;
                    }
                    gameFound = true;
                }
            }
            return res;
        }

        public int Compare(Club x, Club y)
        {
            int res = 0;
            if(_knockoutRound)
            {
                res = CompareKnockoutRound(x, y);
            }
            else
            {
                int pointsY = Points(y);
                int pointsX = Points(x);
                if (pointsY < pointsX)
                {
                    res = -1;
                }
                if (pointsY > pointsX)
                {
                    res = 1;
                }
                if (res == 0)
                {
                    for (int i = 0; i < _tiebreakers.Count && res == 0; i++)
                    {
                        Tiebreaker tb = _tiebreakers[i];
                        switch (tb)
                        {
                            case Tiebreaker.GoalDifference:
                                int differenceY = Difference(y);
                                int differenceX = Difference(x);
                                if (differenceY < differenceX)
                                {
                                    res = -1;
                                }
                                if (differenceY > differenceX)
                                {
                                    res = 1;
                                }
                                break;
                            case Tiebreaker.GoalFor:
                                int goalForY = GoalFor(y);
                                int goalForX = GoalFor(x);
                                if (goalForY < goalForX)
                                {
                                    res = -1;
                                }
                                if (goalForY > goalForX)
                                {
                                    res = 1;
                                }
                                break;
                            case Tiebreaker.GoalAgainst:
                                int goalAgainstY = GoalAgainst(y);
                                int goalAgainstX = GoalAgainst(x);
                                if (goalAgainstY > goalAgainstX)
                                {
                                    res = -1;
                                }
                                if (goalAgainstY < goalAgainstX)
                                {
                                    res = 1;
                                }
                                break;
                            case Tiebreaker.HeadToHead:
                                List<Tiebreaker> tiebreakersH2H = new List<Tiebreaker>(_tiebreakers);
                                tiebreakersH2H.Remove(Tiebreaker.HeadToHead);
                                int points = Points(x);
                                List<Club> roundClub = ClubsFromGames();
                                List<Club> clubs = new List<Club>() { x, y };
                                foreach (Club c in roundClub)
                                {
                                    if(!clubs.Contains(c) && Points(c) == points)
                                    {
                                        clubs.Add(c);
                                    }
                                }
                                List<Match> games = new List<Match>();
                                foreach(Match game in _round)
                                {
                                    if(game.Played && clubs.Contains(game.home) && clubs.Contains(game.away))
                                    {
                                        games.Add(game);
                                    }
                                }
                                if(games.Count > 0)
                                {
                                    clubs.Sort(new ClubRankingComparator(games, tiebreakersH2H, _rankingType, false, _knockoutRound));
                                    res = clubs.IndexOf(y) > clubs.IndexOf(x) ? -1 : 1;
                                }
                                break;
                            case Tiebreaker.Discipline:
                            default:
                                int disciplineX = Discipline(y);
                                int disciplineY = Discipline(x);
                                if (disciplineY > disciplineX)
                                {
                                    res = -1;
                                }
                                if (disciplineY < disciplineX)
                                {
                                    res = 1;
                                }
                                break;
                        }
                    }
                    if(res == 0)
                    {
                        res = y.name.CompareTo(x.name);
                    }
                }
            }
            return _inverted ? -res : res;
        }

        private int Points(Club c)
        {
            return Utils.Points(_round, c, _rankingType);
        }

        private int Difference(Club c)
        {
            return Utils.Difference(_round, c, _rankingType);
        }

        private int GoalFor(Club c)
        {
            return Utils.Gf(_round, c, _rankingType);
        }

        private int GoalAgainst(Club c)
        {
            return Utils.Ga(_round, c, _rankingType);
        }

        private int Discipline(Club c)
        {
            return Utils.CountEvent(GameEvent.YellowCard, _round, c, _rankingType) + (3 * Utils.CountEvent(GameEvent.RedCard, _round, c, _rankingType));
        }

        private List<Club> ClubsFromGames()
        {
            List<Club> res = new List<Club>();
            foreach(Match m in _round)
            {
                if(!res.Contains(m.home))
                {
                    res.Add(m.home);
                }
                if (!res.Contains(m.away))
                {
                    res.Add(m.away);
                }
            }
            return res;
        }
    }
}