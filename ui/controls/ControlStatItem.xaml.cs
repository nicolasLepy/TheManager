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

namespace TheManager_GUI.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlStatItem.xaml
    /// </summary>
    public partial class ControlStatItem : UserControl
    {

        public string StatName { get; set; }
        public double StatHome { get; set; }
        public string StatHomeString { get; set; }
        public double StatAway { get; set; }
        public string StatAwayString { get; set; }
        public double StatTotal { get; set; }

        public ControlStatItem(string name, double statHome, double statAway, bool isPercent)
        {
            StatName = name;
            StatHome = statHome;
            StatHomeString = isPercent ? String.Format("{0}%", statHome) : statHome.ToString();
            StatAwayString = isPercent ? String.Format("{0}%", statAway) : statAway.ToString();
            StatTotal = statHome + statAway;
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
