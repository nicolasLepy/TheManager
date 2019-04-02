using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager
{
    public interface IEquipesRecuperables
    {
        List<Club> RecupererEquipes(int nombre, MethodeRecuperation methode);
    }
}
