using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm
{
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
        List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams, Association associationFilter);
        int CountWithoutReserves();
    }
}