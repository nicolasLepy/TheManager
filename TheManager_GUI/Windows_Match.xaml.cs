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
            imgLogoHome.Source = new BitmapImage(new Uri(Utils.Logo(match.home)));
            imgLogoAway.Source = new BitmapImage(new Uri(Utils.Logo(match.away)));
            lbStade.Content = match.home.stadium.name;
            lbAffluence.Content = match.attendance + " spectateurs";
            lbEquipe1.Content = match.home.name;
            lbEquipe2.Content = match.away.name;
            lbScore.Content = match.score1 + " - " + match.score2;
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
                lbScore.Content += " a.p.";
            }
            if(match.PenaltyShootout)
            {
                lbScore.Content += " (" + match.penaltyShootout1 + "-" + match.penaltyShootout2 + " tab)";
            }
            lbMT.Content = "(" + match.ScoreHalfTime1 + " - " + match.ScoreHalfTime2 + ")";

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

                    Label l1 = ViewUtils.CreateLabel(c1, "StyleLabel2", -1, 40);
                    l1.HorizontalContentAlignment = HorizontalAlignment.Center;

                    Label l2 = ViewUtils.CreateLabel(c2, "StyleLabel2", -1, 130);
                    l2.HorizontalContentAlignment = HorizontalAlignment.Left;

                    Label l3 = ViewUtils.CreateLabel(c3, "StyleLabel2", -1, 130);
                    l3.HorizontalContentAlignment = HorizontalAlignment.Right;

                    Label l4 = ViewUtils.CreateLabel(c4, "StyleLabel2", -1, 40);
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

            foreach (KeyValuePair<TheManager.Media, Journalist> j in match.journalists)
            {
                dgJournalistes.Items.Add(new JournalisteElement { Journaliste = j.Value, Media = j.Key.name });
            }

        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DgJournalistes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgJournalistes.SelectedItem != null)
            {
                JournalisteElement selected = (JournalisteElement)dgJournalistes.SelectedItem;
                Windows_Journaliste wj = new Windows_Journaliste(selected.Journaliste);
                wj.Show();
            }
        }

        private void FillSubstitutions(Match match)
        {
            foreach(Substitution s in match.substitutions)
            {

                StackPanel spSub = new StackPanel();
                spSub.Orientation = Orientation.Horizontal;
                spSub.Children.Add(ViewUtils.CreateLabel(s.PlayerOut.firstName + " " + s.PlayerOut.lastName, "StyleLabel2", 11, 120, Brushes.Red));
                spSub.Children.Add(ViewUtils.CreateLabel(s.PlayerIn.firstName + " " + s.PlayerIn.lastName, "StyleLabel2", 11, 120, Brushes.Green));
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

    public struct JournalisteElement : IEquatable<JournalisteElement>
    {
        public Journalist Journaliste { get; set; }
        public string Media{ get; set; }
        public bool Equals(JournalisteElement other)
        {
            throw new NotImplementedException();
        }
    }

}
