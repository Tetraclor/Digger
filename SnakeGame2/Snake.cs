using Common;
using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame2
{
    public class SnakeGameService : GameService, IGameStateForPlayer
    {
        Snake Snake;
        IPlayer Player;
        List<ITransformAble> transforms = new();
        List<IMapAble> mapAble = new();

        public int MapWidth => GameState.MapWidth;

        public int MapHeight => GameState.MapHeight;

        public SnakeGameService(int width, int height) : base(width, height, typeof(Snake))
        {
        }

        public override bool AddPlayer(IPlayer player)
        {
            Player = player;
            Snake = new Snake(new Point(2, 2), new Point(1, 2));
            mapAble.Add(Snake);
            return true;
        }

        public override List<CreatureTransformation> GetCreatureTransformations()
        {
            return transforms
                .SelectMany(v => v.ToTransformation())
                .ToList();
        }

        public override string ToStringMap()
        {
            return base.ToStringMap();
        }

        public override void MakeGameTick()
        {
            ClearMap();

            var move = (PlayerCommand)Player.GetCommand(this);
            Snake.Move(move.Move, GameState);

            mapAble.ForEach(v => v.SetToMap(GameState));
        }

        private void ClearMap()
        {
            for (int i = 0; i < GameState.MapHeight; i++)
            {
                for (int j = 0; j < GameState.MapWidth; j++)
                {
                    GameState.Map[j, i] = null;
                }
            }
        }

        public ICreature GetCreatureOrNull(Point point)
        {
            return GameState.GetCreatureOrNull(point);
        }
    }

    public interface ITransformAble
    {
        IEnumerable<CreatureTransformation> ToTransformation();
    }

    public interface IMapAble
    {
        void SetToMap(GameState gameState);
    }

    public class Snake : IMapAble
    {
        public Point Head;
        public Queue<Point> Body = new();
        public Point PrevLastTailPosition;

        public Snake(Point head, params Point[] body)
        {
            Head = head;

            foreach (var item in body)
            {
                Body.Enqueue(item);
            }
        }

        public void AddTail()
        {
            Body.Enqueue(PrevLastTailPosition);
        }

        private FourDirMove PrevMove = FourDirMove.Right;

        public void Move(FourDirMove move, GameState gameState)
        {
            var target = Head
                .PointWithDir(move)
                .TorSpace(gameState);
            
            if (target == Head || target == Body.Last()) // Check move back
            {
                move = PrevMove;
                target = Head
                    .PointWithDir(move)
                    .TorSpace(gameState);
            }
            PrevLastTailPosition = Body.Dequeue();
            Body.Enqueue(Head);

            Head = target;
            PrevMove = move;
        }

        public void SetToMap(GameState gameState)
        {
            gameState.SetCreature(Head, new HeadSnake());
            foreach (var item in Body)
            {
                gameState.SetCreature(item, new BodySnake());
            }
        }

        public IEnumerable<CreatureTransformation> ToTransformation()
        {
            var prevHead = Body.Last();
            yield return new CreatureTransformation(new HeadSnake(), prevHead, Head);

            var prev = PrevLastTailPosition;
            foreach(var item in Body)
            {
                yield return new CreatureTransformation(new BodySnake(), prev, item);
                prev = item;
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

    public abstract class Fict : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y) => null;

        public bool DeadInConflict(ICreature conflictedObject) => false;

        public int TransformPriority() => 1;
    }
}
