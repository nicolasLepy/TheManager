﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace tm
{
    [DataContract(IsReference = true)]
    public class Article
    {

        [DataMember]
        private int _id;

        [DataMember]
        private string _title;
        [DataMember]
        private string _content;
        [DataMember]
        private DateTime _publication;
        [DataMember]
        private int _importance;

        public int id => _id;
        public string title { get => _title; }
        public string content { get => _content; }
        public DateTime publication { get => _publication; }
        public int importance { get => _importance; }

        public Article(int id, string title, string content, DateTime publication, int importance)
        {
            _id = id;
            _title = title;
            _content = content;
            _publication = publication;
            _importance = importance;
        }

    }
}