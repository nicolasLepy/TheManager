using LiveCharts;
using System;
using System.Collections;
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
using tm;
using TheManager_GUI.Styles;
using TheManager_GUI.views;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour GlobalView.xaml
    /// </summary>
    public partial class GlobalView : Window
    {

        private readonly int chartHeight = 475;

        public GlobalView()
        {
            InitializeComponent();
            Initialize(Session.Instance.Game);
        }
        
        private int Games(Tournament tournament)
        {
            int i = 0;
            foreach(Round round in tournament.rounds)
            {
                i += round.matches.Count;
            }
            return i;
        }

        private void Initialize(Game game)
        {

            List<string> years = new List<string>();
            int year = Utils.beginningYear;
            foreach (float f in Session.Instance.Game.gameUniverse.AverageClubLevelInGame)
            {
                years.Add(year.ToString());
                year++;
            }
            int totalGames = 0;
            foreach(Tournament tournament in Session.Instance.Game.kernel.Competitions)
            {
                totalGames += Games(tournament);
                foreach(KeyValuePair<int, Tournament> kvp in tournament.previousEditions)
                {
                    totalGames += Games(kvp.Value);
                }
            }

            textDate.Text = game.date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern);
            textSeasonsCount.Text = String.Format("{0} {1}", years.Count, FindResource("str_seasons").ToString().ToLower());
            textClubsCount.Text = String.Format("{0} {1}", Session.Instance.Game.kernel.Clubs.Count, FindResource("str_clubs").ToString().ToLower());
            textPlayers.Text = String.Format(FindResource("str_playersCount").ToString(), Session.Instance.Game.kernel.PlayersCount());
            textPastPlayers.Text = String.Format(FindResource("str_retiredPlayers").ToString(), Session.Instance.Game.kernel.retiredPlayersCount);
            textPlayedGames.Text = String.Format(FindResource("str_playedGames").ToString(), totalGames);

            //Formation centre
            string titleAverageLevel = "Average Club Level In Game";
            string titleAverageClubLevel = "Average Club Level In Game";
            string titleAveragePlayerLevel = "Average Player Level In Game";
            string titleAverageFormationCentre = "Average Formation Level In Game";
            ChartView chartAverageLevel = new ChartView(ChartType.LINE_CHART, titleAverageLevel, new List<string>() { titleAverageClubLevel, titleAveragePlayerLevel, titleAverageFormationCentre}, FindResource("str_level").ToString(), FindResource("str_years").ToString(), years, false, false, 1, new List<List<double>>() { Session.Instance.Game.gameUniverse.AverageClubLevelInGame.ConvertAll(x => (double)x), Session.Instance.Game.gameUniverse.AveragePlayerLevelInGame.ConvertAll(x => (double)x), Session.Instance.Game.gameUniverse.AverageFormationInGame.ConvertAll(x => (double)x) }, -1, chartHeight, 0, 100);
            chartAverageLevel.RenderChart(panelChartAverageLevel);

            ChartView chartAverageGoals = new ChartView(ChartType.LINE_CHART, "Average goals by game", new List<string>() { "Average goals by game" }, FindResource("str_goals").ToString(), FindResource("str_years").ToString(), years, false, false, 1, new List<List<double>>() { Session.Instance.Game.gameUniverse.AverageGoals.ConvertAll(x => (double)x) }, -1, chartHeight, 0, -1);
            chartAverageGoals.RenderChart(panelChartGoals);

            ChartView chartPlayersCount = new ChartView(ChartType.LINE_CHART, "Players in game", new List<string>() { "Players in game" }, FindResource("str_total").ToString(), FindResource("str_years").ToString(), years, false, false, 1, new List<List<double>>() { Session.Instance.Game.gameUniverse.PlayersInGame.ConvertAll(x => (double)x) }, -1, chartHeight, 0, -1);
            chartPlayersCount.RenderChart(panelChartPlayers);

            ChartView chartDebts = new ChartView(ChartType.LINE_CHART, "Rate of indebtes clubs", new List<string>() { "Rate of indebtes clubs" }, "Taux", FindResource("str_years").ToString(), years, false, true, 1, new List<List<double>>() { Session.Instance.Game.gameUniverse.RateIndebtesClubs.ConvertAll(x => (double)x) }, -1, chartHeight, 0, 1);
            chartDebts.RenderChart(panelChartDebts);

            ChartView chartTotalBudget = new ChartView(ChartType.LINE_CHART, "Total money in game", new List<string>() { "Total money in game" }, "Taux", FindResource("str_budget").ToString(), years, true, false, 1, new List<List<double>>() { Session.Instance.Game.gameUniverse.TotalBudgetInGame.ConvertAll(x => (double)x) }, -1, chartHeight);
            chartTotalBudget.RenderChart(panelChartBudgets);

            for(int i = 0; i < years.Count()-1; i++)
            {
                TabItem tabItem = new TabItem() { Header = years[i], Style = FindResource(StyleDefinition.tabItemStyle) as Style };
                tabItem.FontSize = (double)FindResource(StyleDefinition.fontSizeRegular);
                StackPanel hostHistogram = new StackPanel() { Orientation = Orientation.Vertical };
                tabItem.Content = hostHistogram;
                List<int> budgets = new List<int>();
                foreach (Club club in Session.Instance.Game.kernel.Clubs)
                {
                    CityClub cityClub = club as CityClub;
                    if(cityClub != null)
                    {
                        if(i < cityClub.history.elements.Count)
                        {
                            int budget = cityClub.history.elements[i].budget;
                            budgets.Add(budget);
                        }
                    }
                }
                ChartView chartHistogram = Histogram(budgets);
                chartHistogram.RenderChart(hostHistogram);
                tabControlBudgets.Items.Add(tabItem);
            }

        }

        private ChartView Histogram(List<int> values)
        {
            int binSize = 10000000;
            int binCount = 20;
            int[] bins = new int[binCount];
            List<string> labels = new List<string>();
            for(int i = 0; i< bins.Length; i++)
            {
                int min = binSize * i;
                labels.Add(String.Format("{0}", Utils.FormatMoney(min)));
            }
            Random r = new Random();
            for(int i = 0; i< bins.Length; i++)
            {
                bins[i] = 0;
            }
            foreach(int value in values)
            {
                int bin = value / binSize;
                bin = bin >= bins.Length ? bins.Length - 1 : bin;
                bin = bin < 0 ? 0 : bin;
                bins[bin]++;
            }
            ChartView chartHistogram = new ChartView(ChartType.BAR_CHART, FindResource("str_club_budgets").ToString(), new List<string>() { FindResource("str_budget").ToString() }, "Count", "Money", labels, false, false, 0.75f, new List<List<double>>() { bins.ToList().ConvertAll(x => (double)x) }, -1, chartHeight, 0, -1);
            return chartHistogram;

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
