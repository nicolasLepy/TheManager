using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public class FabriqueVueClassement
    {

        public static IVueClassement CreerVue(DataGrid grille, Round tour)
        {
            IVueClassement res = null;

            if (tour as ChampionshipRound != null)
                res = new VueCalendrierChampionnat(grille, tour as ChampionshipRound, 1);
            if (tour as KnockoutRound != null)
                res = new VueClassementEliminatoires(grille, tour as KnockoutRound);
            if (tour as GroupsRound != null)
                res = new VueClassementPoules(grille, tour as GroupsRound);

            return res;
        }

    }
}
