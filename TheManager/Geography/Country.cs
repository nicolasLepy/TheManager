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
        private List<Tournament> _tournaments;
        [DataMember]
        private string _dbName;
        [DataMember]
        private string _name;
        [DataMember]
        private int _shapeNumber;
        [DataMember]
        private List<AdministrativeDivision> _administrativeDivisions;

        public List<City> cities { get { return _cities; } }
        public List<Stadium> stadiums { get { return _stadiums; } }
        public Language language { get => _language; }

        public List<AdministrativeDivision> administrativeDivisions => _administrativeDivisions;
        
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
            for (int i = 1; i < 4; i++)
            {
                Tournament continentalTournament = Continent.GetContinentalClubTournament(i);
                if(continentalTournament != null)
                {
                    int j = continentalTournament.previousEditions.Count - (-nSeason);

                    if (j >= 0)
                    {
                        Tournament yearContinentalTournament = continentalTournament.previousEditions.ToList()[j].Value;
                        foreach (Tournament championship in _tournaments)
                        {
                            if (championship.isChampionship)
                            {
                                foreach (Club c in championship.rounds[0].clubs)
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
            _administrativeDivisions = new List<AdministrativeDivision>();
        }

        public AdministrativeDivision GetCountryAdministrativeDivision()
        {
            AdministrativeDivision res = null;
            foreach (AdministrativeDivision ad in _administrativeDivisions)
            {
                if (ad.name == this._name)
                {
                    res = ad;
                }
            }

            return res;
        }
        
        public AdministrativeDivision GetAdministrativeDivision(int id)
        {
            AdministrativeDivision res = null;
            foreach (AdministrativeDivision ad in _administrativeDivisions)
            {
                if (ad.id == id)
                {
                    res = ad;
                }

                AdministrativeDivision resChild = ad.GetAdministrativeDivision(id);
                if (resChild != null)
                {
                    res = resChild;
                }
            }
            return res;
        }
        
        public List<Tournament> Tournaments()
        {
            return _tournaments;
        }

        private Tournament GetTournamentByLevel(int rank, bool isChampionship)
        {
            Tournament res = null;

            foreach(Tournament t in Tournaments())
            {
                if(t.isChampionship == isChampionship && t.level == rank)
                {
                    res = t;
                }
            }

            return res;   
            
        }
        
        public Tournament League(int leagueRank)
        {
            return GetTournamentByLevel(leagueRank, true);
        }

        /**
         * cupRank : for exemple : Coupe de France is level 1 and Coupe de la Ligue is level 2
         */
        public Tournament Cup(int cupRank)
        {
            return GetTournamentByLevel(cupRank, false);
        }

        /**
         * Get last league with a national level, then league is subdivised by groups
         */
        public Tournament GetLastNationalLeague()
        {
            int res = -1;
            foreach (Tournament t in Tournaments())
            {
                GroupsRound gr = t.rounds[0] as GroupsRound;
                if (gr != null && gr.RandomDrawingMethod != RandomDrawingMethod.Administrative && t.level > res)
                {
                    res = t.level;
                }
            }

            return League(res);
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