using Cassandra.Mapping;

public class CassandraServerRepository : IServerRepository
{
    private IMapper db;

    public CassandraServerRepository(IMapper cassandraMapper)
    {
        db = cassandraMapper;
    }

    public IEnumerable<Server> GetAvailableByGameTypeAndRegion(GameType gameType, Region region, int limit)
    {
        return db.Fetch<Server>(
            $"WHERE gametype=? AND region=? AND status=? LIMIT ?",
            (int)gameType,
            (int)region,
            ServerStatus.WaitingForPlayers,
            limit
        );
    }

    public void Remove(Guid serverId, GameType gameType, Region region)
    {
        db.Delete<Server>(
            $"WHERE gametype=? AND region=? AND serverid=?",
            (int)gameType,
            (int)region,
            serverId
        );
    }

    public void SetStatus(Guid serverId, GameType gameType, Region region, ServerStatus status)
    {
        db.Update<Server>(
            $"SET status=? WHERE gametype=? AND region=? AND serverid=?",
            (int)status,
            (int)gameType,
            (int)region,
            serverId
        );
    }

    public void Upsert(Server Server)
    {
        db.Insert(Server);
    }
}