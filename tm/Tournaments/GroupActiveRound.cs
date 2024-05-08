using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using tm.Comparators;

namespace tm.Tournaments
{
    [DataContract(IsReference = true)]
    public class GroupActiveRound : GroupsRound
    {
        protected override void SpecialInitialize()
        {
            if (_fusionGroupAndNoGroupGames)
            {
                _matches.AddRange(Calendar.GenerateCalendar(clubs, this));
            }
            else
            {
                for (int i = 0; i < _groupsNumber; i++)
                {
                    _matches.AddRange(Calendar.GenerateCalendar(_groups[i], this));
                }
                if (this.nonGroupGamesByTeams > 0)
                {
                    _matches.AddRange(Calendar.GenerateNonConferenceGames(this));

                }
            }
        }

        protected override List<Club> RankClubs(List<Club> clubs, List<Tiebreaker> tiebreakers, Dictionary<Club, List<PointDeduction>> pointsDeduction)
        {
            List<Club> copy = new List<Club>(clubs);
            copy.Sort(new ClubRankingComparator(_matches, tiebreakers, pointsDeduction));
            return copy;
        }


        protected override GroupsRound Clone()
        {
            return new GroupActiveRound(Session.Instance.Game.kernel.NextIdRound(), name, Tournament, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), groupsCount, phases, programmation.initialisation, programmation.end, keepRankingFromPreviousRound, _randomDrawingMethod, _administrativeLevel, _fusionGroupAndNoGroupGames, _nonGroupGamesByTeams, _nonGroupGamesByGameday, programmation.gamesPriority);
        }

        public override List<Club> Ranking(int group, bool inverse = false)
        {
            List<Club> res = new List<Club>();
            if (_cacheRanking == null || _cacheRanking.Length != _groups.Length) //TODO: Normally, _cacheRanking can't be null. To remove.
            {
                ClearCache();
            }
            if (_cacheRanking.Length > group && _cacheRanking[group].Count > 0)
            {
                res = new List<Club>(_cacheRanking[group]);
                if (inverse)
                {
                    res.Reverse();
                }
            }
            else
            {
                int gamesPlayed = matches.Count(p => p.Played);
                res = new List<Club>(_groups[group]);
                if (gamesPlayed > 0)
                {
                    ClubRankingComparator comparator = new ClubRankingComparator(matches, tiebreakers, pointsDeduction, RankingType.General, inverse);
                    res.Sort(comparator);
                }
                _cacheRanking[group] = inverse ? Enumerable.Reverse(res).ToList() : res;
            }
            return res;
        }

        public GroupActiveRound() : base()
        {

        }
        public GroupActiveRound(int id, string name, Tournament tournament, Hour hour, List<GameDay> dates, List<TvOffset> offsets, int groupsCount, int phases, GameDay initialisation, GameDay end, int keepRankingFromPreviousRound, RandomDrawingMethod randomDrawingMethod, int administrativeLevel, bool fusionGroupAndNoGroupGames, int nonGroupGamesByTeams, int nonGroupGamesByGameday, int gamesPriority) : base(id, name, tournament, hour, dates, offsets, groupsCount, phases, initialisation, end, keepRankingFromPreviousRound, randomDrawingMethod, administrativeLevel, fusionGroupAndNoGroupGames, nonGroupGamesByTeams, nonGroupGamesByGameday, gamesPriority)
        {

        }
    }
}
