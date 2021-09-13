using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TheManager_GUI.VueClassement
{
    public class ViewMatches : IViewRanking
    {

        private readonly DataGrid _grid;
        private readonly List<Match> _matches;
        public ViewMatches(DataGrid grid, List<Match> matches)
        {
            _grid = grid;
            _matches = matches;
        }

        public void Show()
        {
            throw new NotImplementedException();
        }

        public void Full(StackPanel spRanking)
        {

            _grid.Items.Clear();

           


        }
    }
}
