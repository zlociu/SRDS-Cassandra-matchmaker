using System.Diagnostics;
using Cassandra;
using Cassandra.Mapping;
/*
PlayerGerenator playerGerenator = new();
var list =  playerGerenator.GeneratePlayers(100);
Console.WriteLine(Player.ColumnsNamesString);
foreach (var item in list)
{
    Console.WriteLine(item.ToString());
}

ServerGenerator serverGenerator = new();
var list2 =  serverGenerator.GenerateServers(10);
Console.WriteLine($"\n{Server.ColumnsNamesString}");
foreach (var item in list2)
{
    Console.WriteLine(item.ToString());
}
*/

#region Creating Cassandra keyspace and required tables if not exists

MappingConfiguration.Global.Define<MatchmakerMappings>();
Cassandra.Diagnostics.CassandraTraceSwitch.Level = TraceLevel.Info;
Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(9042)
                     .Build();

var tmp = cluster.Connect();

tmp.CreateKeyspaceIfNotExists("matchmaker", new Dictionary<string, string>{
                                                { "class", ReplicationStrategies.SimpleStrategy },
                                                { "replication_factor", "3" } 
                                            });

var session = cluster.Connect("matchmaker");
Console.WriteLine(session.Keyspace);

IMapper mapper = new Mapper(session);

//mapper.Execute("DROP TABLE IF EXISTS Players");
mapper.Execute("DROP TABLE IF EXISTS Servers");
mapper.Execute("DROP TABLE IF EXISTS MatchRequests");
mapper.Execute("DROP TABLE IF EXISTS MatchSuggestions");

//mapper.Execute(Player.CreateTableString);
mapper.Execute(Server.CreateTableString);
mapper.Execute(MatchRequest.CreateTableString);
mapper.Execute(MatchSuggestion.CreateTableString);
mapper.Execute("CREATE INDEX ON MatchRequests (priority)");
mapper.Execute("CREATE INDEX ON Servers (status)");

var random = new Random();
var matchRequestRepository = new CassandraMatchRequestRepository(mapper);
var matchRequest = RandomMatchRequest(random);
matchRequestRepository.Upsert(matchRequest);
System.Console.WriteLine("Upsert successful");
var result = matchRequestRepository.GetByGameTypeAndRegion(matchRequest.GameType, matchRequest.Region, 10);
PrintMatchRequests(result);
result = matchRequestRepository.GetByPriority(0, 10);
PrintMatchRequests(result);

void PrintMatchRequests(IEnumerable<MatchRequest> matchRequests)
{
    Console.WriteLine("Match requests");
    Console.WriteLine(MatchRequest.ColumnsNamesString);
    Console.WriteLine("-----------------------------------------------------------------------------------");

    foreach (var row in matchRequests)
    {
        Console.WriteLine(row.ToString());
    }
}

MatchRequest RandomMatchRequest(Random random)
{
    return new MatchRequest
    {
        PlayerId = Guid.NewGuid(),
        PlayerRank = random.Next(),
        Region = RandomRegion(random),
        GameType = RandomGameType(random),
        RequestTimestamp = DateTimeOffset.Now,
        Priority = 0
    };
}

GameType RandomGameType(Random random)
{
    var gameTypes = Enum.GetValues<GameType>();
    return gameTypes[random.Next(gameTypes.Count())];
}

Region RandomRegion(Random random)
{
    var regions = Enum.GetValues<Region>();
    return regions[random.Next(regions.Count())];
}
#endregion

List<PlayersSimulator> playersSimulators = new List<PlayersSimulator>{
    new PlayersSimulator(1000, 9042),
    new PlayersSimulator(1000, 9042),
    new PlayersSimulator(1000, 9042)
};

List<Task> playerTasks = new();

Stopwatch s1 = new();
s1.Start();

foreach(var simulator in playersSimulators)
{
    playerTasks.Add(simulator.SimulatePlayers());
}


Task.WaitAll(playerTasks.ToArray());
s1.Stop();
Console.WriteLine($"inserting 3000 elements in time: {s1.ElapsedMilliseconds} ms");

var num = mapper.Fetch<MatchRequest>();
// foreach(var item in num)
// {
//     Console.WriteLine(item.ToString());
// }
Console.WriteLine($"number of requests: {num.Count()}");
