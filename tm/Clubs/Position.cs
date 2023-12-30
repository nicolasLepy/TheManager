using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace tm
{
    public enum Position
    {
        Goalkeeper,
        Defender,
        Midfielder,
        Striker
    }

    [DataContract(IsReference = true)]
    public class GeographicPosition
    {
        [DataMember] private float _longitude;

        [DataMember] private float _latitude;

        public float Longitude
        {
            get => _longitude;
        }

        public float Latitude
        {
            get => _latitude;
        }

        public GeographicPosition()
        {

        }

        public GeographicPosition(float latitude, float longitude)
        {
            _longitude = longitude;
            _latitude = latitude;
        }

        public override string ToString()
        {
            return _longitude + ";" + _latitude;
        }
    }
}