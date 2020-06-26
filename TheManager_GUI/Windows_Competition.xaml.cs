using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            List<Match> res = _competition.rounds[_indexTour].matches;
            ChampionshipRound tc = _competition.rounds[_indexTour] as ChampionshipRound;
            if(tc != null)
            {
                res = tc.GamesDay(_indexJournee);
            }
            return res;
        }

        private void InitWidgets()
        {
            lbCompetition.Content = _competition.name;
            lbNomTour.Content = _competition.rounds[_indexTour].name;
            dgButeurs.Items.Clear();


            if (_competition.statistics.LargerScore != null)
            {
                lbGrandScore.Content = _competition.statistics.LargerScore.home.name + " " + _competition.statistics.LargerScore.score1 + "-" + _competition.statistics.LargerScore.score2 + " " + _competition.statistics.LargerScore.away.name;
            }
            else
            {
                lbGrandScore.Content = "";
            }

            if (_competition.statistics.LargerScore != null)
            {
                lbGrosEcart.Content = _competition.statistics.LargerScore.home.name + " " + _competition.statistics.LargerScore.score1 + "-" + _competition.statistics.LargerScore.score2 + " " + _competition.statistics.LargerScore.away.name;
            }
            else
            {
                lbGrosEcart.Content = "";
            }
            Palmares();
            Buteurs();
            Calendrier(_competition.rounds[_indexTour]);
            IVueClassement vue = FabriqueVueClassement.CreerVue(null, _competition.rounds[_indexTour]);
            vue.Remplir(spClassement);

            int nbRegles = 0;
            foreach (Rule r in _competition.rounds[_indexTour].rules)
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
                dgButeurs.Items.Add(new ButeurElement { Buteur = buteur.Key, Club = buteur.Key.Club == null ? buteur.Key.nationality.Name() : Utils.Logo(buteur.Key.Club), NbButs = buteur.Value });
            }
        }


        private void Calendrier(Round t)
        {
            spMatchs.Children.Clear();

            List<Match> matchs = Journee();
            matchs.Sort(new MatchDateComparator());
            DateTime lastTime = new DateTime(2000, 1, 1);
            KnockoutRound te = t as KnockoutRound;
            int i = 0;
            foreach (Match m in matchs)
            {
                //Nouveau jour
                if (lastTime != m.day.Date)
                {

                    StackPanel spJour = new StackPanel();
                    spJour.Orientation = Orientation.Horizontal;
                    spJour.HorizontalAlignment = HorizontalAlignment.Left;

                    Label labelJour = new Label();
                    labelJour.Content = m.day.ToShortDateString();
                    labelJour.Style = Application.Current.FindResource("StyleLabel1") as Style;

                    spJour.Children.Add(labelJour);
                    spMatchs.Children.Add(spJour);
                }

                lastTime = m.day.Date;
                string score = "A jouer";
                string scoreMt = "";
                string affluence = "-";
                if (m.Played)
                {
                    score = m.score1 + " - " + m.score2;
                    affluence = m.attendance.ToString();
                    if (m.prolongations)
                    {
                        score += " ap";
                    }
                    if (m.PenaltyShootout)
                    {
                        score += " (" + m.penaltyShootout1 + "-" + m.penaltyShootout2 + " tab)";
                    }
                    scoreMt = "(" + m.ScoreHalfTime1 + " - " + m.ScoreHalfTime2 + ")";
                }
                string equipe1 = m.home.shortName;
                string equipe2 = m.away.shortName;

                Tournament champD = m.home.Championship;
                Tournament champE = m.away.Championship;
                if (te != null && champD != null && champE != null)
                {
                    equipe1 += " (" + champD.shortName + ")";
                    equipe2 += " (" + champE.shortName + ")";
                }

                StackPanel spHeureMatch = new StackPanel();
                spHeureMatch.Orientation = Orientation.Horizontal;
                spHeureMatch.HorizontalAlignment = HorizontalAlignment.Center;

                Label labelHeure = new Label();
                labelHeure.Content = m.day.ToShortTimeString();
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


                Round t = arc.rounds[arc.rounds.Count - 1];
                //If the final round was not inactive, we can make the palmares
                if (t.matches.Count > 0)
                {
                    int annee = t.matches[t.matches.Count - 1].day.Year;
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
            if (_indexTour < _competition.rounds.Count - 1)
            {
                _indexTour++;
            }
            InitWidgets();
        }

        private void BtnTourGauche_Click(object sender, RoutedEventArgs e)
        {
            if (_indexTour > 0)
            {
                _indexTour--;
            }
            InitWidgets();
        }

        private void BtnJourneeGauche_Click(object sender, RoutedEventArgs e)
        {
            if (_indexJournee > 1)
            {
                _indexJournee--;
            }
            InitWidgets();
        }

        private void BtnJourneeDroite_Click(object sender, RoutedEventArgs e)
        {
            ChampionshipRound tc = _competition.rounds[_indexTour] as ChampionshipRound;
            if (tc != null)
            {
                if (_indexJournee < tc.MatchesDayNumber())
                {
                    _indexJournee++;
                }
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