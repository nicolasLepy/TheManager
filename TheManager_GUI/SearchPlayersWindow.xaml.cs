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
            FillPlayersList(Session.Instance.Game.kernel.freePlayers);
        }

        private void FillPlayersList(List<Player> players)
        {
            spPlayers.Children.Clear();
            foreach(Player p in players)
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
                spPlayers.Children.Add(spPlayer);
            }
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
