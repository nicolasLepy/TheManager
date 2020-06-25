using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(Player))]
    [System.Xml.Serialization.XmlInclude(typeof(Player))]
    [KnownType(typeof(Manager))]
    [System.Xml.Serialization.XmlInclude(typeof(Manager))]
    public class Person
    {
        [DataMember]
        private string _lastName;
        [DataMember]
        private string _firstName;
        [DataMember]
        private DateTime _birthDay;
        [DataMember]
        private Country _nationality;


        public string lastName { get => _lastName; }
        public string firstName { get => _firstName; }
        public DateTime birthday { get => _birthDay; }
        public Country nationality { get => _nationality; }

        public int Age
        {
            get
            {
                DateTime date = Session.Instance.Game.date;
                int age = date.Year - birthday.Year;
                if (date.Month < birthday.Month)
                {
                    age--;
                }
                else if (date.Month == birthday.Month && date.Day < birthday.Day)
                {
                    age--;
                }
                return age;
            }
        }

        public Person(string firstName, string lastName, DateTime birthDay, Country nationality)
        {
            _firstName = firstName;
            _lastName = lastName;
            _birthDay = birthDay;
            _nationality = nationality;
        }

        public override string ToString()
        {
            return _firstName + " " + _lastName;
        }
    }
}