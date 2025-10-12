using System.Data;
using System.Xml.Linq;
using tests;
using tests.tm;
using tm;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyProject;

public interface ISimulationExport
{
    public abstract void Export(string dir);
}

public class ExportTournament: ISimulationExport
{

    private struct GameDefinition
    {
        public int homeId { get; set; }
        public int awayId { get; set; }
    }

    public Tournament Tournament { get; set; }
    public int RoundIndex { get; set; }

    public ExportTournament(Tournament tournament, int roundIndex)
    {
        Tournament = tournament;
        RoundIndex = roundIndex;
    }

    private List<GameDefinition> GetGameDefinitions(Round round)
    {
        List<GameDefinition> gameDefinitions = new List<GameDefinition>();
        foreach (Match m in round.matches)
        {
            gameDefinitions.Add(new GameDefinition() { homeId = m.home.id, awayId = m.away.id });
        }
        return gameDefinitions;
    }

    public async void Export(string dir)
    {
        if(Tournament == null)
        {
            throw new Exception("Tournament is null");
        }
        string fileName = Path.Join(dir, String.Format("games_{0}_{1}_{2}.json", Tournament.Id, Session.Instance.Game.date.Year, RoundIndex));
        Round r = Tournament.rounds[RoundIndex];
        List<GameDefinition> gameDefinitions = GetGameDefinitions(r);
        foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
        {
            if(t.parent == Tournament && RoundIndex < t.rounds.Count)
            {
                Round tr = t.rounds[RoundIndex];
                gameDefinitions.AddRange(GetGameDefinitions(tr));
            }
        }

        await using FileStream createStream = File.Create(fileName);
        await JsonSerializer.SerializeAsync(createStream, gameDefinitions, new JsonSerializerOptions { WriteIndented = true});
    }
}

public class TheManagerRunner : TheManagerTest
{

    private string _databaseName;
    private DateTime _simulationEnd;
    private List<ISimulationExport> _simulationResults;

    private void ReadSimulationArgs()
    {

        XDocument doc = XDocument.Load(Path.Join(Utils.dataFolderName, "simulation.xml"));
        XElement root = doc.Root;
        _simulationEnd = DateTime.MinValue;
        DateTime.TryParseExact(root.Attribute("until").Value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _simulationEnd);

        _simulationResults = new List<ISimulationExport>();
        foreach (XElement exp in root.Descendants("export"))
        {
            Tournament t = Session.Instance.Game.kernel.String2Tournament(exp.Attribute("tournament").Value);
            int roundIdx = int.Parse(exp.Attribute("round_index").Value);
            _simulationResults.Add(new ExportTournament(t, roundIdx));
        }
    }

    private void ExtractResults()
    {
        Tournament t = Session.Instance.Game.kernel.worldAssociation.associations[0].associations[0].Cup(1);
        TestUtils.PrintTournament(t);
        foreach(ISimulationExport export in _simulationResults)
        {
            string exportPath = Path.Join(Utils.dataFolderName, "export");
            System.IO.Directory.CreateDirectory(exportPath);
            export.Export(exportPath);
        }
    }

    private void RunGame()
    {
        while(Utils.IsBefore(Session.Instance.Game.date, _simulationEnd))
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
        ReadSimulationArgs();
        RunGame();
        ExtractResults();
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
