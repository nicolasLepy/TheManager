using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference = true)]
    public class Continent : ILocalisation
    {
        [DataMember]
        private List<Country> _countries;
        [DataMember]
        private List<Tournament> _tournaments;
        [DataMember]
        private string _name;
        [DataMember]
        private string _logo;

        public List<Country> countries => _countries;

        public string Name()
        {
            return _name;
        }

        public string Logo()
        {
            return _logo;
        }
        

        public Continent(string name, string logo)
        {
            _name = name;
            _logo = logo;
            _countries = new List<Country>();
        }


        public override string ToString()
        {
            return _name;
        }
    }
}