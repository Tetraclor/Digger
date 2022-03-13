using Common;
using GameCore;
using System.Linq;

namespace GameSnake
{
    public class Spawn : ICreature
    {
        public Point Point;
        public bool HasFreePointNear;
        public Point NearFreePoint;

        public CreatureCommand Act(GameState game, int x, int y)
        {
            Point = new Point(x, y);

            NearFreePoint = Point
                .GetNear()
                .Where(v => v.IsInBound(game))
                .Where(v => game.GetCreatureOrNull(v) == null)
                .FirstOrDefault();

            HasFreePointNear = NearFreePoint != default;

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
