using System;
using System.Collections.Generic;
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
using tm;
using TheManager_GUI.Styles;

namespace TheManager_GUI.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlCompositionPlayer.xaml
    /// </summary>
    public partial class ControlCompositionPlayer : UserControl
    {

        public string PlayerName { get; set; }
        public string PlayerNumber { get; set; }
        public SolidColorBrush JerseyColor { get; set; }
        public SolidColorBrush JerseyTextColor { get; set; }

        public ControlCompositionPlayer(Player player, SolidColorBrush backgroundColor, SolidColorBrush frontColor, float sizeMultiplier)
        {
            InitializeComponent();
            this.PlayerName = player.ShortName;
            this.PlayerNumber = player.level.ToString();
            this.JerseyColor = backgroundColor;
            if(frontColor != null)
            {
                this.JerseyTextColor = frontColor;
            }
            else
            {
                this.JerseyTextColor = FindResource(StyleDefinition.solidColorBrushColorLight) as SolidColorBrush;
            }

            textPlayer.FontSize *= sizeMultiplier;
            textNumber.FontSize *= sizeMultiplier;
            grid.MinHeight *= sizeMultiplier;
            grid.MaxHeight *= sizeMultiplier;
            grid.MinWidth *= sizeMultiplier;
            grid.MaxWidth *= sizeMultiplier;

            DataContext = this;
        }
    }
}
