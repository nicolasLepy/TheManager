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
            System.Windows.Media.Color c = new System.Windows.Media.Color();
            c.R = color.red;
            c.G = color.green;
            c.B = color.blue;
            SolidColorBrush scb = new SolidColorBrush(c);

            Console.WriteLine(color.ToHexa());
            return color.ToHexa();

            //TODO : give to XAML another thing than a Brushes
            if (color.red == 200 && color.green == 200 && color.blue == 200)
            {
                return Brushes.LightGray;
            }
            else if (color.red == 40 && color.green == 40 && color.blue == 230)
            {
                return Brushes.DeepSkyBlue;
            }
            else if (color.red == 250 && color.green == 0 && color.blue == 0)
            {
                return Brushes.Red;
            }
            else if (color.red == 250 && color.green == 150 && color.blue == 150)
            {
                return Brushes.PaleVioletRed;
            }
            else if (color.red == 200 && color.green == 200 && color.blue == 0)
            {
                return Brushes.Yellow;
            }
            else if (color.red == 0 && color.green == 0 && color.blue == 200)
            {
                return Brushes.Blue;
            }
            else if (color.red == 0 && color.green == 200 && color.blue == 200)
            {
                return Brushes.DarkCyan;
            }
            else if (color.red == 200 && color.green == 100 && color.blue == 50)
            {
                return Brushes.Orange;
            }


            return Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
