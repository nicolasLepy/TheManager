using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using TheManager;

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
        /// Create a WPF label object
        /// </summary>
        /// <param name="content">Text of the label</param>
        /// <param name="style">Style of the label</param>
        /// <param name="fontSize">Font size (-1 to let style default font size)</param>
        /// <param name="width">Width of the label box</param>
        /// <param name="color">Color of the label</param>
        /// <returns></returns>
        public static Label CreateLabel(string content, string style, double fontSize, double width, Brush color = null)
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
            return label;
        }

        public static StackPanel CreateCompositionPanel(List<Player> players, bool showEnergy)
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

                StackPanel conteneurJoueur = new StackPanel();
                conteneurJoueur.Orientation = Orientation.Vertical;

                Label label = new Label();
                label.Content = j.lastName;
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.Style = Application.Current.FindResource("StyleLabel2") as Style;
                conteneurJoueur.Children.Add(label);

                if (showEnergy)
                {
                    ProgressBar pb = new ProgressBar();
                    pb.Value = j.energy;
                    pb.Maximum = 100;
                    pb.Height = 5;
                    pb.Width = 40;
                    conteneurJoueur.Children.Add(pb);
                }

                Label note = new Label();
                note.Content = j.level;
                note.HorizontalAlignment = HorizontalAlignment.Center;
                note.FontSize = 10;
                note.Style = Application.Current.FindResource("StyleLabel2") as Style;

                conteneurJoueur.Children.Add(note);

                conteneur.Children.Add(conteneurJoueur);
            }

            res.Children.Add(spGardiens);
            res.Children.Add(spDefender);
            res.Children.Add(spMidfielder);
            res.Children.Add(spStrickers);
            return res;
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

            return res;
        }


    }
}
