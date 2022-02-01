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
        
        CassandraMatchRequestRepository mReqRepo = new(_cassandraPort, _consistencyLevel);
        CassandraMatchSuggestionRepository mSugRepo = new(_cassandraPort, _consistencyLevel);
        CassandraServerRepository serverRepo = new(_cassandraPort, _consistencyLevel);
        Matchmaker m1 = new Matchmaker(serverRepo, mReqRepo, mSugRepo);
        await m1.MatchmakerLoop();
    }
}