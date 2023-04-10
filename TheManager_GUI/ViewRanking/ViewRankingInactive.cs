using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public class ViewRankingInactive : ViewRanking
    {

        private InactiveRound _round;
        private bool _focusOnTeam;
        private double _sizeMultiplier;
        private Club _team;

        public ViewRankingInactive(InactiveRound round, double sizeMultiplier, bool focusOnTeam, Club team) : base(round.Tournament)
        {
            _round = round;
            _focusOnTeam = focusOnTeam;
            _sizeMultiplier = sizeMultiplier;
            _team = team;
        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            StackPanel spList = new StackPanel();
            spList.Orientation = Orientation.Vertical;

            foreach (Club c in _round.clubs)
            {
                SolidColorBrush backgroundColor = _focusOnTeam && _team == c ? Application.Current.TryFindResource("UpperPlayOff") as SolidColorBrush : null;
                Label labelClub = ViewUtils.CreateLabel(c.name, "StyleLabel2", (int)(14 * _sizeMultiplier), -1, null, backgroundColor);
                spList.Children.Add(labelClub);
            }
            spRanking.Children.Add(spList);
        }
    }
}
