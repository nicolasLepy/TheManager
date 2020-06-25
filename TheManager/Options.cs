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
        private List<Tournament> _tournamentsToExport;

        [DataMember]
        public bool ExportEnabled { get; set; }
        public List<Tournament> tournamentsToExport { get => _tournamentsToExport; }

        [DataMember]
        private bool _transfersEnabled;

        public bool transfersEnabled { get => _transfersEnabled; set => _transfersEnabled = value; }

        [DataMember]
        private bool _simulateGames;
        public bool simulateGames { get => _simulateGames; set => _simulateGames = value; }

        public Options()
        {
            ExportEnabled = false;
            _tournamentsToExport = new List<Tournament>();
            _transfersEnabled = false;
            _simulateGames = false;
        }


        

    }
}