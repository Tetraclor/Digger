using GameCore;
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

        public static Point ToDir(this Point point, FourDirMove move)
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
            yield return point.ToDir(FourDirMove.Left);
            yield return point.ToDir(FourDirMove.Right);
            yield return point.ToDir(FourDirMove.Up);
            yield return point.ToDir(FourDirMove.Down);
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

        public static CreatureCommand FromPlayerCommand(GameState game, ICreature creature, int x, int y)
        {
            var playerCommand = (PlayerCommand)game.GetPlayerCommandOrNull(creature);
            if (playerCommand == null) 
                return NoneCommand;

            var creatureCommand = playerCommandToCreatureComand[playerCommand.Move];

            return creatureCommand.IsInBound(game, x, y) ? creatureCommand : NoneCommand;
        }

        public static CreatureCommand FromPlayerCommandNoCheckBound(GameState game, ICreature creature)
        {
            var playerCommand = (PlayerCommand)game.GetPlayerCommandOrNull(creature);
            if (playerCommand == null)
                return NoneCommand;

            var creatureCommand = playerCommandToCreatureComand[playerCommand.Move];

            return creatureCommand;
        }
    }
}
