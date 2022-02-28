using GameCore;
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
            var command = CommandHelper.playerCommandToCreatureComand[PlayerCommand.Move];

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

    public static class PlayerCommand
    {
        public static DiggerMove Move = DiggerMove.None;
    }

    public enum DiggerMove
    {
        None, Left, Rigth, Up, Down
    }

    public static class CommandHelper
    {
        public static Dictionary<DiggerMove, CreatureCommand> playerCommandToCreatureComand = new Dictionary<DiggerMove, CreatureCommand>()
        {
            [DiggerMove.Rigth] = new CreatureCommand { DeltaX = 1 },
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
