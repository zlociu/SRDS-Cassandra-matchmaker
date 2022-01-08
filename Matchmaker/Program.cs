using System;
using Cassandra;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var cluster = Cluster.Builder()
                     .AddContactPoint("srds")
                     .Build();


var session = cluster.Connect("matchmaker");
