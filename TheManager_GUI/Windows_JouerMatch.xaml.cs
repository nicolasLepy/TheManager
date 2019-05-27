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

        private  Match _match;
        private Tour _tour;

        private Media _media;

        private bool _enCours;


        async Task Match()
        {
            List<RetourMatch> res = _match.MinuteSuivante();
            if (Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH, res)) _enCours = false;
            //Si y a un évenement
            if (Utils.RetoursContient(RetourMatchEvenement.EVENEMENT,res))
            {
                EvenementMatch em = _match.Evenements[_match.Evenements.Count - 1];
                if(em.Type == Evenement.BUT || em.Type == Evenement.BUT_PENALTY || em.Type == Evenement.BUT_CSC)
                {
                    if(em.Club == _match.Domicile)
                    {
                        _media.But12000();
                    }
                }
            }


            //Refresh
            lbTemps.Content = _match.Temps;
            lbScore.Content = _match.Score1 + " - " + _match.Score2;
            Classement();

            await Task.Delay((int)sliderVitesseSimulation.Value);
        }

        public void ThreadProc()
        {
            
            this.Dispatcher.Invoke(async () =>
            {
                while(_enCours == true)
                {
                    await Match();
                    
                    //Thread.CurrentThread.Join(160);

                }
            });
            
            
            
        }

        public Windows_JouerMatch(Match m)
        {
            InitializeComponent();
            _enCours = true;
            _match = m;
            _media = new Media();
            _tour = _match.Tour;

            _media.Ambiance12000();

            Thread t = new Thread(new ThreadStart(ThreadProc));

            
            t.Start();


        }



        private void BtnSimuler_Click(object sender, RoutedEventArgs e)
        {
            while (!Utils.RetoursContient(RetourMatchEvenement.FIN_MATCH,_match.MinuteSuivante()));
            _media.Detruire();
            Close();
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
                        dgClassement.Items.Add(new ClassementElement { Classement = i, Club = c, Logo = Utils.Logo(c), Nom = c.NomCourt, Pts = tc.Points(c), J = tc.Joues(c), bc = tc.ButsContre(c), bp = tc.ButsPour(c), Diff = tc.Difference(c), G = tc.Gagnes(c), N = tc.Nuls(c), P = tc.ButsPour(c) });
                        i++;
                    }
                }
            }
        }
    }
}
