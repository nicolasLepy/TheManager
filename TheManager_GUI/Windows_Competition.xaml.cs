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
using TheManager_GUI.VueClassement;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Competition.xaml
    /// </summary>
    public partial class Windows_Competition : Window
    {

        private Tournament _competition;
        private int _indexTour;
        private int _indexJournee;

        public Windows_Competition(Tournament competition)
        {
            InitializeComponent();
            _competition = competition;
            _indexTour = 0;
            _indexJournee = 1;
            InitWidgets();
        }

        private List<Match> Journee()
        {
            List<Match> res = _competition.rounds[_indexTour].Matchs;
            TourChampionnat tc = _competition.rounds[_indexTour] as TourChampionnat;
            if(tc != null)
            {
                res = tc.Journee(_indexJournee);
            }
            return res;
        }

        private void InitWidgets()
        {
            lbCompetition.Content = _competition.name;
            lbNomTour.Content = _competition.rounds[_indexTour].Nom;
            dgButeurs.Items.Clear();
            

            if (_competition.statistics.LargerScore != null)
                lbGrandScore.Content = _competition.statistics.LargerScore.Domicile.name + " " + _competition.statistics.LargerScore.Score1 + "-" + _competition.statistics.LargerScore.Score2 + " " + _competition.statistics.LargerScore.Exterieur.name;
            else
                lbGrandScore.Content = "";
            if (_competition.statistics.LargerScore != null)
                lbGrosEcart.Content = _competition.statistics.LargerScore.Domicile.name + " " + _competition.statistics.LargerScore.Score1 + "-" + _competition.statistics.LargerScore.Score2 + " " + _competition.statistics.LargerScore.Exterieur.name;
            else
                lbGrosEcart.Content = "";
            Palmares();
            Buteurs();
            Calendrier(_competition.rounds[_indexTour]);
            IVueClassement vue = FabriqueVueClassement.CreerVue(null, _competition.rounds[_indexTour]);
            vue.Remplir(spClassement);

            int nbRegles = 0;
            foreach (Rule r in _competition.rounds[_indexTour].Regles)
            {
                Label l = new Label();
                l.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l.Width = 20;
                l.Content = Utils.Rule2String(r);
                spBlocClassement.Children.Add(l);
                nbRegles++;
            }



        }

        private void Buteurs()
        {
            dgButeurs.Items.Clear();

            foreach (KeyValuePair<Player, int> buteur in _competition.Goalscorers())
            {
                dgButeurs.Items.Add(new ButeurElement { Buteur = buteur.Key, Club = buteur.Key.Club == null ? buteur.Key.Nationalite.Name() : Utils.Logo(buteur.Key.Club), NbButs = buteur.Value });
            }
        }


        private void Calendrier(Tour t)
        {
            spMatchs.Children.Clear();

            List<Match> matchs = Journee();
            matchs.Sort(new MatchDateComparator());
            DateTime lastTime = new DateTime(2000, 1, 1);
            TourElimination te = t as TourElimination;
            int i = 0;
            foreach (Match m in matchs)
            {
                //Nouveau jour
                if (lastTime != m.Jour.Date)
                {

                    StackPanel spJour = new StackPanel();
                    spJour.Orientation = Orientation.Horizontal;
                    spJour.HorizontalAlignment = HorizontalAlignment.Left;

                    Label labelJour = new Label();
                    labelJour.Content = m.Jour.ToShortDateString();
                    labelJour.Style = Application.Current.FindResource("StyleLabel1") as Style;

                    spJour.Children.Add(labelJour);
                    spMatchs.Children.Add(spJour);
                }

                lastTime = m.Jour.Date;
                string score = "A jouer";
                string scoreMt = "";
                string affluence = "-";
                if (m.Joue)
                {
                    score = m.Score1 + " - " + m.Score2;
                    affluence = m.Affluence.ToString();
                    if (m.Prolongations) score += " ap";
                    if (m.TAB) score += " (" + m.Tab1 + "-" + m.Tab2 + " tab)";
                    scoreMt = "(" + m.ScoreMT1 + " - " + m.ScoreMT2 + ")";
                }
                string equipe1 = m.Domicile.shortName;
                string equipe2 = m.Exterieur.shortName;

                Tournament champD = m.Domicile.Championship;
                Tournament champE = m.Exterieur.Championship;
                if (te != null && champD != null && champE != null)
                {
                    equipe1 += " (" + champD.shortName + ")";
                    equipe2 += " (" + champE.shortName + ")";
                }

                StackPanel spHeureMatch = new StackPanel();
                spHeureMatch.Orientation = Orientation.Horizontal;
                spHeureMatch.HorizontalAlignment = HorizontalAlignment.Center;

                Label labelHeure = new Label();
                labelHeure.Content = m.Jour.ToShortTimeString();
                labelHeure.HorizontalContentAlignment = HorizontalAlignment.Center;
                labelHeure.Width = 50;
                labelHeure.Style = Application.Current.FindResource("StyleLabel2") as Style;
                labelHeure.FontWeight = FontWeights.Bold;
                labelHeure.FontSize = 13;

                spHeureMatch.Children.Add(labelHeure);

                StackPanel spCorps = new StackPanel();
                spCorps.Orientation = Orientation.Horizontal;

                Label lbEq1 = new Label();
                lbEq1.HorizontalContentAlignment = HorizontalAlignment.Left;
                lbEq1.Content = equipe1;
                lbEq1.Style = Application.Current.FindResource("StyleLabel2") as Style;
                lbEq1.FontSize = 16;
                lbEq1.Width = 200;

                Button btnScore = new Button();
                btnScore.Name = "btnScore_" + i;
                btnScore.Click += new RoutedEventHandler(BtnMatch_Click);
                btnScore.Content = score;
                btnScore.Style = Application.Current.FindResource("StyleButton1") as Style;
                btnScore.FontSize = 16;
                btnScore.Width = 50;

                Label lbEq2 = new Label();
                lbEq2.HorizontalContentAlignment = HorizontalAlignment.Right;
                lbEq2.Content = equipe2;
                lbEq2.Style = Application.Current.FindResource("StyleLabel2") as Style;
                lbEq2.FontSize = 16;
                lbEq2.Width = 200;

                spCorps.Children.Add(lbEq1);
                spCorps.Children.Add(btnScore);
                spCorps.Children.Add(lbEq2);

                StackPanel spMT = new StackPanel();
                spMT.Orientation = Orientation.Horizontal;
                spMT.HorizontalAlignment = HorizontalAlignment.Center;

                Label labelMT = new Label();
                labelMT.HorizontalContentAlignment = HorizontalAlignment.Center;
                labelMT.Content = scoreMt;
                labelMT.Style = Application.Current.FindResource("StyleLabel2") as Style;
                labelMT.Width = 50;
                labelMT.FontSize = 10;
                labelMT.FontStyle = FontStyles.Italic;

                spMT.Children.Add(labelMT);

                spMatchs.Children.Add(spHeureMatch);
                spMatchs.Children.Add(spCorps);
                spMatchs.Children.Add(spMT);

                i++;

            }
        }

        private void Palmares()
        {
            dgPalmares.Items.Clear();
            foreach (Tournament arc in _competition.previousEditions)
            {
                Club vainqueur = arc.Winner();


                Tour t = arc.rounds[arc.rounds.Count - 1];
                //If the final round was not inactive, we can make the palmares
                if (t.Matchs.Count > 0)
                {
                    int annee = t.Matchs[t.Matchs.Count - 1].Jour.Year;
                    dgPalmares.Items.Add(new PalmaresElement { Annee = annee, Club = vainqueur });
                }
            }
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void BtnTourDroite_Click(object sender, RoutedEventArgs e)
        {
            if (_indexTour < _competition.rounds.Count-1) _indexTour++;
            InitWidgets();
        }

        private void BtnTourGauche_Click(object sender, RoutedEventArgs e)
        {
            if (_indexTour > 0) _indexTour--;
            InitWidgets();
        }

        private void BtnJourneeGauche_Click(object sender, RoutedEventArgs e)
        {
            if (_indexJournee > 1) _indexJournee--;
            InitWidgets();
        }

        private void BtnJourneeDroite_Click(object sender, RoutedEventArgs e)
        {
            TourChampionnat tc = _competition.rounds[_indexTour] as TourChampionnat;
            if (tc != null)
            {
                if (_indexJournee < tc.NombreJournees()) _indexJournee++;
                InitWidgets();
            }
        }

        private void BtnMatch_Click(object sender, RoutedEventArgs e)
        {

            Button btn = sender as Button;
            int idMatch = int.Parse(btn.Name.Split('_')[1]);
            Match match = Journee()[idMatch];
            Windows_Match wm = new Windows_Match(match);
            wm.Show();
        }
    }
}