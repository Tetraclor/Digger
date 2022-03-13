using GameCore;

namespace GameSnake
{
    public class Wall : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y)
        {
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
}
