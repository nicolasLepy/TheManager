using LiveCharts;
using LiveCharts.Wpf;
using System;
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
                buts.Add(hj.Goals);
                joues.Add(hj.GamesPlayed);
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

            if(_player.history.Count > 0)
            {
                Club last = null;
                int arrival = _player.history[0].Year;
                foreach (PlayerHistory hj in _player.history)
                {
                    if (last != hj.Club)
                    {
                        string nameClub = "";
                        if(last == null)
                        {
                            nameClub = FindResource("str_free").ToString();
                        }
                        else
                        {
                            nameClub = last.name;
                        }
                        int depart = hj.Year;

                        StackPanel line = new StackPanel();
                        line.Orientation = Orientation.Horizontal;
                        line.Children.Add(ViewUtils.CreateLabel((arrival - 1).ToString() + " - " + _player.history[_player.history.Count - 1].Year.ToString(), "StyleLabel2", 11, 80));
                        line.Children.Add(ViewUtils.CreateLabel(nameClub, "StyleLabel2", 11, 100));
                        line.Children.Add(ViewUtils.CreateLabel(cumulativeMatchesPlayed.ToString(), "StyleLabel2", 11, 40));
                        line.Children.Add(ViewUtils.CreateLabel(cumulativeGoals.ToString(), "StyleLabel2", 11, 40));
                        spPlayerHistory.Children.Add(line);

                        cumulativeGoals = 0;
                        cumulativeMatchesPlayed = 0;
                        arrival = hj.Year;
                    }
                    cumulativeGoals += hj.Goals;
                    cumulativeMatchesPlayed += hj.GamesPlayed;
                    last = hj.Club;
                }

                string clubName = "";
                if (last == null)
                {
                    clubName = FindResource("str_free").ToString();
                }
                else
                {
                    clubName = last.name;
                }


                StackPanel lastLine = new StackPanel();
                lastLine.Orientation = Orientation.Horizontal;
                lastLine.Children.Add(ViewUtils.CreateLabel((arrival-1).ToString() + " - " + _player.history[_player.history.Count - 1].Year.ToString(), "StyleLabel2", 11, 80));
                lastLine.Children.Add(ViewUtils.CreateLabel(clubName, "StyleLabel2", 11, 100));
                lastLine.Children.Add(ViewUtils.CreateLabel(cumulativeMatchesPlayed.ToString(), "StyleLabel2", 11, 40));
                lastLine.Children.Add(ViewUtils.CreateLabel(cumulativeGoals.ToString(), "StyleLabel2", 11, 40));
                spPlayerHistory.Children.Add(lastLine);
            }
        }
    }
}
