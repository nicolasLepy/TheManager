using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;
using TheManager_GUI.ViewMisc;

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

            foreach (Continent c in Session.Instance.Game.kernel.continents)
            {
                foreach (Country p in c.countries)
                {
                    cbNationalite.Items.Add(p);
                }
            }

            RemplirTreeView();

        }

        private void RemplirTreeView()
        {

            tvClubs.Items.Clear();

            foreach(Continent c in Session.Instance.Game.kernel.continents)
            {
                foreach(Country p in c.countries)
                {
                    foreach(Tournament cp in p.Tournaments())
                    {
                        if(cp.isChampionship)
                        {
                            TreeViewItem tv = new TreeViewItem();
                            tv.Header = cp.name;

                            if((cp.rounds[0] as InactiveRound) == null)
                            {
                                foreach (Club club in cp.rounds[0].clubs)
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
                                    btnClub.FontSize = 11;
                                    btnClub.Foreground = Brushes.Black;
                                    btnClub.Name = "club_" + Session.Instance.Game.kernel.Clubs.IndexOf(club).ToString();
                                    if (club as CityClub != null)
                                    {
                                        btnClub.Click += new RoutedEventHandler(BtnClub_Click);
                                    }
                                    else
                                    {
                                        btnClub.Foreground = Brushes.DarkGray;
                                    }
                                    sp.Children.Add(logo);
                                    sp.Children.Add(btnClub);
                                    tv.Items.Add(sp);
                                }
                            }
                            else
                            {
                                tv.Items.Add(ViewUtils.CreateLabel("Compétition inactive", "StyleLabel2", 12, 100, Brushes.DarkGray));
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
            ViewPlayers view = new ViewPlayers(c.Players(), 10, true, true, true, true, true, false, false, true, true, true, false, false, false, false, false) ;
            view.Full(spEffectif);

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
                Utils.Debug("Pas de logo disponible pour " + club.logo);
            }

            spEtoiles.Children.Add(ViewUtils.CreateStarNotation(club.Stars, 20));

            /*
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
            }*/
        }
        
        private void BtnClub_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int id = int.Parse(btn.Name.Split('_')[1]);
            Club c = Session.Instance.Game.kernel.Clubs[id];

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
            Country nationalite = Session.Instance.Game.kernel.String2Country("France");
            Country pays_selected = cbNationalite.SelectedItem as Country;
            if (pays_selected != null)
            {
                nationalite = pays_selected;
            }

            if(club != null)
            {
                Session.Instance.Game.club = club as CityClub;
                Manager entraineur = new Manager(prenom, nom, 70, naissance, nationalite);
                Session.Instance.Game.club.ChangeManager(entraineur);
                Windows_Menu wm = new Windows_Menu();
                wm.Show();
                Close();
            }

        }
    }

}
