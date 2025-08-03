using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using tm.Tournaments;

namespace tm
{
    [DataContract(IsReference = true)]
    public class InternationalDates : IEquatable<InternationalDates>
    {
        [DataMember]
        private GameDay _start;
        [DataMember]
        private GameDay _end;
        [DataMember]
        private Tournament _tournament;
        [DataMember]
        private bool _currentlyCalled;

        public GameDay start => _start;
        public GameDay end => _end;

        public bool currentlyCalled { get => _currentlyCalled; set => _currentlyCalled = value; }

        public Tournament tournament => _tournament;

        public bool IsValid()
        {
            return tournament == null || tournament.IsCurrentlyPlaying(); // ((tournament.currentRound > -1) && (tournament.currentRound < tournament.rounds.Count - 1));
        }

        public bool IsEquals(InternationalDates obj)
        {
            return start == obj.start && end == obj.end && _tournament == tournament;
        }

        public int StartYear(int currentWeekNumber)
        {
            bool startIsNextYear = start.WeekNumber < (currentWeekNumber);
            return Session.Instance.Game.date.Year + (startIsNextYear ? 1 : 0);
        }

        public int EndYear(int currentWeekNumber)
        {
            bool endIsNextYear = end.WeekNumber < (currentWeekNumber - 2); //-1 because players are release 2 days after the _end week definition so probably the next week, to avoiding getting a day one year after
            return Session.Instance.Game.date.Year + (endIsNextYear ? 1 : 0);
        }

        public bool Equals(InternationalDates other)
        {
            throw new NotImplementedException();
        }

        public InternationalDates()
        {

        }

        public InternationalDates(GameDay start, GameDay end, Tournament tournament, bool currentlyCalled)
        {
            _start = start;
            _end = end;
            _tournament = tournament;
            _currentlyCalled = currentlyCalled;
            if (tournament != null)
            {
                _start = tournament.rounds.First().programmation.gamesDays.First();
                _end = tournament.rounds.Last().programmation.gamesDays.Last();
            }
        }
    }
}
