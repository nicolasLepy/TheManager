using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.ViewMisc;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Competition.xaml
    /// </summary>
    public partial class Windows_Competition : Window
    {

        private readonly Tournament _competition;
        private int _indexTour;
        private int _indexJournee;

        public Windows_Competition(Tournament competition)
        {
            InitializeComponent();
            _competition = competition;
            _indexTour = 0;
            _indexJournee = 1;
            InitWidgets();
        }

        private void FillComboBoxYear()
        {
            foreach(KeyValuePair<int,Tournament> history in _competition.previousEditions)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = "Saison " + (history.Key - 1) + "-" + history.Key;
                cbi.Selected += new RoutedEventHandler((s, e) => NewYearSelected(history));
                cbYear.Items.Add(cbi);
            }
        }

        private void NewYearSelected(KeyValuePair<int, Tournament> history)
        {
            Console.WriteLine(history.Key);
        }



        private List<Match> Journee()
        {
            List<Match> res = _competition.rounds[_indexTour].matches;
            ChampionshipRound tc = _competition.rounds[_indexTour] as ChampionshipRound;
            if(tc != null)
            {
                res = tc.GamesDay(_indexJournee);
            }
            return res;
        }

        private void Ranking()
        {
            View vue = FactoryViewRanking.CreerVue(null, _competition.rounds[_indexTour]);
            vue.Full(spRanking);
        }

        private void InitWidgets()
        {
            lbCompetition.Content = _competition.name;
            lbNomTour.Content = _competition.rounds[_indexTour].name;

            Ranking();
            Calendrier(_competition.rounds[_indexTour]);
            Map(_competition.rounds[_indexTour]);

            int nbRegles = 0;
            foreach (Rule r in _competition.rounds[_indexTour].rules)
            {
                Label l = new Label();
                l.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l.Width = 20;
                l.Content = Utils.Rule2String(r);
                spBlocClassement.Children.Add(l);
                nbRegles++;
            }


        }

        private float LongToX(float longitude, float imageWidth)
        {
            return (imageWidth / 360.0f) * (180 + longitude);
        }

        private float LatToY(float latitude, float imageHeight)
        {
            return (imageHeight / 180.0f) * (90 - latitude);
        }
        private void Map(Round t)
        {

            AxMapWinGIS.AxMap map = new AxMapWinGIS.AxMap();
            map.Width = 380;
            map.Height = 380;
            host.Child = map;
            map.Show();
            map.CreateControl();
            map.ShowZoomBar = false;
            map.ShowCoordinates = MapWinGIS.tkCoordinatesDisplay.cdmNone;
            map.CursorMode = MapWinGIS.tkCursorMode.cmNone;

            MapWinGIS.Shapefile shapeFileMap = new MapWinGIS.Shapefile();
            shapeFileMap.Open(@"D:\Projets\TheManager\TheManager_GUI\bin\Debug\gis\world\World_Countries.shp", null);
            map.AddLayer(shapeFileMap, true);
            map.ZoomToShape(0, 77);
            //map.CurrentScale = 5;
            MapWinGIS.Shape shpFrance = shapeFileMap.Shape[79];
            foreach(Club c in t.clubs)
            {
                double projX = -1;
                double projY = -1;
                map.DegreesToProj((c as CityClub).city.Position.Longitude, (c as CityClub).city.Position.Latitude, ref projX, ref projY);

                MapWinGIS.Image img = new MapWinGIS.Image();
                img.CreateNew(30, 30);
                img.Open(Utils.Logo(c));

                MapWinGIS.Shapefile sf = new MapWinGIS.Shapefile();
                sf.CreateNew("", MapWinGIS.ShpfileType.SHP_POINT);
                sf.DefaultDrawingOptions.AlignPictureByBottom = false;
                sf.DefaultDrawingOptions.PointType = MapWinGIS.tkPointSymbolType.ptSymbolPicture;
                sf.DefaultDrawingOptions.Picture = img;
                sf.DefaultDrawingOptions.PictureScaleX = Math.Round(30.0 / img.OriginalWidth,2);
                sf.DefaultDrawingOptions.PictureScaleY = Math.Round(30.0 / img.OriginalHeight,2);
                sf.CollisionMode = MapWinGIS.tkCollisionMode.AllowCollisions;

                MapWinGIS.Shape shp = new MapWinGIS.Shape();
                shp.Create(MapWinGIS.ShpfileType.SHP_POINT);
                shp.AddPoint(projX, projY);
                sf.EditAddShape(shp);

                map.AddLayer(sf, true);

            }
            map.Redraw();

        }

        private void Calendrier(Round t)
        {
            spMatchs.Children.Clear();

            List<Match> matchs = Journee();
            matchs.Sort(new MatchDateComparator());

            ViewMatches view = new ViewMatches(matchs, true, true, true, false, false, false, 17, false, null, true, true, true, 1.75f);
            view.Full(spMatchs);


        }
        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void BtnTourDroite_Click(object sender, RoutedEventArgs e)
        {
            if (_indexTour < _competition.rounds.Count - 1)
            {
                _indexTour++;
            }
            InitWidgets();
        }

        private void BtnTourGauche_Click(object sender, RoutedEventArgs e)
        {
            if (_indexTour > 0)
            {
                _indexTour--;
            }
            InitWidgets();
        }

        private void BtnJourneeGauche_Click(object sender, RoutedEventArgs e)
        {
            if (_indexJournee > 1)
            {
                _indexJournee--;
            }
            InitWidgets();
        }

        private void BtnJourneeDroite_Click(object sender, RoutedEventArgs e)
        {
            ChampionshipRound tc = _competition.rounds[_indexTour] as ChampionshipRound;
            if (tc != null)
            {
                if (_indexJournee < tc.MatchesDayNumber())
                {
                    _indexJournee++;
                }
                InitWidgets();
            }
        }

        private void BtnHistoric_Click(object sender, RoutedEventArgs e)
        {

            TournamentHistoryWindow thw = new TournamentHistoryWindow(_competition);
            thw.Show();
        }

        private void BtnMatchClick(Match m)
        {
            Windows_Match wm = new Windows_Match(m);
            wm.Show();
        }

        private void SelectedHomeRanking(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
        }

        private void SelectedRanking(object sender, RoutedEventArgs e)
        {
            Ranking();
        }

        private void SelectedAwayRanking(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
        }

        private void SelectedGoalscorers(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();

            foreach (KeyValuePair<Player, int> goalscorer in _competition.Goalscorers())
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                if(goalscorer.Key.Club != null)
                {
                    sp.Children.Add(ViewUtils.CreateLogo(goalscorer.Key.Club, 25, 25));
                }
                else
                {
                    sp.Children.Add(ViewUtils.CreateFlag(goalscorer.Key.nationality, 25, 18));
                }
                sp.Children.Add(ViewUtils.CreateLabel(goalscorer.Key.Name, "StyleLabel2", 12, 200));
                sp.Children.Add(ViewUtils.CreateLabel(goalscorer.Value.ToString(), "StyleLabel2", 12, 50));
                spRanking.Children.Add(sp);
            }
        }

        private void SelectedStatPossession(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
        }

        private void SelectedStatShot(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
        }

        private void SelectedRecords(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();

            spRanking.Children.Add(ViewUtils.CreateLabel("Plus gros score", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel(_competition.statistics.LargerScore != null ? _competition.statistics.LargerScore.home.name + " " + _competition.statistics.LargerScore.score1 + "-" + _competition.statistics.LargerScore.score2 + " " + _competition.statistics.LargerScore.away.name : "Pas encore de match joué", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel("Plus large score", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel(_competition.statistics.BiggerScore != null ? _competition.statistics.BiggerScore.home.name + " " + _competition.statistics.BiggerScore.score1 + "-" + _competition.statistics.BiggerScore.score2 + " " + _competition.statistics.BiggerScore.away.name : "Pas encore de match joué", "StyleLabel2", 12, -1));

        }

        private void SelectedPalmaresClubs(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();

            Dictionary<Club, List<int>> palmares = new Dictionary<Club, List<int>>();

            foreach (KeyValuePair<int, Tournament> arc in _competition.previousEditions)
            {
                Club winner = arc.Value.Winner();
                if (winner != null)
                {
                    if(!palmares.ContainsKey(winner))
                    {
                        palmares.Add(winner, new List<int>());
                    }
                    palmares[winner].Add(arc.Key);
                }
            }

            List<KeyValuePair<Club, List<int>>> list = palmares.ToList();
            list.Sort((pair1, pair2) => pair2.Value.Count.CompareTo(pair1.Value.Count));

            foreach(KeyValuePair<Club, List<int>> club in list)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(ViewUtils.CreateLogo(club.Key, 25, 25));
                sp.Children.Add(ViewUtils.CreateLabel(club.Key.name, "StyleLabel2", 12, 200));
                sp.Children.Add(ViewUtils.CreateLabel(club.Value.Count.ToString(), "StyleLabel2", 12, 50, null, null, true));
                sp.Children.Add(ViewUtils.CreateLabel(club.Value.ToString(), "StyleLabel2", 12, 250));
                spRanking.Children.Add(sp);
            }


        }

        private void SelectedPalmaresYears(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();

            foreach (KeyValuePair<int, Tournament> arc in _competition.previousEditions)
            {
                Club winner = arc.Value.Winner();

                Round t = arc.Value.rounds[arc.Value.rounds.Count - 1];
                //If the final round was not inactive, we can make the palmares
                if (t.matches.Count > 0)
                {
                    int year = arc.Key;
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;
                    sp.Children.Add(ViewUtils.CreateLabel(year.ToString(), "StyleLabel2", 12, 100));
                    sp.Children.Add(ViewUtils.CreateLogo(winner, 25, 25));
                    sp.Children.Add(ViewUtils.CreateLabel(winner.name, "StyleLabel2", 12, 200));
                    spRanking.Children.Add(sp);
                }
            }

        }

        private void SelectedBudget(object sender, RoutedEventArgs e)
        {
            
            spRanking.Children.Clear();
            List<Club> clubs = new List<Club>(_competition.rounds[_indexTour].clubs);
            clubs.Sort(new ClubComparator(ClubAttribute.BUDGET));
            int i = 0;
            foreach (Club c in clubs)
            {
                if(c as CityClub != null)
                {
                    i++;
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;
                    sp.Children.Add(ViewUtils.CreateLabel(i.ToString(), "StyleLabel2", 12, 30));
                    sp.Children.Add(ViewUtils.CreateLogo(c, 25, 25));
                    sp.Children.Add(ViewUtils.CreateLabel(c.name, "StyleLabel2", 12, 200));
                    sp.Children.Add(ViewUtils.CreateLabel(Utils.FormatMoney((c as CityClub).budget), "StyleLabel2", 12, 100));
                    spRanking.Children.Add(sp);
                }
            }
        }

        private void SelectedLevel(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
            List<Club> clubs = new List<Club>(_competition.rounds[_indexTour].clubs);
            clubs.Sort(new ClubComparator(ClubAttribute.LEVEL));
            int i = 0;
            foreach (Club c in clubs)
            {
                i++;
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(ViewUtils.CreateLabel(i.ToString(), "StyleLabel2", 12, 30));
                sp.Children.Add(ViewUtils.CreateLogo(c, 25, 25));
                sp.Children.Add(ViewUtils.CreateLabel(c.name, "StyleLabel2", 12, 200));
                sp.Children.Add(ViewUtils.CreateLabel(c.Level().ToString("0.0"), "StyleLabel2", 12, 50));
                spRanking.Children.Add(sp);
            }
        }

        private void SelectedPotential(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
            List<Club> clubs = new List<Club>(_competition.rounds[_indexTour].clubs);
            clubs.Sort(new ClubComparator(ClubAttribute.POTENTIEL));
            int i = 0;
            foreach (Club c in clubs)
            {
                i++;
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(ViewUtils.CreateLabel(i.ToString(), "StyleLabel2", 12, 30));
                sp.Children.Add(ViewUtils.CreateLogo(c, 25, 25));
                sp.Children.Add(ViewUtils.CreateLabel(c.name, "StyleLabel2", 12, 200));
                sp.Children.Add(ViewUtils.CreateLabel(c.Potential().ToString("0.0"), "StyleLabel2", 12, 50));
                spRanking.Children.Add(sp);
            }
        }

        private void SelectedStadium(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
            List<Club> clubs = new List<Club>(_competition.rounds[_indexTour].clubs);
            clubs.Sort(new ClubComparator(ClubAttribute.STADIUM));
            int i = 0;
            foreach(Club c in clubs)
            {
                i++;
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(ViewUtils.CreateLabel(i.ToString(), "StyleLabel2", 12, 30));
                sp.Children.Add(ViewUtils.CreateLogo(c, 25, 25));
                sp.Children.Add(ViewUtils.CreateLabel(c.name, "StyleLabel2", 12, 200));
                sp.Children.Add(ViewUtils.CreateLabel(c.stadium.name, "StyleLabel2", 12, 150));
                sp.Children.Add(ViewUtils.CreateLabel(c.stadium.capacity.ToString(), "StyleLabel2", 12, 100)) ;
                spRanking.Children.Add(sp);
            }
        }

    }
}