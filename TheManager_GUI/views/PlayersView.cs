using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TheManager.Comparators;
using TheManager;
using TheManager_GUI.VueClassement;
using System.Text.RegularExpressions;
using TheManager_GUI.Styles;
using System.Windows;
using System.Numerics;

namespace TheManager_GUI.views
{
    public class PlayersView : View
    {

        private readonly List<GridColumnDefinition> columns;

        private readonly float fontSize;
        private readonly float logoSize;
        private readonly bool LevelsInNumbers;

        private List<Player> Players;

        private StackPanel spRanking;

        private bool sortOrder;

        public PlayersView(List<Player> players, float sizeMultiplier, bool age, bool position, bool nationality, bool level, bool potential, bool games, bool goals, bool condition, bool value, bool wage, bool isInjuried, bool isSuspended, bool isInternational, bool internationalSelections, bool internationalGoals, bool contractBegin, bool contractEnd, bool levelsInNumbers = false)
        {

            Players = players;
            columns = new List<GridColumnDefinition>();
            columns.Add(new GridColumnDefinition(GridColumn.PLAYER_NAME, 250, 0));
            if (age)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_AGE, 75, 2));
            }
            if (position)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_POSITION, 30, 1));
            }
            if (nationality)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_NATIONALITY, 50, 6));
            }
            if (level)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_LEVEL, 100, 3));
            }
            if (potential)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_POTENTIAL, 100, 4));
            }
            if (games)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_GAMES_NUMBER, 75, 12));
            }
            if (goals)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_GOALS_NUMBER, 75, 13));
            }
            if (condition)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_CONDITION, 75, 5));
            }
            if (value)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_VALUE, 75, 7));
            }
            if (wage)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_WAGE, 75, 8));
            }
            if (isSuspended)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_IS_SUSPENDED, 75, 14));
            }
            if (isInjuried)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_IS_INJURIED, 75, 11));
            }
            if (isInternational)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_IS_INTERNATIONAL, 75, 15));
            }
            if (internationalSelections)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_INTERNATIONAL_SELECTIONS, 75, 16));
            }
            if (internationalGoals)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_INTERNATIONAL_GOALS, 75, 17));
            }
            if (contractBegin)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_CONTRACT_BEGIN, 75, 9));
            }
            if (contractEnd)
            {
                columns.Add(new GridColumnDefinition(GridColumn.PLAYER_CONTRACT_END, 75, 10));
            }

            fontSize = (float)(double)Application.Current.FindResource(StyleDefinition.fontSizeRegular);
            logoSize = fontSize * 5/3f;
            sortOrder = false;
            LevelsInNumbers = levelsInNumbers;
        }

        private void FillPlayerName(Grid grid, Player player, int row, int col)
        {
            TextBlock tbPlayer = ViewUtils.CreateTextBlockOpenWindow(player, OpenPlayer, player.Name, StyleDefinition.styleTextPlain, fontSize, -1);
            AddElementToGrid(grid, tbPlayer, row, col);
        }

        private void FillPlayerAge(Grid grid, Player player, int row, int col)
        {
            TextBlock tbAge = ViewUtils.CreateTextBlock(player.Age + " " + Application.Current.FindResource("str_yo").ToString(), StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbAge, row, col);
        }

        private void FillPlayerPosition(Grid grid, Player player, int row, int col)
        {
            TextBlock tbPosition = ViewUtils.CreateTextBlock(ViewUtils.PlayerPositionOneLetter(player), StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbPosition, row, col);
        }

        private void FillPlayerNationality(Grid grid, Player player, int row, int col)
        {
            Image imageCountry = ViewUtils.CreateFlag(player.nationality, logoSize, logoSize*0.66);
            AddElementToGrid(grid, imageCountry, row, col);
        }

        private void FillPlayerLevel(Grid grid, Player player, int row, int col)
        {
            if (!LevelsInNumbers)
            {
                AddElementToGrid(grid, ViewUtils.CreateStarsView(player.Stars, logoSize/2), row, col);
            }
            else
            {
                TextBlock tbLevel = ViewUtils.CreateTextBlock(player.level.ToString(), StyleDefinition.styleTextPlainCenter, fontSize, -1);
                AddElementToGrid(grid, tbLevel, row, col);
            }
        }

        private void FillPlayerPotential(Grid grid, Player player, int row, int col)
        {
            if (!LevelsInNumbers)
            {
                AddElementToGrid(grid, ViewUtils.CreateStarsView(player.StarsPotential, logoSize/2), row, col);
            }
            else
            {
                TextBlock tbPotential = ViewUtils.CreateTextBlock(player.potential.ToString(), StyleDefinition.styleTextPlainCenter, fontSize, -1);
                AddElementToGrid(grid, tbPotential, row, col);
            }
        }

        private void FillPlayerGamesNumber(Grid grid, Player player, int row, int col)
        {
            int playedGames = 0;
            foreach(KeyValuePair<Club, int> games in player.playedGames)
            {
                playedGames += games.Value;
            }
            TextBlock tbPlayedGames = ViewUtils.CreateTextBlock(playedGames.ToString(), StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbPlayedGames, row, col);
        }

        private void FillPlayerGoalsNumber(Grid grid, Player player, int row, int col)
        {
            int goalsScored = 0;
            foreach (KeyValuePair<Club, int> club in player.goalsScored)
            {
                goalsScored += club.Value;
            }
            TextBlock tbGoalsScored = ViewUtils.CreateTextBlock(goalsScored.ToString(), StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbGoalsScored, row, col);
        }

        private void FillPlayerCondition(Grid grid, Player player, int row, int col)
        {
            ProgressBar pb = ViewUtils.CreateProgressBar(player.energy, 0, 100, 40, 10);
            AddElementToGrid(grid, pb, row, col);
        }

        private void FillPlayerValue(Grid grid, Player player, int row, int col)
        {
            TextBlock tbValue = ViewUtils.CreateTextBlock(Utils.FormatMoney(player.EstimateTransferValue()), StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbValue, row, col);
        }

        private void FillPlayerWage(Grid grid, Player player, int row, int col)
        {
            string wageStr = player.Club != null ? Utils.FormatMoney(player.Club.FindContract(player).wage) + "/m" : "-";
            TextBlock tbWage = ViewUtils.CreateTextBlock(wageStr, StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbWage, row, col);
        }

        private void FillPlayerIsSuspended(Grid grid, Player player, int row, int col)
        {
            throw new NotImplementedException();
        }

        private void FillPlayerIsInjuried(Grid grid, Player player, int row, int col)
        {
            throw new NotImplementedException();
        }

        private void FillPlayerIsInternational(Grid grid, Player player, int row, int col)
        {
            throw new NotImplementedException();
        }

        private void FillPlayerInternationalSelections(Grid grid, Player player, int row, int col)
        {
            TextBlock tbCaps = ViewUtils.CreateTextBlock(player.InternationalCaps.ToString(), StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbCaps, row, col);
        }

        private void FillPlayerInternationalGoals(Grid grid, Player player, int row, int col)
        {
            TextBlock tbGoals = ViewUtils.CreateTextBlock(player.InternationalGoals.ToString(), StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbGoals, row, col);
        }

        private void FillPlayerContractBegin(Grid grid, Player player, int row, int col)
        {
            string contractBegin = player.Club != null ? player.Club.FindContract(player).beginning.ToShortDateString() : "-";
            TextBlock tbContractBegin = ViewUtils.CreateTextBlock(contractBegin, StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbContractBegin, row, col);
        }

        private void FillPlayerContractEnd(Grid grid, Player player, int row, int col)
        {
            string contractEnd = player.Club != null ? player.Club.FindContract(player).end.ToShortDateString() : "-";
            TextBlock tbContractEnd = ViewUtils.CreateTextBlock(contractEnd, StyleDefinition.styleTextPlainCenter, fontSize, -1);
            AddElementToGrid(grid, tbContractEnd, row, col);
        }

        public override void Full(StackPanel spRanking)
        {
            this.spRanking = spRanking;
            spRanking.Children.Clear();

            Grid grid = new Grid();
            

            for (int i = 0; i < columns.Count; i++)
            {
                GridColumnDefinition gColumn = columns[i];
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(gColumn.width, GridUnitType.Star) });
            }

            for (int i = 0; i < Players.Count+1; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(logoSize, GridUnitType.Pixel) });
            }

            foreach (GridColumnDefinition gcd in columns)
            {
                switch (gcd.columnType)
                {
                    case GridColumn.PLAYER_NAME:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.NAME, sortPlayers, Application.Current.FindResource("str_name").ToString(), StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_AGE:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.AGE, sortPlayers, "Age", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_POSITION:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.POSITION, sortPlayers, "Pos", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_NATIONALITY:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.NATIONALITY, sortPlayers, "Nat.", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_LEVEL:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.LEVEL, sortPlayers, "Lvl", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_POTENTIAL:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.POTENTIAL, sortPlayers, "Pot.", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_GAMES_NUMBER:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.GAMES, sortPlayers, "Games", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_GOALS_NUMBER:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.GOALS, sortPlayers, "Goals", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_CONDITION:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.CONDITION, sortPlayers, "Cond.", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_VALUE:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.VALUE, sortPlayers, "Value", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_WAGE:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.WAGE, sortPlayers, "Wage", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_IS_SUSPENDED:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.IS_SUSPENDED, sortPlayers, "Sus.", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_IS_INJURIED:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.IS_INJURIED, sortPlayers, "Inj.", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_IS_INTERNATIONAL:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.IS_INTERNATIONAL, sortPlayers, "Int.", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_INTERNATIONAL_SELECTIONS:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.INTERNATIONAL_SELECTIONS, sortPlayers, "Int. S", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_INTERNATIONAL_GOALS:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.INTERNATIONAL_GOALS, sortPlayers, "Int. G", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_CONTRACT_BEGIN:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.CONTRACT_BEGIN, sortPlayers, "Begin", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                    case GridColumn.PLAYER_CONTRACT_END:
                        AddElementToGrid(grid, ViewUtils.CreateTextBlockOpenWindow(PlayerAttribute.CONTRACT_END, sortPlayers, "End", StyleDefinition.styleTextPlainCenter, fontSize, -1), 0, columns.IndexOf(gcd));
                        break;
                }
            }

            int row = 1;
            foreach (Player player in Players)
            {

                foreach(GridColumnDefinition gcd in columns)
                {
                    switch (gcd.columnType)
                    {
                        case GridColumn.PLAYER_NAME:
                            FillPlayerName(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_AGE:
                            FillPlayerAge(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_POSITION:
                            FillPlayerPosition(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_NATIONALITY:
                            FillPlayerNationality(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_LEVEL:
                            FillPlayerLevel(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_POTENTIAL:
                            FillPlayerPotential(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_GAMES_NUMBER:
                            FillPlayerGamesNumber(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_GOALS_NUMBER:
                            FillPlayerGoalsNumber(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_CONDITION:
                            FillPlayerCondition(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_VALUE:
                            FillPlayerValue(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_WAGE:
                            FillPlayerWage(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_IS_SUSPENDED:
                            FillPlayerIsSuspended(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_IS_INJURIED:
                            FillPlayerIsInjuried(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_IS_INTERNATIONAL:
                            FillPlayerIsInternational(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_INTERNATIONAL_SELECTIONS:
                            FillPlayerInternationalSelections(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_INTERNATIONAL_GOALS:
                            FillPlayerInternationalGoals(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_CONTRACT_BEGIN:
                            FillPlayerContractBegin(grid, player, row, columns.IndexOf(gcd));
                            break;
                        case GridColumn.PLAYER_CONTRACT_END:
                            FillPlayerContractEnd(grid, player, row, columns.IndexOf(gcd));
                            break;
                    }
                }

                row++;
            }
            spRanking.Children.Add(grid);
        }

        public void sortPlayers(PlayerAttribute attribute)
        {
            sortOrder = !sortOrder;
            Players.Sort(new PlayerComparator(sortOrder, attribute));
            Full(spRanking);
        }

    }
}
