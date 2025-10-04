using System.Data;
using tests.tm;
using tm;

namespace MyProject;

public class TheManagerRunner : TheManagerTest
{

    private void RunGame()
    {
        for(int i = 0; i < 200; i++)
        {
            Session.Instance.Game.NextDay();
            Session.Instance.Game.UpdateTournaments();
        }
    }

    public TheManagerRunner(String databaseName)
    {
        InitGame("console", databaseName, null);
        RunGame();
    }
}

class Program
{
    static void Main(string[] args)
    {
        if(args.Length > 0 && args[0] == "cl")
        {
            using(var writer = new StreamWriter(Path.Join(Directory.GetCurrentDirectory(), "log.txt")))
            {
                Console.SetOut(writer);
                try
                {
                    string databaseName = args[1];
                    TheManagerRunner runner = new TheManagerRunner(databaseName);
                }catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
