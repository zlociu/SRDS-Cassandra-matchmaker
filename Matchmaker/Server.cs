public record Server
{
    public Guid Id {get; init;}

    public string IPAddr {get; init;}

    public string Region {get; init;}

    public GameType GameType {get; init;}

    public int MaxPlayers {get; init;}

    public override string ToString()
    {
        return $"{this.Id}\t{this.IPAddr}\t{this.Region}\t\t{(GameType)this.GameType}\t{this.MaxPlayers}";
    }

    public static string CreateTableString => "CREATE TABLE IF NOT EXISTS Servers (id uuid, ipaddr text, region text, gametype int, maxplayers int, PRIMARY KEY (id))";
    public static string ColumnsNamesString => $"{nameof(Server.Id)}\t\t\t\t\t{nameof(Server.IPAddr)}\t\t{nameof(Server.Region)}\t\t{nameof(Server.GameType)}\t{nameof(Server.MaxPlayers)}";
}