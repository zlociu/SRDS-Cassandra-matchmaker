using Cassandra.Mapping;
using Cassandra;

public class CassandraMatchSuggestionRepository : IMatchSuggestionRepository
{
    private IMapper db;

    public CassandraMatchSuggestionRepository(int port)
    {
       var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(port)
                     .Build();

        var session = cluster.Connect("matchmaker");

        db = new Mapper(session);
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