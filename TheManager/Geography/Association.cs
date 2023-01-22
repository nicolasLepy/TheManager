using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    [DataContract(IsReference = true)]
    public class Association
    {

        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public ILocalisation localization { get; set; }

        [DataMember]
        private List<Association> _associations;

        public List<Association> associations => _associations;


        public Association(int id, string name, ILocalisation localization)
        {
            _associations = new List<Association>();
            this.id = id;
            this.name = name;
            this.localization = localization;
        }

    }
}