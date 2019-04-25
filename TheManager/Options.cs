using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Options
    {

        [DataMember]
        private List<Competition> _competitionsAExporter;

        [DataMember]
        public bool Exporter { get; set; }
        public List<Competition> CompetitionsAExporter { get => _competitionsAExporter; }

        public Options()
        {
            Exporter = false;
            _competitionsAExporter = new List<Competition>();
        }
        

    }
}