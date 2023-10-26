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
using TheManager_GUI.ViewMisc;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour MediaWindow.xaml
    /// </summary>
    public partial class MediaWindow : Window
    {

        private readonly Media _media;
        private List<int> _indexOrders;

        public MediaWindow(Media media)
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            _media = media;
            _indexOrders = new List<int>();
            imgLogo.Source = new BitmapImage(new Uri(Utils.MediaLogo(media)));
            Map();
        }

        private void Map()
        {
            
        }

        private void Map_ShapeIdentified(object sender, AxMapWinGIS._DMapEvents_ShapeIdentifiedEvent e)
        {
            if(e.shapeIndex > -1)
            {
                spJournalistInfo.Children.Clear();

                Journalist j = _media.journalists[_indexOrders[e.shapeIndex]];
                spJournalistInfo.Children.Add(ViewUtils.CreateLabel(j.ToString() + " (" + j.age + " ans)", "StyleLabel2", 12, -1));
                spJournalistInfo.Children.Add(ViewUtils.CreateLabel("Basé à " + j.baseCity.Name, "StyleLabel2", 12, -1));
                List<Match> commentedGames = j.CommentedGames;
                commentedGames.Sort(new MatchDateComparator());
                ViewMatches view = new ViewMatches(commentedGames, true, false, false, false, false, true);
                view.Full(spMatches);
            }
        }

        private void Map_ShapeHighlighted(object sender, AxMapWinGIS._DMapEvents_ShapeHighlightedEvent e)
        {
        }

        private void Map_MouseDownEvent(object sender, AxMapWinGIS._DMapEvents_MouseDownEvent e)
        {
        }

        private void btnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
