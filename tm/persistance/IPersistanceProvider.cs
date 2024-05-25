using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.persistance
{
    public interface IPersistanceProvider
    {

        public void Save(Game game);
        public Game Load();

    }
}
