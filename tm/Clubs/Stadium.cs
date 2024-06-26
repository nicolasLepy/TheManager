﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace tm
{
    [DataContract(IsReference =true)]
    public class Stadium
    {

        [DataMember]
        [Key]
        public int Id { get; set; }
        [DataMember]
        private string _name;
        [DataMember]
        private City _city;

        [DataMember]
        public int capacity { get; set; }
        public string name { get => _name; }
        public City city { get => _city; set => _city = value; }

        public Stadium()
        {

        }

        public Stadium(int id, string name, int stadiumCapacity, City city)
        {
            Id = id;
            _name = name;
            capacity = stadiumCapacity;
            _city = city;
        }
    }
}