using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tm
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

        [DataMember]
        [Key]
        public int Id { get; set; }
        public string Source => source;
        public int Min => min;
        public int Max => max;
        public AudioType Type => type;


        public AudioSource()
        {

        }

        public AudioSource(int id, string source, int min, int max, AudioType type)
        {
            this.Id = id;
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
