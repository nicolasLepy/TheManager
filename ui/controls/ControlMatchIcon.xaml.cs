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
using System.Windows.Navigation;
using TheManager_GUI;
using tm;

namespace ui.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlMatchIcon.xaml
    /// </summary>
    public partial class ControlMatchIcon : UserControl
    {

        private Brush defaultForeground;


        public ControlMatchIcon()
        {
            InitializeComponent();
            defaultForeground = textScore.Foreground;
        }

        private void Refresh(string home, string away, int scoreHome, int scoreAway, BitmapImage imgHome, BitmapImage imgAway, bool isFinished)
        {
            textHome.Text = home;
            textAway.Text = away;
            textScore.Text = String.Format("{0}-{1}", scoreHome, scoreAway);
            imageHome.Source = imgHome;
            imageAway.Source = imgAway;
            borderScore.Background = isFinished ? TryFindResource("colorNegative") as SolidColorBrush : Brushes.Transparent;
        }

        public string ClubName(Club club)
        {
            //return club.shortName.Length > 5 ? club.shortName.ToUpper().Substring(0, 5) : club.shortName.ToUpper();
            return club.abbr();
        }

        public void Update(Match match, bool isFinished)
        {
            BitmapImage imageHome = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.home)));
            BitmapImage imageAway = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.away)));

            Refresh(ClubName(match.home), ClubName(match.away), match.score1, match.score2, imageHome, imageAway, isFinished);
        }

        public async void TriggerGoalEvent()
        {
            textScore.Foreground = FindResource("colorButtonOver") as SolidColorBrush;
            await Task.Delay(2000);
            textScore.Foreground = defaultForeground;
        }
    }
}
