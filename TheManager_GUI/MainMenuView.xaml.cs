﻿using Microsoft.Win32;
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
using TheManager.Comparators;
using TheManager_GUI.Styles;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.views;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI
{


    public class ComboBoxDayController
    {
        private List<KeyValuePair<Round, int>> roundsShortcuts;
        private bool _showDays;
        public bool showDays
        {
            get
            {
                return _showDays;
            }
            set
            {
                _showDays = value;
                date = Session.Instance.Game.date;
            }
        }
        public Round activeRound { get; set; }
        public DateTime date { get; set; }
        
        public Tournament tournament { get; set; }

        public ComboBoxDayController()
        {
            roundsShortcuts = new List<KeyValuePair<Round, int>>();
            showDays = false;
            activeRound = null;
        }

        public List<KeyValuePair<Round, int>> GetRoundsRegistered()
        {
            return roundsShortcuts;
        }

        public void Initialize(bool showDays)
        {
            this.showDays = showDays;
            roundsShortcuts.Clear();
        }

        public int GetRegisteredRoundIndex(Round round, int gameDay)
        {
            int res = 0;
            for(int i = 0; i < roundsShortcuts.Count; i++)
            {
                if (roundsShortcuts[i].Key == round && roundsShortcuts[i].Value == gameDay)
                {
                    res = i;
                }
            }
            return res;
        }

        public void Populate(Tournament tournament)
        {
            foreach (Round round in tournament.rounds)
            {
                for (int i = 0; i < round.MatchesDayNumber(); i++)
                {
                    roundsShortcuts.Add(new KeyValuePair<Round, int>(round, i + 1));
                }
            }
        }
    }

    /// <summary>
    /// Logique d'interaction pour MainMenuView.xaml
    /// </summary>
    public partial class MainMenuView : Window
    {

        private readonly Game _game;
        private bool _noScreenRefresh;
        private ComboBoxDayController _comboBoxDayController;
        private GameModifiers _modifiers;

        public MainMenuView()
        {
            InitializeComponent();
            _game = Session.Instance.Game;
            _modifiers = new GameModifiers();
            _noScreenRefresh = false;
            _comboBoxDayController = new ComboBoxDayController();

            imageClub.Source = new BitmapImage(new Uri(Utils.Logo(_game.club)));
            this.KeyDown += new KeyEventHandler(KeyPress);

            Refresh();
            FillTournamentsTreeView();
            if (_game.club.Championship != null && _game.club.Championship.rounds[0].matches.Count > 0)
            {
                RefreshGameDayComboBox(_game.club.Championship);
            }
            else
            {
                buttonSwitchScoresMode_Click(null, null);

            }
            /*FillRankingPanel(_game.club.Championship.rounds[0]);
            FillScoresPanel(_game.club.Championship.rounds[0].matches);*/
        }

        private void Save()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "The Manager Save|*.csave";
            if (saveFileDialog.ShowDialog() == true)
            {
                Session.Instance.Game.Save(saveFileDialog.FileName);
            }
        }

        private void ShowOptions()
        {
            Windows_Options wo = new Windows_Options();
            wo.Show();
        }

        private void ShowSearchPlayers()
        {
            SearchPlayersWindow spw = new SearchPlayersWindow();
            spw.Show();
        }

        private void Play()
        {
            List<Match> matchs = _game.NextDay();
            if (matchs.Count > 0)
            {
                Windows_AvantMatch wam = new Windows_AvantMatch(matchs, _game.club);
                wam.ShowDialog();
            }
            _game.UpdateTournaments();
            Refresh();
        }

        private void Refresh()
        {
            if(!_noScreenRefresh)
            {
                tbGameDate.Text = _game.date.ToString("dddd dd MMMM yyyy");
                Match nextGame = _game.club.NextGame;
                FillNextGamePanel(nextGame);
            }
        }

        private void RefreshTournament(Tournament tournament)
        {
            FillRankingPanel(tournament.rounds[0]);
            RefreshGameDayComboBox(tournament);
        }

        private void RefreshGameDayComboBox(Tournament tournament)
        {
            comboBoxGameDay.Items.Clear();
            _comboBoxDayController.Initialize(false);
            _comboBoxDayController.Populate(tournament);
            _comboBoxDayController.tournament = tournament;
            foreach(KeyValuePair<Round, int> registeredRound in _comboBoxDayController.GetRoundsRegistered())
            {
                string name = registeredRound.Key.IsKnockOutRound() ? (registeredRound.Key.MatchesDayNumber() == 1 ? "" : (registeredRound.Value == 1 ? "Matchs aller" : "Matchs retour")) : String.Format("{0}{1} journée", (registeredRound.Value), (registeredRound.Value) == 1 ? "ère" : "ème");
                bool isChampionship = registeredRound.Key as ChampionshipRound != null || (tournament.rounds.Count > 0 && tournament.rounds[0] == registeredRound.Key && tournament.isChampionship);
                if(!isChampionship)
                {
                    name = String.Format("{0}{1}{2}", registeredRound.Key.name, name.Length > 0 ? " - " : "", name);
                }
                comboBoxGameDay.Items.Add(name);
            }
            if(comboBoxGameDay.Items.Count > 0)
            {
                DateTime today = _game.date;
                DateTime closestTime = DateTime.MinValue;
                int comboBoxIndex = 0;
                foreach(Round round in tournament.rounds)
                {
                    DateTime nextRoundDate = round.NextMatchesDate();
                    if( (today - nextRoundDate).Days < (today - closestTime).Days)
                    {
                        closestTime = nextRoundDate;
                        comboBoxIndex = _comboBoxDayController.GetRegisteredRoundIndex(round, round.NextMatchesGameDay());
                    }
                }
                comboBoxGameDay.SelectedIndex = comboBoxIndex;
            }
        }

        private void FillNextGamePanel(Match game)
        {
            tbNextGameHomeTeam.Text = "";
            tbNextGameAwayTeam.Text = "";
            tbNextGameDate.Text = "Pas de match à venir";
            tbNextGameHour.Text = "";
            tbNextGameStadiumName.Text = "";
            tbNextGameTournamentName.Text = "";
            imageNextGameHomeLogo.Source = null;
            imageNextGameAwayLogo.Source = null;
            imageNextGameTournamentLogo.Source = null;
            spNextGameHomeStars.Children.Clear();
            spNextGameAwayStars.Children.Clear();

            if (game != null)
            {
                tbNextGameDate.Text = game.day.ToShortDateString();
                tbNextGameHour.Text = game.day.ToShortTimeString();

                tbNextGameHomeTeam.Text = game.home.name.ToUpper();
                tbNextGameAwayTeam.Text = game.away.name.ToUpper();
                spNextGameHomeStars.Children.Add(ViewUtils.CreateStarsView(game.home.Stars, 20));
                spNextGameAwayStars.Children.Add(ViewUtils.CreateStarsView(game.away.Stars, 20));
                imageNextGameHomeLogo.Source = new BitmapImage(new Uri(Utils.Logo(game.home)));
                imageNextGameAwayLogo.Source = new BitmapImage(new Uri(Utils.Logo(game.away)));

                imageNextGameTournamentLogo.Source = new BitmapImage(new Uri(Utils.LogoTournament(game.Tournament)));

                tbNextGameStadiumName.Text = game.stadium.name;
                tbNextGameTournamentName.Text = game.Tournament.name.ToUpper();

            }
        }

        private void FillRankingPanel(Round round)
        {
            FactoryViewRanking.CreateView(round, 0.9).Full(spRanking);
        }

        private void FillScoresPanel(List<Match> matchs)
        {
            List<MatchAttribute> comparaisonAttributes = _comboBoxDayController.showDays ? new List<MatchAttribute>() { MatchAttribute.TOURNAMENT, MatchAttribute.DATE } : new List<MatchAttribute>() { MatchAttribute.DATE };
            matchs.Sort(new MatchComparator(comparaisonAttributes));
            ViewScores view = new ViewScores(matchs, true, true, true, false, true, _comboBoxDayController.showDays, (float)(double)Application.Current.FindResource(StyleDefinition.fontSizeSecondary), false, null, false, false, false, _comboBoxDayController.showDays);
            view.Full(spRoundScores);
        }

        private void FillTournamentsTreeView()
        {
            tvTournaments.Items.Clear();
            tvTournaments.Items.Add(CreateNavigationContinent(_game.kernel.world));
        }

        private StackPanel CreateTreeViewItemComponent(string itemName, string imagePath)
        {
            StackPanel spNavigationItem = new StackPanel();
            spNavigationItem.Orientation = Orientation.Horizontal;
            spNavigationItem.Margin = new Thickness(0, 2, 0, 2);
            Image logoTournament = new Image();
            logoTournament.Style = Application.Current.FindResource("image") as Style;
            logoTournament.Source = new BitmapImage(new Uri(imagePath));
            logoTournament.Height = 14;
            logoTournament.Width = 21;
            logoTournament.Margin = new Thickness(0, 0, 10, 0);
            TextBlock tbTournament = new TextBlock();
            tbTournament.Style = Application.Current.FindResource("textNavigation") as Style;
            tbTournament.Text = itemName;
            tbTournament.VerticalAlignment = VerticalAlignment.Center;

            spNavigationItem.Children.Add(logoTournament);
            spNavigationItem.Children.Add(tbTournament);

            return spNavigationItem;
        }

        private StackPanel CreateNavigationTournament(Tournament tournament)
        {
            StackPanel spTournament = CreateTreeViewItemComponent(tournament.name, Utils.LogoTournament(tournament));
            spTournament.MouseLeftButtonUp += (sender, e) => stackPanelNavigation_OnClick(sender, e, tournament);
            return spTournament;
        }
        
        private TreeViewItem CreateNavigationContinent(Continent continent)
        {
            TreeViewItem treeViewItemContainer = new TreeViewItem();
            treeViewItemContainer.Margin = new Thickness(0, 2, 0, 2);

            StackPanel spTreeViewItemHeader = CreateTreeViewItemComponent(continent.Name(), Utils.Logo(continent));
            treeViewItemContainer.Header = spTreeViewItemHeader;

            foreach (Tournament t in continent.Tournaments())
            {
                treeViewItemContainer.Items.Add(CreateNavigationTournament(t));
            }

            foreach(Continent subContinent in continent.continents)
            {
                treeViewItemContainer.Items.Add(CreateNavigationContinent(subContinent));
            }

            foreach(Country country in continent.countries)
            {
                if(country.Tournaments().Count > 0)
                {
                    treeViewItemContainer.Items.Add(CreateNavigationCountry(country));
                }
            }

            return treeViewItemContainer;

        }

        private TreeViewItem CreateNavigationCountry(Country country)
        {
            TreeViewItem treeViewItemContainer = new TreeViewItem();
            treeViewItemContainer.Margin = new Thickness(0, 2, 0, 2);

            StackPanel spTreeViewItemHeader = CreateTreeViewItemComponent(country.Name(), Utils.Flag(country));
            treeViewItemContainer.Header = spTreeViewItemHeader;

            foreach(Tournament t in country.Tournaments())
            {
                treeViewItemContainer.Items.Add(CreateNavigationTournament(t));
            }

            return treeViewItemContainer;
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

        private void tvTournaments_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TreeView && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void tvTournaments_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }

        private void stackPanelNavigation_OnClick(object sender, RoutedEventArgs e, Tournament tournament)
        {
            RefreshTournament(tournament);
        }


        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void buttonOptions_Click(object sender, RoutedEventArgs e)
        {
            ShowOptions();
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowSearchPlayers();
        }

        private void buttonLongSimulation_Click(object sender, RoutedEventArgs e)
        {
            bool oldNoScreenRefreshValue = _noScreenRefresh;
            bool oldSimulateGamesValue = _game.options.simulateGames;
            _noScreenRefresh = true;
            _game.options.simulateGames = true;
            for (int i = 0; i < 10; i++)
            {
                Play();
                while (!(_game.date.Month == 6 && _game.date.Day == 13))
                {
                    Play();
                }
            }
            _noScreenRefresh = oldNoScreenRefreshValue;
            _game.options.simulateGames = oldSimulateGamesValue;
        }

        private void buttonSimulation_Click(object sender, RoutedEventArgs e)
        {
            bool oldNoScreenRefreshValue = _noScreenRefresh;
            bool oldSimulateGamesValue = _game.options.simulateGames;
            _noScreenRefresh = true;
            _game.options.simulateGames = true;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Play();
            while (!(_game.date.Month == 5 && _game.date.Day == 21))
            {
                Play();
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Utils.Debug("Total execution " + elapsedMs + "ms");
            _noScreenRefresh = oldNoScreenRefreshValue;
            _game.options.simulateGames = oldSimulateGamesValue;
            /*
                PrintAdministrativeRetrogradations();
                PrintGames();
             */
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            _modifiers.CheckDuplicates();
            Play();
        }

        private void buttonWorldCoefficients_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonWorldRankings_Click(object sender, RoutedEventArgs e)
        {

        }

        public void ComboBoxUpdateDay()
        {
            comboBoxGameDay.Items.Clear();
            comboBoxGameDay.Items.Add(_comboBoxDayController.date.ToLongDateString());
            comboBoxGameDay.SelectedIndex = 0;
            List<Match> matchs = Session.Instance.Game.kernel.MatchsOfDate(_comboBoxDayController.date);
            FillScoresPanel(matchs);
        }

        private void comboBoxGameDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!_comboBoxDayController.showDays)
            {
                if (comboBoxGameDay.SelectedIndex > -1 && comboBoxGameDay.SelectedIndex < _comboBoxDayController.GetRoundsRegistered().Count)
                {
                    KeyValuePair<Round, int> selectedRound = _comboBoxDayController.GetRoundsRegistered()[comboBoxGameDay.SelectedIndex];
                    if (_comboBoxDayController.activeRound != selectedRound.Key)
                    {
                        FillRankingPanel(selectedRound.Key);
                    }
                    FillScoresPanel(selectedRound.Key.GamesDay(selectedRound.Value));
                }
            }
        }

        private void buttonScoresLeft_Click(object sender, RoutedEventArgs e)
        {
            if(_comboBoxDayController.showDays)
            {
                _comboBoxDayController.date = _comboBoxDayController.date.AddDays(-1);
                ComboBoxUpdateDay();
            }
            else
            {
                if (comboBoxGameDay.SelectedIndex > 0)
                {
                    comboBoxGameDay.SelectedIndex--;
                }
            }
        }

        private void buttonScoreRight_Click(object sender, RoutedEventArgs e)
        {
            if(_comboBoxDayController.showDays)
            {
                _comboBoxDayController.date = _comboBoxDayController.date.AddDays(1);
                ComboBoxUpdateDay();
            }
            else
            {
                if (comboBoxGameDay.SelectedIndex < comboBoxGameDay.Items.Count - 1)
                {
                    comboBoxGameDay.SelectedIndex++;
                }
            }
        }

        private void buttonSwitchScoresMode_Click(object sender, RoutedEventArgs e)
        {
            _comboBoxDayController.showDays = !_comboBoxDayController.showDays;
            buttonSwitchScoresMode.Content = _comboBoxDayController.showDays ? "Voir compétition" : "Voir tous les matchs";
            if(_comboBoxDayController.showDays)
            {
                ComboBoxUpdateDay();
            }
            else if(_comboBoxDayController.tournament != null)
            {
                RefreshGameDayComboBox(_comboBoxDayController.tournament);
            }
        }

        /* CONTROL GAME */

        private void KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                _modifiers.RemovingPoints();
            }
            if (e.Key == Key.O)
            {
                _modifiers.CheckPlayoffTrees();
            }
            if (e.Key == Key.R)
            {
                Console.WriteLine("Relegations");
                _modifiers.SetAdministrativeRetrogradationsFr();
            }
        }
    }
}
