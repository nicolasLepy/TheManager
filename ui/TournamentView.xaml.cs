using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using TheManager.Tournaments;
using TheManager_GUI.pages;
using TheManager_GUI.Styles;
using TheManager_GUI.views;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour TournamentView.xaml
    /// </summary>
    public partial class TournamentView : Window
    {
        private Tournament baseTournament;
        private Tournament activeTournament;

        private MapView map;

        public TournamentView(Tournament tournament)
        {
            foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                if(tournament.name == t.name)
                {
                    this.baseTournament = t;
                    this.activeTournament = t;

                }
            }
            //this.baseTournament = tournament;
            //this.activeTournament = tournament;
            InitializeComponent();
            map = new MapView();
            Initialize();
            UpdatePage(new TournamentResultsPage(activeTournament));
        }

        public void UpdatePage(Grid grid)
        {
            frameHostContent.Content = grid;
        }

        public void UpdatePage(Page page)
        {
            frameHostContent.Content = page;
        }

        public void Initialize()
        {
            textTournamentName.Text = baseTournament.name;
            FillComboBoxYear();
        }

        private void Refresh()
        {
            UpdatePage(new TournamentResultsPage(activeTournament));
        }

        private void FillComboBoxYear()
        {
            comboBoxSeasons.Items.Clear();

            ComboBoxItem cbi = new ComboBoxItem();
            cbi.Content = FindResource("str_currentSeason").ToString();
            comboBoxSeasons.Items.Add(cbi);
            comboBoxSeasons.SelectedIndex = 0;
            cbi.Selected += new RoutedEventHandler((s, e) => NewYearSelected(new KeyValuePair<int, Tournament>(-1, baseTournament)));

            foreach (KeyValuePair<int, Tournament> history in baseTournament.previousEditions)
            {
                //It's a civil year championship is the last round ending is after the season beginning on the same year
                bool civilYearChampionship = history.Value.rounds.Last().programmation.end.WeekNumber > history.Value.seasonBeginning.WeekNumber;
                cbi = new ComboBoxItem();
                cbi.Content = FindResource("str_season").ToString() + " " + (!civilYearChampionship ? (history.Key - 1) + "-" : "") + history.Key;
                cbi.Selected += new RoutedEventHandler((s, e) => NewYearSelected(history));
                comboBoxSeasons.Items.Add(cbi);
            }
        }

        private void NewYearSelected(KeyValuePair<int, Tournament> history)
        {
            activeTournament = history.Value;
            Refresh();
        }

        private void buttonStatResults_Click(object sender, RoutedEventArgs e)
        {
            UpdatePage(new TournamentResultsPage(activeTournament));
        }

        private void buttonStatGoalscorers_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { FindResource("str_goals").ToString()});

            List<KeyValuePair<Player, List<string>>> stats = new List<KeyValuePair<Player, List<string>>>();
            foreach (KeyValuePair<Player, int> goalscorer in activeTournament.Goalscorers())
            {
                stats.Add(new KeyValuePair<Player, List<string>>(goalscorer.Key, new List<string>() { goalscorer.Value.ToString() }));
            }
            page.Full(stats);
            UpdatePage(page);
        }

        private void buttonStatPossession_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { FindResource("str_possession").ToString(), FindResource("str_matchPlayed").ToString() });
            Dictionary<Club, float> possessions = new Dictionary<Club, float>();
            Dictionary<Club, int> playedGames = new Dictionary<Club, int>();
            foreach (Round r in activeTournament.rounds)
            {
                foreach (Match m in r.matches)
                {
                    if (!possessions.ContainsKey(m.home))
                    {
                        possessions.Add(m.home, 0);
                        playedGames.Add(m.home, 0);
                    }
                    if (!possessions.ContainsKey(m.away))
                    {
                        possessions.Add(m.away, 0);
                        playedGames.Add(m.away, 0);
                    }
                    possessions[m.home] += m.statistics.HomePossession;
                    possessions[m.away] += m.statistics.AwayPossession;
                    playedGames[m.home]++;
                    playedGames[m.away]++;
                }
            }
            List<KeyValuePair<Club, float>> list = possessions.ToList();
            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            List<KeyValuePair<Club, List<string>>> stats = new List<KeyValuePair<Club, List<string>>>();
            
            foreach (KeyValuePair<Club, float> kvp in list)
            {
                stats.Add(new KeyValuePair<Club, List<string>>(kvp.Key, new List<string>() { String.Format("{0}%", (kvp.Value / playedGames[kvp.Key] * 100)), playedGames[kvp.Key].ToString() }));
            }
            page.Full(stats);
            UpdatePage(page);
        }

        private void buttonStatShots_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { FindResource("str_shots").ToString(), FindResource("str_matchPlayed").ToString() });
            Dictionary<Club, float> possessions = new Dictionary<Club, float>();
            Dictionary<Club, int> playedGames = new Dictionary<Club, int>();
            foreach (Round r in activeTournament.rounds)
            {
                foreach (Match m in r.matches)
                {
                    if (!possessions.ContainsKey(m.home))
                    {
                        possessions.Add(m.home, 0);
                        playedGames.Add(m.home, 0);
                    }
                    if (!possessions.ContainsKey(m.away))
                    {
                        possessions.Add(m.away, 0);
                        playedGames.Add(m.away, 0);
                    }
                    possessions[m.home] += m.statistics.HomeShoots;
                    possessions[m.away] += m.statistics.AwayShoots;
                    playedGames[m.home]++;
                    playedGames[m.away]++;
                }
            }
            List<KeyValuePair<Club, float>> list = possessions.ToList();
            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            List<KeyValuePair<Club, List<string>>> stats = new List<KeyValuePair<Club, List<string>>>();

            foreach (KeyValuePair<Club, float> kvp in list)
            {
                stats.Add(new KeyValuePair<Club, List<string>>(kvp.Key, new List<string>() { kvp.Value.ToString(), playedGames[kvp.Key].ToString() }));
            }
            page.Full(stats);
            UpdatePage(page);
        }

        private void DisplayMap(Grid host, Tournament tournament, Round round)
        {
            bool nationalTeamTournament = round.clubs.Count > 0 && round.clubs[0] as NationalTeam != null;
            MapType mapType = nationalTeamTournament ? MapType.INTERNATIONAL : MapType.CLUB;
            Dictionary<int, int> colorMap = new Dictionary<int, int>();
            Dictionary<int, string> colorLegend = new Dictionary<int, string>();
            int zoomLevel;
            List<MapClub> mapClubs = new List<MapClub>();

            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(tournament);
            if (localisation as Country != null)
            {
                zoomLevel = (localisation as Country).ShapeNumber;
            }
            else
            {
                zoomLevel = MapView.ZoomEurope; //TODO
            }

            //Tournament with national teams
            if (nationalTeamTournament)
            {
                Club winner = tournament.Winner();
                Dictionary<NationalTeam, int> clubCourses = new Dictionary<NationalTeam, int>();
                foreach (Club club in tournament.Clubs())
                {
                    string teamPerformance = null;
                    for (int i = tournament.rounds.Count - 1; i >= 0 && teamPerformance == null; i--)
                    {
                        if (tournament.rounds[i].clubs.Contains(club) && !clubCourses.ContainsKey(club as NationalTeam))
                        {
                            clubCourses[club as NationalTeam] = i + 1 + (club == winner ? 1 : 0);
                            if (!colorMap.ContainsKey(club.Country().ShapeNumber))
                            {
                                colorMap.Add(club.Country().ShapeNumber, i + 1 + (club == winner ? 1 : 0));
                            }
                        }
                    }
                }
                for (int i = 0; i < tournament.rounds.Count; i++)
                {
                    colorLegend.Add(i + 1, tournament.rounds[i].name);
                }
                if(winner != null)
                {
                    colorLegend.Add(tournament.rounds.Count+1, FindResource("str_winner").ToString());
                }
            }
            else
            {
                foreach (Club c in round.clubs)
                {
                    CityClub cc = c as CityClub;
                    if (cc != null)
                    {
                        mapClubs.Add(new MapClub(cc.city.Position.Latitude, cc.city.Position.Longitude, Utils.Logo(cc)));
                    }
                    ReserveClub rc = c as ReserveClub;
                    if (rc != null)
                    {
                        mapClubs.Add(new MapClub(rc.FannionClub.city.Position.Latitude, rc.FannionClub.city.Position.Longitude, Utils.Logo(rc)));
                    }
                }
            }
            map.Refresh(mapType, colorMap, colorLegend, zoomLevel, mapClubs);
            host.Children.Clear();
            map.Show(host);

        }

        private void buttonStatMap_Click(object sender, RoutedEventArgs e)
        {
            Round round = activeTournament.rounds[0];
            bool nationalTeamTournament = round.clubs.Count > 0 && round.clubs[0] as NationalTeam != null;

            Grid mainPanel = new Grid();
            mainPanel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) });
            mainPanel.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.8, GridUnitType.Star) });
            mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.4, GridUnitType.Star) });
            mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.2, GridUnitType.Star) });
            mainPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.4, GridUnitType.Star) });

            Border borderRound = new Border();
            borderRound.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(125, System.Windows.Media.Brushes.Gray.Color.R, System.Windows.Media.Brushes.Gray.Color.G, System.Windows.Media.Brushes.Gray.Color.B));
            ComboBox comboRounds = new ComboBox();
            comboRounds.Margin = new Thickness(15);
            comboRounds.Style = FindResource(StyleDefinition.comboBoxFlatStyle) as Style;
            foreach(Round tournamentRound in activeTournament.rounds)
            {
                comboRounds.Items.Add(tournamentRound.name);
            }
            if(comboRounds.Items.Count > 0)
            {
                comboRounds.SelectedIndex = 0;
            }
            Grid gridMap = new Grid();
            gridMap.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.15, GridUnitType.Star) });
            gridMap.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.8, GridUnitType.Star) });
            gridMap.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.2, GridUnitType.Star) });
            gridMap.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.6, GridUnitType.Star) });
            gridMap.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.2, GridUnitType.Star) });

            comboRounds.SelectionChanged += (s, ce) => ComboRounds_SelectionChanged(gridMap, activeTournament, activeTournament.rounds[comboRounds.SelectedIndex]);

            ViewUtils.AddElementToGrid(mainPanel, gridMap, 0, 0, 3, 2);
            ViewUtils.AddElementToGrid(mainPanel, borderRound, 0, 1);
            ViewUtils.AddElementToGrid(mainPanel, comboRounds, 0, 1);
            DisplayMap(gridMap, activeTournament, round);
            UpdatePage(mainPanel);

        }

        private void ComboRounds_SelectionChanged(Grid host, Tournament tournament, Round round)
        {
            DisplayMap(host, tournament, round);
        }

        private void buttonStatBudget_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { FindResource("str_budget").ToString(), FindResource("str_sponsors").ToString() });

            List<Club> clubs = new List<Club>(activeTournament.Clubs());
            clubs.Sort(new ClubComparator(ClubAttribute.BUDGET, false));

            List<KeyValuePair<Club, List<string>>> stats = new List<KeyValuePair<Club, List<string>>>();

            foreach (Club c in clubs)
            {
                if (c as CityClub != null)
                {
                    stats.Add(new KeyValuePair<Club, List<string>>(c, new List<string>() { Utils.FormatMoney((c as CityClub).budget), Utils.FormatMoney((c as CityClub).sponsor) }));
                }
            }

            page.Full(stats);
            UpdatePage(page);
        }

        private void buttonStatStadiums_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { FindResource("str_stadium").ToString(), FindResource("str_averageAttendance").ToString(), FindResource("str_capacity").ToString() }, new List<int>() { 3, 2, 1});

            List<Club> clubs = new List<Club>(activeTournament.Clubs());
            clubs.Sort(new ClubComparator(ClubAttribute.STADIUM, false));

            List<KeyValuePair<Club, List<string>>> stats = new List<KeyValuePair<Club, List<string>>>();

            Dictionary<Club, float> attendances = new Dictionary<Club, float>();
            foreach (Club c in activeTournament.Clubs())
            {
                attendances.Add(c, 0);
                int count = 0;
                foreach (Round r in activeTournament.rounds)
                {
                    foreach (Match m in r.matches)
                    {
                        if (m.home == c)
                        {
                            count++;
                            attendances[c] += m.attendance;
                        }
                    }
                }
                if (count > 0)
                {
                    attendances[c] /= count;
                }
            }

            foreach (Club c in clubs)
            {
                stats.Add(new KeyValuePair<Club, List<string>>(c, new List<string>() { c.stadium.name, attendances[c].ToString("0"), c.stadium.capacity.ToString()}));
            }

            page.Full(stats);
            UpdatePage(page);

        }

        private void buttonStatLevel_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { FindResource("str_level").ToString()});

            List<Club> clubs = new List<Club>(activeTournament.Clubs());
            clubs.Sort(new ClubComparator(ClubAttribute.LEVEL, false));

            List<KeyValuePair<Club, List<string>>> stats = new List<KeyValuePair<Club, List<string>>>();
            foreach (Club c in clubs)
            {
                stats.Add(new KeyValuePair<Club, List<string>>(c, new List<string>() { c.Level().ToString("0.0") }));
            }
            page.Full(stats);
            UpdatePage(page);

        }

        private void buttonStatPotential_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { FindResource("str_potential").ToString() });

            List<Club> clubs = new List<Club>(activeTournament.Clubs());
            clubs.Sort(new ClubComparator(ClubAttribute.LEVEL, false));

            List<KeyValuePair<Club, List<string>>> stats = new List<KeyValuePair<Club, List<string>>>();
            foreach (Club c in clubs)
            {
                stats.Add(new KeyValuePair<Club, List<string>>(c, new List<string>() { c.Potential().ToString("0.0") }));
            }
            page.Full(stats);
            UpdatePage(page);
        }

        private void buttonStatTotalRanking_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { FindResource("str_seasonsByClub").ToString(), FindResource("str_ranking_points").ToString(), FindResource("str_ranking_played").ToString(), FindResource("str_ranking_wins").ToString(), FindResource("str_ranking_draws").ToString(), FindResource("str_ranking_loses").ToString(), FindResource("str_ranking_goals_for").ToString(), FindResource("str_ranking_goals_against").ToString(), FindResource("str_ranking_goals_diff").ToString() });

            Dictionary<Club, int> clubsSeasons = new Dictionary<Club, int>();
            Dictionary<Club, int> clubsPoints = new Dictionary<Club, int> ();
            Dictionary<Club, int> clubsWins = new Dictionary<Club, int> ();
            Dictionary<Club, int> clubsDraws = new Dictionary<Club, int> ();
            Dictionary<Club, int> clubsLoses = new Dictionary<Club, int> ();
            Dictionary<Club, int> clubsGoalsFor = new Dictionary<Club, int> ();
            Dictionary<Club, int> clubsGoalsAgainst = new Dictionary<Club, int> ();
            foreach (KeyValuePair<int, Tournament> t in baseTournament.previousEditions)
            {
                if (t.Value.isChampionship)
                {
                    foreach (Club c in t.Value.rounds[0].clubs)
                    {
                        if (!clubsSeasons.ContainsKey(c))
                        {
                            clubsSeasons.Add(c, 0);
                        }
                        clubsSeasons[c]++;
                    }
                }
                else
                {
                    List<Club> clubsList = new List<Club>();
                    foreach (Round r in t.Value.rounds)
                    {
                        foreach (Club c in r.clubs)
                        {
                            if (!clubsList.Contains(c))
                            {
                                clubsList.Add(c);
                            }
                        }
                    }
                    foreach (Club c in clubsList)
                    {
                        if (!clubsSeasons.ContainsKey(c))
                        {
                            clubsSeasons.Add(c, 0);
                        }
                        clubsSeasons[c]++;
                    }
                }
                foreach(Round r in t.Value.rounds)
                {
                    foreach(Club c in r.clubs)
                    {
                        if(!clubsPoints.ContainsKey(c))
                        {
                            clubsPoints.Add(c, 0);
                            clubsWins.Add(c, 0);
                            clubsDraws.Add(c, 0);
                            clubsLoses.Add(c, 0);
                            clubsGoalsFor.Add(c, 0);
                            clubsGoalsAgainst.Add(c, 0);
                        }
                        clubsPoints[c] += r.Points(c);
                        clubsWins[c] += r.Wins(c);
                        clubsDraws[c] += r.Draws(c);
                        clubsLoses[c] += r.Loses(c);
                        clubsGoalsFor[c] += r.GoalsFor(c);
                        clubsGoalsAgainst[c] += r.GoalsAgainst(c);
                    }
                }
            }
            List<KeyValuePair<Club, int>> list = clubsSeasons.ToList();
            list.Sort((pair1, pair2) => pair2.Value != pair1.Value ? pair2.Value.CompareTo(pair1.Value) : clubsPoints[pair2.Key].CompareTo(clubsPoints[pair1.Key]));

            List<KeyValuePair<Club, List<string>>> stats = new List<KeyValuePair<Club, List<string>>>();
            foreach (KeyValuePair<Club, int> club in list)
            {
                int gamesPlayed = clubsWins[club.Key] + clubsDraws[club.Key] + clubsLoses[club.Key];
                int goalAverage = clubsGoalsFor[club.Key] - clubsGoalsAgainst[club.Key];
                stats.Add(new KeyValuePair<Club, List<string>>(club.Key, new List<string>() { club.Value.ToString(), clubsPoints[club.Key].ToString(), gamesPlayed.ToString(), clubsWins[club.Key].ToString(), clubsDraws[club.Key].ToString(), clubsLoses[club.Key].ToString(), clubsGoalsFor[club.Key].ToString(), clubsGoalsAgainst[club.Key].ToString(), goalAverage.ToString() }));
            }
            page.Full(stats);
            UpdatePage(page);
        }

        private void buttonStatRecords_Click(object sender, RoutedEventArgs e)
        {
            Grid gridRecords = new Grid();
            gridRecords.Margin = new Thickness(35);
            gridRecords.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.3, GridUnitType.Star) });
            gridRecords.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.7, GridUnitType.Star) });
            gridRecords.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) });
            gridRecords.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) });
            gridRecords.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) });
            gridRecords.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) });
            gridRecords.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) });
            gridRecords.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) });
            
            ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_biggestScore").ToString(), StyleDefinition.styleTextPlain), 0, 0);
            ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_biggestWin").ToString(), StyleDefinition.styleTextPlain), 1, 0);
            ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_bestAttack").ToString(), StyleDefinition.styleTextPlain), 2, 0);
            ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_bestDefense").ToString(), StyleDefinition.styleTextPlain), 3, 0);
            ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_worstAttack").ToString(), StyleDefinition.styleTextPlain), 4, 0);
            ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_worstDefense").ToString(), StyleDefinition.styleTextPlain), 5, 0);
            if(baseTournament.statistics.LargerScore != null)
            {
                StackPanel panelBiggestScore = new StackPanel() { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center };
                ViewScores view = new ViewScores(new List<Match>() { baseTournament.statistics.LargerScore }, true, false, false, true, false, false, (float)(double)FindResource(StyleDefinition.fontSizeRegular));
                view.Full(panelBiggestScore);
                ViewUtils.AddElementToGrid(gridRecords, panelBiggestScore, 0, 1);
            }
            if (baseTournament.statistics.BiggerScore != null)
            {
                StackPanel panelBiggestScore = new StackPanel() { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center };
                ViewScores view = new ViewScores(new List<Match>() { baseTournament.statistics.BiggerScore }, true, false, false, true, false, false, (float)(double)FindResource(StyleDefinition.fontSizeRegular));
                view.Full(panelBiggestScore);
                ViewUtils.AddElementToGrid(gridRecords, panelBiggestScore, 1, 1);
            }

            KeyValuePair<int, KeyValuePair<Club, int>> bestAttack = new KeyValuePair<int, KeyValuePair<Club, int>>(-1, new KeyValuePair<Club, int>(null, -1));
            KeyValuePair<int, KeyValuePair<Club, int>> bestDefense = new KeyValuePair<int, KeyValuePair<Club, int>>(-1, new KeyValuePair<Club, int>(null, -1));
            KeyValuePair<int, KeyValuePair<Club, int>> worstAttack = new KeyValuePair<int, KeyValuePair<Club, int>>(-1, new KeyValuePair<Club, int>(null, -1));
            KeyValuePair<int, KeyValuePair<Club, int>> worstDefense = new KeyValuePair<int, KeyValuePair<Club, int>>(-1, new KeyValuePair<Club, int>(null, -1));
            foreach (KeyValuePair<int, Tournament> t in baseTournament.previousEditions)
            {
                if (t.Value.isChampionship)
                {
                    Round r = t.Value.rounds[0];
                    foreach (Club c in r.clubs)
                    {
                        int goalsFor = r.GoalsFor(c);
                        int goalsAgainst = r.GoalsAgainst(c);
                        if (goalsFor > bestAttack.Value.Value || bestAttack.Key == -1)
                        {
                            bestAttack = new KeyValuePair<int, KeyValuePair<Club, int>>(t.Key, new KeyValuePair<Club, int>(c, goalsFor));
                        }
                        if (goalsFor < worstAttack.Value.Value || worstAttack.Key == -1)
                        {
                            worstAttack = new KeyValuePair<int, KeyValuePair<Club, int>>(t.Key, new KeyValuePair<Club, int>(c, goalsFor));
                        }
                        if (goalsAgainst < bestDefense.Value.Value || bestDefense.Key == -1)
                        {
                            bestDefense = new KeyValuePair<int, KeyValuePair<Club, int>>(t.Key, new KeyValuePair<Club, int>(c, goalsAgainst));
                        }
                        if (goalsAgainst > worstDefense.Value.Value || worstDefense.Key == -1)
                        {
                            worstDefense = new KeyValuePair<int, KeyValuePair<Club, int>>(t.Key, new KeyValuePair<Club, int>(c, goalsAgainst));
                        }
                    }
                }
            }

            if(baseTournament.isChampionship)
            {
                ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(bestAttack.Key == -1 ? FindResource("str_noplayedgames").ToString() : bestAttack.Value.Key + " (" + bestAttack.Value.Value + " " + FindResource("str_mgoals").ToString() + ", " + bestAttack.Key + ")", StyleDefinition.styleTextPlain), 2, 1);
                ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(bestDefense.Key == -1 ? FindResource("str_noplayedgames").ToString() : bestDefense.Value.Key + " (" + bestDefense.Value.Value + " " + FindResource("str_mgoals").ToString() + ", " + bestDefense.Key + ")", StyleDefinition.styleTextPlain), 3, 1);
                ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(worstAttack.Key == -1 ? FindResource("str_noplayedgames").ToString() : worstAttack.Value.Key + " (" + worstAttack.Value.Value + " " + FindResource("str_mgoals").ToString() + ", " + worstAttack.Key + ")", StyleDefinition.styleTextPlain), 4, 1);
                ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(worstDefense.Key == -1 ? FindResource("str_noplayedgames").ToString() : worstDefense.Value.Key + " (" + worstDefense.Value.Value + " " + FindResource("str_mgoals").ToString() + ", " + worstDefense.Key + ")", StyleDefinition.styleTextPlain), 5, 1);
            }
            else
            {
                ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_noAvailableForCup").ToString(), StyleDefinition.styleTextPlain), 2, 1);
                ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_noAvailableForCup").ToString(), StyleDefinition.styleTextPlain), 3, 1);
                ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_noAvailableForCup").ToString(), StyleDefinition.styleTextPlain), 4, 1);
                ViewUtils.AddElementToGrid(gridRecords, ViewUtils.CreateTextBlock(FindResource("str_noAvailableForCup").ToString(), StyleDefinition.styleTextPlain), 5, 1);
            }
            
            UpdatePage(gridRecords);

        }

        private void buttonStatAchievementsClub_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(true);
            page.InitializeTable(new List<string>() { "", FindResource("str_years").ToString() }, new List<int>() { 1, 9});

            Dictionary<Club, List<int>> palmares = new Dictionary<Club, List<int>>();

            foreach (KeyValuePair<int, Tournament> arc in baseTournament.previousEditions)
            {
                Club winner = arc.Value.Winner();
                if (winner != null)
                {
                    if (!palmares.ContainsKey(winner))
                    {
                        palmares.Add(winner, new List<int>());
                    }
                    palmares[winner].Add(arc.Key);
                }
            }

            List<KeyValuePair<Club, List<int>>> list = palmares.ToList();
            list.Sort((pair1, pair2) => pair2.Value.Count.CompareTo(pair1.Value.Count));

            List<KeyValuePair<Club, List<string>>> stats = new List<KeyValuePair<Club, List<string>>>();
            foreach (KeyValuePair<Club, List<int>> club in list)
            {
                stats.Add(new KeyValuePair<Club, List<string>>(club.Key, new List<string>() { club.Value.Count.ToString(), string.Join(", ", club.Value) }));
            }
            page.Full(stats);
            UpdatePage(page);

        }

        private void buttonStatYears_Click(object sender, RoutedEventArgs e)
        {
            TournamentStatisticPage page = new TournamentStatisticPage(false);
            if(baseTournament.isChampionship)
            {
                page.InitializeTable(new List<string>() { "", FindResource("str_winner").ToString(), "", FindResource("str_runnerup").ToString(), "", FindResource("str_third").ToString() }, null, new List<bool>() { true, false, true, false, true, false });
            }
            else
            {
                page.InitializeTable(new List<string>() { "", FindResource("str_winner").ToString(), "", FindResource("str_runnerup").ToString() }, null, new List<bool>() { true, false, true, false });
            }

            List<KeyValuePair<string, List<string>>> stats = new List<KeyValuePair<string, List<string>>>();

            foreach (KeyValuePair<int, Tournament> arc in baseTournament.previousEditions)
            {
                Round t = arc.Value.rounds[arc.Value.rounds.Count - 1];
                //If the final round was not inactive, we can make the palmares
                if ((t as GroupInactiveRound) == null)
                {
                    List<Club> finalPhaseClubs = baseTournament.isChampionship ? arc.Value.GetFinalPhasesClubs() : new List<Club>();
                    ChampionshipRound lastChampionshipRound = arc.Value.GetLastChampionshipRound() as ChampionshipRound;
                    Club winner = arc.Value.Winner();
                    Club runnerup = baseTournament.isChampionship ? (finalPhaseClubs.Count > 1 ? finalPhaseClubs[1] : (lastChampionshipRound.Ranking().Count > 1 ? lastChampionshipRound.Ranking()[1] : null)) : arc.Value.rounds.Last().matches.Last().Looser;
                    Club third = baseTournament.isChampionship ? (finalPhaseClubs.Count > 2 ? finalPhaseClubs[2] : (lastChampionshipRound.Ranking().Count > 2 ? lastChampionshipRound.Ranking()[2] : null)) : null;


                    int year = arc.Key;
                    KeyValuePair<string, List<string>> statsKvp = new KeyValuePair<string, List<string>>(year.ToString(), new List<string>() { Utils.Logo(winner), winner.name });
                    if(runnerup != null)
                    {
                        statsKvp.Value.Add(Utils.Logo(runnerup));
                        statsKvp.Value.Add(runnerup.name);
                    }
                    if (third != null)
                    {
                        statsKvp.Value.Add(Utils.Logo(third));
                        statsKvp.Value.Add(third.name);
                    }
                    stats.Add(statsKvp);
                }
            }

            page.Full(stats);
            UpdatePage(page);


        }

        private void buttonStatByYears_Click(object sender, RoutedEventArgs e)
        {

            TournamentStatisticPage page = new TournamentStatisticPage(false);
            page.InitializeTable(new List<string>() { FindResource("str_goalsByGame").ToString(), FindResource("str_goalsByGame").ToString(), FindResource("str_goals").ToString(), FindResource("str_YCByGame").ToString(), FindResource("str_RCByGame").ToString() });

            List<KeyValuePair<string, List<string>>> stats = new List<KeyValuePair<string, List<string>>>();

            foreach (KeyValuePair<int, Tournament> t in baseTournament.previousEditions)
            {
                int gameCount = 0;
                int goalCount = 0;
                int yellowCards = 0;
                int redCards = 0;

                foreach (Round r in t.Value.rounds)
                {
                    foreach (Match m in r.matches)
                    {
                        gameCount++;
                        goalCount += m.score1 + m.score2;
                        yellowCards += m.YellowCards;
                        redCards += m.RedCards;
                    }
                }
                stats.Add(new KeyValuePair<string, List<string>>(t.Key.ToString(), new List<string>() { (goalCount / (gameCount + 0.0)).ToString("0.00"), goalCount.ToString(), (yellowCards / (gameCount + 0.0)).ToString("0.00"), (redCards / (gameCount + 0.0)).ToString("0.00") }));
            }
            page.Full(stats);
            UpdatePage(page);
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
