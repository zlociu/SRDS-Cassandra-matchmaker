using System.Diagnostics;
using Cassandra;

var cassandra = new CassandraSetup(address: "127.0.0.1", port: 9043, replicationFactor: 3);
var mapper = cassandra.Mapper;
Stopwatch stopwatch = new();

#region generate and start servers

var serverGenerator = new ServerGenerator(cassandraAddress: "127.0.0.1", cassandraPort: 9042);
var statsCollector = new StatsCollector();
stopwatch.Start();
var servers = serverGenerator.Generate(serversPerRegionAndGameType: 3, statsCollector);
stopwatch.Stop();
Console.WriteLine($"generated {servers.Count()} servers in time: {stopwatch.ElapsedMilliseconds} ms");
var serversSimulator = new ServersSimulator();
serversSimulator.SimulateServers(servers);

#endregion

#region generate and save match requests

var matchRequestRepository = new CassandraMatchRequestRepository(mapper, ConsistencyLevel.One);
var matchRequestGenerator = new MatchRequestGenerator(matchRequestRepository);
stopwatch.Restart();
matchRequestGenerator.Generate(10);
stopwatch.Stop();
var stopPlayerGenerator = DateTimeOffset.Now;
Console.WriteLine($"generated 300 player requests in time: {stopwatch.ElapsedMilliseconds} ms");
#endregion

#region Matchmaker work

List<MatchmakerSimulator> matchmakerSimulators = new List<MatchmakerSimulator>{
    new MatchmakerSimulator(9042, ConsistencyLevel.One),
    new MatchmakerSimulator(9043, ConsistencyLevel.One),
    new MatchmakerSimulator(9044, ConsistencyLevel.One)
};

List<Task> matchmakerTasks = new List<Task>();
foreach (var simulator in matchmakerSimulators)
{
    matchmakerTasks.Add(simulator.SimulateMatchmaker());
}

Task.WaitAll(matchmakerTasks.ToArray());

#endregion

#region Calculations

var suggestions = mapper.Fetch<MatchSuggestion>().ToList();
Console.WriteLine($"number of suggestions: {suggestions.Count()}");

statsCollector.PrintSummary();

#endregion