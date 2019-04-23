using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Options
    {

        private List<Competition> _competitionsAExporter;

        public bool Exporter { get; set; }
        public List<Competition> CompetitionsAExporter { get => _competitionsAExporter; }

        public Options()
        {
            Exporter = false;
            _competitionsAExporter = new List<Competition>();
        }
        

    }
}