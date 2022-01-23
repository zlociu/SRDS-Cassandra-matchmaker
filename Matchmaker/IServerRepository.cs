public interface IServerRepository
{
    IEnumerable<Server> GetAvailableByGameTypeAndRegion(GameType gameType, string region);
}