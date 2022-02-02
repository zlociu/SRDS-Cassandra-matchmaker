public class PriorityManager
{

    private IMatchRequestRepository matchRequestRepository;
    private const int priorityUpdateIntervalMillis = 250;
    private int[] priorityThresholds = { 250, 500, 1000, 3000 };

    public PriorityManager(
        IMatchRequestRepository matchRequestRepository
    )
    {
        this.matchRequestRepository = matchRequestRepository;
    }

    public async Task PriorityManagerLoop()
    {
        while (true)
        {
            UpdatePriorities();
            await Task.Delay(priorityUpdateIntervalMillis);
        }
    }

    private void UpdatePriorities()
    {
        var priorityValues = Enumerable.Range(0, priorityThresholds.Count()).Reverse();
        foreach (var priority in priorityValues)
        {
            var matchRequests = matchRequestRepository.GetByPriority(priority, limit: 100_000);
            UpdatePriorities(matchRequests.ToList());
        }
    }

    private void UpdatePriorities(List<MatchRequest> matchRequests)
    {
        var currentTime = DateTimeOffset.Now;
        foreach (var matchRequest in matchRequests)
        {
            var newPriority = GetPriority(matchRequest, currentTime);
            if (newPriority != matchRequest.Priority)
            {
                matchRequestRepository.Upsert(matchRequest with { Priority = newPriority });
            }
        }
    }

    private int GetPriority(MatchRequest matchRequest, DateTimeOffset currentTime)
    {
        var waitingTime = currentTime.Subtract(matchRequest.RequestTimestamp).TotalMilliseconds;
        var thresholdIndex = Array.BinarySearch(priorityThresholds, (int)waitingTime);
        return thresholdIndex >= 0 ? thresholdIndex : ~thresholdIndex;
    }
}