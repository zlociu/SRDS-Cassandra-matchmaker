public record Server
{
    public Guid Id { get; init; }

    public string Status { get; init; }

    public Region Region { get; init; }

    public GameType GameType { get; init; }

    public int MaxPlayers { get; init; }

    public override string ToString()
    {
        return $"{this.Id}\t{this.Status.PadRight(12, ' ')}\t{this.Region}\t\t{((GameType)this.GameType).ToString().PadRight(12, ' ')}\t{this.MaxPlayers}";
    }

    public static string CreateTableString => "CREATE TABLE IF NOT EXISTS Servers (id uuid, status text, region text, gametype int, maxplayers int, PRIMARY KEY ((gametype, region), id))";
    public static string ColumnsNamesString => $"{nameof(Server.Id)}\t\t\t\t\t{nameof(Server.Status)}\t\t{nameof(Server.Region)}\t\t{nameof(Server.GameType)}\t{nameof(Server.MaxPlayers)}";
}