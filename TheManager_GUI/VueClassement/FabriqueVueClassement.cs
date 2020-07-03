using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public class FabriqueVueClassement
    {

        public static IVueClassement CreerVue(DataGrid grille, Round tour, double sizeMultiplier = 1, bool focusOnTeam = false, Club team = null)
        {
            IVueClassement res = null;

            if (tour as ChampionshipRound != null)
            {
                res = new VueClassementChampionnat(grille, tour as ChampionshipRound, sizeMultiplier, focusOnTeam, team);
            }
            if (tour as KnockoutRound != null)
            {
                res = new VueClassementEliminatoires(grille, tour as KnockoutRound, sizeMultiplier);
            }
            if (tour as GroupsRound != null)
            {
                res = new VueClassementPoules(grille, tour as GroupsRound, sizeMultiplier, focusOnTeam, team);
            }

            return res;
        }

    }
}
