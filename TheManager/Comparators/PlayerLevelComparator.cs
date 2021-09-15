using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{

    public enum PlayerAttribute
    {
        NAME,
        POSITION,
        AGE,
        LEVEL,
        POTENTIAL,
        CONDITION,
        VALUE,
        WAGE,
        GOALS,
        GAMES,
        NATIONALITY,
        IS_SUSPENDED,
        IS_INJURIED,
        IS_INTERNATIONAL,
        INTERNATIONAL_SELECTIONS,
        INTERNATIONAL_GOALS
    }

    public class PlayerComparator : IComparer<Player>
    {

        private bool order;
        private PlayerAttribute attribute;

        public PlayerComparator(bool order, PlayerAttribute attribute)
        {
            this.order = order;
            this.attribute = attribute;
        }

        public int ComparePlayer(Player x, Player y)
        {
            switch (attribute)
            {
                case PlayerAttribute.NAME:
                    return x.Name.CompareTo(y.Name);
                case PlayerAttribute.POSITION:
                    return x.position - y.position;
                case PlayerAttribute.AGE:
                    return y.Age - x.Age;
                case PlayerAttribute.LEVEL:
                    return y.level - x.level;
                case PlayerAttribute.POTENTIAL:
                    return y.potential - x.potential;
                case PlayerAttribute.CONDITION:
                    return y.energy - x.energy;
                case PlayerAttribute.VALUE:
                    return y.EstimateTransferValue() - x.EstimateTransferValue();
                case PlayerAttribute.WAGE:
                    return y.EstimateWage() - x.EstimateWage();
                case PlayerAttribute.GOALS:
                    return y.goalsScored - x.goalsScored;
                case PlayerAttribute.GAMES:
                    return y.playedGames - x.playedGames;
                case PlayerAttribute.NATIONALITY:
                    return y.nationality.Name().CompareTo(x.nationality.Name());
                case PlayerAttribute.IS_SUSPENDED:
                    return (y.suspended && x.suspended) ? 1 : -1;
                case PlayerAttribute.IS_INJURIED:
                    throw new NotImplementedException();
                case PlayerAttribute.IS_INTERNATIONAL:
                    throw new NotImplementedException();
                case PlayerAttribute.INTERNATIONAL_SELECTIONS:
                    throw new NotImplementedException();
                case PlayerAttribute.INTERNATIONAL_GOALS:
                    throw new NotImplementedException();
                default:
                    return y.level - x.level;
            }
        }

        public int Compare(Player x, Player y)
        {
            return ComparePlayer(x, y) * (order ? 1 : -1);
        }


    }

}