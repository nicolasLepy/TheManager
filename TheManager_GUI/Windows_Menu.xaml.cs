using Microsoft.Win32;
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
using TheManager.Comparators;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Menu.xaml
    /// </summary>
    public partial class Windows_Menu : Window
    {
        private Partie _partie = null;

        public Windows_Menu()
        {
            InitializeComponent();


            _partie = Session.Instance.Partie;

            imgClub.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Output\\Logos\\" + _partie.Club.Logo + ".png"));
            comboPays.Items.Clear();
            foreach (Continent c in _partie.Gestionnaire.Continents)
            {
                if (c.Competitions().Count > 0) this.comboPays.Items.Add(c);
                foreach (Pays p in c.Pays) if (p.Competitions().Count > 0) { this.comboPays.Items.Add(p); Console.WriteLine(p); }
            }
            Refresh();

            
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Avancer()
        {
            _partie.Avancer();
            Refresh();
        }

        private void BtnAvancer_Click(object sender, RoutedEventArgs e)
        {

            Avancer();
        }

        private void LbChampionnats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Competition c = lbChampionnats.SelectedItem as Competition;
            if(c != null)
            {
                lbTours.Items.Clear();
                dgButeurs.Items.Clear();
                dgClassement.Items.Clear();
                dgMatchs.Items.Clear();
                foreach (Tour t in c.Tours)
                {
                    lbTours.Items.Add(t);
                }

                if (c.Statistiques.PlusGrandScore != null)
                    lbGrandScore.Content = c.Statistiques.PlusGrandScore.Domicile.Nom + " " + c.Statistiques.PlusGrandScore.Score1 + "-" + c.Statistiques.PlusGrandScore.Score2 + " " + c.Statistiques.PlusGrandScore.Exterieur.Nom;
                else
                    lbGrandScore.Content = "";
                if (c.Statistiques.PlusGrandEcart != null)
                    lbGrosEcart.Content = c.Statistiques.PlusGrandEcart.Domicile.Nom + " " + c.Statistiques.PlusGrandEcart.Score1 + "-" + c.Statistiques.PlusGrandEcart.Score2 + " " + c.Statistiques.PlusGrandEcart.Exterieur.Nom;
                else
                    lbGrosEcart.Content = "";


                

                Palmares(c);

            }
        }

        private void LbTours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tour t = lbTours.SelectedItem as Tour;
            if (t as TourChampionnat != null)
                Classement(t as TourChampionnat);
            else if (t as TourPoules != null)
                Classement(t as TourPoules);
            else
                dgClassement.Items.Clear();
            if (t != null)
            {
                Calendrier(t);
                Buteurs(t);
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgMatchs.SelectedItem != null)
            {
                CalendrierElement selected = (CalendrierElement)dgMatchs.SelectedItem;
                Windows_Match match = new Windows_Match(selected.Match);
                match.Show();
            }
            
        }

        private void DgClassement_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgClassement.SelectedItem != null)
            {
                ClassementElement selected = (ClassementElement)dgClassement.SelectedItem;
                if (selected.Club as Club_Ville != null)
                {
                    Windows_Club club = new Windows_Club(selected.Club as Club_Ville);
                    club.Show();
                }
                   
            }
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            Windows_Options wo = new Windows_Options();
            wo.Show();
        }

        private void ComboPays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ILocalisation localisation = comboPays.SelectedItem as ILocalisation;
            ListerCompetitions(localisation);

        }

        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            Avancer();
            while (!(_partie.Date.Month == 6 && _partie.Date.Day == 13))
            {
                Avancer();
            }
        }

        private void BtnSimuler2_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i<10;i++)
            {
                _partie.Avancer();
                while (!(_partie.Date.Month == 6 && _partie.Date.Day == 13))
                {
                    _partie.Avancer();
                }
                Refresh();
            }
            Refresh();

        }

        private void DgButeurs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgButeurs.SelectedItem != null)
            {
                ButeurElement je = (ButeurElement)dgButeurs.SelectedItem;
                Windows_Joueur wj = new Windows_Joueur(je.Buteur);
                wj.Show();
            }
        }

        private void ListerCompetitions(ILocalisation localisation)
        {
            if(localisation != null)
            {
                lbChampionnats.Items.Clear();
                lbTours.Items.Clear();
                foreach (Competition c in localisation.Competitions())
                {
                    this.lbChampionnats.Items.Add(c);
                }
            }
        }

        private void Refresh()
        {
            this.labelDate.Content = _partie.Date.ToLongDateString();
        }

        private void Calendrier(Tour t)
        {
            dgMatchs.Items.Clear();
            List<Match> matchs = new List<Match>(t.Matchs);
            matchs.Sort(new Match_Date_Comparator());
            DateTime lastTime = new DateTime(2000, 1, 1);
            TourElimination te = t as TourElimination;
            foreach (Match m in matchs)
            {
                if (lastTime != m.Jour.Date)
                {
                    dgMatchs.Items.Add(new CalendrierElement { Heure=m.Jour.ToShortDateString()});
                }
                lastTime = m.Jour.Date;
                string score = "A jouer";
                string affluence = "-";
                if (m.Joue)
                {
                    score = m.Score1 + " - " + m.Score2;
                    affluence = m.Affluence.ToString();
                    if (m.Prolongations) score += " ap";
                    if (m.TAB) score += " (" + m.Tab1 + "-" + m.Tab2 + " tab)";
                }
                string equipe1 = m.Domicile.NomCourt;
                string equipe2 = m.Exterieur.NomCourt;

                Competition champD = m.Domicile.Championnat;
                Competition champE = m.Exterieur.Championnat;
                if (te != null && champD != null && champE != null)
                {
                    equipe1 += " (" + champD.NomCourt + ")";
                    equipe2 += " (" + champE.NomCourt + ")";
                }
                dgMatchs.Items.Add(new CalendrierElement { Heure = m.Jour.ToShortTimeString(), Equipe1 = equipe1, Equipe2 = equipe2, Score = score, Affluence = affluence, Match = m, Cote1 = m.Cote1.ToString("0.00"), Cote2 = m.Cote2.ToString("0.00"), CoteN = m.CoteN.ToString("0.00") });
            }
        }

        private void Classement(TourPoules t)
        {
            dgClassement.Items.Clear();
            for(int poules = 0;poules < t.NombrePoules; poules++)
            {
                List<Club> poule = new List<Club>(t.Poules[poules]);
                poule.Sort(new Club_Classement_Comparator(t.Matchs));
                int i = 0;
                dgClassement.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                dgClassement.Items.Add(new ClassementElement { Classement = i, Nom = "Poule " + (int)(poules + 1) });
                dgClassement.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                foreach (Club c in poule)
                {
                    i++;
                    dgClassement.Items.Add(new ClassementElement { Logo = System.IO.Directory.GetCurrentDirectory() + "\\Output\\Logos\\" + c.Logo + ".png", Classement = i, Nom = c.NomCourt, Pts = t.Points(c), J = t.Joues(c), G = t.Gagnes(c), N = t.Nuls(c), P = t.Perdus(c), bp = t.ButsPour(c), bc = t.ButsContre(c), Diff = t.Difference(c) });
                    }
            }
        }

        private void Classement(TourChampionnat t)
        {
            dgClassement.Items.Clear();
            int i = 0;
            foreach(Club c in t.Classement())
            {
                i++;
                dgClassement.Items.Add(new ClassementElement { Logo = System.IO.Directory.GetCurrentDirectory() + "\\Output\\Logos\\" + c.Logo + ".png", Club =c, Classement = i, Nom = c.NomCourt, Pts = t.Points(c), J = t.Joues(c), G = t.Gagnes(c), N = t.Nuls(c), P = t.Perdus(c), bp = t.ButsPour(c), bc = t.ButsContre(c), Diff = t.Difference(c)});
            }
            Style s = new Style();
            /*s.Setters.Add(new Setter(){ Property = Control.HeightProperty, Value = height });
            s.Setters.Add(new Setter() { Property = Control.FontSizeProperty, Value = 12 });
            s.Setters.Add(new Setter() { Property = Control.BorderThicknessProperty, Value = 1 });*/
            
            s.Setters.Add(new Setter() { Property = Control.BackgroundProperty, Value = App.Current.TryFindResource("color2") as SolidColorBrush });
            s.Setters.Add(new Setter() { Property = Control.ForegroundProperty, Value = App.Current.TryFindResource("color2") as SolidColorBrush });

            
            //Pour chaque couleur
            foreach(Qualification q in t.Qualifications)
            {
                if(q.Competition.Championnat)
                {
                    string couleur = "backgroundColor";
                    if (q.Competition.Niveau < (lbChampionnats.SelectedItem as Competition).Niveau)
                        couleur = "promotionColor";
                    else if (q.Competition.Niveau > (lbChampionnats.SelectedItem as Competition).Niveau)
                        couleur = "relegationColor";
                    else if (q.Competition.Niveau == (lbChampionnats.SelectedItem as Competition).Niveau && q.IDTour > (lbChampionnats.SelectedItem as Competition).Tours.IndexOf(t))
                        couleur = "barrageColor";

                    DataTrigger tg = new DataTrigger()
                    {
                        Binding = new Binding("Classement"),
                        Value = q.Classement
                    };
                    tg.Setters.Add(new Setter()
                    {
                        Property = Control.BackgroundProperty,
                        Value = App.Current.TryFindResource(couleur) as SolidColorBrush
                    });
                    s.Triggers.Add(tg);

                    dgClassement.CellStyle = s;
                }
                
            }
        }

        private void Buteurs(Tour t)
        {
            dgButeurs.Items.Clear();
            foreach(KeyValuePair<Joueur,int> buteur in t.Buteurs())
            {
                dgButeurs.Items.Add(new ButeurElement { Buteur = buteur.Key, Club = buteur.Key.Club == null ? buteur.Key.Nationalite.Nom() : buteur.Key.Club.NomCourt, NbButs = buteur.Value });
            }
        }

        private void Palmares(Competition c)
        {
            dgPalmares.Items.Clear();
            foreach (Competition arc in c.EditionsPrecedentes)
            {
                Club vainqueur = arc.Vainqueur();

                Tour t = arc.Tours[arc.Tours.Count - 1];
                //Si le tour final n'est pas un tour inactif, on peut établir le palmarès
                if (t.Matchs.Count > 0)
                {
                    int annee = t.Matchs[t.Matchs.Count - 1].Jour.Year;
                    dgPalmares.Items.Add(new PalmaresElement { Annee = annee, Club = vainqueur.Nom });
                }
            }
        }

        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                Session.Instance.Partie.Sauvegarder(saveFileDialog.FileName);
            }
        }

        private void BtnParticipants_Click(object sender, RoutedEventArgs e)
        {
            Competition c = lbChampionnats.SelectedItem as Competition;
            if (c != null)
            {
                Windows_Participants participants = new Windows_Participants(c);
                participants.Show();
            }
        }
    }

    public struct PalmaresElement
    {
        public int Annee { get; set; }
        public string Club { get; set; }
    }

    

    public struct ButeurElement
    {
        public Joueur Buteur { get; set; }
        public int NbButs { get; set; }
        public string Club { get; set; }
    }

    public struct CalendrierElement
    {
        public Match Match { get; set; }
        public string Heure { get; set; }
        public string Equipe1 { get; set; }
        public string Equipe2 { get; set; }
        public string Score { get; set; }
        public string Affluence { get; set; }
        public string Cote1 { get; set; }
        public string CoteN { get; set; }
        public string Cote2 { get; set; }
    }

    public struct ClassementElement
    {
        public string Logo { get; set; }
        public int Classement { get; set; }
        public string Nom { get; set; }
        public int Pts { get; set; }
        public int J { get; set; }
        public int G { get; set; }
        public int N { get; set; }
        public int P { get; set; }
        public int bp { get; set; }
        public int bc { get; set; }
        public int Diff { get; set; }
        public Club Club { get; set; }
    }
}