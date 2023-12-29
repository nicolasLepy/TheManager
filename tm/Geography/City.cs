using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace tm
{
    [DataContract(IsReference =true)]
    public class City
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Population { get; set; }
        [DataMember]
        public GeographicPosition Position { get; set; }

        private Country _country;

        public City(int id, string name, int population, float latitude, float longitude)
        {
            Id = id;
            _country = null;
            Name = name;
            Population = population;
            Position = new GeographicPosition(latitude, longitude);
        }

        public Country Country()
        {
            if(_country == null)
            {
                Country res = null;
                foreach (Continent c in Session.Instance.Game.kernel.world.continents)
                {
                    foreach (Country p in c.countries)
                    {
                        foreach (City v in p.cities)
                        {
                            if (v == this)
                            {
                                res = p;
                            }
                        }
                    }
                }
                _country = res;
            }
            return _country;
        }
    }
}