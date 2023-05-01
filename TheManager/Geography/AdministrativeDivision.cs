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

        public int GetLevelOfAdministrativeDivision(AdministrativeDivision association, int currentLevel)
        {
            if(association == this)
            {
                return currentLevel;
            }
            else
            {
                int newLevel = -1;
                foreach(AdministrativeDivision ad in _divisions)
                {
                    int adLevel = ad.GetLevelOfAdministrativeDivision(association, currentLevel + 1);
                    if(adLevel != -1)
                    {
                        newLevel = adLevel;
                    }
                }
                return newLevel;
            }
        }

        public List<AdministrativeDivision> GetAdministrativeDivisionsLevel(int level)
        {
            List<AdministrativeDivision> res = new List<AdministrativeDivision>();
            if (level == 1)
            {
                res = _divisions;
            }
            else
            {
                foreach (AdministrativeDivision ad in _divisions)
                {
                    res.AddRange(ad.GetAdministrativeDivisionsLevel(level - 1));
                }
            }
            return res;
        }

        public bool ContainsAdministrativeDivision(AdministrativeDivision administrativeDivision)
        {
            bool res = this == administrativeDivision;

            if(!res)
            {
                foreach (AdministrativeDivision adm in _divisions)
                {
                    if (adm.ContainsAdministrativeDivision(administrativeDivision))
                    {
                        res = true;
                    }
                }
            }
            
            return res;
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