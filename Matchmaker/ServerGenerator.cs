using Cassandra;
using Cassandra.Mapping;

public class ServerGenerator
{
    private string cassandraAddress;
    private int cassandraPort;

    public ServerGenerator(string cassandraAddress, int cassandraPort)
    {
        this.cassandraAddress = cassandraAddress;
        this.cassandraPort = cassandraPort;
    }

    public List<ServerBehaviour> Generate(int serversPerRegionAndGameType, StatsCollector statsCollector)
    {
        var allGameTypes = Enum.GetValues<GameType>();
        var allRegions = Enum.GetValues<Region>();
        var allCombinations = from gameType in allGameTypes
                              from region in allRegions
                              select (gameType, region);
        var result = new List<ServerBehaviour>();
        foreach (var (gameType, region) in allCombinations)
        {
            var servers = Generate(serversPerRegionAndGameType, gameType, region, statsCollector);
            result.AddRange(servers);
        }
        return result;
    }

    private List<ServerBehaviour> Generate(int matchRequests, GameType gameType, Region region, StatsCollector statsCollector)
    {
        var servers = new List<ServerBehaviour>();
        for (var i = 0; i < matchRequests; i++)
        {
            var server = GenerateServer(gameType, region, statsCollector);
            servers.Add(server);
        }
        return servers;
    }

    private ServerBehaviour GenerateServer(GameType gameType, Region region, StatsCollector statsCollector)
    {
        var cluster = Cluster.Builder()
                             .AddContactPoint(cassandraAddress)
                             .WithPort(cassandraPort)
                             .Build();
        var session = cluster.Connect("matchmaker");
        var mapper = new Mapper(session);
        var serverRepository = new CassandraServerRepository(mapper, ConsistencyLevel.One);
        var matchRequestRepository = new CassandraMatchRequestRepository(mapper, ConsistencyLevel.One);
        var matchSuggestionRepository = new CassandraMatchSuggestionRepository(mapper, ConsistencyLevel.One);

        return new ServerBehaviour(
            serverRepository,
            matchRequestRepository,
            matchSuggestionRepository,
            statsCollector,
            region,
            gameType,
            maxPlayers: 10
        );
    }
}