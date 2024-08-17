using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace tm
{
    [DataContract(IsReference =true)]
    public class Association
    {
        [DataMember]
        private List<Association> _divisions;
        [DataMember]
        private string _name;
        [DataMember]
        [Key]
        public int Id { get; set; }
        
        public List<Association> divisions => _divisions;
        public string name => _name;
        
        public Association()
        {
            _divisions = new List<Association>();
        }

        public Association(int id, string name)
        {
            Id = id;
            _name = name;
            _divisions = new List<Association>();
        }

        public int GetLevelOfAssociation(Association association, int currentLevel)
        {
            if(association == this)
            {
                return currentLevel;
            }
            else
            {
                int newLevel = -1;
                foreach(Association ad in _divisions)
                {
                    int adLevel = ad.GetLevelOfAssociation(association, currentLevel + 1);
                    if(adLevel != -1)
                    {
                        newLevel = adLevel;
                    }
                }
                return newLevel;
            }
        }

        public List<Association> GetAssociationsLevel(int level)
        {
            List<Association> res = new List<Association>();
            if (level == 1)
            {
                res = _divisions;
            }
            else
            {
                foreach (Association ad in _divisions)
                {
                    res.AddRange(ad.GetAssociationsLevel(level - 1));
                }
            }
            return res;
        }

        public bool ContainsAssociation(Association association)
        {
            bool res = this == association;

            if(!res)
            {
                foreach (Association adm in _divisions)
                {
                    if (adm.ContainsAssociation(association))
                    {
                        res = true;
                    }
                }
            }
            
            return res;
        }
        
        public Association GetAssociation(int id)
        {
            Association res = null;

            foreach (Association ad in divisions)
            {
                if (ad.Id == id)
                {
                    res = ad;
                }
            }
            
            return res;
        }
    }
}