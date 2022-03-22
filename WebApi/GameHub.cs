using Common;
using GameCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SnakeGame2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi
{
    public class StartGameInfo
    {
        public string GameId { get; set; }
        public string MapName { get; set; }
        public int ApplesCount { get; set; }
        public int TicksToEnd { get; set; }
        public string[] Players { get; set; }

        public bool IsOver { get; private set; } = false;
        public bool IsInProgress { get; private set; } = false;
        public bool IsNotStarted => !IsOver && !IsInProgress;

        public void MarkAsOver() { IsOver = true; IsInProgress = false; }
        public void MarkAsInProgress() { IsInProgress = true;  }
    }

    public class PlayerInfo
    {
        public string Name { get; set; }
        public int Rate { get; set; } = 1500;
        public Func<IPlayer> CreateGamePlayer = () => new RemotePlayer();
        public List<string> IdGamesIsMember = new(); // Ид игр где является участником
        public List<IPlayer> GamesPlayers = new(); // 

        public IPlayer JoinGame(string gameId)
        {
            var player = CreateGamePlayer();
            IdGamesIsMember.Add(gameId);
            GamesPlayers.Add(player);
            return player;
        }

        public IPlayer GetPlayerFromGame(string gameId)
        {
            var index = IdGamesIsMember.IndexOf(gameId);
            if (index == -1) return null;
            return GamesPlayers[index];
        }

        public void ExcludeFromGame(string gameId)
        {
            var index = IdGamesIsMember.IndexOf(gameId);
            if (index == -1) return;
            IdGamesIsMember.RemoveAt(index);
            GamesPlayers.RemoveAt(index);
        }

        public void ExcludeFromGame(IPlayer player)
        {

        }
    }

    public class MainHub : Hub
    {
        public static List<MapInfo> Maps = new()
        {
            new MapInfo() { Name = "Close Map", Map = SnakeGame2.SnakeGameService.TestMapNoTorSpace },
            new MapInfo() { Name = "Open Map", Map = SnakeGame2.SnakeGameService.TestMap },
           // new MapInfo() { Name = "T + D :)", Map = SnakeGame2.SnakeGameService.Hah },
            new MapInfo() { Name = "Free Map", Map = SnakeGame2.SnakeGameService.FreeMap},
            new MapInfo() { Name = "Small Map", Map = SnakeGame2.SnakeGameService.SmallMap},
        };

        public GamesHubService GamesHub { get; }

        public class MapInfo
        {
            public string Name { get; set; }
            public string Map { get; set; }
            public int ApplesCount { get; set; }
        }

        public MainHub(GamesHubService gamesHub)
        {
            GamesHub = gamesHub;
        }

        public void StartGame(StartGameInfo startGameInfo)
        {
            GamesHubService.GamesInfo.Add(startGameInfo);
        }

        public override Task OnConnectedAsync()
        {
            GamesHub.TryAddPlayer(Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
          //  GamesHub.RemovePlayer(Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }

        public List<PlayerInfo> GetPlayers()
        {
            return GamesHubService.Players;
        }

        public List<StartGameInfo> GetGames()
        {
            return GamesHubService.GamesInfo;
        }

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
        public static List<StartGameInfo> GamesInfo { get; set; } = new();
        public static List<PlayerInfo> Players { get; set; } = new() {  // Локальные боты для тестов
            new PlayerInfo() { Name = "SimpleBot", CreateGamePlayer = () => new SnakeBot()},
            new PlayerInfo() { Name = "RandomBot", CreateGamePlayer = () => new RandomBotPlayer(7)},
            new PlayerInfo() { Name = "SimpleBot", CreateGamePlayer = () => new SnakeBot() },
            new PlayerInfo() { Name = "RandomBot", CreateGamePlayer = () => new RandomBotPlayer(7) },
            new PlayerInfo() { Name = "SimpleBot", CreateGamePlayer = () => new SnakeBot() },
            new PlayerInfo() { Name = "RandomBot", CreateGamePlayer = () => new RandomBotPlayer(7) },
            new PlayerInfo() { Name = "SimpleBot", CreateGamePlayer = () => new SnakeBot() },
            new PlayerInfo() { Name = "RandomBot", CreateGamePlayer = () => new RandomBotPlayer(7) },
            new PlayerInfo() { Name = "SimpleBot", CreateGamePlayer = () => new SnakeBot() },
            new PlayerInfo() { Name = "RandomBot", CreateGamePlayer = () => new RandomBotPlayer(7) },
            new PlayerInfo() { Name = "SimpleBot", CreateGamePlayer = () => new SnakeBot() },
            new PlayerInfo() { Name = "RandomBot", CreateGamePlayer = () => new RandomBotPlayer(7) },
        };

        static ConcurrentDictionary<string, GameInfo> Games = new();
        public static Dictionary<IPlayer, string>  PlayerNames = new();
        public static Dictionary<string, List<IPlayer>> GamePlayers = new();

        public class GameInfo
        {
            public GameService GameService { get; set; }
            public System.Timers.Timer timer = new System.Timers.Timer();
            public int GameTickMs = 300;
            public int TicksToEnd = 1000;
        }

        public void TryAddPlayer(string playerId)
        {
            var playerInfo = Players.FirstOrDefault(v => v.Name == playerId);

            if(playerInfo == null)
            {
                playerInfo = new PlayerInfo() { Name = playerId };
                Players.Add(playerInfo);
            }
        }


        public IPlayer AddPlayerToGame(string gameId, string playerId)
        {
            if (Games.TryGetValue(gameId, out GameInfo game) == false)
            {
                return null;
            }

            var playerInfo = Players.FirstOrDefault(v => v.Name == playerId);

            var player = playerInfo.JoinGame(gameId);

            PlayerNames[player] = playerId;
            game.GameService.AddPlayer(player);

            return player;
        }

        public IPlayer GetPlayerOfGame(string gameId, string playerId)
        {
            var playerInfo = Players.FirstOrDefault(v => v.Name == playerId);
            return playerInfo.GetPlayerFromGame(gameId);
        }

        public void RemovePlayer(string playerId)
        {
            var playerInfo = Players.FirstOrDefault(v => v.Name == playerId);
            Players.Remove(playerInfo);
        }

        public void ExcludeFromGame(string gameId, string playerId)
        {
            var playerInfo = Players.FirstOrDefault(v => v.Name == playerId);
            if (playerInfo == null) return;
            playerInfo.ExcludeFromGame(gameId);
        }

        public GameService GetGameOrNull(string gameId)
        {
            if(Games.TryGetValue(gameId, out GameInfo game) == false)
            {
                return null;
            }
            return game.GameService;
        }

        public void CreateGame(string gameId, GameService gameService, int ticksToEnd)
        {
            var gameInfo = new GameInfo();
            gameInfo.GameService = gameService;
            gameInfo.TicksToEnd = ticksToEnd;
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
            var ticksToEnd = gameInfo.TicksToEnd;

            timer.Elapsed += (o, e) =>
            {
                if (gameService.CurrentTick <= ticksToEnd)
                    GameTick(gameId, gameService);
                else
                    StopGame(gameId);
            };

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
            var startGameInfo = GamesInfo.FirstOrDefault(v => v.GameId == gameId);
            startGameInfo.MarkAsOver();
            game.timer.Stop();
            Games.Remove(gameId, out GameInfo gameInfo);
        }
    }

    public class GameHub : Hub
    {
        static Dictionary<string, RemotePlayer> RemotePlayers = new();
        static Dictionary<string, string> ConnectionIdToGameId = new();

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

            if (RemotePlayers.TryGetValue(Context.ConnectionId, out RemotePlayer remotePlayer) == false)
                return;

            if (remotePlayer == null)
                return;

            remotePlayer.SetCommand(command);
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            if(ConnectionIdToGameId.TryGetValue(Context.ConnectionId, out string gameId))
                GamesHubService.ExcludeFromGame(gameId, Context.UserIdentifier);
            RemotePlayers.Remove(Context.ConnectionId);
          
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

        public StartGameInfo StartGame(string gameId)
        {
            var startGameInfo = GamesHubService.GamesInfo.FirstOrDefault(v => v.GameId == gameId);

            Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            if (startGameInfo == null)
            {
                Clients.Caller.SendAsync("ShowMessage", $"Игры с таким id={gameId} не найдено");
                return null;
            }

            ConnectionIdToGameId[Context.ConnectionId] = gameId;


            if (startGameInfo.IsNotStarted)
            {
                startGameInfo.MarkAsInProgress();

                var snakeGame = new AnimateSnakeGameService
                (
                    (gameService) => null,
                    GetMap()
                );

                snakeGame.ApplesManager.SetMaxApplesCount(startGameInfo.ApplesCount);

                GamesHubService.CreateGame(gameId, snakeGame, startGameInfo.TicksToEnd);

                foreach (var playerName in startGameInfo.Players)
                {
                    var player = GamesHubService.AddPlayerToGame(gameId, playerName);

                    if (player is RemotePlayer remotePlayer)
                        RemotePlayers[Context.ConnectionId] = remotePlayer;
                }

                GamesHubService.StartGame(gameId, GameTick);
            }
            else
            {
                RemotePlayers[Context.ConnectionId] = (RemotePlayer)GamesHubService.GetPlayerOfGame(gameId, Context.UserIdentifier);
            }

            return startGameInfo;

            string GetMap()
            {
                var mapInfo = MainHub.Maps
                    .FirstOrDefault(v => v.Name == startGameInfo.MapName);
                if (mapInfo == null)
                    return SnakeGame2.SnakeGameService.TestMap;
                return mapInfo.Map;
            }
        }

        public void StopGame(string gameId)
        {
            GamesHubService.StopGame(gameId);
        }

        private void GameTick(string gameId, GameService gameService)
        {
            var playersScores = GetPlayerInfos();
            var stringMap = gameService.ToStringMap();

            hubContext.Clients.Group(gameId).SendAsync("Receive", stringMap, gameService.CurrentTick, playersScores);

            gameService.MakeGameTick();

            List<PlayerInfo> GetPlayerInfos()
            {
                var playersScores = gameService.PlayersScores
                    .Select(v => new PlayerInfo(GetUserNameFromPlayer(v.Key), v.Value))
                    .ToList();

                return playersScores;

                string GetUserNameFromPlayer(IPlayer player)
                {
                    return GamesHubService.PlayerNames.TryGetValue(player, out string name) ? name : null;
                };
            }
        }

        public AnimationInfo GetAnimateInfo()
        {
            return AnimationInfo.Instanse;
        }
    }

    public class AnimationInfo
    {

        public static AnimationInfo Instanse = new();

        static AnimationInfo()
        {
            AnimateSnakeGameService.InitAnimationInfo(Instanse);
        }

        public ConcurrentDictionary<char, string> MapCharToSprite { get; set; } = new()
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
            return connection.User.Identity.Name;
            return connection.ConnectionId; // Для тестирования
            // или так
            //return connection.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
