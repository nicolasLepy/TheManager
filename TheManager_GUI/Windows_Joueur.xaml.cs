using LiveCharts;
using LiveCharts.Wpf;
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

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Joueur.xaml
    /// </summary>
    public partial class Windows_Joueur : Window
    {

        public SeriesCollection NiveauCollection { get; set; }
        public string[] LabelsAnnees { get; set; }

        public Windows_Joueur(Joueur joueur)
        {
            InitializeComponent();
            lbJoueur.Content = joueur.ToString();
            lbAge.Content = joueur.Age + " ans";

            ChartValues<int> niveaux = new ChartValues<int>();
            foreach (HistoriqueJoueur hj in joueur.Historique)
            {
                niveaux.Add(hj.Niveau);
            }

            NiveauCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Niveau",
                    Values = niveaux,
                }
            };

            LabelsAnnees = new string[joueur.Historique.Count];
            int i = 0;
            foreach (HistoriqueJoueur hj in joueur.Historique)
            {
                LabelsAnnees[i] = hj.Annee.ToString();
                i++;
            }
 
            DataContext = this;
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
