using GameCore;
using System;
using System.Collections.Generic;

namespace Common
{
    public class PlayerCommand : IPlayerCommand
    {
        public FourDirMove Move { get; set; } = FourDirMove.None;
    }

    public enum FourDirMove
    {
        None, Left, Right, Up, Down
    }

    public static class CreatureCommandHelper
    {
        public static CreatureCommand NoneCommand = new();

        public static bool IsInBound(this CreatureCommand command, GameState game, int x, int y)
        {
            return !(x + command.DeltaX < 0 ||
                     x + command.DeltaX >= game.MapWidth ||
                     y + command.DeltaY < 0 ||
                     y + command.DeltaY >= game.MapHeight);
        }

        public static bool IsInBound(this Point point, GameState game)
        {
            return !(point.X < 0 ||
                     point.X >= game.MapWidth ||
                     point.Y < 0 ||
                     point.Y >= game.MapHeight);
        }


        public static bool IsInBound(this Point point, IGameStateForPlayer game)
        {
            return !(point.X < 0 ||
                     point.X >= game.MapWidth ||
                     point.Y < 0 ||
                     point.Y >= game.MapHeight);
        }

        public static CreatureCommand TorSpace(this CreatureCommand command, GameState game, int x, int y)
        {
            if (command.IsInBound(game, x, y) == false)
            {
                var movePoint = command.MoveFrom(x, y);
                var cc = command.Clone();
                movePoint.X = (movePoint.X + game.MapWidth) % game.MapWidth;
                movePoint.Y = (movePoint.Y + game.MapHeight) % game.MapHeight;
                cc.DeltaX = movePoint.X - x;
                cc.DeltaY = movePoint.Y - y;

                return cc;
            }
            return command;
        }

        public static FourDirMove ToDir(this Point a, Point b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;

            if (dx == 1) return FourDirMove.Right;
            if (dx == -1) return FourDirMove.Left;
            if (dy == 1) return FourDirMove.Down;
            if (dy == -1) return FourDirMove.Up;

            return FourDirMove.None;
        }

        public static Point PointWithDir(this Point point, FourDirMove move)
        {
            switch (move)
            {
                case FourDirMove.None: 
                    return point;
                case FourDirMove.Left: 
                    return new Point(point.X - 1, point.Y);
                case FourDirMove.Right:
                    return new Point(point.X + 1, point.Y);
                case FourDirMove.Up:
                    return new Point(point.X, point.Y - 1);
                case FourDirMove.Down:
                    return new Point(point.X, point.Y + 1);
                default:
                    break;
            }
            return point;
        }

        public static IEnumerable<Point> GetNear(this Point point)
        {
            yield return point.PointWithDir(FourDirMove.Left);
            yield return point.PointWithDir(FourDirMove.Right);
            yield return point.PointWithDir(FourDirMove.Up);
            yield return point.PointWithDir(FourDirMove.Down);
        }

        public static Point TorSpace(this Point point, GameState game)
        {
            if (point.IsInBound(game) == false)
            {
                point.X = (point.X + game.MapWidth) % game.MapWidth;
                point.Y = (point.Y + game.MapHeight) % game.MapHeight;
                return point;
            }
            return point;
        }

        public static void ForEach<T>(this T[,] map, Action<Point, T> action)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    action(new Point(i, j), map[i,j]);
                }
            }
        }

        public static IEnumerable<Point> GetAllLocations<T>(this ICreature[,] map)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if(map[i, j] is T)
                        yield return new Point(i, j);
                }
            }
        }

        public static CreatureCommand ToCreatureCommand(this Point point, Point target)
        {
            return new CreatureCommand() { DeltaX = target.X - point.X, DeltaY = target.Y - point.Y };
        }

        public static Dictionary<FourDirMove, CreatureCommand> playerCommandToCreatureComand = new Dictionary<FourDirMove, CreatureCommand>()
        {
            [FourDirMove.Right] = new CreatureCommand { DeltaX = 1 },
            [FourDirMove.Left] = new CreatureCommand { DeltaX = -1 },
            [FourDirMove.Up] = new CreatureCommand { DeltaY = -1 },
            [FourDirMove.Down] = new CreatureCommand { DeltaY = 1 },
            [FourDirMove.None] = new CreatureCommand()
        };
    }
}
