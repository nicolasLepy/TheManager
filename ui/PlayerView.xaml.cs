using LiveCharts;
using Newtonsoft.Json.Linq;
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

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour PlayerView.xaml
    /// </summary>
    public partial class PlayerView : Window
    {

        private readonly Player player;

        public PlayerView(Player player)
        {
            this.player = player;
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            tbPlayerName.Text = String.Format("{0} {1}", player.firstName, player.lastName);
            panelLevel.Children.Add(ViewUtils.CreateStarsView(player.Stars, 30));
            panelPotential.Children.Add(ViewUtils.CreateStarsView(player.StarsPotential, 30));
            imageLogo.Source = new BitmapImage(new Uri(Utils.Flag(player.nationality)));
            tbAge.Text = String.Format("{0} {1}", player.Age, FindResource("str_yo").ToString());
            tbBorn.Text = String.Format("{0} {1}", FindResource("str_born").ToString(), player.birthday.ToShortDateString());

            CityClub club = player.Club;
            if (club != null)
            {
                tbPlayerClub.Text = club.name;
                imagePlayerClub.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(club)));
            }
            else
            {
                tbPlayerClub.Text = FindResource("str_free").ToString();
                imagePlayerClub.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(club)));
            }

            tbValue.Text = Utils.FormatMoney(player.EstimateTransferValue());
            Contract ct = club == null ? null : club.FindContract(player);
            if (ct != null)
            {
                tbContract.Text = ct.end.ToShortDateString();
                tbWage.Text = string.Format("{0} / {1}", Utils.FormatMoney(ct.wage), FindResource("str_month").ToString());
            }
            else
            {
                tbContract.Text = FindResource("str_noClub").ToString();
                tbWage.Text = "";
            }

            List<Match> games = player.PlayedGamesThisYear();
            games.Sort(new MatchDateComparator());
            ViewScores view = new ViewScores(games, true, false, false, false, false, true);
            view.Full(panelPlayedGames);

            InitializeCharts();
            InitializePlayerHistory();
        }

        public void InitializeCharts()
        {

            List<string> labels = new List<string>();
            List<double> levels = new List<double>();
            List<double> played = new List<double>();
            List<double> goals = new List<double>();
            int i = player.history.Count;
            foreach (PlayerHistory hj in player.history)
            {
                labels.Add(hj.Year.ToString());
                levels.Add(hj.Level);
                played.Add(hj.GamesPlayed.Sum(k => k.Value));
                goals.Add(hj.Goals.Sum(k => k.Value));
            }
            ChartView chartProgression = new ChartView(ChartType.LINE_CHART, FindResource("str_progression").ToString(), new List<string>() { FindResource("str_progression").ToString() }, FindResource("str_level").ToString(), FindResource("str_years").ToString(), labels, false, 1, new List<List<double>>() { levels }, -1, 250, 0, 100);
            chartProgression.RenderChart(panelProgression);
            ChartView chartGames = new ChartView(ChartType.LINE_CHART, FindResource("str_matchPlayed").ToString(), new List<string>() { FindResource("str_matchPlayed").ToString() }, FindResource("str_games").ToString(), FindResource("str_years").ToString(), labels, false, 1, new List<List<double>>() { played }, -1, 200, 0);
            chartGames.RenderChart(panelHistoryGames);
            ChartView chartGoals = new ChartView(ChartType.LINE_CHART, FindResource("str_goalsScored").ToString(), new List<string>() { FindResource("str_goalsScored").ToString() }, FindResource("str_goals").ToString(), FindResource("str_years").ToString(), labels, false, 1, new List<List<double>>() { goals }, -1, 200, 0);
            chartGoals.RenderChart(panelHistoryGoals);
        }

        private void AddHistoryItem(int startYear, int endYear, string clubName, int games, int goals)
        {
            gridPlayerHistory.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            TextBlock tbYear = ViewUtils.CreateTextBlock(String.Format("{0}-{1}", startYear, endYear > -1 ? endYear.ToString() : ""), StyleDefinition.styleTextSecondary);
            TextBlock tbName = ViewUtils.CreateTextBlock(clubName, StyleDefinition.styleTextSecondary);
            TextBlock tbGames = ViewUtils.CreateTextBlock(games.ToString(), StyleDefinition.styleTextSecondary);
            TextBlock tbGoals = ViewUtils.CreateTextBlock(goals.ToString(), StyleDefinition.styleTextSecondary);
            ViewUtils.AddElementToGrid(gridPlayerHistory, tbYear, gridPlayerHistory.RowDefinitions.Count - 1, 0);
            ViewUtils.AddElementToGrid(gridPlayerHistory, tbName, gridPlayerHistory.RowDefinitions.Count - 1, 1);
            ViewUtils.AddElementToGrid(gridPlayerHistory, tbGames, gridPlayerHistory.RowDefinitions.Count - 1, 2);
            ViewUtils.AddElementToGrid(gridPlayerHistory, tbGoals, gridPlayerHistory.RowDefinitions.Count - 1, 3);
        }

        private void InitializePlayerHistory()
        {

            int cumulativeGoals = 0;
            int cumulativeMatchesPlayed = 0;
            int totalGoals = 0;
            int totalMatchsPlayed = 0;

            Dictionary<NationalTeam, int[]> nationalTeamHistory = new Dictionary<NationalTeam, int[]>();

            if (player.history.Count > 0)
            {
                Club last = player.history[0].Club;
                int arrival = player.history[0].Year;
                for (int i = 0; i < player.history.Count + 1; i++)
                {
                    PlayerHistory hj = i < player.history.Count ? player.history[i] : new PlayerHistory(0, -1, new Dictionary<Club, int>(), new Dictionary<Club, int>(), null);
                    totalGoals += hj.Goals.Sum(k => k.Value);
                    totalMatchsPlayed += hj.GamesPlayed.Sum(k => k.Value);

                    if (i == player.history.Count || last != hj.Club)
                    {
                        string nameClub = last == null ? FindResource("str_free").ToString() : last.name;
                        int depart = hj.Year;
                        AddHistoryItem(arrival - 1, last != player.Club ? hj.Year : -1, nameClub, cumulativeMatchesPlayed, cumulativeGoals);
                        cumulativeGoals = 0;
                        cumulativeMatchesPlayed = 0;
                        arrival = hj.Year;
                    }
                    if (i < player.history.Count)
                    {
                        foreach (KeyValuePair<Club, int> kvp in hj.Goals)
                        {
                            NationalTeam nt = kvp.Key as NationalTeam;
                            if (nt != null)
                            {
                                if (!nationalTeamHistory.ContainsKey(nt))
                                {
                                    nationalTeamHistory.Add(nt, new int[4]);
                                }
                                nationalTeamHistory[nt][3] += kvp.Value;
                            }
                            else
                            {
                                cumulativeGoals += kvp.Value;
                            }
                        }
                        foreach (KeyValuePair<Club, int> kvp in hj.GamesPlayed)
                        {
                            NationalTeam nt = kvp.Key as NationalTeam;
                            if (nt != null)
                            {
                                if (!nationalTeamHistory.ContainsKey(nt))
                                {
                                    nationalTeamHistory.Add(nt, new[] { -1, -1, 0, 0 });
                                }
                                nationalTeamHistory[nt][2] += kvp.Value;
                                nationalTeamHistory[nt][1] = nationalTeamHistory[nt][1] == -1 || hj.Year > nationalTeamHistory[nt][1] ? hj.Year : nationalTeamHistory[nt][1];
                                nationalTeamHistory[nt][0] = nationalTeamHistory[nt][0] == -1 || hj.Year < nationalTeamHistory[nt][0] ? hj.Year : nationalTeamHistory[nt][0];
                            }
                            else
                            {
                                cumulativeMatchesPlayed += kvp.Value;
                            }
                        }

                        last = hj.Club;
                    }
                }

                foreach (KeyValuePair<NationalTeam, int[]> kvp in nationalTeamHistory)
                {
                    AddHistoryItem(kvp.Value[0] - 1, player.IsRetired ? kvp.Value[1] : -1, kvp.Key.name, kvp.Value[2], kvp.Value[3]);
                }

                AddHistoryItem(player.history[0].Year - 1, player.IsRetired ? player.history[player.history.Count - 1].Year : -1, FindResource("str_total").ToString(), totalMatchsPlayed, totalGoals);
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
