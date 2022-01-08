


public record Player
{
    public Guid Id {get; init;}

    public string Region {get; init;}

    public GameType GameType {get; init;}

    public int Rank {get; init;}
}