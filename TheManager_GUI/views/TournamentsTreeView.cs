using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using TheManager;
using static SkiaSharp.HarfBuzz.SKShaper;
using System.Windows.Input;
using System.Runtime.InteropServices;
using TheManager_GUI.Styles;

namespace TheManager_GUI.views
{
    public class TournamentsTreeViewController
    {

        private TreeView treeView;
        private Continent rootNode;

        public Func<Tournament, bool> TournamentValidator { get; set; }
        public Action<object, MouseButtonEventArgs, Tournament> OnClickTournament { get; set; }

        public string ContentStyle { get; set; }

        public TournamentsTreeViewController(TreeView host, Continent rootNode)
        {
            this.treeView = host;
            this.rootNode = rootNode;
            ContentStyle = StyleDefinition.styleTextNavigation;
            treeView.PreviewMouseWheel += treeView_PreviewMouseWheel;
        }

        ~TournamentsTreeViewController()
        {
            treeView.PreviewMouseWheel -= treeView_PreviewMouseWheel;
        }

        public void Fill()
        {
            treeView.Items.Clear();
            treeView.Items.Add(CreateNavigationContinent(rootNode));
        }

        private StackPanel CreateTreeViewItemComponent(string itemName, string imagePath)
        {
            StackPanel spNavigationItem = new StackPanel();
            spNavigationItem.Orientation = Orientation.Horizontal;
            spNavigationItem.Margin = new Thickness(0, 2, 0, 2);
            Image logoTournament = new Image();
            logoTournament.Style = Application.Current.FindResource("image") as Style;
            logoTournament.Source = new BitmapImage(new Uri(imagePath));
            logoTournament.Height = 14;
            logoTournament.Width = 21;
            logoTournament.Margin = new Thickness(0, 0, 10, 0);
            TextBlock tbTournament = new TextBlock();
            tbTournament.Style = Application.Current.FindResource(ContentStyle) as Style;
            tbTournament.Text = itemName;
            tbTournament.VerticalAlignment = VerticalAlignment.Center;

            spNavigationItem.Children.Add(logoTournament);
            spNavigationItem.Children.Add(tbTournament);

            return spNavigationItem;
        }

        private StackPanel CreateNavigationTournament(Tournament tournament)
        {
            StackPanel spTournament = CreateTreeViewItemComponent(tournament.name, Utils.LogoTournament(tournament));
            if(OnClickTournament != null)
            {
                spTournament.MouseLeftButtonUp += (sender, e) => OnClickTournament(sender, e, tournament);
            }
            return spTournament;
        }

        private TreeViewItem CreateNavigationContinent(Continent continent)
        {
            TreeViewItem treeViewItemContainer = new TreeViewItem();
            treeViewItemContainer.Margin = new Thickness(0, 2, 0, 2);

            StackPanel spTreeViewItemHeader = CreateTreeViewItemComponent(continent.Name(), Utils.Logo(continent));
            treeViewItemContainer.Header = spTreeViewItemHeader;

            foreach (Tournament t in continent.Tournaments())
            {
                if (TournamentValidator == null || TournamentValidator(t))
                {
                    treeViewItemContainer.Items.Add(CreateNavigationTournament(t));
                }
            }

            foreach (Continent subContinent in continent.continents)
            {
                treeViewItemContainer.Items.Add(CreateNavigationContinent(subContinent));
            }

            foreach (Country country in continent.countries)
            {
                int countryValidTournaments = country.Tournaments().Where(t => TournamentValidator == null || TournamentValidator(t)).Count();
                if (countryValidTournaments > 0)
                {
                    treeViewItemContainer.Items.Add(CreateNavigationCountry(country));
                }
            }

            return treeViewItemContainer;

        }



        private TreeViewItem CreateNavigationCountry(Country country)
        {
            TreeViewItem treeViewItemContainer = new TreeViewItem();
            treeViewItemContainer.Margin = new Thickness(0, 2, 0, 2);

            StackPanel spTreeViewItemHeader = CreateTreeViewItemComponent(country.Name(), Utils.Flag(country));
            treeViewItemContainer.Header = spTreeViewItemHeader;

            foreach (Tournament t in country.Tournaments())
            {
                if (TournamentValidator == null || TournamentValidator(t))
                {
                    treeViewItemContainer.Items.Add(CreateNavigationTournament(t));
                }
            }

            return treeViewItemContainer;
        }

        /** EVENTS HANDLER **/

        //For navigation
        private void treeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TreeView && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

    }
}
