using Cassandra;
using Cassandra.Mapping;
using Cassandra.Data.Linq;

public class CassandraSetup
{
    public IMapper Mapper { get; }

    public CassandraSetup(string address, int port, int replicationFactor)
    {
        MappingConfiguration.Global.Define<MatchmakerMappings>();
        //Cassandra.Diagnostics.CassandraTraceSwitch.Level = TraceLevel.Info;
        //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

        var cluster = Cluster.Builder()
                             .AddContactPoint(address)
                             .WithPort(port)
                             .Build();

        var session = cluster.Connect();
        session.CreateKeyspaceIfNotExists("matchmaker",
            new Dictionary<string, string>{
                { "class", ReplicationStrategies.SimpleStrategy },
                { "replication_factor", $"{replicationFactor}" }
            });
        session = cluster.Connect("matchmaker");

        Mapper = new Mapper(session);

        var serversTable = new Table<Server>(session);
        Mapper.DropTableIfExists("Servers");
        serversTable.CreateIfNotExists();

        var matchRequestsTable = new Table<MatchRequest>(session);
        Mapper.DropTableIfExists("MatchRequests");
        matchRequestsTable.CreateIfNotExists();

        var matchSuggestionsTable = new Table<MatchSuggestion>(session);
        Mapper.DropTableIfExists("MatchSuggestions");
        matchSuggestionsTable.CreateIfNotExists();

        Mapper.CreateIndex("MatchRequests", "priority");
        Mapper.CreateIndex("Servers", "status");
    }
}