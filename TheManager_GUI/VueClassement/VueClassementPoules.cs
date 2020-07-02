using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;
using TheManager.Comparators;

namespace TheManager_GUI.VueClassement
{
    public class VueClassementPoules : IVueClassement
    {

        private readonly DataGrid _grille;
        private readonly GroupsRound _tour;

        public VueClassementPoules(DataGrid grille, GroupsRound tour)
        {
            _grille = grille;
            _tour = tour;
        }

        public void Remplir(StackPanel spClassement)
        {
            spClassement.Children.Clear();

            for(int poule = 0; poule<_tour.groupsCount; poule++)
            {
                Label labelPoule = new Label();
                labelPoule.Content = _tour.GroupName(poule);
                labelPoule.Style = Application.Current.FindResource("StyleLabel1") as Style;
                spClassement.Children.Add(labelPoule);

                int i = 0;
                foreach (Club c in _tour.Ranking(poule))
                {
                    i++;
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;

                    Label l1 = new Label();
                    l1.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l1.Width = 30;
                    l1.Content = i.ToString();

                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri(Utils.Logo(c)));
                    image.Width = 30;

                    Label l2 = new Label();
                    l2.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l2.Width = 150;
                    l2.Content = c.shortName;

                    Label l3 = new Label();
                    l3.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l3.Width = 25;
                    l3.Content = _tour.Points(c);

                    Label l4 = new Label();
                    l4.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l4.Width = 25;
                    l4.Content = _tour.Played(c);

                    Label l5 = new Label();
                    l5.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l5.Width = 25;
                    l5.Content = _tour.Wins(c);

                    Label l6 = new Label();
                    l6.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l6.Width = 25;
                    l6.Content = _tour.Draws(c);

                    Label l7 = new Label();
                    l7.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l7.Width = 25;
                    l7.Content = _tour.Loses(c);

                    Label l8 = new Label();
                    l8.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l8.Width = 25;
                    l8.Content = _tour.GoalsFor(c);
                    Label l9 = new Label();
                    l9.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l9.Width = 25;
                    l9.Content = _tour.GoalsAgainst(c);
                    Label l10 = new Label();
                    l10.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l10.Width = 25;
                    l10.Content = _tour.Difference(c);


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

                
            }


            foreach (Qualification q in _tour.qualifications)
            {
                if (q.tournament.isChampionship)
                {
                    int niveau = _tour.Tournament.level;
                    string couleur = "backgroundColor";
                    if (q.tournament.level < niveau)
                    {
                        couleur = "promotionColor";
                    }
                    else if (q.tournament.level > niveau)
                    {
                        couleur = "relegationColor";
                    }
                    else if (q.tournament.level == niveau && q.roundId > _tour.Tournament.rounds.IndexOf(_tour))
                    {
                        couleur = "barrageColor";
                    }

                    int index = q.ranking - 1;

                    SolidColorBrush color = Application.Current.TryFindResource(couleur) as SolidColorBrush;
                    int nbChildrenParPoule = (_tour.clubs.Count / _tour.groupsCount)+1;
                    index++;
                    for (int j = 0; j < _tour.groupsCount; j++)
                    {
                        (spClassement.Children[j* nbChildrenParPoule + index] as StackPanel).Background = color;
                    }
                }

            }

        }

        public void Afficher()
        {
            _grille.Items.Clear();
            for (int poules = 0; poules < _tour.groupsCount; poules++)
            {
                List<Club> poule = new List<Club>(_tour.groups[poules]);
                poule.Sort(new ClubRankingComparator(_tour.matches));
                int i = 0;
                _grille.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                _grille.Items.Add(new ClassementElement { Classement = i, Nom = "Poule " + (int)(poules + 1) });
                _grille.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                foreach (Club c in poule)
                {
                    i++;
                    _grille.Items.Add(new ClassementElement { Logo = Utils.Logo(c), Classement = i, Nom = c.shortName, Pts = _tour.Points(c), J = _tour.Played(c), G = _tour.Wins(c), N = _tour.Draws(c), P = _tour.Loses(c), bp = _tour.GoalsFor(c), bc = _tour.GoalsAgainst(c), Diff = _tour.Difference(c) });
                }
            }
        }
    }
}
