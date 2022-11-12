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
            sprite.Width = width;
            sprite.Height = height;
            return sprite;
        }

        public static Image CreateImage(string path, double width, double height)
        {
            Image sprite = new Image();
            sprite.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
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

        public static Image CreateImage(Uri uri, double width, double height)
        {
            Image sprite = new Image();
            sprite.Source = new BitmapImage(uri);
            sprite.Width = width;
            sprite.Height = height;
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

            main.Children.Add(CreateLabel(time.ToString("dddd dd"), "StyleLabel2", 10, -1));

            if(match != null)
            {
                StackPanel spMatch = new StackPanel();
                spMatch.Orientation = Orientation.Horizontal;
                main.Children.Add(spMatch);
                spMatch.Children.Add(CreateLogo(match.home, 26, 26));
                if(match.Played)
                {
                    spMatch.Children.Add(CreateLabel(match.ScoreToString(), "StyleLabel2", 9, -1));
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
                    return "G";
                case Position.Defender:
                    return "D";
                case Position.Midfielder:
                    return "M";
                case Position.Striker:
                default:
                    return "S";
            }
            return "-";
        }

        public static StackPanel CreateStarNotation(float notation, float starsSize)
        {
            StackPanel res = new StackPanel();
            res.Orientation = Orientation.Horizontal;

            int entireStars = (int)Math.Floor(notation);
            for (int i = 1; i <= entireStars; i++)
            {
                Image img = new Image();
                img.Width = starsSize;
                img.Height = starsSize;
                img.Source = new BitmapImage(new Uri(Utils.Image("star.png")));
                res.Children.Add(img);
            }
            if (notation - entireStars != 0)
            {
                Image img = new Image();
                img.Width = starsSize;
                img.Height = starsSize;
                img.Source = new BitmapImage(new Uri(Utils.Image("demistar.png")));
                res.Children.Add(img);
            }

            res.Width = starsSize * 6;

            return res;
        }



    }
}
