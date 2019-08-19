using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TheManager;
using TheManager.Comparators;

namespace TheManager_GUI.VueClassement
{
    public class VueClassementPoules : IVueClassement
    {

        private DataGrid _grille;
        private TourPoules _tour;

        public VueClassementPoules(DataGrid grille, TourPoules tour)
        {
            _grille = grille;
            _tour = tour;
        }

        public void Remplir(StackPanel spClassement)
        {

        }

        public void Afficher()
        {
            _grille.Items.Clear();
            for (int poules = 0; poules < _tour.NombrePoules; poules++)
            {
                List<Club> poule = new List<Club>(_tour.Poules[poules]);
                poule.Sort(new Club_Classement_Comparator(_tour.Matchs));
                int i = 0;
                _grille.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                _grille.Items.Add(new ClassementElement { Classement = i, Nom = "Poule " + (int)(poules + 1) });
                _grille.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                foreach (Club c in poule)
                {
                    i++;
                    _grille.Items.Add(new ClassementElement { Logo = Utils.Logo(c), Classement = i, Nom = c.NomCourt, Pts = _tour.Points(c), J = _tour.Joues(c), G = _tour.Gagnes(c), N = _tour.Nuls(c), P = _tour.Perdus(c), bp = _tour.ButsPour(c), bc = _tour.ButsContre(c), Diff = _tour.Difference(c) });
                }
            }
        }
    }
}
