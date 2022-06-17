namespace WebApi
{
    public class GameStartInfo
    {
        public string GameId { get; set; }
        public string MapName { get; set; }
        public int ApplesCount { get; set; }
        public int TicksToEnd { get; set; }
        public int TickMs { get; set; }
        public string[] UserNames { get; set; } 
    }
}
