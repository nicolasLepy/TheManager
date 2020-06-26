using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Country : ILocalisation
    {
        [DataMember]
        private List<City> _cities;
        [DataMember]
        private List<Stadium> _stadiums;
        [DataMember]
        private Language _language;
        [DataMember]
        private List<Tournament> _tournaments;
        [DataMember]
        private string _name;

        public List<City> cities { get { return _cities; } }
        public List<Stadium> stadiums { get { return _stadiums; } }
        public Language language { get => _language; }

        public string Flag
        {
            get
            {
                string flag = _name;
                flag = flag.Replace(" ", "");
                flag = flag.ToLower();
                return flag;
            }
        }

        public Country(string name, Language language)
        {
            _name = name;
            _language = language;
            _cities = new List<City>();
            _stadiums = new List<Stadium>();
            _tournaments = new List<Tournament>();
        }

        public List<Tournament> Tournaments()
        {
            return _tournaments;
        }

        public string Name()
        {
            return _name;
        }

        public override string ToString()
        {
            return _name;
        }

    }
}