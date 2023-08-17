using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TheManager_GUI.Styles;

namespace TheManager_GUI.views
{
    public class MoneyValueForegroundConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = Application.Current.FindResource(StyleDefinition.solidColorBrushColorTitle1) as SolidColorBrush;
            
            if ((double)value < 0)
                brush = new SolidColorBrush(Colors.Red);

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
