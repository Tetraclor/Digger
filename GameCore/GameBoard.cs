using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public class GameBoard
    {
        public readonly ICreature[,] Map;

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }

        public GameBoard()
        {
        }

        public GameBoard(ICreature[,] map)
        {
            Map = map;
            MapWidth = Map.GetLength(0);
            MapHeight = Map.GetLength(1);
        }

        public void SetCreature(Point point, ICreature creature)
        {
            Map[point.X, point.Y] = creature;
        }

        public ICreature GetCreatureOrNull(Point point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= MapWidth || point.Y >= MapHeight)
                return null;
            return Map[point.X, point.Y];
        }

        public IEnumerable<T> GetCreatures<T>() where T : ICreature
        {
            for (int  i = 0;  i < MapHeight;  i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    if (Map[j, i] is T t)
                        yield return t;
                }
            }
        }
    }
}
