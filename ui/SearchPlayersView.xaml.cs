using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
using tm;
using TheManager_GUI.views;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour SearchPlayersView.xaml
    /// </summary>
    public partial class SearchPlayersView : Window
    {
        private List<Player> _currentPlayersBase;

        public SearchPlayersView()
        {
            _currentPlayersBase = new List<Player>();
            InitializeComponent();
        }

        public void Refresh()
        {
            if(playersPanel != null)
            {
                List<Player> players = FilterPlayers(_currentPlayersBase);
                PlayersView view = new PlayersView(players, 11, true, true, true, true, true, true, true, true, false, true, true, false, false, false, false, false, true, true);
                view.Full(playersPanel);
                textPlayersCount.Text = players.Count > 1000 ? FindResource("str_tooManyPlayers").ToString() : String.Format("{0} {1}", players.Count, FindResource("str_players"));
            }
        }

        private List<Player> FilterPlayers(List<Player> players)
        {
            List<Player> res = new List<Player>();
            int i = 0;
            foreach (Player p in players)
            {
                if (i < 1000 && p.Age <= int.Parse(textBoxUpperAge.Text) && p.Age >= int.Parse(textBoxBottomAge.Text))
                {
                    bool add = true;
                    if ((!checkboxG.IsChecked.Value && p.position == Position.Goalkeeper) ||
                        (!checkboxD.IsChecked.Value && p.position == Position.Defender) ||
                        (!checkboxM.IsChecked.Value && p.position == Position.Midfielder) ||
                        (!checkboxS.IsChecked.Value && p.position == Position.Striker))
                    {
                        add = false;
                    }
                    if (add)
                    {
                        res.Add(p);
                        i++;
                    }
                }
            }
            return res;
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

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void textBoxAge_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            bool ok = false;
            if (int.TryParse(e.Text, out int age) && age < 100)
            {
                ok = true;
            }

            e.Handled = regex.IsMatch(e.Text) && ok;
        }

        private void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void RefreshCheck()
        {
            if(checkboxTransferable.IsChecked == true)
            {
                _currentPlayersBase = new List<Player>();
                foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if (t.isChampionship)
                    {
                        _currentPlayersBase.AddRange(Session.Instance.Game.kernel.TransferList(t));
                    }
                }
            }
            else if (checkboxFree.IsChecked == true)
            {
                _currentPlayersBase = Session.Instance.Game.kernel.freePlayers;
            }
            else
            {
                _currentPlayersBase = new List<Player>();
            }
            Refresh();
        }

        private void textBoxAge_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Refresh();
            }
        }

        private void textBoxAge_LostFocus(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void checkboxTransferable_Checked(object sender, RoutedEventArgs e)
        {
            checkboxFree.IsChecked = false;
            RefreshCheck();
        }

        private void checkboxFree_Checked(object sender, RoutedEventArgs e)
        {
            checkboxTransferable.IsChecked = false;
            RefreshCheck();
        }
    }
}
