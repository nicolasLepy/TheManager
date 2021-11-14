using LiveCharts;
using LiveCharts.Wpf;
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
    /// Logique d'interaction pour GlobalWindow.xaml
    /// </summary>
    public partial class GlobalWindow : Window
    {
        private Func<double, string> YFormatter { get; set; }


        public GlobalWindow()
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));

            YFormatter = value => value.ToString("C");

            string[] years = new string[Session.Instance.Game.gameUniverse.AverageClubLevelInGame.Count];
            int year = 2021;
            int i = 0;
            foreach (float f in Session.Instance.Game.gameUniverse.AverageClubLevelInGame)
            {
                years[i++] = year.ToString();
                year++;
            }

            ChartValues<float> averageClubLevelInGame = new ChartValues<float>(Session.Instance.Game.gameUniverse.AverageClubLevelInGame);
            CreateChart(years, "Average Club Level In Game", averageClubLevelInGame, false, "Niveau", 0, 100, "Années");

            ChartValues<float> averageGoals = new ChartValues<float>(Session.Instance.Game.gameUniverse.AverageGoals);
            CreateChart(years, "Average goals by game", averageGoals, false, "Buts", 0, double.NaN, "Années");

            ChartValues<float> averagePlayerLevel = new ChartValues<float>(Session.Instance.Game.gameUniverse.AveragePlayerLevelInGame);
            CreateChart(years, "Average Player Level In Game", averagePlayerLevel, false, "Niveau", 0, 100, "Années");

            ChartValues<int> playersInGame = new ChartValues<int>(Session.Instance.Game.gameUniverse.PlayersInGame);
            CreateChart(years, "Players in game", playersInGame, false, "Total", 0, double.NaN, "Années");

            ChartValues<float> indebtesClubs = new ChartValues<float>(Session.Instance.Game.gameUniverse.RateIndebtesClubs);
            CreateChart(years, "Rate of indebtes clubs", indebtesClubs, false, "Taux", 0, 1, "Années");

            ChartValues<int> totalBugetInGame = new ChartValues<int>(Session.Instance.Game.gameUniverse.TotalBudgetInGame);
            CreateChart(years, "Total money in game", totalBugetInGame, true, "Argent", double.NaN, double.NaN, "Années");


        }

        private void CreateChart(string[] years, string title, IChartValues values, bool isMoney, string axisYtitle, double minValue, double maxValue, string axisXtitle)
        {

            Label labelTitle = ViewUtils.CreateLabel(title, "StyleLabel2Center", 18, -1);

            spMain.Children.Add(labelTitle);

            SeriesCollection averageClubLevelInGameCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = title,
                    Values = values,
                }
            };

            CartesianChart cc = new CartesianChart();
            cc.Width = 800;
            cc.Height = 375;

            cc.Series = averageClubLevelInGameCollection;

            Axis axisY = new Axis();
            axisY.Title = axisYtitle;
            axisY.MinValue = minValue;
            axisY.MaxValue = maxValue;

            if (isMoney)
            {
                axisY.LabelFormatter = YFormatter;
            }


            Axis axisX = new Axis();
            axisX.Title = axisXtitle;
            axisX.Labels = years;

            cc.AxisY.Add(axisY);
            cc.AxisX.Add(axisX);
            spMain.Children.Add(cc);

        }

        private void btnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
