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
using TheManager;
using TheManager_GUI.Styles;

namespace TheManager_GUI.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlStickerTournament.xaml
    /// </summary>
    public partial class ControlStickerTournament : UserControl
    {
        public ControlStickerTournament(Tournament tournament, Dictionary<Club, Round> clubs)
        {
            InitializeComponent();
            tbTournament.Text = tournament.name;
            imageTournament.Source = new BitmapImage(new Uri(Utils.LogoTournament(tournament), UriKind.RelativeOrAbsolute));

            foreach(KeyValuePair<Club, Round> kvp in clubs)
            {
                Club c = kvp.Key;
                Round r = kvp.Value;
                TextBlock tbClub = ViewUtils.CreateTextBlock(c.shortName, StyleDefinition.styleTextPlain);
                tbClub.TextWrapping = TextWrapping.Wrap;
                Image logoClub = ViewUtils.CreateLogo(c, 25, 25);
                TextBlock tbRound = ViewUtils.CreateTextBlock(r.name, StyleDefinition.styleTextPlain);
                gridMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40, GridUnitType.Pixel)});
                ViewUtils.AddElementToGrid(gridMain, logoClub, gridMain.RowDefinitions.Count - 1, 0);
                ViewUtils.AddElementToGrid(gridMain, tbClub, gridMain.RowDefinitions.Count - 1, 1);
                ViewUtils.AddElementToGrid(gridMain, tbRound, gridMain.RowDefinitions.Count - 1, 2);
                borderTournament.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(tournament.color.ToHexa()));
            }
        }
    }
}
