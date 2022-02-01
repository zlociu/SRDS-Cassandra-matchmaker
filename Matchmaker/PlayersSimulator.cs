using Cassandra;
using Cassandra.Mapping;

public class PlayersSimulator
{
    private List<Player> _players;
    private int _cassandraPort;
    private ConsistencyLevel _consistencyLevel;
    private int _count;

    public PlayersSimulator(int playersCount, int cassandraPort, ConsistencyLevel consistencyLevel = ConsistencyLevel.One)
    {   
        _players = new PlayerGerenator().GeneratePlayers(playersCount).ToList();
        _cassandraPort = cassandraPort;
        _count = playersCount;
        _consistencyLevel = consistencyLevel;
    }

    public async Task SimulatePlayers()
    {
        //await Task.Yield();
        
        var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(_cassandraPort)
                     .Build();

        var session = cluster.Connect("matchmaker");
        IMapper mapper = new Mapper(session);

        foreach(var player in _players)
        {   
            await mapper.InsertAsync<MatchRequest>(
                player.GetPlayerMatchRequest(), 
                new CqlQueryOptions().SetConsistencyLevel(_consistencyLevel));
            //await mapper.InsertIfNotExistsAsync<MatchRequest>(player.GetPlayerMatchRequest(), CqlQueryOptions.New().SetConsistencyLevel(ConsistencyLevel.One));
        }
    }
}