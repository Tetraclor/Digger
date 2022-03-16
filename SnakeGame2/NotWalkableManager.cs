using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame2
{
    public class NotWalkableManager : IMapAble
    {
        public HashSet<Point> Walls = new();
        public HashSet<Point> Spawns = new();

        public NotWalkableManager()
        {
        }

        public NotWalkableManager(IEnumerable<Point> walls, IEnumerable<Point> spawns)
        {
            Walls = walls.ToHashSet();
            Spawns = spawns.ToHashSet();
        }

        public bool IsWalkable(Point point)
        {
            return !Walls.Contains(point) && !Spawns.Contains(point);
        }

        public void SetToMap(Action<Point, ICreature> set)
        {
            foreach (var p in Walls)
                set(p, new Wall());
            foreach (var p in Spawns)
                set(p, new Spawn());
        }
    }
}
