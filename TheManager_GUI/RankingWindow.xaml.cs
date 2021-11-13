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
    /// Logique d'interaction pour RankingWindow.xaml
    /// </summary>
    public partial class RankingWindow : Window
    {
        public RankingWindow()
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            FillFifaRanking();
            FillUEFARanking();
            FillUEFAClubRanking();
        }

        private void FillUEFARanking()
        {
            int i = 0;
            StackPanel spHead = new StackPanel();
            spHead.Orientation = Orientation.Horizontal;
            spHead.Children.Add(ViewUtils.CreateLabel("", "StyleLabel2Center", -1, 30, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("Nation", "StyleLabel2", -1, 250, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("-5", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("-4", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("-3", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("-2", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("-1", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("Total", "StyleLabel2Center", -1, 50, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("CL", "StyleLabel2Center", 10, 20, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("EL", "StyleLabel2Center", 10, 20, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("ECL", "StyleLabel2Center", 10, 20, null, null, false));
            spUEFARanking.Children.Add(spHead);

            Continent europe = Session.Instance.Game.kernel.String2Continent("Europe");
            int rank = 0;
            List<Country> countries = europe.associationRanking;
            foreach (Country c in countries)
            {
                rank++;
                Dictionary<int, int> qualifications = new Dictionary<int, int>();
                foreach(Qualification q in europe.continentalQualifications)
                {
                    if(q.ranking == rank)
                    {
                        if(!qualifications.ContainsKey(q.tournament.level))
                        {
                            qualifications.Add(q.tournament.level, 0);
                        }
                        qualifications[q.tournament.level] += q.qualifies;
                    }
                }
                StackPanel spLine = new StackPanel();
                spLine.Orientation = Orientation.Horizontal;
                spLine.Children.Add(ViewUtils.CreateLabel(rank.ToString(), "StyleLabel2", -1, 30, null, null, true));
                spLine.Children.Add(ViewUtils.CreateFlag(c, 20, 13));
                spLine.Children.Add(ViewUtils.CreateLabel(c.Name(), "StyleLabel2", -1, 220, null, null, true));
                spLine.Children.Add(ViewUtils.CreateLabel(c.YearAssociationCoefficient(-5).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.YearAssociationCoefficient(-4).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.YearAssociationCoefficient(-3).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.YearAssociationCoefficient(-2).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.YearAssociationCoefficient(-1).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.AssociationCoefficient.ToString("0.00"), "StyleLabel2Center", -1, 50, null, null, true));
                spLine.Children.Add(ViewUtils.CreateLabel(qualifications.ContainsKey(1) ? qualifications[1].ToString() : "0", "StyleLabel2Center", 9, 20, null, null, true));
                spLine.Children.Add(ViewUtils.CreateLabel(qualifications.ContainsKey(2) ? qualifications[2].ToString() : "0", "StyleLabel2Center", 9, 20, null, null, true));
                spLine.Children.Add(ViewUtils.CreateLabel(qualifications.ContainsKey(3) ? qualifications[3].ToString() : "0", "StyleLabel2Center", 9, 20, null, null, true));
                spUEFARanking.Children.Add(spLine);
                
            }
        }

        private void FillUEFAClubRanking()
        {
            int i = 0;
            StackPanel spHead = new StackPanel();
            spHead.Orientation = Orientation.Horizontal;
            spHead.Children.Add(ViewUtils.CreateLabel("", "StyleLabel2Center", -1, 30, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("Club", "StyleLabel2", -1, 250, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("-5", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("-4", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("-3", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("-2", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("-1", "StyleLabel2Center", -1, 35, null, null, false));
            spHead.Children.Add(ViewUtils.CreateLabel("Total", "StyleLabel2Center", -1, 50, null, null, true));
            spUEFAClubsRanking.Children.Add(spHead);

            List<Club> clubs = new List<Club>();
            Continent europe = Session.Instance.Game.kernel.String2Continent("Europe");
            foreach (Country c in europe.countries)
            {
                foreach(Tournament championship in c.Tournaments())
                {
                    if(championship.isChampionship)
                    {
                        foreach(Club club in championship.rounds[0].clubs)
                        {
                            if(club.ClubCoefficient() > 0)
                            {
                                clubs.Add(club);
                            }
                        }
                    }
                }
            }

            clubs.Sort(new ClubComparator(ClubAttribute.CONTINENTAL_COEFFICIENT));
            int rank = 0;
            foreach(Club c in clubs)
            {
                rank++;
                StackPanel spLine = new StackPanel();
                spLine.Orientation = Orientation.Horizontal;
                spLine.Children.Add(ViewUtils.CreateLabel(rank.ToString(), "StyleLabel2", -1, 30, null, null, true));
                spLine.Children.Add(ViewUtils.CreateFlag(Session.Instance.Game.kernel.LocalisationTournament(c.Championship) as Country, 20, 13));
                spLine.Children.Add(ViewUtils.CreateLabel(c.name, "StyleLabel2", -1, 220, null, null, true));
                spLine.Children.Add(ViewUtils.CreateLabel(c.ClubYearCoefficient(-5).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.ClubYearCoefficient(-4).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.ClubYearCoefficient(-3).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.ClubYearCoefficient(-2).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.ClubYearCoefficient(-1).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(c.ClubCoefficient().ToString("0.00"), "StyleLabel2Center", -1, 50, null, null, true));
                spUEFAClubsRanking.Children.Add(spLine);
            }
        }

        private void FillFifaRanking()
        {
            int i = 0;

            StackPanel spHead = new StackPanel();
            spHead.Orientation = Orientation.Horizontal;
            spHead.Children.Add(ViewUtils.CreateLabel("", "StyleLabel2Center", -1, 50, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("Nation", "StyleLabel2", -1, 315, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("Points", "StyleLabel2Center", -1, 75, null, null, true));
            spFIFARanking.Children.Add(spHead);

            foreach (NationalTeam nt in Session.Instance.Game.kernel.FifaRanking())
            {
                i++;
                StackPanel spTeam = new StackPanel();
                spTeam.Orientation = Orientation.Horizontal;
                spTeam.Children.Add(ViewUtils.CreateLabel(i.ToString(), "StyleLabel2Center", -1, 50));
                spTeam.Children.Add(ViewUtils.CreateFlag(nt.country, 40, 25));
                spTeam.Children.Add(ViewUtils.CreateLabel(nt.name, "StyleLabel2", -1, 275));
                spTeam.Children.Add(ViewUtils.CreateLabel(nt.officialFifaPoints.ToString("0.00"), "StyleLabel2Center", -1, 75));
                spFIFARanking.Children.Add(spTeam);
            }
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
