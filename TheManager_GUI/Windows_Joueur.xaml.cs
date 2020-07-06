using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Joueur.xaml
    /// </summary>
    public partial class Windows_Joueur : Window
    {

        public SeriesCollection NiveauCollection { get; set; }
        public SeriesCollection ButsCollection { get; set; }
        public SeriesCollection MatchsCollection { get; set; }
        public string[] LabelsAnnees { get; set; }

        public Windows_Joueur(Player joueur)
        {
            InitializeComponent();
            lbJoueur.Content = joueur.ToString();
            lbAge.Content = joueur.Age + " ans";

            if(joueur.history.Count > 0)
            {
                /*
                Club precedant = null;
                int arrivee = joueur.history[0].Year;
                foreach (PlayerHistory hj in joueur.history)
                {
                    if (precedant == null)
                    {
                        precedant = hj.Club;
                    }
                    else if (precedant != hj.Club)
                    {
                        int depart = hj.Year;

                        dgHistorique.Items.Add(new JoueurHistoriqueElement { Club = precedant, AnneeA = arrivee, AnneeD = depart });

                        arrivee = hj.Year;
                    }
                    precedant = hj.Club;
                }
                dgHistorique.Items.Add(new JoueurHistoriqueElement { Club = precedant, AnneeA = arrivee, AnneeD = joueur.history[joueur.history.Count-1].Year });
                */
            }

            ChartValues<int> niveaux = new ChartValues<int>();
            ChartValues<int> buts = new ChartValues<int>();
            ChartValues<int> joues = new ChartValues<int>();
            foreach (PlayerHistory hj in joueur.history)
            {
                niveaux.Add(hj.Level);
                buts.Add(hj.Goals);
                joues.Add(hj.GamesPlayed);
            }

            NiveauCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Niveau",
                    Values = niveaux,
                }
            };

            ButsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Buts",
                    Values = buts,
                }
            };

            MatchsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Matchs",
                    Values = joues,
                }
            };

            LabelsAnnees = new string[joueur.history.Count];
            int i = 0;
            foreach (PlayerHistory hj in joueur.history)
            {
                LabelsAnnees[i] = hj.Year.ToString();
                i++;
            }
 
            DataContext = this;

            FillPlayerHistory(joueur);
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FillPlayerHistory(Player player)
        {

            StackPanel firstLine = new StackPanel();
            firstLine.Orientation = Orientation.Horizontal;
            firstLine.Children.Add(ViewUtils.CreateLabel("Durée", "StyleLabel2", 11, 80));
            firstLine.Children.Add(ViewUtils.CreateLabel("Club", "StyleLabel2", 11, 100));
            firstLine.Children.Add(ViewUtils.CreateLabel("Matchs", "StyleLabel2", 11, 40));
            firstLine.Children.Add(ViewUtils.CreateLabel("Buts", "StyleLabel2", 11, 40));
            spPlayerHistory.Children.Add(firstLine);

            int cumulativeGoals = 0;
            int cumulativeMatchesPlayed = 0;

            if(player.history.Count > 0)
            {
                Club last = null;
                int arrival = player.history[0].Year;
                foreach (PlayerHistory hj in player.history)
                {
                    if (last == null)
                    {
                        last = hj.Club;
                    }
                    else if (last != hj.Club)
                    {
                        int depart = hj.Year;

                        StackPanel line = new StackPanel();
                        line.Orientation = Orientation.Horizontal;
                        line.Children.Add(ViewUtils.CreateLabel((arrival - 1).ToString() + " - " + player.history[player.history.Count - 1].Year.ToString(), "StyleLabel2", 11, 80));
                        line.Children.Add(ViewUtils.CreateLabel(last.name, "StyleLabel2", 11, 100));
                        line.Children.Add(ViewUtils.CreateLabel(cumulativeMatchesPlayed.ToString(), "StyleLabel2", 11, 40));
                        line.Children.Add(ViewUtils.CreateLabel(cumulativeGoals.ToString(), "StyleLabel2", 11, 40));
                        spPlayerHistory.Children.Add(line);

                        cumulativeGoals = 0;
                        cumulativeMatchesPlayed = 0;
                        arrival = hj.Year;
                    }
                    cumulativeGoals += hj.Goals;
                    cumulativeMatchesPlayed += hj.GamesPlayed;
                    last = hj.Club;
                }

                StackPanel lastLine = new StackPanel();
                lastLine.Orientation = Orientation.Horizontal;
                lastLine.Children.Add(ViewUtils.CreateLabel((arrival-1).ToString() + " - " + player.history[player.history.Count - 1].Year.ToString(), "StyleLabel2", 11, 80));
                lastLine.Children.Add(ViewUtils.CreateLabel(last.name, "StyleLabel2", 11, 100));
                lastLine.Children.Add(ViewUtils.CreateLabel(cumulativeMatchesPlayed.ToString(), "StyleLabel2", 11, 40));
                lastLine.Children.Add(ViewUtils.CreateLabel(cumulativeGoals.ToString(), "StyleLabel2", 11, 40));
                spPlayerHistory.Children.Add(lastLine);
            }
        }
    }

}
