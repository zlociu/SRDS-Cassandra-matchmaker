using Cassandra.Mapping;
using Cassandra;

public class CassandraServerRepository : IServerRepository
{
    private IMapper db;
    private ConsistencyLevel _consistencyLevel;

    public CassandraServerRepository(IMapper mapper, ConsistencyLevel consistencyLvl)
    {
        db = mapper;
        _consistencyLevel = consistencyLvl;
    }

    public IEnumerable<Server> GetAvailableByGameTypeAndRegion(GameType gameType, Region region, int limit)
    {
        Cql cql = new Cql(
            $"WHERE gametype=? AND region=? AND status=? LIMIT ?",
            (int)gameType,
            (int)region,
            (int)ServerStatus.WaitingForPlayers,
            limit
        ).WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));
        return db.Fetch<Server>(cql);
    }

    public void Remove(Guid serverId, GameType gameType, Region region)
    {
        Cql cql = new Cql($"WHERE gametype=? AND region=? AND id=?",
            (int)gameType,
            (int)region,
            serverId).WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));
        db.Delete<Server>(cql);
    }

    public void SetStatus(Guid serverId, GameType gameType, Region region, ServerStatus status)
    {
        Cql cql = new Cql($"SET status=? WHERE gametype=? AND region=? AND id=?",
            (int)status,
            (int)gameType,
            (int)region,
            serverId).WithOptions(x => x.SetConsistencyLevel(_consistencyLevel));
        db.Update<Server>(cql);
    }

    public void Upsert(Server Server)
    {
        db.Insert(Server, new CqlQueryOptions().SetConsistencyLevel(_consistencyLevel));
    }
}