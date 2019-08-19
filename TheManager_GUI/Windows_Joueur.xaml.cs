﻿using LiveCharts;
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
        public SeriesCollection ButsCollection { get; set; }
        public SeriesCollection MatchsCollection { get; set; }
        public string[] LabelsAnnees { get; set; }

        public Windows_Joueur(Joueur joueur)
        {
            InitializeComponent();
            lbJoueur.Content = joueur.ToString();
            lbAge.Content = joueur.Age + " ans";

            if(joueur.Historique.Count > 0)
            {
                Club precedant = null;
                int arrivee = joueur.Historique[0].Annee;
                foreach (HistoriqueJoueur hj in joueur.Historique)
                {
                    if (precedant == null) precedant = hj.Club;
                    else if (precedant != hj.Club)
                    {
                        int depart = hj.Annee;

                        dgHistorique.Items.Add(new JoueurHistoriqueElement { Club = precedant, AnneeA = arrivee, AnneeD = depart });

                        arrivee = hj.Annee;
                    }
                    precedant = hj.Club;
                }
                dgHistorique.Items.Add(new JoueurHistoriqueElement { Club = precedant, AnneeA = arrivee, AnneeD = joueur.Historique[joueur.Historique.Count-1].Annee });

            }

            ChartValues<int> niveaux = new ChartValues<int>();
            ChartValues<int> buts = new ChartValues<int>();
            ChartValues<int> joues = new ChartValues<int>();
            foreach (HistoriqueJoueur hj in joueur.Historique)
            {
                niveaux.Add(hj.Niveau);
                buts.Add(hj.Buts);
                joues.Add(hj.MatchsJoues);
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

    public struct JoueurHistoriqueElement
    {
        public int AnneeA { get; set; }
        public int AnneeD { get; set; }
        public Club Club { get; set; }
    }
}