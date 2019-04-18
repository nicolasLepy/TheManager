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

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Journaliste.xaml
    /// </summary>
    public partial class Windows_Journaliste : Window
    {
        public Windows_Journaliste(Journaliste journaliste)
        {
            InitializeComponent();
            lbJournaliste.Content = journaliste.Prenom + " " + journaliste.Nom;
            lbMedia.Content = journaliste.Media.Nom;
            List<Match> matchs = new List<Match>();
            foreach (Match m in Session.Instance.Partie.Gestionnaire.Matchs)
            {
                if (m.Journalistes.Contains(journaliste)) matchs.Add(m);
            }
            matchs.Sort(new Match_Date_Comparator());

            foreach(Match m in matchs)
            {
                dgMatchs.Items.Add(new MatchElement { Date = m.Jour.ToShortDateString(), Heure = m.Jour.ToShortTimeString(), Equipe1 = m.Domicile.Nom, Equipe2 = m.Exterieur.Nom });
            }
            
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public struct MatchElement
    {
        public string Date { get; set; }
        public string Heure { get; set; }
        public string Equipe1 { get; set; }
        public string Equipe2 { get; set; }
    }
}
