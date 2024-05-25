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
using tm;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour GlobalWindow.xaml
    /// </summary>
    public partial class GlobalWindow : Window
    {
        //private Func<double, string> YFormatter { get; set; }


        public GlobalWindow()
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));

            Func<double, string> YFormatter = value => value.ToString("C");
            Func<double, string> YFormatterPercent = value => value.ToString("P");

            string[] years = new string[Session.Instance.Game.gameUniverse.AverageClubLevelInGame.Count];
            int year = 2021;
            int i = 0;
            foreach (float f in Session.Instance.Game.gameUniverse.AverageClubLevelInGame)
            {
                years[i++] = year.ToString();
                year++;
            }

            ChartValues<float> averageClubLevelInGame = new ChartValues<float>(Session.Instance.Game.gameUniverse.AverageClubLevelInGame);
            ViewUtils.CreateYearChart(spMain, years, "Average Club Level In Game", averageClubLevelInGame, false, false, "Niveau", 0, 100, "Années", YFormatter);

            ChartValues<float> averageGoals = new ChartValues<float>(Session.Instance.Game.gameUniverse.AverageGoals);
            ViewUtils.CreateYearChart(spMain, years, "Average goals by game", averageGoals, false, false, "Buts", 0, double.NaN, "Années", YFormatter);

            ChartValues<float> averagePlayerLevel = new ChartValues<float>(Session.Instance.Game.gameUniverse.AveragePlayerLevelInGame);
            ViewUtils.CreateYearChart(spMain, years, "Average Player Level In Game", averagePlayerLevel, false, false, "Niveau", 0, 100, "Années", YFormatter);

            ChartValues<int> playersInGame = new ChartValues<int>(Session.Instance.Game.gameUniverse.PlayersInGame);
            ViewUtils.CreateYearChart(spMain, years, "Players in game", playersInGame, false, false, "Total", 0, double.NaN, "Années", YFormatter);

            ChartValues<float> indebtesClubs = new ChartValues<float>(Session.Instance.Game.gameUniverse.RateIndebtesClubs);
            ViewUtils.CreateYearChart(spMain, years, "Rate of indebtes clubs", indebtesClubs, false, true, "Taux", 0, 1, "Années", YFormatterPercent);

            ChartValues<int> totalBugetInGame = new ChartValues<int>(Session.Instance.Game.gameUniverse.TotalBudgetInGame);
            ViewUtils.CreateYearChart(spMain, years, "Total money in game", totalBugetInGame, true, false, "Argent", double.NaN, double.NaN, "Années", YFormatter);


        }

        private void btnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
