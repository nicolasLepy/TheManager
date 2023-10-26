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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.Styles;
using TheManager_GUI.views;
using TheManager_GUI.Views;

namespace TheManager_GUI.pages
{
    /// <summary>
    /// Logique d'interaction pour TournamentResultsPage.xaml
    /// </summary>
    public partial class TournamentResultsPage : Page
    {

        private readonly Tournament tournament;
        private ComboBoxDayController controler;
        private Round activeRound;

        public TournamentResultsPage(Tournament tournament)
        {
            this.tournament = tournament;
            this.controler = new ComboBoxDayController();
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            RefreshGameDayComboBox(tournament);
        }

        private void RefreshGameDayComboBox(Tournament tournament)
        {
            comboBoxGameDay.Items.Clear();
            controler.Initialize(false);
            controler.Populate(tournament);
            controler.tournament = tournament;
            foreach (KeyValuePair<Round, int> registeredRound in controler.GetRoundsRegistered())
            {
                string name = registeredRound.Key.IsKnockOutRound() ? (registeredRound.Key.MatchesDayNumber() == 1 ? "" : (registeredRound.Value == 1 ? FindResource("str_first_leg").ToString() : FindResource("str_second_leg").ToString())) : String.Format(FindResource("str_matchweek").ToString(), (registeredRound.Value), (registeredRound.Value) == 1 ? FindResource("str_matchweek_numeral_first").ToString() : FindResource("str_matchweek_numeral_more").ToString());
                bool isChampionship = registeredRound.Key as ChampionshipRound != null || (tournament.rounds.Count > 0 && tournament.rounds[0] == registeredRound.Key && tournament.isChampionship);
                if (!isChampionship)
                {
                    name = String.Format("{0}{1}{2}", registeredRound.Key.name, name.Length > 0 ? " - " : "", name);
                }
                comboBoxGameDay.Items.Add(name);
            }
            if (comboBoxGameDay.Items.Count > 0)
            {
                DateTime today = Session.Instance.Game.date;
                DateTime closestTime = DateTime.MinValue;
                int comboBoxIndex = 0;
                foreach (Round round in tournament.rounds)
                {
                    DateTime nextRoundDate = round.NextMatchesDate();
                    if ((today - nextRoundDate).Days < (today - closestTime).Days)
                    {
                        closestTime = nextRoundDate;
                        comboBoxIndex = controler.GetRegisteredRoundIndex(round, round.NextMatchesGameDay());
                    }
                }
                comboBoxGameDay.SelectedIndex = comboBoxIndex;
            }
        }

        public void RefreshRanking(Round round, RankingType rankingType)
        {
            FactoryViewRanking.CreateView(round, 1, false, null, false, rankingType).Full(panelRanking);
        }

        public void RefreshScores(List<Match> matchs)
        {
            List<MatchAttribute> comparaisonAttributes = new List<MatchAttribute>() { MatchAttribute.DATE };
            matchs.Sort(new MatchComparator(comparaisonAttributes));
            ViewScores view = new ViewScores(matchs, true, true, true, false, true, false, (float)(double)Application.Current.FindResource(StyleDefinition.fontSizeSecondary), false, null, false, false, false, false);
            view.Full(panelResults);

        }

        private void buttonAwayRanking_Click(object sender, RoutedEventArgs e)
        {
            if(activeRound != null)
            {
                RefreshRanking(activeRound, RankingType.Away);
            }
        }

        private void buttonHomeRanking_Click(object sender, RoutedEventArgs e)
        {
            if(activeRound != null)
            {
                RefreshRanking(activeRound, RankingType.Home);
            }
        }

        private void buttonGeneralRanking_Click(object sender, RoutedEventArgs e)
        {
            if(activeRound != null)
            {
                RefreshRanking(activeRound, RankingType.General);
            }
        }

        private void buttonScoresLeft_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxGameDay.SelectedIndex > 0)
            {
                comboBoxGameDay.SelectedIndex--;
            }
        }

        private void buttonScoreRight_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxGameDay.SelectedIndex < comboBoxGameDay.Items.Count - 1)
            {
                comboBoxGameDay.SelectedIndex++;
            }
        }

        private void comboBoxGameDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxGameDay.SelectedIndex > -1 && comboBoxGameDay.SelectedIndex < controler.GetRoundsRegistered().Count)
            {
                KeyValuePair<Round, int> selectedRound = controler.GetRoundsRegistered()[comboBoxGameDay.SelectedIndex];
                if (activeRound != selectedRound.Key)
                {
                    activeRound = selectedRound.Key;
                    RefreshRanking(selectedRound.Key, RankingType.General);
                }
                RefreshScores(selectedRound.Key.GamesDay(selectedRound.Value));
            }
        }
    }
}
