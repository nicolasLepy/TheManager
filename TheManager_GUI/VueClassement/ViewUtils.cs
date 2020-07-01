using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TheManager_GUI
{
    public class ViewUtils
    {

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
