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
    /// Logique d'interaction pour Windows_Match.xaml
    /// </summary>
    public partial class Windows_Match : Window
    {


        public Windows_Match(Match match)
        {
            InitializeComponent();
            try
            {
                imgCompetition.Source = new BitmapImage(new Uri(Utils.LogoCompetition(match.Competition)));
            }
            catch { }
            lbStade.Content = match.Domicile.Stade.Nom;
            lbAffluence.Content = match.Affluence + " spectateurs";
            lbEquipe1.Content = match.Domicile.Nom;
            lbEquipe2.Content = match.Exterieur.Nom;
            lbScore.Content = match.Score1 + " - " + match.Score2;
            lbTirs1.Content = match.Statistiques.TirsDomicile;
            lbTirs2.Content = match.Statistiques.TirsExterieurs;
            pbTirs.Maximum = match.Statistiques.TirsDomicile + match.Statistiques.TirsExterieurs;
            pbTirs.Value = match.Statistiques.TirsDomicile;
            lbPossession1.Content = match.Statistiques.Possession1 + "%";
            lbPossession2.Content = match.Statistiques.Possession2 + "%";
            pbPossession.Maximum = match.Statistiques.Possession1 + match.Statistiques.Possession2;
            pbPossession.Value = match.Statistiques.Possession1;
            if (match.Prolongations)
                lbScore.Content += " a.p.";
            if(match.TAB)
            {
                lbScore.Content += " (" + match.Tab1 + "-" + match.Tab2 + " tab)";
            }
            lbMT.Content = "(" + match.ScoreMT1 + " - " + match.ScoreMT2 + ")";

            foreach(EvenementMatch em in match.Evenements)
            {
                string icone = "";
                switch (em.Type)
                {
                    case Evenement.BUT: icone = "goal.png";
                        break;
                    case Evenement.BUT_PENALTY: icone = "goal.png";
                        break;
                    case Evenement.BUT_CSC:
                        icone = "goal.png";
                        break;
                    case Evenement.CARTON_JAUNE:
                        icone = "yellow_card.png";
                        break;
                    case Evenement.CARTON_ROUGE:
                        icone = "red_card.png";
                        break;
                }
                string c1 = "";
                string c2 = "";
                string c3 = "";
                string c4 = "";
                string img1 = "";
                string img2 = "";
                if (em.Club == match.Domicile)
                {
                    img1 = Utils.Image(icone);
                    c1 = em.MinuteStr;
                    c2 = em.Joueur.Prenom + " " + em.Joueur.Nom;
                    if (em.Type == Evenement.BUT_PENALTY) c2 += " (sp)";
                    if (em.Type == Evenement.BUT_CSC) c2 += " (csc)";
                }
                else
                {
                    img2 = Utils.Image(icone);
                    c4 = em.MinuteStr;
                    c3 = em.Joueur.Prenom + " " + em.Joueur.Nom;
                    if (em.Type == Evenement.BUT_PENALTY) c3 += " (sp)";
                    if (em.Type == Evenement.BUT_CSC) c3 += " (csc)";
                }
                dgEvenements.Items.Add(new EvenementElement { Col1 = c1, Col2 = c2, Col3 = c3, Col4 = c4, Img1 = img1, Img2 = img2 });
                
            }

            foreach (Joueur j in match.Compo1)
            {
                dgCompo1.Items.Add(new JoueurElement { Joueur = j, Poste = j.Poste });
            }

            foreach (Joueur j in match.Compo2)
            {
                dgCompo2.Items.Add(new JoueurElement { Joueur = j, Poste = j.Poste });
            }

            foreach (Journaliste j in match.Journalistes)
            {
                dgJournalistes.Items.Add(new JournalisteElement { Journaliste = j, Media = j.Media.Nom });
            }
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DgJournalistes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgJournalistes.SelectedItem != null)
            {
                JournalisteElement selected = (JournalisteElement)dgJournalistes.SelectedItem;
                Windows_Journaliste wj = new Windows_Journaliste(selected.Journaliste);
                wj.Show();
            }
        }

        private void DgCompo1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgCompo1.SelectedItem != null)
            {
                JoueurElement je = (JoueurElement)dgCompo1.SelectedItem;
                Windows_Joueur wj = new Windows_Joueur(je.Joueur);
                wj.Show();
            }
        }

        private void DgCompo2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCompo2.SelectedItem != null)
            {
                JoueurElement je = (JoueurElement)dgCompo2.SelectedItem;
                Windows_Joueur wj = new Windows_Joueur(je.Joueur);
                wj.Show();
            }
        }
    }

    public struct JoueurElement
    {
        public Joueur Joueur { get; set; }
        public Poste Poste { get; set; }
    }

    public struct JournalisteElement
    {
        public Journaliste Journaliste { get; set; }
        public string Media{ get; set; }
    }

    public struct EvenementElement
    {
        public string Col1 { get; set; }
        public string Col2 { get; set; }
        public string Col3 { get; set; }
        public string Col4 { get; set; }
        public string Img1 { get; set; }
        public string Img2 { get; set; }
    }
}
