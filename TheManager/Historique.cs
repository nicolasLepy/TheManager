using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{

    [DataContract]
    public struct EntreeHistorique
    {
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public int Budget { get; set; }
        [DataMember]
        public int CentreFormation { get; set; }

        public EntreeHistorique(DateTime date, int budget, int centreFormation)
        {
            Date = date;
            Budget = budget;
            CentreFormation = centreFormation;
        }
    }

    [DataContract(IsReference =true)]
    public class HistoriqueClub
    {
        [DataMember]
        private List<EntreeHistorique> _elements;

        public List<EntreeHistorique> Elements { get => _elements; }

        public HistoriqueClub()
        {
            _elements = new List<EntreeHistorique>();
        }
    }
}