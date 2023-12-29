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
using System.Windows.Navigation;
using System.Windows.Shapes;
using tm;
using TheManager_GUI.Styles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace TheManager_GUI.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlCalendarItem.xaml
    /// </summary>
    public partial class ControlCalendarItem : UserControl
    {

        public SolidColorBrush ColorBackground { get; set; }

        public ControlCalendarItem(DateTime date, bool highlight, Match match, List<Tournament> tournaments)
        {
            InitializeComponent();

            if (highlight)
            {
                ColorBackground = FindResource(StyleDefinition.solidColorBrushColorPanel3) as SolidColorBrush;
            }
            else
            {
                ColorBackground = FindResource(StyleDefinition.solidColorBrushColorPanel1) as SolidColorBrush;
            }

            tbDate.Text = date.ToString("dddd dd/MM");
            if (match != null)
            {
                imageMatchHome.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.home)));
                imageMatchAway.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.away)));
                string text = match.Played ? match.ScoreToString(true, true, Application.Current.FindResource("str_aet").ToString()) : match.Time.ToString();
                tbMatchInfo.Text = text;
            }
            else
            {
                imageMatchHome.Source = null;
                imageMatchAway.Source = null;
                tbMatchInfo.Text = "";
            }

            if (tournaments != null && tournaments.Count > 0)
            {
                foreach (Tournament tournament in tournaments)
                {
                    gridTournaments.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(15, GridUnitType.Pixel) });
                    TextBlock textTournamentName = ViewUtils.CreateTextBlock(tournament.name, StyleDefinition.styleTextPlain, 8);
                    textTournamentName.Margin = new Thickness(5, 0, 0, 0);
                    Image imageTournamentLogo = ViewUtils.CreateLogo(tournament, 13, 13);
                    ViewUtils.AddElementToGrid(gridTournaments, imageTournamentLogo, gridTournaments.RowDefinitions.Count - 1, 0);
                    ViewUtils.AddElementToGrid(gridTournaments, textTournamentName, gridTournaments.RowDefinitions.Count - 1, 1);
                }
            }

            DataContext = this;
        }
    }
}
