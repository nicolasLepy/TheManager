using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using NetTopologySuite.Geometries;
using TheManager_GUI.Styles;

namespace TheManager_GUI.views
{

    public struct MapClub
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string ImagePath { get; set; }

        public MapClub(double lat, double lon, string imagePath)
        {
            Lat = lat;
            Lon = lon;
            ImagePath = imagePath;
        }
    }

    public enum MapType
    {
        CLUB,
        INTERNATIONAL
    }

    public class MapView
    {

        public const int ZoomEurope = -1;
        public const int ZoomAfrica = -2;
        public const int ZoomNorthAmerica = -3;
        public const int ZoomSouthAmerica = -4;
        public const int ZoomAsia = -5;
        public const int ZoomOceania = -6;
        public const int ZoomWorld = -7;

        private Dictionary<string, int> registryCache = new Dictionary<string, int>();

        private MapType mapType;
        private List<GeometryFeature> countryFeatures = new List<GeometryFeature>();
        private Dictionary<int, int> colorMap = new Dictionary<int, int>(); //From 1 to n
        private Dictionary<int, string> colorLegend = new Dictionary<int, string>();
        private List<MapClub> clubs = new List<MapClub>();
        private Mapsui.UI.Wpf.MapControl mapControl = null;
        private int zoomLevel = ZoomWorld;
        private Color BorderColor()
        {
            return new Color(155, 155, 155, 155);
        }

        private Color ColorRange(int value)
        {
            int max = int.MinValue;
            int min = 1;
            foreach(KeyValuePair<int, int> kvp in colorMap)
            {
                max = kvp.Value > max ? kvp.Value : max;
            }
            Color maxColor = new Color(8, 48, 107);
            Color minColor = new Color(215, 230, 245);
            if(value > 0)
            {
                int red = (int)((minColor.R - maxColor.R) * ((max - value) / (0.0 + max - min)) + maxColor.R);
                int green = (int)((minColor.G - maxColor.G) * ((max - value) / (0.0 + max - min)) + maxColor.G);
                int blue = (int)((minColor.B - maxColor.B) * ((max - value) / (0.0 + max - min)) + maxColor.B);
                return new Color(red, green, blue);
            }
            else
            {
                return new Color(215, 215, 215);
            }
            /*if (value == 1) return new Color(247, 251, 255);
            if (value == 2) return new Color(215, 230, 245);
            if (value == 3) return new Color(175, 209, 231);
            if (value == 4) return new Color(115, 178, 216);
            if (value == 5) return new Color(62, 142, 196);
            if (value == 6) return new Color(22, 99, 170);
            if (value == 7) return new Color(8, 48, 107);
            else return new Color(215, 215, 215);*/
        }

        private SymbolStyle CreateSvgStyle(string path)
        {
            var bitmapId = GetBitmapIdForEmbeddedResource(path);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = 0.25, SymbolOffset = new Offset(0, 0, true) };
        }

        private SymbolStyle CreatePointStyle(Color color)
        {
            return new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                SymbolScale = 0.4,
                Fill = new Brush(color),
                Line = new Pen(BorderColor(), 0.5)
            };
        }

        private ILayer CreatePointsLayer()
        {
            GenericCollectionLayer<List<IFeature>> layer = new GenericCollectionLayer<List<IFeature>>
            {
                //Style = SymbolStyles.CreatePinStyle()
                Style = null// CreateSymbolStyle("data//img//img1.png")
            };
            foreach (MapClub mapClub in clubs)
            {
                double[] point = epsg4326epsg3857(mapClub.Lon, mapClub.Lat);
                Console.WriteLine("Create club " + point[0] + ", " + point[1] + " - " + mapClub.ImagePath);
                GeometryFeature feature = new GeometryFeature() { Geometry = new NetTopologySuite.Geometries.Point(point[0], point[1]) };
                feature["Image"] = mapClub.ImagePath;
                feature.Styles.Add(CreateSvgStyle(feature["Image"].ToString()));
                layer.Features.Add(feature);
            }
            layer.DataHasChanged(); //If need to redraw in live the layer

            return layer;
        }

        private ILayer CreateCountryPointLayer(List<GeometryFeature> countryFeatures)
        {
            GenericCollectionLayer<List<IFeature>> layer = new GenericCollectionLayer<List<IFeature>>
            {
                Style = null,
            };

            foreach (GeometryFeature feature in countryFeatures)
            {
                if (feature.Geometry.Area < 1.5)
                {
                    Console.WriteLine(feature["COUNTRY"] + " : " + feature.Geometry.Area);
                    int id = int.Parse(feature["FID"].ToString());
                    int colorValue = colorMap.ContainsKey(id) ? colorMap[id] : 0;

                    NetTopologySuite.Geometries.Point centroid = feature.Geometry.Centroid;
                    double[] pointLocation = epsg4326epsg3857(centroid.X, centroid.Y);
                    GeometryFeature newFeature = new GeometryFeature() { Geometry = new NetTopologySuite.Geometries.Point(pointLocation[0], pointLocation[1]) };
                    newFeature.Styles.Add(CreatePointStyle(ColorRange(colorValue)));
                    layer.Features.Add(newFeature);
                }
            }

            return layer;
        }

        private ILayer CreateCountryLayer(IProvider countrySource)
        {
            return new Layer
            {
                Name = "Countries",
                DataSource = countrySource,
                Style = new ThemeStyle(f =>
                {
                    if (f.RenderedGeometry is NetTopologySuite.Geometries.Point)
                        return null;

                    VectorStyle style = new VectorStyle();

                    int id = int.Parse(f["FID"].ToString());
                    int colorValue = colorMap.ContainsKey(id) ? colorMap[id] : 0;
                    style.Fill = new Brush(ColorRange(colorValue));
                    style.Outline = new Pen(BorderColor(), 0.5);
                    return style;
                })
            };
        }

        private void ZoomToBox(Navigator navigator, MRect rect)
        {
            navigator.ZoomToBox(rect);
        }

        private void Zoom(Mapsui.UI.Wpf.MapControl mapControl, int id)
        {
            double[] p1 = epsg4326epsg3857(-24.806436, 66.529297);
            double[] p2 = epsg4326epsg3857(49.3777582, 27.741821);

            switch (id)
            {
                case ZoomEurope:
                    //EU
                    p1 = epsg4326epsg3857(-24.806436, 66.529297);
                    p2 = epsg4326epsg3857(49.3777582, 27.741821);
                    break;
                case ZoomAfrica:
                    //AF
                    p1 = epsg4326epsg3857(-19.945092, 38.825894);
                    p2 = epsg4326epsg3857(58.101780, -37.016712);
                    break;
                case ZoomNorthAmerica:
                    //AN
                    p1 = epsg4326epsg3857(-124.869979, 56.287948);
                    p2 = epsg4326epsg3857(-59.105865, 8.054563);
                    break;
                case ZoomSouthAmerica:
                    //AS
                    p1 = epsg4326epsg3857(-80.792732, 13.780342);
                    p2 = epsg4326epsg3857(-35.505777, -55.753188);
                    break;
                case ZoomAsia:
                    //AI
                    p1 = epsg4326epsg3857(33.681237, 53.926608);
                    p2 = epsg4326epsg3857(154.730408, -39.655427);
                    break;
                case ZoomOceania:
                    //OC
                    p1 = epsg4326epsg3857(136.097598, 18.626689);
                    p2 = epsg4326epsg3857(-131.620259, -49.908392);
                    break;
                case ZoomWorld:
                    return;
                default:
                    GeometryFeature feature = GetFeatureByFID(id);
                    Envelope envelope = feature.Geometry.EnvelopeInternal;
                    p1 = epsg4326epsg3857(envelope.MinX - 0.2, envelope.MinY - 0.2);
                    p2 = epsg4326epsg3857(envelope.MaxX + 0.2, envelope.MaxY + 0.2);
                    break;
            }
            MRect rect = new MRect(p1[0], p1[1], p2[0], p2[1]);
            mapControl.Map.Home = (n) => ZoomToBox(n, rect);
        }


        private GeometryFeature GetFeatureByFID(int fid)
        {
            foreach (GeometryFeature feature in countryFeatures)
            {
                if (int.Parse(feature["FID"].ToString()) == fid)
                {
                    return feature;
                }
            }
            return null;
        }

        private double[] epsg4326epsg3857(double baseX, double baseY)
        {
            double x = (baseX * 20037508.34) / 180;
            double y = Math.Log(Math.Tan(((90 + baseY) * Math.PI) / 360)) / (Math.PI / 180);
            y = (y * 20037508.34) / 180;
            return new double[] { x, y };
        }

        private int GetBitmapIdForEmbeddedResource(string imagePath)
        {           
            if(!registryCache.ContainsKey(imagePath))
            {
                using (FileStream fs = new FileStream(imagePath, FileMode.Open))
                {
                    var memoryStream = new MemoryStream();
                    fs.CopyTo(memoryStream);
                    registryCache[imagePath] = BitmapRegistry.Instance.Register(memoryStream);
                }
            }
            return registryCache[imagePath];
        }


        public MapView(MapType mapType, Dictionary<int, int> colorMap, Dictionary<int, string> colorLegend, int zoomLevel, List<MapClub> clubs)
        {
            this.mapType = mapType;
            this.colorMap = colorMap;
            this.colorLegend = colorLegend;
            this.zoomLevel = zoomLevel;
            this.clubs = clubs;

            mapControl = new Mapsui.UI.Wpf.MapControl();

            ShapeFile countrySource = new ShapeFile("data\\gis\\countries.shp", true) { CRS = "EPSG:4326" };
            countryFeatures = new List<GeometryFeature>();
            for (uint i = 0; i < countrySource.GetFeatureCount(); i++)
            {
                countryFeatures.Add(countrySource.GetFeature(i));
            }

            var countryProjected = new ProjectingProvider(countrySource)
            {
                CRS = "EPSG:3857"
            };

            if (this.mapType == MapType.CLUB)
            {
                mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
                mapControl.Map?.Layers.Add(CreatePointsLayer());
            }
            if (this.mapType == MapType.INTERNATIONAL)
            {
                ILayer countryLayer = CreateCountryLayer(countryProjected);
                mapControl.Map?.Layers.Add(countryLayer);
                mapControl.Map?.Layers.Add(CreateCountryPointLayer(countryFeatures));
            }
            mapControl.Map.CRS = "EPSG:3857";
        }

        public void Clear()
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Dispose();
            mapControl.Map.ClearCache();
        }

        public void Show(Panel host)
        {
            host.Children.Clear();
            Grid hostGrid = host as Grid;
            if (hostGrid != null)
            {
                ViewUtils.AddElementToGrid(hostGrid, mapControl, 0, 0, hostGrid.ColumnDefinitions.Count, hostGrid.RowDefinitions.Count);
                if(mapType == MapType.INTERNATIONAL)
                {
                    ViewUtils.AddElementToGrid(hostGrid, CreateLegend(), 0, 0);
                }
            }
            else
            {
                host.Children.Add(mapControl);
                if(mapType == MapType.INTERNATIONAL)
                {
                    host.Children.Add(CreateLegend());
                }
            }
            Zoom(mapControl, zoomLevel);
        }

        private Panel CreateLegend()
        {
            Grid legendGrid = new Grid();
            
            legendGrid.Margin = new Thickness(15);
            legendGrid.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(125, System.Windows.Media.Brushes.Gray.Color.R, System.Windows.Media.Brushes.Gray.Color.G, System.Windows.Media.Brushes.Gray.Color.B));
            legendGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            legendGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });
            foreach(KeyValuePair<int, string> kvp in colorLegend)
            {
                legendGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                Color color = ColorRange(kvp.Key);
                Rectangle rectangle = new Rectangle();
                rectangle.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)color.R, (byte)color.G, (byte)color.B));
                rectangle.Stroke = System.Windows.Media.Brushes.Black;
                rectangle.StrokeThickness = 1;
                rectangle.Width = 20;
                rectangle.Height = 10;
                TextBlock textLegend = ViewUtils.CreateTextBlock(kvp.Value, StyleDefinition.styleTextPlain);
                textLegend.VerticalAlignment = VerticalAlignment.Center;
                ViewUtils.AddElementToGrid(legendGrid, textLegend, legendGrid.RowDefinitions.Count-1, 1);
                ViewUtils.AddElementToGrid(legendGrid, rectangle, legendGrid.RowDefinitions.Count-1, 0);
            }
            return legendGrid;
        }
    }
}
