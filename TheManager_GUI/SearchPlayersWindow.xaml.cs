using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using TheManager;
using System.Windows.Documents;
using TheManager_GUI.ViewMisc;

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
            int i = 0;
            foreach(Player p in players)
            {
                if(i < 1000 && p.Age <= int.Parse(tbMaxAge.Text) && p.Age >= int.Parse(tbMinAge.Text))
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
                i++;
            }
            return res;
        }

        private void playerNameClick(Player p)
        {
            Windows_Joueur wj = new Windows_Joueur(p);
            wj.Show();
        }

        private void FillPlayersList()
        {
            List<Player> players = FilterPlayers(_currentPlayersBase);
            ViewPlayers view = new ViewPlayers(players, 11, true, true, true, true, true, true, true, false, true, true, false, false, false, false, false, true, true);
            view.Full(spPlayers);
            

            lbPlayersCount.Content = players.Count + " joueurs";
            if(players.Count > 1000)
            {
                lbPlayersCount.Content = "Trop de joueurs trouvés. Seuls les 1000 premiers affichés";
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
                    _currentPlayersBase.AddRange(Session.Instance.Game.kernel.TransferList(t));
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
