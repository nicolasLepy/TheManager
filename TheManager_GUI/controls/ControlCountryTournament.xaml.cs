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
    /// Logique d'interaction pour ControlCountryTournament.xaml
    /// </summary>
    public partial class ControlCountryTournament : UserControl
    {
        public string TitleHolder { get; set; }
        public string TournamentName { get; set; }
        public string ImagePath { get; set; }

        public ControlCountryTournament()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
