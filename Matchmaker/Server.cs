public record Server
{
    public Guid Id {get; init;}

    public string IPAddr {get; init;}

    public string Region {get; init;}

    public GameType GameType {get; init;}

    public int MaxPlayers {get; init;}
}