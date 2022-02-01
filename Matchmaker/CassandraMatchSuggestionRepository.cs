using Cassandra.Mapping;
using Cassandra;

public class CassandraMatchSuggestionRepository : IMatchSuggestionRepository
{
    private IMapper db;
    private ConsistencyLevel _consistencyLevel;

    public CassandraMatchSuggestionRepository(int port, ConsistencyLevel consistencyLvl)
    {
       var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(port)
                     .Build();

        var session = cluster.Connect("matchmaker");

        db = new Mapper(session);
        _consistencyLevel = consistencyLvl;
    }

    public IEnumerable<MatchSuggestion> GetByServerIds(IEnumerable<Guid> serverIds, int limit)
    {
        Cql cql = new Cql($"WHERE serverid IN ? LIMIT ?", serverIds, limit)
            .WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));
        return db.Fetch<MatchSuggestion>(cql);
    }

    public void RemoveByServerId(Guid serverId)
    {
        Cql cql = new Cql($"WHERE serverid=?", serverId)
            .WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));
        db.Delete<MatchSuggestion>(cql);
    }

    public void Upsert(MatchSuggestion MatchSuggestion)
    {
        db.Insert(MatchSuggestion, new CqlQueryOptions().SetConsistencyLevel(_consistencyLevel));
    }
}