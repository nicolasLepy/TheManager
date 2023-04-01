using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Stadium
    {

        [DataMember]
        private string _name;
        [DataMember]
        private City _city;

        [DataMember]
        public int capacity { get; set; }
        public string name { get => _name; }
        public City city { get => _city; set => _city = value; }

        public Stadium(string name, int stadiumCapacity, City city)
        {
            _name = name;
            capacity = stadiumCapacity;
            _city = city;
        }
    }
}