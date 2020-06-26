using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_AvantMatch.xaml
    /// </summary>
    public partial class Windows_AvantMatch : Window
    {
        private List<Match> _matchs;
        private Club _club;
        private List<Player> _joueurs;

        public void AfficherComposition()
        {
            spDefenseurs.Children.Clear();
            spMilieux.Children.Clear();
            spAttaquants.Children.Clear();
            spGardiens.Children.Clear();

            foreach(Player j in _joueurs)
            {
                StackPanel conteneur = null;
                switch(j.position)
                {
                    case Position.Goalkeeper: conteneur = spGardiens;break;
                    case Position.Defender: conteneur = spDefenseurs; break;
                    case Position.Midfielder: conteneur = spMilieux; break;
                    case Position.Striker: conteneur = spAttaquants; break;
                }

                StackPanel conteneurJoueur = new StackPanel();
                conteneurJoueur.Orientation = Orientation.Vertical;

                Label label = new Label();
                label.Content = j.lastName;
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.Style = Application.Current.FindResource("StyleLabel2") as Style;

                ProgressBar pb = new ProgressBar();
                pb.Value = j.energy;
                pb.Maximum = 100;
                pb.Height = 5;
                pb.Width = 40;


                Label note = new Label();
                note.Content = j.level;
                note.HorizontalAlignment = HorizontalAlignment.Center;
                note.FontSize = 10;
                note.Style = Application.Current.FindResource("StyleLabel2") as Style;

                conteneurJoueur.Children.Add(label);
                conteneurJoueur.Children.Add(pb);
                conteneurJoueur.Children.Add(note);

                conteneur.Children.Add(conteneurJoueur);

            }
        }

        public Windows_AvantMatch(List<Match> m, Club c)
        {
            InitializeComponent();
            _joueurs = new List<Player>();
            _matchs = m;
            _club = c;

            foreach(Match match in _matchs)
            {
                dgMatchs.Items.Add(new ProgrammeElement { Heure = match.day.ToShortTimeString(), Equipe1 = match.home, Equipe2 = match.away, Score = match.score1 + " - " + match.score2 });
            }


            foreach (Player j in c.Players())
            {
                dgJoueursDispo.Items.Add(new JoueurCompoElement { Poste = j.position.ToString(), Age = j.Age, Energie = j.energy, Niveau = j.level, Nom = j});
            }

            lbMatch.Content = m[0].home + " - " + m[0].away;
            lbStade.Content = m[0].home.stadium.name;
            lbCote1.Content = m[0].odd1.ToString("0.00");
            lbCoteN.Content = m[0].oddD.ToString("0.00");
            lbCote2.Content = m[0].odd2.ToString("0.00");
        }

        
        private void DgJoueursDispo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(_joueurs.Count < 11)
            {
                if(dgJoueursDispo.SelectedItem != null)
                {
                    JoueurCompoElement jce = (JoueurCompoElement)dgJoueursDispo.SelectedItem;
                    dgJoueursDispo.Items.Remove(jce);
                    _joueurs.Add(jce.Nom);
                    AfficherComposition();
                }
            }
        }

        private void BtnCompoAuto_Click(object sender, RoutedEventArgs e)
        {
            _joueurs.Clear();

            List<Player> compo = _club.Composition();
            foreach(Player j in compo)
            {
                _joueurs.Add(j);
            }
            AfficherComposition();
        }

        private void BtnRAZ_Click(object sender, RoutedEventArgs e)
        {
            _joueurs.Clear();
            AfficherComposition();
        }


        private void SetPlayerClubCompo()
        {
            List<Player> compo = new List<Player>();
            for (int i = 0; i < _joueurs.Count; i++)
            {
                compo.Add(_joueurs[i]);
            }

            _matchs[0].SetCompo(compo, _club);
   
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {

           
            if (VerifierComposition())
            {
                SetPlayerClubCompo();
                Windows_JouerMatch wjm = new Windows_JouerMatch(_matchs);
                wjm.ShowDialog();
                Close();
            }
        }


        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            
            if (VerifierComposition())
            {
                SetPlayerClubCompo();
                
                
                foreach(Match m in _matchs)
                {
                    m.Play();
                }
                Close();
            }
        }

        private bool VerifierComposition()
        {
            bool pursue = false;
            if (_joueurs.Count < 11)
            {
                MessageBoxResult result = MessageBox.Show("Moins de 11 joueurs sélectionnés. Continuer ?", "Composition", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    pursue = true;
                }
            }
            else
            {
                pursue = true;
            }
            return pursue;
        }
    }

    public struct JoueurCompoElement : IEquatable<JoueurCompoElement>
    {
        public string Poste { get; set; }
        public Player Nom { get; set; }
        public int Age { get; set; }
        public int Niveau { get; set; }
        public int Energie { get; set; }
        public bool Equals(JoueurCompoElement other)
        {
            throw new NotImplementedException();
        }
    }

    public struct ProgrammeElement : IEquatable<ProgrammeElement>
    {
        public string Heure { get; set; }
        public Club Equipe1 { get; set; }
        public string Score { get; set; }
        public Club Equipe2 { get; set; }
        public bool Equals(ProgrammeElement other)
        {
            throw new NotImplementedException();
        }
    }
}
