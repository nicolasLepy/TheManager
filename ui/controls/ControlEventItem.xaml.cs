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
using TheManager_GUI;
using tm;

namespace ui.controls
{
    /// <summary>
    /// Logique d'interaction pour ControlEventItem.xaml
    /// </summary>
    public partial class ControlEventItem : UserControl
    {
        public ControlEventItem()
        {
            InitializeComponent();
        }

        private string Icon(GameEvent gameEvent)
        {
            string icon;
            switch (gameEvent)
            {
                case GameEvent.Goal:
                case GameEvent.PenaltyGoal:
                case GameEvent.AgGoal:
                    icon = "goal.png";
                    break;
                case GameEvent.YellowCard:
                    icon = "yellow_card.png";
                    break;
                case GameEvent.RedCard:
                    icon = "red_card.png";
                    break;
                case GameEvent.Shot:
                default:
                    icon = "";
                    break;
            }
            return icon;
        }

        public void Update(Match match, MatchEvent evnt)
        {
            textHome.Text = match.home.abbr();
            textAway.Text = match.away.abbr();
            textScore.Text = ""; // String.Format("{0}-{1}", match.score1, match.score2);
            textTime.Text = match.Time;
            textPlayer.Text = evnt.player.ShortName;
            BitmapImage bitmapHome = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.home)));
            BitmapImage bitmapAway = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Logo(match.away)));
            imageHome.Source = bitmapHome;
            imageAway.Source = bitmapAway;
            imageIcon.Source = ViewUtils.LoadBitmapImageWithCache(new Uri(Utils.Icon(Icon(evnt.type))));

        }
    }
}
