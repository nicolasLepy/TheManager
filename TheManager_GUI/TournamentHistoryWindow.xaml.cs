using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour TournamentHistoryWindow.xaml
    /// </summary>
    public partial class TournamentHistoryWindow : Window
    {

        private readonly Tournament _tournament;

        private Tournament _currentArchive;
        private int _currentRound;

        private DateTime _resultsCurrentDate;
        private DateTime _firstDateOfRound;
        private DateTime _lastDateOfRound;

        public TournamentHistoryWindow(Tournament tournament)
        {
            InitializeComponent();
            _tournament = tournament;
            _currentRound = 0;
            FillHistoryList();
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnPreviousDay_Click(object sender, RoutedEventArgs e)
        {
            Round r = _currentArchive.rounds[_currentRound];
            if (r != null)
            {
                if (!Utils.CompareDates(_resultsCurrentDate, _firstDateOfRound))
                {
                    bool pursue = true;
                    while (pursue)
                    {
                        _resultsCurrentDate = _resultsCurrentDate.AddDays(-1);
                        if (r.GetMatchesByDate(_resultsCurrentDate).Count > 0)
                        {
                            pursue = false;
                        }
                    }
                }
                DisplayDay();
            }
        }

        private void BtnNextDay_Click(object sender, RoutedEventArgs e)
        {
            Round r = _currentArchive.rounds[_currentRound];
            if (r != null)
            {
                if (!Utils.CompareDates(_resultsCurrentDate, _lastDateOfRound))
                {
                    bool pursue = true;
                    while (pursue)
                    {
                        _resultsCurrentDate = _resultsCurrentDate.AddDays(1);
                        if (r.GetMatchesByDate(_resultsCurrentDate).Count > 0)
                        {
                            pursue = false;
                        }
                    }
                }
                DisplayDay();
            }

        }

        private void BtnPreviousRound_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRound > 0)
            {
                _currentRound--;
                DisplayRound();
            }
        }

        private void BtnNextRound_Click(object sender, RoutedEventArgs e)
        {
            if (_currentArchive != null && _currentRound < _currentArchive.rounds.Count-1)
            {
                _currentRound++;
                DisplayRound();
            }
        }


        private void FillHistoryList()
        {
            foreach(KeyValuePair<int,Tournament> t in _tournament.previousEditions)
            {
                lbSeasons.Items.Add(t.Key);
            }
        }

        private void DisplayDay()
        {
            List<Match> matches = _currentArchive.rounds[_currentRound].GetMatchesByDate(_resultsCurrentDate);
            matches.Sort(new MatchDateComparator());
            lbDay.Content = _resultsCurrentDate.ToLongDateString();
            MatchesDataGridView view = new MatchesDataGridView(spGames, matches, true, true, true, true, false);
            view.Refresh();
        }

        private void DisplayRound()
        {
            Round rnd = _currentArchive.rounds[_currentRound];
            lbRoundName.Content = rnd.name;
            IVueClassement vc = FabriqueVueClassement.CreerVue(null, rnd, 0.8f);
            vc.Remplir(spRoundRanking);

            List<Match> matches = new List<Match>(rnd.matches);
            if (matches.Count > 0)
            {
                matches.Sort(new MatchDateComparator());
                _resultsCurrentDate = matches[matches.Count - 1].day;
                _firstDateOfRound = matches[0].day;
                _lastDateOfRound = matches[matches.Count - 1].day;
                DisplayDay();
            }

        }

        private void lbSeasons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int t;
            int.TryParse(lbSeasons.SelectedItem.ToString(), out t);
            if(t > 0)
            {
                foreach(var kvp in _tournament.previousEditions)
                {
                    if(kvp.Key == t)
                    {
                        _currentArchive = kvp.Value;
                    }
                }
                if(_currentArchive != null && _currentArchive.rounds.Count > 0)
                {
                    _currentRound = 0;
                    DisplayRound();
                }
            }
        }
    }
}
