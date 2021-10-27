using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    public interface IRecoverableTeams
    {
        List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams);
        int CountWithoutReserves();
    }
}