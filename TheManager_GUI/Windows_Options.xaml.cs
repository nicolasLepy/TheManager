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
    /// Logique d'interaction pour Windows_Options.xaml
    /// </summary>
    public partial class Windows_Options : Window
    {

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public List<CheckBox> _checkbox;

        public Windows_Options()
        {
           

            InitializeComponent();
            _checkbox = new List<CheckBox>();
            cbExporter.IsChecked = Session.Instance.Partie.Options.Exporter;
            cbTransferts.IsChecked = Session.Instance.Partie.Options.Transferts;
            cbSimuler.IsChecked = Session.Instance.Partie.Options.SimulerMatchs;

            foreach(Competition c in Session.Instance.Partie.Gestionnaire.Competitions)
            {
                CheckBox cb = new CheckBox();
                cb.IsChecked = Session.Instance.Partie.Options.CompetitionsAExporter.Contains(c);
                cb.Content = c.Nom;
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
            Session.Instance.Partie.Options.CompetitionsAExporter.Clear();
            foreach(CheckBox cb in _checkbox)
            {
                Competition comp = Session.Instance.Partie.Gestionnaire.String2Competition(cb.Content.ToString());
                if (cb.IsChecked == true)
                    Session.Instance.Partie.Options.CompetitionsAExporter.Add(comp);
            }
            Session.Instance.Partie.Options.Exporter = (bool)cbExporter.IsChecked;
            Session.Instance.Partie.Options.Transferts = (bool)cbTransferts.IsChecked;
            Session.Instance.Partie.Options.SimulerMatchs = (bool)cbSimuler.IsChecked;

            Close();
        }

        private void Rb_Theme1_Click(object sender, RoutedEventArgs e)
        {
            Color c1 = Colors.DarkBlue;
            c1.R = 28;
            c1.G = 116;
            c1.B = 53;
            Color c2 = Colors.Cyan;
            c2.R = 164;
            c2.G = 241;
            c2.B = 122;
            Application.Current.Resources["Color1"] = c1;
            Application.Current.Resources["Color2"] = c2;
            Application.Current.Resources["Font"] = new FontFamily("Arial Narrow");
        }

        private void Rb_Theme2_Click(object sender, RoutedEventArgs e)
        {
            Color c1 = Colors.DarkBlue;
            c1.R = 28;
            c1.G = 88;
            c1.B = 116;
            Color c2 = Colors.Cyan;
            c2.R = 121;
            c2.G = 237;
            c2.B = 240;
            Application.Current.Resources["Color1"] = c1;
            Application.Current.Resources["Color2"] = c2;
            Application.Current.Resources["Font"] = new FontFamily("Montserrat");
        }

        private void CbTransferts_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CbSimuler_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
