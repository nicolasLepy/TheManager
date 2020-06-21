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
    /// Logique d'interaction pour Windows_Participants.xaml
    /// </summary>
    public partial class Windows_Participants : Window
    {
        public Windows_Participants(Competition c)
        {
            InitializeComponent();
            if (c.Championnat)
            {
                dgClubs.Items.Clear();
                foreach (Club cl in c.Tours[0].Clubs)
                {
                    int budget = 0;
                    int masseSalariale = 0;
                    if (cl as Club_Ville != null)
                    {
                        budget = (cl as Club_Ville).Budget;
                        masseSalariale = (int)(cl as Club_Ville).MasseSalariale;
                    }

                    float etoiles = cl.Etoiles;
                    string e1 = "";
                    string e2 = "";
                    string e3 = "";
                    string e4 = "";
                    string e5 = "";
                    if (etoiles >= 1)
                        e1 = Utils.Image("star.png");
                    if (etoiles >= 2)
                        e2 = Utils.Image("star.png");
                    if (etoiles >= 3)
                        e3 = Utils.Image("star.png");
                    if (etoiles >= 4)
                        e4 = Utils.Image("star.png");
                    if (etoiles >= 5)
                        e5 = Utils.Image("star.png");

                    if (etoiles < 1)
                        e1 = Utils.Image("demistar.png");
                    if (etoiles > 1 && etoiles < 2)
                        e2 = Utils.Image("demistar.png");
                    if (etoiles > 2 && etoiles < 3)
                        e3 = Utils.Image("demistar.png");
                    if (etoiles > 3 && etoiles < 4)
                        e4 = Utils.Image("demistar.png");
                    if (etoiles > 4 && etoiles < 5)
                        e5 = Utils.Image("demistar.png");

                    dgClubs.Items.Add(new ClubElement { Nom = cl.NomCourt, Niveau = cl.Niveau(), Budget = budget, Affluence = c.AffluenceMoyenne(cl), MasseSalariale = masseSalariale, Star1=e1, Star2=e2, Star3=e3, Star4=e4,Star5=e5 });
                }
            }
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public struct ClubElement : IEquatable<ClubElement>
    {
        public string Nom { get; set; }
        public float Niveau { get; set; }
        public int Budget { get; set; }
        public int Affluence { get; set; }
        public int MasseSalariale { get; set; }
        public string Star1 { get; set; }
        public string Star2 { get; set; }
        public string Star3 { get; set; }
        public string Star4 { get; set; }
        public string Star5 { get; set; }
        public bool Equals(ClubElement other)
        {
            throw new NotImplementedException();
        }
    }
}
