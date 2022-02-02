public record Server
{
    public Guid Id { get; init; }
    public ServerStatus Status { get; init; }
    public Region Region { get; init; }
    public GameType GameType { get; init; }
    public int MaxPlayers { get; init; }
}