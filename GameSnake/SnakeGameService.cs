using Common;
using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSnake
{
    public class SnakeBot : IPlayer
    {
        public IPlayerCommand GetCommand(IGameStateForPlayer gameState)
        {
            var state = (SnakeGameStateForPlayer)gameState;
            var move = FourDirMove.None;

            var headPoint = state.MySnakeHead;
            var targetPoint = headPoint;

            var nearApples = headPoint
               .GetNear()
               .Where(v => v.IsInBound(gameState))
               .Where(v => gameState.GetCreatureOrNull(v) is Apple);

            if(nearApples.Any())
            {
                targetPoint = nearApples.FirstOrDefault();
                return ReturnCommand();
            }

            var nearFree = headPoint
                .GetNear()
                .Where(v => v.IsInBound(gameState))
                .Where(v => gameState.GetCreatureOrNull(v) == null);

            var random = new Random();

            targetPoint = nearFree.PickRandom();

            return ReturnCommand();


            PlayerCommand ReturnCommand()
            {
                move = headPoint.ToDir(targetPoint);
                return new PlayerCommand() { Move = move };
            }
        }
    }

    public class SnakeGameStateForPlayer : IGameStateForPlayer
    {
        private GameState gameState;
        private Snake snakePlayer;

        public SnakeGameStateForPlayer(GameState gameState, Snake snakePlayer)
        {
            this.gameState = gameState;
            this.snakePlayer = snakePlayer;
        }

        public int MapWidth => gameState.MapWidth;

        public int MapHeight => gameState.MapHeight;

        public ICreature GetCreatureOrNull(Point point) => gameState.GetCreatureOrNull(point);

        public Point MySnakeHead => snakePlayer.Head.Point;
        public List<Point> MySnakeBody => snakePlayer.Tail.Select(v => v.Point).ToList();
    }

    public class SnakeGameService : GameService
    {
        class PlayerInfo
        {
            public bool IsActive => player != null;
            public Snake snake;
            public Spawn spawn;
            public IPlayer player;

            public bool IsSpawnInProgress = false;
            public int SpawnTailElement = 2;
            private int CurrentSpawnTailElement = 0;

            public void ProgressSpawn()
            {
                CurrentSpawnTailElement++;
                snake.AddTailItem();
                if(CurrentSpawnTailElement == SpawnTailElement)
                {
                    IsSpawnInProgress = false;
                    CurrentSpawnTailElement = 0;
                }   
            }

            public bool SpawnSnake(Game game)
            {
                IsSpawnInProgress = true;

                if (spawn.HasFreePointNear == false)
                    return false;

                var headPoint = spawn.NearFreePoint;

                snake = new Snake(game, headPoint);
                return true;
            }

            public FourDirMove GetPlayerMove(GameState gameState)
            {
                var playerCommand = player.GetCommand(new SnakeGameStateForPlayer(gameState, snake));
                var playerMove = FourDirMove.None;

                if (playerCommand != null)
                    playerMove = (playerCommand as PlayerCommand).Move;

                return playerMove;
            }
        }

        List<PlayerInfo> playerInfos = new();

        public SnakeGameService(int width, int height) : base(width, height, typeof(HeadSnake))
        {
            var appleManager = new ApplesManager(Game, 10);

            base.MakeGameTick();
        }

        public SnakeGameService(string mapString) 
        {
            Init(mapString, typeof(HeadSnake));

            var appleManager = new ApplesManager(Game, 10);
            playerInfos = GameState.GetCreatures<Spawn>()
                .Select(v => new PlayerInfo() { spawn = v })
                .ToList();

            base.MakeGameTick();
        }

        public override bool AddPlayer(IPlayer player)
        {
            var newPlayerInfo = playerInfos.FirstOrDefault(v => v.IsActive == false);

            if (newPlayerInfo == null) return false;

            newPlayerInfo.SpawnSnake(Game); 
            newPlayerInfo.player = player;

            return true;
        }

        public override IPlayerCommand ParsePlayerCommand(string command)
        {
            var move = Enum.Parse<FourDirMove>(command, true);
            var playerCommand = new PlayerCommand() { Move = move };

            return playerCommand;
        }

        public override void MakeGameTick()
        {
            foreach (var pi in playerInfos.Where(v => v.IsActive))
            {
                var snake = pi.snake;
                var playerMove = pi.GetPlayerMove(GameState);
                snake.Move(playerMove, GameState);
            }

            base.MakeGameTick();

            foreach(var pi in playerInfos.Where(v => v.IsActive))
            {
                if (pi.IsSpawnInProgress)
                {
                    pi.ProgressSpawn();
                }
                if (pi.snake.IsDead)
                {
                    pi.SpawnSnake(Game);
                }
            }
        }



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
    }
}
