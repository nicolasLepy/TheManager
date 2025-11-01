using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using tm;
using ui.controls;
using System.Threading;
using TheManager_GUI.Views;
using TheManager_GUI.controls;
using System.Windows.Controls;
using TheManager_GUI.Styles;
using TheManager_GUI.utils;

namespace TheManager_GUI
{

    public class MatchLive
    {
        public Match match { get; set; }
        public bool isLive { get; set; }

        public MatchLive(Match match, bool isLive)
        {
            this.match = match;
            this.isLive = isLive;
        }
    }

    /// <summary>
    /// Logique d'interaction pour LiveGameView.xaml
    /// </summary>
    public partial class LiveGameView : Window
    {

        private List<MatchLive> matches;
        private Match liveMatch;
        private Round round;
        private bool flagEndThreads;

        private Dictionary<Match, ControlMatchIcon> mapControlMatchs;

        private List<Thread> threads;

        private ControlComposition ccHome;
        private ControlComposition ccAway;
        private ControlComposition ccSubsHome;
        private ControlComposition ccSubsAway;

        private MediaPlayerTM player;

        public LiveGameView(List<Match> matches)
        {
            flagEndThreads = false;
            threads = new List<Thread>();
            InitializeComponent();
            this.matches = new List<MatchLive>();
            foreach(Match m in matches)
            {
                this.matches.Add(new MatchLive(m, true));
            }
            this.liveMatch = matches[0];
            this.round = liveMatch.Round;
            mapControlMatchs = new Dictionary<Match, ControlMatchIcon>();
            player = new MediaPlayerTM();
            Initialize();
        }

        public void Initialize()
        {
            ccHome = new ControlComposition(ControlCompositionType.Composition, liveMatch.home, true);
            ccAway = new ControlComposition(ControlCompositionType.Composition, liveMatch.away, true);
            ccSubsHome = new ControlComposition(ControlCompositionType.Subs, liveMatch.home, false);
            ccSubsAway = new ControlComposition(ControlCompositionType.Subs, liveMatch.away, false);
            ViewUtils.AddElementToGrid(gridLineups, ccHome, 0, 0);
            ViewUtils.AddElementToGrid(gridLineups, ccSubsHome, 1, 0);
            ViewUtils.AddElementToGrid(gridLineups, ccAway, 0, 2);
            ViewUtils.AddElementToGrid(gridLineups, ccSubsAway, 1, 2);
            RefreshComposition();

            this.tbStadium.Text = liveMatch.stadium.name;
            this.tbAttendance.Text = String.Format(FindResource("str_attendance").ToString(), liveMatch.attendance);
            this.tbGameDate.Text = liveMatch.day.ToString("dd/MM/yyyy");
            this.tbGameTime.Text = liveMatch.day.ToString("HH:mm");

            this.tbTournament.Text = liveMatch.Tournament.name;
            this.imageTournament.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.LogoTournament(liveMatch.Tournament)));
            ucScoreboard.Update(liveMatch, true);
            ucStats.Update(liveMatch);
            UpdateRanking();
            InitializeMatchs();
        }

        private void Update(MatchLive m, bool triggerGoalEvent)
        {
            mapControlMatchs[m.match].Update(m.match, !m.isLive);
            if(triggerGoalEvent)
            {
                mapControlMatchs[m.match].TriggerGoalEvent();
            }
        }

        private void InitializeMatchs()
        {
            foreach(MatchLive m in matches)
            {
                ControlMatchIcon control = new ControlMatchIcon();
                control.Margin = new Thickness(0, 20, 0, 20);
                layoutScores.Children.Add(control);
                mapControlMatchs[m.match] = control;
                Update(m, false);
            }
        }

        private void CloseView()
        {
            Close();
        }

        private void RegisterEvent(MatchLive matchLive, MatchEvent evnt)
        {
            ControlEventItem cei = new ControlEventItem();
            cei.Update(matchLive.match, evnt);
            layoutEvents.Children.Insert(0, cei);
        }

        private void RegisterAction(KeyValuePair<string, string> action, Club club)
        {
            tbActionTime.Text = action.Key;
            tbActionDesc.Text = action.Value;
            imageActionClub.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(club)));
        }

        private int SliderSpeed()
        {
            int maxFrequency = 50;
            int minFrequency = 1500;
            return (int)(minFrequency - ((minFrequency - maxFrequency) * (sliderSpeed.Value / 100.0)));

        }

        private void UpdateRanking()
        {
            GroupsRound gr = round as GroupsRound;
            if (round != null && gr != null)
            {
                gr.ClearCache();
                FactoryViewRanking.CreateView(round, 1, false, null, true).Full(layoutRanking);
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            Utils.DisplayThreads();
            for (int i = 0; i < matches.Count; i++)
            {
                int j = i;
                Thread t = new Thread(() => ThreadMatch(matches[j]));
                threads.Add(t);
                t.Start();
            }
            player.Background(liveMatch);
            buttonStart.IsEnabled = false;
            buttonQuit.IsEnabled = true;
        }

        private void StopAllThreads()
        {
            flagEndThreads = true;
            //TODO: Make sure threads are finished
        }

        private void EnsureGamesAreFinished()
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].isLive)
                {
                    Match match = matches[i].match;
                    bool finished = false;
                    while (!finished)
                    {
                        List<MatchFeedback> rm = match.NextMinute();
                        finished = Utils.FeedbackContains(MatchFeedbackEvent.END_GAME, rm);
                    }
                }
            }
        }

        private void RefreshComposition()
        {
            ccHome.Fill(liveMatch.compo1);
            ccAway.Fill(liveMatch.compo2);
            ccSubsHome.Fill(liveMatch.Subs1OnBench);
            ccSubsAway.Fill(liveMatch.Subs2OnBench);
        }

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            if (!matches[0].isLive || MessageBox.Show(FindResource("str_abandon_game").ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Utils.DisplayThreads();
                StopAllThreads();
                EnsureGamesAreFinished();
                Utils.DisplayThreads();
                CloseView();
            }
        }

        /* THREAD MATCH */

        public void ThreadMatch(MatchLive match)
        {

            this.Dispatcher.Invoke(async () =>
            {
                while (match.isLive && !flagEndThreads)
                {
                    await TaskMatch(match);
                }
            });
        }

        async Task TaskMatch(MatchLive game)
        {
            List<MatchFeedback> res = game.match.NextMinute();
            if (Utils.FeedbackContains(MatchFeedbackEvent.END_GAME, res))
            {
                game.isLive = false;
                Update(game, false);
            }

            if (Utils.FeedbackContains(MatchFeedbackEvent.EVENT, res))
            {
                MatchEvent em = game.match.events[game.match.events.Count - 1];
                bool registerAction = new GameEvent[] { GameEvent.Goal, GameEvent.PenaltyGoal, GameEvent.AgGoal, GameEvent.YellowCard, GameEvent.RedCard}.Contains(em.type);

                if (em.type == GameEvent.Goal || em.type == GameEvent.PenaltyGoal || em.type == GameEvent.AgGoal)
                {
                    if (em.club == game.match.home)
                    {
                        player.TriggerEvent(game.match);
                    }
                    player.AddSound("jingle", false);

                    //Goal: need refresh
                    Update(game, true);
                    UpdateRanking();
                }

                if (registerAction)
                {
                    RegisterEvent(game, em);
                }


                if (game.match == liveMatch)
                {
                    RegisterAction(liveMatch.actions.Last(), em.club);
                    RefreshComposition();
                }
            }

            //Refresh
            if (game.match == liveMatch)
            {
                ucScoreboard.Update(game.match, true);
                ucStats.Update(game.match);
            }

            await Task.Delay(SliderSpeed());
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

    }
}
