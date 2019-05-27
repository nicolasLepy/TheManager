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

        public Windows_AvantMatch(List<Match> m, Club c)
        {
            InitializeComponent();
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
            if(dgCompo.Items.Count < 11)
            {
                if(dgJoueursDispo.SelectedItem != null)
                {
                    JoueurCompoElement jce = (JoueurCompoElement)dgJoueursDispo.SelectedItem;
                    dgJoueursDispo.Items.Remove(jce);
                    dgCompo.Items.Add(jce);
                }
            }
        }

        private void BtnCompoAuto_Click(object sender, RoutedEventArgs e)
        {
            dgCompo.Items.Clear();

            List<Joueur> compo = _club.Composition();
            foreach(Joueur j in compo)
            {
                dgCompo.Items.Add(new JoueurCompoElement { Poste = j.Poste.ToString(), Age = j.Age, Energie = j.Energie, Niveau = j.Niveau, Nom = j});
            }
        }

        private void BtnRAZ_Click(object sender, RoutedEventArgs e)
        {
            dgCompo.Items.Clear();
        }

        private void DgCompo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgCompo.SelectedItem != null)
            {
                JoueurCompoElement jce = (JoueurCompoElement)dgCompo.SelectedItem;
                dgCompo.Items.Remove(jce);
                dgJoueursDispo.Items.Add(jce);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

           
            if (VerifierComposition())
            {
                List<Joueur> compo = new List<Joueur>();
                for (int i = 0; i < dgCompo.Items.Count; i++)
                {
                    JoueurCompoElement jce = (JoueurCompoElement)dgCompo.Items[i];
                    compo.Add(jce.Nom);
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
                for (int i = 0; i < dgCompo.Items.Count; i++)
                {
                    JoueurCompoElement jce = (JoueurCompoElement)dgCompo.Items[i];
                    compo.Add(jce.Nom);
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
            if (dgCompo.Items.Count < 11)
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
