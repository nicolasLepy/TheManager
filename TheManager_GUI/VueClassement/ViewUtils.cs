using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

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


    }
}
