using System;
using Cassandra;
using Cassandra.Mapping;


PlayerGerenator playerGerenator = new();
var list =  playerGerenator.GeneratePlayers(100);
Console.WriteLine(Player.ColumnsNamesString);
foreach (var item in list)
{
    Console.WriteLine(item.ToString());
}

ServerGenerator serverGenerator = new();
var list2 =  serverGenerator.GenerateServers(10);
Console.WriteLine($"\n{Server.ColumnsNamesString}");
foreach (var item in list2)
{
    Console.WriteLine(item.ToString());
}


MappingConfiguration.Global.Define<MatchmakerMappings>();

var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(9043)
                     .Build();

var tmp = cluster.Connect();

tmp.CreateKeyspaceIfNotExists("matchmaker", new Dictionary<string, string>{ { "class", ReplicationStrategies.SimpleStrategy }, { "replication_factor", "3" } });

var session = cluster.Connect("matchmaker");
Console.WriteLine(session.Keyspace);

IMapper mapper = new Mapper(session);


mapper.Execute("DROP TABLE IF EXISTS Players");
mapper.Execute("DROP TABLE IF EXISTS Servers");
mapper.Execute("DROP TABLE IF EXISTS MatchRequests");
mapper.Execute("DROP TABLE IF EXISTS MatchSuggestions");

mapper.Execute(Player.CreateTableString);
mapper.Execute(Server.CreateTableString);
mapper.Execute(MatchRequest.CreateTableString);
mapper.Execute(MatchSuggestion.CreateTableString);

/*
mapper.InsertIfNotExists(new Player{
    Id = Guid.NewGuid(),
    Region = "Europe",
    Rank = 500,
    GameType = GameType.TeamDeathmatch
});

mapper.InsertIfNotExists(new Player{
    Id = Guid.NewGuid(),
    Region = "Asia",
    Rank = 300,
    GameType = GameType.Deathmatch
});

mapper.InsertIfNotExists(new Player{
    Id = Guid.NewGuid(),
    Region = "Europe",
    Rank = 550,
    GameType = GameType.TeamDeathmatch
});

mapper.InsertIfNotExists(new Player{
    Id = Guid.NewGuid(),
    Region = "Asia",
    Rank = 700,
    GameType = GameType.Extraction
});

mapper.InsertIfNotExists(new Server{
    Id = Guid.NewGuid(),
    Region = "Europe",
    IPAddr = "77.66.55.44",
    MaxPlayers = 16,
    GameType = GameType.Deathmatch
});

mapper.InsertIfNotExists(new Server{
    Id = Guid.NewGuid(),
    Region = "Asia",
    IPAddr = "222.100.0.33",
    MaxPlayers = 14,
    GameType = GameType.Extraction
});
*/

//session.Execute("INSERT INTO Players (Id, Region, GameType, Rank) VALUES (1,'Europe', 0, 500) IF NOT EXISTS");

//var rows = session.Execute("SELECT Id, Region, GameType, Rank FROM Players");

/*
var p_rows = mapper.Fetch<Player>("WHERE rank > 300 ALLOW FILTERING");

Console.WriteLine("Players");
Console.WriteLine(Player.ColumnsNamesString);
Console.WriteLine("-----------------------------------------------------------------------------------");

foreach(var row in p_rows)
{
    Console.WriteLine(row.ToString());
}

var s_rows = mapper.Fetch<Server>();

Console.WriteLine("\nServers");
Console.WriteLine(Server.ColumnsNamesString);
Console.WriteLine("----------------------------------------------------------------------------------------------------");

foreach(var row in s_rows)
{
    Console.WriteLine(row.ToString());
}

*/