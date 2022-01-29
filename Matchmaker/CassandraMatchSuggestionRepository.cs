using Cassandra.Mapping;

public class CassandraMatchSuggestionRepository : IMatchSuggestionRepository
{
    private IMapper db;

    public CassandraMatchSuggestionRepository(IMapper cassandraMapper)
    {
        db = cassandraMapper;
    }

    public IEnumerable<MatchSuggestion> GetByServerIds(IEnumerable<Guid> serverIds, int limit)
    {
        return db.Fetch<MatchSuggestion>($"WHERE serverid IN ? LIMIT ?", serverIds, limit);
    }

    public void RemoveByServerId(Guid serverId)
    {
        db.Delete<MatchSuggestion>($"WHERE serverid=?", serverId);
    }

    public void Upsert(MatchSuggestion MatchSuggestion)
    {
        db.Insert(MatchSuggestion);
    }
}