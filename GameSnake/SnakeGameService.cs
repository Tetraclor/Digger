using Common;
using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSnake
{
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
                var playerCommand = player.GetCommand(gameState);
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
