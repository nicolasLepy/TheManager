using System;
using System.Windows;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Participants.xaml
    /// </summary>
    public partial class Windows_Participants : Window
    {
        public Windows_Participants(Tournament c)
        {
            InitializeComponent();
            if (c.isChampionship)
            {
                dgClubs.Items.Clear();
                foreach (Club cl in c.rounds[0].clubs)
                {
                    int budget = 0;
                    int masseSalariale = 0;
                    if (cl as CityClub != null)
                    {
                        budget = (cl as CityClub).budget;
                        masseSalariale = (int)(cl as CityClub).SalaryMass;
                    }

                    float etoiles = cl.Stars;
                    string e1 = "";
                    string e2 = "";
                    string e3 = "";
                    string e4 = "";
                    string e5 = "";
                    if (etoiles >= 1)
                    {
                        e1 = Utils.Image("star.png");
                    }

                    if (etoiles >= 2)
                    {
                        e2 = Utils.Image("star.png");
                    }

                    if (etoiles >= 3)
                    {
                        e3 = Utils.Image("star.png");
                    }

                    if (etoiles >= 4)
                    {
                        e4 = Utils.Image("star.png");
                    }

                    if (etoiles >= 5)
                    {
                        e5 = Utils.Image("star.png");
                    }

                    if (etoiles < 1)
                    {
                        e1 = Utils.Image("demistar.png");
                    }

                    if (etoiles > 1 && etoiles < 2)
                    {
                        e2 = Utils.Image("demistar.png");
                    }

                    if (etoiles > 2 && etoiles < 3)
                    {
                        e3 = Utils.Image("demistar.png");
                    }

                    if (etoiles > 3 && etoiles < 4)
                    {
                        e4 = Utils.Image("demistar.png");
                    }

                    if (etoiles > 4 && etoiles < 5)
                    {
                        e5 = Utils.Image("demistar.png");
                    }

                    dgClubs.Items.Add(new ClubElement { Nom = cl.shortName, Niveau = cl.Level(), Budget = budget, Affluence = c.AverageAttendance(cl), MasseSalariale = masseSalariale, Status = FindResource(Utils.ClubStatus2ResourceString(cl.status)).ToString(), Star1=e1, Star2=e2, Star3=e3, Star4=e4,Star5=e5 });
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
        public string Status { get; set; }
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
