using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using TheManager.Comparators;
using TheManager_GUI.controls;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.views;
using TheManager_GUI.Styles;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour CountryView.xaml
    /// </summary>
    public partial class CountryView : Window
    {

        private readonly NationalTeam nationalTeam;

        public CountryView(NationalTeam nationalTeam)
        {
            this.nationalTeam = nationalTeam;
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {

            tbCountryName.Text = nationalTeam.name;
            List<NationalTeam> fifaRanking = Session.Instance.Game.kernel.FifaRanking();
            int continentalRank = 0;
            int continentalTeams = 0;
            for (int i = 0; i < fifaRanking.Count; i++)
            {
                if (fifaRanking[i].country.Continent == nationalTeam.country.Continent)
                {
                    continentalTeams++;
                }
                if (fifaRanking[i] == nationalTeam)
                {
                    continentalRank = continentalTeams;
                }
            }

            int associationIndex = nationalTeam.country.Continent.associationRanking.IndexOf(nationalTeam.country);
            string associationRankingStr = associationIndex > -1 ? String.Format("{0}/{1}", associationIndex + 1, nationalTeam.country.Continent.associationRanking.Count) : String.Format("{0}", FindResource("str_notranked").ToString());

            tbHeadCoach.Text = nationalTeam.manager != null ? nationalTeam.manager.ToString() : "-";

            tbRankingWorld.Text = String.Format("{0}/{1}", (fifaRanking.IndexOf(nationalTeam) + 1), fifaRanking.Count);
            imageRankingWorld.Source = ViewUtils.LoadBitmapImageWithCache(new Uri("images\\universe\\world.png", UriKind.RelativeOrAbsolute));

            tbRankingAssociation.Text = String.Format("{0}/{1}", continentalRank, continentalTeams);
            imageRankingAssociation.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(nationalTeam.country.Continent), UriKind.RelativeOrAbsolute));

            tbRankingClubAssociation.Text = String.Format("{0} ({1})", associationRankingStr, FindResource("str_club").ToString().ToLower());
            imageRankingClubAssociation.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(nationalTeam.country.Continent), UriKind.RelativeOrAbsolute));

            imageLogo.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Flag(nationalTeam.country)));

            List<Player> nationalPlayers = nationalTeam.Players();
            PlayersView vp = new PlayersView(nationalPlayers, 1, true, true, false, true, true, true, false, false, true, false, false, false, false, false, true, true, false, false);
            vp.Full(panelPlayers);

            List<Match> nationalMatchs = nationalTeam.Games;
            ViewScores vm = new ViewScores(nationalMatchs, true, false, false, true, false, true, 12, true, nationalTeam);
            vm.Full(panelGames);

            InitializeTournaments();
            InitializeContinentalTeams();
            InitializeCharts();
            InitializeHistory();
        }

        public void InitializeTournaments()
        {
            List<Tournament> tournaments = new List<Tournament>(nationalTeam.country.Tournaments());
            tournaments.Sort(new TournamentComparator());
            foreach (Tournament tournament in tournaments)
            {
                string winnerName = "-";
                if (tournament.previousEditions.Count > 0)
                {
                    int closestYear = tournament.previousEditions.Aggregate((l, r) => l.Key > r.Key ? l : r).Key;
                    Club winner = tournament.previousEditions[closestYear].Winner();
                    winnerName = winner != null ? winner.name : "-";
                }
                gridTournaments.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                ControlCountryTournament control = new ControlCountryTournament() { TitleHolder = winnerName, TournamentName = tournament.name, ImagePath = Utils.LogoTournament(tournament) };
                control.Margin = new Thickness(0, 15, 0, 0);
                ViewUtils.AddElementToGrid(gridTournaments, control, gridTournaments.RowDefinitions.Count - 1, 0);
            }

        }

        private void InitializeContinentalTeams()
        {
            Continent continent = nationalTeam.country.Continent;
            int tournamentLevel = 1;
            Tournament tournament = continent.GetContinentalClubTournament(tournamentLevel);
            List<Club> clubs = new List<Club>();
            while (tournament != null)
            {
                Dictionary<Club, Round> clubsMap = new Dictionary<Club, Round>();
                for (int i = tournament.rounds.Count - 1; i > -1; i--)
                {
                    Round r = tournament.rounds[i];
                    foreach (Club c in r.clubs)
                    {
                        if (c.Country() == nationalTeam.country && !clubs.Contains(c) && (c as CityClub) != null)
                        {
                            clubs.Add(c);
                            clubsMap.Add(c, r);
                        }
                    }
                }
                ControlStickerTournament control = new ControlStickerTournament(tournament, clubsMap);
                panelClubTournaments.Children.Add(control);
                tournamentLevel++;
                tournament = continent.GetContinentalClubTournament(tournamentLevel);
            }
        }

        private void InitializeCharts()
        {
            //Fifa Ranking
            int archivalEntries = nationalTeam.archivalFifaPoints.Count;
            double[] totalCountries = new double[archivalEntries];
            double[] rankings = new double[archivalEntries];
            
            string[] labelsYears = new string[archivalEntries];
            DateTime dt = new DateTime(Session.Instance.Game.date.Year, Session.Instance.Game.date.Month, 1);

            for (int i = 0; i < archivalEntries; i++)
            {
                totalCountries[i] = 0;
                rankings[i] = 0;
                labelsYears[archivalEntries - i - 1] = String.Format("{0}", dt.ToString("MM/yyyy"));
                foreach (Club c in Session.Instance.Game.kernel.Clubs)
                {
                    NationalTeam nt = c as NationalTeam;
                    if (nt != null)
                    {
                        totalCountries[i]++;
                        if (nt.archivalFifaPoints[i] > nationalTeam.archivalFifaPoints[i])
                        {
                            rankings[i]++;
                        }
                    }
                }
                dt = dt.AddMonths(-1);
            }
            //Invert ranking for chart display
            for (int i = 0; i < rankings.Length; i++)
            {
                rankings[i] = -rankings[i];
            }
            ChartView chartFifa = new ChartView(ChartType.LINE_CHART, FindResource("str_fifaRanking").ToString(), new List<string>() { FindResource("str_fifaRanking").ToString() }, FindResource("str_ranking").ToString(), FindResource("str_years").ToString(), labelsYears.ToList(), false, 1, new List<List<double>>() { rankings.ToList() }, -1, 250, archivalEntries > 0 ? -totalCountries[0] : -1, archivalEntries > 0 ? 0 : -1);
            chartFifa.RenderChart(panelHistoryWorldRanking);

            //Association Ranking
            Continent continent = nationalTeam.country.Continent;
            int assoArchivalEntries = continent.archivalAssociationRanking.Count;
            totalCountries = new double[assoArchivalEntries];
            rankings = new double[assoArchivalEntries];
            string[] labelsYearsAsso = new string[assoArchivalEntries];

            for (int i = 0; i < assoArchivalEntries; i++)
            {
                totalCountries[i] = 0;
                rankings[i] = 0;
                labelsYearsAsso[assoArchivalEntries - i - 1] = String.Format("{0}", Utils.beginningYear + i);
                rankings[i] = continent.archivalAssociationRanking[i].IndexOf(nationalTeam.country) + 1;
                totalCountries[i] = continent.archivalAssociationRanking[i].Count;
            }

            //Invert ranking for chart display
            for (int i = 0; i < rankings.Length; i++)
            {
                rankings[i] = -rankings[i];
            }

            ChartView chartAssociations = new ChartView(ChartType.LINE_CHART, FindResource("str_assoRanking").ToString(), new List<string>() { FindResource("str_assoRanking").ToString() }, FindResource("str_ranking").ToString(), FindResource("str_years").ToString(), labelsYears.ToList(), false, 1, new List<List<double>>() { rankings.ToList() }, -1, 250, assoArchivalEntries > 0 && totalCountries[0] > 0 ? -totalCountries[0] : -1, assoArchivalEntries > 0 && totalCountries[0] > 0 ? 0 : -1);
            chartAssociations.RenderChart(panelHistoryClubRanking);

        }

        public void InitializeHistory()
        {
            foreach (Continent c in Session.Instance.Game.kernel.world.GetAllContinents())
            {
                if (c.countries.Count == 0 || nationalTeam.country.Continent == c)
                {
                    foreach (Tournament t in c.Tournaments())
                    {
                        if (t.rounds.Last().qualifications.Count == 0 && t.previousEditions.Count > 0 && (t.previousEditions.Values.Last().rounds.Last().clubs.Count > 0) && (t.previousEditions.Values.Last().rounds.Last().clubs[0] as NationalTeam) != null)
                        {
                            gridCountryHistory.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50, GridUnitType.Pixel) });
                            ViewUtils.AddElementToGrid(gridCountryHistory, ViewUtils.CreateTextBlock(t.name, StyleDefinition.styleTextSecondary), gridCountryHistory.RowDefinitions.Count - 1, 0, 4);

                            foreach (KeyValuePair<int, Tournament> previousEdition in t.previousEditions)
                            {
                                Tournament tournament = previousEdition.Value;
                                string teamPerformance = null;
                                for (int i = tournament.rounds.Count - 1; i >= 0 && teamPerformance == null; i--)
                                {
                                    if (tournament.rounds[i].clubs.Contains(nationalTeam))
                                    {
                                        teamPerformance = (i == tournament.rounds.Count - 1 && tournament.Winner() == nationalTeam) ? FindResource("str_winner").ToString() : tournament.rounds[i].name;
                                    }
                                }
                                teamPerformance = teamPerformance == null ? FindResource("str_notQualified").ToString() : teamPerformance;

                                List<Country> hosts = tournament.Hosts();
                                gridCountryHistory.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50, GridUnitType.Pixel) });
                                TextBlock tbYear = ViewUtils.CreateTextBlockOpenWindow(tournament, OpenTournament, previousEdition.Key.ToString(), StyleDefinition.styleTextSecondary, -1, -1);
                                TextBlock tbPerformance = ViewUtils.CreateTextBlock(teamPerformance, StyleDefinition.styleTextSecondary);
                                ViewUtils.AddElementToGrid(gridCountryHistory, tbYear, gridCountryHistory.RowDefinitions.Count - 1, 0);
                                ViewUtils.AddElementToGrid(gridCountryHistory, tbPerformance, gridCountryHistory.RowDefinitions.Count - 1, 3);
                                if(hosts.Count > 0)
                                {
                                    TextBlock tbHost = ViewUtils.CreateTextBlock(hosts[0].Name(), StyleDefinition.styleTextSecondary);
                                    double logoSize = (double)FindResource(StyleDefinition.fontSizeSecondary) * 5 / 3.0;
                                    Image imageFlag = ViewUtils.CreateFlag(hosts[0], logoSize, logoSize * 0.66);
                                    ViewUtils.AddElementToGrid(gridCountryHistory, imageFlag, gridCountryHistory.RowDefinitions.Count - 1, 1);
                                    ViewUtils.AddElementToGrid(gridCountryHistory, tbHost, gridCountryHistory.RowDefinitions.Count - 1, 2);
                                }
                            }
                        }
                    }

                }
            }
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
