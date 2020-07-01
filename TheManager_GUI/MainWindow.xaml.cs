using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Xml.Linq;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        private void LoadThemes()
        {
            XDocument doc = XDocument.Load("themes/themes.xml");

            foreach (XElement e in doc.Descendants("Themes"))
            {
                foreach (XElement e2 in e.Descendants("Theme"))
                {
                    string name = e2.Attribute("name").Value;
                    string backgroundColor = e2.Descendants("BackgroundColor").First().Attribute("color").Value;
                    string mainColor = e2.Descendants("MainColor").First().Attribute("color").Value;
                    string secondaryColor = e2.Descendants("SecondaryColor").First().Attribute("color").Value;
                    string fontFamily = e2.Descendants("FontFamily").First().Attribute("name").Value;
                    Theme t = new Theme(name, backgroundColor, mainColor, secondaryColor, fontFamily);
                    Theme.themes.Add(t);
                }
            }

        }

        public MainWindow()
        {
            InitializeComponent();
            LoadThemes();
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
                Game p = new Game();
                p.Load(openFileDialog.FileName);
                Session.Instance.Game = p;
                Windows_Menu wm = new Windows_Menu();
                wm.Show();
            }
        }
    }
}
