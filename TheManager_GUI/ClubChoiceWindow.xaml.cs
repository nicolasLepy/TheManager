﻿using System;
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

            foreach(Association a in Session.Instance.Game.kernel.GetAssociationsOfHierarchyLevel(2))
            {
                cbNationalite.Items.Add(a.localization as Country);
            }
            cbNationalite.SelectedIndex = 0;

            RemplirTreeView();

        }

        private void RemplirTreeView()
        {

            tvClubs.Items.Clear();

            foreach(Tournament cp in Session.Instance.Game.kernel.worldAssociation.AllTournaments())
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

        private void FillSquad(Club c)
        {
            
            ViewPlayers view = new ViewPlayers(c.Players(), 11, true, true, true, true, true, false, false, true, true, true, false, false, false, false, false, false, false) ;
            view.Full(spEffectif);

        }

        private void ClubIsSelected()
        {
            lbClub.Content = club.name;
            lbStadium.Content = club.stadium.name + " (" + club.stadium.capacity + " "+ FindResource("str_seats").ToString() + ")";
            lbBudget.Content = FindResource("str_budget").ToString() + " : " + Utils.FormatMoney((club as CityClub).budget);
            lbCountry.Content = club.Country().Name();
            FillSquad(club);
            spEtoiles.Children.Clear();
            try
            {
                imgClub.Source = new BitmapImage(new Uri(Utils.Logo(club)));
            }
            catch
            {
                Utils.Debug("Pas de logo disponible pour " + club.logo);
            }

            spEtoiles.Children.Add(ViewUtils.CreateStarNotation(club.Stars, 25));

        }
        
        private void BtnClub_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int id = int.Parse(btn.Name.Split('_')[1]);
            Club c = Session.Instance.Game.kernel.Clubs[id];

            if (c != null)
            {
                club = c;
                ClubIsSelected();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string prenom = tbPrenom.Text;
            string nom = tbNom.Text;
            string[] strBirthday = dpNaissance.Text.Split('/');
            DateTime birthday = new DateTime( int.Parse(strBirthday[2]), int.Parse(strBirthday[1]), int.Parse(strBirthday[0]));
            Country nationality = Session.Instance.Game.kernel.String2Country("France");
            Country selectedCountry = cbNationalite.SelectedItem as Country;
            if (selectedCountry != null)
            {
                nationality = selectedCountry;
            }

            if(club != null)
            {
                Session.Instance.Game.club = club as CityClub;
                Manager manager = new Manager(prenom, nom, 70, birthday, nationality);
                Session.Instance.Game.club.ChangeManager(manager);
                Windows_Menu wm = new Windows_Menu();
                wm.Show();
                Close();
            }

        }
    }

}
