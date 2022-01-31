public class Matchmaker
{
    private const double PriorityWeight = 1.0;
    private const double RankWeight = 1.0;
    private const double FullnessWeight = 1.0;
    private const double MinimalMatchQuality = 0;
    private const int RequestBatchSize = 100;
    private IServerRepository serverRepository;
    private IMatchRequestRepository matchRequestRepository;
    private IMatchSuggestionRepository matchSuggestionRepository;
    private List<(GameType, Region)> processingOrder;

    public Matchmaker(
        IServerRepository serverRepository,
        IMatchRequestRepository matchRequestRepository,
        IMatchSuggestionRepository matchSuggestionRepository
    )
    {
        this.serverRepository = serverRepository;
        this.matchRequestRepository = matchRequestRepository;
        this.matchSuggestionRepository = matchSuggestionRepository;
        processingOrder = GenerateProcessingOrder().ToList();
    }

    public Task MatchmakerLoop()
    {
        //while (true)
        //{
            foreach (var (gameType, region) in processingOrder)
            {
                FindAndAssignMatches(gameType, region, RequestBatchSize);
                //yield return (gameType, region);
            }
            return Task.CompletedTask;
        //}
    }

    private IEnumerable<(GameType, Region)> GenerateProcessingOrder()
    {
        var allGameTypes = Enum.GetValues<GameType>();
        var allRegions = Enum.GetValues<Region>();
        var allCombinations = from gameType in allGameTypes
                              from region in allRegions
                              select (gameType, region);
        var random = new Random();
        return allCombinations.OrderBy(_ => random.Next());
    }

    private void FindAndAssignMatches(GameType gameType, Region region, int requestLimit)
    {
        var possibleMatches = FindMatches(gameType, region, requestLimit).OrderByDescending(m => m.Quality);
        var cnt = possibleMatches.Count();
        AssignMatches(possibleMatches);
    }

    private IEnumerable<PossibleMatch> FindMatches(GameType gameType, Region region, int requestLimit)
    {
        var servers = serverRepository.GetAvailableByGameTypeAndRegion(gameType, region).ToList();
        var assignedPlayers = matchSuggestionRepository.GetByServerIds(servers.Select(s => s.Id)).ToList();
        var serverSummaries = GetServerSummaries(servers, assignedPlayers).ToList();
        var requests = matchRequestRepository.GetByGameTypeAndRegion(gameType, region, requestLimit).ToList();
        return FindPossibleMatches(requests, serverSummaries);
    }

    private void AssignMatches(IOrderedEnumerable<PossibleMatch> possibleMatches)
    {
        var assignedPlayers = new HashSet<Guid>();
        var assignedPlayerCountsByServer = new Dictionary<Guid, int>();
        foreach (var match in possibleMatches)
        {
            if (assignedPlayers.Contains(match.Request.PlayerId)) continue;
            var assignedPlayerCount = assignedPlayerCountsByServer.GetValueOrDefault(match.Server.Id);
            if (match.Server.PlayerCount + assignedPlayerCount >= match.Server.MaxPlayers) continue;

            Assign(match.Request, match.Server.Id);
            assignedPlayers.Add(match.Request.PlayerId);
            assignedPlayerCountsByServer[match.Server.Id] = assignedPlayerCount + 1;
        }
    }

    private IEnumerable<ServerSummary> GetServerSummaries(IEnumerable<Server> servers, IEnumerable<MatchSuggestion> assignedPlayers)
    {
        return servers.GroupJoin(assignedPlayers, s => s.Id, p => p.ServerId, (s, ap) => GetServerSummary(s, ap));
    }

    private ServerSummary GetServerSummary(Server server, IEnumerable<MatchSuggestion> assignedPlayers)
    {
        return new ServerSummary(
            Id: server.Id,
            PlayerCount: assignedPlayers.Count(),
            MaxPlayers: server.MaxPlayers,
            MeanRank: assignedPlayers.Average(p => (int?)p.PlayerRank)
        );
    }

    private IEnumerable<PossibleMatch> FindPossibleMatches(IEnumerable<MatchRequest> matchRequests, IEnumerable<ServerSummary> availableServers)
    {
        var allMatches = from matchRequest in matchRequests
                         from serverSummary in availableServers
                         select new PossibleMatch(
                             Request: matchRequest,
                             Server: serverSummary,
                             Quality: GetMatchQuality(matchRequest, serverSummary)
                         );
        return allMatches.Where(m => m.Quality >= MinimalMatchQuality);
    }

    private double GetMatchQuality(MatchRequest matchRequest, ServerSummary serverSummary)
    {
        return
            // if a player waits for too long, any match is better than none
            PriorityWeight * matchRequest.Priority +
            // PlayerRank closer to the mean means better match, 0 if server is empty
            RankWeight * Math.Abs(matchRequest.PlayerRank - serverSummary.MeanRank ?? matchRequest.PlayerRank) +
            // Try to fill a server with some players already waiting before assigning to an empty one
            FullnessWeight * (double)serverSummary.PlayerCount / serverSummary.MaxPlayers;
    }

    private void Assign(MatchRequest matchRequest, Guid serverId)
    {
        var matchSuggestion = matchRequest.ToMatchSuggestion(serverId);
        matchSuggestionRepository.Upsert(matchSuggestion);
        matchRequestRepository.RemoveByPlayerId(matchRequest.PlayerId, matchRequest.Region, matchRequest.GameType);
    }

    private readonly record struct ServerSummary(Guid Id, int PlayerCount, int MaxPlayers, double? MeanRank);
    private readonly record struct PossibleMatch(MatchRequest Request, ServerSummary Server, double Quality);
}