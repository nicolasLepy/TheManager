using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
using TheManager_GUI;
using TheManager_GUI.controls;
using tm;

namespace TheManager_GUI.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlStats.xaml
    /// </summary>
    public partial class ControlStats : UserControl
    {

        public ControlStats()
        {
            InitializeComponent();
        }

        public ControlStats(Match match) : this()
        {
            Update(match);
        }
        public void Update(Match match)
        {
            layout.Children.Clear();
            ControlStatItem csiPossession = new ControlStatItem(FindResource("str_possession").ToString(), match.statistics.HomePossession * 100, match.statistics.AwayPossession * 100, true);
            ControlStatItem csiShots = new ControlStatItem(FindResource("str_shots").ToString(), match.statistics.HomeShoots, match.statistics.AwayShoots, false);
            layout.Children.Add(csiPossession);
            layout.Children.Add(csiShots);

        }

    }
}
