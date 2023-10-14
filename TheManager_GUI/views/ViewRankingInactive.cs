using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheManager;
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

            int rowsNumber = clubsByAdministrativeDivision.Count + clubsWithoutAssociation.Count + 1;
            foreach (KeyValuePair<AdministrativeDivision, List<Club>> adm in clubsByAdministrativeDivision)
            {
                rowsNumber += adm.Value.Count;
            }
            Grid grid = new Grid();
            for(int row = 0; row < rowsNumber; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(fontSize * 1.8, GridUnitType.Pixel) });
            }

            int i = 0;
            foreach (KeyValuePair<AdministrativeDivision, List<Club>> adm in clubsByAdministrativeDivision)
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
