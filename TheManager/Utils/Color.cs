using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{

    [DataContract]
    public struct Color : IEquatable<Color>
    {

        [DataMember]
        public byte red { get; set; }
        [DataMember]
        public byte green { get; set; }
        [DataMember]
        public byte blue { get; set; }

        public Color(byte r, byte g, byte b)
        {
            red = r;
            green = g;
            blue = b;
        }

        public string ToHexa()
        {
            return String.Format("#{0:X2}{1:X2}{2:X2}", red, green, blue);
        }

        /// <summary>
        /// No sense to compare two colors in our project
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Color other)
        {
            return false;
        }
    }
}
