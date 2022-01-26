using System;
using Cassandra;
using Cassandra.Mapping;
using System.Diagnostics;
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

mapper.Execute("DROP TABLE IF EXISTS Players");
mapper.Execute("DROP TABLE IF EXISTS Servers");
mapper.Execute("DROP TABLE IF EXISTS MatchRequests");
mapper.Execute("DROP TABLE IF EXISTS MatchSuggestions");

mapper.Execute(Player.CreateTableString);
mapper.Execute(Server.CreateTableString);
mapper.Execute(MatchRequest.CreateTableString);
mapper.Execute(MatchSuggestion.CreateTableString);

#endregion

List<PlayersSimulator> playersSimulators = new List<PlayersSimulator>{
    new PlayersSimulator(300, 9042),
    new PlayersSimulator(300, 9043),
    new PlayersSimulator(300, 9044)
};

List<Task> playerTasks = new();
foreach(var simulator in playersSimulators)
{
    playerTasks.Add(simulator.SimulatePlayers());
}
Stopwatch s1 = new();
s1.Start();

Task.WaitAll(playerTasks.ToArray());
s1.Stop();
Console.WriteLine($"inserting 900 elements in time: {s1.ElapsedMilliseconds} ms");

var num = mapper.Fetch<MatchRequest>();
// foreach(var item in num)
// {
//     Console.WriteLine(item.ToString());
// }
Console.WriteLine($"number of requests: {num.Count()}");
