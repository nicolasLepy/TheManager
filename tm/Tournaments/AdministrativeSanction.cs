using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tm
{

    public enum SanctionType
    {
        EnteringAdministration,
        Forfeit,
        IneligiblePlayer,
        FinancialIrregularities
    }

    [DataContract]
    public struct AdministrativeSanction
    {
        [DataMember]
        private SanctionType _type;
        [DataMember]
        private int _minPointsDeduction;
        [DataMember]
        private int _maxPointsDeduction;
        [DataMember]
        private int _minRetrogradation;
        [DataMember]
        private int _maxRetrogradation;

        public SanctionType type => _type;
        public int minPointsDeduction => _minPointsDeduction;
        public int maxPointsDeduction => _maxPointsDeduction;
        public int minRetrogradation => _minRetrogradation;
        public int maxRetrogradation => _maxRetrogradation;

        public AdministrativeSanction(SanctionType type, int minPointsDeduction, int maxPointsDeduction, int minRetrogradation, int maxRetrogradation)
        {
            _type = type;
            _minPointsDeduction = minPointsDeduction;
            _maxPointsDeduction = maxPointsDeduction;
            _minRetrogradation = minRetrogradation;
            _maxRetrogradation = maxRetrogradation;
        }

    }
}
