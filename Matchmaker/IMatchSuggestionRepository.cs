public interface IMatchSuggestionRepository
{
    IEnumerable<MatchSuggestion> GetByServerIds(IEnumerable<Guid> serverIds, int limit = 10000);

    void Upsert(MatchSuggestion matchSuggestion);

    void RemoveByServerId(Guid serverId);
}