using Common;
using GameCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SnakeGame2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi
{
    public class MainHub : Hub
    {
        public static MapInfo ChoisedMap;

        public static List<MapInfo> Maps = new()
        {
            new MapInfo() { Name = "Close Map", Map = SnakeGame2.SnakeGameService.TestMapNoTorSpace },
            new MapInfo() { Name = "Open Map", Map = SnakeGame2.SnakeGameService.TestMap },
            new MapInfo() { Name = "T + D :)", Map = SnakeGame2.SnakeGameService.Hah },
        };

        public class MapInfo
        {
            public string Name { get; set; }
            public string Map { get; set; }
        }

        public void SetMap(string name)
        {
            var map = Maps.FirstOrDefault(v => v.Name == name);
            if (map == null) return;
            ChoisedMap = map;
        }

        public List<MapInfo> GetMaps()
        {
            Maps.ForEach(v => v.Map = v.Map.Replace("\r", "").Trim());
            return Maps;
        }


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

        public AnimationInfo GetAnimateInfo()
        {
            return animationInfo;
        }
    }

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

        IHubContext<GameHub> hubContext { get; }

        public GameHub(IHubContext<GameHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public void SendTurn(string command)
        {
            if (command == null)
                return;

            if (RemotePlayers.TryGetValue(Context.UserIdentifier, out RemotePlayer remotePlayer) == false)
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

                gameService.AddPlayer(snakeBot);

                timer = new System.Timers.Timer();

                timer.Elapsed += (o, e) => GameTick();
                timer.Interval = GameTickMs;
                timer.Enabled = true;

                GameTick();
            }

            IsFirstStart = false;

            if (RemotePlayers.ContainsKey(Context.UserIdentifier))
                return;

            var remotePlayer = new RemotePlayer();

            if (gameService.AddPlayer(remotePlayer) == false)
                return;

            RemotePlayers[Context.UserIdentifier] = remotePlayer;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            if(RemotePlayers.TryGetValue(Context.UserIdentifier, out RemotePlayer player) == false)
                return base.OnDisconnectedAsync(exception);

            RemotePlayers.Remove(Context.UserIdentifier);
            gameService.RemovePlayer(player);

            return base.OnDisconnectedAsync(exception);
        }

        class PlayerInfo
        {
            public string Name { get; set; }
            public int Score { get; set; }

            public PlayerInfo()
            {
            }

            public PlayerInfo(string name, int score)
            {
                Name = name;
                Score = score;
            }

            public static explicit operator PlayerInfo((string, int) b) => new (b.Item1, b.Item2);
        }

        void GameTick()
        {
            gameService.MakeGameTick();

            var stringMap = "";
            var playersScores = new List<PlayerInfo>();

            foreach (var (id, remotePlayer) in RemotePlayers) // Игроки
            {
                UserId = id;
                playersScores = GetPlayerInfos();
                stringMap = gameService.ToStringMap();
                hubContext.Clients.Client(id).SendAsync("Receive", stringMap, gameService.CurrentTick, playersScores);
            }


            playersScores = GetPlayerInfos();

            // Наблюдатели
            UserId = null;
            stringMap = gameService.ToStringMap();

            hubContext.Clients.AllExcept(RemotePlayers.Keys).SendAsync("Receive", stringMap, gameService.CurrentTick, playersScores);
        }

        List<PlayerInfo> GetPlayerInfos()
        {
            var playersScores = gameService.PlayersScores
                .Select(v => new PlayerInfo(GetUserNameFromPlayer(v.Key), v.Value))
                .ToList();

            return playersScores;

            string GetUserNameFromPlayer(IPlayer player)
            {
                var name = RemotePlayers.FirstOrDefault(v => v.Value == player).Key;
                if (name == null || name == "")
                {
                    return $"It's SnakeBot!";
                }
                if (UserId == name)
                {
                    return $"It's You! {name}";
                }
                return name;
            };
        }

        static string UserId = null;

        public void StopGame()
        {
            timer.Stop();
            IsFirstStart = true;
            RemotePlayers.Clear();

            gameService = new AnimateSnakeGameService
            (
                GetThisUserSnake, 
                animationInfo,
                GetMap()
            );

            Snake GetThisUserSnake(SnakeGameService gameService)
            {
                if (UserId == null)
                    return null;

                if (RemotePlayers.TryGetValue(UserId, out RemotePlayer remotePlayer) == false)
                    return null;

                return gameService
                    .SnakeSpawners
                    .FirstOrDefault(v => v.Player == remotePlayer) 
                    .SpawnedSnake;
            }
        }

        private string GetMap()
        {
            var mapInfo = MainHub.ChoisedMap;
            if (mapInfo == null)
                return SnakeGame2.SnakeGameService.TestMap;
            return mapInfo.Map;
        }

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

        public AnimationInfo GetAnimateInfo()
        {
            return animationInfo;
        }
    }

    public class AnimationInfo
    {
        public Dictionary<char, string> MapCharToSprite { get; set; } = new();
    }

    public class CustomUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.ConnectionId; // Для тестирования
            // или так
            //return connection.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
