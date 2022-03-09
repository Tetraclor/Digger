using Common;
using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSnake
{
    public class Snake
    {
        public HeadSnake Head;
        public List<BodySnake> Tail = new List<BodySnake>();
        public Game Game;

        public Snake(Point head, params Point[] tail)
        {
            Head = new HeadSnake(this, head);
            foreach (var point in tail)
            {
                Tail.Add(new BodySnake(this, point));
            }
        }

        public void AddToGame(Game game)
        {
            this.Game = game;
            game.GameState.SetCreature(Head.Point, Head);
            foreach (var item in Tail)
            {
                game.GameState.SetCreature(item.Point, item);
            }
        }

        FourDirMove prevDir = FourDirMove.Right;

        public void Move(FourDirMove dir, GameState gameState)
        {
            var target = Head.Point.ToDir(dir).TorSpace(gameState);

            if (target == Head.Point || target == Head.PrevPoint)
                dir = prevDir;
            
            prevDir = dir;
            Move(Head.Point.ToDir(dir).TorSpace(gameState));
        }

        public void AddTailItem()
        {
            var last = Tail.Last();
            var newTailItem = new BodySnake(this, last.PrevPoint);
            Tail.Add(newTailItem);
            Game.GameState.SetCreature(newTailItem.Point, newTailItem);
        }

        private void Move(Point target)
        {
            Head.PrevPoint = Head.Point;
            Head.Point = target;

            var temp = Head.PrevPoint;

            foreach (var item in Tail)
            {
                item.PrevPoint = item.Point;
                item.Point = temp;
                temp = item.PrevPoint;
            }
        }

        public void Tick(GameState game)
        {
        }

        public bool HeadConflict(ICreature creature)
        {
            if(creature is Apple)
            {
                AddTailItem();
            }
            return false;
        }

        public bool BodyConflict(ICreature creature)
        {
            return false;
        }
    }


    public class HeadSnake : ICreature
    {
        public Point PrevPoint;
        public Point Point;
        public Snake Snake;

        public HeadSnake(Snake snake, Point point)
        {
            Snake = snake;
            Point = point;
        }

        public CreatureCommand Act(GameState game, int x, int y) => PrevPoint.ToCreatureCommand(Point);

        public bool DeadInConflict(ICreature conflictedObject) => Snake.HeadConflict(conflictedObject);

        public int TransformPriority() => 1;
    }

    public class BodySnake : ICreature
    {
        public Point PrevPoint;
        public Point Point;
        public Snake Snake;

        public BodySnake(Snake snake, Point point)
        {
            Snake = snake;
            Point = point;
        }

        public CreatureCommand Act(GameState game, int x, int y) => PrevPoint.ToCreatureCommand(Point);

        public bool DeadInConflict(ICreature conflictedObject) => Snake.BodyConflict(conflictedObject);

        public int TransformPriority() => 1;
    }

    public class Apple : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is HeadSnake;
        }

        public int TransformPriority()
        {
            return 0;
        }
    }
}
