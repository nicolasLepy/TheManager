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
