using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TheManager_GUI.VueClassement
{
    public class MatchesView : IVueClassement
    {

        private readonly DataGrid _grid;
        private readonly List<Match> _matches;
        public MatchesView(DataGrid grid, List<Match> matches)
        {
            _grid = grid;
            _matches = matches;
        }

        public void Afficher()
        {
            throw new NotImplementedException();
        }

        public void Remplir(StackPanel spClassement)
        {

            _grid.Items.Clear();

           


        }
    }
}
