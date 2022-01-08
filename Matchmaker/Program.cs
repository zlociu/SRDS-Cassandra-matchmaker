using System;
using Cassandra;

var cluster = Cluster.Builder()
                     .AddContactPoint("127.0.0.1")
                     .WithPort(9042)
                     .Build();

var tmp = cluster.Connect();

tmp.CreateKeyspaceIfNotExists("matchmaker", new Dictionary<string, string>{ { "class", ReplicationStrategies.SimpleStrategy }, { "replication_factor", "3" } });

var session = cluster.Connect("matchmaker");
Console.WriteLine(session.Keyspace);

//session.UserDefinedTypes.Define(UdtMap.For<Player>());

session.Execute("CREATE TABLE IF NOT EXISTS Players (Id int, Region text, GameType int, Rank int, PRIMARY KEY (Id))");

session.Execute("INSERT INTO Players (Id, Region, GameType, Rank) VALUES (1,'Europe', 0, 500) IF NOT EXISTS");

var rows = session.Execute("SELECT Id, Region, GameType, Rank FROM Players");

foreach(var row in rows)
{
    Console.WriteLine(row.GetValue<int>(0));
    Console.WriteLine(row.GetValue<string>(1));
    Console.WriteLine(row.GetValue<int>(2));
    Console.WriteLine(row.GetValue<int>(3));

    Console.WriteLine(row[0]);
    Console.WriteLine(row[1]);
    Console.WriteLine(row[2]);
    Console.WriteLine(row[3]);

    Console.WriteLine(row.GetValue<int>("id"));
    Console.WriteLine(row.GetValue<string>("region"));
    Console.WriteLine(row.GetValue<int>("gametype"));
    Console.WriteLine(row.GetValue<int>("rank"));
    
}



