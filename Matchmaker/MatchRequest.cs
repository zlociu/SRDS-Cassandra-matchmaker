public record MatchRequest
{
    public Guid PlayerId { get; init; }

    public int PlayerRank { get; init; }

    public string Region { get; init; }

    public GameType GameType { get; init; }

    public long RequestTimestamp { get; init; }

    public int Priority { get; init; }

    public override string ToString()
    {
        return $"{this.PlayerId}\t{this.PlayerRank}\t{this.Region}\t\t{(GameType)this.GameType}\t{this.RequestTimestamp}\t{this.Priority}";
    }

    public static string CreateTableString => "CREATE TABLE IF NOT EXISTS MatchRequests (playerid uuid, playerrank int, region text, gametype int, requesttimestamp timestamp, priority int, PRIMARY KEY ((gametype, region), playerid))";
    public static string ColumnsNamesString => $"{nameof(MatchRequest.PlayerId)}\t\t\t\t\t{nameof(MatchRequest.PlayerRank)}\t\t{nameof(MatchRequest.Region)}\t\t{nameof(MatchRequest.GameType)}\t{nameof(MatchRequest.RequestTimestamp)}\t{nameof(MatchRequest.Priority)}";

    public MatchSuggestion ToMatchSuggestion(Guid serverId)
    {
        return new MatchSuggestion
        {
            PlayerId = this.PlayerId,
            PlayerRank = this.PlayerRank,
            Region = this.Region,
            GameType = this.GameType,
            RequestTimestamp = this.RequestTimestamp,
            ServerId = serverId
        };
    }
}