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

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_AvantMatch.xaml
    /// </summary>
    public partial class Windows_AvantMatch : Window
    {
        private List<Match> _matchs;
        private Club _club;
        private List<Joueur> _joueurs;

        public void AfficherComposition()
        {
            spDefenseurs.Children.Clear();
            spMilieux.Children.Clear();
            spAttaquants.Children.Clear();
            spGardiens.Children.Clear();

            foreach(Joueur j in _joueurs)
            {
                StackPanel conteneur = null;
                switch(j.Poste)
                {
                    case Poste.GARDIEN: conteneur = spGardiens;break;
                    case Poste.DEFENSEUR: conteneur = spDefenseurs; break;
                    case Poste.MILIEU: conteneur = spMilieux; break;
                    case Poste.ATTAQUANT: conteneur = spAttaquants; break;
                }

                StackPanel conteneurJoueur = new StackPanel();
                conteneurJoueur.Orientation = Orientation.Vertical;

                Label label = new Label();
                label.Content = j.Nom;
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.Style = Application.Current.FindResource("StyleLabel2") as Style;

                ProgressBar pb = new ProgressBar();
                pb.Value = j.Energie;
                pb.Maximum = 100;
                pb.Height = 5;
                pb.Width = 40;


                Label note = new Label();
                note.Content = j.Niveau;
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
            _joueurs = new List<Joueur>();
            _matchs = m;
            _club = c;

            foreach(Match match in _matchs)
            {
                dgMatchs.Items.Add(new ProgrammeElement { Heure = match.Jour.ToShortTimeString(), Equipe1 = match.Domicile, Equipe2 = match.Exterieur, Score = match.Score1 + " - " + match.Score2 });
            }


            foreach (Joueur j in c.Joueurs())
            {
                dgJoueursDispo.Items.Add(new JoueurCompoElement { Poste = j.Poste.ToString(), Age = j.Age, Energie = j.Energie, Niveau = j.Niveau, Nom = j});
            }

            lbMatch.Content = m[0].Domicile + " - " + m[0].Exterieur;
            lbStade.Content = m[0].Domicile.Stade.Nom;
            lbCote1.Content = m[0].Cote1.ToString("0.00");
            lbCoteN.Content = m[0].CoteN.ToString("0.00");
            lbCote2.Content = m[0].Cote2.ToString("0.00");
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

            List<Joueur> compo = _club.Composition();
            foreach(Joueur j in compo)
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


        private void Button_Click(object sender, RoutedEventArgs e)
        {

           
            if (VerifierComposition())
            {
                List<Joueur> compo = new List<Joueur>();
                for (int i = 0; i < _joueurs.Count; i++)
                {
                    compo.Add(_joueurs[i]);
                }

                _matchs[0].DefinirCompo(compo, _club);
                Windows_JouerMatch wjm = new Windows_JouerMatch(_matchs);
                wjm.ShowDialog();
                Close();
            }
        }


        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            
            if (VerifierComposition())
            {
                List<Joueur> compo = new List<Joueur>();
                for (int i = 0; i < _joueurs.Count; i++)
                {
                    compo.Add(_joueurs[i]);
                }

                _matchs[0].DefinirCompo(compo, _club);
                foreach(Match m in _matchs)
                {
                    m.Jouer();
                }
                Close();
            }
        }

        private bool VerifierComposition()
        {
            bool continuer = false;
            if (_joueurs.Count < 11)
            {
                MessageBoxResult result = MessageBox.Show("Moins de 11 joueurs sélectionnés. Continuer ?", "Composition", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) continuer = true;
            }
            else
            {
                continuer = true;
            }
            return continuer;
        }
    }

    public struct JoueurCompoElement
    {
        public string Poste { get; set; }
        public Joueur Nom { get; set; }
        public int Age { get; set; }
        public int Niveau { get; set; }
        public int Energie { get; set; }
    }

    public struct ProgrammeElement
    {
        public string Heure { get; set; }
        public Club Equipe1 { get; set; }
        public string Score { get; set; }
        public Club Equipe2 { get; set; }
    }
}
