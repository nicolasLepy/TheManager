using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tm.Tournaments
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

            DateTime jan1 = new DateTime(year + _yearOffset, 1, 1);
            int daysOffset = DayOfWeek.Tuesday - jan1.DayOfWeek;

            DateTime firstMonday = jan1.AddDays(daysOffset);

            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            int weekNum = _weekNumber;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            DateTime res = firstMonday.AddDays(weekNum * 7 + 5 - 1);
            if (_midWeekGame)
            {
                res = res.AddDays(4);
            }
            res = res.AddDays(_dayOffset);
            return res;
            
        }
    }
}