using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.controls;
using TheManager_GUI.Styles;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.views;
using TheManager_GUI.Views;

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
        public Round activeRound { get; set; } //TODO: Not used ? Could be used to avoid refresh ranking each time gameday is changed
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

            imageClub.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(_game.club)));
            this.KeyDown += new KeyEventHandler(KeyPress);

            Refresh();
            FillTournamentsTreeView();
            if (_game.club.Championship != null && _game.club.Championship.rounds[0].matches.Count > 0)
            {
                textActiveTournamentName.Text = _game.club.Championship.name;
                RefreshGameDayComboBox(_game.club.Championship);
            }
            else
            {
                textActiveTournamentName.Text = "";
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
            OptionsView view = new OptionsView();
            view.Show();
        }

        private void ShowSearchPlayers()
        {
            SearchPlayersView view = new SearchPlayersView();
            view.Show();
        }

        private void ShowCalendar()
        {
            CalendarView view = new CalendarView();
            view.Show();
        }

        private void Play()
        {
            List<Match> matchs = _game.NextDay();
            if (matchs.Count > 0)
            {
                PreGameView view = new PreGameView(matchs, _game.club);
                view.Show();
            }
            _game.UpdateTournaments();
            Refresh();
        }

        private void Refresh()
        {
            if(!_noScreenRefresh)
            {
                tbGameDate.Text = _game.date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern);
                //tbGameDate.Text = _game.date.ToString("dddd dd MMMM yyyy");
                Match nextGame = _game.club.NextGame;
                FillNextGamePanel(nextGame);
                FillNews();
            }
        }

        private void FillNews()
        {
            spNews.Children.Clear();
            for (int i = Session.Instance.Game.articles.Count - 1, total=0; i >= 0 && total < 5; i--, total++)
            {
                Article article = Session.Instance.Game.articles[i];
                int days = Utils.DaysNumberBetweenTwoDates(Session.Instance.Game.date, article.publication);
                string dateString = days == 0 ? spNews.FindResource("str_today").ToString() : String.Format(spNews.FindResource("str_daysAgo").ToString(), days, (days == 1 ? "" : "s"));
                ControlNewsItem controlNewsItem = new ControlNewsItem() { Title = dateString, NewsContent = article.title };
                spNews.Children.Add(controlNewsItem);

            }
        }

        private void RefreshTournament(Tournament tournament)
        {
            textActiveTournamentName.Text = tournament.name;
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
                string name = registeredRound.Key.IsKnockOutRound() ? (registeredRound.Key.MatchesDayNumber() == 1 ? "" : (registeredRound.Value == 1 ? FindResource("str_first_leg").ToString() : FindResource("str_second_leg").ToString())) : String.Format(FindResource("str_matchweek").ToString(), (registeredRound.Value), (registeredRound.Value) == 1 ? FindResource("str_matchweek_numeral_first").ToString() : FindResource("str_matchweek_numeral_more").ToString());
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
            tbNextGameDate.Text = FindResource("str_noupcomingmatches").ToString();
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
                imageNextGameHomeLogo.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(game.home)));
                imageNextGameAwayLogo.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(game.away)));

                imageNextGameTournamentLogo.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.LogoTournament(game.Tournament)));

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
            TournamentsTreeViewController controller = new TournamentsTreeViewController(tvTournaments, _game.kernel.world);
            controller.TournamentValidator = (t) => true;
            controller.OnClickTournament = stackPanelNavigation_OnClick;
            controller.Fill();
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

        private void buttonCalendar_Click(object sender, RoutedEventArgs e)
        {
            ShowCalendar();
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

        private void buttonWorldRankings_Click(object sender, RoutedEventArgs e)
        {
            InternationalRankingView irv = new InternationalRankingView();
            irv.ShowDialog();

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
            buttonSwitchScoresMode.Content = _comboBoxDayController.showDays ? FindResource("str_seeTournament").ToString() : FindResource("str_allGames").ToString();
            if(_comboBoxDayController.showDays)
            {
                ComboBoxUpdateDay();
            }
            else if(_comboBoxDayController.tournament != null)
            {
                RefreshGameDayComboBox(_comboBoxDayController.tournament);
            }
        }

        private void textActiveTournamentName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(_comboBoxDayController.tournament != null)
            {
                TournamentView view = new TournamentView(_comboBoxDayController.tournament);
                view.Show();
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
                _modifiers.SetAdministrativeRetrogradationsFr();
            }
            if(e.Key == Key.T)
            {
                DateTime date = new DateTime(2021, 1, 1);
                Tournament tournament = Session.Instance.Game.kernel.String2Tournament("Airtricity League");
                for(int i = 0; i< 350; i++)
                {
                    date = date.AddDays(1);
                    IsCurrentlyPlaying(tournament, date);
                }
                date = new DateTime(2022, 1, 1);
                tournament = Session.Instance.Game.kernel.String2Tournament("South America World Cup Qualifiers");
                for (int i = 0; i < 1300; i++)
                {
                    date = date.AddDays(1);
                    IsCurrentlyPlaying(tournament, date);
                }
                date = new DateTime(2022, 1, 1);
                tournament = Session.Instance.Game.kernel.String2Tournament("Africa WC Qualifiers");
                for (int i = 0; i < 1300; i++)
                {
                    date = date.AddDays(1);
                    IsCurrentlyPlaying(tournament, date);
                }
            }
        }

        public bool IsCurrentlyPlaying(Tournament tournament, DateTime date)
        {
            int offsetYear = tournament.remainingYears % tournament.periodicity;
            DateTime startDate = tournament.rounds[0].DateInitialisationRound().AddYears(offsetYear);
            DateTime endDate = tournament.rounds.Last().DateEndRound().AddYears(offsetYear + tournament.rounds.Last().programmation.end.YearOffset);
            bool isCurrentlyPlaying = Utils.IsBefore(startDate, date) && Utils.IsBefore(date, endDate);
            Console.WriteLine(date.ToShortDateString() + " [IsCurrentlyPlaying][" + tournament.name + "][" + tournament.remainingYears + "]" + startDate.ToShortDateString() + " to " + endDate.ToShortDateString() + " : " + isCurrentlyPlaying);
            return isCurrentlyPlaying;
        }



        private void buttonGlobals_Click(object sender, RoutedEventArgs e)
        {
            GlobalView view = new GlobalView();
            view.Show();
        }
    }
}
