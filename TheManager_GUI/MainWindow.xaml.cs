using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
                    string dateColor = e2.Descendants("DateColor").First().Attribute("color").Value;
                    string promotionColor = e2.Descendants("PromotionColor").First().Attribute("color").Value;
                    string relegationColor = e2.Descendants("RelegationColor").First().Attribute("color").Value;
                    string upperPlayOffColor = e2.Descendants("UpperPlayOffColor").First().Attribute("color").Value;
                    string bottomPlayOffColor = e2.Descendants("BottomPlayOffColor").First().Attribute("color").Value;
                    string fontFamily = e2.Descendants("FontFamily").First().Attribute("name").Value;
                    Theme t = new Theme(name, backgroundColor, mainColor, secondaryColor, promotionColor, upperPlayOffColor, bottomPlayOffColor, relegationColor, fontFamily, dateColor);
                    Theme.themes.Add(t);
                }
            }

        }

        public void LoadBackgroundImage()
        {
            ImageSource imgSource = new BitmapImage(new Uri(Utils.Image("themanager.png")));
            mainWindow.Background = new ImageBrush(imgSource);
        }

        private DatabaseLoader _loader;
        public MainWindow()
        {
            Style = (Style) Resources["StyleWindows"];
            InitializeComponent();
            LoadBackgroundImage();
            LoadThemes();
        }

        private void BtnNouvellePartie_Click(object sender, RoutedEventArgs e)
        {

            //await CallLoadingGame();


            Game partie = new Game();
            Session.Instance.Game = partie;
            Kernel g = partie.kernel;
            _loader = new DatabaseLoader(g);
            DatabaseLoader cbdd = _loader;
            //cbdd.ReformateCities();

           
            
            cbdd.LoadLanguages();
            pbLoading.Value = 2;
            lbCreationPartie.Content = "Chargement de l'environnement";
            cbdd.LoadGeography();

            pbLoading.Value = 3;
            Console.WriteLine("3");
            lbCreationPartie.Content = "Chargement des calendriers";
            cbdd.LoadCalendars();

            pbLoading.Value = 4;
            Console.WriteLine("4");
            lbCreationPartie.Content = "Chargement des villes";
            cbdd.LoadCities();

            pbLoading.Value = 6;
            Console.WriteLine("6");
            lbCreationPartie.Content = "Chargement des stades";
            cbdd.LoadStadiums();

            pbLoading.Value = 10;
            Console.WriteLine("10");
            lbCreationPartie.Content = "Chargement des clubs";
            cbdd.LoadClubs();

            pbLoading.Value = 30;
            Console.WriteLine("30");
            lbCreationPartie.Content = "Chargement des compétitions";
            cbdd.LoadTournaments();

            pbLoading.Value = 40;
            Console.WriteLine("40");
            lbCreationPartie.Content = "Chargement des joueurs";
            cbdd.LoadPlayers();

            pbLoading.Value = 60;
            Console.WriteLine("60");
            lbCreationPartie.Content = "Chargement des entraîneurs";
            cbdd.LoadManagers();

            pbLoading.Value = 65;
            Console.WriteLine("65");
            lbCreationPartie.Content = "Initialisation des équipes";
            cbdd.InitTeams();

            pbLoading.Value = 90;
            Console.WriteLine("90");
            lbCreationPartie.Content = "Initialisation des joueurs";
            cbdd.InitPlayers();

            pbLoading.Value = 95;
            Console.WriteLine("95");
            lbCreationPartie.Content = "Chargement des médias";
            cbdd.LoadMedias();

            pbLoading.Value = 98;
            Console.WriteLine("98");
            lbCreationPartie.Content = "Chargement des commentaires de match";
            cbdd.LoadGamesComments();

            pbLoading.Value = 100;
            Console.WriteLine("100");

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
            openFileDialog.Filter = "The Manager Save|*.save";
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
