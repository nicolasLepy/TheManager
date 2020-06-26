using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_ConfigurationPartie.xaml
    /// </summary>
    public partial class Windows_ConfigurationPartie : Window
    {

        private List<CheckBox> _checkbox;

        public Windows_ConfigurationPartie()
        {
            InitializeComponent();
            _checkbox = new List<CheckBox>();

            Game partie = new Game();
            Session.Instance.Game = partie;
            Kernel g = partie.kernel;
            DatabaseLoader cbdd = new DatabaseLoader(g);
            cbdd.Load();

            foreach(Continent c in g.continents)
            {
                StackPanel box = spCompEu;
                switch (c.Name())
                {
                    case "Europe":
                        box = spCompEu;
                        break;
                    case "Amérique du Nord":
                        box = spCompAmN;
                        break;
                    case "Amérique du Sud":
                        box = spCompAmS;
                        break;
                    case "Océanie":
                        box = spCompOc;
                        break;
                    case "Asie":
                        box = spCompAsie;
                        break;
                }

                if (c.Tournaments().Count>0)
                {
                    Label lb = new Label();
                    lb.Content = c.Name();
                    lb.Style = FindResource("StyleLabel2") as Style;
                    box.Children.Add(lb);
                    foreach(Tournament cp in c.Tournaments())
                    {
                        CheckBox cb = new CheckBox();
                        cb.IsChecked = true;
                        cb.Content = cp.name;
                        cb.Style = FindResource("StyleCheckBox") as Style;
                        cb.Click += new RoutedEventHandler(CheckboxComp_Click);
                        box.Children.Add(cb);
                        _checkbox.Add(cb);
                    }
                }
                foreach(Country p in c.countries)
                {
                    if(p.Tournaments().Count > 0)
                    {
                        Label lb = new Label();
                        lb.Content = p.Name();
                        lb.Style = FindResource("StyleLabel2") as Style;
                        Image i = new Image();
                        i.Source = new BitmapImage(new Uri( Environment.CurrentDirectory + "/Images/Drapeaux/" + p.Flag + ".png", UriKind.RelativeOrAbsolute));
                        i.Width = 30;
                        i.Height = 15;
                        StackPanel sp = new StackPanel();
                        sp.Orientation = Orientation.Horizontal;
                        sp.Children.Add(i);
                        sp.Children.Add(lb);
                        box.Children.Add(sp);
                        foreach (Tournament cp in p.Tournaments())
                        {
                            if(cp.isChampionship)
                            {
                                CheckBox cb = new CheckBox();
                                cb.IsChecked = true;
                                cb.Content = cp.name;
                                cb.Style = FindResource("StyleCheckBox") as Style;
                                box.Children.Add(cb);
                                _checkbox.Add(cb);

                            }

                        }
                    }
                }
            }

        }

        private void CheckboxComp_Click(object sender, RoutedEventArgs e)
        {
            int nbClubs = 0;
            int nbJoueurs = 0;
            foreach(CheckBox cb in _checkbox)
            {
                if(cb.IsChecked == true)
                {
                    Tournament c = Session.Instance.Game.kernel.String2Tournament(cb.Content.ToString());
                    foreach (Club cl in c.rounds[0].clubs)
                    {
                        nbClubs++;
                        nbJoueurs += 21;
                    }
                }
            }
            lbnbClubs.Content = "Nombre de clubs : " + nbClubs;
            lbnbJoueurs.Content = "Nombre de joueurs : " + nbJoueurs;
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnLancer_Click(object sender, RoutedEventArgs e)
        {
            List<Tournament> toDesactivate = new List<Tournament>();
            foreach (CheckBox cb in _checkbox)
            {
                if(cb.IsChecked == false)
                {
                    string nom = cb.Content.ToString();
                    foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
                    {
                        if (c.name == nom)
                        {
                            toDesactivate.Add(c);
                        }
                    }
                }
                
            }
            foreach(Tournament c in toDesactivate)
            {
                c.RendreInactive();
            }
            Windows_ChoixClub wch = new Windows_ChoixClub();
            wch.Show();

            Close();
        }
    }
}
