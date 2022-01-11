using Cassandra.Mapping;


public class MatchmakerMappings : Mappings
{

    public MatchmakerMappings()
    {
        For<Player>()
            .TableName("players")
            .PartitionKey(u => u.Id)
            .Column(c => c.Id, n => n.WithName("id"))
            .Column(c => c.GameType, n=> n.WithName("gametype").WithDbType<int>())
            .Column(c => c.Region, n => n.WithName("region"))
            .Column(c => c.Rank, n => n.WithName("rank"));

        For<Server>()
            .TableName("servers")
            .PartitionKey(u => u.Id)
            .Column(c => c.Id, n => n.WithName("id"))
            .Column(c => c.IPAddr, n => n.WithName("ipaddr"))
            .Column(c => c.MaxPlayers, n => n.WithName("maxplayers"))
            .Column(c => c.Region, n => n.WithName("region"))
            .Column(c => c.GameType, n => n.WithName("gametype").WithDbType<int>());
    }
    
}