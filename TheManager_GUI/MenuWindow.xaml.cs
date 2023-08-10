using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Menu.xaml
    /// </summary>
    public partial class Windows_Menu : Window
    {

        private View _viewRanking;

        private readonly Game _partie;

        private DateTime _resultsCurrentDate;
        private DateTime _firstDateOfRound;
        private DateTime _lastDateOfRound;


        public Windows_Menu()
        {
            InitializeComponent();

            _resultsCurrentDate = new DateTime();

            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\close.png"));
            imgBtnGauche.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\left.png"));
            imgBtnDroite.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\right.png"));
            imgBtnSave.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\save.png"));
            imgBtnSearch.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\search.png"));
            imgBtnOptions.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\options.png"));
            imgBtnRanking.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\ranking.png"));
            imgBtnGlobal.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\ranking.png"));

            _partie = Session.Instance.Game;

            imgClub.Source = new BitmapImage(new Uri(Utils.Logo(_partie.club)));
            comboContinent.Items.Clear();
            foreach (Continent c in _partie.kernel.world.GetAllContinents())
            {
                comboContinent.Items.Add(c);
            }
            Refresh();
            this.KeyDown += new KeyEventHandler(KeyPress);
        }

        private void RefreshTransferListPanel()
        {
            spTransferList.Children.Clear();
            foreach(Club c in Session.Instance.Game.club.Championship.rounds[0].clubs)
            {
                CityClub cc = c as CityClub;
                if(cc != null)
                {
                    foreach(ContractOffer co in cc.clubTransfersManagement.offersHistory)
                    {
                        StackPanel spT = new StackPanel();
                        spT.Orientation = Orientation.Horizontal;
                        Label lbPlayer = ViewUtils.CreateLabel(co.Player.lastName, "StyleLabel2", 8, 40);
                        lbPlayer.MouseLeftButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                        { Windows_Joueur wj = new Windows_Joueur(co.Player); wj.Show(); };
                        spT.Children.Add(lbPlayer);
                        string from = "Libre";
                        if (co.Origin != null)
                        {
                            from = co.Origin.shortName;
                        }
                        spT.Children.Add(ViewUtils.CreateLabel(from, "StyleLabel2", 8, 55));
                        Label labelClub = ViewUtils.CreateLabel(cc.shortName, "StyleLabel2", 8, 55);
                        labelClub.MouseLeftButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                        { Windows_Club wc = new Windows_Club(cc); wc.Show(); };
                        spT.Children.Add(labelClub);
                        spT.Children.Add(ViewUtils.CreateLabel(co.TransferIndemnity + "€", "StyleLabel2", 8, 60));


                        spT.Children.Add(ViewUtils.CreateLabel(co.Result.ToString(), "StyleLabel2", 8, 60));
                        spTransferList.Children.Add(spT);
                    }
                }
            }
        }

        private void FillNews()
        {
            spNews.Children.Clear();
            //Read article in inverted sense, the last news will appear on top
            for(int i = Session.Instance.Game.articles.Count-1; i>=0; i--)
            {
                Article a = Session.Instance.Game.articles[i];
                spNews.Children.Add(ViewUtils.CreateNewsItem(a));
            }
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Avancer()
        {
            List<Match> matchs = _partie.NextDay();
            if (matchs.Count > 0)
            {
                Windows_AvantMatch wam = new Windows_AvantMatch(matchs, _partie.club);
                wam.ShowDialog();
            }
            _partie.UpdateTournaments();
            Refresh();
        }

        private void BtnAvancer_Click(object sender, RoutedEventArgs e)
        {
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("France"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Martinique"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Guadeloupe"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Réunion"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Guyane"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Mayotte"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Tahiti"));
            Utils.CheckDuplicates(Session.Instance.Game.kernel.String2Country("Nouvelle-Calédonie"));
            Avancer();
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchPlayersWindow spw = new SearchPlayersWindow();
            spw.Show();
        }

        private void LbChampionnats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tournament c = lbChampionnats.SelectedItem as Tournament;
            if(c != null)
            {
                comboRounds.Items.Clear();
                foreach (Round t in c.rounds)
                {
                    comboRounds.Items.Add(t);
                }
                if(comboRounds.Items.Count > 0)
                {
                    comboRounds.SelectedItem = comboRounds.Items[0];
                }
            }
        }

        private void ComboRounds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Round t = comboRounds.SelectedItem as Round;

            if (t != null)
            {
                View vue = FactoryViewRanking.CreateView(t, 0.7);
                if (vue != null)
                {
                    vue.Full(spRoundRanking);
                    //vue.Afficher();
                }
                List<Match> matches = new List<Match>(t.matches);
                if (matches.Count > 0)
                {
                    matches.Sort(new MatchDateComparator());
                    _resultsCurrentDate = t.NextMatchesDate();
                    _firstDateOfRound = matches[0].day;
                    _lastDateOfRound = matches[matches.Count - 1].day;
                    Calendrier(t);
                }
            }
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            Windows_Options wo = new Windows_Options();
            wo.Show();
        }

        private void CheckPlayoffTrees()
        {
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Console.WriteLine("Final Phases Clubs");
            foreach(Club c in fr.Leagues()[0].GetFinalPhasesClubs())
            {
                Console.WriteLine(". " + c.name);
            }
            List<Tournament> leagues = fr.Leagues();
            foreach (Tournament league in leagues)
            {
                Console.WriteLine("PLAYOFFS " + league.name);
                Round topPlayOffRound = league.GetFinalTopPlayOffRound();
                if(topPlayOffRound != null)
                {
                    Console.WriteLine("topPlayOffRound " + topPlayOffRound.Tournament.name + ", " + topPlayOffRound.name);
                    if(topPlayOffRound != league.rounds[0])
                    {
                        List<Round> rounds = new List<Round>();
                        rounds = league.GetPlayOffsTree(topPlayOffRound.Tournament, topPlayOffRound, new List<Round>());
                        foreach (Round r in rounds)
                        {
                            Console.WriteLine("- " + r.Tournament.name + ", " + r.name);
                        }
                    }
                }
                Console.WriteLine("========================");
            }
            int i = 0;
            ClubComparator comparator = new ClubComparator(ClubAttribute.CURRENT_RANKING, false);
            foreach (List<Club> clubs in fr.GetAdministrativeRetrogradations())
            {
                Console.WriteLine("============" + leagues[i].name + "==========");
                foreach (Club c in clubs)
                {
                    Round clubC = (from Tournament t in leagues where t.rounds.Count > 0 && t.rounds[0].clubs.Contains(c) select t.rounds[0]).FirstOrDefault();
                    string adm = (leagues[i].rounds[0] as GroupsRound != null && (leagues[i].rounds[0] as GroupsRound).administrativeLevel > 0) ? "[" + fr.GetAdministrativeDivisionLevel(c.AdministrativeDivision(), (leagues[i].rounds[0] as GroupsRound).administrativeLevel).name + "] " : "";
                    Console.WriteLine(adm + c.Championship.name + " - " + comparator.GetRanking(clubC, c) + ". " + c.name);
                }
                i++;
            }

        }

        private void RemovingPoints()
        {
            Country fr = Session.Instance.Game.kernel.String2Country("France");
            Round r = fr.League(4).rounds[0];
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("Stade de Reims B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("Olympique de Marseille B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("FC Nantes B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("FC Metz B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("AJ Auxerre B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
            r.AddPointsDeduction(Session.Instance.Game.kernel.String2Club("Paris Saint-Germain B"), SanctionType.Forfeit, Session.Instance.Game.date, 30);
        }

        private void SetAdministrativeRetrogradationsFr()
        {
            Country fr = Session.Instance.Game.kernel.String2Country("France");

            foreach (Tournament t in fr.Leagues())
            {
                foreach (Round r in t.rounds)
                {
                    if ((r as ChampionshipRound) != null || (r as GroupsRound) != null)
                    {
                        r.rules.Add(Rule.BottomTeamNotEligibleForRepechage);
                    }
                }
            }

            Dictionary<string, int> retrogradations = new Dictionary<string, int>()
            {
                ["Dijon FCO"] = 5,
                ["AJ Auxerre"] = 4,
                ["SC Bastia"] = 7,
                ["Gfc Ajaccio"] = 5,
                ["Amiens SC"] = 5,
                ["CS Sedan Ardennes"] = 5,
                ["SO Cholet"] = 5,
                ["US Boulogne CO"] = 5,
                ["FC Chambly"] = 5,
                ["Stade Lavallois"] = 5,
                ["Jura Lacs F"] = 7,
//                ["RC Lons LE Saunier"] = 7,
//                ["US ST Vit"] = 7,
//                ["AS Levier"] = 7,
//                ["Football Club DE Vesoul"] = 7,
                ["CA DE Pontarlier"] = 7,
                ["AS ST Apollinaire"] = 7,
                ["Paron F C"] = 7,
                ["FC Grandvillars"] = 7
            };


            foreach (Club c in Session.Instance.Game.kernel.Clubs)
            {
                if (retrogradations.ContainsKey(c.name))
                {
                    Console.WriteLine("Relegue " + c.name);
                    fr.AddAdministrativeRetrogradation(c, fr.League(retrogradations[c.name]));
                }
            }
        }

        /*
        private void SetAdministrativeRetrogradationsAz()
        {
            Country az = Session.Instance.Game.kernel.String2Country("Azerbaïdjan");

            foreach (Tournament t in az.Leagues())
            {
                foreach (Round r in t.rounds)
                {
                    if ((r as ChampionshipRound) != null || (r as GroupsRound) != null)
                    {
                        r.rules.Add(Rule.BottomTeamNotEligibleForRepechage);
                    }
                }
            }

            Dictionary<string, int> retrogradations = new Dictionary<string, int>()
            {
                ["Club Nord N1 2"] = 7,
                ["Club Nord N1 3"] = 7,
                ["Club Sud9"] = 5,
                ["Club Sud8"] = 5,
                ["Club Nord N1 8"] = 8,
                ["Club Nord N2 6"] = 8,
                ["Club Nord N1 14"] = 8,
                ["Club Nord N1 9"] = 8,
                ["Club Nord N2 6"] = 8,
                ["Club Ouest O1 7"] = 8,
                ["Club Est E2 6"] = 6
            };


            foreach (Club c in Session.Instance.Game.kernel.Clubs)
            {
                if (retrogradations.ContainsKey(c.name))
                {
                    Console.WriteLine("Relegue " + c.name);
                    az.AddAdministrativeRetrogradation(c, az.League(retrogradations[c.name]));
                }
            }
        }*/

        private void KeyPress(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.P)
            {
                RemovingPoints();
            }
            if(e.Key == Key.O)
            {
                CheckPlayoffTrees();
            }
            if(e.Key == Key.R)
            {
                Console.WriteLine("Relegations");
                SetAdministrativeRetrogradationsFr();
            }
        }

        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Country fr = Session.Instance.Game.kernel.String2Country("France");

            Avancer();
            while (!(_partie.date.Month == 5 && _partie.date.Day == 21))
            {
                Avancer();
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Utils.Debug("Total execution " + elapsedMs + "ms");
            /*Console.WriteLine(fr.Name());
            foreach(KeyValuePair<Club, Tournament> ra in fr.administrativeRetrogradations)
            {
                Console.WriteLine(ra.Key.name + " -> " + ra.Value.name);
            }*/

            /*
            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                if(c.Country() == fr)
                {
                    Console.WriteLine("PRINT GAMES FOR " + c.name);
                    List<Match> games = c.Games;
                    games.Sort(new MatchDateComparator());
                    DateTime currentDate = new DateTime(2000, 1, 1);
                    foreach (Match m in games)
                    {
                        int daysDiff = Utils.DaysNumberBetweenTwoDates(currentDate, m.day);
                        string alert = "";
                        switch (daysDiff)
                        {
                            case 0:
                                alert = "||| ";
                                break;
                            case 1:
                                alert = "|| ";
                                break;
                            case 2:
                                alert = "| ";
                                break;
                            default:
                                alert = "";
                                break;
                        }
                        Console.WriteLine(alert + m.day.ToShortDateString() + " [" + m.Tournament + "]" + m.home.name + " - " + m.away.name);
                        currentDate = m.day;
                    }
                }
            }*/

        }

        private void BtnSimuler2_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i<10;i++)
            {
                Avancer();
                while (!(_partie.date.Month == 6 && _partie.date.Day == 13))
                {
                    Avancer();
                }
                Refresh();
            }
            Refresh();

        }

        private void ListerCompetitions(ILocalisation localisation)
        {
            if(localisation != null)
            {
                lbChampionnats.Items.Clear();
                comboRounds.Items.Clear();
                foreach (Tournament c in localisation.Tournaments())
                {
                    this.lbChampionnats.Items.Add(c);
                }
            }
        }

        public void FillGamesOfDayPanel()
        {
            List<Match> matchs = Session.Instance.Game.kernel.MatchsOfDate(Session.Instance.Game.date);
            matchs.Sort(new MatchDateComparator());
            ViewMatches view = new ViewMatches(matchs, false, true, false, false, false, true, 11, false, null, false, false, false, true);
            view.Full(spFullGames);
        }

        private void Refresh()
        {
            if(cbOpti.IsChecked == false)
            {
                NextGamesOfClub();
                ClassementClub();
                BandeauActualites();
                FillNews();
                FillNextMatchPanel();
                FillCalendar();
                RefreshTransferListPanel();
                FillGamesOfDayPanel();
            }
        }

        private void BandeauActualites()
        {
            tbActu.Text = "";
            if (_partie.club != null && _partie.club.Championship != null)
            {
                //Choix de la compétition à afficher dans le bandeau : compétition du dernier match joué par l'équipe
                List<Match> matchs = _partie.club.Games;
                bool trouve = false;
                Tournament comp = null;
                int i = 0;
                if (matchs.Count == 0)
                {
                    trouve = true;
                }
                while(!trouve)
                {
                    if(!matchs[i].Played)
                    {
                        comp = matchs[i].Tournament;
                        trouve = true;
                    }
                    i++;
                    if (i + 1 == matchs.Count)
                    {
                        trouve = true;
                    }
                }

                if(comp != null)
                {
                    Round t = null;
                    if (comp.isChampionship)
                    {
                        t = comp.rounds[0];
                        tbActu.Text = comp.name + " : ";
                        i = 1;
                        ChampionshipRound tc = t as ChampionshipRound;
                        if (tc != null)
                        {
                            foreach (Club c in tc.Ranking())
                            {
                                tbActu.Text += i + " " + c.shortName + " " + t.Points(c) + ", " + t.Difference(c) + " / ";
                                i++;
                            }
                        }
                    }
                    else
                    {
                        t = comp.rounds[comp.currentRound];
                        tbActu.Text += comp.name + ", " + t.name + " : ";
                        foreach(Match m in t.matches)
                        {
                            tbActu.Text += m.home.shortName + " " + m.score1 + "-" + m.score2 + " " + m.away.shortName + ", ";
                        }
                    }
                }
            }
        }

        private void ClassementClub()
        {
            if (_partie.club != null && _partie.club.Championship != null)
            {
                _viewRanking = FactoryViewRanking.CreateView(_partie.club.Championship.rounds[0], 0.75, true, _partie.club, true);
                _viewRanking.Full(spRanking);
            }
        }

        private void NextGamesOfClub()
        {


            if(_partie.club != null)
            {
                List<Match> matches = new List<Match>();
                List<Match> clubMatches = new List<Match>();
                foreach (Match m in _partie.kernel.Matchs)
                {
                    if (m.home == _partie.club || m.away == _partie.club)
                    {
                        clubMatches.Add(m);
                    }
                }
                clubMatches.Sort(new MatchDateComparator());
                int diff = -1;
                int indice = -1;
                foreach (Match m in clubMatches)
                {
                    TimeSpan ts = m.day - _partie.date;

                    int diffM = Math.Abs(ts.Days);
                    if (diffM < diff || diff == -1)
                    {
                        diff = diffM;
                        indice = clubMatches.IndexOf(m);
                    }
                }
                indice = indice - 2;
                if (indice < 0)
                {
                    indice = 0;
                }

                if (indice > clubMatches.Count - 3)
                {
                    indice = clubMatches.Count - 3;
                }
                for(int i = indice; i<indice+5; i++)
                {
                    //Case if there is less than 5 games for the club
                    if(i < clubMatches.Count && i>=0)
                    {
                        matches.Add(clubMatches[i]);
                        
                        /*Match m = matchs[i];
                        string score = m.score1 + " - " + m.score2;
                        if (!m.Played)
                        {
                            score = m.day.ToShortDateString();
                        }
                        dgClubProchainsMatchs.Items.Add(new ProchainMatchElement { Match = m, Competition = m.Tournament, ShortName = m.Tournament.shortName, Equipe1 = m.home.shortName, Equipe2 = m.away.shortName, Score = score, LogoD = Utils.Logo(m.home), LogoE = Utils.Logo(m.away) });*/
                    }
                }
                
                
                ViewMatches viewMatches = new ViewMatches(matches, true, false, false, false, false, true);
                viewMatches.Full(spNextMatches);
            }
        }

        private void FillCalendar()
        {
            spCalendar.Children.Clear();
            for(int i = -3; i<4; i++)
            {

                //TODO: Not efficient way (function in club if has a game this day ?)
                Match match = null;
                foreach(Match m in _partie.club.Games)
                {
                    if (Utils.CompareDates(m.day, _partie.date.AddDays(i)))
                    {
                        match = m;
                    }
                }
                spCalendar.Children.Add(ViewUtils.CreateCalendarItem(_partie.date.AddDays(i), i==0, match));
            }
        }

        private void FillNextMatchPanel()
        {
            spNextMatch.Children.Clear();

            Match next = Session.Instance.Game.club.NextGame;
            if(next != null)
            {

                //Create the 3 stacks panels
                StackPanel spHomeTeam = new StackPanel();
                spHomeTeam.Orientation = Orientation.Vertical;
                spHomeTeam.HorizontalAlignment = HorizontalAlignment.Center;
                StackPanel spAwayTeam = new StackPanel();
                spAwayTeam.Orientation = Orientation.Vertical;
                spAwayTeam.HorizontalAlignment = HorizontalAlignment.Center;
                StackPanel spInfos = new StackPanel();
                spInfos.Orientation = Orientation.Vertical;
                spInfos.HorizontalAlignment = HorizontalAlignment.Center;
                spInfos.Width = 150;

                //Infos stack panel
                Round r = next.Round;
                Tournament trn = r.Tournament;

                TextBox tbTournament = new TextBox();
                tbTournament.Text = next.Round.Tournament.name;
                tbTournament.Style = FindResource("StyleTextBox") as Style;
                tbTournament.IsEnabled = false;
                tbTournament.FontSize = 16;
                tbTournament.HorizontalAlignment = HorizontalAlignment.Center;
                tbTournament.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(trn.color.ToHexa()));
                tbTournament.Foreground = Brushes.AntiqueWhite;

                spInfos.Children.Add(tbTournament);
                spInfos.Children.Add(ViewUtils.CreateLabel(r.name, "StyleLabel2Center", -1, -1));
                spInfos.Children.Add(ViewUtils.CreateLabel(next.day.ToShortTimeString(), "StyleLabel2Center", -1, -1));
                spInfos.Children.Add(ViewUtils.CreateLabel(next.home.stadium.name, "StyleLabel2Center", -1, -1));

                //Home team stack panel
                spHomeTeam.Children.Add(ViewUtils.CreateLogo(next.home, 125, 125));
                Label homeLabel = ViewUtils.CreateLabel(next.home.shortName, "StyleLabel2Center", 16, -1);
                homeLabel.HorizontalAlignment = HorizontalAlignment.Center;
                spHomeTeam.Children.Add(homeLabel);
                StackPanel homeStars = ViewUtils.CreateStarNotation(next.home.Stars, 25);
                homeStars.HorizontalAlignment = HorizontalAlignment.Center;
                spHomeTeam.Children.Add(homeStars);

                //Away team stack panel
                spAwayTeam.Children.Add(ViewUtils.CreateLogo(next.away, 125, 125));
                Label awayLabel = ViewUtils.CreateLabel(next.away.shortName, "StyleLabel2Center", 16, -1);
                awayLabel.HorizontalAlignment = HorizontalAlignment.Center;
                spAwayTeam.Children.Add(awayLabel);
                StackPanel awayStars = ViewUtils.CreateStarNotation(next.away.Stars, 25);
                awayStars.HorizontalAlignment = HorizontalAlignment.Center;
                spAwayTeam.Children.Add(awayStars);

                spNextMatch.Children.Add(spHomeTeam);
                spNextMatch.Children.Add(spInfos);
                spNextMatch.Children.Add(spAwayTeam);
            }
        }

        private void Calendrier(Round t)
        {

            List<Match> matchs = t.GetMatchesByDate(_resultsCurrentDate);
            matchs.Sort(new MatchDateComparator());
            lbRoundDate.Content = _resultsCurrentDate.ToLongDateString();
            ViewMatches view = new ViewMatches(matchs, false, true, true, true, true, false);
            view.Full(spRoundGames);
        }

        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "The Manager Save|*.csave";
            if (saveFileDialog.ShowDialog() == true)
            {
                Session.Instance.Game.Save(saveFileDialog.FileName);
            }
        }

        private void BtnParticipants_Click(object sender, RoutedEventArgs e)
        {
            Tournament c = lbChampionnats.SelectedItem as Tournament;
            if (c != null)
            {
                Windows_Participants participants = new Windows_Participants(c);
                participants.Show();
            }
        }

        private void BtnDroite_Click(object sender, RoutedEventArgs e)
        {
            Round r = comboRounds.SelectedItem as Round;
            if(r != null)
            {
                if(!Utils.CompareDates(_resultsCurrentDate, _lastDateOfRound)){
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
                Calendrier(r);
            }
        }

        private void BtnGauche_Click(object sender, RoutedEventArgs e)
        {
            Round r = comboRounds.SelectedItem as Round;
            if (r != null)
            {
                if (!Utils.CompareDates(_resultsCurrentDate, _firstDateOfRound)){
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
                Calendrier(r);
            }
        }

        private void BtnRanking_Click(object sender, RoutedEventArgs e)
        {
            RankingWindow rw = new RankingWindow();
            rw.Show();
        }

        private void BtnCompetition_Click(object sender, RoutedEventArgs e)
        {
            Tournament c = lbChampionnats.SelectedItem as Tournament;
            if(c != null)
            {
                Windows_Competition wc = new Windows_Competition(c);
                wc.Left = 50;
                wc.Top = 25;
                wc.Show();
            }
        }

        private void btnCalendar_Click(object sender, RoutedEventArgs e)
        {
            CalendarWindow cw = new CalendarWindow();
            cw.Show();
        }

        private void lbCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ILocalisation localisation = lbCountries.SelectedItem as ILocalisation;
            ListerCompetitions(localisation);
        }

        private void comboContinent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Continent c = comboContinent.SelectedItem as Continent;

            lbCountries.Items.Clear();
            if(c.Tournaments().Count > 0)
            {
                lbCountries.Items.Add(c);
            }
            foreach (Country cy in c.countries)
            {
                if(cy.Tournaments().Count > 0)
                {
                    lbCountries.Items.Add(cy);
                }
            }
            
        }

        private void btnGlobal_Click(object sender, RoutedEventArgs e)
        {
            GlobalWindow gw = new GlobalWindow();
            gw.Show();
        }
    }

    public struct PalmaresElement : IEquatable<PalmaresElement>
    {
        public int Annee { get; set; }
        public Club Club { get; set; }
        public bool Equals(PalmaresElement other)
        {
            throw new NotImplementedException();
        }
    }

    

    public struct ButeurElement : IEquatable<ButeurElement>
    {
        public Player Buteur { get; set; }
        public int NbButs { get; set; }
        public string Club { get; set; }
        public bool Equals(ButeurElement other)
        {
            throw new NotImplementedException();
        }
    }

    public struct CalendrierElement : IEquatable<CalendrierElement>
    {
        public Match Match { get; set; }
        public string Heure { get; set; }
        public string Equipe1 { get; set; }
        public string Equipe2 { get; set; }
        public string Score { get; set; }
        public string Affluence { get; set; }
        public string Cote1 { get; set; }
        public string CoteN { get; set; }
        public string Cote2 { get; set; }
        public bool Equals(CalendrierElement other)
        {
            throw new NotImplementedException();
        }
    }

    public struct ClassementElement : IEquatable<ClassementElement>
    {
        public string Logo { get; set; }
        public int Classement { get; set; }
        public string Nom { get; set; }
        public int Pts { get; set; }
        public int J { get; set; }
        public int G { get; set; }
        public int N { get; set; }
        public int P { get; set; }
        public int bp { get; set; }
        public int bc { get; set; }
        public int Diff { get; set; }
        public Club Club { get; set; }
        public bool Equals(ClassementElement other)
        {
            throw new NotImplementedException();
        }
    }


}