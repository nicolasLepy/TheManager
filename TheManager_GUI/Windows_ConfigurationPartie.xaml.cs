﻿using System;
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
    /// Logique d'interaction pour Windows_ConfigurationPartie.xaml
    /// </summary>
    public partial class Windows_ConfigurationPartie : Window
    {

        private List<CheckBox> _checkbox;

        public Windows_ConfigurationPartie()
        {
            InitializeComponent();
            _checkbox = new List<CheckBox>();

            Partie partie = new Partie();
            Session.Instance.Partie = partie;
            Gestionnaire g = partie.Gestionnaire;
            ChargeurBaseDeDonnees cbdd = new ChargeurBaseDeDonnees(g);
            cbdd.Charger();

            foreach(Continent c in g.Continents)
            {
                StackPanel box = spCompEu;
                switch (c.Nom())
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

                if (c.Competitions().Count>0)
                {
                    Label lb = new Label();
                    lb.Content = c.Nom();
                    lb.Style = FindResource("StyleLabel2") as Style;
                    box.Children.Add(lb);
                    foreach(Competition cp in c.Competitions())
                    {
                        CheckBox cb = new CheckBox();
                        cb.IsChecked = true;
                        cb.Content = cp.Nom;
                        cb.Style = FindResource("StyleCheckBox") as Style;
                        //cb.Click += new RoutedEventHandler(CheckboxComp_Click);
                        box.Children.Add(cb);
                        _checkbox.Add(cb);
                    }
                }
                foreach(Pays p in c.Pays)
                {
                    if(p.Competitions().Count > 0)
                    {
                        Label lb = new Label();
                        lb.Content = p.Nom();
                        lb.Style = FindResource("StyleLabel2") as Style;
                        Image i = new Image();
                        i.Source = new BitmapImage(new Uri( Environment.CurrentDirectory + "/Images/Drapeaux/" + p.Drapeau + ".png", UriKind.RelativeOrAbsolute));
                        Console.WriteLine(i.Source);
                        Console.WriteLine(p.Drapeau);
                        i.Width = 30;
                        i.Height = 15;
                        StackPanel sp = new StackPanel();
                        sp.Orientation = Orientation.Horizontal;
                        sp.Children.Add(i);
                        sp.Children.Add(lb);
                        box.Children.Add(sp);
                        foreach (Competition cp in p.Competitions())
                        {
                            
                            CheckBox cb = new CheckBox();
                            cb.IsChecked = true;
                            cb.Content = cp.Nom;
                            cb.Style = FindResource("StyleCheckBox") as Style;
                            box.Children.Add(cb);
                            if (cp.Championnat)
                                cb.IsEnabled = false;
                            _checkbox.Add(cb);

                        }
                    }
                }
            }

        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnLancer_Click(object sender, RoutedEventArgs e)
        {
            List<Competition> toDelete = new List<Competition>();
            foreach (CheckBox cb in _checkbox)
            {
                if(cb.IsChecked == false)
                {
                    string nom = cb.Content.ToString();
                    foreach (Competition c in Session.Instance.Partie.Gestionnaire.Competitions)
                    {
                        if (c.Nom == nom)
                        {
                            toDelete.Add(c);
                        }
                    }
                }
                
            }
            foreach(Competition c in toDelete)
            {
                Session.Instance.Partie.Gestionnaire.LocalisationCompetition(c).Competitions().Remove(c);
            }
            Windows_ChoixClub wch = new Windows_ChoixClub();
            wch.Show();

            Close();
        }
    }
}