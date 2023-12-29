using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using tm;
using TheManager_GUI.Styles;

namespace TheManager_GUI.pages
{

    public enum TournamentPageColumnType
    {
        Image,
        Text,
        Number
    }

    /// <summary>
    /// Logique d'interaction pour TournamentStatisticPage.xaml
    /// </summary>
    public partial class TournamentStatisticPage : Page
    {

        private readonly bool isRanked;
        private List<int> weights;
        private List<bool> cellLogo;

        public TournamentStatisticPage(bool isRanked)
        {
            this.isRanked = isRanked;
            InitializeComponent();
        }

        private int GetColumnIndex(int n)
        {
            int c = StaticColumnCount();
            for(int i = 0; i < n; i++)
            {
                c += weights == null ? 1 : weights[i];
            }
            return c;
        }

        private int StaticColumnCount()
        {
            return isRanked ? 3 : 2;
        }

        public void InitializeTable(List<string> headers, List<int> weights = null, List<bool> cellLogo = null)
        {
            this.cellLogo = cellLogo;
            this.weights = weights;
            grid.RowDefinitions.Clear();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(35, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Clear();
            if(isRanked)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.1, GridUnitType.Star) });
            }
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.8, GridUnitType.Star) });
            for(int i = 0; i < 10; i++)
            {
                double width = cellLogo == null ? 0.2 : i < cellLogo.Count && cellLogo[i] ? 0.05 : i > 0 && i < cellLogo.Count && cellLogo[i - 1] ? 0.35 : 0.2;
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(width, GridUnitType.Star) });
            }
            for(int i = 0; i < headers.Count; i++)
            {
                TextBlock textHeader = ViewUtils.CreateTextBlock(headers[i], StyleDefinition.styleTextPlainCenter);
                ViewUtils.AddElementToGrid(grid, textHeader, 0, GetColumnIndex(i), weights == null ? -1 : weights[i]);
            }
        }

        public void Full(List<KeyValuePair<Club, List<string>>> clubs)
        {
            int rank = 1;
            foreach(KeyValuePair<Club, List<string>> kvp in clubs)
            {
                Club club = kvp.Key;
                List<string> stats = kvp.Value;
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(35, GridUnitType.Pixel) });
                if(isRanked)
                {
                    ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(String.Format("{0}.", rank), StyleDefinition.styleTextPlain), grid.RowDefinitions.Count - 1, 0);
                }
                ViewUtils.AddElementToGrid(grid, ViewUtils.CreateLogo(club, 25, 25), grid.RowDefinitions.Count - 1, StaticColumnCount() - 2);
                ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(club.name, StyleDefinition.styleTextPlainCenter), grid.RowDefinitions.Count - 1, StaticColumnCount() - 1);
                for (int i = 0; i <stats.Count; i++)
                {
                    ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(stats[i], StyleDefinition.styleTextPlainCenter), grid.RowDefinitions.Count-1, GetColumnIndex(i), weights == null ? -1 : weights[i]);
                }
                rank++;
            }
        }

        public void Full(List<KeyValuePair<Player, List<string>>> players)
        {
            int rank = 1;
            foreach (KeyValuePair<Player, List<string>> kvp in players)
            {
                Player goalscorer = kvp.Key;
                List<string> stats = kvp.Value;
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(35, GridUnitType.Pixel) });
                if (isRanked)
                {
                    ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(String.Format("{0}.", rank), StyleDefinition.styleTextPlain), grid.RowDefinitions.Count - 1, 0);
                }
                if(goalscorer.Club != null)
                {
                    ViewUtils.AddElementToGrid(grid, ViewUtils.CreateLogo(goalscorer.Club, 25, 25), grid.RowDefinitions.Count - 1, StaticColumnCount() - 2);
                }
                else
                {
                    ViewUtils.AddElementToGrid(grid, ViewUtils.CreateFlag(goalscorer.nationality, 25, 18), grid.RowDefinitions.Count - 1, StaticColumnCount() - 2);
                }
                ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(goalscorer.Name, StyleDefinition.styleTextPlainCenter), grid.RowDefinitions.Count - 1, StaticColumnCount() - 1);
                for (int i = 0; i < stats.Count; i++)
                {
                    ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(stats[i], StyleDefinition.styleTextPlainCenter), grid.RowDefinitions.Count - 1, GetColumnIndex(i), weights == null ? -1 : weights[i]);
                }
                rank++;
            }
        }

        public void Full(List<KeyValuePair<string, List<string>>> seasons)
        {
            int rank = 1;
            foreach (KeyValuePair<string, List<string>> kvp in seasons)
            {
                string season = kvp.Key;
                List<string> stats = kvp.Value;
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(35, GridUnitType.Pixel) });
                if (isRanked)
                {
                    ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(String.Format("{0}.", rank), StyleDefinition.styleTextPlain), grid.RowDefinitions.Count - 1, 0);
                }

                ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(season, StyleDefinition.styleTextPlainCenter), grid.RowDefinitions.Count - 1, StaticColumnCount() - 1);
                for (int i = 0; i < stats.Count; i++)
                {
                    if(cellLogo != null && cellLogo[i])
                    {
                        ViewUtils.AddElementToGrid(grid, ViewUtils.CreateImage(new Uri(stats[i], UriKind.RelativeOrAbsolute), 25, 25), grid.RowDefinitions.Count - 1, GetColumnIndex(i), weights == null ? -1 : weights[i]);
                    }
                    else
                    {
                        ViewUtils.AddElementToGrid(grid, ViewUtils.CreateTextBlock(stats[i], StyleDefinition.styleTextPlainCenter), grid.RowDefinitions.Count - 1, GetColumnIndex(i), weights == null ? -1 : weights[i]);
                    }
                }
                rank++;
            }
        }

    }
}
