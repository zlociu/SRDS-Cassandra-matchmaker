using Cassandra.Mapping;
using Cassandra;

public class CassandraMatchRequestRepository : IMatchRequestRepository
{
    private IMapper db;
    private ConsistencyLevel _consistencyLevel;
    private Random random = new();

    public CassandraMatchRequestRepository(int port, ConsistencyLevel consistencyLvl)
    {
        var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(port)
                     .Build();

        var session = cluster.Connect("matchmaker");

        db = new Mapper(session);
        _consistencyLevel = consistencyLvl;
    }

    public IEnumerable<MatchRequest> GetByGameTypeAndRegion(GameType gameType, Region region, int limit)
    {
        var randomGuid = Guid.NewGuid();

        Cql cql = new Cql(
            "WHERE gametype=? AND region=? AND playerid>=? LIMIT ?",
            (int)gameType,
            (int)region,
            randomGuid,
            limit).WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));

        var firstPart = db.Fetch<MatchRequest>(cql).ToList();

        cql = new Cql(
            "WHERE gametype=? AND region=? AND playerid<? LIMIT ?",
            (int)gameType,
            (int)region,
            randomGuid,
            limit - firstPart.Count()).WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));

        var secondPart = ((firstPart.Count() < limit) ? db.Fetch<MatchRequest>(cql) : Enumerable.Empty<MatchRequest>()).ToList();
        
        return firstPart.Union(secondPart);
    }

    public IEnumerable<MatchRequest> GetByPriority(int priority, int limit)
    {
        Cql cql = new Cql($"WHERE priority=? LIMIT ?", priority, limit)
            .WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));
        return db.Fetch<MatchRequest>(cql);
    }

    public void RemoveByPlayerId(Guid playerId, Region region, GameType gameType)
    {
        Cql cql = new Cql(
            $"WHERE gametype=? AND region=? AND playerid=?", 
            (int) gameType,
            (int) region,
            playerId).WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));
        db.Delete<MatchRequest>(cql);
    }

    public void Upsert(MatchRequest matchRequest)
    {
        db.Insert(matchRequest, new CqlQueryOptions().SetConsistencyLevel(_consistencyLevel));
    }
}