using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.views;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour MediaView.xaml
    /// </summary>
    public partial class MediaView : Window
    {

        private readonly Media media;

        public MediaView(Media media)
        {
            this.media = media;
            InitializeComponent();
            InitializeMap();
        }

        private void InitializeMap()
        {
            MapView view = new MapView();
            view.OnClickMap = Click;
            List<MapClub> items = new List<MapClub>();
            foreach(Journalist j in media.journalists)
            {
                items.Add(new MapClub(j.baseCity.Position.Latitude, j.baseCity.Position.Longitude, ""));
            }
            view.Refresh(MapType.CLUB, new Dictionary<int, int>(), new Dictionary<int, string>(), media.country.ShapeNumber, items);
            view.Show(gridMap);
        }

        private void Click(int index)
        {
            SelectJournalist(media.journalists[index]);
        }

        private void SelectJournalist(Journalist journalist)
        {
            textName.Text = journalist.ToString();
            textAge.Text = String.Format("{0} {1}", journalist.age, FindResource("str_yo").ToString());
            List<Match> matchs = new List<Match>(journalist.Games);
            matchs.Sort(new MatchComparator(new List<MatchAttribute>() { MatchAttribute.DATE }));
            ViewScores view = new ViewScores(matchs, true, false, false, false, false, true);
            view.Full(panelGames);
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
