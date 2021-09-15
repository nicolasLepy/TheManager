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


        public delegate void SortActionOnButtonClick(object sender, MouseButtonEventArgs e, PlayerAttribute attribute);

        /// <summary>
        /// Create a WPF label object
        /// </summary>
        /// <param name="content">Text of the label</param>
        /// <param name="style">Style of the label</param>
        /// <param name="fontSize">Font size (-1 to let style default font size)</param>
        /// <param name="width">Width of the label box</param>
        /// <param name="color">Color of the label</param>
        /// <returns></returns>
        public static Label CreateLabel(string content, string style, double fontSize, double width, Brush color = null, SortActionOnButtonClick onClick = null, PlayerAttribute attribute = PlayerAttribute.LEVEL)
        {
            Label label = new Label();
            label.Content = content;
            label.Style = Application.Current.FindResource(style) as Style;
            if(fontSize > -1)
            {
                label.FontSize = fontSize;
            }
            label.Width = width;
            if (color != null)
            {
                label.Foreground = color;
            }
            if (onClick != null)
                label.MouseLeftButtonUp += new MouseButtonEventHandler((s, e) => onClick(s, e, attribute));

            return label;
        }

        public static ProgressBar CreateProgressBar(float value, float minimum = 0, float maximum = 100)
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.Minimum = minimum;
            progressBar.Maximum = maximum;
            progressBar.Value = value;
            progressBar.Width = 60;
            return progressBar;

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
                    case Position.Striker: conteneur = spStrickers; break;
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
