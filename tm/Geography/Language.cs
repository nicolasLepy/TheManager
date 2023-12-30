using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace tm
{
    [DataContract(IsReference =true)]
    public class Language
    {
        [DataMember]
        private string _name;
        [DataMember]
        private List<string> _firstNames;
        [DataMember]
        private List<string> _lastNames;

        public string name { get => _name; }

        public Language()
        {
            _firstNames = new List<string>();
            _lastNames = new List<string>();
        }

        public Language(string name)
        {
            _name = name;
            _firstNames = new List<string>();
            _lastNames = new List<string>();
        }

        public void AddFirstName(string firstName)
        {
            _firstNames.Add(firstName);
        }

        public void AddLastName(string lastName)
        {
            _lastNames.Add(lastName);
        }
        
        public string GetFirstName()
        {
            return _firstNames[Session.Instance.Random(0,_firstNames.Count)];
        }

        public string GetLastName()
        {
            return _lastNames[Session.Instance.Random(0, _lastNames.Count)];
        }

    }
}