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
        var randomTokenBound = random.NextInt64();
        var firstPart = db.Fetch<MatchRequest>(
            "WHERE gametype=? AND region=? AND token(gametype, region)>=? LIMIT ?",
            (int)gameType,
            (int)region,
            randomTokenBound,
            limit
        );
        var secondPart = (firstPart.Count() < limit) ? db.Fetch<MatchRequest>(
            "WHERE gametype=? AND region=? AND token(gametype, region)<? LIMIT ?",
            (int)gameType,
            (int)region,
            randomTokenBound,
            limit - firstPart.Count()
        ) : Enumerable.Empty<MatchRequest>();
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