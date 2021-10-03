using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{

    [DataContract]
    public struct TournamentCoverage : IEquatable<TournamentCoverage>
    {
        [DataMember]
        public Tournament Tournament { get; set; }
        [DataMember]
        public int BeginIndex { get; set; }
        [DataMember]
        public int MinimumGamesNumberOfMultiplex { get; set; }
        [DataMember]
        public int GamesNumberByMultiplex { get; set; }
        [DataMember]
        public int MinimumLevel { get; set; }

        public TournamentCoverage(Tournament tournament, int beginIndex, int minimumGamesNumberOfMultiplex, int gamesNumberByMultiplex, int minimumLevel)
        {
            Tournament = tournament;
            BeginIndex = beginIndex;
            MinimumGamesNumberOfMultiplex = minimumGamesNumberOfMultiplex;
            GamesNumberByMultiplex = gamesNumberByMultiplex;
            MinimumLevel = minimumLevel;
        }

        public bool Equals(TournamentCoverage other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract(IsReference =true)]
    public class Media
    {
        [DataMember]
        private string _name;
        [DataMember]
        private List<Journalist> _journalists;
        [DataMember]
        private List<TournamentCoverage> _coverages;
        [DataMember]
        private Country _country;

        public string name { get => _name; }
        public List<Journalist> journalists { get => _journalists; }
        public List<TournamentCoverage> coverages { get => _coverages; }
        public Country country { get => _country; }

        public Media(string name, Country country)
        {
            _name = name;
            _journalists = new List<Journalist>();
            _coverages = new List<TournamentCoverage>();
            _country = country;
        }

        /// <summary>
        /// If a media cover a tournament's round
        /// </summary>
        /// <param name="c">The tournament</param>
        /// <param name="roundIndex">The index to the round</param>
        /// <returns>True if yes, False it not</returns>
        public bool Cover(Tournament c, int roundIndex)
        {
            bool res = false;
            foreach(TournamentCoverage cc in coverages)
            {
                if (cc.Tournament == c && cc.BeginIndex <= roundIndex)
                {
                    res = true;
                }
            }
            return res;
        }

        public TournamentCoverage GetCoverage(Tournament tournament)
        {
            TournamentCoverage res = _coverages[0];

            foreach(TournamentCoverage c in _coverages)
            {
                if (c.Tournament == tournament)
                {
                    res = c;
                }
            }

            return res;
        }

        public void FreeJournalists()
        {
            foreach (Journalist j in _journalists)
            {
                j.isTaken = false;
            }
        }

        public Journalist GetNationalJournalist()
        {
            Journalist res = null;
            List<Journalist> list = new List<Journalist>();
            foreach(Journalist j in journalists)
            {
                if (j.isNationalReporter && !j.isTaken)
                {
                    list.Add(j);
                }
            }
            list.Sort(new JournalistOffsetComparator());
            if (list.Count > 0)
            {
                res = list[0];
            }
            return res;

        }

        public Journalist GetJournalist(City city)
        {
            List<Journalist> availableJournalists = new List<Journalist>();
            foreach (Journalist j in journalists)
            {
                if (!j.isTaken)
                {
                    availableJournalists.Add(j);
                }
            }
            Journalist journalist = null;
            if (availableJournalists.Count > 0)
            {
                availableJournalists.Sort(new JournalistsComparator(city));

                if (Math.Abs(Utils.Distance(availableJournalists[0].baseCity, city)) < 300)
                {
                    journalist = availableJournalists[0];
                }
            }
            if (journalist == null)
            {
                //TODO: Search in unemployed journalists
                Journalist newJournalist = new Journalist(country.language.GetFirstName(), country.language.GetLastName(), Session.Instance.Random(28, 60), city, 100, false);
                journalists.Add(newJournalist);
                journalist = newJournalist;
            }
            journalist.isTaken = true;
            return journalist;
        }

    }
}