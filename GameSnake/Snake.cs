using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSnake
{
    public class Snake : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y)
        {
            var playerCommand = game.GetPlayerCommandOrNull(this);
            
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int TransformPriority()
        {
            return 1;
        }
    }

    public class Apple : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Snake;
        }

        public int TransformPriority()
        {
            return 0;
        }
    }
}
