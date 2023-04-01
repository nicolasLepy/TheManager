using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TheManager
{

    [DataContract]
    public struct HistoricEntry : IEquatable<HistoricEntry>
    {
        [DataMember]
        public DateTime date { get; set; }
        [DataMember]
        public int budget { get; set; }
        [DataMember]
        public int formationFacilities { get; set; }
        [DataMember]
        public int averageAttendance { get; set; }
        [DataMember]
        public ClubStatus status { get; set; }

        public HistoricEntry(DateTime date, int budget, int formation, int averageAttendance, ClubStatus status)
        {
            this.date = date;
            this.budget = budget;
            this.averageAttendance = averageAttendance;
            this.formationFacilities = formation;
            this.status = status;
        }

        public bool Equals(HistoricEntry other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract(IsReference =true)]
    public class ClubHistory
    {
        [DataMember]
        private List<HistoricEntry> _elements;

        public List<HistoricEntry> elements { get => _elements; }

        public ClubHistory()
        {
            _elements = new List<HistoricEntry>();
        }
    }
}