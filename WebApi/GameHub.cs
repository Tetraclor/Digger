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
    public class StartGameInfo
    {
        public string GameId { get; set; }
        public string MapName { get; set; }
        public int ApplesCount { get; set; }
        public string[] Players { get; set; }

        public bool IsOver { get; private set; } = false;
        public bool IsInProgress { get; private set; } = false;
        public bool IsNotStarted => !IsOver && !IsInProgress;

        public void MarkAsOver() { IsOver = true; IsInProgress = false; }
        public void MarkAsInProgress() { IsInProgress = true;  }

    }

    public class MainHub : Hub
    {
        public static List<StartGameInfo> StartGameInfo { get; set; } = new();

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
            public int ApplesCount { get; set; }
        }

        public void StartGame(StartGameInfo startGameInfo)
        {
            StartGameInfo.Add(startGameInfo);
        }

       // public static Dictionary<string, string> ConnectionIdToGroup = new();


        //public void Join(string group)
        //{
        //    if (group == null)
        //        return;

        //    Groups.AddToGroupAsync(this.Context.ConnectionId, group);
        //    ConnectionIdToGroup[this.Context.ConnectionId] = group;
        //}

        //public void SendMessage(string message)
        //{
        //    if (ConnectionIdToGroup.TryGetValue(this.Context.ConnectionId, out var group) == false)
        //    {
        //        Clients.Caller.SendAsync("ReceiveMessage", "Неизвестная группа");
        //        return;
        //    }

        //    Clients.Group(group).SendAsync("ReceiveMessage", message);
        //}

        public List<MapInfo> GetMaps()
        {
            Maps.ForEach(v => v.Map = v.Map.Replace("\r", "").Trim());
            return Maps;
        }

        public AnimationInfo GetAnimateInfo()
        {
            return new AnimationInfo();
        }
    }
 
    public class GamesHubService 
    {
        static Dictionary<string, GameInfo> Games = new();
        List<IPlayer> Players = new();
        Dictionary<IPlayer, string> playerToGameId = new();

        public class GameInfo
        {
            public GameService GameService { get; set; }
            public System.Timers.Timer timer = new System.Timers.Timer();
            public int GameTickMs = 300;
        }

        public bool AddPlayer(string gameId, IPlayer player)
        {
            if (Games.TryGetValue(gameId, out GameInfo game) == false)
            {
                return false;
            }

            Players.Add(player);
            playerToGameId[player] = gameId;

            game.GameService.AddPlayer(player);

            return true;
        }

        public void RemovePlayer(IPlayer player)
        {
            Players.Remove(player);
            playerToGameId.Remove(player, out string gameId);

            if (Games.TryGetValue(gameId, out GameInfo game))
            {
                game.GameService.RemovePlayer(player);
            }
        }

        public GameService GetGameOrNull(string gameId)
        {
            if(Games.TryGetValue(gameId, out GameInfo game) == false)
            {
                return null;
            }
            return game.GameService;
        }

        public void CreateGame(string gameId, GameService gameService)
        {
            var gameInfo = new GameInfo();
            gameInfo.GameService = gameService;
            Games[gameId] = gameInfo;
        }

        public void StartGame(string gameId, Action<string, GameService> GameTick)
        {
            if (Games.TryGetValue(gameId, out GameInfo gameInfo) == false)
            {
                return;
            }

            var timer = gameInfo.timer;
            var gameService = gameInfo.GameService;

            timer.Elapsed += (o, e) => GameTick(gameId, gameService);
            timer.Interval = gameInfo.GameTickMs;
            timer.Enabled = true;

            timer.Start();

            GameTick(gameId, gameService);
        }

        public void StopGame(string gameId)
        {
            if (Games.TryGetValue(gameId, out GameInfo game) == false)
            {
                return;
            }
            game.timer.Stop();
            Games.Remove(gameId);
        }
    }

    public class GameHub : Hub
    {
        static Dictionary<string, RemotePlayer> RemotePlayers = new();

        IHubContext<GameHub> hubContext { get; }
        GamesHubService GamesHubService { get; }

        public GameHub(IHubContext<GameHub> hubContext, GamesHubService gamesHub)
        {
            this.GamesHubService = gamesHub;
            this.hubContext = hubContext;
        }

        public void SendTurn(string command)
        {
            if (command == null)
                return;

            if (RemotePlayers.TryGetValue(Context.UserIdentifier, out RemotePlayer remotePlayer) == false)
                return;

            remotePlayer.SetCommand(command);
        }

        public void JoinGame(string gameId)
        {
            if (RemotePlayers.ContainsKey(Context.UserIdentifier)) // Пока что один игрок может участвовать только в одно игре
                return;

            var remotePlayer = new RemotePlayer();

            if (GamesHubService.AddPlayer(gameId, remotePlayer) == false)
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

            GamesHubService.RemovePlayer(player); 

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
        }

        public void StartGame(string gameId)
        {
            var startGameInfo = MainHub.StartGameInfo.FirstOrDefault(v => v.GameId == gameId);

            Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            if (startGameInfo == null)
            {
                Clients.Caller.SendAsync("ShowMessage", $"Игры с таким id={gameId} не найдено");
                return;
            }

            if (startGameInfo.IsNotStarted)
            {
                startGameInfo.MarkAsInProgress();

                var snakeGame = new AnimateSnakeGameService
                (
                    GetThisUserSnake,
                    animationInfo,
                    GetMap()
                );

                snakeGame.ApplesManager.SetMaxApplesCount(startGameInfo.ApplesCount);

                GamesHubService.CreateGame(gameId, snakeGame);

                foreach (var playerName in startGameInfo.Players)
                {
                    var player = GetPlayer(playerName);
                    GamesHubService.AddPlayer(gameId, player);
                }

                GamesHubService.StartGame(gameId, GameTick);
            }

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

            string GetMap()
            {
                var mapInfo = MainHub.Maps
                    .FirstOrDefault(v => v.Name == startGameInfo.MapName);
                if (mapInfo == null)
                    return SnakeGame2.SnakeGameService.TestMap;
                return mapInfo.Map;
            }

            IPlayer GetPlayer(string playerName)
            {
                IPlayer player;

                if (playerName == "SimpleBot") // Боты заглушки
                    player = new SnakeBot();
                else if (playerName == "RandomBot")
                    player = new RandomBotPlayer(7);
                else
                {
                    player = new RemotePlayer();
                    RemotePlayers[playerName] = (RemotePlayer)player;
                }

                return player;
            }
        }

        public void StopGame(string gameId)
        {
            GamesHubService.StopGame(gameId);
            var startGameInfo = MainHub.StartGameInfo.FirstOrDefault(v => v.GameId == gameId);

            if (startGameInfo != null)
            {
                startGameInfo.MarkAsOver();
            }
        }

        private void GameTick(string gameId, GameService gameService)
        {
            string stringMap;
            List<PlayerInfo> playersScores;

            foreach (var (id, remotePlayer) in RemotePlayers) // Игроки
            {
                UserId = id;
                playersScores = GetPlayerInfos();
                stringMap = gameService.ToStringMap();// Перерисовка для участников  ПЕРЕДЕЛАТЬ!
                hubContext.Clients.Client(id).SendAsync("Receive", stringMap, gameService.CurrentTick, playersScores);
            }

            playersScores = GetPlayerInfos();

            // Наблюдатели
            UserId = null;
            stringMap = gameService.ToStringMap();
            hubContext.Clients.GroupExcept(gameId, RemotePlayers.Keys).SendAsync("Receive", stringMap, gameService.CurrentTick, playersScores);

            gameService.MakeGameTick();

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
        }

        static string UserId = null;

        static AnimationInfo animationInfo = new();

        public AnimationInfo GetAnimateInfo()
        {
            return animationInfo;
        }
    }

    public class AnimationInfo
    {
        public Dictionary<char, string> MapCharToSprite { get; set; } = new()
        {
            ['H'] = "/Images/HeadSnake.png",
            ['B'] = "/Images/BodySnake.png",
            ['A'] = "/Images/Apple.png",
            ['W'] = "/Images/Terrain.png",
            ['S'] = "/Images/Spawn.png",
        };
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
