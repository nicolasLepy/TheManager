using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(Continent))]
    [System.Xml.Serialization.XmlInclude(typeof(Continent))]
    [KnownType(typeof(Country))]
    [System.Xml.Serialization.XmlInclude(typeof(Country))]
    public abstract class Localisation
    {

        [DataMember]
        private GeographicPosition _position;
        [DataMember]
        private float _range;

        public float Range { get => _range; }
        public GeographicPosition Position { get => _position; }

        public Localisation(float latitude, float longitude, float range)
        {
            _position = new GeographicPosition(latitude, longitude);
            _range = range;
        }

        public abstract List<Tournament> Tournaments();
        public abstract string Name();
    }
}
