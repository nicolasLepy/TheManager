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
    /// Logique d'interaction pour Windows_ChoixClub.xaml
    /// </summary>
    public partial class Windows_ChoixClub : Window
    {

        private Club club;

        public Windows_ChoixClub()
        {
            club = null;
            InitializeComponent();

            foreach (Continent c in Session.Instance.Partie.Gestionnaire.Continents)
                foreach (Pays p in c.Pays)
                    cbNationalite.Items.Add(p);

            RemplirTreeView();

        }

        private void RemplirTreeView()
        {

            tvClubs.Items.Clear();

            foreach(Continent c in Session.Instance.Partie.Gestionnaire.Continents)
            {
                foreach(Pays p in c.Pays)
                {
                    foreach(Tournament cp in p.Competitions())
                    {
                        if(cp.isChampionship)
                        {
                            TreeViewItem tv = new TreeViewItem();
                            tv.Header = cp.name;

                            foreach(Club club in cp.rounds[0].Clubs)
                            {
                                StackPanel sp = new StackPanel();
                                sp.Orientation = Orientation.Horizontal;
                                Image logo = new Image();
                                logo.Width = 20;
                                logo.Height = 20;
                                logo.Source = new BitmapImage(new Uri(Utils.Logo(club)));
                                Button btnClub = new Button();
                                btnClub.Content = club.name;
                                btnClub.Style = Application.Current.FindResource("StyleButtonLabel") as Style;
                                btnClub.FontSize = 8;
                                btnClub.Name = "club_" + Session.Instance.Partie.Gestionnaire.Clubs.IndexOf(club).ToString();
                                btnClub.Click += new RoutedEventHandler(BtnClub_Click);
                                Label lbClub = new Label();
                                lbClub.Content = club.name;
                                lbClub.Style = Application.Current.FindResource("StyleLabel2") as Style;
                                lbClub.FontSize = 8;
                                sp.Children.Add(logo);
                                sp.Children.Add(btnClub);
                                tv.Items.Add(sp);
                            }
                            tvClubs.Items.Add(tv);
                        }
                    }
                }
            }
            
            /*
             * <TreeViewItem Header="Employee1">
					<TreeViewItem Header="Jesper Aaberg">
						<StackPanel Orientation="Horizontal">
							<Label Content="ha"/>
							<Label Content="ha"/>
						</StackPanel>
					</TreeViewItem>
             */
        }

        private void RemplirEffectif(Club c)
        {
            spEffectif.Children.Clear();
            Console.WriteLine("Génération du club - " + c.formationFacilities);
            foreach (Joueur j in c.Players())
            {
                Label l = new Label();
                l.Content = j.Nom + " - " + j.Niveau + " (" + j.Potentiel + ")";
                spEffectif.Children.Add(l);
                Console.WriteLine(j.Niveau + " (" + j.Potentiel + ") - " + j.Age);
            }
        }

        private void ClubSelectionne()
        {
            RemplirEffectif(club);
            spEtoiles.Children.Clear();
            try
            {
                imgClub.Source = new BitmapImage(new Uri(Utils.Logo(club)));
            }
            catch
            {
                Console.WriteLine("Pas de logo disponible pour " + club.logo);
            }
            float etoiles = club.Stars;
            int etoilesEntieres = (int)Math.Floor(etoiles);
            for (int i = 1; i <= etoilesEntieres; i++)
            {
                Image img = new Image();
                img.Width = 20;
                img.Height = 20;
                img.Source = new BitmapImage(new Uri(Utils.Image("star.png")));
                spEtoiles.Children.Add(img);
            }
            if (etoiles - etoilesEntieres != 0)
            {
                Image img = new Image();
                img.Width = 20;
                img.Height = 20;
                img.Source = new BitmapImage(new Uri(Utils.Image("demistar.png")));
                spEtoiles.Children.Add(img);
            }
        }
        
        private void BtnClub_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int id = int.Parse(btn.Name.Split('_')[1]);
            Club c = Session.Instance.Partie.Gestionnaire.Clubs[id];

            if (c != null)
            {
                club = c;
                ClubSelectionne();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string prenom = tbPrenom.Text;
            string nom = tbNom.Text;
            string[] strNaissance = dpNaissance.Text.Split('/');
            DateTime naissance = new DateTime( int.Parse(strNaissance[2]), int.Parse(strNaissance[1]), int.Parse(strNaissance[0]));
            Pays nationalite = Session.Instance.Partie.Gestionnaire.String2Pays("France");
            Pays pays_selected = cbNationalite.SelectedItem as Pays;
            if (pays_selected != null) nationalite = pays_selected;

            if(club != null)
            {
                Session.Instance.Partie.Club = club as CityClub;
                Entraineur entraineur = new Entraineur(prenom, nom, 70, naissance, nationalite);
                Session.Instance.Partie.Club.ChangeManager(entraineur);
                Windows_Menu wm = new Windows_Menu();
                wm.Show();
                Close();
            }

        }
    }

}
