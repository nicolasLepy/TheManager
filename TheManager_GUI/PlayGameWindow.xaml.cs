using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TheManager;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI
{

  
    /// <summary>
    /// Logique d'interaction pour Windows_JouerMatch.xaml
    /// </summary>
    public partial class Windows_JouerMatch : Window
    {
        private readonly List<Match> _matchs;
        private readonly Round _tour;

        private MediaWAV _media;

        private readonly List<bool> _enCours;


        async Task Match(Match game)
        {
            List<RetourMatch> res = game.NextMinute();
            if (Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, res))
            {
                _enCours[_matchs.IndexOf(game)] = false;
            }
            //Si y a un évenement
            if (Utils.RetoursContient(RetourMatchEvenement.EVENEMENT,res))
            {
                MatchEvent em = game.events[game.events.Count - 1];
                string icone = "";
                bool afficherAction = false;
                
                if(em.type == GameEvent.Goal || em.type == GameEvent.PenaltyGoal || em.type == GameEvent.AgGoal)
                {
                    icone = "goal.png";
                    afficherAction = true;
                    if (em.club == game.home)
                    {
                        _media.But(game);
                    }

                    if (cbJingleBut.IsChecked == true)
                    {
                        _media.AjouterSon("jingle", false);
                    }

                    //Refresh en cas de but
                    Matchs();
                    Classement();
                }
                else if(em.type == GameEvent.YellowCard)
                {
                    icone = "yellow_card.png";
                    afficherAction = true;
                }
                else if(em.type == GameEvent.RedCard)
                {
                    icone = "red_card.png";
                    afficherAction = true;
                }

                if (afficherAction)
                {
                    StackPanel spAction = new StackPanel();
                    spAction.Orientation = Orientation.Horizontal;

                    spAction.Children.Add(ViewUtils.CreateImage(Utils.Image(icone), 20, 20));
                    spAction.Children.Add(ViewUtils.CreateLabel(em.MinuteToString, "StyleLabel2", 11, 25, System.Windows.Media.Brushes.LightSalmon, null, true));
                    spAction.Children.Add(ViewUtils.CreateLabel(game.home.shortName + " - " + game.away.shortName + " : " + game.score1 + " - " + game.score2, "StyleLabel2", 11, 210, icone == "red_card.png" ? System.Windows.Media.Brushes.LightSalmon : null, null, icone == "goal.png" || icone == "red_card.png"));
                    spAction.Children.Add(ViewUtils.CreateLabel(em.player.lastName + " (" + em.player.Club.shortName + ")", "StyleLabel2", 11, 125, null, null, icone == "goal.png"));
                    spOtherActions.Children.Insert(0,spAction);
                }

                if (game == _matchs[0])
                {
                    ActionsMatch();
                }
            }


            //Refresh
            if (game == _matchs[0])
            {
                lbTemps.Content = game.Time;
                lbScore1.Content = game.score1;
                lbScore2.Content = game.score2;
                lbTirs1.Content = game.statistics.HomeShoots;
                lbTirs2.Content = game.statistics.AwayShoots;
                pbTirs.Maximum = game.statistics.HomeShoots + game.statistics.AwayShoots;
                pbTirs.Value = game.statistics.HomeShoots;
            }
            //Thread t = new Thread(new ThreadStart(ThreadClassement));
            //t.Start();


            await Task.Delay((int)sliderVitesseSimulation.Value)/*.ConfigureAwait(false)*/;
        }

        public void ThreadMatch(Match match)
        {
            
            this.Dispatcher.Invoke(async () =>
            {
                while(_enCours[_matchs.IndexOf(match)] == true)
                {
                    await Match(match)/*.ConfigureAwait(false)*/;
                }

                if (match == _matchs[0])
                {
                    btnTerminer.Visibility = Visibility.Visible;
                }
            });
            
        }

        public void ThreadClassement()
        {
           this.Dispatcher.Invoke(() =>
           {
               Classement();
           });
        }

        public Windows_JouerMatch(List<Match> matchs)
        {
            InitializeComponent();

            cbJingleBut.IsChecked = true;

            _media = new MediaWAV();
            _enCours = new List<bool>();
            _matchs = matchs;
            _tour = _matchs[0].Round;
            Matchs();

            foreach(Match m in _matchs)
            {
                _enCours.Add(true);
            }

            _media.Ambiance4000();

            try
            {
                imgEquipe1.Source = new BitmapImage(new Uri(Utils.Logo(_matchs[0].home)));
                imgEquipe2.Source = new BitmapImage(new Uri(Utils.Logo(_matchs[0].away)));
            }
            catch
            {
                //So we don't show any logo if club has no logo
            }
            lbEquipe1.Content = _matchs[0].home;
            lbEquipe2.Content = _matchs[0].away;


            //Thread t = new Thread(new ThreadStart(ThreadMatch));

            for (int i = 0; i<_matchs.Count; i++)
            {
                int j = i;
                Thread t = new Thread(() => ThreadMatch(_matchs[j]));
                t.Start();
            }
        }

        private void ActionsMatch()
        {

            List<KeyValuePair<string, string>> reversedActions = new List<KeyValuePair<string, string>>(_matchs[0].actions);
            reversedActions.Reverse();
            spActions.Children.Clear();
            foreach (KeyValuePair<string, string> kvp in reversedActions)
            {
                StackPanel spAction = new StackPanel();
                spAction.Orientation = Orientation.Horizontal;
                spAction.Children.Add(ViewUtils.CreateLabel(kvp.Key, "StyleLabel2", 11, 30, System.Windows.Media.Brushes.Salmon, null, true));
                spAction.Children.Add(ViewUtils.CreateLabel(kvp.Value, "StyleLabel2", 11, -1));
                spActions.Children.Add(spAction);
            }
        }

        private void Matchs()
        {
            ViewMatches view = new ViewMatches(_matchs, false, false, false, false, false, false);
            view.Full(spGames);
        }

        private void Classement()
        {
            if(_tour != null)
            {
                View view = FactoryViewRanking.CreateView(_tour, 0.65, false, null, false) ;
                view.Full(spRanking);
            }
        }

        private void BtnTerminer_Click(object sender, RoutedEventArgs e)
        {
            _media.Detruire();
            Close();
        }

        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i<_matchs.Count; i++)
            {
                Match match = _matchs[i];
                bool termine = false;
                while(!termine)
                {
                    List<RetourMatch> rm = match.NextMinute();
                    if (Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, rm))
                    {
                        termine = true;
                    }
                }
                //_matchs[i].Jouer();
            }
            //while (!Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, _matchs[0].MinuteSuivante())) ;
            _media.Detruire();
            _media = null;
            Close();
        }
    }

    public struct MatchLiveElement : IEquatable<MatchLiveElement>
    {
        public Club Equipe1 { get; set; }
        public string Score { get; set; }
        public Club Equipe2 { get; set; }
        public bool Equals(MatchLiveElement other)
        {
            throw new NotImplementedException();
        }
    }

    public struct MatchLiveEvenementElement : IEquatable<MatchLiveEvenementElement>
    {
        public string Logo { get; set; }
        public string Minute { get; set; }
        public string Evenement { get; set; }
        public string Joueur { get; set; }
        public bool Equals(MatchLiveEvenementElement other)
        {
            throw new NotImplementedException();
        }
    }

    public struct MatchLiveAction : IEquatable<MatchLiveAction>
    {
        public string Minute { get; set; }
        public string Action { get; set; }
        public bool Equals(MatchLiveAction other)
        {
            throw new NotImplementedException();
        }
    }
}
