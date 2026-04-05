using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm
{

    /// <summary>
    /// Represent 
    /// </summary>
    public class DummyExternalSource : IRecoverableTeams
    {

        private int count;
        public DummyExternalSource(int count)
        {
            this.count = count;
        }

        public int CountWithoutReserves()
        {
            throw new NotImplementedException();
        }

        public List<Club> RetrieveTeams(int number, RetrieveFlags method, bool onlyFirstTeams, Association associationFilter)
        {
            List<Club> res = new List<Club>();
            if (number == -1)
            {
                for (int i = 0; i < count; i++)
                {
                    res.Add(null);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return res;
        }

        public bool IsDummy()
        {
            return true;
        }
    }

    public interface IRecoverableTeams
    {
        /// <summary>
        /// Get teams from a source (a round or a continent)
        /// </summary>
        /// <param name="number">Number of teams to retrieve</param>
        /// <param name="method">How to select these teams</param>
        /// <param name="onlyFirstTeams">Remove reserves teams from selection</param>
        /// <param name="associationFilter">Only select teams from a defined region/district if not null</param>
        /// <returns></returns>
        List<Club> RetrieveTeams(int number, RetrieveFlags method, bool onlyFirstTeams, Association associationFilter);
        int CountWithoutReserves();
        bool IsDummy();
    }
}