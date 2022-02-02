public record Player
{
    public Guid Id { get; init; }
    public Region Region { get; init; }
    public GameType GameType { get; init; }
    public int Rank { get; init; }

    public MatchRequest GetPlayerMatchRequest()
    {
        return new MatchRequest
        {
            PlayerId = this.Id,
            PlayerRank = this.Rank,
            GameType = this.GameType,
            Region = this.Region,
            Priority = new Random().Next(5),
            RequestTimestamp = DateTimeOffset.Now
        };
    }
}