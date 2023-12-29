using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using tm;
using TheManager_GUI.Styles;

namespace TheManager_GUI.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlPenaltyShootout.xaml
    /// </summary>
    public partial class ControlPenaltyShootout : UserControl
    {
        public ControlPenaltyShootout(Match match)
        {
            InitializeComponent();
            Initialize(match);
        }

        public void Initialize(Match match)
        {
            gridShootout.ColumnDefinitions.Clear();
            int columns = (match.penaltyShoots1.Count > match.penaltyShoots2.Count ? match.penaltyShoots1.Count : match.penaltyShoots2.Count) + 1;
            for(int i = 0; i < columns; i++)
            {
                gridShootout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Pixel) });
            }
            for(int i = 0; i < match.penaltyShoots1.Count; i++)
            {
                bool success = match.penaltyShoots1[i];
                Ellipse kick = new Ellipse();
                kick.Width = 10;
                kick.Height = 10;
                kick.Fill = success ? Brushes.Green : Brushes.Red;
                ViewUtils.AddElementToGrid(gridShootout, kick, 1, i);
            }
            ViewUtils.AddElementToGrid(gridShootout, ViewUtils.CreateTextBlock(match.penaltyShootout1.ToString(), StyleDefinition.styleTextPlain), 1, gridShootout.ColumnDefinitions.Count - 1);
            for (int i = 0; i < match.penaltyShoots2.Count; i++)
            {
                bool success = match.penaltyShoots2[i];
                Ellipse kick = new Ellipse();
                kick.Width = 10;
                kick.Height = 10;
                kick.Fill = success ? Brushes.Green : Brushes.Red;
                ViewUtils.AddElementToGrid(gridShootout, kick, 2, i);
            }
            ViewUtils.AddElementToGrid(gridShootout, ViewUtils.CreateTextBlock(match.penaltyShootout2.ToString(), StyleDefinition.styleTextPlain), 2, gridShootout.ColumnDefinitions.Count - 1);
            Grid.SetColumnSpan(tbTitle, gridShootout.ColumnDefinitions.Count);

        }
    }
}
