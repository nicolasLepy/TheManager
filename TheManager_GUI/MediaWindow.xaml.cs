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
            AxMapWinGIS.AxMap map = new AxMapWinGIS.AxMap();
            map.Width = 450;
            map.MouseDownEvent += Map_MouseDownEvent;
            map.Height = 600;
            host.Child = map;
            map.Show();
            map.ShapeHighlighted += Map_ShapeHighlighted;
            map.CreateControl();
            map.ShowZoomBar = false;
            map.ShowCoordinates = MapWinGIS.tkCoordinatesDisplay.cdmNone;
            map.CursorMode = MapWinGIS.tkCursorMode.cmIdentify;

            MapWinGIS.Shapefile shapeFileMap = new MapWinGIS.Shapefile();
            shapeFileMap.Open(@"D:\Projets\TheManager\TheManager_GUI\bin\Debug\gis\world\World_Countries.shp", null);
            shapeFileMap.Identifiable = false;
            map.AddLayer(shapeFileMap, true);
            map.ZoomToShape(0, 77);

            MapWinGIS.Shapefile sf = new MapWinGIS.Shapefile();
            sf.Identifiable = true;
            sf.CreateNew("", MapWinGIS.ShpfileType.SHP_POINT);
            sf.DefaultDrawingOptions.AlignPictureByBottom = false;
            sf.DefaultDrawingOptions.PointType = MapWinGIS.tkPointSymbolType.ptSymbolStandard;
            sf.CollisionMode = MapWinGIS.tkCollisionMode.AllowCollisions;

            List<City> takenCities = new List<City>();

            foreach (Journalist journalist in _media.journalists)
            {
                double projX = -1;
                double projY = -1;
                map.DegreesToProj(journalist.baseCity.Position.Longitude, journalist.baseCity.Position.Latitude, ref projX, ref projY);

                if(takenCities.Contains(journalist.baseCity))
                {
                    projY += Session.Instance.Random(3, 12) / 10.0;
                }

                MapWinGIS.Shape shp = new MapWinGIS.Shape();
                shp.Create(MapWinGIS.ShpfileType.SHP_POINT);
                shp.AddPoint(projX, projY);
                _indexOrders.Add(sf.EditAddShape(shp));
                takenCities.Add(journalist.baseCity);

            }
            int layer = map.AddLayer(sf, true);

            foreach (Journalist journalist in _media.journalists)
            {
                int handle = map.NewDrawing(MapWinGIS.tkDrawReferenceList.dlScreenReferencedList);
                double pixX = -1;
                double pixY = -1;
                map.DegreesToPixel(journalist.baseCity.Position.Longitude, journalist.baseCity.Position.Latitude, ref pixX, ref pixY);

                float maxDistance = -1;
                foreach(Match m in journalist.CommentedGames)
                {
                    float dist = Utils.Distance(m.home.stadium.city, journalist.baseCity);
                    if(dist > maxDistance)
                    {
                        maxDistance = dist;
                    }
                }
                
                map.DrawCircleEx(handle, pixX, pixY, maxDistance/2, 2883, true, 25);

            }

            map.ShapeIdentified += Map_ShapeIdentified;
            map.Redraw();
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
