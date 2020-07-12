using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Menu.xaml
    /// </summary>
    public partial class Windows_Menu : Window
    {

        private IVueClassement vueClassement;

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

            _partie = Session.Instance.Game;

            imgClub.Source = new BitmapImage(new Uri(Utils.Logo(_partie.club)));
            comboPays.Items.Clear();
            foreach (Continent c in _partie.kernel.continents)
            {
                if (c.Tournaments().Count > 0)
                {
                    this.comboPays.Items.Add(c);
                }

                foreach (Country p in c.countries)
                {
                    if (p.Tournaments().Count > 0)
                    {
                        this.comboPays.Items.Add(p); Console.WriteLine(p);
                    }
                }
            }
            Refresh();

            
        }

        private void RefreshTransferListPanel()
        {
            spTransferList.Children.Clear();
            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                CityClub cc = c as CityClub;
                if(cc != null)
                {
                    foreach(ContractOffer co in cc.clubTransfersManagement.offersHistory)
                    {
                        StackPanel spT = new StackPanel();
                        spT.Orientation = Orientation.Horizontal;
                        spT.Children.Add(ViewUtils.CreateLabel(co.Player.lastName, "StyleLabel2", 10, 50));
                        Label labelClub = ViewUtils.CreateLabel(cc.shortName, "StyleLabel2", 10, 70);
                        labelClub.MouseLeftButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                        { Windows_Club wc = new Windows_Club(cc);wc.Show(); };
                        spT.Children.Add(labelClub);



                        spT.Children.Add(ViewUtils.CreateLabel(co.Successful.ToString(), "StyleLabel2", 10, 40));
                        spTransferList.Children.Add(spT);
                    }
                }
            }
        }

        private void RemplirArticles()
        {
            spNews.Children.Clear();
            foreach(Article a in Session.Instance.Game.articles)
            {
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.WrapWithOverflow;
                tb.Text = a.publication.ToShortDateString() + " - " + a.title;
                tb.Style = Application.Current.FindResource("StyleTextBlockLittle") as Style;
                spNews.Children.Add(tb);

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
            Refresh();
        }

        private void BtnAvancer_Click(object sender, RoutedEventArgs e)
        {
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
                lbTours.Items.Clear();
                /*dgClassement.Items.Clear();*/
                foreach (Round t in c.rounds)
                {
                    lbTours.Items.Add(t);
                }

            }
        }

        private void LbTours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Round t = lbTours.SelectedItem as Round;

            if (t != null)
            {
                IVueClassement vue = FabriqueVueClassement.CreerVue(null, t, 0.7);
                if (vue != null)
                {
                    vue.Remplir(spRoundRanking);
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

        private void ComboPays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ILocalisation localisation = comboPays.SelectedItem as ILocalisation;
            ListerCompetitions(localisation);

        }

        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            Avancer();
            while (!(_partie.date.Month == 6 && _partie.date.Day == 13))
            {
                Avancer();
            }
        }

        private void BtnSimuler2_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i<10;i++)
            {
                _partie.NextDay();
                while (!(_partie.date.Month == 6 && _partie.date.Day == 13))
                {
                    _partie.NextDay();
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
                lbTours.Items.Clear();
                foreach (Tournament c in localisation.Tournaments())
                {
                    this.lbChampionnats.Items.Add(c);
                }
            }
        }

        private void Refresh()
        {
            this.labelDate.Content = _partie.date.ToLongDateString();
            if(cbOpti.IsChecked == false)
            {
                NextGamesOfClub();
                ClassementClub();
                BandeauActualites();
                RemplirArticles();
                FillNextMatchPanel();
                RefreshTransferListPanel();
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
                vueClassement = FabriqueVueClassement.CreerVue(null, _partie.club.Championship.rounds[0], 0.75, true, _partie.club);
                vueClassement.Remplir(spRanking);
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
                    //Cas où si jamais il y a moins de 5 matchs pour le club
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
                
                MatchesDataGridView matchesView = new MatchesDataGridView(spNextMatches, matches, false, false, false, false, true);
                matchesView.Refresh();
            }
        }

        private void FillNextMatchPanel()
        {
            spNextMatch.Children.Clear();
            spNextMatchBox.Children.Clear();

            Match next = Session.Instance.Game.club.NextGame;
            if(next != null)
            {
                Round r = next.Round;
                Tournament trn = r.Tournament;

                TextBox tbTournament = new TextBox();
                tbTournament.Text = next.Round.Tournament.name;
                tbTournament.Style = FindResource("StyleTextBox") as Style;
                tbTournament.IsEnabled = false;
                tbTournament.FontSize = 14;
                tbTournament.HorizontalAlignment = HorizontalAlignment.Center;
                tbTournament.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(trn.color.ToHexa()));
                tbTournament.Foreground = Brushes.AntiqueWhite;

                Label lbRound = ViewUtils.CreateLabel(r.name, "StyleLabel2", -1, 100);

                StackPanel spLeftTeam = new StackPanel();
                spLeftTeam.Orientation = Orientation.Vertical;
                spLeftTeam.HorizontalAlignment = HorizontalAlignment.Center;
                Image imgHome = new Image();
                imgHome.Width = 60;
                imgHome.Height = 60;
                imgHome.Source = new BitmapImage(new Uri(Utils.Logo(next.home)));
                spLeftTeam.Children.Add(imgHome);
                Label homeLabel = ViewUtils.CreateLabel(next.home.shortName, "StyleLabel2", 14, 150);
                homeLabel.HorizontalAlignment = HorizontalAlignment.Center;
                spLeftTeam.Children.Add(homeLabel);
                StackPanel homeStars = ViewUtils.CreateStarNotation(next.home.Stars, 20);
                homeStars.HorizontalAlignment = HorizontalAlignment.Center;
                spLeftTeam.Children.Add(homeStars);

                StackPanel spRightTeam = new StackPanel();
                spRightTeam.Orientation = Orientation.Vertical;
                spRightTeam.HorizontalAlignment = HorizontalAlignment.Center;
                Image imgAway = new Image();
                imgAway.Width = 60;
                imgAway.Height = 60;
                imgAway.Source = new BitmapImage(new Uri(Utils.Logo(next.away)));
                spRightTeam.Children.Add(imgAway);
                Label awayLabel = ViewUtils.CreateLabel(next.away.shortName, "StyleLabel2", 14, 150);
                awayLabel.HorizontalAlignment = HorizontalAlignment.Center;
                spRightTeam.Children.Add(awayLabel);
                StackPanel awayStars = ViewUtils.CreateStarNotation(next.away.Stars, 20);
                awayStars.HorizontalAlignment = HorizontalAlignment.Center;
                spRightTeam.Children.Add(awayStars);

                spNextMatchBox.Children.Add(tbTournament);
                spNextMatchBox.Children.Add(lbRound);
                spNextMatch.Children.Add(spLeftTeam);
                spNextMatch.Children.Add(spRightTeam);
            }
        }

        private void Calendrier(Round t)
        {

            List<Match> matchs = t.GetMatchesByDate(_resultsCurrentDate);
            matchs.Sort(new MatchDateComparator());
            lbRoundDate.Content = _resultsCurrentDate.ToLongDateString();
            MatchesDataGridView view = new MatchesDataGridView(spRoundGames, matchs, true, true, true, true, false);
            view.Refresh();
        }

        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "The Manager Save|*.save";
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
            Round r = lbTours.SelectedItem as Round;
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
            Round r = lbTours.SelectedItem as Round;
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

        private void BtnCompetition_Click(object sender, RoutedEventArgs e)
        {
            Tournament c = lbChampionnats.SelectedItem as Tournament;
            if(c != null)
            {
                Windows_Competition wc = new Windows_Competition(c);
                wc.Left = 50;
                wc.Top = 50;
                wc.Show();
            }
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