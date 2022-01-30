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
Console.WriteLine($"creating 60 servers in time: {s1.ElapsedMilliseconds} ms");

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

var matchmaker = Task.Run( () => {
    CassandraMatchRequestRepository mReqRepo = new(9043);
    CassandraMatchSuggestionRepository mSugRepo = new(9043);
    CassandraServerRepository serverRepo = new(9043);
    Matchmaker m1 = new Matchmaker(serverRepo, mReqRepo, mSugRepo);
    m1.MatchmakerLoop();
});

// CassandraMatchRequestRepository mReqRepo = new(9043);
// CassandraMatchSuggestionRepository mSugRepo = new(9043);
// CassandraServerRepository serverRepo = new(9043);
// Matchmaker m1 = new Matchmaker(serverRepo, mReqRepo, mSugRepo);
// m1.MatchmakerLoop();

matchmaker.Wait();

var suggestions = mapper.Fetch<MatchSuggestion>().ToList();

Console.WriteLine($"number of suggestions: {suggestions.Count()}");

int meanTime = 0;
foreach( var suggestion in suggestions)
{
    meanTime += (suggestion.SuggestionTimestamp - suggestion.RequestTimestamp).Milliseconds;
}

Console.WriteLine($"Mean time between request and suggestion creation:{Math.Round((1.0 * meanTime) / suggestions.Count(),1)} ms");

