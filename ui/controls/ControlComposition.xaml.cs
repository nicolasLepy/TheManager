using ColorThiefDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheManager;
using TheManager_GUI.Styles;

namespace TheManager_GUI.controls
{

    public enum ControlCompositionType
    {
        Composition,
        Subs
    }

    /// <summary>
    /// Logique d'interaction pour ControlComposition.xaml
    /// </summary>
    public partial class ControlComposition : UserControl
    {

        public Action<object, MouseButtonEventArgs, Player> OnClickPlayer { get; set; }

        public ControlCompositionType Type { get; set; }

        private readonly Club club;
        private readonly SolidColorBrush[] jersayBrushes = new SolidColorBrush[] {null, null};

        public ControlComposition(ControlCompositionType type, Club club)
        {
            this.club = club;
            this.Type = type;
            GetColors();

            InitializeComponent();
            if(this.Type == ControlCompositionType.Subs)
            {
                RowDefinition first = grid.RowDefinitions[0];
                RowDefinition second = grid.RowDefinitions[1];
                grid.RowDefinitions.Clear();
                grid.RowDefinitions.Add(first);
                grid.RowDefinitions.Add(second);
            }
        }

        private void GetColors()
        {
            ColorThief colorThief = new ColorThief();
            System.Drawing.Bitmap b = new System.Drawing.Bitmap(Utils.Logo(club));
            List<ColorThiefDotNet.QuantizedColor> colors = colorThief.GetPalette(b, 5);
            if (colors.Count > 0)
            {
                ColorThiefDotNet.Color color = colors[0].Color;
                jersayBrushes[0] = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, color.R, color.G, color.B));
            }
            if (colors.Count > 1)
            {
                ColorThiefDotNet.Color color = colors[1].Color;
                jersayBrushes[1] = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, color.R, color.G, color.B));
            }
            //ColorThiefDotNet.Color uniqueColor = colorThief.GetColor(b, 5, false).Color;
            //jersayBrushes[0] = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, uniqueColor.R, uniqueColor.G, uniqueColor.B));
            b.Dispose();
        }

        public void FillComposition(List<Player> players)
        {
            Dictionary<Position, List<Player>> playersByPosition = new Dictionary<Position, List<Player>>();
            playersByPosition[Position.Goalkeeper] = new List<Player>();
            playersByPosition[Position.Defender] = new List<Player>();
            playersByPosition[Position.Midfielder] = new List<Player>();
            playersByPosition[Position.Striker] = new List<Player>();
            foreach (Player p in players)
            {
                playersByPosition[p.position].Add(p);
            }
            FillCompositionLine(3, playersByPosition[Position.Goalkeeper]);
            FillCompositionLine(2, playersByPosition[Position.Defender]);
            FillCompositionLine(1, playersByPosition[Position.Midfielder]);
            FillCompositionLine(0, playersByPosition[Position.Striker]);
        }

        public void FillSubs(List<Player> players)
        {
            if (players.Count < 7)
            {
                FillCompositionLine(0, players);
            }
            else
            {
                FillCompositionLine(0, players.GetRange(0, 6));
                FillCompositionLine(1, players.GetRange(6, players.Count-6));
            }
        }

        public void Fill(List<Player> players)
        {
            grid.Children.Clear();
            if(Type == ControlCompositionType.Composition)
            {
                FillComposition(players);
            }
            if(Type == ControlCompositionType.Subs)
            {
                FillSubs(players);
            }
        }

        private void FillCompositionLine(int row, List<Player> players)
        {
            Grid gridRow = new Grid();
            int cols = Type == ControlCompositionType.Subs ? 6 : players.Count;
            for(int i = 0; i < cols; i++)
            {
                gridRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < players.Count; i++)
            {
                ControlCompositionPlayer control = new ControlCompositionPlayer(players[i], jersayBrushes[0], jersayBrushes[1], Type == ControlCompositionType.Composition ? 1 : 0.63f);
                control.Margin = new Thickness(10);

                if (OnClickPlayer != null)
                {
                    Player player = players[i];
                    control.MouseLeftButtonUp += (sender, e) => OnClickPlayer(sender, e, player);
                }

                ViewUtils.AddElementToGrid(gridRow, control, 0, i);
            }
            ViewUtils.AddElementToGrid(grid, gridRow, row, 0);
        }
    }
}
