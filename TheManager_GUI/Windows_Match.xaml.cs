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
using System.Windows.Shapes;
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
            catch { }
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
            if (match.prolongations)
                lbScore.Content += " a.p.";
            if(match.PenaltyShootout)
            {
                lbScore.Content += " (" + match.penaltyShootout1 + "-" + match.penaltyShootout2 + " tab)";
            }
            lbMT.Content = "(" + match.ScoreHalfTime1 + " - " + match.ScoreHalfTime2 + ")";

            foreach(MatchEvent em in match.events)
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
                    if (em.type == GameEvent.PenaltyGoal) c2 += " (sp)";
                    if (em.type == GameEvent.AgGoal) c2 += " (csc)";
                }
                else
                {
                    img2 = Utils.Image(icone);
                    c4 = em.MinuteToString;
                    c3 = em.player.firstName + " " + em.player.lastName;
                    if (em.type == GameEvent.PenaltyGoal) c3 += " (sp)";
                    if (em.type == GameEvent.AgGoal) c3 += " (csc)";
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

                    Label l1 = new Label();
                    l1.Width = 40;
                    l1.HorizontalContentAlignment = HorizontalAlignment.Center;
                    l1.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l1.Content = c1;

                    Label l2 = new Label();
                    l2.Width = 130;
                    l2.HorizontalContentAlignment = HorizontalAlignment.Left;
                    l2.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l2.Content = c2;

                    Label l3 = new Label();
                    l3.Width = 130;
                    l3.HorizontalContentAlignment = HorizontalAlignment.Right;
                    l3.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l3.Content = c3;

                    Label l4 = new Label();
                    l4.Width = 40;
                    l4.HorizontalContentAlignment = HorizontalAlignment.Center;
                    l4.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l4.Content = c4;

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

            foreach (Player j in match.compo1)
            {
                dgCompo1.Items.Add(new JoueurElement { Joueur = j, Poste = j.position });
            }

            foreach (Player j in match.compo2)
            {
                dgCompo2.Items.Add(new JoueurElement { Joueur = j, Poste = j.position });
            }

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

        private void DgCompo1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgCompo1.SelectedItem != null)
            {
                JoueurElement je = (JoueurElement)dgCompo1.SelectedItem;
                Windows_Joueur wj = new Windows_Joueur(je.Joueur);
                wj.Show();
            }
        }

        private void DgCompo2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCompo2.SelectedItem != null)
            {
                JoueurElement je = (JoueurElement)dgCompo2.SelectedItem;
                Windows_Joueur wj = new Windows_Joueur(je.Joueur);
                wj.Show();
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
