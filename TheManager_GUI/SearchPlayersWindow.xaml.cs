using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using TheManager;
using System.Windows.Documents;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour SearchPlayersWindow.xaml
    /// </summary>
    public partial class SearchPlayersWindow : Window
    {

        private List<Player> _currentPlayersBase;

        public SearchPlayersWindow()
        {
            InitializeComponent();
            _currentPlayersBase = new List<Player>();
        }

        private void AgeValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            bool ok = false;
            if (int.TryParse(e.Text, out int age) && age < 100)
            {
                ok = true;
            }

            e.Handled = regex.IsMatch(e.Text) && ok;
        }
        
        private List<Player> FilterPlayers(List<Player> players)
        {
            List<Player> res = new List<Player>();
            foreach(Player p in players)
            {
                if(p.Age <= int.Parse(tbMaxAge.Text) && p.Age >= int.Parse(tbMinAge.Text))
                {
                    bool add = true;
                    if((!cbGoalkeeper.IsChecked.Value && p.position == Position.Goalkeeper) ||
                        (!cbDefender.IsChecked.Value && p.position == Position.Defender) ||
                        (!cbMidfielder.IsChecked.Value && p.position == Position.Midfielder) ||
                        (!cbStriker.IsChecked.Value && p.position == Position.Striker) )
                    {
                        add = false;
                    }
                    if (add)
                    {
                        res.Add(p);
                    }
                }
            }
            return res;
        }

        private void FillPlayersList()
        {
            List<Player> players = FilterPlayers(_currentPlayersBase);
            spPlayers.Children.Clear();
            lbPlayersCount.Content = players.Count + " joueurs";
            if(players.Count > 1000)
            {
                lbPlayersCount.Content = "Trop de joueurs trouvés. Seuls les 1000 premiers affichés";
            }
            int i = 0;
            foreach (Player p in players)
            {
                int age = p.Age;
                if (i < 1000 && p.Age <= int.Parse(tbMaxAge.Text) && p.Age >= int.Parse(tbMinAge.Text))
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
            _currentPlayersBase = new List<Player>();
            FillPlayersList();
        }

        private void RbTransfertListChecked(object sender, RoutedEventArgs e)
        {
            _currentPlayersBase = new List<Player>();
            foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                if (t.isChampionship)
                {
                    _currentPlayersBase.AddRange(Session.Instance.Game.kernel.TransfertList(t));
                }
            }
            FillPlayersList();
        }

        private void RbFreePlayersChecked(object sender, RoutedEventArgs e)
        {
            _currentPlayersBase = Session.Instance.Game.kernel.freePlayers;
            FillPlayersList();
        }

        private void cbChanged(object sender, RoutedEventArgs e)
        {
            if (_currentPlayersBase != null)
            {
                FillPlayersList();
            }
        }

        private void tbChanged(object sender, TextChangedEventArgs e)
        {
            if(_currentPlayersBase != null)
            {
                FillPlayersList();
            }
        }
    }
}
