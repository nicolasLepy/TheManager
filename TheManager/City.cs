using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class City
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Population { get; set; }
        [DataMember]
        public GeographicPosition Position { get; set; }

        private Country _country;

        public City(string name, int population, float latitude, float longitude)
        {
            _country = null;
            Name = name;
            Population = population;
            Position = new GeographicPosition(latitude, longitude);
        }

        public Country Country()
        {
            if(_country == null)
            {
                return Session.Instance.Game.kernel.City2Country(this);
            }
            return _country;
        }
    }
}