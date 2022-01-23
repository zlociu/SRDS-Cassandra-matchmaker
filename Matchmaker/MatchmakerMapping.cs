using Cassandra.Mapping;


public class MatchmakerMappings : Mappings
{

    public MatchmakerMappings()
    {
        /*
        For<Player>()
            .TableName("players")
            .PartitionKey(u => u.Id)
            .Column(c => c.Id, n => n.WithName("id"))
            .Column(c => c.GameType, n=> n.WithName("gametype").WithDbType<int>())
            .Column(c => c.Region, n => n.WithName("region"))
            .Column(c => c.Rank, n => n.WithName("rank"));
        */

        For<Server>()
            .TableName("Servers")
            .PartitionKey(u => u.Id)
            .Column(c => c.Id, n => n.WithName("id"))
            .Column(c => c.Status, n => n.WithName("status"))
            .Column(c => c.MaxPlayers, n => n.WithName("maxplayers"))
            .Column(c => c.Region, n => n.WithName("region").WithDbType<int>())
            .Column(c => c.GameType, n => n.WithName("gametype").WithDbType<int>());

        For<MatchRequest>()
            .TableName("MatchRequests")
            .PartitionKey(u => u.GameType, u => u.Region)
            .ClusteringKey(u => u.PlayerId)
            .Column(c => c.PlayerId, n => n.WithName("playerid"))
            .Column(c => c.Region, n => n.WithName("region").WithDbType<int>())
            .Column(c => c.GameType, n => n.WithName("gametype").WithDbType<int>())
            .Column(c => c.RequestTimestamp, n => n.WithName("requesttimestamp"))
            .Column(c => c.Priority, n => n.WithName("priority"));


        For<MatchSuggestion>()
            .TableName("MatchSuggestions")
            .PartitionKey(u => u.PlayerId)
            .Column(c => c.PlayerId, n => n.WithName("playerid"))
            .Column(c => c.Region, n => n.WithName("region").WithDbType<int>())
            .Column(c => c.GameType, n => n.WithName("gametype").WithDbType<int>())
            .Column(c => c.PlayerRank, n => n.WithName("playerrank"))
            .Column(c => c.RequestTimestamp, n => n.WithName("requesttimestamp"))
            .Column(c => c.ServerId, n => n.WithName("serverid"));

    }

}