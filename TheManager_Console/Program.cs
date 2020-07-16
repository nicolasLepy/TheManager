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
            Game game = new Game();
            Session.Instance.Game = game;
            Kernel g = game.kernel;
            DatabaseLoader loader = new DatabaseLoader(g);
            loader.Load();


            for (int i = 0; i < 4840; i++)
            {
                game.NextDay();
            }
            Utils.Debug(game.date.ToShortDateString());
            
            Console.ReadLine();

        }
    }
}
