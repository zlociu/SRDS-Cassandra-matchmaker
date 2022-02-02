public record MatchSuggestion
{
    public Guid PlayerId { get; init; }
    public int PlayerRank { get; init; }
    public Region Region { get; init; }
    public GameType GameType { get; init; }
    public DateTimeOffset RequestTimestamp { get; init; }
    public DateTimeOffset SuggestionTimestamp { get; init; }
    public Guid ServerId { get; init; }

    public MatchRequest ToMatchRequest()
    {
        return new MatchRequest
        {
            PlayerId = this.PlayerId,
            PlayerRank = this.PlayerRank,
            Region = this.Region,
            GameType = this.GameType,
            RequestTimestamp = this.RequestTimestamp,
            Priority = 0
        };
    }
}