﻿using Microsoft.Win32;
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
using System.Windows.Shapes;
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
        private Partie _partie = null;

        private DateTime _calendrierJour;

        public Windows_Menu()
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\close.png"));
            imgBtnGauche.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\left.png"));
            imgBtnDroite.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\Images\\right.png"));

            _partie = Session.Instance.Partie;

            imgClub.Source = new BitmapImage(new Uri(Utils.Logo(_partie.Club)));
            comboPays.Items.Clear();
            foreach (Continent c in _partie.Gestionnaire.Continents)
            {
                if (c.Competitions().Count > 0) this.comboPays.Items.Add(c);
                foreach (Pays p in c.Pays) if (p.Competitions().Count > 0) { this.comboPays.Items.Add(p); Console.WriteLine(p); }
            }
            Refresh();

            
        }

        private void RemplirArticles()
        {
            spNews.Children.Clear();
            foreach(Article a in Session.Instance.Partie.Articles)
            {
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.WrapWithOverflow;
                tb.Text = a.Publication.ToShortDateString() + " - " + a.Titre;
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
            List<Match> matchs = _partie.Avancer();
            if (matchs.Count > 0)
            {
                Windows_AvantMatch wam = new Windows_AvantMatch(matchs, _partie.Club);
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
            Competition c = lbChampionnats.SelectedItem as Competition;
            if(c != null)
            {
                lbTours.Items.Clear();
                dgClassement.Items.Clear();
                dgMatchs.Items.Clear();
                foreach (Tour t in c.Tours)
                {
                    lbTours.Items.Add(t);
                }

            }
        }

        private void LbTours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tour t = lbTours.SelectedItem as Tour;

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
                if (selected.Club as Club_Ville != null)
                {
                    Windows_Club club = new Windows_Club(selected.Club as Club_Ville);
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
            while (!(_partie.Date.Month == 6 && _partie.Date.Day == 13))
            {
                Avancer();
            }
        }

        private void BtnSimuler2_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i<10;i++)
            {
                _partie.Avancer();
                while (!(_partie.Date.Month == 6 && _partie.Date.Day == 13))
                {
                    _partie.Avancer();
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
                foreach (Competition c in localisation.Competitions())
                {
                    this.lbChampionnats.Items.Add(c);
                }
            }
        }

        private void Refresh()
        {
            this.labelDate.Content = _partie.Date.ToLongDateString();
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
            if (_partie.Club != null && _partie.Club.Championnat != null)
            {
                //Choix de la compétition à afficher dans le bandeau : compétition du dernier match joué par l'équipe
                List<Match> matchs = _partie.Club.Matchs;
                bool trouve = false;
                Competition comp = null;
                int i = 0;
                if (matchs.Count == 0) trouve = true;
                while(!trouve)
                {
                    if(!matchs[i].Joue)
                    {
                        comp = matchs[i].Competition;
                        trouve = true;
                    }
                    i++;
                    if (i + 1 == matchs.Count) trouve = true;
                }

                if(comp != null)
                {
                    Tour t = null;
                    if (comp.Championnat)
                    {
                        t = comp.Tours[0];
                        tbActu.Text = comp.Nom + " : ";
                        i = 1;
                        TourChampionnat tc = t as TourChampionnat;
                        foreach (Club c in tc.Classement())
                        {
                            tbActu.Text += i + " " + c.NomCourt + " " + t.Points(c) + ", " + t.Difference(c) + " / ";
                            i++;
                        }
                    }
                    else
                    {
                        t = comp.Tours[comp.TourActuel];
                        tbActu.Text += comp.Nom + ", " + t.Nom + " : ";
                        foreach(Match m in t.Matchs)
                        {
                            tbActu.Text += m.Domicile.NomCourt + " " + m.Score1 + "-" + m.Score2 + " " + m.Exterieur.NomCourt + ", ";
                        }
                    }
                }
            }
        }

        private void ClassementClub()
        {
            dgClubClassement.Items.Clear();
            if(_partie.Club != null && _partie.Club.Championnat != null)
            {
                Tour championnat = _partie.Club.Championnat.Tours[0];
                List<Club> classement = (championnat as TourChampionnat).Classement();
                int indice = classement.IndexOf(_partie.Club);
                indice = indice - 2;
                if (indice < 0) indice = 0;
                if (indice > classement.Count - 5) indice = classement.Count - 5;
                for(int i = indice; i<indice+5; i++)
                {
                    Club c = classement[i];
                    dgClubClassement.Items.Add(new ClassementElement { Classement = i + 1, Club = c, Logo = Utils.Logo(c), Nom = c.NomCourt, Pts = championnat.Points(c), bc = championnat.ButsContre(c), bp = championnat.ButsPour(c), Diff = championnat.Difference(c), G = championnat.Gagnes(c), J = championnat.Joues(c), N = championnat.Nuls(c), P = championnat.Perdus(c) });
                }
            }
        }

        private void ProchainsMatchsClub()
        {
            dgClubProchainsMatchs.Items.Clear();
            if(_partie.Club != null)
            {
                List<Match> matchs = new List<Match>();
                foreach (Match m in _partie.Gestionnaire.Matchs)
                {
                    if (m.Domicile == _partie.Club || m.Exterieur == _partie.Club) matchs.Add(m);
                }
                matchs.Sort(new Match_Date_Comparator());
                int diff = -1;
                int indice = -1;
                foreach (Match m in matchs)
                {
                    TimeSpan ts = m.Jour - _partie.Date;

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
                        string score = m.Score1 + " - " + m.Score2;
                        if (!m.Joue)
                        {
                            score = m.Jour.ToShortDateString();
                        }
                        dgClubProchainsMatchs.Items.Add(new ProchainMatchElement { Match = m, Competition = m.Competition.NomCourt, Equipe1 = m.Domicile.NomCourt, Equipe2 = m.Exterieur.NomCourt, Score = score, LogoD = Utils.Logo(m.Domicile), LogoE = Utils.Logo(m.Exterieur) });
                    }
                }
            }
        }
        
        private void Calendrier(Tour t)
        {
            dgMatchs.Items.Clear();
            List<Match> matchs = new List<Match>(t.Matchs);
            matchs.Sort(new Match_Date_Comparator());
            DateTime lastTime = new DateTime(2000, 1, 1);
            TourElimination te = t as TourElimination;
            foreach (Match m in matchs)
            {
                if (lastTime != m.Jour.Date)
                {
                    dgMatchs.Items.Add(new CalendrierElement { Heure=m.Jour.ToShortDateString()});
                }
                lastTime = m.Jour.Date;
                string score = "A jouer";
                string affluence = "-";
                if (m.Joue)
                {
                    score = m.Score1 + " - " + m.Score2;
                    affluence = m.Affluence.ToString();
                    if (m.Prolongations) score += " ap";
                    if (m.TAB) score += " (" + m.Tab1 + "-" + m.Tab2 + " tab)";
                }
                string equipe1 = m.Domicile.NomCourt;
                string equipe2 = m.Exterieur.NomCourt;

                Competition champD = m.Domicile.Championnat;
                Competition champE = m.Exterieur.Championnat;
                if (te != null && champD != null && champE != null)
                {
                    equipe1 += " (" + champD.NomCourt + ")";
                    equipe2 += " (" + champE.NomCourt + ")";
                }
                dgMatchs.Items.Add(new CalendrierElement { Heure = m.Jour.ToShortTimeString(), Equipe1 = equipe1, Equipe2 = equipe2, Score = score, Affluence = affluence, Match = m, Cote1 = m.Cote1.ToString("0.00"), Cote2 = m.Cote2.ToString("0.00"), CoteN = m.CoteN.ToString("0.00") });
            }
        }

        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                Session.Instance.Partie.Sauvegarder(saveFileDialog.FileName);
            }
        }

        private void BtnParticipants_Click(object sender, RoutedEventArgs e)
        {
            Competition c = lbChampionnats.SelectedItem as Competition;
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
            Competition c = lbChampionnats.SelectedItem as Competition;
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
        public Joueur Buteur { get; set; }
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