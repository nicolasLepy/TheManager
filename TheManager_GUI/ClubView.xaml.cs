using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheManager;
using TheManager_GUI.Styles;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.views;

namespace TheManager_GUI
{

    /// <summary>
    /// Logique d'interaction pour ClubView.xaml
    /// </summary>
    public partial class ClubView : Window
    {

        private readonly CityClub club;

        public ClubView(CityClub club)
        {
            this.club = club;
            InitializeComponent();
            Initialize();
        }

        public void InitializeBudgetReports()
        {
            comboBoxBudgetSelection.Items.Clear();
            foreach (HistoricEntry history in club.history.elements)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = history.date.ToShortDateString();
                cbi.Selected += new RoutedEventHandler((s, e) => NewBudgetReportSelected(history.date));
                comboBoxBudgetSelection.Items.Add(cbi);
            }
            if(club.history.elements.Count > 0)
            {
                comboBoxBudgetSelection.SelectedIndex = 0;
            }
        }

        private void AddValueToDictionnary(Dictionary<BudgetModificationReason, double> dictionnary, BudgetModificationReason key, double value)
        {
            if (!dictionnary.ContainsKey(key))
            {
                dictionnary.Add(key, 0);
            }
            dictionnary[key] += value;
        }

        public void InitializePalmares()
        {
            panelPalmares.Children.Clear();
            foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                List<string> years = new List<string>();
                int count = 0;
                foreach (KeyValuePair<int, Tournament> archive in c.previousEditions)
                {
                    if(archive.Value.Winner() == club)
                    {
                        count++;
                        years.Add(archive.Key.ToString());
                    }
                }
                if (count > 0)
                {
                    Grid gridEntry = new Grid();
                    gridEntry.Margin = new Thickness(15, 0, 15, 0);
                    gridEntry.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    gridEntry.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5, GridUnitType.Star) });
                    gridEntry.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    TextBlock textName = ViewUtils.CreateTextBlockOpenWindow(c, OpenTournament, c.name, StyleDefinition.styleTextSecondary, -1, -1);
                    textName.HorizontalAlignment = HorizontalAlignment.Center;
                    textName.TextAlignment = TextAlignment.Center;
                    TextBlock textCount = ViewUtils.CreateTextBlock(count.ToString(), StyleDefinition.styleTextSecondary, -1, -1, null, Brushes.Gray, true);
                    textCount.HorizontalAlignment = HorizontalAlignment.Center;
                    textCount.TextAlignment = TextAlignment.Center;
                    textCount.Padding = new Thickness(5);
                    TextBlock textYears = ViewUtils.CreateTextBlock(years.Aggregate((x, y) => x + ", " + y), StyleDefinition.styleTextPlainCenter, -1, 150);
                    textYears.TextWrapping = TextWrapping.Wrap;
                    textYears.TextAlignment = TextAlignment.Center;
                    ViewUtils.AddElementToGrid(gridEntry, textName, 0, -1);
                    ViewUtils.AddElementToGrid(gridEntry, ViewUtils.CreateLogo(c, 65, 65), 1, -1);
                    ViewUtils.AddElementToGrid(gridEntry, textCount, 1, -1);
                    ViewUtils.AddElementToGrid(gridEntry, textYears, 2, -1);
                    panelPalmares.Children.Add(gridEntry);
                }
            }
        }

        public void InitializeHistory()
        {
            Dictionary<int, Tournament> historic = new Dictionary<int, Tournament>();
            foreach (Tournament competition in Session.Instance.Game.kernel.Competitions)
            {
                foreach (KeyValuePair<int, Tournament> archive in competition.previousEditions)
                {
                    if (archive.Value.isChampionship && archive.Value.rounds[0].clubs.Contains(club))
                    {
                        historic.Add(archive.Key, archive.Value);
                    }
                }
            }
            foreach (KeyValuePair<int, Tournament> archive in new SortedDictionary<int, Tournament>(historic))
            {
                Tournament next = historic.ContainsKey(archive.Key + 1) ? historic[archive.Key + 1] : null;
                int ranking = 0;
                int points = archive.Value.rounds[0].Points(club);
                int wins = archive.Value.rounds[0].Wins(club);
                int draws = archive.Value.rounds[0].Draws(club);
                int loses = archive.Value.rounds[0].Loses(club);
                int goalsFor = archive.Value.rounds[0].GoalsFor(club);
                int goalsAgainst = archive.Value.rounds[0].GoalsAgainst(club);
                int goalsAverage = archive.Value.rounds[0].Difference(club);
                //If the league is active (round 0 is a championship round, not inactive)
                if ((archive.Value.rounds[0] as ChampionshipRound) != null)
                {
                    ranking = (archive.Value.rounds[0] as ChampionshipRound).Ranking().IndexOf(club) + 1;
                }
                else if ((archive.Value.rounds[0] as GroupsRound) != null)
                {
                    GroupsRound rnd = (archive.Value.rounds[0] as GroupsRound);
                    for (int j = 0; j < rnd.groupsCount; j++)
                    {
                        if (rnd.groups[j].Contains(club))
                        {
                            ranking = rnd.Ranking(j).IndexOf(club) + 1;
                        }
                    }
                }

                string brushColor = next != null && next.level > archive.Value.level ? StyleDefinition.colorNegative : next != null && next.level < archive.Value.level ? StyleDefinition.colorPositive : "";

                gridChampionshipHistory.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50, GridUnitType.Pixel) });
                TextBlock textLeague = ViewUtils.CreateTextBlockOpenWindow<Tournament>(archive.Value, OpenTournament, archive.Value.name, StyleDefinition.styleTextPlainCenter, -1, -1);
                Border borderLeague = new Border();
                if (brushColor.Length > 0)
                {
                    SolidColorBrush scb = Application.Current.FindResource(brushColor) as SolidColorBrush;
                    scb = scb.Clone();
                    scb.Opacity = 0.5;
                    borderLeague.Background = scb;
                }
                borderLeague.Child = textLeague;
                TextBlock textYear = ViewUtils.CreateTextBlockOpenWindow<Tournament>(archive.Value, OpenTournament, archive.Key.ToString(), StyleDefinition.styleTextPlainCenter, -1, -1);
                TextBlock textRanking = ViewUtils.CreateTextBlock(ranking > 0 ? ranking.ToString() : "", StyleDefinition.styleTextPlainCenter, -1, -1, null, null, true);
                TextBlock textPoints = ViewUtils.CreateTextBlock(points.ToString(), StyleDefinition.styleTextPlainCenter);
                TextBlock textWins = ViewUtils.CreateTextBlock(wins.ToString(), StyleDefinition.styleTextPlainCenter);
                TextBlock textDraws = ViewUtils.CreateTextBlock(draws.ToString(), StyleDefinition.styleTextPlainCenter);
                TextBlock textLoses = ViewUtils.CreateTextBlock(loses.ToString(), StyleDefinition.styleTextPlainCenter);
                TextBlock textGoalsFor = ViewUtils.CreateTextBlock(goalsFor.ToString(), StyleDefinition.styleTextPlainCenter);
                TextBlock textGoalsAgainst = ViewUtils.CreateTextBlock(goalsAgainst.ToString(), StyleDefinition.styleTextPlainCenter);
                TextBlock textGoalsDifference = ViewUtils.CreateTextBlock(goalsAverage.ToString(), StyleDefinition.styleTextPlainCenter);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textYear, gridChampionshipHistory.RowDefinitions.Count - 1, 0);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, borderLeague, gridChampionshipHistory.RowDefinitions.Count - 1, 1);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textRanking, gridChampionshipHistory.RowDefinitions.Count - 1, 2);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textPoints, gridChampionshipHistory.RowDefinitions.Count - 1, 3);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textWins, gridChampionshipHistory.RowDefinitions.Count - 1, 4);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textDraws, gridChampionshipHistory.RowDefinitions.Count - 1, 5);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textLoses, gridChampionshipHistory.RowDefinitions.Count - 1, 6);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textGoalsFor, gridChampionshipHistory.RowDefinitions.Count - 1, 7);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textGoalsAgainst, gridChampionshipHistory.RowDefinitions.Count - 1, 8);
                ViewUtils.AddElementToGrid(gridChampionshipHistory, textGoalsDifference, gridChampionshipHistory.RowDefinitions.Count - 1, 9);
            }

            List<Tournament> cups = new List<Tournament>(club.Country().Cups());
            foreach (Continent c in Session.Instance.Game.kernel.world.GetAllContinents())
            {
                if (c.countries.Count == 0 || club.Country().Continent == c)
                {
                    int j = 0;
                    Tournament continentalT = c.GetContinentalClubTournament(++j);
                    while (continentalT != null)
                    {
                        cups.Add(continentalT);
                        continentalT = c.GetContinentalClubTournament(++j);
                    }
                }
            }
            foreach (Tournament t in cups)
            {
                StackPanel spTournament = new StackPanel();
                spTournament.Orientation = Orientation.Vertical;
                spTournament.Children.Add(ViewUtils.CreateTextBlock(t.name, StyleDefinition.styleTextPlainCenter, -1, -1));
                foreach (KeyValuePair<int, Tournament> previousEdition in t.previousEditions)
                {
                    Tournament tournament = previousEdition.Value;
                    string teamPerformance = null;
                    for (int i = tournament.rounds.Count - 1; i >= 0 && teamPerformance == null; i--)
                    {
                        if (tournament.rounds[i].clubs.Contains(club))
                        {
                            teamPerformance = (i == tournament.rounds.Count - 1 && tournament.Winner() == club) ? FindResource("str_winner").ToString() : tournament.rounds[i].name;
                        }
                    }
                    teamPerformance = teamPerformance == null ? FindResource("str_notQualified").ToString() : teamPerformance;

                    StackPanel spEdition = new StackPanel();
                    spEdition.Orientation = Orientation.Horizontal;
                    spEdition.Children.Add(ViewUtils.CreateTextBlock(String.Format("{0}", previousEdition.Key), StyleDefinition.styleTextPlain, -1, 100));
                    spEdition.Children.Add(ViewUtils.CreateTextBlock(teamPerformance, StyleDefinition.styleTextPlain, -1, 500));
                    spTournament.Children.Add(spEdition);

                }
                panelCupHistory.Children.Add(spTournament);
            }

            if (club.records.BiggestWin != null)
            {
                ViewScores view = new ViewScores(new List<Match>() { club.records.BiggestWin }, true, false, false, true, false, true);
                view.Full(panelRecordBiggestWin);
            }
            if (club.records.BiggestLose != null)
            {
                ViewScores view = new ViewScores(new List<Match>() { club.records.BiggestLose }, true, false, false, true, false, true);
                view.Full(panelRecordBiggestLose);
            }
        }

        public void InitializeFinances()
        {
            List<double> budgets = new List<double>();
            List<double> formationCentre = new List<double>();
            List<string> years = new List<string>();
            List<double> attendances = new List<double>();
            foreach (HistoricEntry he in club.history.elements)
            {
                budgets.Add(he.budget);
                attendances.Add(he.averageAttendance);
                formationCentre.Add(he.formationFacilities);
                years.Add(he.date.Year.ToString());
            }

            DateTime beginSeason = (Session.Instance.Game.date.Month <= 6 || (Session.Instance.Game.date.Month == 6 && Session.Instance.Game.date.Day < 15)) ? new DateTime(Session.Instance.Game.date.Year - 1, 6, 15) : new DateTime(Session.Instance.Game.date.Year, 6, 15);
            Dictionary<BudgetModificationReason, double> depenses = new Dictionary<BudgetModificationReason, double>();
            Dictionary<BudgetModificationReason, double> incomes = new Dictionary<BudgetModificationReason, double>();

            foreach (BudgetEntry be in club.budgetHistory)
            {
                StackPanel spEntry = new StackPanel();
                spEntry.Orientation = Orientation.Horizontal;
                spEntry.Children.Add(ViewUtils.CreateTextBlock(be.Date.ToShortDateString(), StyleDefinition.styleTextPlain, 12, 90));
                spEntry.Children.Add(ViewUtils.CreateTextBlock(Utils.FormatMoney(be.Amount), StyleDefinition.styleTextPlain, 12, 100, be.Amount < 0 ? Application.Current.FindResource(StyleDefinition.colorNegative) as SolidColorBrush : null));
                spEntry.Children.Add(ViewUtils.CreateTextBlock(Utils.GetDescription(be.Reason), StyleDefinition.styleTextPlain, 12, 125));
                panelBudgetOperations.Children.Add(spEntry);

                if (Utils.IsBefore(beginSeason, be.Date))
                {
                    AddValueToDictionnary(be.Amount < 0 ? depenses : incomes, be.Reason, be.Amount);
                }
            }

            List<string> expensesLabels = new List<string>();
            List<double> expensesValue = new List<double>();
            foreach (KeyValuePair<BudgetModificationReason, double> kvp in depenses)
            {
                expensesLabels.Add(kvp.Key.ToString());
                expensesValue.Add(kvp.Value);
            }
            List<string> incomeLabels = new List<string>();
            List<double> incomeValue = new List<double>();
            foreach (KeyValuePair<BudgetModificationReason, double> kvp in incomes)
            {
                incomeLabels.Add(kvp.Key.ToString());
                incomeValue.Add(kvp.Value);
            }

            ChartView chartBudget = new ChartView(ChartType.LINE_CHART, FindResource("str_balanceEvolution").ToString(), FindResource("str_budget").ToString(), FindResource("str_years").ToString(), years, true, 1, budgets, -1, 300);
            chartBudget.RenderChart(panelBalanceEvolution);
            ChartView chartFormation = new ChartView(ChartType.LINE_CHART, FindResource("str_formationCentre").ToString(), FindResource("str_formationCentre").ToString(), FindResource("str_years").ToString(), years, false, 1, formationCentre, -1, 300);
            chartFormation.RenderChart(panelFormationCentre);
            ChartView chartAttendance = new ChartView(ChartType.LINE_CHART, FindResource("str_averageAttendance").ToString(), FindResource("str_averageAttendance").ToString(), FindResource("str_years").ToString(), years, false, 1, attendances, -1, 300);
            chartAttendance.RenderChart(borderAttendance);

            ChartView chartExpenses = new ChartView(ChartType.PIE_CHART, "", "", "", expensesLabels, false, 1, expensesValue, 200, 200);
            chartExpenses.RenderChart(panelChartExpenses);
            ChartView chartIncomes = new ChartView(ChartType.PIE_CHART, "", "", "", incomeLabels, false, 1, incomeValue, 200, 200);
            chartIncomes.RenderChart(panelChartIncomes);
        }

        public void Initialize()
        {

            tbClubName.Text = club.name;

            if (club.manager != null)
            {
                tbManager.Text = club.manager.ToString();
            }
            else
            {
                tbManager.Text = FindResource("str_noManager").ToString();
            }

            tbBudget.Text = Utils.FormatMoney(club.budget);
            tbCity.Text = club.city.Name;

            tbStadium.Text = club.stadium.name;
            tbCapacity.Text = String.Format("{0} {1}", club.stadium.capacity, FindResource("str_seats").ToString());
            imageClub.Source = new BitmapImage(new Uri(Utils.Logo(club)));
            imageLogo.Source = new BitmapImage(new Uri(Utils.Logo(club)));

            PlayersView pv = new PlayersView(club.Players(), 0.9f, true, true, true, false, true, true, true, true, true, true, true, false, false, false, false, false, false, true);
            pv.Full(panelClubTeam);

            List<Match> matchs = club.Games;
            ViewScores view = new ViewScores(matchs, true, true, false, true, false, true, 10.5f, true, club);
            view.Full(panelClubLastGames);

            InitializeBudgetReports();
            InitializeFinances();
            InitializeHistory();
            InitializePalmares();
            InitializeTournaments();

        }

        public bool isLastRound(Round round)
        {
            Tournament tournament = round.Tournament;
            bool res = true;
            foreach(Qualification q in round.qualifications)
            {
                if(!q.isNextYear && q.tournament == tournament)
                {
                    res = false;
                }
            }
            return res;
        }

        public void InitializeTournaments()
        {
            Dictionary<Tournament, Round> clubTournaments = new Dictionary<Tournament, Round>();
            foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                DateTime dateReference = new DateTime(1000, 1, 1);
                foreach(Round r in t.rounds)
                {
                    if(r.clubs.Contains(club) && Utils.IsBefore(dateReference, r.DateInitialisationRound()))
                    {
                        dateReference = r.DateInitialisationRound();
                        if (!clubTournaments.ContainsKey(t))
                        {
                            clubTournaments.Add(t, r);
                        }
                        else
                        {
                            clubTournaments[t] = r;
                        }
                    }
                }
            }
            foreach(KeyValuePair<Tournament, Round> kvp in clubTournaments)
            {
                gridTournaments.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                TextBlock tournamentName = ViewUtils.CreateTextBlock(kvp.Key.shortName, StyleDefinition.styleTextPlainCenter);

                bool isWinner = kvp.Key.Winner() == club;
                SolidColorBrush brush = isWinner ? Application.Current.FindResource(StyleDefinition.colorPositive) as SolidColorBrush : (!isLastRound(kvp.Value) && !kvp.Key.isChampionship) ? Application.Current.FindResource(StyleDefinition.colorNegative) as SolidColorBrush : null;

                TextBlock tournamentStatus = ViewUtils.CreateTextBlock(isWinner ? FindResource("str_winner").ToString() : kvp.Value.name, StyleDefinition.styleTextPlainCenter, (double)FindResource(StyleDefinition.fontSizeRegular)*0.8, -1, brush);
                Image tournamentLogo = ViewUtils.CreateLogo(kvp.Key, 15, 30);
                ViewUtils.AddElementToGrid(gridTournaments, tournamentName, gridTournaments.RowDefinitions.Count - 1, 0);
                ViewUtils.AddElementToGrid(gridTournaments, tournamentLogo, gridTournaments.RowDefinitions.Count - 1, 1);
                ViewUtils.AddElementToGrid(gridTournaments, tournamentStatus, gridTournaments.RowDefinitions.Count - 1, 2);
            }
        }

        private StackPanel CreateBudgetReportEntry(string name, float value, BudgetReportEntry budgetReportEntry)
        {
            StackPanel spEntry = new StackPanel();
            spEntry.Orientation = Orientation.Horizontal;
            spEntry.Children.Add(ViewUtils.CreateTextBlockOpenWindow<BudgetReportEntry>(budgetReportEntry, UpdateBudgetReportChartSelected, name, StyleDefinition.styleTextPlain, 13, 400));
            string brushName = value < 0 ? StyleDefinition.colorNegative : StyleDefinition.colorPositive;
            spEntry.Children.Add(ViewUtils.CreateTextBlock(Utils.FormatMoney(value), StyleDefinition.styleTextPlain, 13, 200, Application.Current.FindResource(brushName) as SolidColorBrush));
            return spEntry;
        }

        public List<DateTime> GetHistoryEntriesDates()
        {
            List<DateTime> historyEntriesDates = new List<DateTime>();
            foreach (HistoricEntry he in club.history.elements)
            {
                historyEntriesDates.Add(he.date);
            }
            return historyEntriesDates;

        }

        public void UpdateBudgetReportChartSelected(BudgetReportEntry budgetReportEntry)
        {
            string title = budgetReportEntry.ToString();
            List<BudgetModificationReason> budgetEntries = club.GetBudgetEntries(budgetReportEntry);
            panelBudgetReportChart.Children.Clear();
            List<string> years = new List<string>();
            List<double> amount = new List<double>();
            foreach (DateTime dt in GetHistoryEntriesDates())
            {
                years.Add(dt.Year.ToString());
                amount.Add(club.GetBudgetOnYear(dt, budgetEntries));
            }
            double min = Math.Min(amount.Min(), 0);
            double max = Math.Max(amount.Max(), 0);
            if (min == 0 && max == 0)
            {
                panelBudgetReportChart.Children.Add(ViewUtils.CreateTextBlock(FindResource("str_no_entry").ToString(), StyleDefinition.styleTextPlain, 14, 400));
            }
            else
            {
                ChartView view = new ChartView(ChartType.LINE_CHART, title, FindResource("str_total").ToString(), title, years, true, 1, amount, -1, 550, min, max);
                view.RenderChart(panelBudgetReportChart);
            }
        }

        private void NewBudgetReportSelected(DateTime date)
        {
            DateTime beginningDate = date.AddYears(-1);
            float broadcastingRights = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.BroadcastRights));
            float sponsorsAds = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.Sponsors));
            float gateReceipts = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.GateReceipts));
            float otherIncome = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.OtherIncome));

            float payroll = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.Payroll));
            float transferAmortization = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.TransferAmortization));
            float agentsFees = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.Agents));
            float otherExpenses = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.OtherExpenses));

            float incomeTax = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.IncomeTax));
            float transfersResult = club.GetBudgetOnYear(beginningDate, club.GetBudgetEntries(BudgetReportEntry.Transfers));

            float totalIncomes = broadcastingRights + sponsorsAds + gateReceipts + otherIncome;
            float totalExpenses = payroll + transferAmortization + agentsFees + otherExpenses;
            float operatingResult = totalIncomes + totalExpenses;
            float resultBeforeTax = operatingResult + transfersResult;
            float netResult = resultBeforeTax + incomeTax;

            panelBudgetReport.Children.Clear();
            panelBudgetReport.Children.Add(ViewUtils.CreateTextBlock(FindResource("str_income").ToString(), StyleDefinition.styleTextPlain, 14, 200));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_broadcasting_rights").ToString(), broadcastingRights, BudgetReportEntry.BroadcastRights));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_sponsors_ads").ToString(), sponsorsAds, BudgetReportEntry.Sponsors));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_gate_receipts").ToString(), gateReceipts, BudgetReportEntry.GateReceipts));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_other_income").ToString(), otherIncome, BudgetReportEntry.OtherIncome));


            panelBudgetReport.Children.Add(ViewUtils.CreateTextBlock(FindResource("str_expenses").ToString(), StyleDefinition.styleTextPlain, 14, 200));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_payroll").ToString(), payroll, BudgetReportEntry.Payroll));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_transfer_amortization").ToString(), transferAmortization, BudgetReportEntry.TransferAmortization));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_agents_fees").ToString(), agentsFees, BudgetReportEntry.Agents));
            StackPanel spOtherExpenses = CreateBudgetReportEntry(FindResource("str_other_expenses").ToString(), otherExpenses, BudgetReportEntry.OtherExpenses);
            spOtherExpenses.Margin = new Thickness(0, 0, 0, 50);
            panelBudgetReport.Children.Add(spOtherExpenses);

            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_operating_result").ToString().ToUpper(), operatingResult, BudgetReportEntry.OtherIncome));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_result_from_transfers").ToString().ToUpper(), transfersResult, BudgetReportEntry.Transfers));

            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_result_before_tax").ToString().ToUpper(), resultBeforeTax, BudgetReportEntry.OtherIncome));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_income_tax").ToString(), incomeTax, BudgetReportEntry.OtherIncome));
            panelBudgetReport.Children.Add(CreateBudgetReportEntry(FindResource("str_net_results").ToString().ToUpper(), netResult, BudgetReportEntry.OtherIncome));

        }

        private void OpenTournament(Tournament t)
        {
            TournamentView view = new TournamentView(t);
            view.Show();
        }

        /* EVENTS HANDLER */

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int mParam, int lParam);

        private void spControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void spControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
