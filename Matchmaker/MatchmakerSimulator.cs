using Cassandra;
using Cassandra.Mapping;

public class MatchmakerSimulator
{
    private int _cassandraPort;
    private ConsistencyLevel _consistencyLevel;

    public MatchmakerSimulator(int cassandraPort, ConsistencyLevel consistencyLevel = ConsistencyLevel.One)
    {   
        
        _cassandraPort = cassandraPort;
        _consistencyLevel = consistencyLevel;
    }

    public async Task SimulateMatchmaker()
    {
        await Task.Yield();

        var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(_cassandraPort)
                     .Build();

        var session = cluster.Connect("matchmaker");
        
        IMapper mapper = new Mapper(session);

        CassandraMatchRequestRepository mReqRepo = new(mapper, _consistencyLevel);
        CassandraMatchSuggestionRepository mSugRepo = new(mapper, _consistencyLevel);
        CassandraServerRepository serverRepo = new(mapper, _consistencyLevel);
        Matchmaker m1 = new Matchmaker(serverRepo, mReqRepo, mSugRepo);
        await m1.MatchmakerLoop();
    }
}