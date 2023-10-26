using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace TheManager_GUI.Styles
{
    public class TMButtonProperties
    {
        public static ImageSource GetImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageProperty);
        }

        public static void SetImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageProperty, value);
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.RegisterAttached("Image", typeof(ImageSource), typeof(TMButtonProperties), new UIPropertyMetadata((ImageSource)null));
    }

}
