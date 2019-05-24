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
using System.Windows.Shapes;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_JouerMatch.xaml
    /// </summary>
    public partial class Windows_JouerMatch : Window
    {

        private Match _match;

        public Windows_JouerMatch(Match m)
        {
            InitializeComponent();
            _match = m;
        }

        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            _match.Jouer();
            Close();
        }
    }
}
