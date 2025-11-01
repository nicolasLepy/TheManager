using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using System.Xml.Linq;
using tm;
using tm.Comparators;
using TheManager_GUI.Styles;
using TheManager_GUI.views;
using Path = System.IO.Path;

namespace TheManager_GUI
{

    public enum LoadDatabaseProgressReportType
    {
        PROGRESS,
        FINISH
    }

    public struct LoadDatabaseProgressReport
    {
        public int value;
        public string text;
        public LoadDatabaseProgressReportType type;
        public Game game;
        public LoadDatabaseProgressReport(int value, string text, LoadDatabaseProgressReportType type, Game game)
        {
            this.value = value;
            this.text = text;
            this.type = type;
            this.game = game;
        }
    }

    public class ThreadLoadDatabase
    {
        public delegate void StatusUpdateHandler(int progressValue, string text);

        private IProgress<LoadDatabaseProgressReport> progress;

        private void RaiseUpdateEvent(int progressValue, string text, LoadDatabaseProgressReportType type, Game game=null)
        {
            progress.Report(new LoadDatabaseProgressReport(progressValue, text, type, game));
        }

        public async Task Run(IProgress<LoadDatabaseProgressReport> progress)
        {
            this.progress = progress;
            await Task.Run(() =>
            {
                Game game = new Game();
                Session.Instance.Game = game;
                Kernel g = game.kernel;

                DatabaseLoader _loader = new DatabaseLoader(game, g);
                DatabaseLoader cbdd = _loader;
                //cbdd.ReformateCities();

                cbdd.LoadDatabaseMetadata();
                cbdd.LoadLanguages();
                RaiseUpdateEvent(2, Application.Current.FindResource("str_loading_env").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.LoadWorld();
                cbdd.LoadAudios();
                cbdd.LoadCalendars();
                cbdd.LoadCities();
                cbdd.LoadStadiums();
                RaiseUpdateEvent(10, Application.Current.FindResource("str_loading_clubs").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.LoadClubs();
                RaiseUpdateEvent(30, Application.Current.FindResource("str_loading_tournaments").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.LoadTournaments();
                cbdd.LoadInternationalDates();
                RaiseUpdateEvent(40, Application.Current.FindResource("str_loading_players").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.LoadPlayers();
                RaiseUpdateEvent(50, Application.Current.FindResource("str_loading_managers").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.LoadManagers();
                RaiseUpdateEvent(55, Application.Current.FindResource("str_loading_init_teams").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.InitTeams();
                RaiseUpdateEvent(75, Application.Current.FindResource("str_loading_init_players").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.InitPlayers();
                RaiseUpdateEvent(90, Application.Current.FindResource("str_loading_init_tournaments").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.InitTournaments();
                cbdd.LoadMedias();
                cbdd.LoadGamesComments();
                cbdd.LoadRules();
                cbdd.CreateRegionalPathForCups();
                RaiseUpdateEvent(98, Application.Current.FindResource("str_loading_archives").ToString(), LoadDatabaseProgressReportType.PROGRESS);
                cbdd.LoadArchives();
                cbdd.PostProcess();
                RaiseUpdateEvent(100, "", LoadDatabaseProgressReportType.FINISH, game);

            });
        }
    }

    /// <summary>
    /// Logique d'interaction pour NewGameView.xaml
    /// </summary>
    public partial class NewGameView : Window
    {

        private int gridCountriesColumns = 5;
        private int defaultGridCountriesRows = 25;
        private int gridCountriesRows;
        private double flagSize = 0;

        private Club selectedClub = null;

        private Dictionary<Tournament, CheckBox> checkBoxes;

        public NewGameView()
        {
            InitializeComponent();
            progressBarLoading.Visibility = Visibility.Hidden;
            flagSize = (double)Application.Current.FindResource(StyleDefinition.fontSizeNavigation) * (5 / 3f);
            FillComboDatabases();
            SetVisibilityClubSelectionStep(false);
            SetEnabledButtonsClubSelection(false);
            SetEnabledButtonsLeagueSelection(false);
        }

        private void SetVisibilityClubSelectionStep(bool visible)
        {
            Visibility visibility = visible ? Visibility.Visible : Visibility.Hidden;
            imagePresentationBudget.Visibility = visibility;
            imagePresentationCountry.Visibility = visibility;
            imagePresentationStadium.Visibility = visibility;
            imagePresentationStartDate.Visibility = visibility;
            imagePresentationStatus.Visibility = visibility;
            tbPresentationBudget.Visibility = visibility;
            tbPresentationCountry.Visibility = visibility;
            tbPresentationStadium.Visibility = visibility;
            tbPresentationStartDate.Visibility = visibility;
            tbPresentationStatus.Visibility = visibility;
        }

        private void SetEnabledButtonsLeagueSelection(bool enabled)
        {
            buttonEnableAllLeagues.IsEnabled = enabled;
            buttonDisableAllLeagues.IsEnabled = enabled;
            buttonLeaguesValidation.IsEnabled = enabled;
        }

        private void SetEnabledButtonsClubSelection(bool enabled)
        {
            buttonSelectClub.IsEnabled = enabled;
        }

        private void SetEnabledButtonsDatabaseSelection(bool enabled)
        {
            buttonSelectDatabase.IsEnabled = enabled;
        }

        private void SetCheckBoxesStatus(bool enabled)
        {
            foreach(KeyValuePair<Tournament, CheckBox> kvp in checkBoxes)
            {
                kvp.Value.IsEnabled = enabled;
            }
        }

        private List<string> GetDatabases()
        {
            List<string> databases = new List<string>();
            if(Directory.Exists("data"))
            {
                string[] directories = Directory.GetDirectories("data");
                foreach (string directory in directories)
                {
                    if (directory.StartsWith(Path.Join("data", "database_")))
                    {
                        databases.Add(directory.Remove(0, 14));

                    }
                }
            }
            return databases;
        }

        private void FillComboDatabases()
        {
            comboBoxDatabase.Items.Clear();
            foreach (string database in GetDatabases())
            {
                comboBoxDatabase.Items.Add(database);
            }
            if(comboBoxDatabase.Items.Count == 0)
            {
                comboBoxDatabase.Items.Add("No database found");
                comboBoxDatabase.IsEnabled = false;
            }
            comboBoxDatabase.SelectedIndex = 0;
        }

        private void FillNationalities()
        {
            foreach (Continent c in Session.Instance.Game.kernel.world.GetAllContinents())
            {
                foreach (Country country in c.countries)
                {
                    comboBoxCountry.Items.Add(country);
                }
            }
            comboBoxCountry.SelectedIndex = 0;
        }

        public void DatabaseLoadProgressUpdate(LoadDatabaseProgressReport report)
        {
            if(report.type == LoadDatabaseProgressReportType.PROGRESS)
            {
                progressBarLoading.Value = report.value;
                textBlockLoading.Text = report.text;
            }
            else if(report.type == LoadDatabaseProgressReportType.FINISH)
            {
                OnDatabaseLoaded(report.game);
            }

            /*
             * If thread context, use Dispatcher to update UI. Check in PlayGameWindows.xaml.cs
             *  this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    progressBarLoading.Value = value;
                    textBlockLoading.Text = text;
                }));
             */
        }

        private void OnDatabaseLoaded(Game game)
        {
            progressBarLoading.Visibility = Visibility.Hidden;
            textBlockLoading.Text = "";
            FillNationalities();
            FillLeaguesGrid(game);
            SetEnabledButtonsDatabaseSelection(false);
            SetEnabledButtonsLeagueSelection(true);
        }

        private void FillLeaguesGrid(Game game)
        {
            gridCountriesRows = defaultGridCountriesRows;
            gridCountriesSelection.Children.Clear();
            gridCountriesSelection.RowDefinitions.Clear();
            gridCountriesSelection.ColumnDefinitions.Clear();
            for(int i = 0; i < gridCountriesColumns; i++)
            {
                gridCountriesSelection.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                gridCountriesSelection.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });
            }
            int countItems = FillAssociation(game.kernel.worldAssociation, 0, false);
            if (countItems > gridCountriesRows * gridCountriesColumns)
            {
                gridCountriesRows = (int)Math.Ceiling(countItems / (gridCountriesColumns + 0.0));
            }

            for (int i = 0; i < gridCountriesRows; i++)
            {
                gridCountriesSelection.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(flagSize * 1.2, GridUnitType.Pixel) });
            }
            checkBoxes = new Dictionary<Tournament, CheckBox>();
            FillAssociation(game.kernel.worldAssociation, 0, true);
            CountConfiguration();
        }

        private int FillAssociation(Association association, int counter, bool addItemsToGrid)
        {

            //Display association name only if childs have championships
            if (association.GetAllTournaments().Count(t => t.isChampionship) > association.Tournaments().Count(t => t.isChampionship))
            {
                TextBlock tbContinentName = ViewUtils.CreateTextBlock(association.Name(), StyleDefinition.styleTextPlainCenter);
                if(addItemsToGrid)
                {
                    ViewUtils.AddElementToGrid(gridCountriesSelection, tbContinentName, counter % gridCountriesRows, (counter / gridCountriesRows) * 2, 2);
                }
                counter++;
            }

            //TODO: Only tournaments where the top level is handled by this association
            int championships = association.tournaments.Count(p => p.isChampionship);
            if(championships > 0)
            {
                Image imageCountry = ViewUtils.CreateFlag(association.ClosestStateAssociation().localisation as Country, flagSize, flagSize * 0.66);
                TextBlock tbCountryName = ViewUtils.CreateTextBlock(association.Name(), StyleDefinition.styleTextPlain);
                if(addItemsToGrid)
                {
                    ViewUtils.AddElementToGrid(gridCountriesSelection, imageCountry, counter % gridCountriesRows, (counter / gridCountriesRows) * 2);
                    ViewUtils.AddElementToGrid(gridCountriesSelection, tbCountryName, counter % gridCountriesRows, (counter / gridCountriesRows) * 2 + 1);
                }
                counter++;
                foreach (Tournament league in association.Tournaments())
                {
                    if (league.isChampionship)
                    {
                        CheckBox cbLeague = new CheckBox();
                        cbLeague.IsChecked = DefaultChecked(league);
                        cbLeague.Content = league.name;
                        cbLeague.Style = FindResource(StyleDefinition.styleCheckBox) as Style;
                        cbLeague.Click += CheckboxLeague_Click;
                        if(addItemsToGrid)
                        {
                            ViewUtils.AddElementToGrid(gridCountriesSelection, cbLeague, counter % gridCountriesRows, (counter / gridCountriesRows) * 2, 2);
                            checkBoxes.Add(league, cbLeague);
                        }
                        counter++;
                    }
                }
            }
            foreach (Association a in association.associations)
            {
                counter = FillAssociation(a, counter, addItemsToGrid);
            }
            return counter;
        }

        private bool DefaultChecked(Tournament t)
        {
            Association fr = Session.Instance.Game.kernel.String2Association("France");
            return Session.Instance.Game.kernel.LocalisationTournament(t).IsDirectConnected(fr);
        }

        private void CountConfiguration()
        {
            int clubs = 0;
            int totalClubs = 0;
            int players = 0;
            int leagues = 0;
            int cups = 0;
            foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                if(!t.isChampionship)
                {
                    cups++;
                }
            }
            foreach (KeyValuePair<Tournament, CheckBox> kvp in checkBoxes)
            {
                Tournament c = kvp.Key;
                totalClubs += c.rounds[0].clubs.Count;
                if (kvp.Value.IsChecked == true)
                {
                    leagues++;
                    clubs += c.rounds[0].clubs.Count;
                    players += c.rounds[0].clubs.Count * 21;
                }
            }

            tbLeagues.Text = leagues.ToString();
            tbCups.Text = cups.ToString();
            tbActiveClubs.Text = clubs.ToString();
            tbTotalClubs.Text = totalClubs.ToString();
            tbPlayersEstimation.Text = players.ToString();
        }

        private void DisableTournaments()
        {
            List<Tournament> toDesactivate = new List<Tournament>();
            foreach (KeyValuePair<Tournament, CheckBox> kvp in checkBoxes)
            {
                if (kvp.Value.IsChecked == false)
                {
                    Console.WriteLine("Desactivate " + kvp.Key.name);
                    toDesactivate.Add(kvp.Key);
                }

            }
            foreach (Tournament c in toDesactivate)
            {
                c.DisableTournament();
                int pr = 0;
                int re = 0;
                foreach (Qualification q in c.rounds[0].qualifications)
                {
                    if (q.isNextYear && q.roundId == 0 && c.IsAbove(q.target))
                    {
                        re++;
                    }
                    if (q.isNextYear && q.roundId == 0 && c.IsBelow(q.target))
                    {
                        pr++;
                    }
                }
                Console.WriteLine("[" + c.name + "] " + pr + " promotions et " + re + " relegations");
            }
        }

        private void FillSelectClub(Tournament tournament)
        {
            if(tournament.rounds.Count > 0)
            {
                List<Club> clubs = new List<Club>(tournament.rounds[0].clubs);
                clubs.Sort(new ClubComparator(ClubAttribute.CITY_NAME));
                gridSelectClub.Children.Clear();
                gridSelectClub.RowDefinitions.Clear();
                for (int i = 0; i < clubs.Count; i++)
                {
                    Club club = clubs[i];
                    gridSelectClub.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(flagSize * 1.2, GridUnitType.Pixel) });
                    ViewUtils.AddElementToGrid(gridSelectClub, ViewUtils.CreateLogo(clubs[i], flagSize, flagSize), i, 0);
                    TextBlock tbClub = ViewUtils.CreateTextBlock(clubs[i].name, StyleDefinition.styleTextNavigation, -1, -1, club as CityClub == null ? Application.Current.FindResource(StyleDefinition.solidColorBrushColorBorderLight) as SolidColorBrush : null, null, false, true);
                    ViewUtils.AddElementToGrid(gridSelectClub, tbClub, i, 1);
                    if (club as CityClub != null)
                    {
                        tbClub.MouseLeftButtonUp += (sender, e) => textBlockPlayerClub_OnClick(sender, e, club);
                    }
                    ViewUtils.AddElementToGrid(gridSelectClub, ViewUtils.CreateStarsView(clubs[i].Stars, (float)(flagSize/2)), i, 2);
                }
            }
        }

        private void FillClubsInformation(Club club)
        {
            SetVisibilityClubSelectionStep(true);
            selectedClub = club;
            List<Player> players = new List<Player>(club.Players());
            players.Sort(new PlayerComparator(true, PlayerAttribute.POSITION));
            PlayersView view = new PlayersView(players, 1, true, true, true, false, true, true, false, false, false, true, false, false, false, false, false, false, false, false, true);
            view.Full(spClubPlayers);

            DateTime beginDate = Session.Instance.Game.GetBeginDate(club.Association());

            tbClubBudget.Text = Utils.FormatMoney((club as CityClub).budget);
            tbClubCountry.Text = club.Country().Name();
            tbClubStadium.Text = club.stadium.name + " (" + club.stadium.capacity + " " + FindResource("str_seats").ToString() + ")";
            tbClubStartDate.Text = beginDate.ToShortDateString();
            tbClubStatus.Text = FindResource(Utils.ClubStatus2ResourceString(club.status)).ToString();
        }

        private void StartGame()
        {
            if (selectedClub != null)
            {
                string firstName = textBoxFirstName.Text;
                string lastName = textBoxLastName.Text;
                string[] strBirthday = datePickerBirthDate.Text.Split('/');
                DateTime birthday = new DateTime(int.Parse(strBirthday[2]), int.Parse(strBirthday[1]), int.Parse(strBirthday[0]));
                Country defaultNationality = Session.Instance.Game.kernel.String2Country("France");
                Country selectedCountry = comboBoxCountry.SelectedItem as Country;
                if (selectedCountry == null)
                {
                    selectedCountry = defaultNationality;
                }

                Session.Instance.Game.club = selectedClub as CityClub;
                Session.Instance.Game.SetBeginDate(Session.Instance.Game.GetBeginDate(selectedClub.Association()));
                Manager manager = new Manager(Session.Instance.Game.kernel.NextIdPerson(), firstName, lastName, 70, birthday, selectedCountry);
                Session.Instance.Game.club.ChangeManager(manager);
                MainMenuView view = new MainMenuView();
                view.Show();
                Close();
            }
        }

        private void FillSelectLeague()
        {
            TournamentsTreeViewController controller = new TournamentsTreeViewController(tvSelectLeague, Session.Instance.Game.kernel.worldAssociation);
            controller.TournamentValidator = TreeViewSelectClubTournamentIsValid;
            controller.OnClickTournament = stackPanelNavigation_OnClick;
            controller.ContentStyle = StyleDefinition.styleTextPlain;
            controller.Fill();
        }

        public bool TreeViewSelectClubTournamentIsValid(Tournament tournament)
        {
            return checkBoxes.ContainsKey(tournament) && checkBoxes[tournament].IsChecked == true;
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

        private async void buttonSelectDatabase_Click(object sender, RoutedEventArgs e)
        {
            if(comboBoxDatabase.IsEnabled && comboBoxDatabase.SelectedItem != null)
            {
                Utils.dataFolderName = String.Format("data{0}database_{1}", System.IO.Path.DirectorySeparatorChar, comboBoxDatabase.SelectedItem.ToString());
                progressBarLoading.Visibility = Visibility.Visible;
                ThreadLoadDatabase thread = new ThreadLoadDatabase();
                await thread.Run(new Progress<LoadDatabaseProgressReport>(DatabaseLoadProgressUpdate));
            }
        }

        private List<Tournament> GetTournamentsBelow(Tournament selected)
        {
            List<Tournament> tournaments = new List<Tournament>();

            Association tAssociation = Session.Instance.Game.kernel.LocalisationTournament(selected);
            foreach (Tournament t in tAssociation.Tournaments())
            {
                if (t.level > selected.level && t.isChampionship)
                {
                    tournaments.Add(t);
                }
            }
            foreach(Association a in tAssociation.GetAllChilds())
            {
                tournaments.AddRange(a.Leagues());
            }


            return tournaments;
        }

        private List<Tournament> GetTournamentsAbove(Tournament selected)
        {
            List<Tournament> tournaments = new List<Tournament>();

            Association tAssociation = Session.Instance.Game.kernel.LocalisationTournament(selected);
            foreach (Tournament t in tAssociation.Tournaments())
            {
                if (t.level < selected.level && t.isChampionship)
                {
                    tournaments.Add(t);
                }
            }
            foreach(Tournament t in tAssociation.TournamentsAbove(false))
            {
                if(t.isChampionship)
                {
                    tournaments.Add(t);
                }
            }
            return tournaments;
        }

        private void CheckboxLeague_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            Tournament selected = null;
            foreach (KeyValuePair<Tournament, CheckBox> kvp in checkBoxes)
            {
                if(kvp.Value == checkBox)
                {
                    selected = kvp.Key;
                }
            }
            List<Tournament> tAbove = GetTournamentsAbove(selected);
            List<Tournament> tBelow = GetTournamentsBelow(selected);
            foreach(Tournament t in tAbove)
            {
                checkBoxes[t].IsChecked = true;
            }
            foreach (Tournament t in tBelow)
            {
                checkBoxes[t].IsChecked = false;
            }
            CountConfiguration();

        }
        private void buttonEnableAllLeagues_Click(object sender, RoutedEventArgs e)
        {
            foreach(CheckBox checkbox in checkBoxes.Values)
            {
                checkbox.IsChecked = true;
            }
            CountConfiguration();
        }

        private void buttonDisableAllLeagues_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox checkbox in checkBoxes.Values)
            {
                checkbox.IsChecked = false;
            }
            CountConfiguration();
        }

        private void buttonLeaguesValidation_Click(object sender, RoutedEventArgs e)
        {
            DisableTournaments();
            FillSelectLeague();
            SetEnabledButtonsClubSelection(true);
            SetEnabledButtonsLeagueSelection(false);
            SetCheckBoxesStatus(false);

        }

        private void stackPanelNavigation_OnClick(object sender, RoutedEventArgs e, Tournament tournament)
        {
            FillSelectClub(tournament);
        }

        private void textBlockPlayerClub_OnClick(object sender, RoutedEventArgs e, Club club)
        {
            FillClubsInformation(club);
        }

        private void buttonSelectClub_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }
    }
}
