using Cassandra.Mapping;
using Cassandra;

public class CassandraMatchRequestRepository : IMatchRequestRepository
{
    private IMapper db;
    private Random random = new();

    public CassandraMatchRequestRepository(int port)
    {
        var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(port)
                     .Build();

        var session = cluster.Connect("matchmaker");

        db = new Mapper(session);
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

    public void RemoveByPlayerId(Guid playerId, Region region, GameType gameType)
    {
        db.Delete<MatchRequest>($"WHERE gametype=? AND region=? AND playerid=?", 
                                (int) gameType,
                                (int) region,
                                playerId);
    }

    public void Upsert(MatchRequest matchRequest)
    {
        db.Insert(matchRequest);
    }
}