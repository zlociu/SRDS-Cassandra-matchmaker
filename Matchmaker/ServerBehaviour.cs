public class ServerBehaviour
{
    private IServerRepository serverRepository;
    private IMatchRequestRepository matchRequestRepository;
    private IMatchSuggestionRepository matchSuggestionRepository;
    private Server server;
    private const int playerCheckIntervalMillis = 100;

    public ServerBehaviour(
        IServerRepository serverRepository,
        IMatchRequestRepository matchRequestRepository,
        IMatchSuggestionRepository matchSuggestionRepository,
        Region region,
        GameType gameType,
        int maxPlayers
    )
    {
        this.serverRepository = serverRepository;
        this.matchRequestRepository = matchRequestRepository;
        this.matchSuggestionRepository = matchSuggestionRepository;
        server = Register(region, gameType, maxPlayers);
    }

    ~ServerBehaviour()
    {
        Unregister();
    }

    public void StartNewGame()
    {
        SetStatus(ServerStatus.WaitingForPlayers);
        var assignedPlayers = WaitForPlayers();
        SetStatus(ServerStatus.InGame);
        matchSuggestionRepository.RemoveByServerId(server.Id);
        var orderedPlayers = assignedPlayers.OrderBy(player => player.RequestTimestamp);
        var playersWithinLimit = orderedPlayers.Take(server.MaxPlayers);
        var excessivePlayers = orderedPlayers.Skip(server.MaxPlayers);
        AcceptPlayers(playersWithinLimit.ToList());
        RestoreMatchRequests(excessivePlayers.ToList());
        PlayGame();
        SetStatus(ServerStatus.Idle);
    }

    private Server Register(Region region, GameType gameType, int maxPlayers)
    {
        var server = new Server
        {
            Id = Guid.NewGuid(),
            Status = ServerStatus.Idle,
            Region = region,
            GameType = gameType,
            MaxPlayers = maxPlayers
        };
        serverRepository.Upsert(server);
        return server;
    }

    private void Unregister()
    {
        serverRepository.Remove(server.Id, server.GameType, server.Region);
    }

    private void SetStatus(ServerStatus newStatus)
    {
        serverRepository.SetStatus(server.Id, server.GameType, server.Region, newStatus);
    }

    private List<MatchSuggestion> WaitForPlayers()
    {
        var players = new List<MatchSuggestion>();
        while (players.Count() < server.MaxPlayers)
        {
            Thread.Sleep(playerCheckIntervalMillis);
            players = matchSuggestionRepository.GetByServerIds(new List<Guid> { server.Id }).ToList();
        }
        return players;
    }

    private void AcceptPlayers(List<MatchSuggestion> players)
    {
        // TODO: callback to players
    }

    private void RestoreMatchRequests(List<MatchSuggestion> matchSuggestions)
    {
        foreach (var matchSuggestion in matchSuggestions)
        {
            matchRequestRepository.Upsert(matchSuggestion.ToMatchRequest());
        }
    }

    private void PlayGame()
    {
        var gameDurationMillis = 2 * 60 * 1000; // 2 min
        Thread.Sleep(gameDurationMillis);
    }
}