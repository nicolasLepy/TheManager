using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Options.xaml
    /// </summary>
    public partial class Windows_Options : Window
    {

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public List<CheckBox> _checkbox;


        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {

            Button btn = sender as Button;
            int themeId = int.Parse(btn.Name.Split('_')[1]);
            Theme t = Theme.themes[themeId];
            System.Windows.Media.Color backgroundColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.backgroundColor);
            System.Windows.Media.Color mainColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.mainColor);
            System.Windows.Media.Color secondaryColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.secondaryColor);
            System.Windows.Media.Color promotionColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.promotionColor);
            System.Windows.Media.Color upperPlayOffColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.upperPlayOffColor);
            System.Windows.Media.Color bottomPlayOffColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.bottomPlayOffColor);
            System.Windows.Media.Color relegationColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.relegationColor);
            Application.Current.Resources["BackgroundColor"] = backgroundColor;
            Application.Current.Resources["Color1"] = mainColor;
            Application.Current.Resources["Color2"] = secondaryColor;
            Application.Current.Resources["Promotion"] = promotionColor;
            Application.Current.Resources["UpperPlayOff"] = upperPlayOffColor;
            Application.Current.Resources["LowerPlayOff"] = bottomPlayOffColor;
            Application.Current.Resources["Relegation"] = relegationColor;
            Application.Current.Resources["Font"] = new FontFamily(t.fontFamily);

        }

        private void ListThemes()
        {
            int i = 0;
            foreach(Theme t in Theme.themes)
            {
                Button btnTheme = new Button();
                btnTheme.Name = "btnTheme_" + i;
                btnTheme.Click += new RoutedEventHandler(BtnTheme_Click);
                btnTheme.Content = t.name;
                btnTheme.Style = Application.Current.FindResource("StyleButton1") as Style;
                btnTheme.FontSize = 16;
                btnTheme.Width = 100;
                spThemesList.Children.Add(btnTheme);
                i++;


            }
        }

        public Windows_Options()
        {
            InitializeComponent();
            ListThemes();
            _checkbox = new List<CheckBox>();
            cbExporter.IsChecked = Session.Instance.Game.options.ExportEnabled;
            cbTransferts.IsChecked = Session.Instance.Game.options.transfersEnabled;
            cbSimuler.IsChecked = Session.Instance.Game.options.simulateGames;

            foreach(Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                CheckBox cb = new CheckBox();
                cb.IsChecked = Session.Instance.Game.options.tournamentsToExport.Contains(c);
                cb.Content = c.name;
                cb.Style = FindResource("StyleCheckBox") as Style;
                spOptions.Children.Add(cb);
                _checkbox.Add(cb);
            }

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
            SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            //modifying any series values will also animate and update the chart
            SeriesCollection[3].Values.Add(5d);

            DataContext = this;

        }

        private void CbExporter_Click(object sender, RoutedEventArgs e)
        {
            //Session.Instance.Partie.Options.Exporter = (bool)cbExporter.IsChecked;
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnValider_Click(object sender, RoutedEventArgs e)
        {
            Session.Instance.Game.options.tournamentsToExport.Clear();
            foreach(CheckBox cb in _checkbox)
            {
                Tournament comp = Session.Instance.Game.kernel.String2Tournament(cb.Content.ToString());
                if (cb.IsChecked == true)
                {
                    Session.Instance.Game.options.tournamentsToExport.Add(comp);
                }
            }
            Session.Instance.Game.options.ExportEnabled = (bool)cbExporter.IsChecked;
            Session.Instance.Game.options.transfersEnabled = (bool)cbTransferts.IsChecked;
            Session.Instance.Game.options.simulateGames = (bool)cbSimuler.IsChecked;

            Close();
        }

        private void CbTransferts_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CbSimuler_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
