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
    public class Contract
    {
        [DataMember]
        private DateTime _beginning;
        [DataMember]
        private int _wage;
        [DataMember]
        private DateTime _end;
        [DataMember]
        private Player _player;

        [DataMember]
        [Key]
        public int Id { get; set; }

        public int wage => _wage;
        public DateTime end => _end;
        [DataMember]
        public bool isTransferable { get; set; }
        public Player player { get => _player; }
        public DateTime beginning { get => _beginning; }

        public Contract()
        {
            isTransferable = false;
        }

        public Contract(int id, Player player, int wage, DateTime end, DateTime begin)
        {
            Id = id;
            _player = player;
            _wage = wage;
            _end = end;
            isTransferable = false;
            _beginning = begin;
        }

        public void Update(int wage, DateTime end)
        {
            _wage = wage;
            _end = end;
        }
    }
}