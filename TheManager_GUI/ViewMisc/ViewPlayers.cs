using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TheManager;
using TheManager.Comparators;
using TheManager_GUI.VueClassement;

namespace TheManager_GUI.ViewMisc
{
    public class ViewPlayers : View
    {

        private readonly bool Age;
        private readonly bool Position;
        private readonly bool Nationality;
        private readonly bool Level;
        private readonly bool Potential;
        private readonly bool Games;
        private readonly bool Goals;
        private readonly bool Condition;
        private readonly bool Value;
        private readonly bool Wage;
        private readonly bool IsSuspended;
        private readonly bool IsInjuried;
        private readonly bool IsInternational;
        private readonly bool InternationalSelections;
        private readonly bool InternationalGoals;
        private readonly bool ContractBegin;
        private readonly bool ContractEnd;
        private readonly float FontSize;
        private readonly bool LevelsInNumbers;

        private List<Player> Players;

        private StackPanel spRanking;

        private bool sortOrder;

        public ViewPlayers(List<Player> players, float fontSize, bool age, bool position, bool nationality, bool level, bool potential, bool games, bool goals, bool condition, bool value, bool wage, bool isInjuried, bool isSuspended, bool isInternational, bool internationalSelections, bool internationalGoals, bool contractBegin, bool contractEnd, bool levelsInNumbers = false)
        {
            Age = age;
            Position = position;
            Nationality = nationality;
            Level = level;
            Potential = potential;
            Games = games;
            Goals = goals;
            Condition = condition;
            Value = value;
            Wage = wage;
            IsSuspended = isSuspended;
            IsInjuried = isInjuried;
            IsInternational = isInternational;
            InternationalSelections = internationalSelections;
            InternationalGoals = internationalGoals;
            Players = players;
            FontSize = fontSize;
            ContractBegin = contractBegin;
            ContractEnd = contractEnd;
            sortOrder = false;
            LevelsInNumbers = levelsInNumbers;
        }

        public override void Full(StackPanel spRanking)
        {
            this.spRanking = spRanking;
            spRanking.Children.Clear();

            StackPanel spLine = new StackPanel();
            spLine.Orientation = Orientation.Horizontal;
            spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.NAME, sortPlayers, "Nom", "StyleLabel2", FontSize, 100));

            if (Position)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.POSITION, sortPlayers, "Pos", "StyleLabel2", FontSize, 30));
            }
            if (Age)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.AGE, sortPlayers, "Age", "StyleLabel2", FontSize, 60));
            }
            if (Level)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.LEVEL, sortPlayers, "GLO", "StyleLabel2", FontSize, LevelsInNumbers ? 40 : FontSize * 1.5f * 6));
            }
            if (Potential)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.POTENTIAL, sortPlayers, "Pot", "StyleLabel2", FontSize, LevelsInNumbers ? 40 : FontSize * 1.5f * 6));

            }
            if (Condition)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.CONDITION, sortPlayers, "Cond.", "StyleLabel2", FontSize, 40));
            }
            if (Nationality)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.NATIONALITY, sortPlayers, "Nat.", "StyleLabel2", FontSize, 40));
            }
            if (Value)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.VALUE, sortPlayers, "Value", "StyleLabel2", FontSize, 60));
            }
            if (Wage)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.WAGE, sortPlayers, "Wage", "StyleLabel2", FontSize, 60));
            }
            if (ContractBegin)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.CONTRACT_BEGIN, sortPlayers, "Begin", "StyleLabel2", FontSize, 80));
            }
            if (ContractEnd)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.CONTRACT_END, sortPlayers, "End", "StyleLabel2", FontSize, 80));
            }
            if (IsInjuried)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.IS_INJURIED, sortPlayers, "Inj.", "StyleLabel2", FontSize, 30));
            }
            if (Games)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.GAMES, sortPlayers, "Games", "StyleLabel2", FontSize, 50));
            }
            if (Goals)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.GOALS, sortPlayers, "Goals", "StyleLabel2", FontSize, 50));
            }
            if (IsSuspended)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.IS_SUSPENDED, sortPlayers, "Sus", "StyleLabel2", FontSize, 30));
            }
            if (IsInternational)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.IS_INTERNATIONAL, sortPlayers, "Int.", "StyleLabel2", FontSize, 30));
            }
            if (InternationalSelections)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.INTERNATIONAL_SELECTIONS, sortPlayers, "Int. S", "StyleLabel2", FontSize, 50));
            }
            if (InternationalGoals)
            {
                spLine.Children.Add(ViewUtils.CreateLabelOpenWindow<PlayerAttribute>(PlayerAttribute.INTERNATIONAL_GOALS, sortPlayers, "Int. G", "StyleLabel2", FontSize, 50));
            }

            spRanking.Children.Add(spLine);

            foreach (Player player in Players)
            {
                StackPanel spPlayer = new StackPanel();
                spPlayer.Orientation = Orientation.Horizontal;
                spPlayer.Children.Add(ViewUtils.CreateLabelOpenWindow<Player>(player, OpenPlayer, player.Name, "StyleLabel2", FontSize, 100));

                if (Position)
                {
                    spPlayer.Children.Add(ViewUtils.CreateLabel(ViewUtils.PlayerPositionOneLetter(player), "StyleLabel2", FontSize, 30));
                }
                if (Age)
                {
                    spPlayer.Children.Add(ViewUtils.CreateLabel(player.Age + " ans", "StyleLabel2", FontSize, 60));
                }
                if (Level)
                {
                    if(!LevelsInNumbers)
                    {
                        spPlayer.Children.Add(ViewUtils.CreateStarNotation(player.Stars, FontSize * 1.5f));
                    }
                    else
                    {
                        spPlayer.Children.Add(ViewUtils.CreateLabel(player.level.ToString(), "StyleLabel2", FontSize, 40, null, null, true));
                    }
                }
                if (Potential)
                {
                    if (!LevelsInNumbers)
                    {
                        spPlayer.Children.Add(ViewUtils.CreateStarNotation(player.StarsPotential, FontSize * 1.5f));
                    }
                    else
                    {
                        spPlayer.Children.Add(ViewUtils.CreateLabel(player.potential.ToString(), "StyleLabel2", FontSize, 40));
                    }
                }
                if (Condition)
                {
                    ProgressBar pb = ViewUtils.CreateProgressBar(player.energy, 0, 100, 40, 10);
                    spPlayer.Children.Add(pb);
                }
                if (Nationality)
                {
                    spPlayer.Children.Add(ViewUtils.CreateFlag(player.nationality, 30, 15));
                }
                if (Value)
                {
                    spPlayer.Children.Add(ViewUtils.CreateLabel(Utils.FormatMoney(player.EstimateTransferValue()), "StyleLabel2", FontSize, 60));
                }
                if (Wage)
                {
                    string wageStr = player.Club != null ? Utils.FormatMoney(player.Club.FindContract(player).wage) + "/m" : "-";
                    spPlayer.Children.Add(ViewUtils.CreateLabel(wageStr, "StyleLabel2", FontSize, 60));
                }
                if(ContractBegin)
                {
                    string contractBegin = player.Club != null ? player.Club.FindContract(player).beginning.ToShortDateString() : "-";
                    spPlayer.Children.Add(ViewUtils.CreateLabel(contractBegin, "StyleLabel2", FontSize, 80));
                }
                if (ContractEnd)
                {
                    string contractEnd = player.Club != null ? player.Club.FindContract(player).beginning.ToShortDateString() : "-";
                    spPlayer.Children.Add(ViewUtils.CreateLabel(contractEnd, "StyleLabel2", FontSize, 80));

                }
                if (IsInjuried)
                {
                    throw new NotImplementedException();
                }
                if (Games)
                {
                    spPlayer.Children.Add(ViewUtils.CreateLabel(player.playedGames.ToString(), "StyleLabel2", FontSize, 50));
                }
                if (Goals)
                {
                    spPlayer.Children.Add(ViewUtils.CreateLabel(player.goalsScored.ToString(), "StyleLabel2", FontSize, 50));
                }
                if (IsSuspended)
                {
                    throw new NotImplementedException();
                }
                if (IsInternational)
                {
                    throw new NotImplementedException();
                }
                if (InternationalSelections)
                {
                    throw new NotImplementedException();
                }
                if (InternationalGoals)
                {
                    throw new NotImplementedException();
                }

                spRanking.Children.Add(spPlayer);
            }
        }

        public void sortPlayers(PlayerAttribute attribute)
        {
            sortOrder = !sortOrder;
            Players.Sort(new PlayerComparator(sortOrder, attribute));
            Full(spRanking);
        }

        public override void Show()
        {
            
        }
    }
}
