public interface IMatchRequestRepository
{
    IEnumerable<MatchRequest> GetByGameTypeAndRegion(GameType gameType, string region, int limit);

    void RemoveByPlayerId(Guid playerId);
}