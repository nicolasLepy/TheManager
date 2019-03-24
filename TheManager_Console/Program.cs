using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager;

namespace TheManager_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Competition c = new Competition("Test", "logo.png", new DateTime(2000, 6, 25));
            Ville v1 = new Ville("ville 1");
            Stade s1 = new Stade("stade 1", 2400, v1);
            Club c1 = new Club_Ville("FC 1", "FC1", 25, 2500, 140, 74, v1, 183, "logo1.png", s1);
            Console.ReadLine();

        }
    }
}
