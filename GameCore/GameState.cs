using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public class GameState
    {
        public ICreature[,] Map;
        public int Scores;
        public bool IsOver;

        public int MapWidth => Map.GetLength(0);
        public int MapHeight => Map.GetLength(1);

        public ICreature GetCreatureOrNull(Point point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= MapWidth || point.Y >= MapHeight)
                return null;
            return Map[point.X, point.Y];
        }
    }
}
