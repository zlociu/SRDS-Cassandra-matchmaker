using System.Diagnostics;
using Cassandra;

var cassandra = new CassandraSetup(address: "127.0.0.1", port: 9043, replicationFactor: 3);
var mapper = cassandra.Mapper;

#region create and save servers

List<ServersSimulator> serversSimulators = new List<ServersSimulator>{
    new ServersSimulator(30, 9042),
    new ServersSimulator(30, 9043),
    new ServersSimulator(30, 9044)
};

List<Task> serverTasks = new();

Stopwatch s1 = new();
s1.Start();

foreach (var simulator in serversSimulators)
{
    serverTasks.Add(simulator.SimulateServers());
}

Task.WaitAll(serverTasks.ToArray());
s1.Stop();
Console.WriteLine($"creating 90 servers in time: {s1.ElapsedMilliseconds} ms");

#endregion

#region generate and save players

var matchRequestRepository = new CassandraMatchRequestRepository(mapper, ConsistencyLevel.One);
var matchRequestGenerator = new MatchRequestGenerator(matchRequestRepository);
s1.Restart();
matchRequestGenerator.Generate(10);
s1.Stop();
var stopPlayerGenerator = DateTimeOffset.Now;
Console.WriteLine($"inserting 300 player requests in time: {s1.ElapsedMilliseconds} ms");
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
var servers = mapper.Fetch<Server>().ToList();
Console.WriteLine($"number of suggestions: {suggestions.Count()}");

var meanRounded = Math.Round(suggestions.Average(s => (s.SuggestionTimestamp - stopPlayerGenerator).TotalMilliseconds));
var max = Math.Round(suggestions.Max(s => (s.SuggestionTimestamp - stopPlayerGenerator).TotalMilliseconds));
var p99 = Math.Round(suggestions
            .Select(s => (s.SuggestionTimestamp - stopPlayerGenerator).TotalMilliseconds)
            .OrderBy(s => s)
            .SkipLast(suggestions.Count() / 100)
            .Last());

var peopleErrors = suggestions.GroupBy(x => x.PlayerId).Where(x => x.Count() > 1).Count();
var serversGroupJoinSuggestions = servers.GroupJoin(
                                suggestions,
                                e => e.Id,
                                u => u.ServerId,
                                (server, number) => new
                                {
                                    Server = server.Id,
                                    MaxPlayers = server.MaxPlayers,
                                    Players = number.Count()
                                });

var serverErrors = serversGroupJoinSuggestions
                    .Where(x => x.Players > x.MaxPlayers)
                    .Count();

var serverEmpty = serversGroupJoinSuggestions
                    .Where(x => x.Players == 0)
                    .Count();

var meanServerFillness = serversGroupJoinSuggestions
                    .Where(x => x.Players > 0)
                    .Average(x => (1.0 * x.Players / x.MaxPlayers));

Console.WriteLine($"\nMatchmaker time statistics: time between send request and get suggestion");
Console.WriteLine($"  mean time: {meanRounded} ms");
Console.WriteLine($"  max time: {max} ms");
Console.WriteLine($"  p99 time: {p99} ms"); //two standard deviations from mean
Console.WriteLine($"found {peopleErrors} ({Math.Round(100.0 * peopleErrors / suggestions.Count(), 2)}%) people with assignment to more than ONE server");
Console.WriteLine($"found {serverErrors} ({Math.Round(100.0 * serverErrors / servers.Count(), 2)}%) servers with assiged more players than 'maxPlayers' property");
Console.WriteLine($"found {serverEmpty} empty servers");
Console.WriteLine($"non-empty servers mean occupancy: {Math.Round(meanServerFillness * 100, 1)}%");

#endregion