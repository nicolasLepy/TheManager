using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TheManager_GUI.VueClassement
{
    public class VueClassementEliminatoires : IVueClassement
    {

        private DataGrid _grille;

        public VueClassementEliminatoires(DataGrid grille)
        {
            _grille = grille;
        }

        public void Remplir(StackPanel spClassement)
        {

        }

        public void Afficher()
        {
            _grille.Items.Clear();
        }
    }
}
