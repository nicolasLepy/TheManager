using System;
using System.Collections.Generic;
using System.Windows;
using TheManager;
using TheManager.Comparators;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Journaliste.xaml
    /// </summary>
    public partial class Windows_Journaliste : Window
    {
        public Windows_Journaliste(Journalist journaliste)
        {
            InitializeComponent();
            lbJournaliste.Content = journaliste.firstName + " " + journaliste.lastName;
            lbMedia.Content = journaliste.Media.name;
            List<Match> matchs = new List<Match>();
            foreach (Match m in Session.Instance.Game.kernel.Matchs)
            {
                foreach(KeyValuePair<TheManager.Media, Journalist> j in m.journalists)
                {
                    if (j.Value == journaliste) matchs.Add(m);
                }
            }
            matchs.Sort(new MatchDateComparator());

            foreach(Match m in matchs)
            {
                dgMatchs.Items.Add(new MatchElement { Date = m.day.ToShortDateString(), Heure = m.day.ToShortTimeString(), Equipe1 = m.home.name, Equipe2 = m.away.name });
            }
            
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public struct MatchElement : IEquatable<MatchElement>
    {
        public string Date { get; set; }
        public string Heure { get; set; }
        public string Equipe1 { get; set; }
        public string Equipe2 { get; set; }
        public bool Equals(MatchElement other)
        {
            throw new NotImplementedException();
        }
    }
}
