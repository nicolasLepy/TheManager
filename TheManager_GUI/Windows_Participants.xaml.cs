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

                    dgClubs.Items.Add(new ClubElement { Nom = cl.NomCourt, Niveau = cl.Niveau(), Budget = budget, Affluence = c.AffluenceMoyenne(cl), MasseSalariale = masseSalariale });
                }
            }
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public struct ClubElement
    {
        public string Nom { get; set; }
        public float Niveau { get; set; }
        public int Budget { get; set; }
        public int Affluence { get; set; }
        public int MasseSalariale { get; set; }
    }
}
