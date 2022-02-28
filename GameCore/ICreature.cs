using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public interface ICreature
    {
        CreatureCommand Act(GameState game, int x, int y);
        bool DeadInConflict(ICreature conflictedObject);
        int TransformPriority();
    }
}
