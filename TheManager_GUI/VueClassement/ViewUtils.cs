using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
