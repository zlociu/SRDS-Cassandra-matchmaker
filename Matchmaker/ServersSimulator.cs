public class ServersSimulator
{
    public Task SimulateServers(List<ServerBehaviour> servers)
    {
        var serverTasks = servers.Select(server => server.StartNewGame());
        return Task.WhenAll(serverTasks);
    }
}