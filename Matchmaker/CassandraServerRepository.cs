using Cassandra.Mapping;
using Cassandra;

public class CassandraServerRepository : IServerRepository
{
    private IMapper db;

    public CassandraServerRepository(int port)
    {
        var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(port)
                     .Build();

        var session = cluster.Connect("matchmaker");

        db = new Mapper(session);
    }

    public IEnumerable<Server> GetAvailableByGameTypeAndRegion(GameType gameType, Region region, int limit)
    {
        return db.Fetch<Server>(
            $"WHERE gametype=? AND region=? AND status=? LIMIT ?",
            (int)gameType,
            (int)region,
            (int)ServerStatus.WaitingForPlayers,
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