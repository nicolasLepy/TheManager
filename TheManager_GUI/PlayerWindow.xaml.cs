using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TheManager;
using TheManager_GUI.ViewMisc;

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

        private readonly Player _player;

        public Windows_Joueur(Player joueur)
        {
            _player = joueur;
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            lbJoueur.Content = joueur.ToString();
            Club c = joueur.Club;
            if(c != null)
            {
                lbClub.Content = c.name;
                imgClub.Source = new BitmapImage(new Uri(Utils.Logo(c)));
            }
            else
            {
                lbClub.Content = "Libre";
            }
            lbAge.Content = FindResource("str_born").ToString() + " " + joueur.birthday.ToShortDateString() + " (" + joueur.Age + " "+FindResource("str_yo").ToString()+")";
            imgFlag.Source = new BitmapImage(new Uri(Utils.Flag(joueur.nationality)));

            ChartValues<int> niveaux = new ChartValues<int>();
            ChartValues<int> buts = new ChartValues<int>();
            ChartValues<int> joues = new ChartValues<int>();
            foreach (PlayerHistory hj in joueur.history)
            {
                niveaux.Add(hj.Level);
                buts.Add(hj.Goals.Sum(k => k.Value));
                joues.Add(hj.GamesPlayed.Sum(k => k.Value));
            }

            NiveauCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = FindResource("str_level").ToString(),
                    Values = niveaux,
                }
            };

            ButsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = FindResource("str_goalsScored").ToString(),
                    Values = buts,
                }
            };

            MatchsCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = FindResource("str_matchPlayed").ToString(),
                    Values = joues,
                }
            };

            LabelsAnnees = new string[joueur.history.Count];
            int i = 0;
            foreach (PlayerHistory hj in joueur.history)
            {
                LabelsAnnees[i] = hj.Year.ToString();
                i++;
            }
 
            DataContext = this;

            FillPlayerHistory();
            FillPlayerGames();
            lbValue.Content = FindResource("str_value").ToString() + " : " + Utils.FormatMoney(_player.EstimateTransferValue());
            Contract ct = _player.Club == null ? null : _player.Club.FindContract(_player);
            if(ct != null)
            {
                lbContract.Content = FindResource("str_underContractUntil").ToString() + " " + ct.end.ToShortDateString() + " ("+FindResource("str_wageOf").ToString() +" " + Utils.FormatMoney(ct.wage) + " /m)";
            }
            else
            {
                lbContract.Content = FindResource("str_noClub").ToString();
            }
            spLevel.Children.Add(ViewUtils.CreateStarNotation(_player.Stars, 15));
            spPotentiel.Children.Add(ViewUtils.CreateStarNotation(_player.StarsPotential, 15));
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FillPlayerGames()
        {
            //spPlayerGames
            ViewMatches view = new ViewMatches(_player.PlayedGamesThisYear(), true, false, false, false, false, true);
            view.Full(spPlayerGames);
        }

        private void FillPlayerHistory()
        {
            StackPanel firstLine = new StackPanel();
            firstLine.Orientation = Orientation.Horizontal;
            firstLine.Children.Add(ViewUtils.CreateLabel(FindResource("str_duration").ToString(), "StyleLabel2", 11, 80));
            firstLine.Children.Add(ViewUtils.CreateLabel(FindResource("str_club").ToString(), "StyleLabel2", 11, 100));
            firstLine.Children.Add(ViewUtils.CreateLabel(FindResource("str_games").ToString(), "StyleLabel2", 11, 40));
            firstLine.Children.Add(ViewUtils.CreateLabel(FindResource("str_goals").ToString(), "StyleLabel2", 11, 40));
            spPlayerHistory.Children.Add(firstLine);

            int cumulativeGoals = 0;
            int cumulativeMatchesPlayed = 0;
            int totalGoals = 0;
            int totalMatchsPlayed = 0;

            Dictionary<NationalTeam, int[]> nationalTeamHistory = new Dictionary<NationalTeam, int[]>();

            if(_player.history.Count > 0)
            {
                Club last = _player.history[0].Club;
                int arrival = _player.history[0].Year;
                for (int i = 0; i < _player.history.Count+1; i++)
                {
                    PlayerHistory hj = i < _player.history.Count ? _player.history[i] : new PlayerHistory(0, -1, new Dictionary<Club, int>(), new Dictionary<Club, int>(), null);
                    totalGoals += hj.Goals.Sum(k => k.Value);
                    totalMatchsPlayed += hj.GamesPlayed.Sum(k => k.Value);

                    if (i == _player.history.Count || last != hj.Club)
                    {
                        string nameClub = last == null ? FindResource("str_free").ToString() : last.name;
                        int depart = hj.Year;

                        StackPanel line = new StackPanel();
                        line.Orientation = Orientation.Horizontal;
                        line.Children.Add(ViewUtils.CreateLabel(String.Format("{0}-{1}", (arrival - 1), last != _player.Club ? hj.Year.ToString() : ""), "StyleLabel2", 11, 80));
                        line.Children.Add(ViewUtils.CreateLabel(nameClub, "StyleLabel2", 11, 100));
                        line.Children.Add(ViewUtils.CreateLabel(cumulativeMatchesPlayed.ToString(), "StyleLabel2", 11, 40));
                        line.Children.Add(ViewUtils.CreateLabel(cumulativeGoals.ToString(), "StyleLabel2", 11, 40));
                        spPlayerHistory.Children.Add(line);

                        cumulativeGoals = 0;
                        cumulativeMatchesPlayed = 0;
                        arrival = hj.Year;
                    }
                    if(i < _player.history.Count)
                    {
                        foreach (KeyValuePair<Club, int> kvp in hj.Goals)
                        {
                            NationalTeam nt = kvp.Key as NationalTeam;
                            if (nt != null)
                            {
                                if (!nationalTeamHistory.ContainsKey(nt))
                                {
                                    nationalTeamHistory.Add(nt, new int[4]);
                                }
                                nationalTeamHistory[nt][3] += kvp.Value;
                            }
                            else
                            {
                                cumulativeGoals += kvp.Value;
                            }
                        }
                        foreach (KeyValuePair<Club, int> kvp in hj.GamesPlayed)
                        {
                            NationalTeam nt = kvp.Key as NationalTeam;
                            if (nt != null)
                            {
                                if (!nationalTeamHistory.ContainsKey(nt))
                                {
                                    nationalTeamHistory.Add(nt, new int[] { -1, -1, 0, 0 });
                                }
                                nationalTeamHistory[nt][2] += kvp.Value;
                                nationalTeamHistory[nt][1] = nationalTeamHistory[nt][1] == -1 || hj.Year > nationalTeamHistory[nt][1] ? hj.Year : nationalTeamHistory[nt][1];
                                nationalTeamHistory[nt][0] = nationalTeamHistory[nt][0] == -1 || hj.Year < nationalTeamHistory[nt][0] ? hj.Year : nationalTeamHistory[nt][0];
                            }
                            else
                            {
                                cumulativeMatchesPlayed += kvp.Value;
                            }
                        }

                        last = hj.Club;
                    }
                }

                foreach (KeyValuePair<NationalTeam, int[]> kvp in nationalTeamHistory)
                {
                    StackPanel spNational = new StackPanel();
                    spNational.Margin = new Thickness(0, 10, 0, 0);
                    spNational.Orientation = Orientation.Horizontal;
                    spNational.Children.Add(ViewUtils.CreateLabel(String.Format("{0}-{1}", (kvp.Value[0] - 1), _player.IsRetired ? kvp.Value[1].ToString() : ""), "StyleLabel2", 11, 80));
                    spNational.Children.Add(ViewUtils.CreateLabel(kvp.Key.name, "StyleLabel2", 11, 100));
                    spNational.Children.Add(ViewUtils.CreateLabel(kvp.Value[2].ToString(), "StyleLabel2", 11, 40));
                    spNational.Children.Add(ViewUtils.CreateLabel(kvp.Value[3].ToString(), "StyleLabel2", 11, 40));
                    spPlayerHistory.Children.Add(spNational);

                }

                StackPanel spTotal = new StackPanel();
                spTotal.Margin = new Thickness(0, 10, 0, 0);
                spTotal.Orientation = Orientation.Horizontal;
                spTotal.Children.Add(ViewUtils.CreateLabel(String.Format("{0}-{1}",(_player.history[0].Year - 1), _player.IsRetired ? _player.history[_player.history.Count - 1].Year.ToString() : ""), "StyleLabel2", 11, 80));
                spTotal.Children.Add(ViewUtils.CreateLabel("Total", "StyleLabel2", 11, 100));
                spTotal.Children.Add(ViewUtils.CreateLabel(totalMatchsPlayed.ToString(), "StyleLabel2", 11, 40));
                spTotal.Children.Add(ViewUtils.CreateLabel(totalGoals.ToString(), "StyleLabel2", 11, 40));
                spPlayerHistory.Children.Add(spTotal);

            }
        }
    }
}
