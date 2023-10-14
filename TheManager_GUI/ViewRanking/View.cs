using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{

    public enum GridColumn
    {
        HOME_TEAM,
        AWAY_TEAM,
        HOME_LOGO,
        AWAY_LOGO,
        SCORE,
        HOUR,
        ATTENDANCE,
        ODDS,
        TOURNAMENT,
        DATE,
        HALFTIME_SCORE,
        PLAYER_NAME,
        PLAYER_AGE,
        PLAYER_POSITION,
        PLAYER_NATIONALITY,
        PLAYER_LEVEL,
        PLAYER_POTENTIAL,
        PLAYER_GAMES_NUMBER,
        PLAYER_GOALS_NUMBER,
        PLAYER_CONDITION,
        PLAYER_VALUE,
        PLAYER_WAGE,
        PLAYER_IS_SUSPENDED,
        PLAYER_IS_INJURIED,
        PLAYER_IS_INTERNATIONAL,
        PLAYER_INTERNATIONAL_SELECTIONS,
        PLAYER_INTERNATIONAL_GOALS,
        PLAYER_CONTRACT_BEGIN,
        PLAYER_CONTRACT_END
    }

    public class GridColumnComparator : IComparer<GridColumnDefinition>
    {
        public int Compare(GridColumnDefinition x, GridColumnDefinition y)
        {
            return x.order - y.order;
        }
    }

    public struct GridColumnDefinition
    {
        public GridColumn columnType { get; set; }
        public double width { get; set; }
        public int order { get; set; }

        public GridColumnDefinition(GridColumn columnType, double width, int order)
        {
            this.columnType = columnType;
            this.width = width;
            this.order = order;
        }
    }

    public abstract class View
    {
        public abstract void Full(StackPanel spRanking);

        public void OpenPlayer(Player p)
        {
            PlayerView view = new PlayerView(p);
            view.Show();
        }

        public void OpenClub(Club c)
        {
            if (c as CityClub != null)
            {
                ClubView wc = new ClubView(c as CityClub);
                wc.Show();
            }
            else if(c as ReserveClub != null)
            {
                ClubView wc = new ClubView((c as ReserveClub).FannionClub);
                wc.Show();
            }
            else if(c as NationalTeam != null)
            {
                CountryView cw = new CountryView(c as NationalTeam);
                cw.Show();
            }
        }

        public void OpenMatch(Match m)
        {
            MatchView view = new MatchView(m);
            view.Show();
            //Windows_Match wm = new Windows_Match(m);
            //wm.Show();
        }

        protected void AddElementToGrid(Grid grid, UIElement element, int row, int col, int colspan = -1)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, col);
            if (colspan > -1)
            {
                Grid.SetColumnSpan(element, colspan);
            }
            grid.Children.Add(element);
        }


    }
}
