using Cassandra;
using Cassandra.Mapping;

public class ServersSimulator
{
    private List<Server> _servers;
    private int _cassandraPort;
    private ConsistencyLevel _consistencyLevel;
    private int _count;

    public ServersSimulator(int serversCount, int cassandraPort, ConsistencyLevel consistencyLevel = ConsistencyLevel.One)
    {   
        _servers = new ServerGenerator().GenerateServers(serversCount).ToList();
        _cassandraPort = cassandraPort;
        _count = serversCount;
        _consistencyLevel = consistencyLevel;
    }

    public async Task SimulateServers()
    {
        //await Task.Yield();
        
        var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(_cassandraPort)
                     .Build();

        var session = cluster.Connect("matchmaker");
        IMapper mapper = new Mapper(session);

        foreach(var server in _servers)
        {   
            await mapper.InsertAsync<Server>(
                server, new CqlQueryOptions().SetConsistencyLevel(_consistencyLevel));
            //await mapper.InsertIfNotExistsAsync<MatchRequest>(player.GetPlayerMatchRequest(), CqlQueryOptions.New().SetConsistencyLevel(ConsistencyLevel.One));
        }
    }
}