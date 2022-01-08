using System;
using Cassandra;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var cluster = Cluster.Builder()
                     .AddContactPoint("0.0.0.0:9042")
                     .Build();


var tmp = cluster.Connect();

tmp.CreateKeyspaceIfNotExists("matchmaker", new Dictionary<string, string>{ { "class", ReplicationStrategies.SimpleStrategy }, { "replication_factor", "3./" } });

var session = cluster.Connect("matchmaker");



