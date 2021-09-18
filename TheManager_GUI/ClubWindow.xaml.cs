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
using TheManager_GUI.ViewMisc;

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
        public SeriesCollection AttendanceCollection { get; set; }
        public string[] LabelsAnnees { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private readonly Club _club;


        public void FillBudget()
        {
            CityClub cc = _club as CityClub;
            if(cc != null)
            {
                foreach(BudgetEntry be in cc.budgetHistory)
                {
                    StackPanel spEntry = new StackPanel();
                    spEntry.Orientation = Orientation.Horizontal;

                    spEntry.Children.Add(ViewUtils.CreateLabel(be.Date.ToShortDateString(), "StyleLabel2", 11, 70));
                    if(be.Amount < 0)
                    {
                        spEntry.Children.Add(ViewUtils.CreateLabel(Utils.FormatMoney(be.Amount), "StyleLabel2", 11, 75, Brushes.Red));
                    }
                    else
                    {
                        spEntry.Children.Add(ViewUtils.CreateLabel(Utils.FormatMoney(be.Amount), "StyleLabel2", 11, 75));
                    }

                    spEntry.Children.Add(ViewUtils.CreateLabel(Utils.GetDescription(be.Reason), "StyleLabel2", 10, 100));

                    spBudget.Children.Add(spEntry);
                }
            }
        }

        public void RemplirMatchs()
        {
            List<Match> matchs = _club.Games;
            ViewMatches view = new ViewMatches(matchs, true, true, false, true, false, true, 12, true, _club);
            view.Full(spMatchs);
        }

        public void Palmares(CityClub club)
        {
            foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                int nombre = 0;
                string annees = "";
                foreach(KeyValuePair<int,Tournament> archive in c.previousEditions)
                {
                    if(archive.Value.isChampionship)
                    {
                        if (archive.Value.rounds[0].Winner() == club)
                        {
                            nombre++;
                        }
                    }
                    else
                    {
                        Round t = archive.Value.rounds[archive.Value.rounds.Count - 1];
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

            imgBudget.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\budget.png"));
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            imgManager.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\manager.png"));

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

            lbBudget.Content = Utils.FormatMoney(c.budget);

            try
            {
                imgLogo.Source = new BitmapImage(new Uri(Utils.Logo(c)));
            }
            catch(Exception e)
            {
                Utils.Debug(e.ToString());
            }
            Palmares(c);
            RemplirMatchs();
            FillBudget();

            

            List<Player> newContracts = new List<Player>();
            
            foreach (Contract ct in c.allContracts)
            {
                if ((ct.beginning.Year == Session.Instance.Game.date.Year - 1 && ct.beginning.Month < 7) ||
                    (ct.beginning.Year == Session.Instance.Game.date.Year && ct.beginning.Month >= 7))
                {
                    newContracts.Add(ct.player);
                }
            }
            ViewPlayers viewNewPlayers = new ViewPlayers(newContracts, 11, false, false, false, true, false, false, false, false, false, true, false, false, false, false, false, false, false);
            viewNewPlayers.Full(spArrivees);
            ViewPlayers viewPlayers = new ViewPlayers(c.Players(), 12, true, true, true, true, true, false, false, true, false, true, false, false, false, false, false, true, true, true);
            viewPlayers.Full(spPlayers);

            List<HistoriqueClubElement> lhce = new List<HistoriqueClubElement>();
            foreach(Tournament competition in Session.Instance.Game.kernel.Competitions)
            {
                foreach(KeyValuePair<int,Tournament> ancienne in competition.previousEditions)
                {
                    if(ancienne.Value.isChampionship && ancienne.Value.rounds[0].clubs.Contains(c))
                    {
                        int classement = 0;
                        //Si la compétition était active (tour 0 un tour de type championnat, pas inactif)
                        if((ancienne.Value.rounds[0] as ChampionshipRound) != null)
                        {
                            classement = (ancienne.Value.rounds[0] as ChampionshipRound).Ranking().IndexOf(c) + 1;
                        }
                        else if ((ancienne.Value.rounds[0] as GroupsRound) != null)
                        {
                            GroupsRound rnd = (ancienne.Value.rounds[0] as GroupsRound);
                            for (int j = 0; j<rnd.groupsCount; j++)
                            {
                                if (rnd.groups[j].Contains(c))
                                {
                                    classement = rnd.Ranking(j).IndexOf(c);
                                }
                            }
                        }
                        lhce.Add(new HistoriqueClubElement { Competition = ancienne.Value, Classement = classement, Annee = ancienne.Key });
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
            ChartValues<int> attendance = new ChartValues<int>();
            foreach (HistoricEntry eh in c.history.elements)
            {
                budgets.Add(eh.budget);
                centreFormation.Add(eh.formationFacilities);
                attendance.Add(eh.averageAttendance);
            }

            BudgetsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Budget",
                    Values = budgets,
                }
            };

            //Formation facilities
            
            CFCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Niveau",
                    Values = centreFormation,
                }
            };

            //Average attendance

            AttendanceCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Affluence moyenne",
                    Values = attendance,
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

            if(c.records.BiggestWin != null)
            {
                lbBiggestWin.Content = c.records.BiggestWin.home.name + " " + c.records.BiggestWin.score1 + " - " + c.records.BiggestWin.score2 + " " + c.records.BiggestWin.away.name;
            }
            if (c.records.BiggestLose != null)
            {
                lbBiggestLose.Content = c.records.BiggestLose.home.name + " " + c.records.BiggestLose.score1 + " - " + c.records.BiggestLose.score2 + " " + c.records.BiggestLose.away.name;
            }
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgHistorique_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgHistorique.SelectedItem != null)
            {
                HistoriqueClubElement hce = (HistoriqueClubElement)dgHistorique.SelectedItem;
                if(hce.Competition != null)
                {
                    Windows_Competition wc = new Windows_Competition(hce.Competition);
                    wc.Show();
                }
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
