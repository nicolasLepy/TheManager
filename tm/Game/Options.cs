using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace tm
{
    [DataContract(IsReference =true)]
    public class Options
    {

        [DataMember]
        private List<Tournament> _tournamentsToExport;

        public List<Tournament> tournamentsToExport { get => _tournamentsToExport; }

        [DataMember]
        private bool _transfersEnabled;

        public bool transfersEnabled { get => _transfersEnabled; set => _transfersEnabled = value; }

        [DataMember]
        private bool _simulateGames;
        public bool simulateGames { get => _simulateGames; set => _simulateGames = value; }

        [DataMember]
        private bool _reduceSaveSize;
        public bool reduceSaveSize { get => _reduceSaveSize; set => _reduceSaveSize = value; }


        public Options()
        {
            _tournamentsToExport = new List<Tournament>();
            _transfersEnabled = false;
            _simulateGames = false;
            _reduceSaveSize = false;
        }


        

    }
}