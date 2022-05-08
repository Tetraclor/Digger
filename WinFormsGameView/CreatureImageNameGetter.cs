using SnakeGame2;

namespace WinFormsGameView
{
    public class CommonImageNameGetter
    {
        public string Get(SnakeGame2.HeadSnake _) => "HeadSnake.png";
        public string Get(SnakeGame2.BodySnake _) => "BodySnake.png";
        public string Get(SnakeGame2.Apple _) => "Apple.png";
        public string Get(SnakeGame2.Wall _) => "Terrain.png";
        public string Get(SnakeGame2.Spawn _) => "Spawn.png";
    }
}