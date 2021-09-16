using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        public ViewMatches(List<Match> matches, bool showDate, bool showHour, bool showDateSeparated, bool showAttendance, bool showOdds, bool showTournament, float fontSize = 12)
        {
            this.matches = matches;
            this.showDate = showDate;
            this.showAttendance = showAttendance;
            this.showDateSeparated = showDateSeparated;
            this.showHour = showHour;
            this.showOdds = showOdds;
            this.showTournament = showTournament;
            this.fontSize = fontSize;
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

                if(showHour)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.day.ToShortTimeString(), "StyleLabel2", fontSize * 0.9, 35));
                }
                if(showTournament)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.Tournament.shortName, "StyleLabel2", fontSize, 30));   
                }
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(match.home, OpenClub, match.home.shortName, "StyleLabel2", fontSize, 70));
                spLine.Children.Add(ViewUtils.CreateLogo(match.home, 20, 20));
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<Match>(match, OpenMatch, match.Played ? match.score1 + "-" + match.score2 : "", "StyleLabel2Center", fontSize, 40));
                spLine.Children.Add(ViewUtils.CreateLogo(match.away, 20, 20));
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(match.away, OpenClub, match.away.shortName, "StyleLabel2", fontSize, 70));

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
