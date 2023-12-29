using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tm;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_ConfigurationPartie.xaml
    /// </summary>
    public partial class Windows_ConfigurationPartie : Window
    {

        private readonly List<CheckBox> _checkbox;
        private int _order;


        private CheckBox GetCheckBox(Tournament t)
        {
            CheckBox res = null;
            foreach(CheckBox cb in _checkbox)
            {
                if (cb.Content.ToString() == t.name)
                {
                    res = cb;
                }
            }
            return res;
        }

        private void CreateCheckBox(Tournament t, StackPanel box, bool disable)
        {
            StackPanel spTournament = new StackPanel();
            spTournament.Orientation = Orientation.Horizontal;

            TextBlock l = new TextBlock();
            l.Text = "   ";
            l.Margin = new Thickness(0, 0, 5, 0);
            l.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(t.color.ToHexa()));
            spTournament.Children.Add(l);

            if(!disable)
            {
                CheckBox cb = new CheckBox();
                cb.IsChecked = true;
                cb.Content = t.name;
                cb.Style = FindResource("StyleCheckBox") as Style;
                cb.Foreground = Brushes.LightGreen;
                cb.Click += CheckboxComp_Click;
                _checkbox.Add(cb);
                spTournament.Children.Add(cb);
            }
            else
            {
                spTournament.Children.Add(ViewUtils.CreateLabel(t.name, "StyleLabel2", -1, -1));
            }
            box.Children.Add(spTournament);

        }

        private void FillContinent(Continent c)
        {
            StackPanel spContinent = _order < 3 ? (spWorld.Children[_order] as Border).Child as StackPanel : ((spWorld.Children[3] as StackPanel).Children[_order-3] as Border).Child as StackPanel ;
            spContinent.Children.Add(ViewUtils.CreateLabel(c.Name(), "StyleLabel2Center", 16, -1));

            StackPanel box = new StackPanel();
            box.Orientation = Orientation.Vertical;

            if (c.Tournaments().Count > 0)
            {
                Label lb = new Label();
                lb.Content = c.Name();
                lb.Style = FindResource("StyleLabel2") as Style;
                lb.FontWeight = FontWeights.Bold;
                box.Children.Add(lb);
                foreach (Tournament cp in c.Tournaments())
                {
                    CreateCheckBox(cp, box, true);
                }
            }
            foreach (Country p in c.countries)
            {
                if (p.Tournaments().Count > 0)
                {
                    Label lb = new Label();
                    lb.Content = p.Name();
                    lb.Style = FindResource("StyleLabel2") as Style;
                    Image i = new Image();
                    i.Source = new BitmapImage(new Uri(Utils.Flag(p), UriKind.RelativeOrAbsolute));
                    i.Width = 30;
                    i.Height = 15;
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;
                    sp.Children.Add(i);
                    sp.Children.Add(lb);
                    box.Children.Add(sp);
                    foreach (Tournament cp in p.Tournaments())
                    {
                        if (cp.isChampionship)
                        {
                            CreateCheckBox(cp, box, false);

                        }

                    }
                }
            }
            spContinent.Children.Add(box);

            _order++;
        }

        public Windows_ConfigurationPartie()
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            _order = 0;
            _checkbox = new List<CheckBox>();

            Kernel g = Session.Instance.Game.kernel;
            
            foreach(Continent c in g.world.GetAllContinents())
            {
                FillContinent(c);
            }
        }

        private void CheckboxComp_Click(object sender, RoutedEventArgs e)
        {
            int nbClubs = 0;
            int nbJoueurs = 0;

            CheckBox checkBox = sender as CheckBox;
            Tournament selected = Session.Instance.Game.kernel.String2Tournament(checkBox.Content.ToString());
            foreach (Tournament t in Session.Instance.Game.kernel.LocalisationTournament(selected).Tournaments())
            {
                if (t.level > selected.level && t.isChampionship)
                {
                    GetCheckBox(t).IsChecked = false;
                }
                else if(t.level < selected.level && t.isChampionship)
                {
                    GetCheckBox(t).IsChecked = true;
                }
            }

            foreach (CheckBox cb in _checkbox)
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
            lbnbClubs.Content = FindResource("str_activeClubs").ToString() + " : " + nbClubs;
            lbnbJoueurs.Content = FindResource("str_playersEstimation").ToString() + " : " + nbJoueurs;
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDisable_Click(object sender, RoutedEventArgs e)
        {
            foreach(CheckBox cb in _checkbox)
            {
                cb.IsChecked = false;
            }
        }

        private void BtnEnable_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox cb in _checkbox)
            {
                cb.IsChecked = true;
            }
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
                c.DisableTournament();
                int pr = 0;
                int re = 0;
                foreach(Qualification q in c.rounds[0].qualifications)
                {
                    if(q.isNextYear && q.roundId == 0 && q.tournament.level > c.level)
                    {
                        re++;
                    }
                    if (q.isNextYear && q.roundId == 0 && q.tournament.level < c.level)
                    {
                        pr++;
                    }
                }
                Console.WriteLine("[" + c.name + "] " + pr + " promotions et " + re + " relegations");
            }
            Windows_ChoixClub wch = new Windows_ChoixClub();
            wch.Show();

            Close();
        }
    }
}
