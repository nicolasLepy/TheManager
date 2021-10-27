using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
        private List<Tournament> _tournaments;
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

        public float YearAssociationCoefficient(int nSeason)
        {
            List<Club> clubs = new List<Club>();
            float total = 0;
            Continent europe = Session.Instance.Game.kernel.String2Continent("Europe");
            for (int i = 1; i < 3; i++)
            {
                Tournament continentalTournament = europe.GetContinentalClubTournament(i);
                int j = continentalTournament.previousEditions.Count - (-nSeason);

                if (j >= 0)
                {
                    
                    Tournament yearContinentalTournament = continentalTournament.previousEditions.ToList()[j].Value;
                    foreach(Tournament championship in _tournaments)
                    {
                        if(championship.isChampionship)
                        {
                            foreach(Club c in championship.rounds[0].clubs)
                            {
                                if (yearContinentalTournament.IsInvolved(c))
                                {
                                    clubs.Add(c);
                                }
                            }
                        }
                    }
                }
            }
            foreach(Club c in clubs)
            {
                float clubCoefficient = c.ClubYearCoefficient(nSeason, true);
                total += clubCoefficient;
            }

            return clubs.Count > 0 ? total / clubs.Count : 0;
        }

        public float AssociationCoefficient
        {
            get
            {
                float res = 0;
                for(int i = -5; i < 0; i++)
                {
                    res += YearAssociationCoefficient(i);
                }
                return res;
            }
        }

        public Country(string dbName, string name, Language language, int shapeNumber)
        {
            _dbName = dbName;
            _name = name;
            _language = language;
            _cities = new List<City>();
            _stadiums = new List<Stadium>();
            _tournaments = new List<Tournament>();
            _shapeNumber = shapeNumber;
        }

        public List<Tournament> Tournaments()
        {
            return _tournaments;
        }

        public Tournament FirstDivisionChampionship()
        {
            Tournament res = null;
            foreach(Tournament t in _tournaments)
            {
                if(t.isChampionship && t.level == 1)
                {
                    res = t;
                }
            }
            return res;
        }

        public override string ToString()
        {
            return _name;
        }

        public string Name()
        {
            return _name;
        }

        public Continent Continent
        {
            get
            {
                Continent res = null;
                foreach(Continent c in Session.Instance.Game.kernel.continents)
                {
                    foreach(Country cy in c.countries)
                    {
                        if(cy == this)
                        {
                            res = c;
                        }
                    }
                }
                return res;
            }
        }
    }
}