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

        private readonly Tournament _competition;
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
            IViewRanking vue = FactoryViewRanking.CreerVue(null, _competition.rounds[_indexTour]);
            vue.Full(spClassement);

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

                Label labelHeure = ViewUtils.CreateLabel(m.day.ToShortTimeString(), "StyleLabel2", 13, 50);
                labelHeure.HorizontalContentAlignment = HorizontalAlignment.Center;
                labelHeure.FontWeight = FontWeights.Bold;

                spHeureMatch.Children.Add(labelHeure);

                StackPanel spCorps = new StackPanel();
                spCorps.Orientation = Orientation.Horizontal;

                Label lbEq1 = ViewUtils.CreateLabel(equipe1, "StyleLabel2", 16, 200);
                lbEq1.HorizontalContentAlignment = HorizontalAlignment.Left;

                Button btnScore = new Button();
                btnScore.Click += (object sender, RoutedEventArgs e) =>
                { BtnMatchClick(m); };
                btnScore.Content = score;
                btnScore.Style = Application.Current.FindResource("StyleButton1") as Style;
                btnScore.FontSize = 16;
                btnScore.Width = 50;

                Label lbEq2 = ViewUtils.CreateLabel(equipe2, "StyleLabel2", 16, 200);
                lbEq2.HorizontalContentAlignment = HorizontalAlignment.Right;

                spCorps.Children.Add(lbEq1);
                spCorps.Children.Add(btnScore);
                spCorps.Children.Add(lbEq2);

                StackPanel spMT = new StackPanel();
                spMT.Orientation = Orientation.Horizontal;
                spMT.HorizontalAlignment = HorizontalAlignment.Center;

                Label labelMT = ViewUtils.CreateLabel(scoreMt, "StyleLabel2", 10, 50);
                labelMT.HorizontalContentAlignment = HorizontalAlignment.Center;
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
            foreach (KeyValuePair<int,Tournament> arc in _competition.previousEditions)
            {
                Club vainqueur = arc.Value.Winner();


                Round t = arc.Value.rounds[arc.Value.rounds.Count - 1];
                //If the final round was not inactive, we can make the palmares
                if (t.matches.Count > 0)
                {
                    int annee = arc.Key;
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

        private void BtnHistoric_Click(object sender, RoutedEventArgs e)
        {

            TournamentHistoryWindow thw = new TournamentHistoryWindow(_competition);
            thw.Show();
        }

        private void BtnMatchClick(Match m)
        {
            Windows_Match wm = new Windows_Match(m);
            wm.Show();
        }
    }
}