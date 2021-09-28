using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public class FactoryViewRanking
    {

        public static View CreateView(Round round, double sizeMultiplier = 1, bool focusOnTeam = false, Club team = null, bool reduced = false)
        {
            View res = null;

            if (round as ChampionshipRound != null)
            {
                res = new ViewRankingChampionship(round as ChampionshipRound, sizeMultiplier, focusOnTeam, team, reduced);
            }
            if (round as KnockoutRound != null)
            {
                res = new ViewRankingElimination(round as KnockoutRound, sizeMultiplier+0.2);
            }
            if (round as GroupsRound != null)
            {
                res = new ViewRankingGroups(round as GroupsRound, sizeMultiplier, focusOnTeam, team);
            }

            return res;
        }

    }
}
