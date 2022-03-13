using Common;
using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSnake
{
    public class Spawn : ICreature
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

    public class Snake
    {
        public bool IsDead;
        public HeadSnake Head;
        public List<BodySnake> Tail = new List<BodySnake>();
        public Game Game;

        public Snake(Game game, Point head, params Point[] tail)
        {
            Game = game;
            Head = new HeadSnake(this, head);
            Head.PrevPoint = tail.FirstOrDefault();
            game.GameState.SetCreature(Head.Point, Head);
            foreach (var point in tail)
            {
                var body = new BodySnake(this, point);
                Tail.Add(body);
                game.GameState.SetCreature(point, body);
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

        public void Dead()
        {
            IsDead = true;
            Game.DeleteCreature(Head.Point);
            Tail.ForEach(v => Game.DeleteCreature(v.Point));
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
            SolveConflicted(creature);

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

        private void SolveConflicted(ICreature creature)
        {
            if (creature is Apple apple)
            {
                AddTailItem();
                apple.ApplesManager.AppleDead(apple);
            }
            else if(creature is Wall wall)
            {
                Dead();
            }
            else if (Tail.Contains(creature))
            {
                CutTail((BodySnake)creature);
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
}
