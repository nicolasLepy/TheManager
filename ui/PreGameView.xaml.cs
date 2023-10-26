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
using TheManager;
using TheManager_GUI.controls;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.views;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour PreGameView.xaml
    /// </summary>
    public partial class PreGameView : Window
    {

        private readonly List<Match> matchs;
        private readonly Club activeClub;
        private List<Player> players;
        private List<Player> subs;
        private ControlComposition controlHomeComposition;
        private ControlComposition controlAwayComposition;
        private ControlComposition controlHomeSubs;
        private ControlComposition controlAwaySubs;

        public PreGameView(List<Match> matchs, Club club)
        {
            this.matchs = matchs;
            this.activeClub = club;
            players = new List<Player>();
            subs = new List<Player>();
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            textHomeClubName.Text = matchs[0].home.name;
            textAwayClubName.Text = matchs[0].away.name;
            textStadium.Text = matchs[0].stadium.name;
            textTournament.Text = matchs[0].Tournament.name;
            imageTournament.Source = new BitmapImage(new Uri(Utils.LogoTournament(matchs[0].Tournament)));

            textOddHome.Text = matchs[0].odd1.ToString("0.00");
            textOddDraw.Text = matchs[0].oddD.ToString("0.00");
            textOddAway.Text = matchs[0].odd2.ToString("0.00");

            imageHomeClub.Source = new BitmapImage(new Uri(Utils.Logo(matchs[0].home)));
            imageAwayClub.Source = new BitmapImage(new Uri(Utils.Logo(matchs[0].away)));

            controlHomeComposition = new ControlComposition(ControlCompositionType.Composition, matchs[0].home);
            controlAwayComposition = new ControlComposition(ControlCompositionType.Composition, matchs[0].away);
            controlHomeSubs = new ControlComposition(ControlCompositionType.Subs, matchs[0].home);
            controlAwaySubs = new ControlComposition(ControlCompositionType.Subs, matchs[0].away);
            if (matchs[0].home == activeClub)
            {
                controlHomeComposition.OnClickPlayer = OnRemovePlayerFromComposition;
                controlHomeSubs.OnClickPlayer = OnRemoveSubFromComposition;
            }
            if (matchs[0].away == activeClub)
            {
                controlAwayComposition.OnClickPlayer = OnRemovePlayerFromComposition;
                controlAwaySubs.OnClickPlayer = OnRemoveSubFromComposition;
            }
            ViewUtils.AddElementToGrid(gridHomeComposition, controlHomeComposition, 0, 0);
            ViewUtils.AddElementToGrid(gridAwayComposition, controlAwayComposition, 0, 0);
            ViewUtils.AddElementToGrid(gridHomeComposition, controlHomeSubs, 1, 0);
            ViewUtils.AddElementToGrid(gridAwayComposition, controlAwaySubs, 1, 0);


            ViewScores view = new ViewScores(matchs, false, true, false, false, false, false, 11);
            view.Full(panelOtherGames);
            InitializeViewAvailablePlayers();
            RefreshComposition(matchs[0].home);
            RefreshComposition(matchs[0].away);
        }

        public void RefreshComposition(Club club)
        {
            List<Player> compoPlayers = club == activeClub ? players : matchs[0].home == club ? matchs[0].compo1 : matchs[0].compo2;
            ControlComposition controlComposition = club == matchs[0].home ? controlHomeComposition : controlAwayComposition;

            List<Player> subsPlayers = club == activeClub ? subs : matchs[0].home == club ? matchs[0].Subs1 : matchs[0].Subs2;
            ControlComposition controlSubs = club == matchs[0].home ? controlHomeSubs : controlAwaySubs;

            controlComposition.Fill(compoPlayers);
            controlSubs.Fill(subsPlayers);
        }

        private void InitializeViewAvailablePlayers()
        {
            List<Player> availablePlayers = new List<Player>();
            foreach (Player player in activeClub.Players())
            {
                if(!players.Contains(player) && !subs.Contains(player))
                {
                    availablePlayers.Add(player);
                }
            }
            PlayersView view = new PlayersView(availablePlayers, 1, true, true, true, false, true, true, false, false, true, false, false, true, true, false, false, false, false, false, true);
            view.OnClickPlayer = OnAddPlayerToComposition;
            view.Full(panelAvailable);
        }

        private void SetPlayerClubCompo()
        {
            matchs[0].SetCompo(new List<Player>(players), new List<Player>(subs), activeClub);
        }

        public void OnRemovePlayerFromComposition(object sender, MouseEventArgs e, Player player)
        {
            players.Remove(player);
            RefreshComposition(activeClub);
            InitializeViewAvailablePlayers();
        }

        public void OnRemoveSubFromComposition(object sender, MouseEventArgs e, Player player)
        {
            subs.Remove(player);
            RefreshComposition(activeClub);
            InitializeViewAvailablePlayers();
        }

        public void OnAddPlayerToComposition(object sender, MouseEventArgs e, Player player)
        {
            int fieldPlayers = players.Where(c => c.position != Position.Goalkeeper).Count();
            int gk = players.Where(c => c.position == Position.Goalkeeper).Count();
            bool isGk = player.position == Position.Goalkeeper;
            if((!isGk && fieldPlayers < 10) || (isGk && gk == 0))
            {
                players.Add(player);
                RefreshComposition(activeClub);
                InitializeViewAvailablePlayers();
            }
            else if(subs.Count < 7)
            {
                subs.Add(player);
                RefreshComposition(activeClub);
                InitializeViewAvailablePlayers();
            }
        }

        private bool CheckComposition()
        {
            bool pursue = false;
            if (players.Count < 11)
            {
                MessageBoxResult result = MessageBox.Show(FindResource("str_lessThan11Players").ToString(), FindResource("str_lineups").ToString(), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    pursue = true;
                }
            }
            else
            {
                pursue = true;
            }
            return pursue;
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            players.Clear();
            subs.Clear();
            RefreshComposition(activeClub);
            InitializeViewAvailablePlayers();
        }

        private void buttonAutoTeam_Click(object sender, RoutedEventArgs e)
        {
            players.Clear();
            subs.Clear();

            List<Player> matchCompo = activeClub.Composition(matchs[0]);
            List<Player> matchSubs = activeClub.Subs(matchs[0], matchCompo, 7);
            foreach (Player p in matchCompo)
            {
                players.Add(p);
            }
            foreach(Player p in matchSubs)
            {
                subs.Add(p);
            }
            RefreshComposition(activeClub);
            InitializeViewAvailablePlayers();
        }

        private void buttonInstantResult_Click(object sender, RoutedEventArgs e)
        {
            if (CheckComposition())
            {
                SetPlayerClubCompo();
                foreach (Match m in matchs)
                {
                    m.Play();
                }
                Close();
            }

        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (CheckComposition())
            {
                SetPlayerClubCompo();
                Windows_JouerMatch wjm = new Windows_JouerMatch(matchs);
                wjm.ShowDialog();
                Close();
            }
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
    }
}
