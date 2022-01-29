using Cassandra.Mapping;

public class CassandraMatchRequestRepository : IMatchRequestRepository
{
    private IMapper db;
    private Random random = new();

    public CassandraMatchRequestRepository(IMapper cassandraMapper)
    {
        db = cassandraMapper;
    }

    public IEnumerable<MatchRequest> GetByGameTypeAndRegion(GameType gameType, Region region, int limit)
    {
        var randomGuid = Guid.NewGuid();
        var firstPart = db.Fetch<MatchRequest>(
            "WHERE gametype=? AND region=? AND playerid>=? LIMIT ?",
            (int)gameType,
            (int)region,
            randomGuid,
            limit
        ).ToList();
        var secondPart = ((firstPart.Count() < limit) ? db.Fetch<MatchRequest>(
            "WHERE gametype=? AND region=? AND playerid<? LIMIT ?",
            (int)gameType,
            (int)region,
            randomGuid,
            limit - firstPart.Count()
        ) : Enumerable.Empty<MatchRequest>()).ToList();
        return firstPart.Union(secondPart);
    }

    public IEnumerable<MatchRequest> GetByPriority(int priority, int limit)
    {
        return db.Fetch<MatchRequest>($"WHERE priority=? LIMIT ?", priority, limit);
    }

    public void RemoveByPlayerId(Guid playerId)
    {
        db.Delete<MatchRequest>($"WHERE playerid=?", playerId);
    }

    public void Upsert(MatchRequest matchRequest)
    {
        db.Insert(matchRequest);
    }
}