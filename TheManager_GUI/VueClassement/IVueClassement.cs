using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public interface IVueClassement
    {

        void Afficher();
        void Remplir(StackPanel spClassement);

    }
}
