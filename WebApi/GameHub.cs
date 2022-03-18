using Common;
using GameCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SnakeGame2;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi
{
    public class GameHub : Hub
    {
        int GameTickMs = 300;

        static System.Timers.Timer timer = new System.Timers.Timer();

        static Dictionary<string, RemotePlayer> RemotePlayers = new Dictionary<string, RemotePlayer>();

        static IPlayer randomBot = new RandomBotPlayer(23); 
        static IPlayer snakeBot = new SnakeBot();

        //static GameService gameService = new SnakeGameService(SnakeGameService.TestMap);
        static GameService gameService;
        //static GameService gameService = new DiggerGameService(DiggerGameService.mapWithPlayerTerrain);

        static AnimationInfo animationInfo = new()
        {
            MapCharToSprite = new()
            {
                ['H'] = "/Images/HeadSnake.png",
                ['B'] = "/Images/BodySnake.png",
                ['A'] = "/Images/Apple.png",
                ['W'] = "/Images/Terrain.png",
                ['S'] = "/Images/Spawn.png",
            }
        };

        IHubContext<GameHub> hubContext { get; }

        public GameHub(IHubContext<GameHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public void SendTurn(string command)
        {
            if (command == null)
                return;

            if (RemotePlayers.TryGetValue(this.Context.ConnectionId, out RemotePlayer remotePlayer) == false)
                return;

            var playerCommand = gameService.ParsePlayerCommand(command);
            remotePlayer.PlayerCommand = playerCommand;  
        }

        static bool IsFirstStart = true;

        public void StartGame()
        {
            if (IsFirstStart)
            {
                StopGame();

                timer = new System.Timers.Timer();

                timer.Elapsed += (o, e) => GameTick();
                timer.Interval = GameTickMs;
                timer.Enabled = true;

                GameTick();
            }

            IsFirstStart = false;

            if (RemotePlayers.ContainsKey(this.Context.ConnectionId))
                return;

            var remotePlayer = new RemotePlayer();
            RemotePlayers[this.Context.ConnectionId] = remotePlayer;
            gameService.AddPlayer(remotePlayer);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            if(RemotePlayers.TryGetValue(this.Context.ConnectionId, out RemotePlayer player) == false)
                return base.OnDisconnectedAsync(exception);

            RemotePlayers.Remove(this.Context.ConnectionId);
            gameService.RemovePlayer(player);

            return base.OnDisconnectedAsync(exception);
        }

        void GameTick()
        {
            gameService.MakeGameTick();

            var stringMap = "";

            foreach (var (id, remotePlayer) in RemotePlayers) // Игроки
            {
                connectionId = id;
                stringMap = gameService.ToStringMap();
                hubContext.Clients.Client(id).SendAsync("Receive", stringMap, gameService.CurrentTick);
            }

            // Наблюдатели
            connectionId = null;
            stringMap = gameService.ToStringMap();

            hubContext.Clients.AllExcept(RemotePlayers.Keys).SendAsync("Receive", stringMap, gameService.CurrentTick);
        }

        static string connectionId = null;

        public void StopGame()
        {
            timer.Stop();
            IsFirstStart = true;
            RemotePlayers.Clear();

            gameService = new AnimateSnakeGameService
            (
                GetThisUserSnake, 
                animationInfo, 
                SnakeGame2.SnakeGameService.TestMap
            );

            Snake GetThisUserSnake(SnakeGameService gameService)
            {
                if (connectionId == null)
                    return null;

                if (RemotePlayers.TryGetValue(connectionId, out RemotePlayer remotePlayer) == false)
                    return null;

                return gameService
                    .SnakeSpawners
                    .FirstOrDefault(v => v.Player == remotePlayer) 
                    .SpawnedSnake;
            }
        }

        public AnimationInfo GetAnimateInfo()
        {
            return animationInfo;
        }
    }

    public class AnimationInfo
    {
        public Dictionary<char, string> MapCharToSprite { get; set; } = new();
    }
}
