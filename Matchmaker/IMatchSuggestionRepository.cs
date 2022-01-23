public interface IMatchSuggestionRepository
{
    IEnumerable<MatchSuggestion> GetByServerIds(IEnumerable<Guid> ServerIds);

    void Upsert(MatchSuggestion matchSuggestion);
}