using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public class VueClassementChampionnat : IVueClassement
    {

        private readonly DataGrid _grid;
        private readonly ChampionshipRound _round;
        private readonly double _sizeMultiplier;
        private readonly bool _focusOnTeam;
        private readonly Club _team;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="round"></param>
        /// <param name="sizeMultiplier">Width and font size multiplier</param>
        /// <param name="focusOnTeam">If true, only show 5 rows, focus the ranking around the team</param>
        /// <param name="team">The team to focus ranking on</param>
        public VueClassementChampionnat(DataGrid grid, ChampionshipRound round, double sizeMultiplier, bool focusOnTeam = false, Club team = null)
        {
            _grid = grid;
            _round = round;
            _sizeMultiplier = sizeMultiplier;
            _focusOnTeam = focusOnTeam;
            _team = team;
        }

        private void ApplyStyle2Label(Label l, double width)
        {
            l.Style = Application.Current.FindResource("StyleLabel2") as Style;
            l.Width = width * _sizeMultiplier;
            l.FontSize *= _sizeMultiplier;
        }

        public void Remplir(StackPanel spClassement)
        {
            spClassement.Children.Clear();

            int i = 0;

            List<Club> clubs = _round.Ranking();

            //If we choose to focus on a team, we center the ranking on the team and +-2 other teams around
            if (_focusOnTeam && _team != null)
            {
                clubs = new List<Club>();
                List<Club> ranking = _round.Ranking();
                int index = ranking.IndexOf(Session.Instance.Game.club);
                index = index - 2;
                if (index < 0)
                {
                    index = 0;
                }

                if (index > ranking.Count - 5)
                {
                    index = ranking.Count - 5;
                }
                i = index;
                for (int j = index; j < index + 5; j++)
                {
                    Club c = ranking[j];
                    clubs.Add(c);
                }
            }

            foreach (Club c in clubs)
            {
                i++;
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;

                Label l1 = new Label();
                l1.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l1.FontSize *= _sizeMultiplier;
                l1.Width = 30 * _sizeMultiplier;
                l1.Content = i.ToString();

                Image image = new Image();
                image.Source = new BitmapImage(new Uri(Utils.Logo(c)));
                image.Width = 30 * _sizeMultiplier;

                double regularCellWidth = 36/* * (1 + ((_sizeMultiplier - 1)/2))*/;

                Label l2 = new Label();
                ApplyStyle2Label(l2, 175 * _sizeMultiplier);
                l2.Content = c.shortName;
                l2.MouseLeftButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                { clubNameButtonClick(c); };
                //l2.MouseLeftButtonUp += clubNameButtonClick;

                Label l3 = new Label();
                ApplyStyle2Label(l3, regularCellWidth);
                l3.Content = _round.Points(c);

                Label l4 = new Label();
                ApplyStyle2Label(l4, regularCellWidth);
                l4.Content = _round.Played(c);

                Label l5 = new Label();
                ApplyStyle2Label(l5, regularCellWidth);
                l5.Content = _round.Wins(c);

                Label l6 = new Label();
                ApplyStyle2Label(l6, regularCellWidth);
                l6.Content = _round.Draws(c);

                Label l7 = new Label();
                ApplyStyle2Label(l7, regularCellWidth);
                l7.Content = _round.Loses(c);

                Label l8 = new Label();
                ApplyStyle2Label(l8, regularCellWidth);
                l8.Content = _round.GoalsFor(c);
                
                Label l9 = new Label();
                ApplyStyle2Label(l9, regularCellWidth);
                l9.Content = _round.GoalsAgainst(c);

                Label l10 = new Label();
                ApplyStyle2Label(l10, 45);
                l10.Content = _round.Difference(c);

                sp.Children.Add(l1);
                sp.Children.Add(image);
                sp.Children.Add(l2);
                sp.Children.Add(l3);
                sp.Children.Add(l4);
                sp.Children.Add(l5);
                sp.Children.Add(l6);
                sp.Children.Add(l7);
                sp.Children.Add(l8);
                sp.Children.Add(l9);
                sp.Children.Add(l10);

                spClassement.Children.Add(sp);

            }

            //Only show colors when the ranking is not focused on a team
            if (!_focusOnTeam)
            {
                foreach (Qualification q in _round.qualifications)
                {
                    if (q.tournament.isChampionship)
                    {
                        int niveau = _round.Tournament.level;
                        string couleur = "backgroundColor";
                        if (q.tournament.level < niveau)
                        {
                            couleur = "promotionColor";
                        }
                        else if (q.tournament.level > niveau)
                        {
                            couleur = "relegationColor";
                        }
                        else if (q.tournament.level == niveau && q.roundId > _round.Tournament.rounds.IndexOf(_round))
                        {
                            couleur = "barrageColor";
                        }

                        int index = q.ranking - 1;

                        if(couleur != "backgroundColor")
                        {
                            SolidColorBrush color = Application.Current.TryFindResource(couleur) as SolidColorBrush;
                            (spClassement.Children[index] as StackPanel).Background = color;
                        }
                    }

                }
            }

            

            /*
            Style s = new Style();
            s.Setters.Add(new Setter() { Property = Control.BackgroundProperty, Value = App.Current.TryFindResource("color2") as SolidColorBrush });
            s.Setters.Add(new Setter() { Property = Control.ForegroundProperty, Value = App.Current.TryFindResource("color2") as SolidColorBrush });


            //Pour chaque couleur
            foreach (Qualification q in _tour.Qualifications)
            {
                if (q.Competition.Championnat)
                {
                    int niveau = _tour.Competition.Niveau;
                    string couleur = "backgroundColor";
                    if (q.Competition.Niveau < niveau)
                        couleur = "promotionColor";
                    else if (q.Competition.Niveau > niveau)
                        couleur = "relegationColor";
                    else if (q.Competition.Niveau == niveau && q.IDTour > _tour.Competition.Tours.IndexOf(_tour))
                        couleur = "barrageColor";

                    DataTrigger tg = new DataTrigger()
                    {
                        Binding = new System.Windows.Data.Binding("Classement"),
                        Value = q.Classement
                    };
                    tg.Setters.Add(new Setter()
                    {
                        Property = Control.BackgroundProperty,
                        Value = App.Current.TryFindResource(couleur) as SolidColorBrush
                    });
                    s.Triggers.Add(tg);

                    _grille.CellStyle = s;
                }

            }*/
        }

        private void clubNameButtonClick(Club c)
        {
            if(c != null && c as CityClub != null)
            {
                Windows_Club wc = new Windows_Club(c as CityClub);
                wc.Show();
            }
 
        }

        public void Afficher()
        {
            _grid.Items.Clear();
            int i = 0;
            foreach (Club c in _round.Ranking())
            {
                i++;
                _grid.Items.Add(new ClassementElement { Logo = Utils.Logo(c), Club = c, Classement = i, Nom = c.shortName, Pts = _round.Points(c), J = _round.Played(c), G = _round.Wins(c), N = _round.Draws(c), P = _round.Loses(c), bp = _round.GoalsFor(c), bc = _round.GoalsAgainst(c), Diff = _round.Difference(c) });
            }
            Style s = new Style();

            s.Setters.Add(new Setter { Property = Control.BackgroundProperty, Value = App.Current.TryFindResource("color2") as SolidColorBrush });
            s.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = App.Current.TryFindResource("color2") as SolidColorBrush });


            //Pour chaque couleur
            foreach (Qualification q in _round.qualifications)
            {
                if (q.tournament.isChampionship)
                {
                    int niveau = _round.Tournament.level;
                    string couleur = "backgroundColor";
                    if (q.tournament.level < niveau)
                    {
                        couleur = "promotionColor";
                    }
                    else if (q.tournament.level > niveau)
                    {
                        couleur = "relegationColor";
                    }
                    else if (q.tournament.level == niveau && q.roundId > _round.Tournament.rounds.IndexOf(_round))
                    {
                        couleur = "barrageColor";
                    }

                    DataTrigger tg = new DataTrigger
                    {
                        Binding = new System.Windows.Data.Binding("Classement"),
                        Value = q.ranking
                    };
                    tg.Setters.Add(new Setter
                    {
                        Property = Control.BackgroundProperty,
                        Value = App.Current.TryFindResource(couleur) as SolidColorBrush
                    });
                    s.Triggers.Add(tg);

                    _grid.CellStyle = s;
                }

            }
        }
    }
}