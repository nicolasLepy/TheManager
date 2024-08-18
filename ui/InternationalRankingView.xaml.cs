using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tm;
using TheManager_GUI.controls;
using TheManager_GUI.Styles;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour InternationalRankingView.xaml
    /// </summary>
    public partial class InternationalRankingView : Window
    {

        List<Button> navButtons;

        public InternationalRankingView()
        {
            navButtons = new List<Button>();
            InitializeComponent();
        }

        private void buttonFifa_Click(object sender, RoutedEventArgs e)
        {

            buttonFifa.Background = FindResource(StyleDefinition.solidColorBrushColorButtonOver) as Brush;
            buttonAssociations.Background = null;
            buttonClubs.Background = null;

            associationsPanel.Children.Clear();
            navButtons = new List<Button>();
            foreach (Continent c in Session.Instance.Game.kernel.world.GetAllContinents())
            {
                Button continentClick = ViewUtils.CreateButton(c.Name(), StyleDefinition.styleButtonMenuTitle, 15);
                continentClick.Click += (object cs, RoutedEventArgs ce) => CreateFIFARanking(continentClick, Session.Instance.Game.kernel.world == c ? null : c);
                associationsPanel.Children.Add(continentClick);
                navButtons.Add(continentClick);
            }
        }

        private void buttonAssociations_Click(object sender, RoutedEventArgs e)
        {
            buttonAssociations.Background = FindResource(StyleDefinition.solidColorBrushColorButtonOver) as Brush;
            buttonFifa.Background = null;
            buttonClubs.Background = null;

            associationsPanel.Children.Clear();
            navButtons = new List<Button>();
            foreach (Association a in Session.Instance.Game.kernel.worldAssociation.divisions)
            {
                if(a.divisions.Count > 0)
                {
                    Button continentClick = ViewUtils.CreateButton(a.name, StyleDefinition.styleButtonMenuTitle, 15);
                    continentClick.Click += (object cs, RoutedEventArgs ce) => CreateContinentalCountryRanking(continentClick, a);
                    associationsPanel.Children.Add(continentClick);
                    navButtons.Add(continentClick);
                }
            }
        }

        private void buttonClubs_Click(object sender, RoutedEventArgs e)
        {
            buttonClubs.Background = FindResource(StyleDefinition.solidColorBrushColorButtonOver) as Brush;
            buttonFifa.Background = null;
            buttonAssociations.Background = null;

            associationsPanel.Children.Clear();
            navButtons = new List<Button>();
            foreach (Association a in Session.Instance.Game.kernel.GetAllAssociations())
            {
                if(a.GetContinentalClubTournament(1) != null)
                {
                    Button continentClick = ViewUtils.CreateButton(a.name, StyleDefinition.styleButtonMenuTitle, 15);
                    continentClick.Click += (object cs, RoutedEventArgs ce) => CreateContinentalClubRanking(continentClick, a);
                    associationsPanel.Children.Add(continentClick);
                    navButtons.Add(continentClick);
                }
            }
        }

        private void HighlightButton(Button button)
        {
            foreach (Button b in navButtons)
            {
                if (b == button)
                {
                    b.Background = FindResource(StyleDefinition.solidColorBrushColorButtonOver) as Brush;
                }
                else
                {
                    b.Background = null;
                }
            }
        }

        private void CreateContinentalClubRanking(Button sender, Association association)
        {
            HighlightButton(sender);
            List<Club> clubs = new List<Club>();
            foreach (Association a in association.divisions)
            {
                Country ctr = a.localisation as Country;
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

            List<ControlInternationalRankingItem> items = new List<ControlInternationalRankingItem>();
            int rank = 0;
            foreach (Club ctr in clubs)
            {
                rank++;
                List<double> oldCoeffs = new List<double>() { ctr.ClubYearCoefficient(-5), ctr.ClubYearCoefficient(-4), ctr.ClubYearCoefficient(-3), ctr.ClubYearCoefficient(-2), ctr.ClubYearCoefficient(-1) };
                items.Add(new ControlInternationalRankingItem(ctr.name, ViewUtils.CreateFlag(Session.Instance.Game.kernel.LocalisationTournament(ctr.Championship) as Country, 27, 20), rank, 0, oldCoeffs, ctr.ClubCoefficient(), new List<int>()));
            }

            ControlInternationalRanking view = new ControlInternationalRanking(items, new List<string>() { "-5", "-4", "-3", "-2", "-1" }, new List<string>());

            rankingPanel.Child = view;
        }

        private void CreateFIFARanking(Button sender, Continent continent)
        {
            HighlightButton(sender);
            List<ControlInternationalRankingItem> items = new List<ControlInternationalRankingItem>();
            int i = 0;
            foreach (NationalTeam nt in Session.Instance.Game.kernel.FifaRanking())
            {
                i++;
                if (continent == null || nt.Country().Continent == continent)
                {
                    items.Add(new ControlInternationalRankingItem(nt.name, ViewUtils.CreateFlag(nt.country, 27, 20), i, 0, new List<double>(), nt.officialFifaPoints, new List<int>()));
                }
            }
            ControlInternationalRanking view = new ControlInternationalRanking(items, new List<string>(), new List<string>());

            rankingPanel.Child = view;
        }

        private void CreateContinentalCountryRanking(Button sender, Association association)
        {
            HighlightButton(sender);
            List<string> continentalTournamentsNames = new List<string>();
            for (int j = 0; j < association.ContinentalTournamentsCount; j++)
            {
                continentalTournamentsNames.Add(association.GetContinentalClubTournament(j + 1).shortName);
            }

            List<ControlInternationalRankingItem> items = new List<ControlInternationalRankingItem>();
            int rank = 0;
            List<Association> countries = association.associationRanking;
            foreach (Association a in countries)
            {
                Country ctr = a.localisation as Country;
                rank++;
                int[] slots = new int[association.ContinentalTournamentsCount];
                Dictionary<int, int> qualifications = new Dictionary<int, int>();
                foreach (Qualification q in association.continentalQualifications)
                {
                    if (q.ranking == rank)
                    {
                        slots[q.tournament.level-1] += q.qualifies;
                    }
                }
                List<double> oldCoeffs = new List<double>() { a.YearAssociationCoefficient(-5), a.YearAssociationCoefficient(-4), a.YearAssociationCoefficient(-3), a.YearAssociationCoefficient(-2), a.YearAssociationCoefficient(-1) };
                items.Add(new ControlInternationalRankingItem(ctr.Name(), ViewUtils.CreateFlag(ctr, 27, 20), rank, 0, oldCoeffs, a.AssociationCoefficient, slots.ToList()));
            }

            ControlInternationalRanking view = new ControlInternationalRanking(items, new List<string>() { "-5", "-4", "-3", "-2", "-1"}, continentalTournamentsNames);

            rankingPanel.Child = view;
        }

        /* EVENTS HANDLER */

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int mParam, int lParam);

        private void spControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void spControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
