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

        private readonly InactiveRound _round;
        private readonly bool _focusOnTeam;
        private readonly double _sizeMultiplier;
        private readonly Club _team;

        public ViewRankingInactive(InactiveRound round, double sizeMultiplier, bool focusOnTeam, Club team) : base(round.Tournament)
        {
            _round = round;
            _focusOnTeam = focusOnTeam;
            _sizeMultiplier = sizeMultiplier;
            _team = team;
        }

        public void AddLabel(Club c, StackPanel stackPanel)
        {
            string name = (c as ReserveClub != null) ? string.Format("[{0}]", c.name) : c.name;
            SolidColorBrush backgroundColor = _focusOnTeam && _team == c ? Application.Current.TryFindResource("UpperPlayOff") as SolidColorBrush : null;
            Label labelClub = ViewUtils.CreateLabel(name, "StyleLabel2", (int)(14 * _sizeMultiplier), -1, null, backgroundColor);
            stackPanel.Children.Add(labelClub);

        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            StackPanel spList = new StackPanel();
            spList.Orientation = Orientation.Vertical;

            Dictionary<AdministrativeDivision, List<Club>> clubsByAdministrativeDivision = new Dictionary<AdministrativeDivision, List<Club>>();
            List<Club> clubsWithoutAssociation = new List<Club>();

            foreach (Club c in _round.clubs)
            {
                AdministrativeDivision ad = c.Country().GetAdministrativeDivisionLevel(c.AdministrativeDivision(), 1);
                if(ad == null)
                {
                    clubsWithoutAssociation.Add(c);
                }
                else
                {
                    if (!clubsByAdministrativeDivision.ContainsKey(ad))
                    {
                        clubsByAdministrativeDivision.Add(ad, new List<Club>());
                    }
                    clubsByAdministrativeDivision[ad].Add(c);
                }
            }

            foreach (KeyValuePair<AdministrativeDivision, List<Club>> adm in clubsByAdministrativeDivision)
            {
                Label labelAdm = ViewUtils.CreateLabel(adm.Key.name, "StyleLabel2", (int)(14 * _sizeMultiplier), -1, null, null, true);
                spList.Children.Add(labelAdm);
                foreach (Club c in adm.Value)
                {
                    AddLabel(c, spList);
                }
            }
            foreach(Club c in clubsWithoutAssociation)
            {
                AddLabel(c, spList);
            }
            spRanking.Children.Add(spList);
        }
    }
}
