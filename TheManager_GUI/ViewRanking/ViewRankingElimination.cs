using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TheManager;
using TheManager.Comparators;
using System.Windows.Media.Imaging;

namespace TheManager_GUI.VueClassement
{
    public class ViewRankingElimination : View
    {

        private readonly DataGrid _grille;
        private readonly KnockoutRound _tour;
        private readonly double _sizeMultiplier;

        public ViewRankingElimination(DataGrid grille, KnockoutRound tour, double sizeMultiplier)
        {
            _grille = grille;
            _tour = tour;
            _sizeMultiplier = sizeMultiplier;
        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();
            List<Match> matchs = new List<Match>(_tour.matches);
            matchs.Sort(new MatchDateComparator());

            int index = 0;
            foreach(Match m in matchs)
            {
                index++;
                StackPanel spMatch = new StackPanel();
                spMatch.Orientation = Orientation.Horizontal;

                Label l1 = ViewUtils.CreateLabel(m.day.ToShortDateString(), "StyleLabel2", 10 * _sizeMultiplier, 75 * _sizeMultiplier);
                Label l2 = ViewUtils.CreateLabel(m.day.ToShortTimeString(), "StyleLabel2", 10 * _sizeMultiplier, 50 * _sizeMultiplier);

                Image img1 = new Image();
                img1.Width = 20;
                img1.Source = new BitmapImage(new Uri(Utils.Logo(m.home)));

                string l3Content = m.home.name + (m.home as CityClub != null ? " (" + m.home.Championship.shortName + ")" : "");
                Label l3 = ViewUtils.CreateLabel(l3Content, "StyleLabel2", 10 * _sizeMultiplier, 100 * _sizeMultiplier);
                l3.MouseLeftButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                { clubNameButtonClick(m.home); };


                Button btnScore = new Button();
                btnScore.Name = "btnScore_" + index;
                btnScore.Click += (object sender, RoutedEventArgs e) =>
                { matchButtonClick(m); };
                btnScore.Content = m.score1 + " - " + m.score2 + (m.prolongations ? " ap" : "") + (m.PenaltyShootout ? " (" + m.penaltyShootout1 + "-" + m.penaltyShootout2 + " tab)" : "");
                btnScore.Style = Application.Current.FindResource("StyleButtonLabel") as Style;
                btnScore.FontSize = 10 * _sizeMultiplier;
                btnScore.Width = 65 * _sizeMultiplier;

                string l5Content = m.away.name + (m.away as CityClub != null ? " (" + m.away.Championship.shortName + ")" : "");
                Label l5 = ViewUtils.CreateLabel(l5Content, "StyleLabel2", 10 * _sizeMultiplier, 100 * _sizeMultiplier);
                l5.MouseLeftButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                { clubNameButtonClick(m.away); };

                Image img2 = new Image();
                img2.Width = 20;
                img2.Source = new BitmapImage(new Uri(Utils.Logo(m.away)));


                spMatch.Children.Add(l1);
                spMatch.Children.Add(l2);
                spMatch.Children.Add(img1);
                spMatch.Children.Add(l3);
                spMatch.Children.Add(btnScore);
                spMatch.Children.Add(l5);
                spMatch.Children.Add(img2);

                spRanking.Children.Add(spMatch);
            }

        }

        public override void Show()
        {
            _grille.Items.Clear();
        }

        private void matchButtonClick(Match m)
        {
            if (m != null)
            {
                Windows_Match wc = new Windows_Match(m);
                wc.Show();
            }

        }

        private void clubNameButtonClick(Club c)
        {
            if (c != null && c as CityClub != null)
            {
                Windows_Club wc = new Windows_Club(c as CityClub);
                wc.Show();
            }

        }
    }
}
