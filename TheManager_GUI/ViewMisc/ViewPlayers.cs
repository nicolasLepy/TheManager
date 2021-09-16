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
        private readonly float FontSize;

        private List<Player> Players;

        private StackPanel spRanking;

        private bool sortOrder;

        public ViewPlayers(List<Player> players, float fontSize, bool age, bool position, bool nationality, bool level, bool potential, bool games, bool goals, bool condition, bool value, bool wage, bool isInjuried, bool isSuspended, bool isInternational, bool internationalSelections, bool internationalGoals)
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
            sortOrder = false;
        }

        public override void Full(StackPanel spRanking)
        {
            this.spRanking = spRanking;
            spRanking.Children.Clear();

            StackPanel spLine = new StackPanel();
            spLine.Orientation = Orientation.Horizontal;
            spLine.Children.Add(ViewUtils.CreateLabel("Nom", "StyleLabel2", FontSize, 100, null, sortPlayers, PlayerAttribute.NAME));

            if (Position)
                spLine.Children.Add(ViewUtils.CreateLabel("Pos", "StyleLabel2", FontSize, 30, null, sortPlayers, PlayerAttribute.POSITION));
            if (Age)
                spLine.Children.Add(ViewUtils.CreateLabel("Age", "StyleLabel2", FontSize, 60, null, sortPlayers, PlayerAttribute.AGE));
            if (Level)
                spLine.Children.Add(ViewUtils.CreateLabel("Glo", "StyleLabel2", FontSize, FontSize * 1.5f * 6, null, sortPlayers, PlayerAttribute.LEVEL));
            if (Potential)
                spLine.Children.Add(ViewUtils.CreateLabel("Pot", "StyleLabel2", FontSize, FontSize * 1.5f * 6, null, sortPlayers, PlayerAttribute.POTENTIAL));
            if (Condition)
                spLine.Children.Add(ViewUtils.CreateLabel("Cond.", "StyleLabel2", FontSize, 60, null, sortPlayers, PlayerAttribute.CONDITION));
            if (Nationality)
                spLine.Children.Add(ViewUtils.CreateLabel("Nat.", "StyleLabel2", FontSize, 40, null, sortPlayers, PlayerAttribute.NATIONALITY));
            if (Value)
                spLine.Children.Add(ViewUtils.CreateLabel("Value", "StyleLabel2", FontSize, 60, null, sortPlayers, PlayerAttribute.VALUE));
            if (Wage)
                spLine.Children.Add(ViewUtils.CreateLabel("Wage", "StyleLabel2", FontSize, 60, null, sortPlayers, PlayerAttribute.WAGE));
            if (IsInjuried)
                spLine.Children.Add(ViewUtils.CreateLabel("Inj.", "StyleLabel2", FontSize, 30, null, sortPlayers, PlayerAttribute.IS_INJURIED));
            if (Games)
                spLine.Children.Add(ViewUtils.CreateLabel("Games", "StyleLabel2", FontSize, 50, null, sortPlayers, PlayerAttribute.GAMES));
            if (Goals)
                spLine.Children.Add(ViewUtils.CreateLabel("Goals", "StyleLabel2", FontSize, 50, null, sortPlayers, PlayerAttribute.GOALS));
            if (IsSuspended)
                spLine.Children.Add(ViewUtils.CreateLabel("Sus", "StyleLabel2", FontSize, 30, null, sortPlayers, PlayerAttribute.IS_SUSPENDED));
            if (IsInternational)
                spLine.Children.Add(ViewUtils.CreateLabel("Int.", "StyleLabel2", FontSize, 30, null, sortPlayers, PlayerAttribute.IS_INTERNATIONAL));
            if (InternationalSelections)
                spLine.Children.Add(ViewUtils.CreateLabel("Int. S", "StyleLabel2", FontSize, 50, null, sortPlayers, PlayerAttribute.INTERNATIONAL_SELECTIONS));
            if (InternationalGoals)
                spLine.Children.Add(ViewUtils.CreateLabel("Int. G", "StyleLabel2", FontSize, 50, null, sortPlayers, PlayerAttribute.INTERNATIONAL_GOALS));

            spRanking.Children.Add(spLine);

            foreach (Player player in Players)
            {
                StackPanel spPlayer = new StackPanel();
                spPlayer.Orientation = Orientation.Horizontal;
                spPlayer.Children.Add(ViewUtils.CreateLabelOpenWindow<Player>(player, OpenPlayer, player.Name, "StyleLabel2", FontSize, 100));

                if (Position)
                    spPlayer.Children.Add(ViewUtils.CreateLabel(ViewUtils.PlayerPositionOneLetter(player), "StyleLabel2", FontSize, 30));
                if(Age)
                    spPlayer.Children.Add(ViewUtils.CreateLabel(player.Age + " ans", "StyleLabel2", FontSize, 60));
                if(Level)
                    spPlayer.Children.Add(ViewUtils.CreateStarNotation(player.Stars, FontSize * 1.5f));
                if (Potential)
                    spPlayer.Children.Add(ViewUtils.CreateStarNotation(player.StarsPotential, FontSize * 1.5f));
                if (Condition)
                    spPlayer.Children.Add(ViewUtils.CreateProgressBar(player.energy));
                if (Nationality)
                    spPlayer.Children.Add(ViewUtils.CreateFlag(player.nationality, 30, 15));
                if (Value)
                    spPlayer.Children.Add(ViewUtils.CreateLabel(ViewUtils.FormatMoney(player.EstimateTransferValue()), "StyleLabel2", FontSize, 60));
                if (Wage)
                    spPlayer.Children.Add(ViewUtils.CreateLabel(ViewUtils.FormatMoney(player.EstimateWage()) + "/m", "StyleLabel2", FontSize, 60));
                if (IsInjuried)
                    throw new NotImplementedException();
                if (Games)
                    spPlayer.Children.Add(ViewUtils.CreateLabel(player.playedGames.ToString(), "StyleLabel2", FontSize, 50));
                if (Goals)
                    spPlayer.Children.Add(ViewUtils.CreateLabel(player.goalsScored.ToString(), "StyleLabel2", FontSize, 50));
                if (IsSuspended)
                    throw new NotImplementedException();
                if (IsInternational)
                    throw new NotImplementedException();
                if (InternationalSelections)
                    throw new NotImplementedException();
                if (InternationalGoals)
                    throw new NotImplementedException();

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
