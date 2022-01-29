public class ServerGenerator
{
    private Random _rnd;

    public ServerGenerator()
    {
        _rnd = new Random();
    }

    public IEnumerable<Server> GenerateServers(int count)
    {
        if (count < 1) throw new ArgumentOutOfRangeException();
        List<Server> servers = new(count);

        for (int i = 0; i < count; i++)
        {
            Server server = new Server
            {
                Id = Guid.NewGuid(),
                GameType = (_rnd.Next(12)) switch
                {
                    var x when x < 2 => GameType.TeamDeathmatch,
                    var x when x < 4 => GameType.Deathmatch,
                    var x when x < 6 => GameType.CrashSite,
                    var x when x < 8 => GameType.Extraction,
                    var x when x < 10 => GameType.Spears,
                    var x when x < 12 => GameType.HunterMode,
                    _ => GameType.TeamDeathmatch
                },
                MaxPlayers = _rnd.Next(3, 5) * 4,
                Region = (_rnd.Next(5)) switch
                {
                    var x when x == 0 => Region.EastEurope,
                    var x when x == 1 => Region.WestEurope,
                    var x when x == 2 => Region.EastUS,
                    var x when x == 3 => Region.WestUS,
                    _ => Region.Asia
                },
                Status = ServerStatus.WaitingForPlayers

            };
            servers.Add(server);
        }

        return servers;
    }
}