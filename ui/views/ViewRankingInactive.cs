using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using tm;
using TheManager_GUI.Styles;

namespace TheManager_GUI.Views
{
    public class ViewRankingInactive : ViewRanking
    {

        private readonly InactiveRound _round;

        public override Round Round()
        {
            return _round;
        }

        public ViewRankingInactive(InactiveRound round, double sizeMultiplier, bool focusOnTeam, Club team) : base(round, round.Tournament, false, sizeMultiplier, RankingType.General, focusOnTeam, team)
        {
            _round = round;
        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            double fontSize = (double)Application.Current.FindResource(StyleDefinition.fontSizeRegular);

            Dictionary<Association, List<Club>> clubsByAssociation = new Dictionary<Association, List<Club>>();
            List<Club> clubsWithoutAssociation = new List<Club>();

            foreach (Club c in _round.clubs)
            {
                Association ad = c.Country().GetAssociationLevel(c.Association(), 1);
                if(ad == null)
                {
                    clubsWithoutAssociation.Add(c);
                }
                else
                {
                    if (!clubsByAssociation.ContainsKey(ad))
                    {
                        clubsByAssociation.Add(ad, new List<Club>());
                    }
                    clubsByAssociation[ad].Add(c);
                }
            }

            int rowsNumber = clubsByAssociation.Count + clubsWithoutAssociation.Count + 1;
            foreach (KeyValuePair<Association, List<Club>> adm in clubsByAssociation)
            {
                rowsNumber += adm.Value.Count;
            }
            Grid grid = new Grid();
            for(int row = 0; row < rowsNumber; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(fontSize * 1.8, GridUnitType.Pixel) });
            }

            int i = 0;
            foreach (KeyValuePair<Association, List<Club>> adm in clubsByAssociation)
            {
                TextBlock tbAdm = ViewUtils.CreateTextBlock(adm.Key.name, StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                AddElementToGrid(grid, tbAdm, i++, 0);

                foreach (Club c in adm.Value)
                {
                    TextBlock tbClub = ViewUtils.CreateTextBlock(c.name, StyleDefinition.styleTextPlain, fontSize * _sizeMultiplier);
                    AddElementToGrid(grid, tbClub, i++, 0);
                }
            }
            i++;
            foreach(Club c in clubsWithoutAssociation)
            {
                TextBlock tbClub = ViewUtils.CreateTextBlock(c.name, StyleDefinition.styleTextPlain, fontSize * _sizeMultiplier);
                AddElementToGrid(grid, tbClub, i++, 0);
            }
            spRanking.Children.Add(grid);
        }
    }
}
