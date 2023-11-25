using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager.Comparators;

namespace TheManager.Tournaments
{
    public class GroupInactiveRound : GroupsRound
    {
        private List<Club> _ranking;

        public void SetRanking(List<Club> ranking)
        {
            _ranking = new List<Club>(ranking);
        }

        protected override void SpecialInitialize()
        {
            _ranking = null;
        }

        protected override GroupsRound Clone()
        {
            return new GroupInactiveRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), groupsCount, phases, programmation.initialisation, programmation.end, keepRankingFromPreviousRound, _randomDrawingMethod, _administrativeLevel, _fusionGroupAndNoGroupGames, _nonGroupGamesByTeams, _nonGroupGamesByGameday, programmation.gamesPriority);
        }

        protected override List<Club> RankClubs(List<Club> clubs, List<Tiebreaker> tiebreakers, Dictionary<Club, List<PointDeduction>> pointsDeduction)
        {
            List<Club> copy = new List<Club>(clubs);
            copy.Sort((club1, club2) => _ranking.IndexOf(club1).CompareTo(_ranking.IndexOf(club2)));
            return copy;
        }

        public override List<Club> Ranking(int group, bool inverse = false)
        {
            if (_ranking == null)
            {
                _ranking = new List<Club>(_clubs);
                try
                {
                    _ranking.Sort(new ClubRandomRankingComparator(5, -1));
                }
                catch
                {
                    Utils.Debug("Le classement aléatoire n'a pu être généré");
                }
            }
            List<Club> ranking = new List<Club>(_groups[group]);
            ranking.Sort((club1, club2) => _ranking.IndexOf(club1).CompareTo(_ranking.IndexOf(club2)));
            if (inverse)
            {
                ranking.Reverse();
            }
            return ranking;
        }

        public GroupInactiveRound(string name, Hour hour, List<GameDay> dates, List<TvOffset> offsets, int groupsCount, int phases, GameDay initialisation, GameDay end, int keepRankingFromPreviousRound, RandomDrawingMethod randomDrawingMethod, int administrativeLevel, bool fusionGroupAndNoGroupGames, int nonGroupGamesByTeams, int nonGroupGamesByGameday, int gamesPriority) : base(name, hour, dates, offsets, groupsCount, phases, initialisation, end, keepRankingFromPreviousRound, randomDrawingMethod, administrativeLevel, fusionGroupAndNoGroupGames, nonGroupGamesByTeams, nonGroupGamesByGameday, gamesPriority)
        {

        }
    }
}
