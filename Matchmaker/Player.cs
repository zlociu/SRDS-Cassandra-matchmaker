


public record Player
{
    public Guid Id {get; init;}

    public Region Region {get; init;}

    public GameType GameType {get; init;}

    public int Rank {get; init;}

    public override string ToString()
    {
        return $"{this.Id}\t{this.Region}\t{((GameType)this.GameType).ToString().PadRight(15,' ')}\t{this.Rank}";
    }

    public static string CreateTableString => "CREATE TABLE IF NOT EXISTS Players (id uuid, region text, gametype int, rank int, PRIMARY KEY (id))";
    public static string ColumnsNamesString => $"{nameof(Player.Id)}\t\t\t\t\t{nameof(Player.Region)}\t{nameof(Player.GameType)}\t{nameof(Player.Rank)}";
}