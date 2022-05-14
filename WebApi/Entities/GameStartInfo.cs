namespace WebApi
{
    public class GameStartInfo
    {
        public string GameId { get; set; }
        public string MapName { get; set; }
        public int ApplesCount { get; set; }
        public int TicksToEnd { get; set; }
        public string[] Players { get; set; }
        public bool IsOver { get; private set; } = false;
        public bool IsInProgress { get; private set; } = false;
        public bool IsNotStarted => !IsOver && !IsInProgress;

        public void MarkAsOver() { IsOver = true; IsInProgress = false; }
        public void MarkAsInProgress() { IsInProgress = true; }
    }
}
