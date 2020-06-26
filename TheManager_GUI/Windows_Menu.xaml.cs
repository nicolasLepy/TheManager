using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private Game _partie = null;

        private DateTime _calendrierJour;

        public Windows_Menu()
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\close.png"));
            imgBtnGauche.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\left.png"));
            imgBtnDroite.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\right.png"));

            _partie = Session.Instance.Game;

            imgClub.Source = new BitmapImage(new Uri(Utils.Logo(_partie.club)));
            comboPays.Items.Clear();
            foreach (Continent c in _partie.kernel.continents)
            {
                if (c.Tournaments().Count > 0) this.comboPays.Items.Add(c);
                foreach (Country p in c.countries) if (p.Tournaments().Count > 0) { this.comboPays.Items.Add(p); Console.WriteLine(p); }
            }
            Refresh();

            
        }

        private void RemplirArticles()
        {
            spNews.Children.Clear();
            foreach(Article a in Session.Instance.Game.articles)
            {
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.WrapWithOverflow;
                tb.Text = a.publication.ToShortDateString() + " - " + a.title;
                tb.Style = Application.Current.FindResource("StyleTextBlock") as Style;
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

        private void LbChampionnats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tournament c = lbChampionnats.SelectedItem as Tournament;
            if(c != null)
            {
                lbTours.Items.Clear();
                dgClassement.Items.Clear();
                dgMatchs.Items.Clear();
                foreach (Round t in c.rounds)
                {
                    lbTours.Items.Add(t);
                }

            }
        }

        private void LbTours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Round t = lbTours.SelectedItem as Round;

            IVueClassement vue = FabriqueVueClassement.CreerVue(dgClassement, t);
            if (vue != null) vue.Afficher();
            /*
            if (t as TourChampionnat != null)
                Classement(t as TourChampionnat);
            else if (t as TourPoules != null)
                Classement(t as TourPoules);
            else
                dgClassement.Items.Clear();*/
            if (t != null)
            {
                Calendrier(t);
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgMatchs.SelectedItem != null)
            {
                CalendrierElement selected = (CalendrierElement)dgMatchs.SelectedItem;
                if(selected.Match != null)
                {
                    Windows_Match match = new Windows_Match(selected.Match);
                    match.Show();
                }
            }
        }

        private void DgClassement_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgClassement.SelectedItem != null)
            {
                ClassementElement selected = (ClassementElement)dgClassement.SelectedItem;
                if (selected.Club as CityClub != null)
                {
                    Windows_Club club = new Windows_Club(selected.Club as CityClub);
                    club.Show();
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
                ProchainsMatchsClub();
                ClassementClub();
                BandeauActualites();
                RemplirArticles();
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
                if (matchs.Count == 0) trouve = true;
                while(!trouve)
                {
                    if(!matchs[i].Played)
                    {
                        comp = matchs[i].Tournament;
                        trouve = true;
                    }
                    i++;
                    if (i + 1 == matchs.Count) trouve = true;
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
                        foreach (Club c in tc.Ranking())
                        {
                            tbActu.Text += i + " " + c.shortName + " " + t.Points(c) + ", " + t.Difference(c) + " / ";
                            i++;
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
            dgClubClassement.Items.Clear();
            if(_partie.club != null && _partie.club.Championship != null)
            {
                Round championnat = _partie.club.Championship.rounds[0];
                List<Club> classement = (championnat as ChampionshipRound).Ranking();
                int indice = classement.IndexOf(_partie.club);
                indice = indice - 2;
                if (indice < 0) indice = 0;
                if (indice > classement.Count - 5) indice = classement.Count - 5;
                for(int i = indice; i<indice+5; i++)
                {
                    Club c = classement[i];
                    dgClubClassement.Items.Add(new ClassementElement { Classement = i + 1, Club = c, Logo = Utils.Logo(c), Nom = c.shortName, Pts = championnat.Points(c), bc = championnat.GoalsAgainst(c), bp = championnat.GoalsFor(c), Diff = championnat.Difference(c), G = championnat.Wins(c), J = championnat.Played(c), N = championnat.Draws(c), P = championnat.Loses(c) });
                }
            }
        }

        private void ProchainsMatchsClub()
        {
            dgClubProchainsMatchs.Items.Clear();
            if(_partie.club != null)
            {
                List<Match> matchs = new List<Match>();
                foreach (Match m in _partie.kernel.Matchs)
                {
                    if (m.home == _partie.club || m.away == _partie.club) matchs.Add(m);
                }
                matchs.Sort(new MatchDateComparator());
                int diff = -1;
                int indice = -1;
                foreach (Match m in matchs)
                {
                    TimeSpan ts = m.day - _partie.date;

                    int diffM = Math.Abs(ts.Days);
                    if (diffM < diff || diff == -1)
                    {
                        diff = diffM;
                        indice = matchs.IndexOf(m);
                    }
                }
                indice = indice - 2;
                if (indice < 0) indice = 0;
                if (indice > matchs.Count - 3) indice = matchs.Count - 3;
                for(int i = indice; i<indice+5; i++)
                {
                    //Cas où si jamais il y a moins de 5 matchs pour le club
                    if(i < matchs.Count && i>=0)
                    {
                        Match m = matchs[i];
                        string score = m.score1 + " - " + m.score2;
                        if (!m.Played)
                        {
                            score = m.day.ToShortDateString();
                        }
                        dgClubProchainsMatchs.Items.Add(new ProchainMatchElement { Match = m, Competition = m.Tournament.shortName, Equipe1 = m.home.shortName, Equipe2 = m.away.shortName, Score = score, LogoD = Utils.Logo(m.home), LogoE = Utils.Logo(m.away) });
                    }
                }
            }
        }
        
        private void Calendrier(Round t)
        {
            dgMatchs.Items.Clear();
            List<Match> matchs = new List<Match>(t.matches);
            matchs.Sort(new MatchDateComparator());
            DateTime lastTime = new DateTime(2000, 1, 1);
            KnockoutRound te = t as KnockoutRound;
            foreach (Match m in matchs)
            {
                if (lastTime != m.day.Date)
                {
                    dgMatchs.Items.Add(new CalendrierElement { Heure=m.day.ToShortDateString()});
                }
                lastTime = m.day.Date;
                string score = "A jouer";
                string affluence = "-";
                if (m.Played)
                {
                    score = m.score1 + " - " + m.score2;
                    affluence = m.attendance.ToString();
                    if (m.prolongations) score += " ap";
                    if (m.PenaltyShootout) score += " (" + m.penaltyShootout1 + "-" + m.penaltyShootout2 + " tab)";
                }
                string equipe1 = m.home.shortName;
                string equipe2 = m.away.shortName;

                Tournament champD = m.home.Championship;
                Tournament champE = m.away.Championship;
                if (te != null && champD != null && champE != null)
                {
                    equipe1 += " (" + champD.shortName + ")";
                    equipe2 += " (" + champE.shortName + ")";
                }
                dgMatchs.Items.Add(new CalendrierElement { Heure = m.day.ToShortTimeString(), Equipe1 = equipe1, Equipe2 = equipe2, Score = score, Affluence = affluence, Match = m, Cote1 = m.odd1.ToString("0.00"), Cote2 = m.odd2.ToString("0.00"), CoteN = m.oddD.ToString("0.00") });
            }
        }

        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
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

        private void DgClubProchainsMatchs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Match m = dgClubProchainsMatchs.SelectedItem as Match;
            if (m != null)
            {
                Windows_Match wm = new Windows_Match(m);
                wm.Show();
            }
        }

        private void DgClubClassement_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void BtnDroite_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnGauche_Click(object sender, RoutedEventArgs e)
        {

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

    public struct ProchainMatchElement : IEquatable<ProchainMatchElement>
    {
        public string Competition { get; set; }
        public string LogoD { get; set; }
        public string LogoE { get; set; }
        public string Equipe1 { get; set; }
        public string Equipe2 { get; set; }
        public string Score { get; set; }
        public Match Match { get; set; }
        public bool Equals(ProchainMatchElement other)
        {
            throw new NotImplementedException();
        }
    }
}