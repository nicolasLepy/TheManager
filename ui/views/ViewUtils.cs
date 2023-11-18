using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using TheManager;
using TheManager.Comparators;
using Application = System.Windows.Application;
using System.Windows.Annotations;
using TheManager_GUI.Styles;
using System.Windows.Controls.Primitives;

namespace TheManager_GUI
{
    public static class ViewUtils
    {

        public static DataTemplate CreateImageTemplate(string bindingName)
        {
            StringReader stringReader = new StringReader(
            @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""> 
            <Image Source=""{Binding " + bindingName + @"}""/> 
        </DataTemplate>");
            XmlReader xmlReader = XmlReader.Create(stringReader);
            return XamlReader.Load(xmlReader) as DataTemplate;
        }


        /// <summary>
        /// Create a chart showing data every years
        /// </summary>
        /// <param name="host">StackPanel host</param>
        /// <param name="years">List of years</param>
        /// <param name="title">Chart title</param>
        /// <param name="values">Chart values</param>
        /// <param name="isMoney">Data represents money</param>
        /// <param name="axisYtitle">Title of Y axis</param>
        /// <param name="minValue">Min value of Y axis</param>
        /// <param name="maxValue">Max value of Y axis</param>
        /// <param name="axisXtitle">Title of X axis</param>
        /// <param name="Yformatter">How to format Y axis values</param>
        /// <param name="sizeMultiplier">Size multiplier of the chart</param>
        /// <returns></returns>
        public static CartesianChart CreateYearChart(StackPanel host, string[] years, string title, IChartValues values, bool isMoney, string axisYtitle, double minValue, double maxValue, string axisXtitle, Func<double, string> Yformatter, double sizeMultiplier = 1.0)
        {

            Label labelTitle = ViewUtils.CreateLabel(title, "StyleLabel2Center", 18, -1);

            host.Children.Add(labelTitle);

            SeriesCollection averageClubLevelInGameCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = title,
                    Values = values,
                }
            };

            CartesianChart cc = new CartesianChart();
            cc.Width = 800 * sizeMultiplier;
            cc.Height = 375 * sizeMultiplier;

            cc.Series = averageClubLevelInGameCollection;

            Axis axisY = new Axis();
            axisY.Title = axisYtitle;
            axisY.MinValue = minValue;
            axisY.MaxValue = maxValue;

            if (isMoney)
            {
                axisY.LabelFormatter = Yformatter;
            }


            Axis axisX = new Axis();
            axisX.Title = axisXtitle;
            axisX.Labels = years;

            cc.AxisY.Add(axisY);
            cc.AxisX.Add(axisX);
            host.Children.Add(cc);

            return cc;

        }


        public delegate void OpenWindowOnButtonClick<T>(T element);

        /// <summary>
        /// Create a WPF label object
        /// </summary>
        /// <param name="content">Text of the label</param>
        /// <param name="style">Style of the label</param>
        /// <param name="fontSize">Font size (-1 to let style default font size)</param>
        /// <param name="width">Width of the label box (-1 to define no width)</param>
        /// <param name="color">Color of the label</param>
        /// <returns></returns>
        public static TextBlock CreateTextBlock(string content, string styleName, double fontSize = -1, double width = -1, Brush color = null, Brush backgroundColor = null, bool bold = false, bool highlightOnMouseHover = false)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = content;
            Style style = Application.Current.FindResource(styleName) as Style;
            textBlock.Style = style;

            if (fontSize > -1)
            {
                textBlock.FontSize = fontSize;
            }
            if (width > -1)
            {
                textBlock.Width = width;
            }
            if (color != null)
            {
                textBlock.Foreground = color;
            }
            if (backgroundColor != null)
            {
                textBlock.Background = backgroundColor;
            }

            if (bold)
            {
                textBlock.FontWeight = FontWeights.Bold;
                // textBlock.FontFamily = App.Current.TryFindResource("BoldFont") as FontFamily;
            }

            //For layout debug
            //textBlock.MouseEnter += TextBlock_MouseEnter;
            //textBlock.MouseLeave += TextBlock_MouseLeave;
            if (highlightOnMouseHover)
            {
                Brush baseBrush = textBlock.Background;
                textBlock.MouseEnter += (sender, e) => TextBlock_ChangeColor(sender, e, Application.Current.FindResource(StyleDefinition.solidColorBrushColorButtonOver) as Brush);
                textBlock.MouseLeave += (sender, e) => TextBlock_ChangeColor(sender, e, baseBrush);
            }

            return textBlock;
        }

        private static void TextBlock_ChangeColor(object sender, MouseEventArgs e, Brush color)
        {
            (sender as TextBlock).Background = color;
        }

        /// <summary>
        /// Create a WPF label object
        /// </summary>
        /// <param name="content">Text of the label</param>
        /// <param name="style">Style of the label</param>
        /// <param name="fontSize">Font size (-1 to let style default font size)</param>
        /// <param name="width">Width of the label box (-1 to define no width)</param>
        /// <param name="color">Color of the label</param>
        /// <returns></returns>
        public static Label CreateLabel(string content, string styleName, double fontSize, double width, Brush color = null, Brush backgroundColor = null, bool bold = false)
        {
            Label label = new Label();
            label.Content = content;
            Style style = Application.Current.FindResource(styleName) as Style;
            label.Style = style;

            if (fontSize > -1)
            {
                label.FontSize = fontSize;
            }
            if(width > -1)
            {
                label.Width = width;
            }
            if (color != null)
            {
                label.Foreground = color;
            }
            if(backgroundColor != null)
            {
                label.Background = backgroundColor;
            }

            if(bold)
            {
                label.FontFamily = App.Current.TryFindResource("BoldFont") as FontFamily;
                //label.FontWeight = FontWeights.Bold;
            }
 
            return label;
        }

        public static Label CreateLabelOpenWindow<T>(T t, OpenWindowOnButtonClick<T> onClick, string content, string style, double fontSize, double width, Brush color = null)
        {
            Label res = CreateLabel(content, style, fontSize, width, color);
            res.MouseLeftButtonUp += new MouseButtonEventHandler((s, e) => onClick(t));
            return res;
        }

        public static TextBlock CreateTextBlockOpenWindow<T>(T t, OpenWindowOnButtonClick<T> onClick, string content, string style, double fontSize, double width, Brush color = null, bool highlightOnMouseHover = false)
        {
            TextBlock tbBlock = CreateTextBlock(content, style, fontSize, width, color, null, false, highlightOnMouseHover);
            tbBlock.MouseLeftButtonUp += new MouseButtonEventHandler((s, e) => onClick(t));
            return tbBlock;
        }

        /// <summary>
        /// Create a WPF toogle button object
        /// </summary>
        /// <returns></returns>
        public static ToggleButton CreateToggleButton(string content, string styleName, int margin = -1)
        {
            ToggleButton button = new ToggleButton();
            button.Content = content;
            Style style = Application.Current.FindResource(styleName) as Style;
            button.Style = style;

            if (margin > -1)
            {
                button.Margin = new Thickness(margin);
            }

            return button;
        }

        /// <summary>
        /// Create a WPF button object
        /// </summary>
        /// <returns></returns>
        public static Button CreateButton(string content, string styleName, int margin = -1)
        {
            Button button = new Button();
            button.Content = content;
            Style style = Application.Current.FindResource(styleName) as Style;
            button.Style = style;

            if(margin > -1)
            {
                button.Margin = new Thickness(margin);
            }

            return button;
        }

        public static ProgressBar CreateProgressBar(float value, float minimum = 0, float maximum = 100, float width = 60, float height = 10)
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.Minimum = minimum;
            progressBar.Maximum = maximum;
            progressBar.Value = value;
            progressBar.Width = width;
            progressBar.Height = height;
            return progressBar;

        }

        public static Image CreateMediaLogo(Media media, double width, double height)
        {
            Image sprite = new Image();
            sprite.Source = new BitmapImage(new Uri(Utils.MediaLogo(media), UriKind.RelativeOrAbsolute));
            if(width > 0)
            {
                sprite.Width = width;
            }
            if(height > 0)
            {
                sprite.Height = height;
            }
            return sprite;
        }

        public static Image CreateImage(string path, double width, double height)
        {
            Image sprite = new Image();
            sprite.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(path, UriKind.RelativeOrAbsolute));
            if(width != -1)
            {
                sprite.Width = width;
            }
            if(height != -1)
            {
                sprite.Height = height;
            }
            return sprite;

        }

        public static Dictionary<string, BitmapImage> cacheImage = new Dictionary<string, BitmapImage>();

        public static BitmapImage LoadBitmapImageWithCache(Uri uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = uri;
            image.EndInit();
            return image;
        }

        public static Image CreateImage(Uri uri, double width, double height)
        {
            if(!cacheImage.ContainsKey(uri.AbsolutePath))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = uri;
                image.EndInit();
                cacheImage[uri.AbsolutePath] = image;
            }
            Image sprite = new Image();
            sprite.Source = cacheImage[uri.AbsolutePath];
            if(width > -1)
            {
                sprite.Width = width;
            }
            if(height > -1)
            {
                sprite.Height = height;
            }
            sprite.Style = Application.Current.FindResource("image") as Style;
            return sprite;
        }

        public static Image CreateFlag(Country country, double width, double height)
        {
            return CreateImage(new Uri(Utils.Flag(country), UriKind.RelativeOrAbsolute), width, height);
        }

        public static Image CreateContinentLogo(Continent continent, double width, double height)
        {
            return CreateImage(new Uri(Utils.Logo(continent), UriKind.RelativeOrAbsolute), width, height);
        }

        public static Image CreateLogo(Club club, double width, double height)
        {
            return CreateImage(new Uri(Utils.Logo(club), UriKind.RelativeOrAbsolute), width, height);
        }

        public static Image CreateLogo(Tournament tournament, double width, double height)
        {
            return CreateImage(new Uri(Utils.LogoTournament(tournament), UriKind.RelativeOrAbsolute), width, height);
        }

        public static Border CreateCalendarItem(DateTime time, bool today, Match match = null, List<Tournament> tournaments = null)
        {

            Border res = new Border();
            res.Margin = new Thickness(2);
            string styleName = today ? "StyleBorderCalendarToday" : "StyleBorderCalendar";
            Style style = Application.Current.FindResource(styleName) as Style;
            res.Style = style;

            StackPanel main = new StackPanel();
            main.Width = 83;
            main.Height = 83;
            main.Orientation = Orientation.Vertical;

            main.Children.Add(CreateLabel(time.ToString("dddd dd/MM"), "StyleLabel2", 10, -1));

            if(match != null)
            {
                StackPanel spMatch = new StackPanel();
                spMatch.Orientation = Orientation.Horizontal;
                main.Children.Add(spMatch);
                spMatch.Children.Add(CreateLogo(match.home, 26, 26));
                if(match.Played)
                {
                    spMatch.Children.Add(CreateLabel(match.ScoreToString(true, true, Application.Current.FindResource("str_aet").ToString()), "StyleLabel2", 9, -1));
                }
                spMatch.Children.Add(CreateLogo(match.away, 26, 26));
            }

            if(tournaments != null && tournaments.Count > 0)
            {
                foreach(Tournament tournament in tournaments)
                {
                    main.Children.Add(CreateLabel(tournament.shortName, "StyleLabel2", 9, -1, tournament.IsInternational() ? System.Windows.Media.Brushes.DarkOrange : !tournament.isChampionship ? System.Windows.Media.Brushes.Blue : null, null, true));
                }
            }

            res.Child = main;

            return res;
        }

        public static StackPanel CreateNewsItem(Article a)
        {
            StackPanel spNews = new StackPanel();
            spNews.Orientation = Orientation.Horizontal;
            int days = Utils.DaysNumberBetweenTwoDates(Session.Instance.Game.date, a.publication);

            string dateString = days == 0 ? spNews.FindResource("str_today").ToString() : String.Format(spNews.FindResource("str_daysAgo").ToString(), days, (days == 1 ? "" : "s"));

            System.Windows.Media.Color color = (System.Windows.Media.Color)Application.Current.FindResource("ColorDate");
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Opacity = 0.6;
            spNews.Children.Add(CreateLabel(dateString, "StyleLabel2", 11, 100, brush, null, true));
            spNews.Children.Add(CreateLabel(a.title, "StyleLabel2", 11, -1));
            return spNews;
        }

        private static StackPanel GeneratePlayerIcon(Player p, Match match, bool showEnergy, double sizeMultiplier)
        {
            StackPanel playerPanel = new StackPanel();
            playerPanel.Orientation = Orientation.Vertical;

            Label label = new Label();
            label.Content = p.lastName;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.Style = Application.Current.FindResource("StyleLabel2") as Style;
            label.FontSize *= sizeMultiplier;

            bool hasRed = false;
            foreach (MatchEvent me in match.events)
            {
                if (me.type == GameEvent.RedCard && me.player == p)
                {
                    label.Foreground = Brushes.Red;
                    hasRed = true;
                }
                else if (!hasRed && me.type == GameEvent.YellowCard && me.player == p)
                {
                    label.Foreground = Brushes.Yellow;
                }
            }

            playerPanel.Children.Add(label);

            if (showEnergy)
            {
                ProgressBar pb = new ProgressBar();
                pb.Value = p.energy;
                pb.Maximum = 100;
                pb.Height = 5 * sizeMultiplier;
                pb.Width = 40 * sizeMultiplier;
                playerPanel.Children.Add(pb);
            }

            Label note = new Label();
            note.Content = p.level;
            note.HorizontalAlignment = HorizontalAlignment.Center;
            note.FontSize = 10 * sizeMultiplier;
            note.Style = Application.Current.FindResource("StyleLabel2") as Style;

            playerPanel.Children.Add(note);
            return playerPanel;
        }

        public static void AddElementToGrid(Grid grid, UIElement element, int row, int col, int colspan = -1, int rowspan = -1)
        {
            if(row > -1)
            {
                Grid.SetRow(element, row);
            }
            if (col > -1)
            {
                Grid.SetColumn(element, col);
            }
            if (colspan > -1)
            {
                Grid.SetColumnSpan(element, colspan);
            }
            if (rowspan > -1)
            {
                Grid.SetRowSpan(element, rowspan);
            }
            grid.Children.Add(element);
        }

        public static StackPanel CreateCompositionPanel(List<Player> players, bool showEnergy, Match match, List<Player> subs)
        {
            StackPanel res = new StackPanel();
            res.Orientation = Orientation.Vertical;
            res.Margin = new Thickness(10, 0, 10, 0);

            StackPanel spGardiens = new StackPanel();
            spGardiens.Orientation = Orientation.Horizontal;
            spGardiens.HorizontalAlignment = HorizontalAlignment.Center;
            StackPanel spDefender = new StackPanel();
            spDefender.Orientation = Orientation.Horizontal;
            spDefender.HorizontalAlignment = HorizontalAlignment.Center;
            StackPanel spMidfielder = new StackPanel();
            spMidfielder.Orientation = Orientation.Horizontal;
            spMidfielder.HorizontalAlignment = HorizontalAlignment.Center;
            StackPanel spStrickers = new StackPanel();
            spStrickers.Orientation = Orientation.Horizontal;
            spStrickers.HorizontalAlignment = HorizontalAlignment.Center;
            StackPanel spSubs = new StackPanel();
            spSubs.Orientation = Orientation.Horizontal;
            spSubs.HorizontalAlignment = HorizontalAlignment.Center;

            foreach(Player p in subs)
            {
                StackPanel playerPanel = GeneratePlayerIcon(p, match, showEnergy, 0.8);
                spSubs.Children.Add(playerPanel);
            }

            foreach (Player j in players)
            {
                StackPanel conteneur = null;
                switch (j.position)
                {
                    case Position.Goalkeeper: conteneur = spGardiens; break;
                    case Position.Defender: conteneur = spDefender; break;
                    case Position.Midfielder: conteneur = spMidfielder; break;
                    case Position.Striker: default:  conteneur = spStrickers; break;
                }

                conteneur.Children.Add(GeneratePlayerIcon(j, match, showEnergy, 1));
            }

            res.Children.Add(spGardiens);
            res.Children.Add(spDefender);
            res.Children.Add(spMidfielder);
            res.Children.Add(spStrickers);
            res.Children.Add(spSubs);

            return res;
        }

        public static string PlayerPositionOneLetter(Player p)
        {
            switch (p.position)
            {
                case Position.Goalkeeper:
                    return Application.Current.FindResource("str_position_g").ToString();
                case Position.Defender:
                    return Application.Current.FindResource("str_position_d").ToString();
                case Position.Midfielder:
                    return Application.Current.FindResource("str_position_m").ToString();
                case Position.Striker:
                default:
                    return Application.Current.FindResource("str_position_s").ToString();
            }
        }

        public static Border CreateStarsView(float stars, float starSize)
        {
            Border border = new Border();
            border.Padding = new Thickness(5);
            border.Background = Application.Current.TryFindResource("colorBackgroundStars") as SolidColorBrush;
            border.CornerRadius = new CornerRadius(starSize);
            border.Width = (starSize + 5) * 6;
            StackPanel spStars = new StackPanel();
            spStars.Margin = new Thickness(starSize/2, 0, 0, 0);
            spStars.Orientation = Orientation.Horizontal;
            spStars.HorizontalAlignment = HorizontalAlignment.Left;

            int fullStars = (int)Math.Floor(stars);
            for (int i = 1; i <= fullStars; i++)
            {
                Image img = new Image();
                img.Height = starSize;
                img.Width = starSize;
                img.Source = new BitmapImage(new Uri(Utils.Image("star.png")));
                img.Margin = new Thickness(0, 0, 5, 0);
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                spStars.Children.Add(img);
            }
            if (stars - fullStars != 0)
            {
                Image img = new Image();
                img.Height = starSize;
                img.Width = starSize;
                img.Source = new BitmapImage(new Uri(Utils.Image("star_half.png")));
                img.Margin = new Thickness(0, 0, 5, 0);
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                spStars.Children.Add(img);
            }
            border.Child = spStars;

            return border;
        }
    }
}
