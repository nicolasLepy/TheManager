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
using TheManager_GUI;
using tm;

namespace TheManager_GUI.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlScoreboard.xaml
    /// </summary>
    public partial class ControlScoreboard : UserControl
    {

        public ControlScoreboard()
        {
            InitializeComponent();
        }

        private void Refresh(bool displayTime, string home, string away, string scoreHome, string scoreAway, TimeSpan time, int extraTime, BitmapImage imgHome, BitmapImage imgAway, string extraText)
        {
            tbHome.Text = home;
            tbAway.Text = away;
            tbScoreHome.Text = scoreHome;
            tbScoreAway.Text = scoreAway;
            UpdateTime(displayTime, time, extraTime);
            imageHome.Source = imgHome;
            imageAway.Source = imgAway;
            tbExtraText.Text = displayTime ? "" : extraText;

        }

        private void UpdateTime(bool displayTime, TimeSpan time, int extraTime)
        {
            borderTime.Visibility = displayTime ? Visibility.Visible : Visibility.Hidden;
            tbTime.Text = displayTime ? String.Format("{0:00}:{1:00}", time.TotalMinutes, time.Seconds) : "";
            tbExtraTime.Visibility = (displayTime && extraTime > 0) ? Visibility.Visible : Visibility.Hidden;
            tbExtraTime.Text = String.Format("+{0}", extraTime);
        }

        public string ClubName(Club club)
        {
            string name = club.name.ToUpper();
            if(name.Length > 10)
            {
                name = club.shortName.Length > 10 ? club.shortName.ToUpper().Substring(0, 10) : club.shortName.ToUpper();
            }
            return name;
        }

        public void Update(Match match, bool displayTime)
        {
            BitmapImage imageHome = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.home)));
            BitmapImage imageAway = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.away)));

            string extraText = match.prolongations ? FindResource("str_aet").ToString() : "";
            Refresh(displayTime, ClubName(match.home), ClubName(match.away), match.score1.ToString(), match.score2.ToString(), match.TimeSpan, 0, imageHome, imageAway, extraText);
        }
    }
}
