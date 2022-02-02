public class MatchRequestGenerator
{
    private IMatchRequestRepository matchRequestRepository;
    private Random random;

    public MatchRequestGenerator(
        IMatchRequestRepository matchRequestRepository
    )
    {
        this.matchRequestRepository = matchRequestRepository;
        random = new Random();
    }

    public void Generate(int matchRequestsPerRegionAndGameType)
    {
        var allGameTypes = Enum.GetValues<GameType>();
        var allRegions = Enum.GetValues<Region>();
        var allCombinations = from gameType in allGameTypes
                              from region in allRegions
                              select (gameType, region);
        foreach (var (gameType, region) in allCombinations)
        {
            Generate(matchRequestsPerRegionAndGameType, gameType, region);
        }
    }

    private void Generate(int matchRequests, GameType gameType, Region region)
    {
        for (var i = 0; i < matchRequests; i++)
        {
            GenerateMatchRequest(gameType, region);
        }
    }

    private void GenerateMatchRequest(GameType gameType, Region region)
    {
        var matchRequest = new MatchRequest
        {
            PlayerId = Guid.NewGuid(),
            PlayerRank = random.Next(3000),
            Region = region,
            GameType = gameType,
            RequestTimestamp = DateTimeOffset.Now,
            Priority = 0
        };
        matchRequestRepository.Upsert(matchRequest);
    }
}