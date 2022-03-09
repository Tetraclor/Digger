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
            Head.PrevPoint = tail.FirstOrDefault();
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
            Game.CreateCreature(newTailItem.Point, newTailItem);
        }

        public void CutTail(BodySnake fromThis)
        {
            var index = Tail.IndexOf(fromThis);
            if (index == -1) return;

            for (int i = index; i < Tail.Count; i++)
            {
                Game.DeleteCreature(Tail[i]);
                Tail[i].IsDead = true;
            }
            Tail.RemoveRange(index, Tail.Count - index);
        }

        private void Move(Point target)
        {
            var creature = this.Game.GameState.GetCreatureOrNull(target);

            if (creature is Apple apple)
            {
                AddTailItem();
                apple.ApplesManager.AppleDead(apple);
            }
            else if (Tail.Contains(creature))
            {
                CutTail((BodySnake)creature);
            }

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

        public bool DeadInConflict(ICreature conflictedObject) => false;

        public int TransformPriority() => 1;
    }

    public class BodySnake : ICreature
    {
        public bool IsDead = false;
        public Point PrevPoint;
        public Point Point;
        public Snake Snake;

        public BodySnake(Snake snake, Point point)
        {
            Snake = snake;
            Point = point;
        }

        public CreatureCommand Act(GameState game, int x, int y) => 
            IsDead ? 
            new CreatureCommand()  :
            PrevPoint.ToCreatureCommand(Point);

        public bool DeadInConflict(ICreature conflictedObject) => IsDead;

        public int TransformPriority() => 1;
    }

    public class ApplesManager
    {
        private Random Random = new Random();
        private Game game;
        public HashSet<Apple> apples = new HashSet<Apple>();

        public ApplesManager(Game game, int applesCount)
        {
            this.game = game;

            for (int i = 0; i < applesCount; i++)
            {
                CreateRandomApple();
            }
        }

        public ApplesManager(Game game, params Point[] apples)
        {
            this.game = game;

            foreach(var p in apples)
            {
                CreateApple(p);
            }
        }

        public void AppleDead(Apple apple)
        {
            if (apples.Contains(apple) == false) return;
            apples.Remove(apple);
            CreateRandomApple();
        }

        public void CreateRandomApple()
        {
            var x = Random.Next(game.GameState.MapWidth);
            var y = Random.Next(game.GameState.MapWidth);

            var p = new Point(x, y);

            CreateApple(p);
        }

        public void CreateApple(Point point)
        {
            var creature = game.GameState.GetCreatureOrNull(point);
            if (creature != null)
            {
                return;
            }
            var newApple = new Apple(this);
            apples.Add(newApple);
            game.GameState.SetCreature(point, newApple);
        }
    }

    public class Apple : ICreature
    {
        public ApplesManager ApplesManager;

        public Apple(ApplesManager manager)
        {
            this.ApplesManager = manager;
        }

        public CreatureCommand Act(GameState game, int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject) 
        {
            return true;
        }

        public int TransformPriority()
        {
            return 2;
        }
    }
}
