using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager;
using TheManager.Comparators;

namespace TheManager_Console
{
    class Program
    {

        static void Main(string[] args)
        {
            Game partie = new Game();
            Session.Instance.Partie = partie;
            Kernel g = partie.kernel;
            DatabaseLoader cbdd = new DatabaseLoader(g);
            cbdd.Load();

           
            for (int i = 0; i < 4840; i++)
                partie.NextDay();
            Console.WriteLine(partie.date.ToShortDateString());
            
            Console.ReadLine();

        }
    }
}
