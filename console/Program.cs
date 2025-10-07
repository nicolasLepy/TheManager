using System.Data;
using tests;
using tests.tm;
using tm;

namespace MyProject;

public class TheManagerRunner : TheManagerTest
{

    private string _databaseName;

    private void ExtractGame()
    {
        Tournament t = Session.Instance.Game.kernel.worldAssociation.associations[0].associations[0].Cup(1);
        TestUtils.PrintTournament(t);
    }

    private void RunGame()
    {
        for(int i = 0; i < 300; i++)
        {
            Session.Instance.Game.NextDay();
            Session.Instance.Game.UpdateTournaments();
        }
    }

    public TheManagerRunner(String databaseName)
    {
        _databaseName = databaseName;
    }

    public void Run()
    {
        InitGame("console", _databaseName, null);
        RunGame();
        ExtractGame();
    }
}

class Program
{

    private static void Run(string[] args)
    {
        using (var writer = new StreamWriter(Path.Join(Directory.GetCurrentDirectory(), "log.txt")))
        {
            Console.SetOut(writer);
            try
            {
                string databaseName = args[1];
                TheManagerRunner runner = new TheManagerRunner(databaseName);
                runner.Run();
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    private static void Run2(string[] args)
    {
        string databaseName = args[1];
        TheManagerRunner runner = new TheManagerRunner(databaseName);
        runner.Run();
    }

    static void Main(string[] args)
    {
        if(args.Length > 0 && args[0] == "cl")
        {
            Run2(args);
        }
    }
}
