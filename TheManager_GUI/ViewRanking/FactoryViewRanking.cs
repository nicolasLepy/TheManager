using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public class FactoryViewRanking
    {

        public static View CreerVue(DataGrid grid, Round round, double sizeMultiplier = 1, bool focusOnTeam = false, Club team = null)
        {
            View res = null;

            if (round as ChampionshipRound != null)
            {
                res = new ViewRankingChampionship(grid, round as ChampionshipRound, sizeMultiplier, focusOnTeam, team);
            }
            if (round as KnockoutRound != null)
            {
                res = new ViewRankingElimination(grid, round as KnockoutRound, sizeMultiplier);
            }
            if (round as GroupsRound != null)
            {
                res = new ViewRankingGroups(grid, round as GroupsRound, sizeMultiplier, focusOnTeam, team);
            }

            return res;
        }

    }
}
