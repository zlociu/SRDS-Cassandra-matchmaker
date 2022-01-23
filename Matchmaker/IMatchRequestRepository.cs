public interface IMatchRequestRepository
{
    IEnumerable<MatchRequest> GetByGameTypeAndRegion(GameType gameType, Region region, int limit);

    void RemoveByPlayerId(Guid playerId);
}