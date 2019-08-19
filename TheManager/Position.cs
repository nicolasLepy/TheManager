using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference = true)]
    public class Position
    {
        [DataMember]
        private float _longitude;

        [DataMember]
        private float _latitude;

        public float Longitude { get => _longitude; }
        public float Latitude { get => _latitude; }

        public Position(float latitude, float longitude)
        {
            _longitude = longitude;
            _latitude = latitude;
        }
    }
}