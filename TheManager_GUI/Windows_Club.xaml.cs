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
using TheManager.Comparators;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Club.xaml
    /// </summary>
    public partial class Windows_Club : Window
    {

        public SeriesCollection BudgetsCollection { get; set; }
        public SeriesCollection CFCollection { get; set; }
        public string[] LabelsAnnees { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public Windows_Club(Club_Ville c)
        {
            InitializeComponent();
            lbClub.Content = c.Nom;

            List<Match> matchs = new List<Match>();
            foreach(Match m in Session.Instance.Partie.Gestionnaire.Matchs)
            {
                if (m.Domicile == c || m.Exterieur == c) matchs.Add(m);
            }

            matchs.Sort(new Match_Date_Comparator());

            int j = -1;
            for(int index = matchs.Count-1; index >= 0; index--, j++)
            {
                Match m = matchs[index];
                int couleur = 0;
                if (m.Domicile == c)
                {
                    if (m.Score1 > m.Score2) couleur = 2;
                    else if (m.Score2 > m.Score1) couleur = 0;
                }
                else if (m.Exterieur == c)
                {
                    if (m.Score2 > m.Score1) couleur = 2;
                    else if (m.Score1 > m.Score2) couleur = 0;
                }
                if (m.Score1 == m.Score2)
                    couleur = 1;

                dgMatchs.Items.Add(new MatchClubElement { Couleur=couleur, Match=m, Competition="", Date=m.Jour.ToShortDateString(), Equipe1=m.Domicile.Nom, Equipe2=m.Exterieur.Nom, Score = m.Score1 + " - " + m.Score2 + (m.Prolongations ? " ap" : "") + (m.TAB ? " (" + m.Tab1 + "-" + m.Tab2 +" tab)" : "" ) });
                
            }

            foreach (Contrat ct in c.Contrats)
            {
                dgJoueurs.Items.Add(new JoueurClubElement { Joueur=ct.Joueur , Age = ct.Joueur.Age, Contrat = ct.Fin.ToShortDateString(), Poste = ct.Joueur.Poste.ToString(), Nom = ct.Joueur.ToString(), Niveau = ct.Joueur.Niveau, Potentiel = ct.Joueur.Potentiel, Salaire = ct.Salaire + " €", DebutContrat = ct.Debut.ToShortDateString()});
                if((ct.Debut.Year == Session.Instance.Partie.Date.Year-1 && ct.Debut.Month < 7) || (ct.Debut.Year == Session.Instance.Partie.Date.Year && ct.Debut.Month >= 7))
                    dgArrivees.Items.Add(new JoueurClubElement { Joueur = ct.Joueur, Nom = ct.Joueur.ToString(), Niveau = ct.Joueur.Niveau, Salaire = ct.Salaire + " €" });
            }

            Style s = new Style();
            s.Setters.Add(new Setter() { Property = Control.BackgroundProperty, Value = App.Current.TryFindResource("backgroundColor") as SolidColorBrush });
            s.Setters.Add(new Setter() { Property = Control.ForegroundProperty, Value = App.Current.TryFindResource("color2") as SolidColorBrush });

            DataTrigger tg = new DataTrigger()
            {
                Binding = new Binding("Couleur"),
                Value = 0
            };
            tg.Setters.Add(new Setter()
            {
                Property = Control.BackgroundProperty,
                Value = App.Current.TryFindResource("defaiteColor") as SolidColorBrush
            });
            s.Triggers.Add(tg);

            tg = new DataTrigger()
            {
                Binding = new Binding("Couleur"),
                Value = 1
            };
            tg.Setters.Add(new Setter()
            {
                Property = Control.BackgroundProperty,
                Value = App.Current.TryFindResource("nulColor") as SolidColorBrush
            });
            s.Triggers.Add(tg);

            tg = new DataTrigger()
            {
                Binding = new Binding("Couleur"),
                Value = 2
            };
            tg.Setters.Add(new Setter()
            {
                Property = Control.BackgroundProperty,
                Value = App.Current.TryFindResource("victoireColor") as SolidColorBrush
            });
            s.Triggers.Add(tg);

            dgMatchs.CellStyle = s;
            

            ChartValues<int> budgets = new ChartValues<int>();
            ChartValues<int> centreFormation = new ChartValues<int>();
            foreach (EntreeHistorique eh in c.Historique.Elements)
            {
                budgets.Add(eh.Budget);
                centreFormation.Add(eh.CentreFormation);
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

            LabelsAnnees = new string[c.Historique.Elements.Count];
            int i = 0;
            foreach(EntreeHistorique eh in c.Historique.Elements)
            {
                LabelsAnnees[i] = c.Historique.Elements[i].Date.Year.ToString();
                i++;
            }
            YFormatter = value => value.ToString("C");
            
            DataContext = this;
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgMatchs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgMatchs.SelectedItem != null)
            {
                MatchClubElement mce = (MatchClubElement)dgMatchs.SelectedItem;
                Windows_Match wm = new Windows_Match(mce.Match);
                wm.Show();
            }
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
    }

    public struct MatchClubElement
    {
        public string Date { get; set; }
        public string Equipe1 { get; set; }
        public string Score { get; set; }
        public string Equipe2 { get; set; }
        public string Competition { get; set; }
        public Match Match { get; set; }
        public int Couleur { get; set; }
    }

    public struct JoueurClubElement
    {
        public Joueur Joueur { get; set; }
        public string Nom { get; set; }
        public int Age { get; set; }
        public string Poste { get; set; }
        public string Contrat { get; set; }
        public string Salaire { get; set; }
        public int Niveau { get; set; }
        public int Potentiel { get; set; }
        public string DebutContrat { get; set; }
    }
}
