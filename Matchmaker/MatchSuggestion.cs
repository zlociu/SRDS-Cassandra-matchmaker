public record MatchSuggestion
{
    public Guid PlayerId { get; init; }

    public int PlayerRank { get; init; }

    public Region Region { get; init; }

    public GameType GameType { get; init; }

    public long RequestTimestamp { get; init; }

    public Guid ServerId { get; init; }

    public override string ToString()
    {
        return $"{this.PlayerId}\t{this.PlayerRank}\t{this.Region}\t\t{(GameType)this.GameType}\t{this.RequestTimestamp}\t{this.ServerId}";
    }

    public static string CreateTableString => "CREATE TABLE IF NOT EXISTS MatchSuggestions (playerid uuid, playerrank int, region text, gametype int, requesttimestamp timestamp, serverid uuid, PRIMARY KEY (serverid, playerid))";
    public static string ColumnsNamesString => $"{nameof(MatchSuggestion.PlayerId)}\t\t\t\t\t{nameof(MatchSuggestion.PlayerRank)}\t\t{nameof(MatchSuggestion.Region)}\t\t{nameof(MatchSuggestion.GameType)}\t{nameof(MatchSuggestion.RequestTimestamp)}\t{nameof(MatchSuggestion.ServerId)}";
}