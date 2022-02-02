using Cassandra;
using Cassandra.Mapping;

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

        session.CreateKeyspaceIfNotExists("matchmaker", new Dictionary<string, string>{
                                                { "class", ReplicationStrategies.SimpleStrategy },
                                                { "replication_factor", $"{replicationFactor}" }
                                            });

        session = cluster.Connect("matchmaker");

        Mapper = new Mapper(session);

        Mapper.DropTableIfExists("Servers");
        Mapper.DropTableIfExists("MatchRequests");
        Mapper.DropTableIfExists("MatchSuggestions");

        Mapper.Execute(Server.CreateTableString);
        Mapper.Execute(MatchRequest.CreateTableString);
        Mapper.Execute(MatchSuggestion.CreateTableString);

        Mapper.CreateIndex("MatchRequests", "priority");
        Mapper.CreateIndex("Servers", "status");
    }
}