using GameCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GameDigger
{
    public class Terrain : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y)
        {
            return CommandHelper.NoneCommand;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Digger;
        }

        public int TransformPriority()
        {
            return 1;
        }
    }

    public class Digger : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y)
        {
            var playerCommand = (PlayerCommand)game.GetPlayerCommandOrNull(this);

            if (playerCommand == null)
                return CommandHelper.NoneCommand;

            var command = CommandHelper.playerCommandToCreatureComand[playerCommand.Move];

            var targetPoint = new Point(x + command.DeltaX, y + command.DeltaY);
            var creatureInTargePoint = game.GetCreatureOrNull(targetPoint);

            var validate = command.IsInBound(game, x, y) && !(creatureInTargePoint is Digger);

            return validate
                ? command 
                : CommandHelper.NoneCommand;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int TransformPriority()
        {
            return 0;
        }
    }

    public abstract class Player : IPlayer
    {
        public abstract IPlayerCommand GetCommand(IReadOnlyGameState gameState);
    }

    public class PlayerCommand : IPlayerCommand 
    {
        public DiggerMove Move { get; set; } = DiggerMove.None;
    }

    public enum DiggerMove
    {
        None, Left, Right, Up, Down
    }

    public class BotPlayer : Player
    {
        static Random _R = new Random();
        static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(_R.Next(v.Length));
        }

        public override IPlayerCommand GetCommand(IReadOnlyGameState gameState)
        {
            return new PlayerCommand() { Move = RandomEnumValue<DiggerMove>() };
        }
    }

    public static class CommandHelper
    {
        public static Dictionary<DiggerMove, CreatureCommand> playerCommandToCreatureComand = new Dictionary<DiggerMove, CreatureCommand>()
        {
            [DiggerMove.Right] = new CreatureCommand { DeltaX = 1 },
            [DiggerMove.Left] = new CreatureCommand { DeltaX = -1 },
            [DiggerMove.Up] = new CreatureCommand { DeltaY = -1},
            [DiggerMove.Down] = new CreatureCommand { DeltaY = 1},
            [DiggerMove.None] = new CreatureCommand ()
        };

        public static CreatureCommand NoneCommand = new();

        public static bool IsInBound(this CreatureCommand command, GameState game, int x, int y)
        {
            return !(x + command.DeltaX < 0 ||
                     x + command.DeltaX >= game.MapWidth ||
                     y + command.DeltaY < 0 ||
                     y + command.DeltaY >= game.MapHeight);
        }
    }
}
