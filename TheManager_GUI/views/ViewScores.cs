using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheManager;
using TheManager_GUI.Styles;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI.views
{

    public class ViewScores : View
    {

        private StackPanel panel;

        private readonly List<Match> matches;

        private readonly List<GridColumnDefinition> columns;
        private readonly bool showDateSeparated;
        private readonly float fontSize;
        private readonly float logoSize;
        private readonly bool colorizeResult;
        private readonly bool showHourSeparated;
        private readonly bool beautifyScore;
        private readonly float widthMultiplier;
        private readonly bool showTournamentSeparated;
        private readonly Club club;

        public ViewScores(List<Match> matches, bool showDate, bool showHour, bool showDateSeparated, bool showAttendance, bool showOdds, bool showTournament, float fontSize = 12, bool colorizeResult = false, Club club = null, bool showHourSeparated = false, bool showHalfTimeScore = false, bool beautifyScore = false, bool showTournamentSeparated = false, float widthMultiplier = 1.0f)
        {
            this.columns = new List<GridColumnDefinition>();
            this.matches = matches;
            columns.Add(new GridColumnDefinition(GridColumn.HOME_TEAM, 75, 4));
            columns.Add(new GridColumnDefinition(GridColumn.HOME_LOGO, 20, 5));
            columns.Add(new GridColumnDefinition(GridColumn.AWAY_TEAM, 75, 8));
            columns.Add(new GridColumnDefinition(GridColumn.AWAY_LOGO, 20, 7));
            columns.Add(new GridColumnDefinition(GridColumn.SCORE, 20, 6));
            if (showDate)
            {
                columns.Add(new GridColumnDefinition(GridColumn.DATE, 15, 1));
            }
            if (showAttendance)
            {
                columns.Add(new GridColumnDefinition(GridColumn.ATTENDANCE, 10, 9));
            }
            if (showHour)
            {
                columns.Add(new GridColumnDefinition(GridColumn.HOUR, 15, 2));
            }
            if (showOdds)
            {
                columns.Add(new GridColumnDefinition(GridColumn.ODDS, 30, 11));
            }
            if (showTournament)
            {
                columns.Add(new GridColumnDefinition(GridColumn.TOURNAMENT, 20, 3));
            }
            if (showHalfTimeScore)
            {
                columns.Add(new GridColumnDefinition(GridColumn.HALFTIME_SCORE, 15, 10));
            }
            columns.Sort(new GridColumnComparator());
            this.showDateSeparated = showDateSeparated;
            this.fontSize = fontSize;
            this.logoSize = fontSize * 5/3;
            this.colorizeResult = colorizeResult;
            this.showHourSeparated = showHourSeparated;
            this.beautifyScore = beautifyScore;
            this.widthMultiplier = widthMultiplier;
            this.showTournamentSeparated = showTournamentSeparated;
            this.club = club;
        }

        private void FillHomeTeam(Grid grid, Match match, int row, int col)
        {
            string name = match.home.shortName;
            if (!match.Round.Tournament.isChampionship && !match.Round.Tournament.IsInternational())
            {
                name += match.home.Championship != null ? String.Format(" ({0})", match.home.Championship.shortName) : "";
            }

            TextBlock tbClub = ViewUtils.CreateTextBlockOpenWindow<Club>(match.home, OpenClub, name, StyleDefinition.styleTextPlain, fontSize * 0.85, -1);
            tbClub.HorizontalAlignment = HorizontalAlignment.Right;
            tbClub.Padding = new Thickness(0, 0, 5, 0);
            grid.Children.Add(tbClub);
            Grid.SetRow(tbClub, row);
            Grid.SetColumn(tbClub, col);
        }

        private void FillAwayTeam(Grid grid, Match match, int row, int col)
        {
            string name = match.away.shortName;
            if (!match.Round.Tournament.isChampionship && !match.Round.Tournament.IsInternational())
            {
                name = match.away.Championship != null ? String.Format("({0}) {1}", match.home.Championship.shortName, name) : name;
            }

            TextBlock tbClub = ViewUtils.CreateTextBlockOpenWindow<Club>(match.away, OpenClub, name, StyleDefinition.styleTextPlain, fontSize * 0.85, -1);
            tbClub.HorizontalAlignment = HorizontalAlignment.Left;
            tbClub.Padding = new Thickness(5, 0, 0, 0);
            grid.Children.Add(tbClub);
            Grid.SetRow(tbClub, row);
            Grid.SetColumn(tbClub, col);
        }

        private void FillHomeLogo(Grid grid, Match match, int row, int col)
        {
            Image logo = match.Tournament.IsInternational() ? ViewUtils.CreateFlag(match.home.Country(), logoSize, logoSize) : ViewUtils.CreateLogo(match.home, logoSize, logoSize);
            logo.HorizontalAlignment = HorizontalAlignment.Center;
            grid.Children.Add(logo);
            Grid.SetRow(logo, row);
            Grid.SetColumn(logo, col);
        }

        private void FillAwayLogo(Grid grid, Match match, int row, int col)
        {
            Image logo = match.Tournament.IsInternational() ? ViewUtils.CreateFlag(match.away.Country(), logoSize, logoSize) : ViewUtils.CreateLogo(match.away, logoSize, logoSize);
            logo.HorizontalAlignment = HorizontalAlignment.Center;
            grid.Children.Add(logo);
            Grid.SetRow(logo, row);
            Grid.SetColumn(logo, col);
        }

        private void FillScore(Grid grid, Match match, int row, int col)
        {
            StackPanel spPanelScore = new StackPanel();
            spPanelScore.HorizontalAlignment = HorizontalAlignment.Center;
            spPanelScore.Orientation = Orientation.Horizontal;
            if (!beautifyScore)
            {
                StackPanel spScoreBlock = new StackPanel();
                spScoreBlock.Orientation = Orientation.Vertical;
                spScoreBlock.VerticalAlignment = VerticalAlignment.Center;
                TextBlock tbScore = ViewUtils.CreateTextBlockOpenWindow<Match>(match, OpenMatch, match.ScoreToString(false), StyleDefinition.styleTextPlain, fontSize, -1, null, true);
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
                    tbScore.Background = color;
                }

                spScoreBlock.Children.Add(tbScore);
                if(match.PenaltyShootout)
                {
                    TextBlock tbPenalties = ViewUtils.CreateTextBlock(match.PenaltyShootoutToString(), StyleDefinition.styleTextPlainCenter, fontSize*0.5);
                    spScoreBlock.Children.Add(tbPenalties);
                }
                spPanelScore.Children.Add(spScoreBlock);
            }
            else
            {
                Border borderScore1 = new Border();
                borderScore1.Style = Application.Current.FindResource("StyleBorderCalendar") as Style;
                borderScore1.Height = fontSize * 2.25;
                borderScore1.Width = fontSize * 2.25;
                borderScore1.CornerRadius = new CornerRadius(3);
                borderScore1.Child = ViewUtils.CreateTextBlockOpenWindow<Match>(match, OpenMatch, match.score1.ToString(), "StyleLabel2Center", fontSize, -1);
                borderScore1.Margin = new Thickness(fontSize / 2, 0, 0, 0);
                Border borderScore2 = new Border();
                borderScore2.Style = Application.Current.FindResource("StyleBorderCalendar") as Style;
                borderScore2.Height = fontSize * 2.25;
                borderScore2.Width = fontSize * 2.25;
                borderScore2.CornerRadius = new CornerRadius(3);
                borderScore2.Child = ViewUtils.CreateTextBlockOpenWindow<Match>(match, OpenMatch, match.score2.ToString(), "StyleLabel2Center", fontSize, -1);
                borderScore2.Margin = new Thickness(0, 0, fontSize / 2, 0);
                spPanelScore.Children.Add(borderScore1);
                spPanelScore.Children.Add(borderScore2);
            }

            grid.Children.Add(spPanelScore);
            Grid.SetRow(spPanelScore, row);
            Grid.SetColumn(spPanelScore, col);
        }

        private void FillDate(Grid grid, Match match, int row, int col)
        {
            TextBlock tbDate = ViewUtils.CreateTextBlock(match.day.ToString("dd/MM"), StyleDefinition.styleTextPlain, fontSize * 0.9, -1);
            grid.Children.Add(tbDate);
            Grid.SetRow(tbDate, row);
            Grid.SetColumn(tbDate, col);
        }

        private void FillAttendance(Grid grid, Match match, int row, int col)
        {
            TextBlock tbAttendance = ViewUtils.CreateTextBlock(match.attendance.ToString(), StyleDefinition.styleTextPlain, fontSize * 0.8, -1);
            grid.Children.Add(tbAttendance);
            Grid.SetRow(tbAttendance, row);
            Grid.SetColumn(tbAttendance, col);
        }

        private void FillHour(Grid grid, Match match, int row, int col)
        {
            TextBlock tbHour = ViewUtils.CreateTextBlock(match.day.ToShortTimeString(), StyleDefinition.styleTextPlain, fontSize * 0.9, -1);
            grid.Children.Add(tbHour);
            Grid.SetRow(tbHour, row);
            Grid.SetColumn(tbHour, col);
        }

        private Border CreateOddView(float value)
        {
            Border border = new Border();
            border.CornerRadius = new CornerRadius(1);
            border.BorderThickness = new Thickness(1);
            //border.HorizontalAlignment = HorizontalAlignment.Center;
            border.Margin = new Thickness(2);
            border.Padding = new Thickness(2);
            border.BorderBrush = Application.Current.FindResource(StyleDefinition.solidColorBrushColorBorderLight) as SolidColorBrush;
            TextBlock tbChild = ViewUtils.CreateTextBlock(value.ToString("0.00"), StyleDefinition.styleTextPlain, fontSize * 0.55);
            tbChild.HorizontalAlignment = HorizontalAlignment.Center;
            border.Child = tbChild;
            return border;
        }

        private void FillOdds(Grid grid, Match match, int row, int col)
        {
            Grid oddsGrid = new Grid();
            oddsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            oddsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            oddsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            Border borderOdd1 = CreateOddView(match.odd1);
            Border borderOddD = CreateOddView(match.oddD);
            Border borderOdd2 = CreateOddView(match.odd2);
            oddsGrid.Children.Add(borderOdd1);
            oddsGrid.Children.Add(borderOddD);
            oddsGrid.Children.Add(borderOdd2);
            Grid.SetColumn(borderOdd1, 0);
            Grid.SetColumn(borderOddD, 1);
            Grid.SetColumn(borderOdd2, 2);

            grid.Children.Add(oddsGrid);
            Grid.SetRow(oddsGrid, row);
            Grid.SetColumn(oddsGrid, col);
        }

        private void FillTournament(Grid grid, Match match, int row, int col)
        {
            TextBlock tbTournament = ViewUtils.CreateTextBlock(match.Tournament.shortName, StyleDefinition.styleTextPlain, fontSize, -1, new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 15, 15)), new SolidColorBrush(System.Windows.Media.Color.FromRgb(match.Tournament.color.red, match.Tournament.color.green, match.Tournament.color.blue)));
            grid.Children.Add(tbTournament);
            Grid.SetRow(tbTournament, row);
            Grid.SetColumn(tbTournament, col);
        }

        private void FillHalfTimeScore(Grid grid, Match match, int row, int col)
        {
            TextBlock tbHalfTimeScore = ViewUtils.CreateTextBlock("(" + match.ScoreHalfTime1 + "-" + match.ScoreHalfTime2 + ")", StyleDefinition.styleTextPlain, fontSize * 0.7, -1);
            grid.Children.Add(tbHalfTimeScore);
            Grid.SetRow(tbHalfTimeScore, row);
            Grid.SetColumn(tbHalfTimeScore, col);
        }

        private void FillDateSeparated(Grid grid, Match match, int row, int col)
        {
            CultureInfo ci = new CultureInfo("en-EN");
            TextBlock tbDate = ViewUtils.CreateTextBlock(match.day.Date.ToString("dddd dd MMMM yyyy", ci), StyleDefinition.styleTextPlain, fontSize, -1);
            grid.Children.Add(tbDate);
            Grid.SetRow(tbDate, row);
            Grid.SetColumn(tbDate, col);
            Grid.SetColumnSpan(tbDate, columns.Count);
        }

        private void FillHourSeparated(Grid grid, Match match, int row, int col)
        {
            TextBlock tbHour = ViewUtils.CreateTextBlock(match.day.ToShortTimeString(), StyleDefinition.styleTextPlain, fontSize * 0.7, -1);
            grid.Children.Add(tbHour);
            Grid.SetRow(tbHour, row);
            Grid.SetColumn(tbHour, col);
            Grid.SetColumnSpan(tbHour, columns.Count);
        }

        private void FillTournamentSeparated(Grid grid, Match match, int row, int col)
        {
            StackPanel spTournament = new StackPanel();
            spTournament.Orientation = Orientation.Horizontal;
            Country tournamentCountry = Session.Instance.Game.kernel.LocalisationTournament(match.Tournament) as Country;
            if (tournamentCountry != null)
            {
                spTournament.Children.Add(ViewUtils.CreateFlag(tournamentCountry, logoSize * 3/2, logoSize));
            }
            spTournament.Children.Add(ViewUtils.CreateTextBlock(match.Tournament.name, StyleDefinition.styleTextPlain, fontSize, -1));

            grid.Children.Add(spTournament);
            Grid.SetRow(spTournament, row);
            Grid.SetColumn(spTournament, col);
            Grid.SetColumnSpan(spTournament, columns.Count);

        }

        public override void Full(StackPanel spRanking)
        {
            panel = spRanking;
            panel.Children.Clear();

            Grid grid = new Grid();
            for(int i = 0; i < columns.Count; i++)
            {
                GridColumnDefinition gColumn = columns[i];
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(gColumn.width, GridUnitType.Star) });
            }

            for (int i = 0; i < matches.Count; i++)
            {
                //grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(logoSize * 1.25, GridUnitType.Pixel) });
            }

            DateTime lastTime = new DateTime(2000, 1, 1);
            Tournament currentTournament = null;

            int row = 0;
            foreach (Match match in matches)
            {
                if (showDateSeparated && lastTime != match.day.Date) 
                {
                    //grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(logoSize * 1.25, GridUnitType.Pixel) });
                    FillDateSeparated(grid, match, row, 0);
                    row++;                    
                }
                lastTime = match.day.Date;

                if (showHourSeparated)
                {
                    //grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(logoSize * 1.25, GridUnitType.Pixel) });
                    FillHourSeparated(grid, match, row, 0);
                    row++;
                }

                if (showTournamentSeparated && currentTournament != match.Tournament)
                {
                    //grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(logoSize * 1.25, GridUnitType.Pixel) });
                    FillTournamentSeparated(grid, match, row, 0);
                    row++;
                }

                foreach(GridColumnDefinition gcd in columns)
                {
                    switch (gcd.columnType)
                    {
                        case GridColumn.HOME_TEAM:
                            FillHomeTeam(grid, match, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.AWAY_TEAM:
                            FillAwayTeam(grid, match, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.HOME_LOGO:
                            FillHomeLogo(grid, match, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.AWAY_LOGO:
                            FillAwayLogo(grid, match, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.SCORE:
                            FillScore(grid, match, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.HOUR:
                            if(!showHourSeparated)
                            {
                                FillHour(grid, match, row, columns.IndexOf(gcd));
                            }
                            break;
                        case GridColumn.ATTENDANCE:
                            FillAttendance(grid, match, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.ODDS:
                            FillOdds(grid, match, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.TOURNAMENT:
                            if(!showTournamentSeparated)
                            {
                                FillTournament(grid, match, row, columns.IndexOf(gcd));
                            }
                            break;
                        case GridColumn.DATE:
                            if(!showDateSeparated)
                            {
                                FillDate(grid, match, row, columns.IndexOf(gcd));
                            }
                            break;
                        case GridColumn.HALFTIME_SCORE:
                            FillHalfTimeScore(grid, match, row, columns.IndexOf(gcd));
                            break;
                    }
                }
                row++;

                currentTournament = match.Tournament;

            }
            panel.Children.Add(grid);
        }

    }
}
