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
        private Match _match;
        private Club _club;

        public Windows_AvantMatch(Match m, Club c)
        {
            InitializeComponent();
            _match = m;
            _club = c;

            


            foreach (Joueur j in c.Joueurs())
            {
                dgJoueursDispo.Items.Add(new JoueurCompoElement { Poste = j.Poste.ToString(), Age = j.Age, Energie = j.Energie, Niveau = j.Niveau, Nom = j});
            }

            lbMatch.Content = m.Domicile + " - " + m.Exterieur;
            lbStade.Content = m.Domicile.Stade.Nom;
            lbCote1.Content = m.Cote1.ToString("0.00");
            lbCoteN.Content = m.CoteN.ToString("0.00");
            lbCote2.Content = m.Cote2.ToString("0.00");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            bool continuer = false;
            if(dgCompo.Items.Count < 11)
            {
                MessageBoxResult result = MessageBox.Show("Moins de 11 joueurs sélectionnés. Continuer ?", "Composition", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) continuer = true;
            }
            else
            {
                continuer = true;
            }
            if (continuer)
            {
                List<Joueur> compo = new List<Joueur>();
                for (int i = 0; i < dgCompo.Items.Count; i++)
                {
                    JoueurCompoElement jce = (JoueurCompoElement)dgCompo.Items[i];
                    compo.Add(jce.Nom);
                }

                _match.DefinirCompo(compo, _club);
                Windows_JouerMatch wjm = new Windows_JouerMatch(_match);
                wjm.ShowDialog ();
                Close();
            }
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
    }

    public struct JoueurCompoElement
    {
        public string Poste { get; set; }
        public Joueur Nom { get; set; }
        public int Age { get; set; }
        public int Niveau { get; set; }
        public int Energie { get; set; }
    }
}
