using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheManager;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI.ViewMisc
{
    public class ViewMatches : View
    {

        private StackPanel panel;

        private List<Match> matches;

        private bool showHour;
        private bool showDateSeparated;
        private bool showAttendance;
        private bool showOdds;
        private bool showTournament;
        private bool showDate;
        private float fontSize;
        private bool colorizeResult;
        private Club club;

        public ViewMatches(List<Match> matches, bool showDate, bool showHour, bool showDateSeparated, bool showAttendance, bool showOdds, bool showTournament, float fontSize = 12, bool colorizeResult = false, Club club = null)
        {
            this.matches = matches;
            this.showDate = showDate;
            this.showAttendance = showAttendance;
            this.showDateSeparated = showDateSeparated;
            this.showHour = showHour;
            this.showOdds = showOdds;
            this.showTournament = showTournament;
            this.fontSize = fontSize;
            this.colorizeResult = colorizeResult;
            this.club = club;
        }

        public override void Full(StackPanel spRanking)
        {
            panel = spRanking;
            panel.Children.Clear();

            DateTime lastTime = new DateTime(2000, 1, 1);
            foreach (Match match in matches)
            {

                if (showDateSeparated && lastTime != match.day.Date)
                {
                    if(showDate)
                    {
                        StackPanel spDate = new StackPanel();
                        spDate.Orientation = Orientation.Horizontal;
                        spDate.Children.Add(ViewUtils.CreateLabel(match.day.Date.ToLongDateString(), "StyleLabel2", fontSize, 100));
                        panel.Children.Add(spDate);
                    }
                }
                lastTime = match.day.Date;



                StackPanel spLine = new StackPanel();
                spLine.Orientation = Orientation.Horizontal;

                if(!showDateSeparated && showDate)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.day.ToString("dd/MM"), "StyleLabel2", fontSize * 0.9, 40));
                }
                if (showHour)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.day.ToShortTimeString(), "StyleLabel2", fontSize * 0.9, 35));
                }
                if(showTournament)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.Tournament.shortName, "StyleLabel2", fontSize, 30, new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 15, 15)), new SolidColorBrush(System.Windows.Media.Color.FromRgb(match.Tournament.color.red, match.Tournament.color.green, match.Tournament.color.blue))));  
                }
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(match.home, OpenClub, match.home.shortName, "StyleLabel2", fontSize * 0.85, 70));
                spLine.Children.Add(ViewUtils.CreateLogo(match.home, 20, 20));
                Label labelScore = ViewUtils.CreateLabelOpenWindow<Match>(match, OpenMatch, match.ScoreToString(), "StyleLabel2Center", fontSize, 85);
                string fontColor = "defaiteColor";
                if (colorizeResult)
                {

                    if( (club == match.home && match.score1 > match.score2) || (club == match.away && match.score1 < match.score2))
                    {
                        fontColor = "victoireColor";
                    }
                    else if(match.score1 == match.score2)
                    {
                        fontColor = "nulColor";
                    }
                    SolidColorBrush color = Application.Current.TryFindResource(fontColor) as SolidColorBrush;
                    labelScore.Background = color;
                }


                spLine.Children.Add(labelScore);
                spLine.Children.Add(ViewUtils.CreateLogo(match.away, 20, 20));
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(match.away, OpenClub, match.away.shortName, "StyleLabel2", fontSize * 0.85, 70));

                if(showAttendance)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.attendance.ToString(), "StyleLabel2", fontSize * 0.8, 40));
                }

                if (showOdds)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.odd1.ToString("0.00"), "StyleLabel2", fontSize * 0.75, 30));
                    spLine.Children.Add(ViewUtils.CreateLabel(match.oddD.ToString("0.00"), "StyleLabel2", fontSize * 0.75, 30));
                    spLine.Children.Add(ViewUtils.CreateLabel(match.odd2.ToString("0.00"), "StyleLabel2", fontSize * 0.75, 30));
                }

                panel.Children.Add(spLine);

            }
        }

        public override void Show()
        {
            throw new NotImplementedException();
        }
    }
}
