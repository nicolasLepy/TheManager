using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public class FabriqueVueClassement
    {

        public static IVueClassement CreerVue(DataGrid grille, Tour tour)
        {
            IVueClassement res = null;

            if (tour as TourChampionnat != null)
                res = new VueCalendrierChampionnat(grille, tour as TourChampionnat);
            if (tour as TourElimination != null)
                res = new VueClassementEliminatoires(grille);
            if (tour as TourPoules != null)
                res = new VueClassementPoules(grille, tour as TourPoules);

            return res;
        }

    }
}
