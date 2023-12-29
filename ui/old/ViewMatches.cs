using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using tm;
using TheManager_GUI.Views;

namespace TheManager_GUI.ViewMisc
{
    public class ViewMatches : View
    {

        private StackPanel panel;

        private readonly List<Match> matches;

        private readonly bool showHour;
        private readonly bool showDateSeparated;
        private readonly bool showAttendance;
        private readonly bool showOdds;
        private readonly bool showTournament;
        private readonly bool showDate;
        private readonly float fontSize;
        private readonly bool colorizeResult;
        private readonly bool showHourSeparated;
        private readonly bool showHalfTimeScore;
        private readonly bool beautifyScore;
        private readonly float widthMultiplier;
        private readonly bool showTournamentSeparated;
        private readonly Club club;

        public ViewMatches(List<Match> matches, bool showDate, bool showHour, bool showDateSeparated, bool showAttendance, bool showOdds, bool showTournament, float fontSize = 12, bool colorizeResult = false, Club club = null, bool showHourSeparated = false, bool showHalfTimeScore = false, bool beautifyScore = false, bool showTournamentSeparated = false, float widthMultiplier = 1.0f)
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
            this.showHourSeparated = showHourSeparated;
            this.showHalfTimeScore = showHalfTimeScore;
            this.beautifyScore = beautifyScore;
            this.widthMultiplier = widthMultiplier;
            this.showTournamentSeparated = showTournamentSeparated;
            this.club = club;
        }

        public override void Full(StackPanel spRanking)
        {
            float sizeMultiplier = fontSize / 12;
            panel = spRanking;
            panel.Children.Clear();

            DateTime lastTime = new DateTime(2000, 1, 1);
            Tournament currentTournament = null;

            foreach (Match match in matches)
            {

                if (showDateSeparated && lastTime != match.day.Date)
                {
                    if(showDate)
                    {
                        StackPanel spDate = new StackPanel();
                        spDate.Orientation = Orientation.Horizontal;
                        CultureInfo ci = new CultureInfo("en-EN");
                        spDate.Children.Add(ViewUtils.CreateLabel(match.day.Date.ToString("dddd dd MMMM yyyy", ci), "StyleLabel2", fontSize, -1));
                        panel.Children.Add(spDate);
                    }
                }
                lastTime = match.day.Date;

                if(showHourSeparated)
                {
                    StackPanel spHourLine = new StackPanel();
                    spHourLine.Orientation = Orientation.Horizontal;
                    spHourLine.HorizontalAlignment = HorizontalAlignment.Center;
                    spHourLine.Children.Add(ViewUtils.CreateLabel(match.day.ToShortTimeString(), "StyleLabel2Center", fontSize * 0.7, 50*sizeMultiplier));
                    panel.Children.Add(spHourLine);
                }

                if(showTournamentSeparated && currentTournament != match.Tournament)
                {
                    StackPanel spTournamentLine = new StackPanel();
                    spTournamentLine.Orientation = Orientation.Horizontal;
                    Country tournamentCountry = Session.Instance.Game.kernel.LocalisationTournament(match.Tournament) as Country;
                    if(tournamentCountry != null)
                    {
                        spTournamentLine.Children.Add(ViewUtils.CreateFlag(tournamentCountry, 30 * sizeMultiplier, 20 * sizeMultiplier));
                    }
                    spTournamentLine.Children.Add(ViewUtils.CreateLabel(match.Tournament.name, "StyleLabel2", fontSize, 175 * sizeMultiplier));
                    panel.Children.Add(spTournamentLine);
                }

                StackPanel spLine = new StackPanel();
                spLine.Orientation = Orientation.Horizontal;

                if(!showDateSeparated && showDate)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.day.ToString("dd/MM"), "StyleLabel2", fontSize * 0.9, 40 * sizeMultiplier));
                }
                if (showHour && !showHourSeparated)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.day.ToShortTimeString(), "StyleLabel2", fontSize * 0.9, 35 * sizeMultiplier));
                }
                if(showTournament && !showTournamentSeparated)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.Tournament.shortName, "StyleLabel2", fontSize, 30 * sizeMultiplier, new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 15, 15)), new SolidColorBrush(System.Windows.Media.Color.FromRgb(match.Tournament.color.red, match.Tournament.color.green, match.Tournament.color.blue))));  
                }

                if (!match.Round.Tournament.isChampionship)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.home.Championship != null ? match.home.Championship.shortName : "", "StyleLabel2Center", fontSize * 0.85, 20 * sizeMultiplier * widthMultiplier));
                }

                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(match.home, OpenClub, match.home.shortName, "StyleLabel2", fontSize * 0.85, 70 * sizeMultiplier * widthMultiplier));
                spLine.Children.Add(ViewUtils.CreateLogo(match.home, 20 * sizeMultiplier, 20 * sizeMultiplier));
                if(!beautifyScore)
                {
                    Label labelScore = ViewUtils.CreateLabelOpenWindow<Match>(match, OpenMatch, match.ScoreToString(true, true, Application.Current.FindResource("str_aet").ToString()), "StyleLabel2Center", fontSize, 85 * sizeMultiplier);
                    string fontColor = "defaiteColor";
                    if (colorizeResult)
                    {
                        if ((club == match.home && match.score1 > match.score2) || (club == match.away && match.score1 < match.score2))
                        {
                            fontColor = "victoireColor";
                        }
                        else if (match.score1 == match.score2)
                        {
                            fontColor = "nulColor";
                        }
                        SolidColorBrush color = Application.Current.TryFindResource(fontColor) as SolidColorBrush;
                        labelScore.Background = color;
                    }


                    spLine.Children.Add(labelScore);
                }
                else
                {
                    Border borderScore1 = new Border();
                    borderScore1.Style = Application.Current.FindResource("StyleBorderCalendar") as Style;
                    borderScore1.Height = fontSize * 2.25;
                    borderScore1.Width = fontSize * 2.25;
                    borderScore1.CornerRadius = new CornerRadius(3);
                    borderScore1.Child = ViewUtils.CreateLabelOpenWindow<Match>(match, OpenMatch, match.score1.ToString(), "StyleLabel2Center", fontSize, -1);
                    borderScore1.Margin = new Thickness(fontSize / 2, 0, 0, 0);
                    Border borderScore2 = new Border();
                    borderScore2.Style = Application.Current.FindResource("StyleBorderCalendar") as Style;
                    borderScore2.Height = fontSize * 2.25;
                    borderScore2.Width = fontSize * 2.25;
                    borderScore2.CornerRadius = new CornerRadius(3);
                    borderScore2.Child = ViewUtils.CreateLabelOpenWindow<Match>(match, OpenMatch, match.score2.ToString(), "StyleLabel2Center", fontSize, -1);
                    borderScore2.Margin = new Thickness(0, 0, fontSize / 2, 0);
                    spLine.Children.Add(borderScore1);
                    spLine.Children.Add(borderScore2);
                }

                spLine.Children.Add(ViewUtils.CreateLogo(match.away, 20 * sizeMultiplier, 20 * sizeMultiplier));
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(match.away, OpenClub, match.away.shortName, "StyleLabel2Right", fontSize * 0.85, 70 * sizeMultiplier * widthMultiplier));

                if (!match.Round.Tournament.isChampionship)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.away.Championship != null ? match.away.Championship.shortName : "", "StyleLabel2Center", fontSize * 0.85, 20 * sizeMultiplier * widthMultiplier));
                }


                if (showAttendance)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.attendance.ToString(), "StyleLabel2", fontSize * 0.8, 40 * sizeMultiplier));
                }

                if (showOdds)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(match.odd1.ToString("0.00"), "StyleLabel2", fontSize * 0.75, 30 * sizeMultiplier));
                    spLine.Children.Add(ViewUtils.CreateLabel(match.oddD.ToString("0.00"), "StyleLabel2", fontSize * 0.75, 30 * sizeMultiplier));
                    spLine.Children.Add(ViewUtils.CreateLabel(match.odd2.ToString("0.00"), "StyleLabel2", fontSize * 0.75, 30 * sizeMultiplier));
                }

                panel.Children.Add(spLine);

                if (showHalfTimeScore)
                {
                    StackPanel spHalfTimeLine = new StackPanel();
                    spHalfTimeLine.Orientation = Orientation.Horizontal;
                    spHalfTimeLine.HorizontalAlignment = HorizontalAlignment.Center;
                    spHalfTimeLine.Children.Add(ViewUtils.CreateLabel("(" + match.ScoreHalfTime1 + "-" + match.ScoreHalfTime2 + ")", "StyleLabel2Center", fontSize * 0.7, 50));
                    spHalfTimeLine.Margin = new Thickness(0, 0, 0, fontSize / 2);
                    panel.Children.Add(spHalfTimeLine);
                }

                currentTournament = match.Tournament;

            }
        }

    }
}
