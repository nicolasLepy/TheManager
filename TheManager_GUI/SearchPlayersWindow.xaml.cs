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
    /// Logique d'interaction pour SearchPlayersWindow.xaml
    /// </summary>
    public partial class SearchPlayersWindow : Window
    {
        public SearchPlayersWindow()
        {
            InitializeComponent();
        }

        private void FillPlayersList(List<Player> players)
        {
            spPlayers.Children.Clear();
            lbPlayersCount.Content = players.Count + " joueurs";
            if(players.Count > 1000)
            {
                lbPlayersCount.Content = "Trop de joueurs trouvés. Seuls les 1000 premiers affichés";
            }
            int i = 0;
            foreach (Player p in players)
            {
                if (i < 1000)
                {
                    StackPanel spPlayer = new StackPanel();
                    spPlayer.Orientation = Orientation.Horizontal;
                    spPlayer.Children.Add(ViewUtils.CreateLabel(p.firstName + " " + p.lastName, "StyleLabel2", 11, 150));
                    spPlayer.Children.Add(ViewUtils.CreateLabel(p.position.ToString(), "StyleLabel2", 11, 70));
                    spPlayer.Children.Add(ViewUtils.CreateLabel(p.Age.ToString() + " ans", "StyleLabel2", 11, 70));

                    Image imgFlag = new Image();

                    imgFlag.Source = new BitmapImage(new Uri(Utils.Flag(p.nationality), UriKind.RelativeOrAbsolute));
                    imgFlag.Width = 30;
                    imgFlag.Height = 15;
                    StackPanel spFlag = new StackPanel();
                    spFlag.HorizontalAlignment = HorizontalAlignment.Center;
                    spFlag.Width = 100;
                    spFlag.Children.Add(imgFlag);

                    spPlayer.Children.Add(spFlag);
                    StackPanel spStarsLevel = ViewUtils.CreateStarNotation(Utils.GetStars(p.level), 15);
                    spStarsLevel.Width = 100;
                    spPlayer.Children.Add(spStarsLevel);
                    StackPanel spStarsPotential = ViewUtils.CreateStarNotation(Utils.GetStars(p.potential), 15);
                    spStarsPotential.Width = 100;
                    spPlayer.Children.Add(spStarsPotential);

                    spPlayer.Children.Add(ViewUtils.CreateLabel(p.EstimateTransferValue().ToString() + " €", "StyleLabel2", 11, 100));
                    spPlayer.Children.Add(ViewUtils.CreateLabel(p.EstimateWage().ToString() + " €/m", "StyleLabel2", 11, 100));


                    Club c = p.Club;
                    if (c != null)
                    {
                        spPlayer.Children.Add(ViewUtils.CreateLabel(c.shortName, "StyleLabel2", 11, 150));
                    }

                    spPlayers.Children.Add(spPlayer);
                }
                i++;
            }
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RbNoConstraintChecked(object sender, RoutedEventArgs e)
        {
            List<Player> p = new List<Player>();
            FillPlayersList(p);
        }

        private void RbTransfertListChecked(object sender, RoutedEventArgs e)
        {
            List<Player> p = new List<Player>();
            foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                if (t.isChampionship)
                {
                    p.AddRange(Session.Instance.Game.kernel.TransfertList(t));
                }
            }
            FillPlayersList(p);
        }

        private void RbFreePlayersChecked(object sender, RoutedEventArgs e)
        {
            FillPlayersList(Session.Instance.Game.kernel.freePlayers);
        }

    }
}
