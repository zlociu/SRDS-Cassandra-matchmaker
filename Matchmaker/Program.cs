using System.Diagnostics;
using Cassandra;

var cassandra = new CassandraSetup(address: "127.0.0.1", port: 9042, replicationFactor: 3);
var mapper = cassandra.Mapper;
Stopwatch stopwatch = new();

#region generate and start servers

var serverGenerator = new ServerGenerator(cassandraAddress: "127.0.0.1", cassandraPort: 9042);
var statsCollector = new StatsCollector();
stopwatch.Start();
var servers = serverGenerator.Generate(serversPerRegionAndGameType: 5, statsCollector);
stopwatch.Stop();
Console.WriteLine($"generated {servers.Count()} servers in time: {stopwatch.ElapsedMilliseconds} ms");
var serversSimulator = new ServersSimulator();
serversSimulator.SimulateServers(servers);

#endregion

#region create and start matchmakers

var matchmakerSimulators = Enumerable.Range(0, 3).Select(_ =>
{
    var simulator = new MatchmakerSimulator(cassandraAddress: "127.0.0.1", cassandraPort: 9042);
    var thread = new Thread(new ThreadStart(simulator.SimulateMatchmaker));
    thread.Start();
    return (thread, simulator);
}).ToList();

#endregion

#region generate and save match requests

var matchRequestRepository = new CassandraMatchRequestRepository(mapper, ConsistencyLevel.One);
var matchRequestGenerator = new MatchRequestGenerator(matchRequestRepository);
stopwatch.Restart();
matchRequestGenerator.Generate(matchRequestsPerRegionAndGameType: 30);
stopwatch.Stop();
var stopPlayerGenerator = DateTimeOffset.Now;
Console.WriteLine($"generated 900 player requests in time: {stopwatch.ElapsedMilliseconds} ms");
#endregion

#region wait for matchmakers to finish

foreach (var (thread, simulator) in matchmakerSimulators)
{
    simulator.CanStop = true;
    thread.Join();
}

#endregion

#region summary

var suggestions = mapper.Fetch<MatchSuggestion>().ToList();
Console.WriteLine($"number of orphaned suggestions: {suggestions.Count()}");

statsCollector.PrintSummary();

#endregion