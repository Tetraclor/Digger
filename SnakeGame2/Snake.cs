using Common;
using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame2
{ 
    public class Snake : IMapAble
    {
        public List<Point> AllPoints => new List<Point> { Head }.Concat(Body).ToList();

        public Point Head;
        public List<Point> Body = new();
        public Point PrevLastTailPosition;
        public Point PrevHead => Body.FirstOrDefault();
        public bool IsDead { get; private set; }

        public Snake(Point head, params Point[] body)
        {
            Head = head;

            foreach (var item in body)
            {
                Body.Add(item);
            }
        }

        public void AddTail()
        {
            Body.Add(PrevLastTailPosition);
        }

        public void CutTail(Point fromThis)
        {
            var index = Body.IndexOf(fromThis);
            if (index == -1) return;

            Body.RemoveRange(index, Body.Count - index);
        }

        public void Dead()
        {
            IsDead = true;
            Body.Clear();
        }

        private FourDirMove PrevMove = FourDirMove.Right;

        public void Move(FourDirMove move, GameState gameState)
        {
            if (IsDead) return;

            var target = Head
                .PointWithDir(move)
                .TorSpace(gameState);

            if (target == Head || target == PrevHead) // Check move back
            {
                move = PrevMove;
                target = Head
                    .PointWithDir(move)
                    .TorSpace(gameState);
            }

            var next = Head;
       
            for (int i = 0; i < Body.Count; i++)
            {
                var temp = Body[i];
                Body[i] = next;
                next = temp;
            }

            PrevLastTailPosition = next;

            Head = target;
            PrevMove = move;
        }

        public IEnumerable<CreatureTransformation> ToTransformation()
        {
            var prevHead = Body.Last();
            yield return new CreatureTransformation(new HeadSnake(), prevHead, Head);

            var prev = PrevLastTailPosition;
            foreach (var item in Body)
            {
                yield return new CreatureTransformation(new BodySnake(), prev, item);
                prev = item;
            }
        }

        public void SetToMap(Action<Point, ICreature> set)
        {
            if (IsDead) return;

            set(Head, new HeadSnake());
            foreach (var item in Body)
            {
                set(item, new BodySnake());
            }
        }
    }

    public class HeadSnake : Fict
    {
    }

    public class BodySnake : Fict
    {
    }

    public class Apple : Fict
    {
    }

    public class Wall : Fict
    {
    }

    public class Spawn : Fict
    {
    }

    public abstract class Fict : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y) => null;

        public bool DeadInConflict(ICreature conflictedObject) => false;

        public int TransformPriority() => 1;
    }
}
