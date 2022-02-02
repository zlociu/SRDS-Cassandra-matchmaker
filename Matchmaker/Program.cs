using System.Diagnostics;
using Cassandra;

var cassandraAddress = "127.0.0.1";
var cassandraPort = 9042;
var numberOfServersPerRegionAndGameType = 5;
var numberOfPlayersPerRegionAndGameType = 30;
var numberOfMatchmakers = 3;

var cassandra = new CassandraSetup(cassandraAddress, cassandraPort, replicationFactor: 3);
var mapper = cassandra.Mapper;
Stopwatch stopwatch = new();

#region generate and start servers

var serverGenerator = new ServerGenerator(cassandraAddress, cassandraPort);
var statsCollector = new StatsCollector();
stopwatch.Start();
var servers = serverGenerator.Generate(numberOfServersPerRegionAndGameType, statsCollector);
stopwatch.Stop();
Console.WriteLine($"generated {servers.Count()} servers in time: {stopwatch.ElapsedMilliseconds} ms");
var serversSimulator = new ServersSimulator();
serversSimulator.SimulateServers(servers);

#endregion

#region create and start matchmakers

var matchmakerSimulators = Enumerable.Range(0, numberOfMatchmakers).Select(_ =>
{
    var simulator = new MatchmakerSimulator(cassandraAddress, cassandraPort);
    var thread = new Thread(new ThreadStart(simulator.SimulateMatchmaker));
    thread.Start();
    return (thread, simulator);
}).ToList();

#endregion

#region generate and save match requests

var matchRequestRepository = new CassandraMatchRequestRepository(mapper, ConsistencyLevel.One);
var matchRequestGenerator = new MatchRequestGenerator(matchRequestRepository);
stopwatch.Restart();
matchRequestGenerator.Generate(numberOfPlayersPerRegionAndGameType);
stopwatch.Stop();
Console.WriteLine($"generated {numberOfPlayersPerRegionAndGameType * 30} player requests in time: {stopwatch.ElapsedMilliseconds} ms");
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