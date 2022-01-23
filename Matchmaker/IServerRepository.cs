public interface IServerRepository
{
    IEnumerable<Server> GetAvailableByGameTypeAndRegion(GameType gameType, Region region);
}