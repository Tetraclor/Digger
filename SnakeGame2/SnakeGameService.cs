using Common;
using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame2
{
    public class SnakeGameService : GameService, IGameStateForPlayer
    {
        public ApplesManager ApplesManager;
        public NotWalkableManager WallManager;

        public List<SnakeSpawner> SnakeSpawners = new();

        List<ITransformAble> transforms = new();
        List<IMapAble> mapAble = new();

        public int MapWidth => GameState.MapWidth;
        public int MapHeight => GameState.MapHeight;

        public int EatAppleScore = 10;
        public int DeathScore = -10;

        public SnakeGameService(int width, int height) : base(width, height, typeof(Snake))
        {
            ApplesManager = new ApplesManager(GameState, 10);
            WallManager = new NotWalkableManager();
            Init();
        }

        public const string TestMapNoTorSpace = @"
WWWWWWWWWWWWWWWWWWWWWW
W         WW         W
S         WW         S
W         WW         W
W                    W
W    W          W    W
W                    W
W         WW         W
W         WW         W
W         WW         W
WWW     WWWWWW     WWW
W         WW         W
W         WW         W
W         WW         W
W                    W
W    W          W    W
W                    W
W         WW         W
S         WW         S
W         WW         W
WWWWWWWWWWWWWWWWWWWWWW
";
        public const string TestMap = @"
WWWWW WWWWWWWWWW WWWWW
W         WW         W
S         WW         S
W         WW         W
W                    W
     W          W     
W                    W
W         WW         W
W         WW         W
W         WW         W
WWW     WWWWWW     WWW
W         WW         W
W         WW         W
W         WW         W
W                    W
     W          W     
W                    W
W         WW         W
S         WW         S
W         WW         W
WWWWW WWWWWWWWWW WWWWW
";

        public SnakeGameService(string mapString = TestMap) : base(mapString, typeof(Snake))
        {
            ApplesManager = new ApplesManager(GameState, 10);

            var spawnPoints = GameState.Map.GetAllLocations<Spawn>().ToList();

            WallManager = new NotWalkableManager(GameState.Map.GetAllLocations<Wall>(), spawnPoints);

            SnakeSpawners = spawnPoints.Select(v => new SnakeSpawner(v)).ToList();
            Init();
        }

        private void Init()
        {
            mapAble.Add(ApplesManager);
            mapAble.Add(WallManager);
        }

        public override bool AddPlayer(IPlayer player)
        {
            base.AddPlayer(player);

            var freeSpawner = SnakeSpawners.FirstOrDefault(v => v.IsActive == false);

            if (freeSpawner == null) return false;
            freeSpawner.Player = player;
            SnakeSpawn(freeSpawner);

            return true;
        }

        private void SnakeSpawn(SnakeSpawner snakeSpawner)
        {
            if (snakeSpawner.Spawn(this, IsFreePoint) == false) 
                return;
            
            if(mapAble.Contains(snakeSpawner.SpawnedSnake) == false)
                mapAble.Add(snakeSpawner.SpawnedSnake);

            bool IsFreePoint(Point point)
            {
                return WallManager.IsWalkable(point);
            }
        }

        private FourDirMove GetSnakeMove(IPlayer player, Snake snake)
        {
            var playerCommand = (PlayerCommand)player.GetCommand( new SnakeGameStateForPlayer(this.GameState, snake));
            var move = FourDirMove.None;

            if (playerCommand != null)
                move = playerCommand.Move;

            return move;
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
            CurrentTick++;
            ClearMap();
            PrintToMap();

            foreach (var snakeSpawner in SnakeSpawners.Where(v => v.IsActive))
            {
                var snake = snakeSpawner.SpawnedSnake;

                // ACT
                if (snake.IsDead == false)
                {
                    var move = GetSnakeMove(snakeSpawner.Player, snake);
                    snake.Move(move, GameState);// State Points Change
                }

                // ------ Handle Conflict --------
                if (snake.IsDead || snakeSpawner.InProgress)
                    SnakeSpawn(snakeSpawner);

                // Eat apple
                if (ApplesManager.apples.Contains(snake.Head)) // State Points Check
                {
                    PlayersScores[snakeSpawner.Player] += EatAppleScore; // Score count
                    ApplesManager.AppleDead(snake.Head); // Delete Points
                    snake.AddTail(); // Create Points
                }
                // Cut own tail
                if (snake.Body.Contains(snake.Head)) // State Points Check
                {
                    snake.CutTail(snake.Head); // Delete Points
                }
            }

            if(ApplesManager.MaxApplesCount > ApplesManager.apples.Count)
            {
                ApplesManager.CreateRandomApple();
            }

            ClearMap();
            PrintToMap();

            foreach (var snakeSpawner in SnakeSpawners.Where(v => v.IsActive))
            {
                var snake = snakeSpawner.SpawnedSnake;
                // Wall Check
                if (WallManager.IsWalkable(snake.Head) == false)// State Points Check
                {
                    snake.Dead();
                }

                foreach (var otherSnakeSpawner in SnakeSpawners.Where(v => v.IsActive && v != snakeSpawner))
                {
                    var otherSnake = otherSnakeSpawner.SpawnedSnake;
                    if (snake.Head == otherSnake.Head)
                    {
                        var thisCount = snake.Body.Count;
                        var otherCount = snake.Body.Count;

                        var delta = Math.Abs(thisCount - otherCount);

                        if (delta < 2)
                        {
                            snake.Dead();
                            otherSnake.Dead();
                        }
                        else if (thisCount > otherCount)
                        {
                            snake.CutTail(snake.Body[delta]);
                            otherSnake.Dead();
                        }
                        else if (thisCount < otherCount)
                        {
                            snake.Dead();
                            otherSnake.CutTail(otherSnake.Body[delta]);
                        }
                    }
                    else if (otherSnake.Body.Contains(snake.Head))
                    {
                        snake.Dead();
                    }
                }
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

        private void PrintToMap()
        {
            foreach (var item in mapAble)
            {
                item.SetToMap((point, creature) =>
                {
                    GameState.SetCreature(point, creature);
                });
            }
        }

        public ICreature GetCreatureOrNull(Point point)
        {
            return GameState.GetCreatureOrNull(point);
        }

        public override bool RemovePlayer(IPlayer player)
        {
            var spawner = SnakeSpawners.FirstOrDefault(v => v.Player == player);
            if (spawner == null) return false;

            spawner.Deactivate();
            return true;
        }
    }
}
