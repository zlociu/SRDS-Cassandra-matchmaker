public class PlayerGerenator
{
    private Random _rnd;

    public PlayerGerenator()
    {
        _rnd = new Random();
    }

    public List<Player> GeneratePlayers(int count)
    {
        if(count < 1) throw new ArgumentOutOfRangeException();
        List<Player> players = new List<Player>(count);

        for(int i = 0; i < count; i++)
        {
            Player player = new Player{
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
                Rank = _rnd.Next(1,11) * 100,
                Region = (_rnd.Next(5)) switch 
                {
                    var x when x < 2 => "Europe",
                    var x when x < 4 => "USA",
                    _ => "Asia"
                }

            };
            players.Add(player);
        }

        return players;
    }
}