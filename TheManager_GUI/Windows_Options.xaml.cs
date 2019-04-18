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
    /// Logique d'interaction pour Windows_Options.xaml
    /// </summary>
    public partial class Windows_Options : Window
    {

        public Windows_Options()
        {
            InitializeComponent();
            cbExporter.IsChecked = Session.Instance.Partie.Options.Exporter;
        }

        private void CbExporter_Click(object sender, RoutedEventArgs e)
        {
            Session.Instance.Partie.Options.Exporter = (bool)cbExporter.IsChecked;
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
