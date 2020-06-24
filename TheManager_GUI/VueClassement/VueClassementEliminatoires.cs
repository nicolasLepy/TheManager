using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TheManager;
using TheManager.Comparators;
using System.Windows.Media.Imaging;

namespace TheManager_GUI.VueClassement
{
    public class VueClassementEliminatoires : IVueClassement
    {

        private DataGrid _grille;
        private TourElimination _tour;

        public VueClassementEliminatoires(DataGrid grille, TourElimination tour)
        {
            _grille = grille;
            _tour = tour;
        }

        public void Remplir(StackPanel spClassement)
        {
            spClassement.Children.Clear();
            List<Match> matchs = new List<Match>(_tour.Matchs);
            matchs.Sort(new Match_Date_Comparator());

            int index = 0;
            foreach(Match m in matchs)
            {
                index++;
                StackPanel spMatch = new StackPanel();
                spMatch.Orientation = Orientation.Horizontal;

                Label l1 = new Label();
                l1.Content = m.Jour.ToShortDateString();
                l1.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l1.FontSize = 10;
                l1.Width = 75;

                Label l2 = new Label();
                l2.Content = m.Jour.ToShortTimeString();
                l2.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l2.FontSize = 10;
                l2.Width = 50;

                Image img1 = new Image();
                img1.Width = 20;
                img1.Source = new BitmapImage(new Uri(Utils.Logo(m.Domicile)));

                Label l3 = new Label();
                l3.Content = m.Domicile.name + (m.Domicile as Club_Ville != null ? " (" + m.Domicile.Championship.NomCourt + ")" : "");
                l3.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l3.FontSize = 10;
                l3.Width = 100;

                Button btnScore = new Button();
                btnScore.Name = "btnScore_" + index;
                //btnScore.Click += new RoutedEventHandler(BtnMatch_Click);
                btnScore.Content = m.Score1 + " - " + m.Score2 + (m.Prolongations ? " ap" : "") + (m.TAB ? " (" + m.Tab1 + "-" + m.Tab2 + " tab)" : "");
                btnScore.Style = Application.Current.FindResource("StyleButtonLabel") as Style;
                btnScore.FontSize = 10;
                btnScore.Width = 65;

                Label l5 = new Label();
                l5.Content = m.Exterieur.name + (m.Exterieur as Club_Ville != null ? " (" + m.Exterieur.Championship.NomCourt + ")" : "");
                l5.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l5.FontSize = 10;
                l5.Width = 100;

                Image img2 = new Image();
                img2.Width = 20;
                img2.Source = new BitmapImage(new Uri(Utils.Logo(m.Exterieur)));


                spMatch.Children.Add(l1);
                spMatch.Children.Add(l2);
                spMatch.Children.Add(img1);
                spMatch.Children.Add(l3);
                spMatch.Children.Add(btnScore);
                spMatch.Children.Add(l5);
                spMatch.Children.Add(img2);

                spClassement.Children.Add(spMatch);
            }

        }

        public void Afficher()
        {
            _grille.Items.Clear();
        }
    }
}
