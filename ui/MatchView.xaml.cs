using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheManager;
using TheManager_GUI.controls;
using TheManager_GUI.Styles;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour MatchView.xaml
    /// </summary>
    public partial class MatchView : Window
    {
        private Match match;
        private Tournament tournament;
        private double fontSize;

        public MatchView(Match match)
        {
            this.match = match;
            this.tournament = match.Tournament;
            fontSize = (double)Application.Current.FindResource(StyleDefinition.fontSizeRegular);
            InitializeComponent();
            Initialize();
            FillStats();
            FillMedias();
            FillEvents();
            FillCompositions();
            FillGameProgression(true);
            FillGameProgression(false);
        }

        private void FillStats()
        {
            ControlStatItem csiPossession = new ControlStatItem(FindResource("str_possession").ToString(), match.statistics.HomePossession * 100, match.statistics.AwayPossession * 100, true);
            ControlStatItem csiShots = new ControlStatItem(FindResource("str_shots").ToString(), match.statistics.HomeShoots, match.statistics.AwayShoots, false);
            spStats.Children.Add(csiPossession);
            spStats.Children.Add(csiShots);
        }

        private void FillEvents()
        {
            string strOg = FindResource("str_og").ToString();
            string strPen = FindResource("str_pen").ToString();

            foreach (MatchEvent em in match.events)
            {
                string icon = "";
                switch (em.type)
                {
                    case GameEvent.Goal:
                        icon = "goal.png";
                        break;
                    case GameEvent.PenaltyGoal:
                        icon = "goal.png";
                        break;
                    case GameEvent.AgGoal:
                        icon = "goal.png";
                        break;
                    case GameEvent.YellowCard:
                        icon = "yellow_card.png";
                        break;
                    case GameEvent.RedCard:
                        icon = "red_card.png";
                        break;
                }
                string playerName = String.Format("{0}{1}{2}", em.player.Name, em.type == GameEvent.PenaltyGoal ? " (" + strPen + ")" : "", em.type == GameEvent.AgGoal ?  " (" + strOg + ")" : "");
                string time = em.MinuteToString;
                string imageIconPath = Utils.Image(icon);

                if (em.type != GameEvent.Shot)
                {
                    int colPlayer = em.club == match.home ? 1 : 7;
                    int colTime = em.club == match.home ? 2 : 6;
                    int colIcon = em.club == match.home ? 3 : 5;
                    Image imageIcon = new Image();
                    imageIcon.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(imageIconPath));
                    imageIcon.Height = fontSize * 5 / 3;
                    TextBlock tbMinute = ViewUtils.CreateTextBlock(time, StyleDefinition.styleTextPlainCenter);
                    TextBlock tbPlayer = ViewUtils.CreateTextBlock(playerName, StyleDefinition.styleTextPlain);
                    if (em.club == match.home)
                    {
                        tbPlayer.HorizontalAlignment = HorizontalAlignment.Right;
                    }


                    gridEvents.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(fontSize * 2, GridUnitType.Pixel) });
                    ViewUtils.AddElementToGrid(gridEvents, imageIcon, gridEvents.RowDefinitions.Count - 1, colIcon);
                    ViewUtils.AddElementToGrid(gridEvents, tbMinute, gridEvents.RowDefinitions.Count - 1, colTime);
                    ViewUtils.AddElementToGrid(gridEvents, tbPlayer, gridEvents.RowDefinitions.Count - 1, colPlayer);
                }
            }
        }

        private UIElement CreatePlayerElement(Player player, Club club)
        {
            TextBlock tbPlayerName = ViewUtils.CreateTextBlock(player.Name, StyleDefinition.styleTextPlain);
            tbPlayerName.Margin = new Thickness(5, 0, 0, 0);
            bool wasSubstitued = false;
            StackPanel spPlayer = new StackPanel();
            spPlayer.Orientation = Orientation.Horizontal;
            if(club == match.away)
            {
                tbPlayerName.HorizontalAlignment = HorizontalAlignment.Right;
                spPlayer.HorizontalAlignment = HorizontalAlignment.Right;
            }
            foreach (Substitution substitution in match.substitutions)
            {
                if(substitution.PlayerIn == player || substitution.PlayerOut == player)
                {
                    wasSubstitued = true;
                    Image imageSub = new Image();
                    imageSub.Source = new BitmapImage(new Uri(Utils.Image("sub.png")));
                    imageSub.Height = fontSize * 5 / 3;
                    int minute = substitution.Period == 1 ? substitution.Minute : substitution.Period == 2 ? substitution.Minute + 45 : substitution.Period == 3 ? substitution.Minute + 90 : substitution.Minute + 105;
                    string subText = string.Format("{0}° {1} - {2}", substitution.Minute, substitution.PlayerOut, substitution.PlayerIn);
                    Border toolTip = new Border();
                    toolTip.Background = FindResource(StyleDefinition.solidColorBrushColorButtonOver) as SolidColorBrush;
                    toolTip.Child = ViewUtils.CreateTextBlock(subText, StyleDefinition.styleTextPlainCenter);
                    ToolTipService.SetInitialShowDelay(imageSub, 0);
                    imageSub.ToolTip = toolTip;
                    imageSub.Margin = new Thickness(5, 0, 5, 0);

                    spPlayer.Children.Add(imageSub);
                }
            }
            UIElement element = tbPlayerName;
            if (wasSubstitued)
            {
                if(club == match.home)
                {
                    spPlayer.Children.Insert(0, tbPlayerName);
                }
                else
                {
                    spPlayer.Children.Add(tbPlayerName);
                }
                element = spPlayer;
            }

            return element;
        }

        private void FillCompositions()
        {
            gridCompositions.Children.Clear();
            gridCompositions.RowDefinitions.Clear();
            int rowsCount = 11 + (match.Subs1.Count > 0 && match.Subs2.Count > 0 ? 1 : 0) + (match.Subs1.Count > match.Subs2.Count ? match.Subs1.Count : match.Subs2.Count);
            int defaultRowsCount = match.Subs1.Count > 0 && match.Subs2.Count > 0 ? 12 : 11;
            TextBlock tbSubs = ViewUtils.CreateTextBlock(FindResource("str_substitutes").ToString().ToUpper(), StyleDefinition.styleTextSecondary);
            tbSubs.HorizontalAlignment = HorizontalAlignment.Center;
            ViewUtils.AddElementToGrid(gridCompositions, tbSubs, 11, 1, 7);

            for (int i = 0; i < rowsCount; i++)
            {
                gridCompositions.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(fontSize * 1.8, GridUnitType.Pixel) });
            }
            int row = 0;
            foreach (Player player in match.compo1)
            {
                
                ViewUtils.AddElementToGrid(gridCompositions, ViewUtils.CreateTextBlock(ViewUtils.PlayerPositionOneLetter(player), StyleDefinition.styleTextPlainCenter), row, 1);
                ViewUtils.AddElementToGrid(gridCompositions, ViewUtils.CreateFlag(player.nationality, -1, fontSize), row, 2);
                UIElement playerElement = CreatePlayerElement(player, match.home);
                ViewUtils.AddElementToGrid(gridCompositions, playerElement, row, 3);
                row++;
            }
            row++;
            foreach (Player player in match.Subs1)
            {
                ViewUtils.AddElementToGrid(gridCompositions, ViewUtils.CreateTextBlock(ViewUtils.PlayerPositionOneLetter(player), StyleDefinition.styleTextPlainCenter), row, 1);
                ViewUtils.AddElementToGrid(gridCompositions, ViewUtils.CreateFlag(player.nationality, -1, fontSize), row, 2);
                UIElement playerElement = CreatePlayerElement(player, match.home);
                ViewUtils.AddElementToGrid(gridCompositions, playerElement, row, 3);
                row++;
            }
            int rowsSecondTeam = 0;
            foreach (Player player in match.compo2)
            {
                ViewUtils.AddElementToGrid(gridCompositions, ViewUtils.CreateTextBlock(ViewUtils.PlayerPositionOneLetter(player), StyleDefinition.styleTextPlainCenter), rowsSecondTeam, 7);
                ViewUtils.AddElementToGrid(gridCompositions, ViewUtils.CreateFlag(player.nationality, -1, fontSize), rowsSecondTeam, 6);
                UIElement playerElement = CreatePlayerElement(player, match.away);
                ViewUtils.AddElementToGrid(gridCompositions, playerElement, rowsSecondTeam, 5);
                rowsSecondTeam++;
            }
            rowsSecondTeam++;
            foreach (Player player in match.Subs2)
            {
                ViewUtils.AddElementToGrid(gridCompositions, ViewUtils.CreateTextBlock(ViewUtils.PlayerPositionOneLetter(player), StyleDefinition.styleTextPlainCenter), rowsSecondTeam, 7);
                ViewUtils.AddElementToGrid(gridCompositions, ViewUtils.CreateFlag(player.nationality, -1, fontSize), rowsSecondTeam, 6);
                UIElement playerElement = CreatePlayerElement(player, match.away);
                ViewUtils.AddElementToGrid(gridCompositions, playerElement, rowsSecondTeam, 5);
                rowsSecondTeam++;
            }
        }

        private void FillMedias()
        {
            gridMedias.Children.Clear();
            gridMedias.RowDefinitions.Clear();
            int row = 0;
            foreach (KeyValuePair<Media, Journalist> media in match.medias)
            {
                Media med = media.Key;
                gridMedias.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(fontSize * 1.8, GridUnitType.Pixel) });
                ViewUtils.AddElementToGrid(gridMedias, ViewUtils.CreateMediaLogo(media.Key, -1, fontSize), row, 0);
                TextBlock tbMedia = ViewUtils.CreateTextBlock(media.Value.ToString(), StyleDefinition.styleTextPlain);
                tbMedia.MouseLeftButtonDown += (s, e) => OnClickMedia(med);
                tbMedia.Margin = new Thickness(5, 0, 0, 0);
                ViewUtils.AddElementToGrid(gridMedias, tbMedia, row, 1);
                row++;
            }
        }

        private void OnClickMedia(Media media)
        {
            MediaView view = new MediaView(media);
            view.Show();
        }

        private void FillGameProgression(bool home)
        {
            Club club = home ? match.home : match.away;
            Grid grid = home ? gridEventsHome : gridEventsAway;
            double totalMinutes = match.prolongations ? 120.0 : 90.0;

            MatchEvent last = null;
            foreach (MatchEvent em in match.events)
            {
                if(em.club == club && em.type != GameEvent.Shot)
                {
                    int offset = last == null ? em.EventMinute : em.EventMinute - last.EventMinute;
                    if(offset < 0) // If a goal was scored at the 46° when the last was at 45+2° for exemple
                    {
                        offset = 1;
                    }
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(offset, GridUnitType.Star) });

                    string icone = "";
                    switch (em.type)
                    {
                        case GameEvent.Goal:
                            icone = "goal.png";
                            break;
                        case GameEvent.PenaltyGoal:
                            icone = "goal.png";
                            break;
                        case GameEvent.AgGoal:
                            icone = "goal.png";
                            break;
                        case GameEvent.YellowCard:
                            icone = "yellow_card.png";
                            break;
                        case GameEvent.RedCard:
                            icone = "red_card.png";
                            break;
                    }
                    string ogText = FindResource("str_og").ToString();
                    string penText = FindResource("str_pen").ToString();
                    string eventText = String.Format("{0} {1}{2}{3}", em.MinuteToString, em.player.Name, em.type == GameEvent.PenaltyGoal ? string.Format(" ({0})", penText) : "", em.type == GameEvent.AgGoal ? string.Format(" ({0})", ogText) : "");
                    string eventImg = "";
                    eventImg = Utils.Image(icone);

                    Image imEvent = new Image();
                    imEvent.Width = 20;
                    imEvent.Height = 20;
                    imEvent.HorizontalAlignment = HorizontalAlignment.Right;
                    imEvent.Source = new BitmapImage(new Uri(eventImg));

                    Border border = new Border();
                    border.Background = Brushes.Transparent;
                    border.Child = ViewUtils.CreateTextBlock(eventText, StyleDefinition.styleTextPlainCenter);
                    ToolTipService.SetInitialShowDelay(imEvent, 0);
                    imEvent.ToolTip = border;

                    ViewUtils.AddElementToGrid(grid, imEvent, -1, grid.ColumnDefinitions.Count - 1);
                    last = em;
                }
            }
            double finalOffset = last == null ? totalMinutes : totalMinutes - last.EventMinute;
            if(finalOffset > 0)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(finalOffset, GridUnitType.Star) });
            }

        }

        public void Initialize()
        {
            tbStatistics.Text = tbStatistics.Text.ToUpper();
            tbLineUps.Text = tbLineUps.Text.ToUpper();
            tbTournament.Text = tournament.name;
            tbAttendance.Text = String.Format(FindResource("str_attendance").ToString(), match.attendance);
            tbStadium.Text = match.stadium.name;
            imageTournament.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.LogoTournament(tournament)));
            imageHomeClub.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.home)));
            imageAwayClub.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.away)));
            tbHomeClubName.Text = match.home.name.ToUpper();
            tbAwayClubName.Text = match.away.name.ToUpper();
            tbScore.Text = match.Played ? String.Format("{0} - {1}{2}", match.score1, match.score2, match.prolongations ? String.Format(" {0}", FindResource("str_aet").ToString()) : "") : "";
            tbHalfTimeScore.Text = match.Played ? String.Format("({0} - {1})", match.ScoreHalfTime1, match.ScoreHalfTime2) : "";
            tbOddHome.Text = String.Format("{0:0.00}", match.odd1);
            tbOddDraw.Text = String.Format("{0:0.00}", match.oddD);
            tbOddAway.Text = String.Format("{0:0.00}", match.odd2);

            if(match.PenaltyShootout)
            {
                //Double the size of the score cell : to display penalty shootout results
                gridMainScore.ColumnDefinitions[(gridMainScore.ColumnDefinitions.Count - 1) / 2].Width = new GridLength(gridMainScore.ColumnDefinitions[(gridMainScore.ColumnDefinitions.Count - 1) / 2].Width.Value * 2, GridUnitType.Star);
                ControlPenaltyShootout cps = new ControlPenaltyShootout(match);
                cps.HorizontalAlignment = HorizontalAlignment.Center;
                ViewUtils.AddElementToGrid(gridScore, cps, 3, -1);
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int mParam, int lParam);

        private void spControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void spControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
