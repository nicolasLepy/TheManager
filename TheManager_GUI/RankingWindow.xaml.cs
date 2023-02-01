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
            foreach(Association a in Session.Instance.Game.kernel.worldAssociation.associations)
            {
                //At least one continental club tournament
                if (a.GetAssociationClubTournament(1) != null)
                {
                    AddContinentalCountryRanking(a);
                    //AddContinentalClubRanking(c);
                }
            }
        }

        private void AddContinentalCountryRanking(Association c)
        {
            TabItem tab = new TabItem();
            tab.Header = FindResource("str_ranking").ToString() + " " + c.name + " (associations)";
            tab.Style = Application.Current.FindResource("StyleTabHeader") as Style;
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Height = 650;
            tab.Content = scrollViewer;
            StackPanel spRanking = new StackPanel();
            spRanking.Orientation = Orientation.Vertical;
            scrollViewer.Content = spRanking;
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
            for(int i = 0; i<c.AssociationTournamentsCount; i++)
            {
                spHead.Children.Add(ViewUtils.CreateLabel(c.GetAssociationClubTournament(i+1).shortName, "StyleLabel2Center", 10, 20, null, null, false));
            }
            spRanking.Children.Add(spHead);

            int rank = 0;
            List<Association> countriesAssociations = c.associationRanking;
            foreach (Association aCtr in countriesAssociations)
            {
                rank++;
                Dictionary<int, int> qualifications = new Dictionary<int, int>();
                foreach (Qualification q in c.associationQualifications)
                {
                    if (q.ranking == rank)
                    {
                        if (!qualifications.ContainsKey(q.tournament.level))
                        {
                            qualifications.Add(q.tournament.level, 0);
                        }
                        qualifications[q.tournament.level] += q.qualifies;
                    }
                }
                StackPanel spLine = new StackPanel();
                spLine.Orientation = Orientation.Horizontal;
                spLine.Children.Add(ViewUtils.CreateLabel(rank.ToString(), "StyleLabel2", -1, 30, null, null, true));
                spLine.Children.Add(ViewUtils.CreateFlag(aCtr.localization, 20, 13));
                spLine.Children.Add(ViewUtils.CreateLabel(aCtr.name, "StyleLabel2", -1, 220, null, null, true));
                spLine.Children.Add(ViewUtils.CreateLabel(aCtr.YearAssociationCoefficient(-5).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(aCtr.YearAssociationCoefficient(-4).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(aCtr.YearAssociationCoefficient(-3).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(aCtr.YearAssociationCoefficient(-2).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(aCtr.YearAssociationCoefficient(-1).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(aCtr.AssociationCoefficient.ToString("0.00"), "StyleLabel2Center", -1, 50, null, null, true));
                for (int i = 0; i < c.AssociationTournamentsCount; i++)
                {
                    spLine.Children.Add(ViewUtils.CreateLabel(qualifications.ContainsKey(i+1) ? qualifications[i + 1].ToString() : "0", "StyleLabel2Center", 9, 20, null, null, true));
                }
                spRanking.Children.Add(spLine);
            }

            tcMain.Items.Add(tab);

        }

        private void AddContinentalClubRanking(Continent c)
        {
            TabItem tab = new TabItem();
            tab.Header = FindResource("str_ranking").ToString() + " " + c.Name() + " (clubs)";
            tab.Style = Application.Current.FindResource("StyleTabHeader") as Style;
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Height = 650;
            tab.Content = scrollViewer;
            StackPanel spRanking = new StackPanel();
            spRanking.Orientation = Orientation.Vertical;
            scrollViewer.Content = spRanking;

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
            spRanking.Children.Add(spHead);

            List<Club> clubs = new List<Club>();
            foreach (Country ctr in c.countries)
            {
                foreach (Tournament championship in ctr.Tournaments())
                {
                    if (championship.isChampionship)
                    {
                        foreach (Club club in championship.rounds[0].clubs)
                        {
                            if (club.ClubCoefficient() > 0)
                            {
                                clubs.Add(club);
                            }
                        }
                    }
                }
            }

            clubs.Sort(new ClubComparator(ClubAttribute.CONTINENTAL_COEFFICIENT));
            int rank = 0;
            foreach (Club ctr in clubs)
            {
                rank++;
                StackPanel spLine = new StackPanel();
                spLine.Orientation = Orientation.Horizontal;
                spLine.Children.Add(ViewUtils.CreateLabel(rank.ToString(), "StyleLabel2", -1, 30, null, null, true));
                spLine.Children.Add(ViewUtils.CreateFlag(Session.Instance.Game.kernel.LocalisationTournament(ctr.Championship) as Country, 20, 13));
                spLine.Children.Add(ViewUtils.CreateLabel(ctr.name, "StyleLabel2", -1, 220, null, null, true));
                spLine.Children.Add(ViewUtils.CreateLabel(ctr.ClubYearCoefficient(-5).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(ctr.ClubYearCoefficient(-4).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(ctr.ClubYearCoefficient(-3).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(ctr.ClubYearCoefficient(-2).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(ctr.ClubYearCoefficient(-1).ToString("0.00"), "StyleLabel2Center", 10, 35, null, null, false));
                spLine.Children.Add(ViewUtils.CreateLabel(ctr.ClubCoefficient().ToString("0.00"), "StyleLabel2Center", -1, 50, null, null, true));
                spRanking.Children.Add(spLine);
            }

            tcMain.Items.Add(tab);

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
