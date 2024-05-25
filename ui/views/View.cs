using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using tm;
using TheManager_GUI.utils;

namespace TheManager_GUI.Views
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
        PLAYER_CONTRACT_END,
        PLAYER_CLUB
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
            Handlers.OpenPlayer(p);
        }

        public void OpenClub(Club c)
        {
            Handlers.OpenClub(c);
        }

        public void OpenMatch(Match m)
        {
            Handlers.OpenMatch(m);
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
