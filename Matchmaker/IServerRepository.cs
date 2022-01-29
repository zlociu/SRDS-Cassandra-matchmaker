public interface IServerRepository
{
    IEnumerable<Server> GetAvailableByGameTypeAndRegion(GameType gameType, Region region, int limit = 10000);

    void SetStatus(Guid serverId, GameType gameType, Region region, ServerStatus status);

    void Upsert(Server server);

    void Remove(Guid serverId, GameType gameType, Region region);
}