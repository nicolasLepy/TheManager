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

        private Tournament _competition;
        private Tournament _baseTournament;
        private int _indexTour;
        private int _indexJournee;

        public Windows_Competition(Tournament tournament)
        {
            InitializeComponent();
            _competition = tournament;
            _baseTournament = tournament;
            _indexTour = 0;
            _indexJournee = 1;
            InitWidgets(true);
            FillComboBoxYear();

            imgBtnJourneeGauche.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\left.png"));
            imgBtnJourneeDroite.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\right.png"));
            imgBtnTourGauche.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\left.png"));
            imgBtnTourDroite.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\right.png"));

            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));

        }

        private void FillComboBoxYear()
        {
            ComboBoxItem cbi = new ComboBoxItem();
            cbi.Content = "Saison actuelle";
            cbi.Selected += new RoutedEventHandler((s, e) => NewYearSelected(new KeyValuePair<int, Tournament>(-1, _baseTournament)));
            cbYear.Items.Add(cbi);

            foreach (KeyValuePair<int,Tournament> history in _competition.previousEditions)
            {
                cbi = new ComboBoxItem();
                cbi.Content = "Saison " + (history.Key - 1) + "-" + history.Key;
                cbi.Selected += new RoutedEventHandler((s, e) => NewYearSelected(history));
                cbYear.Items.Add(cbi);
            }
        }

        private void NewYearSelected(KeyValuePair<int, Tournament> history)
        {
            Console.WriteLine(history.Key);
            _competition = history.Value;
            _indexTour = 0;
            _indexJournee = 1;
            InitWidgets(true);
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

        private void Ranking(RankingType rankingType)
        {
            View vue = FactoryViewRanking.CreateView(_competition.rounds[_indexTour], 1, false, null, false, rankingType);
            vue.Full(spRanking);
        }

        private void InitWidgets(bool initMap)
        {
            lbCompetition.Content = _competition.name;
            lbNomTour.Content = _competition.rounds[_indexTour].name;
            Ranking(RankingType.General);
            Calendrier(_competition.rounds[_indexTour]);
            if(initMap)
            {
                Map(_competition.rounds[_indexTour]);
            }

            int rulesCount = 0;
            foreach (Rule r in _competition.rounds[_indexTour].rules)
            {
                Label l = new Label();
                l.Style = Application.Current.FindResource("StyleLabel2") as Style;
                l.Content = Utils.Rule2String(r);
                l.FontSize -= 2;
                spRanking.Children.Add(l);
                rulesCount++;
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
            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(t.Tournament);
            double logoSize = 30.0;
            if(localisation as Country != null)
            {
                map.ZoomToShape(0, (localisation as Country).ShapeNumber);
            }
            else
            {
                if(localisation.Name() == "Europe")
                {
                    map.ZoomToShape(0, 68 /*12 101*/);
                    map.CurrentZoom = 4;
                    logoSize = 15.0;
                }
            }
            //map.CurrentScale = 5;
            MapWinGIS.Shape shpFrance = shapeFileMap.Shape[79];
            foreach(Club c in t.clubs)
            {
                CityClub cc = c as CityClub;
                if(cc != null)
                {
                    double projX = -1;
                    double projY = -1;
                    map.DegreesToProj(cc.city.Position.Longitude, cc.city.Position.Latitude, ref projX, ref projY);

                    MapWinGIS.Image img = new MapWinGIS.Image();
                    img.Open(Utils.Logo(c));

                    MapWinGIS.Shapefile sf = new MapWinGIS.Shapefile();
                    sf.CreateNew("", MapWinGIS.ShpfileType.SHP_POINT);
                    sf.DefaultDrawingOptions.AlignPictureByBottom = false;
                    sf.DefaultDrawingOptions.PointType = MapWinGIS.tkPointSymbolType.ptSymbolPicture;
                    sf.DefaultDrawingOptions.Picture = img;
                    sf.DefaultDrawingOptions.PictureScaleX = Math.Round(logoSize / img.OriginalWidth, 2);
                    sf.DefaultDrawingOptions.PictureScaleY = Math.Round(logoSize / img.OriginalHeight, 2);
                    sf.CollisionMode = MapWinGIS.tkCollisionMode.AllowCollisions;

                    MapWinGIS.Shape shp = new MapWinGIS.Shape();
                    shp.Create(MapWinGIS.ShpfileType.SHP_POINT);
                    shp.AddPoint(projX, projY);
                    sf.EditAddShape(shp);

                    map.AddLayer(sf, true);
                }
            }
            map.Redraw();
        }

        private void Calendrier(Round t)
        {
            spMatchs.Children.Clear();

            List<Match> matchs = Journee();
            matchs.Sort(new MatchDateComparator());

            ViewMatches view = new ViewMatches(matchs, true, true, true, false, false, false, 17, false, null, true, true, true, 1.5f);
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
            InitWidgets(true);
        }

        private void BtnTourGauche_Click(object sender, RoutedEventArgs e)
        {
            if (_indexTour > 0)
            {
                _indexTour--;
            }
            InitWidgets(true);
        }

        private void BtnJourneeGauche_Click(object sender, RoutedEventArgs e)
        {
            if (_indexJournee > 1)
            {
                _indexJournee--;
            }
            InitWidgets(false);
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
                InitWidgets(false);
            }
        }

        private void SelectedHomeRanking(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
            Ranking(RankingType.Home);
        }

        private void SelectedRanking(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
            Ranking(RankingType.General);
        }

        private void SelectedAwayRanking(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();
            Ranking(RankingType.Away);
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

            Dictionary<Club, float> possessions = new Dictionary<Club, float>();
            Dictionary<Club, int> playedGames = new Dictionary<Club, int>();
            foreach (Round r in _competition.rounds)
            {
                foreach(Match m in r.matches)
                {
                    if(!possessions.ContainsKey(m.home))
                    {
                        possessions.Add(m.home, 0);
                        playedGames.Add(m.home, 0);
                    }
                    if (!possessions.ContainsKey(m.away))
                    {
                        possessions.Add(m.away, 0);
                        playedGames.Add(m.away, 0);
                    }
                    possessions[m.home] += m.statistics.HomePossession;
                    possessions[m.away] += m.statistics.AwayPossession;
                    playedGames[m.home]++;
                    playedGames[m.away]++;
                }
            }
            List<KeyValuePair<Club, float>> list = possessions.ToList();
            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            foreach(KeyValuePair<Club, float> kvp in list)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(ViewUtils.CreateLogo(kvp.Key, 25, 25));
                sp.Children.Add(ViewUtils.CreateLabel(kvp.Key.name, "StyleLabel2", 12, 250));
                sp.Children.Add(ViewUtils.CreateLabel(kvp.Value.ToString(), "StyleLabel2", 12, 100));
                sp.Children.Add(ViewUtils.CreateLabel(playedGames[kvp.Key].ToString(), "StyleLabel2", 12, 100));
                spRanking.Children.Add(sp);

            }

        }

        private void SelectedStatShot(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();

            Dictionary<Club, float> tirs = new Dictionary<Club, float>();
            Dictionary<Club, int> playedGames = new Dictionary<Club, int>();
            foreach (Round r in _competition.rounds)
            {
                foreach (Match m in r.matches)
                {
                    if (!tirs.ContainsKey(m.home))
                    {
                        tirs.Add(m.home, 0);
                        playedGames.Add(m.home, 0);
                    }
                    if (!tirs.ContainsKey(m.away))
                    {
                        tirs.Add(m.away, 0);
                        playedGames.Add(m.away, 0);
                    }
                    tirs[m.home] += m.statistics.HomeShoots;
                    tirs[m.away] += m.statistics.AwayShoots;
                    playedGames[m.home]++;
                    playedGames[m.away]++;
                }
            }
            List<KeyValuePair<Club, float>> list = tirs.ToList();
            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            foreach (KeyValuePair<Club, float> kvp in list)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(ViewUtils.CreateLogo(kvp.Key, 25, 25));
                sp.Children.Add(ViewUtils.CreateLabel(kvp.Key.name, "StyleLabel2", 12, 250));
                sp.Children.Add(ViewUtils.CreateLabel(kvp.Value.ToString(), "StyleLabel2", 12, 100));
                sp.Children.Add(ViewUtils.CreateLabel((kvp.Value/playedGames[kvp.Key]).ToString("0.00") + "/m", "StyleLabel2", 12, 100));
                sp.Children.Add(ViewUtils.CreateLabel(playedGames[kvp.Key].ToString(), "StyleLabel2", 12, 100));
                spRanking.Children.Add(sp);

            }
        }

        private void SelectedRecords(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();

            spRanking.Children.Add(ViewUtils.CreateLabel("Plus gros score", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel(_competition.statistics.LargerScore != null ? _competition.statistics.LargerScore.home.name + " " + _competition.statistics.LargerScore.score1 + "-" + _competition.statistics.LargerScore.score2 + " " + _competition.statistics.LargerScore.away.name : "Pas encore de match joué", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel("Plus large score", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel(_competition.statistics.BiggerScore != null ? _competition.statistics.BiggerScore.home.name + " " + _competition.statistics.BiggerScore.score1 + "-" + _competition.statistics.BiggerScore.score2 + " " + _competition.statistics.BiggerScore.away.name : "Pas encore de match joué", "StyleLabel2", 12, -1));

            KeyValuePair<int, KeyValuePair<Club, int>> bestAttack = new KeyValuePair<int, KeyValuePair<Club, int>>(-1, new KeyValuePair<Club, int>(null, -1));
            KeyValuePair<int, KeyValuePair<Club, int>> bestDefense = new KeyValuePair<int, KeyValuePair<Club, int>>(-1, new KeyValuePair<Club, int>(null, -1));
            KeyValuePair<int, KeyValuePair<Club, int>> worstAttack = new KeyValuePair<int, KeyValuePair<Club, int>>(-1, new KeyValuePair<Club, int>(null, -1));
            KeyValuePair<int, KeyValuePair<Club, int>> worstDefense = new KeyValuePair<int, KeyValuePair<Club, int>>(-1, new KeyValuePair<Club, int>(null, -1));
            foreach (KeyValuePair<int, Tournament> t in _baseTournament.previousEditions)
            {
                if(t.Value.isChampionship)
                {
                    Round r = t.Value.rounds[0];
                    foreach(Club c in r.clubs)
                    {
                        int goalsFor = r.GoalsFor(c);
                        int goalsAgainst = r.GoalsAgainst(c);
                        if (goalsFor > bestAttack.Value.Value || bestAttack.Key == -1)
                        {
                            bestAttack = new KeyValuePair<int, KeyValuePair<Club, int>>(t.Key, new KeyValuePair<Club, int>(c, goalsFor));
                        }
                        if (goalsFor < worstAttack.Value.Value || worstAttack.Key == -1)
                        {
                            worstAttack = new KeyValuePair<int, KeyValuePair<Club, int>>(t.Key, new KeyValuePair<Club, int>(c, goalsFor));
                        }
                        if (goalsAgainst < bestDefense.Value.Value || bestDefense.Key == -1)
                        {
                            bestDefense = new KeyValuePair<int, KeyValuePair<Club, int>>(t.Key, new KeyValuePair<Club, int>(c, goalsAgainst));
                        }
                        if (goalsAgainst > worstDefense.Value.Value || worstDefense.Key == -1)
                        {
                            worstDefense = new KeyValuePair<int, KeyValuePair<Club, int>>(t.Key, new KeyValuePair<Club, int>(c, goalsAgainst));
                        }
                    }
                }
                else
                {

                }
            }

            spRanking.Children.Add(ViewUtils.CreateLabel("Meilleure attaque", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel(bestAttack.Key == -1 ? "Pas encore de match joué" : bestAttack.Value.Key + " (" + bestAttack.Value.Value + " buts, " + bestAttack.Key + ")", "StyleLabel2", 12, -1));

            spRanking.Children.Add(ViewUtils.CreateLabel("Meilleure défense", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel(bestDefense.Key == -1 ? "Pas encore de match joué" : bestDefense.Value.Key + " (" + bestDefense.Value.Value + " buts, " + bestDefense.Key + ")", "StyleLabel2", 12, -1));

            spRanking.Children.Add(ViewUtils.CreateLabel("Pire attaque", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel(worstAttack.Key == -1 ? "Pas encore de match joué" : worstAttack.Value.Key + " (" + worstAttack.Value.Value + " buts, " + worstAttack.Key + ")", "StyleLabel2", 12, -1));

            spRanking.Children.Add(ViewUtils.CreateLabel("Pire défense", "StyleLabel2", 12, -1));
            spRanking.Children.Add(ViewUtils.CreateLabel(worstDefense.Key == -1 ? "Pas encore de match joué" : worstDefense.Value.Key + " (" + worstDefense.Value.Value + " buts, " + worstDefense.Key + ")", "StyleLabel2", 12, -1));


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
                string yearList = "";
                foreach(float years in club.Value)
                {
                    yearList += years + ", ";
                }
                //yearList = yearList.Substring(yearList.Length - 2);
                sp.Children.Add(ViewUtils.CreateLabel(yearList, "StyleLabel2", 12, 250));
                spRanking.Children.Add(sp);
            }


        }

        private void SelectedStatsYears(object sender, RoutedEventArgs e)
        {
            spRanking.Children.Clear();

            StackPanel spTitle = new StackPanel();
            spTitle.Orientation = Orientation.Horizontal;
            spTitle.Children.Add(ViewUtils.CreateLabel("Année", "StyleLabel2", 12, 150));
            spTitle.Children.Add(ViewUtils.CreateLabel("Buts/Match", "StyleLabel2", 12, 80));
            spTitle.Children.Add(ViewUtils.CreateLabel("Buts", "StyleLabel2", 12, 80));
            spTitle.Children.Add(ViewUtils.CreateLabel("CJ/Match", "StyleLabel2", 12, 80));
            spTitle.Children.Add(ViewUtils.CreateLabel("CR/Match", "StyleLabel2", 12, 80));
            spRanking.Children.Add(spTitle);


            foreach (KeyValuePair<int, Tournament> t in _baseTournament.previousEditions)
            {
                int gameCount = 0;
                int goalCount = 0;
                int yellowCards = 0;
                int redCards = 0;

                foreach(Round r in t.Value.rounds)
                {
                    foreach(Match m in r.matches)
                    {
                        gameCount++;
                        goalCount += m.score1 + m.score2;
                        yellowCards += m.YellowCards;
                        redCards += m.RedCards;
                    }
                }

                StackPanel spYear = new StackPanel();
                spYear.Orientation = Orientation.Horizontal;
                spYear.Children.Add(ViewUtils.CreateLabel(t.Key.ToString(), "StyleLabel2", 12, 150));
                spYear.Children.Add(ViewUtils.CreateLabel((goalCount/(gameCount+0.0)).ToString("0.00"), "StyleLabel2", 12, 80));
                spYear.Children.Add(ViewUtils.CreateLabel(goalCount.ToString(), "StyleLabel2", 12, 80));
                spYear.Children.Add(ViewUtils.CreateLabel((yellowCards / (gameCount + 0.0)).ToString("0.00"), "StyleLabel2", 12, 80));
                spYear.Children.Add(ViewUtils.CreateLabel((redCards / (gameCount + 0.0)).ToString("0.00"), "StyleLabel2", 12, 80));
                spRanking.Children.Add(spYear);

            }


        }


        private void SelectedSeasonsClubs(object sender, RoutedEventArgs e)
        {
            Dictionary<Club, int> clubs = new Dictionary<Club, int>();
            spRanking.Children.Clear();
            foreach (KeyValuePair<int, Tournament> t in _competition.previousEditions)
            {
                if(t.Value.isChampionship)
                {
                    foreach(Club c in t.Value.rounds[0].clubs)
                    {
                        if(!clubs.ContainsKey(c))
                        {
                            clubs.Add(c, 0);
                        }
                        clubs[c]++;
                    }
                }
                else
                {
                    List<Club> clubsList = new List<Club>();
                    foreach(Round r in t.Value.rounds)
                    {
                        foreach (Club c in r.clubs)
                        {
                            if(!clubsList.Contains(c))
                            {
                                clubsList.Add(c);
                            }
                        }
                    }
                    foreach(Club c in clubsList)
                    {
                        if(!clubs.ContainsKey(c))
                        {
                            clubs.Add(c, 0);
                        }
                        clubs[c]++;
                    }
                }
            }
            List<KeyValuePair<Club, int>> list = clubs.ToList();
            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            foreach(KeyValuePair<Club, int> club in list)
            {
                StackPanel spClub = new StackPanel();
                spClub.Orientation = Orientation.Horizontal;
                spClub.Children.Add(ViewUtils.CreateLogo(club.Key, 25, 25));
                spClub.Children.Add(ViewUtils.CreateLabel(club.Key.name, "StyleLabel2", 12, 200));
                spClub.Children.Add(ViewUtils.CreateLabel(club.Value.ToString(), "StyleLabel2", 12, 50, null, null, true));
                spRanking.Children.Add(spClub);
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
            clubs.Sort(new ClubComparator(ClubAttribute.BUDGET, false));
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
            clubs.Sort(new ClubComparator(ClubAttribute.LEVEL, false));
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
            clubs.Sort(new ClubComparator(ClubAttribute.POTENTIEL, false));
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
            clubs.Sort(new ClubComparator(ClubAttribute.STADIUM, false));
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