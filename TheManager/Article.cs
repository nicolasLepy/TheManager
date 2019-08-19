using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference = true)]
    public class Article
    {

        [DataMember]
        private string _titre;
        [DataMember]
        private string _contenu;
        [DataMember]
        private DateTime _publication;
        [DataMember]
        private int _importance;

        public string Titre { get => _titre; }
        public string Contenu { get => _contenu; }
        public DateTime Publication { get => _publication; }
        public int Importance { get => _importance; }

        public Article(string titre, string contenu, DateTime publication, int importance)
        {
            _titre = titre;
            _contenu = contenu;
            _publication = publication;
            _importance = importance;
        }

    }
}