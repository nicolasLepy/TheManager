using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.ViewMisc;
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
            ViewMatches view = new ViewMatches(matches, false, true, false, true, true, false);
            view.Full(spGames);
        }

        private void DisplayRound()
        {
            Round rnd = _currentArchive.rounds[_currentRound];
            lbRoundName.Content = rnd.name;
            View vc = FactoryViewRanking.CreerVue(null, rnd, 0.75);
            vc.Full(spRoundRanking);

            List<Match> matches = new List<Match>(rnd.matches);
            if (matches.Count > 0)
            {
                matches.Sort(new MatchDateComparator());
                _resultsCurrentDate = matches[matches.Count - 1].day;
                _firstDateOfRound = matches[0].day;
                _lastDateOfRound = matches[matches.Count - 1].day;
                DisplayDay();

                //Display stats
                int goals = 0;
                int yellowCards = 0;
                int redCards = 0;
                foreach(Match m in matches)
                {
                    goals += m.score1 + m.score2;
                    yellowCards += m.YellowCards;
                    redCards += m.RedCards;
                }
                lbStatsGoals.Content = goals.ToString();
                lbStatsRedCards.Content = redCards.ToString();
                lbStatsYellowCards.Content = yellowCards.ToString();
                lbStatsGoalsNumber.Content = (goals / (matches.Count + 0.0f)).ToString("0.00");

                if (_currentArchive.statistics.LargerScore != null)
                {
                    lbStatsBiggestScore.Content = _currentArchive.statistics.LargerScore.home.name + " " + _currentArchive.statistics.LargerScore.score1 + "-" + _currentArchive.statistics.LargerScore.score2 + " " + _currentArchive.statistics.LargerScore.away.name;
                }
                else
                {
                    lbStatsBiggestScore.Content = "";
                }

                if (_currentArchive.statistics.BiggerScore != null)
                {
                    lbStatsHigherResult.Content = _currentArchive.statistics.LargerScore.home.name + " " + _currentArchive.statistics.LargerScore.score1 + "-" + _currentArchive.statistics.LargerScore.score2 + " " + _currentArchive.statistics.LargerScore.away.name;
                }
                else
                {
                    lbStatsHigherResult.Content = "";
                }

                if (_currentArchive.statistics.BiggestAttack.Value != null)
                {
                    lbStatsAllTimeBestAttack.Content = _currentArchive.statistics.BiggestAttack.Value.name + " (" + _currentArchive.statistics.BiggestAttack.Key + ")";
                }
                else
                {
                    lbStatsAllTimeBestAttack.Content = "";
                }

                if (_currentArchive.statistics.BiggestDefense.Value != null)
                {
                    lbStatsAllTimeBestDefense.Content = _currentArchive.statistics.BiggestDefense.Value.name + " (" + _currentArchive.statistics.BiggestDefense.Key + ")";
                }
                else
                {
                    lbStatsAllTimeBestDefense.Content = "";
                }

                if (_currentArchive.statistics.WeakestAttack.Value != null)
                {
                    lbStatsAllTimeWorstAttack.Content = _currentArchive.statistics.WeakestAttack.Value.name + " (" + _currentArchive.statistics.WeakestAttack.Key + ")";
                }
                else
                {
                    lbStatsAllTimeWorstAttack.Content = "";
                }

                if (_currentArchive.statistics.WeakestDefense.Value != null)
                {
                    lbStatsAllTimeWorstDefense.Content = _currentArchive.statistics.WeakestDefense.Value.name + " (" + _currentArchive.statistics.WeakestDefense.Key + ")";
                }
                else
                {
                    lbStatsAllTimeWorstDefense.Content = "";
                }



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
