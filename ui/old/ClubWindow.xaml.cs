using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tm;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.views;

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

        private void AddValueToDictionnary(Dictionary<BudgetModificationReason, double> dictionnary, BudgetModificationReason key, double value)
        {
            if(!dictionnary.ContainsKey(key))
            {
                dictionnary.Add(key, 0);
            }
            dictionnary[key] += value;
        }

        private StackPanel CreateBudgetReportEntry(string name, float value, BudgetReportEntry budgetReportEntry)
        {
            StackPanel spEntry = new StackPanel();
            spEntry.Orientation = Orientation.Horizontal;
            spEntry.Children.Add(ViewUtils.CreateLabelOpenWindow<BudgetReportEntry>(budgetReportEntry, UpdateBudgetReportChartSelected, name, "StyleLabel2", 12, 400));
            spEntry.Children.Add(ViewUtils.CreateLabel(Utils.FormatMoney(value), "StyleLabel2", 12, 150, value < 0 ? Brushes.Red : Brushes.DarkGreen));
            return spEntry;
        }

        public void UpdateBudgetReportChartSelected(BudgetReportEntry budgetReportEntry)
        {
            CityClub cc = _club as CityClub;
            if(cc != null)
            {
                string title = budgetReportEntry.ToString();
                List<BudgetModificationReason> budgetEntries = cc.GetBudgetEntries(budgetReportEntry);
                spBudgetReportChart.Children.Clear();
                List<string> years = new List<string>();
                List<float> amount = new List<float>();
                Func<double, string> YFormatter = value => value.ToString("C");
                foreach (DateTime dt in GetHistoryEntriesDates())
                {
                    years.Add(dt.Year.ToString());
                    amount.Add(cc.GetBudgetOnYear(dt, budgetEntries));
                }
                ChartValues<float> amountValues = new ChartValues<float>(amount);
                float min = Math.Min(amount.Min(), 0);
                float max = Math.Max(amount.Max(), 0);
                if (min == 0 && max == 0)
                {
                    spBudgetReportChart.Children.Add(ViewUtils.CreateLabel("Aucune entrée enregistrée", "StyleLabel2", 14, 400));
                }
                else
                {
                    ViewUtils.CreateYearChart(spBudgetReportChart, years.ToArray(), title, amountValues, true, false, "Total", min, max, title, YFormatter, 0.75);
                }
            }
        }

        private void NewBudgetReportSelected(DateTime date)
        {
            CityClub cc = _club as CityClub;
            DateTime beginningDate = date.AddYears(-1);

            if(cc != null)
            {
                float broadcastingRights = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.BroadcastRights));
                float sponsorsAds = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.Sponsors));
                float gateReceipts = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.GateReceipts));
                float otherIncome = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.OtherIncome));

                float payroll = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.Payroll));
                float transferAmortization = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.TransferAmortization));
                float agentsFees = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.Agents));
                float otherExpenses = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.OtherExpenses));

                float incomeTax = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.IncomeTax));
                float transfersResult = cc.GetBudgetOnYear(beginningDate, cc.GetBudgetEntries(BudgetReportEntry.Transfers));

                float totalIncomes = broadcastingRights + sponsorsAds + gateReceipts + otherIncome;
                float totalExpenses = payroll + transferAmortization + agentsFees + otherExpenses;
                float operatingResult = totalIncomes + totalExpenses;
                float resultBeforeTax = operatingResult + transfersResult;
                float netResult = resultBeforeTax + incomeTax;

                spBudgetReport.Children.Clear();
                spBudgetReport.Children.Add(ViewUtils.CreateLabel(FindResource("str_income").ToString(), "StyleLabel2", 14, 200));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_broadcasting_rights").ToString(), broadcastingRights, BudgetReportEntry.BroadcastRights));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_sponsors_ads").ToString(), sponsorsAds, BudgetReportEntry.Sponsors));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_gate_receipts").ToString(), gateReceipts, BudgetReportEntry.GateReceipts));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_other_income").ToString(), otherIncome, BudgetReportEntry.OtherIncome));


                spBudgetReport.Children.Add(ViewUtils.CreateLabel(FindResource("str_expenses").ToString(), "StyleLabel2", 14, 200));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_payroll").ToString(), payroll, BudgetReportEntry.Payroll));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_transfer_amortization").ToString(), transferAmortization, BudgetReportEntry.TransferAmortization));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_agents_fees").ToString(), agentsFees, BudgetReportEntry.Agents));
                StackPanel spOtherExpenses = CreateBudgetReportEntry(FindResource("str_other_expenses").ToString(), otherExpenses, BudgetReportEntry.OtherExpenses);
                spOtherExpenses.Margin = new Thickness(0, 0, 0, 50);
                spBudgetReport.Children.Add(spOtherExpenses);

                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_operating_result").ToString().ToUpper(), operatingResult, BudgetReportEntry.OtherIncome));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_result_from_transfers").ToString().ToUpper(), transfersResult, BudgetReportEntry.Transfers));

                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_result_before_tax").ToString().ToUpper(), resultBeforeTax, BudgetReportEntry.OtherIncome));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_income_tax").ToString(), incomeTax, BudgetReportEntry.OtherIncome));
                spBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_net_results").ToString().ToUpper(), netResult, BudgetReportEntry.OtherIncome));
            }
        }

        public List<DateTime> GetHistoryEntriesDates()
        {
            List<DateTime> historyEntriesDates = new List<DateTime>();
            CityClub cc = _club as CityClub;
            if(cc != null)
            {
                foreach(HistoricEntry he in cc.history.elements)
                {
                    historyEntriesDates.Add(he.date);
                }
            }
            return historyEntriesDates;

        }

        public void FillBudgetReport()
        {
            CityClub cc = _club as CityClub;
            if(cc != null)
            {
                cbSeason.Items.Clear();
                foreach (HistoricEntry history in cc.history.elements)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = history.date.ToShortDateString();
                    cbi.Selected += new RoutedEventHandler((s, e) => NewBudgetReportSelected(history.date));
                    cbSeason.Items.Add(cbi);
                }
            }
        }

        public void FillBudget()
        {
            DateTime beginSeason = (Session.Instance.Game.date.Month <= 6 ||(Session.Instance.Game.date.Month == 6 && Session.Instance.Game.date.Day < 15)) ? new DateTime(Session.Instance.Game.date.Year-1, 6, 15) : new DateTime(Session.Instance.Game.date.Year, 6, 15); 
            CityClub cc = _club as CityClub;
            if(cc != null)
            {
                Dictionary<BudgetModificationReason, double> depenses = new Dictionary<BudgetModificationReason, double>();
                Dictionary<BudgetModificationReason, double> incomes = new Dictionary<BudgetModificationReason, double>();

                foreach (BudgetEntry be in cc.budgetHistory)
                {
                    StackPanel spEntry = new StackPanel();
                    spEntry.Orientation = Orientation.Horizontal;
                    spEntry.Children.Add(ViewUtils.CreateLabel(be.Date.ToShortDateString(), "StyleLabel2", 12, 90));
                    spEntry.Children.Add(ViewUtils.CreateLabel(Utils.FormatMoney(be.Amount), "StyleLabel2", 12, 100, be.Amount < 0 ? Brushes.Red : null));
                    spEntry.Children.Add(ViewUtils.CreateLabel(Utils.GetDescription(be.Reason), "StyleLabel2", 12, 125));
                    spBudget.Children.Add(spEntry);

                    if(Utils.IsBefore(beginSeason, be.Date))
                    {
                        AddValueToDictionnary(be.Amount < 0 ? depenses : incomes, be.Reason, be.Amount);
                    }
                }

                List<string> depensesLabels = new List<string>();
                List<double> depensesValue = new List<double>();
                foreach (KeyValuePair<BudgetModificationReason, double> kvp in depenses)
                {
                    depensesLabels.Add(kvp.Key.ToString());
                    depensesValue.Add(kvp.Value);
                }
                List<string> incomeLabels = new List<string>();
                List<double> incomeValue = new List<double>();
                foreach (KeyValuePair<BudgetModificationReason, double> kvp in incomes)
                {
                    incomeLabels.Add(kvp.Key.ToString());
                    incomeValue.Add(kvp.Value);
                }

                ChartView depensesChart = new ChartView(ChartType.PIE_CHART, "", new List<string>() { "" }, "", "", depensesLabels, false, false, 1, new List<List<double>>() { depensesValue }, 200, 200);
                depensesChart.RenderChart(spRepartitions);

                ChartView incomesChart = new ChartView(ChartType.PIE_CHART, "", new List<string>() { "" }, "", "", incomeLabels, false, false, 1, new List<List<double>>() { incomeValue }, 200, 200);
                incomesChart.RenderChart(spRepartitions);

            }
        }

        public void FillGames()
        {
            List<Match> matchs = _club.Games;
            ViewMatches view = new ViewMatches(matchs, true, true, false, true, false, true, 14, true, _club);
            view.Full(spMatchs);
        }

        public void Palmares(CityClub club)
        {
            foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                int nombre = 0;
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
                    StackPanel spPalmaresEntry = new StackPanel() { Orientation = Orientation.Horizontal };
                    spPalmaresEntry.Children.Add(ViewUtils.CreateLabel(c.name, "StyleLabel2", 12, 175));
                    spPalmaresEntry.Children.Add(ViewUtils.CreateLabel(nombre.ToString(), "StyleLabel2", 12, 75));
                    spPalmares.Children.Add(spPalmaresEntry);
                }
            }
        }

        public Windows_Club(CityClub c)
        {
            InitializeComponent();

            imgBudget.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\budget.png"));
            imgCurrentBudget.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\budget.png"));
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            imgManager.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\manager.png"));

            _club = c;
            lbClub.Content = c.name;

            if(c.manager != null)
            {
                lbEntraineur.Content = c.manager.ToString();
            }
            else
            {
                lbEntraineur.Content = FindResource("str_noManager").ToString();
            }

            lbBudget.Content = Utils.FormatMoney(c.budget);
            lbCurrentBudget.Content = Utils.FormatMoney(c.budget);

            lbAdministrativeDivision.Content =
                c.AdministrativeDivision() != null ? c.AdministrativeDivision().name : "-";

            try
            {
                imgLogo.Source = new BitmapImage(new Uri(Utils.Logo(c)));
            }
            catch(Exception e)
            {
                Utils.Debug(e.ToString());
            }
            Palmares(c);
            FillGames();
            FillBudget();
            FillBudgetReport();
            FillHistory();

            List<Player> newContracts = new List<Player>();
            
            foreach (Contract ct in c.allContracts)
            {
                if ((ct.beginning.Year == Session.Instance.Game.date.Year - 1 && ct.beginning.Month < 7) ||
                    (ct.beginning.Year == Session.Instance.Game.date.Year && ct.beginning.Month >= 7))
                {
                    newContracts.Add(ct.player);
                }
            }
            
            /*TODO
            ViewPlayers viewNewPlayers = new ViewPlayers(newContracts, 11, false, false, false, true, false, false, false, false, false, true, false, false, false, false, false, false, false);
            viewNewPlayers.Full(spArrivees);
            */
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
                StackPanel spHistoryEntry = new StackPanel();
                spHistoryEntry.Orientation = Orientation.Horizontal;
                spHistoryEntry.Children.Add(ViewUtils.CreateLabelOpenWindow<Tournament>(hce.Competition, OpenTournament, hce.Annee.ToString(), "StyleLabel2", 11, 75));
                spHistoryEntry.Children.Add(ViewUtils.CreateLabelOpenWindow<Tournament>(hce.Competition, OpenTournament, hce.Competition.name, "StyleLabel2", 11, 125));
                spHistoryEntry.Children.Add(ViewUtils.CreateLabel(hce.Classement.ToString(), "StyleLabel2", 11, 50));
                spHistory.Children.Add(spHistoryEntry);
            }

            ChartValues<int> budgets = new ChartValues<int>();
            ChartValues<int> attendance = new ChartValues<int>();
            List<double> centreFormation = new List<double>();
            foreach (HistoricEntry eh in c.history.elements)
            {
                budgets.Add(eh.budget);
                centreFormation.Add(eh.formationFacilities);
                attendance.Add(eh.averageAttendance);
            }
            DataContext = this;

            BudgetsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = FindResource("str_budget").ToString(),
                    Values = budgets,
                }
            };

            //Average attendance

            AttendanceCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = FindResource("str_averageAttendance").ToString(),
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
            

            if(c.records.BiggestWin != null)
            {
                lbBiggestWin.Content = c.records.BiggestWin.home.name + " " + c.records.BiggestWin.score1 + " - " + c.records.BiggestWin.score2 + " " + c.records.BiggestWin.away.name;
            }
            if (c.records.BiggestLose != null)
            {
                lbBiggestLose.Content = c.records.BiggestLose.home.name + " " + c.records.BiggestLose.score1 + " - " + c.records.BiggestLose.score2 + " " + c.records.BiggestLose.away.name;
            }

            ChartView formationCentreChart = new ChartView(ChartType.LINE_CHART, FindResource("str_formationCentre").ToString(), new List<string>() { FindResource("str_formationCentre").ToString() }, FindResource("str_level").ToString(), FindResource("str_year").ToString(), LabelsAnnees.ToList(), false, false, 1, new List<List<double>>() { centreFormation.ToList() }, 450, 300, 0, 100);
            formationCentreChart.RenderChart(spChartRight);

        }

        private void FillHistory()
        {
            List<Tournament> tournaments = new List<Tournament>(_club.Country().Cups());
            foreach (Continent c in Session.Instance.Game.kernel.world.GetAllContinents())
            {
                if (c.countries.Count == 0 || _club.Country().Continent == c)
                {
                    int j = 0;
                    Tournament continentalT = c.GetContinentalClubTournament(++j);
                    while(continentalT != null)
                    {
                        tournaments.Add(continentalT);
                        continentalT = c.GetContinentalClubTournament(++j);
                    }
                }
            }
            foreach (Tournament t in tournaments)
            {
                StackPanel spTournament = new StackPanel();
                spTournament.Orientation = Orientation.Vertical;
                spTournament.Children.Add(ViewUtils.CreateLabel(t.name, "StyleLabel2", 12, -1));
                foreach (KeyValuePair<int, Tournament> previousEdition in t.previousEditions)
                {
                    Tournament tournament = previousEdition.Value;
                    string teamPerformance = null;
                    for (int i = tournament.rounds.Count - 1; i >= 0 && teamPerformance == null; i--)
                    {
                        if (tournament.rounds[i].clubs.Contains(_club))
                        {
                            teamPerformance = (i == tournament.rounds.Count - 1 && tournament.Winner() == _club) ? FindResource("str_winner").ToString() : tournament.rounds[i].name;
                        }
                    }
                    teamPerformance = teamPerformance == null ? FindResource("str_notQualified").ToString() : teamPerformance;

                    StackPanel spEdition = new StackPanel();
                    spEdition.Orientation = Orientation.Horizontal;
                    spEdition.Children.Add(ViewUtils.CreateLabel(String.Format("{0}", previousEdition.Key), "StyleLabel2", 10, 75));
                    spEdition.Children.Add(ViewUtils.CreateLabel(teamPerformance, "StyleLabel2", 10, 500));
                    spTournament.Children.Add(spEdition);

                }
                spTournamentsHistory.Children.Add(spTournament);
            }
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMatch_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int idMatch = int.Parse(btn.Name.Split('_')[1]);
            Match match = _club.Games[idMatch];
            Windows_Match wm = new Windows_Match(match);
            wm.Show();

        }

        private void OpenTournament(Tournament t)
        {
            Windows_Competition wc = new Windows_Competition(t);
            wc.Show();
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
