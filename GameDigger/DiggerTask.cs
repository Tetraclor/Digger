using Common;
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
            return CreatureCommandHelper.NoneCommand;
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
            var playerCommand = (PlayerCommand)null; // TODO

            if (playerCommand == null)
                return CreatureCommandHelper.NoneCommand;

            var command = CreatureCommandHelper.playerCommandToCreatureComand[playerCommand.Move];

            var targetPoint = new Point(x + command.DeltaX, y + command.DeltaY);
            var creatureInTargePoint = game.GetCreatureOrNull(targetPoint);

            var validate = command.IsInBound(game, x, y) && !(creatureInTargePoint is Digger);

            return validate
                ? command 
                : CreatureCommandHelper.NoneCommand;
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
}
