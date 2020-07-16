using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Journalist
    {

        [DataMember]
        private string _firstName;
        [DataMember]
        private string _lastName;
        [DataMember]
        private City _base;
        [DataMember]
        private int _offset;

        public string firstName { get => _firstName; }
        public string lastName { get => _lastName; }
        [DataMember]
        public int age { get; set; }
        [DataMember]
        public bool isTaken { get; set; }
        [DataMember]
        public bool isNationalReporter { get; set; }
        public City baseCity { get => _base; }
        public int offset { get => _offset; }

        public Media Media
        {
            get
            {
                Media res = null;
                foreach(Media m in Session.Instance.Game.kernel.medias)
                {
                    foreach (Journalist j in m.journalists)
                    {
                        if (j == this)
                        {
                            res = m;
                        }
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// Give the number of commented games by the journalist on the non-archived tournaments
        /// </summary>
        public int NumberOfCommentedGames
        {
            get
            {
                int res = 0;

                foreach(Match m in Session.Instance.Game.kernel.Matchs)
                {
                    foreach(KeyValuePair<Media, Journalist> j in m.journalists)
                    {
                        if (j.Value == this)
                        {
                            res++;
                        }
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// Create a journalist
        /// </summary>
        /// <param name="firstName">His first name</param>
        /// <param name="lastName">His last name</param>
        /// <param name="age">His age</param>
        /// <param name="baseCity">His assigned city</param>
        /// <param name="offset">His "offset" : he is more a replacement journalist</param>
        /// <param name="nationalReporter">Can intervene everywhere for prime time matches</param>
        public Journalist(string firstName, string lastName, int age, City baseCity, int offset, bool nationalReporter)
        {
            isTaken = false;
            _firstName = firstName;
            _lastName = lastName;
            this.age = age;
            this._base = baseCity;
            _offset = offset;
            isNationalReporter = nationalReporter;
        }

        public override string ToString()
        {
            return firstName + " " + lastName;
        }
    }
}