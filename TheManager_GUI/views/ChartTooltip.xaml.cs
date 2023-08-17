using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TheManager_GUI.views
{
    /// <summary>
    /// Logique d'interaction pour ChartTooltip.xaml
    /// </summary>
    public partial class ChartTooltip : UserControl, IChartTooltip
    {
        private TooltipData _data;

        public ChartTooltip()
        {
            InitializeComponent();
            DataContext = this;
        }

        public TooltipData Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TooltipSelectionMode? SelectionMode { get; set; }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
            { PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        }

    }
}
