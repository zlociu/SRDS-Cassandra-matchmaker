public class ServerBehaviour
{
    private IServerRepository serverRepository;
    private IMatchRequestRepository matchRequestRepository;
    private IMatchSuggestionRepository matchSuggestionRepository;
    private StatsCollector statsCollector;
    private Server server;
    private const int playerCheckIntervalMillis = 100;

    public ServerBehaviour(
        IServerRepository serverRepository,
        IMatchRequestRepository matchRequestRepository,
        IMatchSuggestionRepository matchSuggestionRepository,
        StatsCollector statsCollector,
        Region region,
        GameType gameType,
        int maxPlayers
    )
    {
        this.serverRepository = serverRepository;
        this.matchRequestRepository = matchRequestRepository;
        this.matchSuggestionRepository = matchSuggestionRepository;
        this.statsCollector = statsCollector;
        server = Register(region, gameType, maxPlayers);
    }

    ~ServerBehaviour()
    {
        Unregister();
    }

    public async Task StartNewGame()
    {
        SetStatus(ServerStatus.WaitingForPlayers);
        var assignedPlayers = await WaitForPlayers();
        SetStatus(ServerStatus.InGame);
        matchSuggestionRepository.RemoveByServerId(server.Id);
        var orderedPlayers = assignedPlayers.OrderBy(player => player.RequestTimestamp);
        var playersWithinLimit = orderedPlayers.Take(server.MaxPlayers);
        var excessivePlayers = orderedPlayers.Skip(server.MaxPlayers);
        AcceptPlayers(playersWithinLimit.ToList());
        RestoreMatchRequests(excessivePlayers.ToList());
        await PlayGame();
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

    private async Task<List<MatchSuggestion>> WaitForPlayers()
    {
        var players = new List<MatchSuggestion>();
        while (players.Count() < server.MaxPlayers)
        {
            await Task.Delay(playerCheckIntervalMillis);
            players = matchSuggestionRepository.GetByServerIds(new List<Guid> { server.Id }).ToList();
        }
        return players;
    }

    private void AcceptPlayers(List<MatchSuggestion> players)
    {
        var currentTime = DateTimeOffset.Now;
        foreach (var player in players)
        {
            statsCollector.RecordPlayerWaitTime(player, currentTime);
        }
    }

    private void RestoreMatchRequests(List<MatchSuggestion> matchSuggestions)
    {
        foreach (var matchSuggestion in matchSuggestions)
        {
            matchRequestRepository.Upsert(matchSuggestion.ToMatchRequest());
        }
    }

    private async Task PlayGame()
    {
        var gameDurationMillis = 2 * 60 * 1000; // 2 min
        await Task.Delay(gameDurationMillis);
    }
}