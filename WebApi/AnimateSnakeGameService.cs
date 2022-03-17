using Common;
using GameCore;
using SnakeGame2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebApi
{
    public class AnimateSnakeGameService : SnakeGame2.SnakeGameService
    {
        Dictionary<string, char> spriteNameToChar = new();

        public AnimateSnakeGameService(Func<SnakeGameService, Snake> getUserSnake, AnimationInfo initAnimateInfo, string mapString) : base(mapString)
        {
            spriteNameToChar = new Dictionary<string, char>();

            var counter = 0;

            foreach (var e in new DirectoryInfo("wwwroot/Images/Snake").GetFiles("*.png"))
            {
                var imagename = e.Name.ToLower();
                var ch = (char)('a' + counter++);
                spriteNameToChar[imagename] = ch;
                initAnimateInfo.MapCharToSprite[ch] = $"/Images/Snake/{imagename}";
            }
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
                stringBuilder.Append('\n');
            }

            var stringMap = stringBuilder.ToString().ToCharArray();

            var userSnake = GetUserSnake(this);

            if (userSnake == null) return new string(stringMap);

            DrawSnake(userSnake.AllPoints, Draw);

            SnakeSpawners
                .Where(v => v.IsActive && v.SpawnedSnake != userSnake)
                .ToList()
                .ForEach(v => DrawSnake(v.SpawnedSnake.AllPoints, Draw));

            return new string(stringMap);

            void Draw(string name, Point point)
            {
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

            var tail = snakePoints.Last();
            draw($"HeadSnake-{head.ToDir(snakePoints[1])}", head);
            draw($"BodySnake-{tail.ToDir(snakePoints[^2])}", tail);

            for (int i = 1; i < snakePoints.Count - 1; i++)
            {
                var prevPoint = snakePoints[i - 1];
                var bodyPoint = snakePoints[i];
                var nextPoint = snakePoints[i + 1];

                var fromDir = bodyPoint.ToDir(prevPoint);
                var toDir = bodyPoint.ToDir(nextPoint);

                draw($"BodySnake-{fromDir}-{toDir}", bodyPoint);
            }
        }

    }

}
