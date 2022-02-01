public interface IMatchRequestRepository
{
    IEnumerable<MatchRequest> GetByGameTypeAndRegion(GameType gameType, Region region, int limit);

    IEnumerable<MatchRequest> GetByPriority(int priority, int limit);

    void Upsert(MatchRequest matchRequest);

    void RemoveByPlayerId(Guid playerId, Region region, GameType gameType);
}