using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using tm;
using TheManager_GUI.Styles;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            SelectCulture();
            InitializeComponent();
            LoadThemes();

        }

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
            Theme.themes[0].SetAsCurrentTheme();

        }

        public static void SelectCulture()
        {
            Langue.langues.Add(new Langue("English", "en", "en-GB", "StringResources.xaml"));
            Langue.langues.Add(new Langue("Français", "fr", "fr-FR", "StringResources.fr.xaml"));

            Langue.langues[0].SetAsCurrentLangue();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            NewGameView newGameView = new NewGameView();
            newGameView.Show();
            return;
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "The Manager Save|*.csave";
            if (openFileDialog.ShowDialog() == true)
            {
                Game p = new Game();
                p.Load(openFileDialog.FileName);
                Session.Instance.Game = p;

                foreach (Tournament t in p.kernel.Competitions)
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

                MainMenuView view = new MainMenuView();
                view.Show();
            }
        }

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /* EVENTS HANDLER */

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int mParam, int lParam);

        private void spControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void spControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }
    }
}
