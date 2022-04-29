using System.Collections.Concurrent;

namespace WebApi
{
    public class AnimationInfo
    {

        public static AnimationInfo Instanse = new();

        static AnimationInfo()
        {
            AnimateSnakeGameService.InitAnimationInfo(Instanse);
        }

        public ConcurrentDictionary<char, string> MapCharToSprite { get; set; } = new()
        {
            ['H'] = "/Images/HeadSnake.png",
            ['B'] = "/Images/BodySnake.png",
            ['A'] = "/Images/Apple.png",
            ['W'] = "/Images/Terrain.png",
            ['S'] = "/Images/Spawn.png",
        };
    }
}
