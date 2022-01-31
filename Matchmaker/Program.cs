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
//Cassandra.Diagnostics.CassandraTraceSwitch.Level = TraceLevel.Info;
//Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(9043)
                     .Build();

var tmp = cluster.Connect();

tmp.CreateKeyspaceIfNotExists("matchmaker", new Dictionary<string, string>{
                                                { "class", ReplicationStrategies.SimpleStrategy },
                                                { "replication_factor", "3" } 
                                            });

var session = cluster.Connect("matchmaker");
Console.WriteLine($"{session.Keyspace}\n");

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


#endregion

#region create and save servers

List<ServersSimulator> serversSimulators = new List<ServersSimulator>{
    new ServersSimulator(40, 9042),
    new ServersSimulator(40, 9043),
    new ServersSimulator(40, 9044)
};

List<Task> serverTasks = new();

Stopwatch s1 = new();
s1.Start();

foreach(var simulator in serversSimulators)
{
    serverTasks.Add(simulator.SimulateServers());
}

Task.WaitAll(serverTasks.ToArray());
s1.Stop();
Console.WriteLine($"creating 120 servers in time: {s1.ElapsedMilliseconds} ms");

#endregion

#region generate and save players

List<PlayersSimulator> playersSimulators = new List<PlayersSimulator>{
    new PlayersSimulator(100, 9042),
    new PlayersSimulator(100, 9043),
    new PlayersSimulator(100, 9044)
};

List<Task> playerTasks = new();

s1.Restart();

foreach(var simulator in playersSimulators)
{
    playerTasks.Add(simulator.SimulatePlayers());
}

Task.WaitAll(playerTasks.ToArray());
s1.Stop();
Console.WriteLine($"inserting 300 player requests in time: {s1.ElapsedMilliseconds} ms");

#endregion

#region Matchmaker work

// var matchmaker = Task.Run( () => {
//     CassandraMatchRequestRepository mReqRepo = new(9043);
//     CassandraMatchSuggestionRepository mSugRepo = new(9043);
//     CassandraServerRepository serverRepo = new(9043);
//     Matchmaker m1 = new Matchmaker(serverRepo, mReqRepo, mSugRepo);
//     m1.MatchmakerLoop();
// });

List<MatchmakerSimulator> matchmakerSimulators = new List<MatchmakerSimulator>{
    new MatchmakerSimulator(9042, ConsistencyLevel.One),
    new MatchmakerSimulator(9043, ConsistencyLevel.One),
    new MatchmakerSimulator(9044, ConsistencyLevel.One)
};

List<Task> matchmakerTasks = new List<Task>();
foreach(var simulator in matchmakerSimulators)
{
    matchmakerTasks.Add(simulator.SimulateMatchmaker());
}

Task.WaitAll(matchmakerTasks.ToArray());

#endregion

var suggestions = mapper.Fetch<MatchSuggestion>().ToList();
Console.WriteLine($"number of suggestions: {suggestions.Count()}");

//calculations 
Console.WriteLine($"\nMatchmaker time statistics: time between send request and get suggestion");
Console.WriteLine($"Mean time: {Math.Round(suggestions.Average(s => (s.SuggestionTimestamp - s.RequestTimestamp).TotalMilliseconds))} ms");
Console.WriteLine($"Max time: {suggestions.Max(s => (s.SuggestionTimestamp - s.RequestTimestamp).TotalMilliseconds)} ms");
Console.WriteLine("P99 time: {0} ms",
    suggestions
        .Select(s => (s.SuggestionTimestamp - s.RequestTimestamp).TotalMilliseconds)
        .OrderBy(s => s)
        .SkipLast(suggestions.Count() / 100)
        .Last());

Console.WriteLine($"found {suggestions.GroupBy(x => x.PlayerId).Where(x => x.Count() > 1).Count()} people with assignment to more than ONE server");

