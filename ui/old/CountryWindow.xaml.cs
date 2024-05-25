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
using tm;
using tm.Comparators;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.Views;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour CountryWindow.xaml
    /// </summary>
    public partial class CountryWindow : Window
    {

        public SeriesCollection rankingsCollection { get; set; }
        public SeriesCollection assoRankingsCollection { get; set; }
        public string[] labelsYears { get; set; }
        public string[] labelsYearsAsso { get; set; }

        private readonly NationalTeam nationalTeam;

        public CountryWindow(NationalTeam nationalTeam)
        {
            InitializeComponent();
            this.nationalTeam = nationalTeam;
            Init();
        }

        private StackPanel CreateGridStackPanel(String content, Image image, int column, int row)
        {
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(image);
            sp.Children.Add(ViewUtils.CreateLabel(content, "StyleLabel2", 12, 100));
            sp.SetValue(Grid.ColumnProperty, column);
            sp.SetValue(Grid.RowProperty, row);
            return sp;
        }

        private void OpenTournament(Tournament t)
        {
            Windows_Competition wc = new Windows_Competition(t);
            wc.Show();
        }

        private void OpenClub(Club c)
        {
            CityClub cc = (c as CityClub);
            ClubView wc = new ClubView(cc);
            wc.Show();
        }

        private void FillTournaments()
        {
            List<Tournament> tournaments = new List<Tournament>(nationalTeam.country.Tournaments());
            tournaments.Sort(new TournamentComparator());
            foreach(Tournament tournament in tournaments)
            {
                Border border = new Border();
                border.Margin = new Thickness(5);
                border.Style = FindResource("StyleBorderStackPanel") as Style;
                StackPanel spTournament = new StackPanel();
                spTournament.Orientation = Orientation.Horizontal;
                StackPanel spTournamentName = new StackPanel();
                spTournamentName.Orientation = Orientation.Vertical;
                spTournamentName.Children.Add(ViewUtils.CreateLogo(tournament, 50, 50));
                spTournamentName.Children.Add(ViewUtils.CreateLabelOpenWindow<Tournament>(tournament, OpenTournament, tournament.name, "StyleLabel2", -1, -1));
                StackPanel spTournamentInfos = new StackPanel();
                spTournamentInfos.Orientation = Orientation.Vertical;
                if(tournament.previousEditions.Count > 0)
                {
                    int closestYear = tournament.previousEditions.Aggregate((l, r) => l.Key > r.Key ? l : r).Key;
                    Club winner = tournament.previousEditions[closestYear].Winner();
                    string winnerName = winner != null ? winner.name : "-";
                    spTournamentInfos.Children.Add(ViewUtils.CreateLabel(String.Format("{0}", FindResource("str_titleholder").ToString()), "StyleLabel2", 10, -1, null, null, true));
                    spTournamentInfos.Children.Add(ViewUtils.CreateLabel(String.Format("{0}", winnerName), "StyleLabel2", 9, -1));
                }
                spTournament.Children.Add(spTournamentName);
                spTournament.Children.Add(spTournamentInfos);
                border.Child = spTournament;
                spTournaments.Children.Add(border);

            }
        }

        private void FillHistory()
        {
            foreach(Continent c in Session.Instance.Game.kernel.world.GetAllContinents())
            {
                if(c.countries.Count == 0 || nationalTeam.country.Continent == c)
                {
                    foreach (Tournament t in c.Tournaments())
                    {
                        if (t.rounds.Last().qualifications.Count == 0 && t.previousEditions.Count > 0 && (t.previousEditions.Values.Last().rounds.Last().clubs.Count > 0) && (t.previousEditions.Values.Last().rounds.Last().clubs[0] as NationalTeam) != null)
                        {
                            StackPanel spTournament = new StackPanel();
                            spTournament.Orientation = Orientation.Vertical;
                            spTournament.Children.Add(ViewUtils.CreateLabel(t.name, "StyleLabel2", 12, -1));
                            foreach (KeyValuePair<int, Tournament> previousEdition in t.previousEditions)
                            {
                                Tournament tournament = previousEdition.Value;
                                string teamPerformance = null;
                                for(int i = tournament.rounds.Count-1; i>=0 && teamPerformance == null; i--)
                                {
                                    if (tournament.rounds[i].clubs.Contains(nationalTeam))
                                    {
                                        teamPerformance = (i == tournament.rounds.Count-1 && tournament.Winner() == nationalTeam) ? FindResource("str_winner").ToString() : tournament.rounds[i].name;
                                    }
                                }
                                teamPerformance = teamPerformance == null ? FindResource("str_notQualified").ToString() : teamPerformance;

                                StackPanel spEdition = new StackPanel();
                                spEdition.Orientation = Orientation.Horizontal;
                                spEdition.Children.Add(ViewUtils.CreateLabel(String.Format("{0}", previousEdition.Key), "StyleLabel2", 10, 75));
                                spEdition.Children.Add(ViewUtils.CreateLabel(teamPerformance, "StyleLabel2", 10, 500));
                                spTournament.Children.Add(spEdition);

                            }
                            spHistoryTournaments.Children.Add(spTournament);

                        }
                    }

                }
            }
        }

        private void FillMainPanel()
        {
            List<Player> nationalPlayers = nationalTeam.Players();
            ViewPlayers vp = new ViewPlayers(nationalPlayers, 10, true, true, false, true, true, false, false, true, false, false, false, false, false, true, true, false, false);
            vp.Full(spTabTeam);

            List<Match> nationalMatchs = nationalTeam.Games;
            ViewMatches vm = new ViewMatches(nationalMatchs, true, false, false, true, false, true, 10, true, nationalTeam);
            vm.Full(spTabMatchs);

            //Fifa Ranking
            int archivalEntries = nationalTeam.archivalFifaPoints.Count;
            int[] totalCountries = new int[archivalEntries];
            int[] rankings = new int[archivalEntries];
            labelsYears = new string[archivalEntries];
            DateTime dt = new DateTime(Session.Instance.Game.date.Year, Session.Instance.Game.date.Month, 1);

            for (int i = 0; i < archivalEntries; i++)
            {
                totalCountries[i] = 0;
                rankings[i] = 0;
                labelsYears[archivalEntries-i-1] = String.Format("{0}", dt.ToString("MM/yyyy"));
                foreach(Club c in Session.Instance.Game.kernel.Clubs)
                {
                    NationalTeam nt = c as NationalTeam;
                    if(nt != null)
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
            if (archivalEntries > 0)
            {
                chartFifaAxis.Position = AxisPosition.RightTop;
                chartFifaAxis.MinValue = -totalCountries[0];
                chartFifaAxis.MaxValue = 0;
            }
            
            //Invert ranking for chart display
            for(int i = 0; i<rankings.Length; i++)
            {
                rankings[i] = -rankings[i];
            }
            ChartValues<int> rankingsChart = new ChartValues<int>(rankings);
            rankingsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = FindResource("str_ranking").ToString(),
                    Values = rankingsChart
                }
            };


            //Association Ranking
            Continent continent = nationalTeam.country.Continent;
            int assoArchivalEntries = continent.archivalAssociationRanking.Count;
            totalCountries = new int[assoArchivalEntries];
            rankings = new int[assoArchivalEntries];
            labelsYearsAsso = new string[assoArchivalEntries];

            for (int i = 0; i < assoArchivalEntries; i++)
            {
                totalCountries[i] = 0;
                rankings[i] = 0;
                labelsYearsAsso[assoArchivalEntries - i - 1] = String.Format("{0}", Utils.beginningYear+i);
                rankings[i] = continent.archivalAssociationRanking[i].IndexOf(nationalTeam.country)+1;
                totalCountries[i] = continent.archivalAssociationRanking[i].Count;
            }
            
            if (assoArchivalEntries > 0 && totalCountries[0] > 0)
            {
                chartAssoAxis.Position = AxisPosition.RightTop;
                chartAssoAxis.MinValue = -totalCountries[0];
                chartAssoAxis.MaxValue = 0;
            }

            //Invert ranking for chart display
            for (int i = 0; i < rankings.Length; i++)
            {
                rankings[i] = -rankings[i];
            }
            rankingsChart = new ChartValues<int>(rankings);
            assoRankingsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = FindResource("str_ranking").ToString(),
                    Values = rankingsChart
                }
            };
            DataContext = this;

        }

        private void FillContinentalTeams()
        {
            Continent continent = nationalTeam.country.Continent;
            int tournamentLevel = 1;
            Tournament tournament = continent.GetContinentalClubTournament(tournamentLevel);
            List<Club> clubs = new List<Club>();
            while(tournament != null)
            {
                StackPanel spTournament = new StackPanel();
                spTournament.Orientation = Orientation.Vertical;
                StackPanel spTournamentHeader = new StackPanel();
                spTournamentHeader.Orientation = Orientation.Horizontal;
                spTournamentHeader.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(tournament.color.ToHexa()));
                spTournamentHeader.Children.Add(ViewUtils.CreateLogo(tournament, 50, 50));
                spTournamentHeader.Children.Add(ViewUtils.CreateLabelOpenWindow<Tournament>(tournament, OpenTournament, tournament.name, "StyleLabel2", -1, -1));
                spTournament.Children.Add(spTournamentHeader);

                for (int i = tournament.rounds.Count-1; i > -1; i--)
                {
                    Round r = tournament.rounds[i];
                    foreach(Club c in r.clubs)
                    {
                        if(c.Country() == nationalTeam.country && !clubs.Contains(c) && (c as CityClub) != null)
                        {
                            clubs.Add(c);
                            spTournament.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(c, OpenClub, String.Format("{0} ({1})", c.shortName, r.name), "StyleLabel2", 11, -1));
                        }
                    }
                }
                spQualifiedTeams.Children.Add(spTournament);
                tournamentLevel++;
                tournament = continent.GetContinentalClubTournament(tournamentLevel);
            }
        }

        private void Init()
        {
            lbCountryName.Content = nationalTeam.name;
            List<NationalTeam> fifaRanking = Session.Instance.Game.kernel.FifaRanking();
            int continentalRank = 0;
            int continentalTeams = 0;
            for(int i = 0; i < fifaRanking.Count; i++)
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
            gridHeader.Children.Add(CreateGridStackPanel(String.Format("{0}/{1}", (fifaRanking.IndexOf(nationalTeam) + 1), fifaRanking.Count), ViewUtils.CreateImage("universe\\world.png", 40, 40), 4, 0));
            gridHeader.Children.Add(CreateGridStackPanel(String.Format("{0}/{1}", continentalRank, continentalTeams), ViewUtils.CreateContinentLogo(nationalTeam.country.Continent, 40, 40), 4, 1));
            int associationIndex = nationalTeam.country.Continent.associationRanking.IndexOf(nationalTeam.country);
            string associationRankingStr = associationIndex > -1 ? String.Format("{0}/{1} (clubs)", associationIndex + 1, nationalTeam.country.Continent.associationRanking.Count) : String.Format("{0} (clubs)", FindResource("str_notranked").ToString());
            gridHeader.Children.Add(CreateGridStackPanel(associationRankingStr, ViewUtils.CreateContinentLogo(nationalTeam.country.Continent, 40, 40), 4, 2));

            ImageBrush ib = new ImageBrush();
            Image flag = ViewUtils.CreateFlag(nationalTeam.country, 700, 300);
            ib.ImageSource = flag.Source;
            ib.Stretch = Stretch.UniformToFill;
            ib.Opacity = 0.25;
            gridHeader.Background = ib;

            FillTournaments();
            FillContinentalTeams();
            FillMainPanel();
            FillHistory();

        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
