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

            Partie partie = new Partie();
            Session.Instance.Partie = partie;
            Gestionnaire g = partie.Gestionnaire;
            ChargeurBaseDeDonnees cbdd = new ChargeurBaseDeDonnees(g);
            cbdd.Charger();

            _partie = Session.Instance.Partie;
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

        private void BtnAvancer_Click(object sender, RoutedEventArgs e)
        {
            _partie.Avancer();
            Refresh();
            
            
        }

        private void LbChampionnats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Competition c = lbChampionnats.SelectedItem as Competition;
            if(c != null)
            {
                lbTours.Items.Clear();
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


                if(c.Championnat)
                {
                    dgClubs.Items.Clear();
                    foreach(Club cl in c.Tours[0].Clubs)
                    {
                        dgClubs.Items.Add(new ClubElement{ Nom = cl.NomCourt, Niveau = cl.Niveau() });
                    }
                }

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

        private void ListerCompetitions(ILocalisation localisation)
        {
            if(localisation != null)
            {
                lbChampionnats.Items.Clear();
                foreach (Competition c in localisation.Competitions())
                {
                    this.lbChampionnats.Items.Add(c);
                }
            }
        }

        private void Refresh()
        {
            this.labelDate.Content = _partie.Date.ToLongDateString();

            /*
            lbChampionnats.Items.Clear();
            foreach(Competition c in _partie.Gestionnaire.Competitions)
            {
                this.lbChampionnats.Items.Add(c);
            }*/

           

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
                dgMatchs.Items.Add(new CalendrierElement { Heure = m.Jour.ToShortTimeString(), Equipe1 = equipe1, Equipe2 = equipe2, Score = score, Affluence = affluence, Match = m });
            }
        }

        private void Classement(TourPoules t)
        {
            dgClassement.Items.Clear();
            for(int poules = 0;poules < t.NombrePoules; poules++)
            {
                List<Club> poule = new List<Club>(t.Poules[poules]);
                poule.Sort(new Club_Classement_Comparator(t));
                int i = 0;
                dgClassement.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                dgClassement.Items.Add(new ClassementElement { Classement = i, Nom = "Poule " + (int)(poules + 1) });
                dgClassement.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                foreach (Club c in poule)
                {
                    i++;
                    dgClassement.Items.Add(new ClassementElement { Classement = i, Nom = c.NomCourt, Pts = t.Points(c), J = t.Joues(c), G = t.Gagnes(c), N = t.Nuls(c), P = t.Perdus(c), bp = t.ButsPour(c), bc = t.ButsContre(c), Diff = t.Difference(c) });
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
                dgClassement.Items.Add(new ClassementElement { Classement = i, Nom = c.NomCourt, Pts = t.Points(c), J = t.Joues(c), G = t.Gagnes(c), N = t.Nuls(c), P = t.Perdus(c), bp = t.ButsPour(c), bc = t.ButsContre(c), Diff = t.Difference(c)});
            }
        }

        private void Buteurs(Tour t)
        {
            dgButeurs.Items.Clear();
            foreach(KeyValuePair<Joueur,int> buteur in t.Buteurs())
            {
                dgButeurs.Items.Add(new ButeurElement { Buteur = buteur.Key.Prenom + " " + buteur.Key.Nom, Club = buteur.Key.Club == null ? buteur.Key.Nationalite.Nom() : buteur.Key.Club.NomCourt, NbButs = buteur.Value });
            }
        }

        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            while (!(_partie.Date.Month == 6 && _partie.Date.Day == 13))
            {
                _partie.Avancer();
            }
            Refresh();
        }
    }

    public struct ClubElement
    {
        public string Nom { get; set; }
        public float Niveau { get; set; }
    }

    public struct ButeurElement
    {
        public string Buteur { get; set; }
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
    }

    public struct ClassementElement
    {
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
    }
}