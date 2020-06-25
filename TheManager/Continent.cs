using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class Continent : IRecoverableTeams, ILocalisation
    {
        [DataMember]
        private List<Pays> _countries;
        [DataMember]
        private List<Tournament> _tournaments;
        [DataMember]
        private string _name;

        public List<Pays> countries => _countries;
        public List<Tournament> Tournaments()
        {
            return _tournaments;
        }

        public string Name()
        {
            return _name;
        }
        

        public Continent(string name)
        {
            _name = name;
            _countries = new List<Pays>();
            _tournaments = new List<Tournament>();
        }

        /// <summary>
        /// The parameter "onlyFirstTeams" is ignored because national teams are always first teams
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="methode"></param>
        /// <param name="onlyFirstTeams"></param>
        /// <returns></returns>
        public List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams)
        {
            List<Club> clubs = new List<Club>();
            foreach(Club c in Session.Instance.Partie.kernel.Clubs)
            {
                SelectionNationale sn = c as SelectionNationale;
                if(sn != null)
                {
                    if (_countries.Contains(sn.Pays)) clubs.Add(sn);
                }
            }
            List<Club> res = new List<Club>();
            if (method == RecuperationMethod.Best)
                clubs.Sort(new ClubLevelComparator());
            else if (method == RecuperationMethod.Worst)
                clubs.Sort(new ClubLevelComparator(true));
            else if (method == RecuperationMethod.Randomly)
                clubs = Utils.ShuffleList<Club>(clubs);

            for (int i = 0; i < number; i++) res.Add(clubs[i]);
            return res;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}