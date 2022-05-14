using Common;
using GameCore;
using System;
using System.Linq;

namespace SnakeGame2
{
    public class SnakeSpawner
    {
        public bool IsActive => Player != null;
        public IPlayer Player;

        public Point Point;
        public Snake SpawnedSnake;
        public bool InProgress;
        public int InitSnakeBodyLength = 2;
        public int CurrentSnakeBodyLength => SpawnedSnake.Body.Count;

        public SnakeSpawner(Point point)
        {
            Point = point;
        }

        public void Deactivate()
        {
            Player = null;
            SpawnedSnake.Dead();
            SpawnedSnake = null;
        }

        public bool Spawn(SnakeGameService gameState, Func<Point, bool> IsFreePoint)
        {
            if (SpawnedSnake != null && SpawnedSnake.IsDead) // if very fast dead 
            {
                InProgress = false;
            }

            if (InProgress == false)
            {
                var freePoints = Point.GetNear()
                    .Where(v => v.IsInBound(gameState))
                    .Where(v => IsFreePoint(v))
                    .ToList();

                if (freePoints.Count == 0)
                    return false;


                var freePoint = freePoints.First();
                SpawnedSnake = new Snake(freePoint);
                SpawnedSnake.PrevMove = Point.ToDir(freePoint);
                InProgress = true;

                return true;
            }

            if(CurrentSnakeBodyLength < InitSnakeBodyLength)
            {
                SpawnedSnake.AddTail();
            }
            else
            {
                InProgress = false;
            }

            return true;
        }
    }
}
