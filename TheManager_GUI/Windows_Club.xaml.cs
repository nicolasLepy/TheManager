using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;

namespace TheManager_GUI
{

    public class HistoriqueClubComparator : IComparer<HistoriqueClubElement>
    {
        public int Compare(HistoriqueClubElement x, HistoriqueClubElement y)
        {
            return y.Annee - x.Annee;
        }
    }

    /// <summary>
    /// Logique d'interaction pour Windows_Club.xaml
    /// </summary>
    public partial class Windows_Club : Window
    {

        public SeriesCollection BudgetsCollection { get; set; }
        public SeriesCollection CFCollection { get; set; }
        public string[] LabelsAnnees { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private Club _club;

        public void RemplirMatchs()
        {
            List<Match> matchs = _club.Games;
            int j = -1;
            for (int index = matchs.Count - 1; index >= 0; index--, j++)
            {
                Match m = matchs[index];

                StackPanel spMatch = new StackPanel();
                spMatch.Orientation = Orientation.Horizontal;

                int couleur = 0;
                if (m.home == _club)
                {
                    if (m.score1 > m.score2)
                    {
                        couleur = 2;
                    }
                    else if (m.score2 > m.score1)
                    {
                        couleur = 0;
                    }
                }
                else if (m.away == _club)
                {
                    if (m.score2 > m.score1)
                    {
                        couleur = 2;
                    }
                    else if (m.score1 > m.score2)
                    {
                        couleur = 0;
                    }
                }

                if (m.score1 == m.score2)
                {
                    couleur = 1;
                }

                string fontStyle = "victoireColor";
                switch (couleur)
                {
                    case 0: fontStyle = "defaiteColor"; break;
                    case 1: fontStyle = "nulColor"; break;
                    case 2: fontStyle = "victoireColor"; break;
                }

                SolidColorBrush color = Application.Current.TryFindResource(fontStyle) as SolidColorBrush;
                spMatch.Background = color;


                Label l1 = new Label();
                l1.Content = m.Tournament.name;
                l1.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l1.FontSize = 10;
                l1.Width = 150;

                Label l2 = new Label();
                l2.Content = m.day.ToShortDateString();
                l2.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l2.FontSize = 10;
                l2.Width = 75;

                Label l3 = new Label();
                l3.Content = m.home.name;
                l3.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l3.FontSize = 10;
                l3.Width = 100;

                Button btnScore = new Button();
                btnScore.Name = "btnScore_" + index;
                btnScore.Click += new RoutedEventHandler(BtnMatch_Click);
                btnScore.Content = m.score1 + " - " + m.score2 + (m.prolongations ? " ap" : "") + (m.PenaltyShootout ? " (" + m.penaltyShootout1 + "-" + m.penaltyShootout2 + " tab)" : "");
                btnScore.Style = Application.Current.FindResource("StyleButtonLabel") as Style;
                btnScore.FontSize = 10;
                btnScore.Width = 50;

                Label l5 = new Label();
                l5.Content = m.away.name;
                l5.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l5.FontSize = 10;
                l5.Width = 100;

                spMatch.Children.Add(l1);
                spMatch.Children.Add(l2);
                spMatch.Children.Add(l3);
                spMatch.Children.Add(btnScore);
                spMatch.Children.Add(l5);

                spMatchs.Children.Add(spMatch);
            }
        }

        public void Palmares(CityClub club)
        {
            foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                int nombre = 0;
                string annees = "";
                foreach(Tournament archive in c.previousEditions)
                {
                    if(archive.isChampionship)
                    {
                        if (archive.rounds[0].Winner() == club)
                        {
                            nombre++;
                        }
                    }
                    else
                    {
                        Round t = archive.rounds[archive.rounds.Count - 1];
                        if (t.Winner() == club)
                        {
                            nombre++;
                        }

                    }
                }
                if(nombre > 0)
                {
                    dgPalmares.Items.Add(new PalmaresClubElement { Annees = annees, Competition = c, Nombre = nombre});
                }
            }
        }

        public Windows_Club(CityClub c)
        {
            InitializeComponent();
            _club = c;
            lbClub.Content = c.name;

            if(c.manager != null)
            {
                lbEntraineur.Content = "Entraîneur : " + c.manager.ToString();
            }
            else
            {
                lbEntraineur.Content = "Aucun entraîneur";
            }

            try
            {
                imgLogo.Source = new BitmapImage(new Uri(Utils.Logo(c)));
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Palmares(c);
            RemplirMatchs();

            
            foreach (Contract ct in c.contracts)
            {
                dgJoueurs.Items.Add(new JoueurClubElement { Joueur=ct.player , Age = ct.player.Age, Contrat = ct.end.ToShortDateString(), Poste = ct.player.position.ToString(), Nom = ct.player.ToString(), Niveau = ct.player.level, Potentiel = ct.player.potential, Salaire = ct.wage + " €", DebutContrat = ct.beginning.ToShortDateString(), Energie = ct.player.energy});
                if ((ct.beginning.Year == Session.Instance.Game.date.Year - 1 && ct.beginning.Month < 7) ||
                    (ct.beginning.Year == Session.Instance.Game.date.Year && ct.beginning.Month >= 7))
                {
                    dgArrivees.Items.Add(new JoueurClubElement { Joueur = ct.player, Nom = ct.player.ToString(), Niveau = ct.player.level, Salaire = ct.wage + " €" });
                }
            }

            List<HistoriqueClubElement> lhce = new List<HistoriqueClubElement>();
            foreach(Tournament competition in Session.Instance.Game.kernel.Competitions)
            {
                int annee = 2019;
                foreach(Tournament ancienne in competition.previousEditions)
                {
                    if(ancienne.isChampionship && ancienne.rounds[0].clubs.Contains(c))
                    {
                        int classement = 0;
                        //Si la compétition était active (tour 0 un tour de type championnat, pas inactif)
                        if((ancienne.rounds[0] as ChampionshipRound) != null)
                        {
                            classement = (ancienne.rounds[0] as ChampionshipRound).Ranking().IndexOf(c) + 1;
                        }
                        
                        //int annee = ancienne.Tours[0].Matchs[ancienne.Tours[0].Matchs.Count - 1].Jour.Year;
                        lhce.Add(new HistoriqueClubElement { Competition = ancienne, Classement = classement, Annee = annee });
                        annee++;
                    }
                }
            }
            lhce.Sort(new HistoriqueClubComparator());
            foreach(HistoriqueClubElement hce in lhce)
            {
                dgHistorique.Items.Add(hce);
            }

            
            ChartValues<int> budgets = new ChartValues<int>();
            ChartValues<int> centreFormation = new ChartValues<int>();
            foreach (HistoricEntry eh in c.history.elements)
            {
                budgets.Add(eh.budget);
                centreFormation.Add(eh.formationFacilities);
            }

            BudgetsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Budget",
                    Values = budgets,
                }
            };

            //Centre de formation
            
            CFCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Niveau",
                    Values = centreFormation,
                }
            };

            LabelsAnnees = new string[c.history.elements.Count];
            int i = 0;
            foreach(HistoricEntry eh in c.history.elements)
            {
                LabelsAnnees[i] = c.history.elements[i].date.Year.ToString();
                i++;
            }
            YFormatter = value => value.ToString("C");
            
            DataContext = this;
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgJoueurs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgJoueurs.SelectedItem != null)
            {
                JoueurClubElement jce = (JoueurClubElement)dgJoueurs.SelectedItem;
                Windows_Joueur wj = new Windows_Joueur(jce.Joueur);
                wj.Show();
            }
        }

        private void DgHistorique_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgHistorique.SelectedItem != null)
            {
                HistoriqueClubElement hce = (HistoriqueClubElement)dgHistorique.SelectedItem;
                //Afficher une fenêtre compétition
            }
        }

        private void BtnMatch_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int idMatch = int.Parse(btn.Name.Split('_')[1]);
            Match match = _club.Games[idMatch];
            Windows_Match wm = new Windows_Match(match);
            wm.Show();

        }
    }

    public struct HistoriqueClubElement : IEquatable<HistoriqueClubElement>
    {
        public int Annee { get; set; }
        public Tournament Competition { get; set; }
        public int Classement { get; set; }
        public bool Equals(HistoriqueClubElement other)
        {
            throw new NotImplementedException();
        }
    }

    public struct JoueurClubElement : IEquatable<JoueurClubElement>
    {
        public Player Joueur { get; set; }
        public string Nom { get; set; }
        public int Age { get; set; }
        public string Poste { get; set; }
        public string Contrat { get; set; }
        public string Salaire { get; set; }
        public int Niveau { get; set; }
        public int Potentiel { get; set; }
        public string DebutContrat { get; set; }
        public int Energie { get; set; }
        public bool Equals(JoueurClubElement other)
        {
            throw new NotImplementedException();
        }
    }

    public struct PalmaresClubElement : IEquatable<PalmaresClubElement>
    {
        public Tournament Competition { get; set; }
        public int Nombre { get; set; }
        public string Annees { get; set; }
        public bool Equals(PalmaresClubElement other)
        {
            throw new NotImplementedException();
        }
    }
}
