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
        ApplesManager ApplesManager;
        WallManager WallManager;

        IPlayer Player;
        List<ITransformAble> transforms = new();
        List<IMapAble> mapAble = new();

        public int MapWidth => GameState.MapWidth;

        public int MapHeight => GameState.MapHeight;

        public SnakeGameService(int width, int height) : base(width, height, typeof(Snake))
        {
            ApplesManager = new ApplesManager(GameState, 10);
            WallManager = new WallManager();
            Init();
        }

        public const string TestMap = @"
WWWWWW WWWWWW
W           W
W           W
W           W
W           W
W           W
      W      
W           W
W           W
W           W
W           W
W           W
WWWWWW WWWWWW
";

        public SnakeGameService(string mapString = TestMap) : base(mapString, typeof(Snake))
        {
            ApplesManager = new ApplesManager(GameState, 10);
            WallManager = new WallManager();
            GameState.Map.ForEach((p, c) => { 
                if (c is Wall)
                    WallManager.Walls.Add(p); 
            });
            Init();
        }

        private void Init()
        {
            mapAble.Add(ApplesManager);
            mapAble.Add(WallManager);
        }

        public override bool AddPlayer(IPlayer player)
        {
            Player = player;
            SnakeSpawn();
            return true;
        }

        public void SnakeSpawn()
        {
            Snake = new Snake(new Point(2, 2), new Point(1, 2));
            mapAble.Add(Snake);
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

            // ACT
            var playerCommand= (PlayerCommand)Player.GetCommand(this);
            var move = FourDirMove.None;

            if (playerCommand != null)
                move = playerCommand.Move;

            Snake.Move(move, GameState); // State Points Change

            // ------ Handle Conflict --------
            if (Snake.IsDead)
                SnakeSpawn();

            // Eat apple
            if (ApplesManager.apples.Contains(Snake.Head)) // State Points Check
            {
                ApplesManager.AppleDead(Snake.Head); // Delete Points
                Snake.AddTail(); // Create Points
            }
            // Cut own tail
            if (Snake.Body.Contains(Snake.Head)) // State Points Check
            {
                Snake.CutTail(Snake.Head); // Delete Points
            }

            // Print to map
            foreach (var item in mapAble)
            {
                item.SetToMap((point, creature) =>
                {
                    GameState.SetCreature(point, creature);
                });
            }

            // Wall Check
            if (WallManager.Walls.Contains(Snake.Head))// State Points Check
            {
                Snake.Dead();
            }
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
        void SetToMap(Action<Point, ICreature> set);
    }

    public class Snake : IMapAble
    {
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
            Head = new Point(-1, -1);
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
            PrevLastTailPosition = Body[^1];
            for (int i = 0; i < Body.Count; i++)
            {
                var temp = Body[i];
                Body[i] = next;
                next = temp;
            }

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

    public class ApplesManager : IMapAble
    {
        private Random Random = new Random();
        private GameState gameState;
        public HashSet<Point> apples = new HashSet<Point>();

        public ApplesManager(GameState gameState, int applesCount)
        {
            this.gameState = gameState;

            for (int i = 0; i < applesCount; i++)
            {
                CreateRandomApple();
            }
        }

        public ApplesManager(GameState gameState, params Point[] apples)
        {
            this.gameState = gameState;

            foreach (var p in apples)
            {
                CreateApple(p);
            }
        }

        public void AppleDead(Point apple)
        {
            if (apples.Contains(apple) == false) return;
            apples.Remove(apple);
            CreateRandomApple();
        }

        public void CreateRandomApple()
        {
            var x = Random.Next(gameState.MapWidth - 1);
            var y = Random.Next(gameState.MapWidth - 1);

            var p = new Point(x, y);

            CreateApple(p);
        }

        public void CreateApple(Point point)
        {
            var creature = gameState.GetCreatureOrNull(point);
            if (creature != null)
            {
                return;
            }
            apples.Add(point);
        }

        public void SetToMap(Action<Point, ICreature> set)
        {
            foreach (var item in apples)
            {
                set(item, new Apple());
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

    public class WallManager : IMapAble
    {
        public HashSet<Point> Walls = new();

        public void SetToMap(Action<Point, ICreature> set)
        {
            foreach (var p in Walls)
                set(p, new Wall());
        }
    }

    public abstract class Fict : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y) => null;

        public bool DeadInConflict(ICreature conflictedObject) => false;

        public int TransformPriority() => 1;
    }
}
