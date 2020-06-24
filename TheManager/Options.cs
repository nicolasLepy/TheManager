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
        private List<Tournament> _competitionsAExporter;

        [DataMember]
        public bool Exporter { get; set; }
        public List<Tournament> CompetitionsAExporter { get => _competitionsAExporter; }

        [DataMember]
        private bool _transferts;

        public bool Transferts { get => _transferts; set => _transferts = value; }

        [DataMember]
        private bool _simulerMatchs;
        public bool SimulerMatchs { get => _simulerMatchs; set => _simulerMatchs = value; }

        public Options()
        {
            Exporter = false;
            _competitionsAExporter = new List<Tournament>();
            _transferts = false;
            _simulerMatchs = false;
        }


        

    }
}