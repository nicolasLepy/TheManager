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
    /// Logique d'interaction pour Windows_ChoixClub.xaml
    /// </summary>
    public partial class Windows_ChoixClub : Window
    {
        public Windows_ChoixClub()
        {
            InitializeComponent();
            foreach(Continent c in Session.Instance.Partie.Gestionnaire.Continents)
            {
                foreach(Pays p in c.Pays)
                {
                    if(p.Competitions().Count > 0)
                    {
                        lbPays.Items.Add(p);
                    }
                }
            }
        }

        private void LbPays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pays p = lbPays.SelectedItem as Pays;
            if(p != null)
            {
                lbChampionnats.Items.Clear();
                foreach(Competition c in p.Competitions())
                {
                    if(c.Championnat)
                    {
                        lbChampionnats.Items.Add(c);
                    }
                }
            }
        }

        private void LbChampionnats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Competition c = lbChampionnats.SelectedItem as Competition;
            if(c != null)
            {
                lbClubs.Items.Clear();
                foreach(Club cl in c.Tours[0].Clubs)
                {
                    lbClubs.Items.Add(cl);
                }
            }
        }

        private void LbClubs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Club c = lbClubs.SelectedItem as Club;
            if(c != null)
            {
                spEtoiles.Children.Clear();
                try
                {
                    imgClub.Source = new BitmapImage(new Uri(Utils.Logo(c)));
                }
                catch
                {
                    Console.WriteLine("Pas de logo disponible pour " + c.Logo);
                }
                float etoiles = c.Etoiles;
                int etoilesEntieres = (int)Math.Floor(etoiles);
                for(int i = 1; i<=etoilesEntieres; i++)
                {
                    Image img = new Image();
                    img.Width = 20;
                    img.Height = 20;
                    img.Source = new BitmapImage(new Uri(Utils.Image("star.png")));
                    spEtoiles.Children.Add(img);
                }
                if(etoiles - etoilesEntieres != 0)
                {
                    Image img = new Image();
                    img.Width = 20;
                    img.Height = 20;
                    img.Source = new BitmapImage(new Uri(Utils.Image("demistar.png")));
                    spEtoiles.Children.Add(img);
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Club c = lbClubs.SelectedItem as Club;
            if(c != null)
            {
                Session.Instance.Partie.Club = c as Club_Ville;
                Windows_Menu wm = new Windows_Menu();
                wm.Show();
                Close();
            }

        }
    }
}
