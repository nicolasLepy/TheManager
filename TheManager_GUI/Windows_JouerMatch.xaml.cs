using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Logique d'interaction pour Windows_JouerMatch.xaml
    /// </summary>
    public partial class Windows_JouerMatch : Window
    {
        private List<Match> _matchs;
        private Tour _tour;

        private Media _media;

        private bool _enCours;


        async Task Match(Match match)
        {
            List<RetourMatch> res = match.MinuteSuivante();
            if (Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, res)) _enCours = false;
            //Si y a un évenement
            if (Utils.RetoursContient(RetourMatchEvenement.EVENEMENT,res))
            {
                EvenementMatch em = match.Evenements[match.Evenements.Count - 1];
                string icone = "";
                
                if(em.Type == Evenement.BUT || em.Type == Evenement.BUT_PENALTY || em.Type == Evenement.BUT_CSC)
                {
                    if(em.Club == match.Domicile)
                    {
                        //_media.But(match);
                    }


                    //Refresh en cas de but
                    Classement();
                    Matchs();
                    if (match == _matchs[0]) ActionsMatch();
                    icone = "goal.png";
                }
                else if(em.Type == Evenement.CARTON_JAUNE)
                {
                    icone = "yellow_card.png";
                }
                else if(em.Type == Evenement.CARTON_ROUGE)
                {
                    icone = "red_card.png";
                }
                dgEvenements.Items.Insert(0,new MatchLiveEvenementElement { Logo = Utils.Image(icone), Minute = em.MinuteStr, Joueur = em.Joueur.Nom + " (" + em.Joueur.Club.NomCourt + ")", Evenement = match.Domicile + " - " + match.Exterieur + " : " + match.Score1 + " - " + match.Score2 });

            }


            //Refresh
            if (match == _matchs[0])
            {
                lbTemps.Content = match.Temps;
                lbScore.Content = match.Score1 + " - " + match.Score2;
                lbTirs1.Content = match.Statistiques.TirsDomicile;
                lbTirs2.Content = match.Statistiques.TirsExterieurs;
                pbTirs.Maximum = match.Statistiques.TirsDomicile + match.Statistiques.TirsExterieurs;
                pbTirs.Value = match.Statistiques.TirsDomicile;
            }
            //Thread t = new Thread(new ThreadStart(ThreadClassement));
            //t.Start();


            await Task.Delay((int)sliderVitesseSimulation.Value);
        }

        public void ThreadMatch(Match match)
        {
            
            this.Dispatcher.Invoke(async () =>
            {
                while(_enCours == true)
                {
                    await Match(match);
                }
                btnTerminer.Visibility = Visibility.Visible;
            });
            
        }

        public void ThreadClassement()
        {
           this.Dispatcher.Invoke(() =>
           {
               Classement();
           });
        }

        public Windows_JouerMatch(List<Match> matchs)
        {
            InitializeComponent();
            _media = new Media();
            _enCours = true;
            _matchs = matchs;
            _tour = _matchs[0].Tour;
            Matchs();

            _media.Ambiance4000();

            try
            {
                imgEquipe1.Source = new BitmapImage(new Uri(Utils.Logo(_matchs[0].Domicile)));
                imgEquipe2.Source = new BitmapImage(new Uri(Utils.Logo(_matchs[0].Exterieur)));
            }
            catch { }
            lbEquipe1.Content = _matchs[0].Domicile;
            lbEquipe2.Content = _matchs[0].Exterieur;


            //Thread t = new Thread(new ThreadStart(ThreadMatch));

            for (int i = 0; i<_matchs.Count; i++)
            {
                int j = i;
                Thread t = new Thread(() => ThreadMatch(_matchs[j]));
                t.Start();
            }
        }
        

        private void ActionsMatch()
        {
            dgActions.Items.Clear();
            foreach(KeyValuePair<string,string> kvp in _matchs[0].Actions)
            {
                dgActions.Items.Insert(0,new MatchLiveAction { Minute = kvp.Key, Action = kvp.Value });
            }
        }

        private void Matchs()
        {
            dgMatchs.Items.Clear();
            foreach(Match match in _matchs)
            {
                dgMatchs.Items.Add(new MatchLiveElement { Equipe1 = match.Domicile, Equipe2 = match.Exterieur, Score = match.Score1 + " - " + match.Score2 });
            }
        }

        private void Classement()
        {
            if(_tour != null)
            {
                dgClassement.Items.Clear();
                TourChampionnat tc = _tour as TourChampionnat;
                if(tc != null)
                {
                    int i = 1;
                    List<Club> classement = tc.Classement();
                    foreach(Club c in classement)
                    {
                        dgClassement.Items.Add(new ClassementElement { Classement = i, Club = c, Logo = Utils.Logo(c), Nom = c.NomCourt, Pts = tc.Points(c), J = tc.Joues(c), bc = tc.ButsContre(c), bp = tc.ButsPour(c), Diff = tc.Difference(c), G = tc.Gagnes(c), N = tc.Nuls(c), P = tc.Perdus(c) });
                        i++;
                    }
                }
            }
        }

        private void BtnTerminer_Click(object sender, RoutedEventArgs e)
        {
            _media.Detruire();
            Close();
        }

        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i<_matchs.Count; i++)
            {
                Match match = _matchs[i];
                bool termine = false;
                while(!termine)
                {
                    List<RetourMatch> rm = match.MinuteSuivante();
                    if (Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, rm)) termine = true;
                }
                //_matchs[i].Jouer();
            }
            //while (!Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, _matchs[0].MinuteSuivante())) ;
            _media.Detruire();
            Close();
        }
    }

    public struct MatchLiveElement
    {
        public Club Equipe1 { get; set; }
        public string Score { get; set; }
        public Club Equipe2 { get; set; }
    }

    public struct MatchLiveEvenementElement
    {
        public string Logo { get; set; }
        public string Minute { get; set; }
        public string Evenement { get; set; }
        public string Joueur { get; set; }
    }

    public struct MatchLiveAction
    {
        public string Minute { get; set; }
        public string Action { get; set; }
    }
}
