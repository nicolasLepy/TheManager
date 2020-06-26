using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Windows;
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
                Club precedant = null;
                int arrivee = joueur.history[0].Year;
                foreach (PlayerHistory hj in joueur.history)
                {
                    if (precedant == null) precedant = hj.Club;
                    else if (precedant != hj.Club)
                    {
                        int depart = hj.Year;

                        dgHistorique.Items.Add(new JoueurHistoriqueElement { Club = precedant, AnneeA = arrivee, AnneeD = depart });

                        arrivee = hj.Year;
                    }
                    precedant = hj.Club;
                }
                dgHistorique.Items.Add(new JoueurHistoriqueElement { Club = precedant, AnneeA = arrivee, AnneeD = joueur.history[joueur.history.Count-1].Year });

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
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public struct JoueurHistoriqueElement : IEquatable<JoueurHistoriqueElement>
    {
        public int AnneeA { get; set; }
        public int AnneeD { get; set; }
        public Club Club { get; set; }
        public bool Equals(JoueurHistoriqueElement other)
        {
            throw new NotImplementedException();
        }
    }
}
