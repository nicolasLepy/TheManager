using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using TheManager;
using TheManager.Parsers;

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


        public static void SelectCulture()
        {

            //Copy all MergedDictionarys into a auxiliar list.
            var dictionaryList = Application.Current.Resources.MergedDictionaries.ToList();

            //Search for the specified culture.
            string requestedCulture = "StringResources.xaml";
            var resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == requestedCulture);


            //If we have the requested resource, remove it from the list and place at the end.     
            //Then this language will be our string table to use.      
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }

        }

        public MainWindow()
        {

            SelectCulture();

            Style = (Style) Resources["StyleWindows"];
            InitializeComponent();
            LoadBackgroundImage();
            LoadThemes();
        }

        private void BtnNouvellePartie_Click(object sender, RoutedEventArgs e)
        {


            /*
            FifaDataParser fifaDataParser = new FifaDataParser();
            fifaDataParser.Parse();*/



            //await CallLoadingGame();

            DialogDatabase dd = new DialogDatabase();
            bool? resultDialog = dd.ShowDialog();
            if(resultDialog == true)
            {
                Game partie = new Game();
                Session.Instance.Game = partie;
                Kernel g = partie.kernel;
                Utils.dataFolderName = "data\\database_" + dd.Database;
                _loader = new DatabaseLoader(g);
                DatabaseLoader cbdd = _loader;
                //cbdd.ReformateCities();



                cbdd.LoadLanguages();
                pbLoading.Value = 2;
                lbCreationPartie.Content = "Chargement de l'environnement";
                cbdd.LoadWorld();

                pbLoading.Value = 3;
                lbCreationPartie.Content = "Chargement des calendriers";
                cbdd.LoadCalendars();

                pbLoading.Value = 4;
                lbCreationPartie.Content = "Chargement des villes";
                cbdd.LoadCities();

                pbLoading.Value = 6;
                lbCreationPartie.Content = "Chargement des stades";
                cbdd.LoadStadiums();

                pbLoading.Value = 10;
                lbCreationPartie.Content = "Chargement des clubs";
                cbdd.LoadClubs();

                pbLoading.Value = 30;
                lbCreationPartie.Content = "Chargement des compétitions";
                cbdd.LoadTournaments();
                cbdd.LoadInternationalDates();

                pbLoading.Value = 40;
                lbCreationPartie.Content = "Chargement des joueurs";
                cbdd.LoadPlayers();

                pbLoading.Value = 50;
                lbCreationPartie.Content = "Chargement des entraîneurs";
                cbdd.LoadManagers();

                pbLoading.Value = 65;
                lbCreationPartie.Content = "Initialisation des équipes";
                cbdd.InitTeams();

                pbLoading.Value = 80;
                lbCreationPartie.Content = "Initialisation des joueurs";
                cbdd.InitPlayers();

                pbLoading.Value = 90;
                lbCreationPartie.Content = "Initialisation des compétitions";
                cbdd.InitTournaments();

                pbLoading.Value = 92;
                lbCreationPartie.Content = "Chargement des médias";
                cbdd.LoadMedias();

                pbLoading.Value = 95;
                lbCreationPartie.Content = "Chargement des commentaires de match";
                cbdd.LoadGamesComments();
                cbdd.LoadRules();
                cbdd.GenerateNationalCup();
                cbdd.CreateRegionalPathForCups();

                pbLoading.Value = 98;
                lbCreationPartie.Content = "Chargement des archives";
                cbdd.LoadArchives();

                pbLoading.Value = 100;

                Windows_ConfigurationPartie wcp = new Windows_ConfigurationPartie();
                wcp.Show();
            }


        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void BtnChargerPartie_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "The Manager Save|*.csave";
            if (openFileDialog.ShowDialog() == true)
            {
                Game p = new Game();
                p.Load(openFileDialog.FileName);
                Session.Instance.Game = p;

                foreach(Tournament t in p.kernel.Competitions)
                {
                    foreach (Round round in t.rounds)
                    {
                        GroupsRound gRound = round as GroupsRound;
                        if (gRound != null)
                        {
                            gRound.ClearCache();
                        }
                    }
                }

                Windows_Menu wm = new Windows_Menu();
                wm.Show();
            }
        }
    }
}
