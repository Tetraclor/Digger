using GameDigger;
using GameSnake;
using SnakeGame2;

namespace WinFormsGameView
{
    public class CommonImageNameGetter
    {
        public string Get(Terrain _) => "Terrain.png";
        public string Get(Digger _) => "Digger.png";
        public string Get(GameSnake.HeadSnake _) => "HeadSnake.png";
        public string Get(GameSnake.BodySnake _) => "BodySnake.png";
        public string Get(GameSnake.Apple _) => "Apple.png";
        public string Get(Wall _) => "Terrain.png";
        public string Get(Spawn _) => "Spawn.png";
        public string Get(SnakeGame2.HeadSnake _) => "HeadSnake.png";
        public string Get(SnakeGame2.BodySnake _) => "BodySnake.png";
        public string Get(SnakeGame2.Apple _) => "Apple.png";
    }
}