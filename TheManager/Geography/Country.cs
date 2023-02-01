using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Country : ILocalisation
    {
        [DataMember]
        private List<City> _cities;
        [DataMember]
        private List<Stadium> _stadiums;
        [DataMember]
        private Language _language;
        [DataMember]
        private string _dbName;
        [DataMember]
        private string _name;
        [DataMember]
        private int _shapeNumber;
        public List<City> cities { get { return _cities; } }
        public List<Stadium> stadiums { get { return _stadiums; } }
        public Language language { get => _language; }
        
        public string Flag
        {
            get
            {
                string flag = _name;
                flag = flag.ToLower();
                flag = flag.Replace(" ", "");
                flag = flag.Replace("î", "i");
                flag = flag.Replace("é", "e");
                flag = flag.Replace("è", "e");
                flag = flag.Replace("ê", "e");
                flag = flag.Replace("ô", "o");
                flag = flag.Replace("ö", "o");
                flag = flag.Replace("ï", "i");
                flag = flag.Replace("ë", "e");
                flag = flag.Replace("à", "a");
                flag = flag.Replace("ä", "a");
                flag = flag.Replace("-", "");
                return flag;
            }
        }

        public string DbName { get => _dbName; }
        public int ShapeNumber { get => _shapeNumber; }

        public Country(string dbName, string name, Language language, int shapeNumber)
        {
            _dbName = dbName;
            _name = name;
            _language = language;
            _cities = new List<City>();
            _stadiums = new List<Stadium>();
            _shapeNumber = shapeNumber;
        }


        public override string ToString()
        {
            return _name;
        }

        public string Name()
        {
            return _name;
        }

        public Association GetAssociation()
        {
            return Session.Instance.Game.kernel.worldAssociation.GetAssociationOfLocalizable(this);
        }
        
     

    }
}