using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Tournaments
{
    [DataContract(IsReference =true)]
    public class GameDay
    {
        [DataMember]
        private int _weekNumber;
        [DataMember]
        private int _yearOffset;
        [DataMember]
        private bool _midWeekGame;
        [DataMember]
        private int _dayOffset;

        public GameDay(int weekNumber, bool midWeekGame, int yearOffset, int dayOffset)
        {
            _weekNumber = weekNumber;
            _yearOffset = yearOffset;
            _midWeekGame = midWeekGame;
            _dayOffset = dayOffset;
        }

        public int WeekNumber => _weekNumber;

        /// <summary>
        /// Year offset of game in case of multiple year tournament (world cup, continental ...)
        /// </summary>
        public int YearOffset => _yearOffset;

        public bool MidWeekGame => _midWeekGame;

        /// <summary>
        /// Day offset in case of standard games day is Friday for exemple
        /// </summary>
        public int DayOffset { get => _dayOffset; set => _dayOffset = value; }


        public DateTime ConvertToDateTime()
        {
            return ConvertToDateTime(Session.Instance.Game.date.Year);
        }

        public DateTime ConvertToDateTime(int year)
        {
            DateTime firstSaturday = new DateTime(year + _yearOffset, 1, 1, 0, 0, 0);
            switch (firstSaturday.DayOfWeek)
            {
                case DayOfWeek.Thursday:
                    firstSaturday = firstSaturday.AddDays(2);
                    break;
                case DayOfWeek.Friday:
                    firstSaturday = firstSaturday.AddDays(3);
                    break;
                case DayOfWeek.Saturday:
                    firstSaturday = firstSaturday.AddDays(4);
                    break;
                case DayOfWeek.Sunday:
                    firstSaturday = firstSaturday.AddDays(5);
                    break;
                case DayOfWeek.Monday:
                    firstSaturday = firstSaturday.AddDays(6);
                    break;
                case DayOfWeek.Tuesday:
                    firstSaturday = firstSaturday.AddDays(7);
                    break;
                case DayOfWeek.Wednesday:
                    firstSaturday = firstSaturday.AddDays(8);
                    break;
            }
            firstSaturday = firstSaturday.AddDays(((_weekNumber - 1) * 7) - 1);
            if(_midWeekGame)
            {
                firstSaturday = firstSaturday.AddDays(4);
            }
            firstSaturday = firstSaturday.AddDays(DayOffset);
            return firstSaturday;
        }

    }
}