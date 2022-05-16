using Common;
using GameCore;
using SnakeGame2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebApi
{
    public class AnimateSnakeGameService : SnakeGame2.SnakeGameService
    {
        static readonly ConcurrentDictionary<string, char> spriteNameToChar = new();
        static readonly ConcurrentDictionary<string, char> spriteNameToCharForUserSnake = new();

        static AnimateSnakeGameService()
        {
            var ch = 'a';
            foreach (var e in new DirectoryInfo("wwwroot/Images/Snake-Green").GetFiles("*.png"))
            {
                var imagename = e.Name.ToLower();
                ch = (char)(ch + 1);
                spriteNameToChar[imagename] = ch;
            }
            foreach (var e in new DirectoryInfo("wwwroot/Images/Snake").GetFiles("*.png"))
            {
                var imagename = e.Name.ToLower();
                ch = (char)(ch + 1);
                spriteNameToCharForUserSnake[imagename] = ch;
            }
        }

        public static void InitAnimationInfo(AnimationInfo initAnimateInfo)
        {
            foreach (var (imagename, ch) in spriteNameToChar)
            {
                initAnimateInfo.MapCharToSprite[ch] = $"/Images/Snake-Green/{imagename}";
            }
            foreach (var (imagename, ch) in spriteNameToCharForUserSnake)
            {
                initAnimateInfo.MapCharToSprite[ch] = $"/Images/Snake/{imagename}";
            }
        }

        public AnimateSnakeGameService(Func<SnakeGameService, Snake> getUserSnake, int applesCount, string mapString) : base(applesCount, mapString)
        {
            GetUserSnake = getUserSnake;
        }

        public Func<SnakeGameService, Snake> GetUserSnake { get; }

        public override string ToStringMap()
        {
            var w = GameState.MapWidth;
            var h = GameState.MapHeight;
            var map = GameState.Map;

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var creature = map[j, i];
                    if (creature == null || creature is BodySnake || creature is HeadSnake)
                        stringBuilder.Append(' ');
                    else if (creature is Apple)
                        stringBuilder.Append('A');
                    else if (creature is Wall)
                        stringBuilder.Append('W');
                    else if (creature is Spawn)
                        stringBuilder.Append('S');
                    else
                        stringBuilder.Append(' ');
                }
                if(i != h - 1)
                    stringBuilder.Append('\n');
            }

            var stringMap = stringBuilder.ToString().ToCharArray();

            var userSnake = GetUserSnake(this);

            if (userSnake != null)
            {
                DrawSnake(userSnake.AllPoints, (n, p) => Draw(n, p, spriteNameToCharForUserSnake));
            }
               

            SnakeSpawners
                .Where(v => v.IsActive && v.SpawnedSnake != userSnake)
                .ToList()
                .ForEach(v => DrawSnake(v.SpawnedSnake.AllPoints, (n, p) => Draw(n, p, spriteNameToChar)));

            return new string(stringMap);

            void Draw(string name, Point point, ConcurrentDictionary<string, char> spriteNameToChar)
            {
                if (name.Contains("None")) return;
                var ch = spriteNameToChar[$"{name.ToLower()}.png"];
                stringMap[point.X + point.Y * (w + 1)] = ch;
            }
        }

        private void DrawSnake(List<GameCore.Point> snakePoints, Action<string, GameCore.Point> draw)
        {
            if (snakePoints == null || snakePoints.Count == 0) return;

            var head = snakePoints.First();
            if (snakePoints.Count == 1)
            {
                draw("HeadSnake", head);
                return;
            }

            var w = GameState.MapWidth;
            var h = GameState.MapHeight;

            var tail = snakePoints.Last();
            draw($"HeadSnake-{head.ToDirWithTorSpave(snakePoints[1], w, h)}", head);
            draw($"BodySnake-{tail.ToDirWithTorSpave(snakePoints[^2], w, h)}", tail);

            for (int i = 1; i < snakePoints.Count - 1; i++)
            {
                var prevPoint = snakePoints[i - 1];
                var bodyPoint = snakePoints[i];
                var nextPoint = snakePoints[i + 1];

                var fromDir = bodyPoint.ToDirWithTorSpave(prevPoint, w, h);
                var toDir = bodyPoint.ToDirWithTorSpave(nextPoint, w, h);

                draw($"BodySnake-{fromDir}-{toDir}", bodyPoint);
            }
        }

    }

}
