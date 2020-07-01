using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using TheManager;

namespace TheManager_GUI
{
    public class DataGridTournamentColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Tournament tournament = value as Tournament;
            TheManager.Color color = tournament.color;
            Console.WriteLine(color.ToHexa());
            return color.ToHexa();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
