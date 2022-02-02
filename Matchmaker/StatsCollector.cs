public class StatsCollector
{

    private List<PlayerWaitTime> playerWaitTimes = new();

    public void RecordPlayerWaitTime(MatchSuggestion matchSuggestion, DateTimeOffset gameStartTime)
    {
        var timeToAssign = (matchSuggestion.SuggestionTimestamp - matchSuggestion.RequestTimestamp).TotalMilliseconds;
        var timeToPlay = (gameStartTime - matchSuggestion.RequestTimestamp).TotalMilliseconds;
        playerWaitTimes.Add(new PlayerWaitTime(matchSuggestion.PlayerId, timeToAssign, timeToPlay));
    }

    public void PrintSummary()
    {
        var recordsByPlayerId = playerWaitTimes.GroupBy(p => p.PlayerId);
        var playersAssignedToMultipleServers = recordsByPlayerId.Where(g => g.Count() > 1).Count();
        var timeToAssignStats = GetStats(playerWaitTimes.Select(p => p.timeToAssign));
        var timeToPlayStats = GetStats(playerWaitTimes.Select(p => p.timeToPlay));
        System.Console.WriteLine("--- General stats ---");
        System.Console.WriteLine($"Number of players who joined a game: {recordsByPlayerId.Count()}");
        System.Console.WriteLine($"Number of players assigned to multiple servers: {playersAssignedToMultipleServers}");
        System.Console.WriteLine("--- Time to assign ---");
        System.Console.WriteLine($"Mean: {timeToAssignStats.Mean}");
        System.Console.WriteLine($"Min: {timeToAssignStats.Min}");
        System.Console.WriteLine($"Max: {timeToAssignStats.Max}");
        System.Console.WriteLine($"P99: {timeToAssignStats.P99}");
        System.Console.WriteLine("--- Time to play ---");
        System.Console.WriteLine($"Mean: {timeToPlayStats.Mean}");
        System.Console.WriteLine($"Min: {timeToPlayStats.Min}");
        System.Console.WriteLine($"Max: {timeToPlayStats.Max}");
        System.Console.WriteLine($"P99: {timeToPlayStats.P99}");
    }

    private Stats GetStats(IEnumerable<double> values)
    {
        var sortedValues = values.OrderBy(t => t);
        var mean = sortedValues.Average();
        var min = sortedValues.First();
        var max = sortedValues.Last();
        var p99 = sortedValues.Skip((int)(sortedValues.Count() * 0.99)).First();
        return new Stats(mean, min, max, p99);
    }

    private readonly record struct PlayerWaitTime(Guid PlayerId, double timeToAssign, double timeToPlay);
    private readonly record struct Stats(double Mean, double Min, double Max, double P99);
}