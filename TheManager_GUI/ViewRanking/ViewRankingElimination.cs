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

        private readonly KnockoutRound _tour;
        private readonly double _sizeMultiplier;

        public ViewRankingElimination(KnockoutRound tour, double sizeMultiplier)
        {
            _tour = tour;
            _sizeMultiplier = sizeMultiplier;
        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();
            List<Match> matchs = new List<Match>(_tour.matches);
            matchs.Sort(new MatchDateComparator());

            int index = 0;

            bool internationalTournament = _tour.Tournament.IsInternational();

            if(_tour.twoLegs)
            {
                List<Match>[] pairs = new List<Match>[matchs.Count/2];
                for (int i = 0; i < matchs.Count / 2; i++) pairs[i] = new List<Match>();

                foreach(Match m in matchs)
                {
                    Utils.Debug(m.home.ToString() + " - " + m.away.ToString());
                    bool foundPair = false;
                    foreach(List<Match> pair in pairs)
                    {

                        if (pair.Count > 0 && pair[0].away == m.home)
                        {
                            pair.Add(m);
                            foundPair = true;
                        }
                    }
                    if(!foundPair)
                    {
                        int i = 0;
                        List<Match> pair = pairs[i];
                        while(pair.Count > 0)
                        {
                            pair = pairs[++i];
                        }
                        pair.Add(m);
                    }
                }

                foreach(List<Match> pair in pairs)
                {
                    int i = 0;
                    foreach(Match m in pair)
                    {
                        StackPanel spMatch = new StackPanel();
                        spMatch.Orientation = Orientation.Horizontal;
                        spMatch.Children.Add(ViewUtils.CreateLabel(m.home.shortName, "StyleLabel2", 10 * _sizeMultiplier, 125 * _sizeMultiplier, null, null, i > 0 && m.Winner == m.home ? true : false));

                        if(internationalTournament)
                        {
                            if(m.home as CityClub != null)
                            {
                                spMatch.Children.Add(ViewUtils.CreateFlag((m.home as CityClub).city.Country(), 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                            }
                        }
                        else if (m.home as CityClub != null)
                        {
                                spMatch.Children.Add(ViewUtils.CreateLabel(m.home.Championship.shortName, "StyleLabel2", 10 * _sizeMultiplier, 50 * _sizeMultiplier));
                        }

                        spMatch.Children.Add(ViewUtils.CreateLogo(m.home, 20 * _sizeMultiplier, 20 * _sizeMultiplier));
                        
                        spMatch.Children.Add(ViewUtils.CreateLabel(m.ScoreToString(), "StyleLabel2Center", 10 * _sizeMultiplier, 100 * _sizeMultiplier));

                        spMatch.Children.Add(ViewUtils.CreateLogo(m.away, 20 * _sizeMultiplier, 20 * _sizeMultiplier));

                        if(internationalTournament)
                        {
                            if(m.away as CityClub != null)
                            {
                                spMatch.Children.Add(ViewUtils.CreateFlag((m.away as CityClub).city.Country(), 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                            }
                        }
                        else if (m.away as CityClub != null)
                        {
                            spMatch.Children.Add(ViewUtils.CreateLabel(m.away.Championship.shortName, "StyleLabel2", 10 * _sizeMultiplier, 50 * _sizeMultiplier));
                        }

                        spMatch.Children.Add(ViewUtils.CreateLabel(m.away.shortName, "StyleLabel2", 10 * _sizeMultiplier, 125 * _sizeMultiplier, null, null, i > 0 && m.Winner == m.away ? true : false));
                        spRanking.Children.Add(spMatch);
                        i++;
                    }
                    Separator sep = new Separator();
                    spRanking.Children.Add(sep);
                }
            }
            else
            {

            

                foreach(Match m in matchs)
                {
                    index++;
                    StackPanel spMatch = new StackPanel();
                    spMatch.Orientation = Orientation.Horizontal;

                    Label l1 = ViewUtils.CreateLabel(m.day.ToShortDateString(), "StyleLabel2", 10 * _sizeMultiplier, 75 * _sizeMultiplier);
                    Label l2 = ViewUtils.CreateLabel(m.day.ToShortTimeString(), "StyleLabel2", 10 * _sizeMultiplier, 50 * _sizeMultiplier);

                    Image img1 = ViewUtils.CreateLogo(m.home, 20 * _sizeMultiplier, 20 * _sizeMultiplier);

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

                    Image img2 = ViewUtils.CreateLogo(m.away, 20 * _sizeMultiplier, 20 * _sizeMultiplier);


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
