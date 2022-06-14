using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class AdministrativeDivision
    {
        [DataMember]
        private List<AdministrativeDivision> _divisions;
        [DataMember]
        private string _name;
        [DataMember]
        private int _id;
        
        public List<AdministrativeDivision> divisions => _divisions;
        public string name => _name;
        public int id => _id;
        
        public AdministrativeDivision(int id, string name)
        {
            _id = id;
            _name = name;
            _divisions = new List<AdministrativeDivision>();
        }

        public AdministrativeDivision GetAdministrativeDivision(int id)
        {
            AdministrativeDivision res = null;

            foreach (AdministrativeDivision ad in divisions)
            {
                if (ad.id == id)
                {
                    res = ad;
                }
            }
            
            return res;
        }
    }
}