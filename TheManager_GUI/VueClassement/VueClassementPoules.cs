using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private DataGrid _grille;
        private TourPoules _tour;

        public VueClassementPoules(DataGrid grille, TourPoules tour)
        {
            _grille = grille;
            _tour = tour;
        }

        public void Remplir(StackPanel spClassement)
        {
            spClassement.Children.Clear();

            for(int poule = 0; poule<_tour.NombrePoules; poule++)
            {
                Label labelPoule = new Label();
                labelPoule.Content = _tour.NomGroupe(poule);
                labelPoule.Style = Application.Current.FindResource("StyleLabel1") as Style;
                spClassement.Children.Add(labelPoule);

                int i = 0;
                foreach (Club c in _tour.Classement(poule))
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
                    l4.Content = _tour.Joues(c);

                    Label l5 = new Label();
                    l5.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l5.Width = 25;
                    l5.Content = _tour.Gagnes(c);

                    Label l6 = new Label();
                    l6.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l6.Width = 25;
                    l6.Content = _tour.Nuls(c);

                    Label l7 = new Label();
                    l7.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l7.Width = 25;
                    l7.Content = _tour.Perdus(c);

                    Label l8 = new Label();
                    l8.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l8.Width = 25;
                    l8.Content = _tour.ButsPour(c);
                    Label l9 = new Label();
                    l9.Style = Application.Current.FindResource("StyleLabel2") as Style;
                    l9.Width = 25;
                    l9.Content = _tour.ButsContre(c);
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

                    int index = q.Classement - 1;

                    SolidColorBrush color = Application.Current.TryFindResource(couleur) as SolidColorBrush;
                    int nbChildrenParPoule = (_tour.Clubs.Count / _tour.NombrePoules)+1;
                    index++;
                    for (int j = 0; j < _tour.NombrePoules; j++)
                    {
                        (spClassement.Children[j* nbChildrenParPoule + index] as StackPanel).Background = color;
                    }
                }

            }

        }

        public void Afficher()
        {
            _grille.Items.Clear();
            for (int poules = 0; poules < _tour.NombrePoules; poules++)
            {
                List<Club> poule = new List<Club>(_tour.Poules[poules]);
                poule.Sort(new Club_Classement_Comparator(_tour.Matchs));
                int i = 0;
                _grille.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                _grille.Items.Add(new ClassementElement { Classement = i, Nom = "Poule " + (int)(poules + 1) });
                _grille.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                foreach (Club c in poule)
                {
                    i++;
                    _grille.Items.Add(new ClassementElement { Logo = Utils.Logo(c), Classement = i, Nom = c.shortName, Pts = _tour.Points(c), J = _tour.Joues(c), G = _tour.Gagnes(c), N = _tour.Nuls(c), P = _tour.Perdus(c), bp = _tour.ButsPour(c), bc = _tour.ButsContre(c), Diff = _tour.Difference(c) });
                }
            }
        }
    }
}
