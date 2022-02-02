public record MatchRequest
{
    public Guid PlayerId { get; init; }
    public int PlayerRank { get; init; }
    public Region Region { get; init; }
    public GameType GameType { get; init; }
    public DateTimeOffset RequestTimestamp { get; init; }
    public int Priority { get; init; }

    public MatchSuggestion ToMatchSuggestion(Guid serverId)
    {
        return new MatchSuggestion
        {
            PlayerId = this.PlayerId,
            PlayerRank = this.PlayerRank,
            Region = this.Region,
            GameType = this.GameType,
            RequestTimestamp = this.RequestTimestamp,
            SuggestionTimestamp = DateTimeOffset.Now,
            ServerId = serverId
        };
    }
}