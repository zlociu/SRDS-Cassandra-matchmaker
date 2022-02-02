using Cassandra;
using Cassandra.Mapping;

public class MatchmakerSimulator
{
    private IMapper mapper;
    public bool CanStop { get; set; } = false;

    public MatchmakerSimulator(string cassandraAddress, int cassandraPort)
    {
        var cluster = Cluster.Builder()
                             .AddContactPoint(cassandraAddress)
                             .WithPort(cassandraPort)
                             .Build();
        var session = cluster.Connect("matchmaker");
        mapper = new Mapper(session);
    }

    public void SimulateMatchmaker()
    {
        var matchmaker = CreateMatchmaker();
        var matchmakerLoop = matchmaker.MatchmakerLoop().GetEnumerator();
        var remainingRequests = 0;
        do
        {
            matchmakerLoop.MoveNext();
            remainingRequests = mapper.Fetch<MatchRequest>().ToList().Count();
        } while (remainingRequests > 0 || !CanStop);
    }

    private Matchmaker CreateMatchmaker()
    {
        var serverRepository = new CassandraServerRepository(mapper, ConsistencyLevel.One);
        var matchRequestRepository = new CassandraMatchRequestRepository(mapper, ConsistencyLevel.One);
        var matchSuggestionRepository = new CassandraMatchSuggestionRepository(mapper, ConsistencyLevel.One);

        return new Matchmaker(
            serverRepository,
            matchRequestRepository,
            matchSuggestionRepository
        );
    }
}