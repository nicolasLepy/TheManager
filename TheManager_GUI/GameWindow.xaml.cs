using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Match.xaml
    /// </summary>
    public partial class Windows_Match : Window
    {

        public Windows_Match(Match match)
        {
            InitializeComponent();
            try
            {
                imgCompetition.Source = new BitmapImage(new Uri(Utils.LogoTournament(match.Tournament)));
            }
            catch
            {
                //If there is no image, a empty image is let
            }
            lbCompetition.Content = match.Tournament.name;
            imgLogoHome.Source = new BitmapImage(new Uri(Utils.Logo(match.home)));
            imgLogoAway.Source = new BitmapImage(new Uri(Utils.Logo(match.away)));
            lbStade.Content = match.home.stadium.name;
            lbAffluence.Content = match.attendance + " spectateurs";
            lbEquipe1.Content = match.home.name;
            lbEquipe2.Content = match.away.name;
            lbScore1.Content = match.score1;
            lbScore2.Content = match.score2;
            lbScoreMt1.Content = match.ScoreHalfTime1;
            lbScoreMt2.Content = match.ScoreHalfTime2;
            lbTirs1.Content = match.statistics.HomeShoots;
            lbTirs2.Content = match.statistics.AwayShoots;
            pbTirs.Maximum = match.statistics.HomeShoots + match.statistics.AwayShoots;
            pbTirs.Value = match.statistics.HomeShoots;
            lbPossession1.Content = match.statistics.Possession1 + "%";
            lbPossession2.Content = match.statistics.Possession2 + "%";
            pbPossession.Maximum = match.statistics.Possession1 + match.statistics.Possession2;
            pbPossession.Value = match.statistics.Possession1;
            lbOdd1.Content = match.odd1.ToString("0.00");
            lbOddD.Content = match.oddD.ToString("0.00");
            lbOdd2.Content = match.odd2.ToString("0.00");
            if (match.prolongations)
            {
                spScoreLine.Children.Insert(3, ViewUtils.CreateLabel("ap", "StyleLabel2", 10, 20));
            }
            if(match.PenaltyShootout)
            {

                /**
                 * 
                 * 			<StackPanel Orientation="Vertical" HorizontalAlignment="Center" Height="40">
				<StackPanel Orientation="Horizontal">
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
				</StackPanel>
				<StackPanel Orientation="Horizontal">
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Lime"></Border>
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
					<Border Style="{StaticResource StyleBorderCalendar}" CornerRadius="3" Height="10" Width="10" Margin="5" Background="Red"></Border>
				</StackPanel>
			</StackPanel>


                 */

                StackPanel spTab = new StackPanel();
                spTab.Orientation = Orientation.Vertical;
                spTab.HorizontalAlignment = HorizontalAlignment.Center;
                spTab.Height = 40;

                StackPanel spTab1 = new StackPanel();
                spTab1.Orientation = Orientation.Horizontal;
                spTab1.Children.Add(ViewUtils.CreateLabel(match.penaltyShootout1.ToString(), "StyleLabel2", 10, 20));
                foreach(bool b in match.penaltyShoots1)
                {
                    Border border = new Border();
                    border.Style = Application.Current.FindResource("StyleBorderCalendar") as Style;
                    border.CornerRadius = new CornerRadius(3);
                    border.Height = 10;
                    border.Width = 10;
                    border.Margin = new Thickness(5);
                    border.Background = b == true ? Brushes.Lime : Brushes.Red;
                    spTab1.Children.Add(border);
                }

                StackPanel spTab2 = new StackPanel();
                spTab2.Orientation = Orientation.Horizontal;
                spTab2.Children.Add(ViewUtils.CreateLabel(match.penaltyShootout2.ToString(), "StyleLabel2", 10, 20));
                foreach (bool b in match.penaltyShoots2)
                {
                    Border border = new Border();
                    border.Style = Application.Current.FindResource("StyleBorderCalendar") as Style;
                    border.CornerRadius = new CornerRadius(3);
                    border.Height = 10;
                    border.Width = 10;
                    border.Margin = new Thickness(5);
                    border.Background = b == true ? Brushes.Lime : Brushes.Red;
                    spTab2.Children.Add(border);
                }

                spTab.Children.Add(spTab1);
                spTab.Children.Add(spTab2);

                spMain.Children.Insert(2, spTab);
            }

            spCompositions.Children.Add(ViewUtils.CreateCompositionPanel(match.compo1, false, match, match.Subs1));
            spCompositions.Children.Add(ViewUtils.CreateCompositionPanel(match.compo2, false, match, match.Subs2));

            foreach (MatchEvent em in match.events)
            {
                string icone = "";
                switch (em.type)
                {
                    case GameEvent.Goal: icone = "goal.png";
                        break;
                    case GameEvent.PenaltyGoal: icone = "goal.png";
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
                string c1 = "";
                string c2 = "";
                string c3 = "";
                string c4 = "";
                string img1 = "";
                string img2 = "";
                if (em.club == match.home)
                {
                    img1 = Utils.Image(icone);
                    c1 = em.MinuteToString;
                    c2 = em.player.firstName + " " + em.player.lastName;
                    if (em.type == GameEvent.PenaltyGoal)
                    {
                        c2 += " (sp)";
                    }

                    if (em.type == GameEvent.AgGoal)
                    {
                        c2 += " (csc)";
                    }
                }
                else
                {
                    img2 = Utils.Image(icone);
                    c4 = em.MinuteToString;
                    c3 = em.player.firstName + " " + em.player.lastName;
                    if (em.type == GameEvent.PenaltyGoal)
                    {
                        c3 += " (sp)";
                    }

                    if (em.type == GameEvent.AgGoal)
                    {
                        c3 += " (csc)";
                    }
                }
                if(em.type != GameEvent.Shot)
                {
                    StackPanel spEv = new StackPanel();
                    spEv.Orientation = Orientation.Horizontal;
                    spEv.Width = 400;

                    Image im1 = new Image();
                    im1.Width = 25;
                    if(img1 != "")
                    {
                        im1.Source = new BitmapImage(new Uri(img1));
                    }

                    Label l1 = ViewUtils.CreateLabel(c1, "StyleLabel2", 11, 40);
                    l1.HorizontalContentAlignment = HorizontalAlignment.Center;

                    Label l2 = ViewUtils.CreateLabel(c2, "StyleLabel2", 11, 130);
                    l2.HorizontalContentAlignment = HorizontalAlignment.Left;

                    Label l3 = ViewUtils.CreateLabel(c3, "StyleLabel2", 11, 130);
                    l3.HorizontalContentAlignment = HorizontalAlignment.Right;

                    Label l4 = ViewUtils.CreateLabel(c4, "StyleLabel2", 11, 40);
                    l4.HorizontalContentAlignment = HorizontalAlignment.Center;

                    Image im2 = new Image();
                    im2.Width = 25;
                    if (img2 != "")
                    {
                        im2.Source = new BitmapImage(new Uri(img2));
                    }

                    spEv.Children.Add(im1);
                    spEv.Children.Add(l1);
                    spEv.Children.Add(l2);
                    spEv.Children.Add(l3);
                    spEv.Children.Add(l4);
                    spEv.Children.Add(im2);
                    spEvenements.Children.Add(spEv);
                }

            }

            FillSubstitutions(match);
            DrawTimeline(match);

            List<Media> presentsMedias = new List<Media>();
            foreach (KeyValuePair<TheManager.Media, Journalist> j in match.journalists)
            {
                StackPanel spJournalists = new StackPanel();
                spJournalists.Orientation = Orientation.Vertical;
                spJournalists.Children.Add(ViewUtils.CreateMediaLogo(j.Key, 80, 40));
                spJournalists.Margin = new Thickness(15);
                Label labelJournalist = ViewUtils.CreateLabel(j.Value.ToString(), "StyleLabel2", 11, -1);
                labelJournalist.VerticalContentAlignment = VerticalAlignment.Center;

                if(presentsMedias.Contains(j.Key))
                {
                    (spMedias.Children[presentsMedias.IndexOf(j.Key)] as StackPanel).Children.Add(labelJournalist);
                }
                else
                {
                    spJournalists.Children.Add(labelJournalist);
                    presentsMedias.Add(j.Key);
                    spMedias.Children.Add(spJournalists);
                }
            }

        }

        private void DrawTimeline(Match match)
        {
            double canvasWidth = 700;
            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Width = canvasWidth;
            rect.Height = 8;
            Canvas.SetTop(rect, 46);
            rect.Stroke = System.Windows.Media.Brushes.Gray;
            canvasEvents.Children.Add(rect);

            MatchEvent last = null;
            foreach (MatchEvent em in match.events)
            {
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
                string eventText = "";
                string eventImg = "";
                eventImg = Utils.Image(icone);
                eventText = em.player.firstName + " " + em.player.lastName;
                if (em.type == GameEvent.PenaltyGoal)
                {
                    eventText += " (sp)";
                }

                if (em.type == GameEvent.AgGoal)
                {
                    eventText += " (csc)";
                }

                eventText += " (" + em.EventMinute + "')";

                if (em.type != GameEvent.Shot)
                {
                    Label labelEvent = ViewUtils.CreateLabel(eventText, "StyleLabel2Center", 10, 150);

                    Image imEvent = new Image();
                    imEvent.Width = 20;
                    imEvent.Source = new BitmapImage(new Uri(eventImg));

                    Canvas.SetLeft(labelEvent, ((em.EventMinute / 90.0) * canvasWidth)-75);
                    Canvas.SetLeft(imEvent, ((em.EventMinute / 90.0) * canvasWidth));
                    if(em.club == match.home)
                    {
                        Canvas.SetTop(labelEvent, (last != null && em.EventMinute-last.EventMinute < 5 && em.club == last.club) ? 10 : 20);
                        Canvas.SetTop(imEvent, 36);
                    }
                    else
                    {
                        Canvas.SetTop(imEvent, 52);
                        Canvas.SetTop(labelEvent, (last != null && em.EventMinute - last.EventMinute < 5 && em.club == last.club) ? 80 : 70);
                    }

                    canvasEvents.Children.Add(labelEvent);
                    canvasEvents.Children.Add(imEvent);

                }
                last = em;

            }




        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FillSubstitutions(Match match)
        {
            foreach(Substitution s in match.substitutions)
            {

                StackPanel spSub = new StackPanel();
                spSub.Orientation = Orientation.Horizontal;
                spSub.Children.Add(ViewUtils.CreateLabel(s.PlayerOut.firstName + " " + s.PlayerOut.lastName, "StyleLabel2", 11, 120, Brushes.LightSalmon));
                spSub.Children.Add(ViewUtils.CreateLabel(s.PlayerIn.firstName + " " + s.PlayerIn.lastName, "StyleLabel2", 11, 120, Brushes.Lime));
                spSub.Children.Add(ViewUtils.CreateLabel((s.Minute + 45).ToString() + "°", "StyleLabel2", 11, 30));
                if (match.compo1.Contains(s.PlayerOut))
                {
                    spHomeSubstitutions.Children.Add(spSub);
                }
                else
                {
                    spAwaySubstitutions.Children.Add(spSub);
                }
            }
        }

    }

    public struct JoueurElement : IEquatable<JoueurElement>
    {
        public Player Joueur { get; set; }
        public Position Poste { get; set; }
        public bool Equals(JoueurElement other)
        {
            throw new NotImplementedException();
        }
    }


}
