using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace tm
{
    [DataContract(IsReference =true)]
    public class Manager : Person
    {

        [DataMember]
        private int _level;

        /// <summary>
        /// Level of the manager.
        /// Corresponds to the level of the teams he is capable to coach
        /// </summary>
        public int level { get => _level; }

        public Manager() : base()
        {

        }

        public Manager(int id, string firstName, string lastName, int level, DateTime birthDay, Country nationality) : base(id, firstName,lastName,birthDay, nationality)
        {
            _level = level;

        }

        /// <summary>
        /// Make evolve the manager
        /// </summary>
        public void Evolve()
        {
            _level += Session.Instance.Random(0, 4);
        }

    }
}