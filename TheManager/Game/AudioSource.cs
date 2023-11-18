using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{

    public enum AudioType
    {
        Background,
        Event
    }

    [DataContract(IsReference = true)]
    public class AudioSource
    {
        [DataMember]
        private string source;
        [DataMember]
        private int min;
        [DataMember]
        private int max;
        [DataMember]
        private AudioType type;

        public string Source => source;
        public int Min => min;
        public int Max => max;
        public AudioType Type => type;

        public AudioSource(string source, int min, int max, AudioType type)
        {
            this.source = source;
            this.min = min;
            this.max = max;
            this.type = type;
        }

        public string getPath()
        {
            string root = "";
            switch (type)
            {
                case AudioType.Background:
                    root = "background";
                    break;
                case AudioType.Event:
                    root = "event";
                    break;
            }

            return String.Format("{0}{1}{2}", root, Path.DirectorySeparatorChar, source);
        }

    }
}
