using Microsoft.Win32;
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
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnNouvellePartie_Click(object sender, RoutedEventArgs e)
        {
            lbCreationPartie.Visibility = Visibility.Visible;
            Windows_ConfigurationPartie wcp = new Windows_ConfigurationPartie();
            wcp.Show();
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void BtnChargerPartie_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Partie p = new Partie();
                p.Charger(openFileDialog.FileName);
                Session.Instance.Partie = p;
                Windows_Menu wm = new Windows_Menu();
                wm.Show();
            }
        }
    }
}
