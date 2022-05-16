using Common;
using GameCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace WebApi.Services
{
    /// <summary>
    /// Отвечает за работу с игроками
    /// </summary>
    public class GamesManagerService
    {
        public class PlayerInfo
        {
            public string Name { get; set; }
            public int Score { get; set; }
            public Point SnakeHead { get; set; }
            public List<Point> SnakeTail { get; set; }

            public PlayerInfo(string name, int score)
            {
                Name = name;
                Score = score;
            }
        }

        public class GameProccesInfo
        {
            public GameStartInfo StartGameInfo { get; set; }
            public int GameTickMs { get; set; } = 300;
            public bool IsOver { get; private set; } = false;
            public bool IsInProgress { get; private set; } = false;
            public bool IsNotStarted => !IsOver && !IsInProgress;

            public Timer timer = new();
            public GameService GameService;

            public void MarkAsOver() { IsOver = true; IsInProgress = false; }
            public void MarkAsInProgress() { IsInProgress = true; }
        }

        static readonly ConcurrentDictionary<string, GameProccesInfo> Games = new();
        static readonly ConcurrentDictionary<IPlayer, string> PlayerNames = new();

        public GamesManagerService()
        {
            var snakebotUser = UserService.GetUserOrNull("SnakeBot");
            var randombotUser = UserService.GetUserOrNull("RandomBot");
            snakebotUser.CreateGamePlayer = () => new SnakeBot();
            randombotUser.CreateGamePlayer = () => new RandomBotPlayer(24);
            UserService.MarkUserBotOnline(snakebotUser.Name);
            UserService.MarkUserBotOnline(randombotUser.Name);
        }

        public static GameProccesInfo GetGame(string gameId)
        {
            return Games.GetValueOrDefault(gameId);
        }

        public static List<GameProccesInfo> GetAllGames()
        {
            return Games.Values.ToList();
        }

        public static List<PlayerInfo> GetPlayerInfos(string gameId)
        {
            if (Games.TryGetValue(gameId, out GameProccesInfo gameInfo) == false)
            {
                return null;
            }
            
            var snakeGame = (SnakeGame2.SnakeGameService)gameInfo.GameService;
            var playersScores = new List<PlayerInfo>();

            foreach (var spawner in snakeGame.SnakeSpawners
                .Where(v => v.IsActive))
            {
                var player = spawner.Player;
                var userName = GetUserNameFromPlayer(player);
                var score = snakeGame.GetScore(player);
                var playerInfo = new PlayerInfo(userName, score);
                playerInfo.SnakeHead = spawner.SpawnedSnake.Head;
                playerInfo.SnakeTail = spawner.SpawnedSnake.Body;

                playersScores.Add(playerInfo);
            }

            return playersScores;

            static string GetUserNameFromPlayer(IPlayer player)
            {
                return PlayerNames.TryGetValue(player, out string name) ? name : null;
            };
        }

        private static IPlayer RegisterPlayerToGame(UserAppInfo userApp, string gameId)
        {
            var userPlayerConnection = new UserPlayerConnection()
            {
                ConnectionId = null,
                Player = userApp.CreateGamePlayer(),
                GameId = gameId,
            };
            userApp.UserPlayerConnections.Add(userPlayerConnection);
            return userPlayerConnection.Player;
        }


        public static void JoinGame(string gameId, string connectionId, string userName)
        {
            var startGameInfo = Games.GetValueOrDefault(gameId)?.StartGameInfo;

            if (startGameInfo == null || startGameInfo.UserNames.Contains(userName) == false)
                return;

            var userPlayerConnections = UserService.GetUserOrNull(userName).UserPlayerConnections;
            var connected = userPlayerConnections
                .FirstOrDefault(x => x.GameId == gameId && x.IsConnected == false);

            var userPlayerConnection = new UserPlayerConnection()
            {
                ConnectionId = connectionId ?? "IsServerBot", // Если connectionId значит подключается бот со стороны сервера
                Player = connected.Player,
                GameId = gameId,
            };

            userPlayerConnections.Remove(connected);
            userPlayerConnections.Add(userPlayerConnection);
        }

        public static bool CreateGame(GameStartInfo startGameInfo)
        {
            if (startGameInfo == null)
            {
                return false;
            }

            var snakeGame = new AnimateSnakeGameService
            (
                (gameService) => null,
                startGameInfo.ApplesCount,
                MapService.GetMap(startGameInfo.MapName).Map
            );

            var gameInfo = new GameProccesInfo();
            gameInfo.GameService = snakeGame;
            gameInfo.StartGameInfo = startGameInfo;
            Games[startGameInfo.GameId] = gameInfo;

            foreach(var userName in startGameInfo.UserNames)
            {
                var userApp = UserService.GetUserOrNull(userName);
                if (userApp == null)
                    continue;

                var player = RegisterPlayerToGame(userApp, startGameInfo.GameId);
                snakeGame.AddPlayer(player);
                PlayerNames[player] = userName;
            }

            return true;
        }

        public static void StartGame(string gameId, Action<string, GameService> GameTick)
        {
            if (Games.TryGetValue(gameId, out GameProccesInfo gameProccesInfo) == false)
            {
                return;
            }

            if(gameProccesInfo.IsNotStarted == false)
            {
                return;
            }

            gameProccesInfo.MarkAsInProgress();
            BotsHub.JoinConnectedBotsToGame(gameProccesInfo.StartGameInfo.UserNames, gameId);

            var timer = gameProccesInfo.timer;
            var gameService = gameProccesInfo.GameService;
            var ticksToEnd = gameProccesInfo.StartGameInfo.TicksToEnd;

            timer.Elapsed += (o, e) =>
            {
                if (gameService.CurrentTick <= ticksToEnd)
                    GameTick(gameId, gameService);
                else
                    StopGame(gameId);
            };

            timer.Interval = gameProccesInfo.GameTickMs;
            timer.Enabled = true;
            timer.Start();

            GameTick(gameId, gameService);
        }

        public static void StopGame(string gameId)
        {
            if (Games.Remove(gameId, out GameProccesInfo gameInfo) == false)
            {
                return;
            }

            gameInfo.timer.Stop();
            gameInfo.MarkAsOver();

            CalcAndSaveRatings(gameInfo.StartGameInfo, gameInfo, gameId);
        }

        private static void CalcAndSaveRatings(GameStartInfo startGameInfo, GameProccesInfo gameInfo, string gameId)
        {
            if (startGameInfo.UserNames.Length < 2)
                return;

            var rateds = new Dictionary<IRated, UserAppInfo>();

            foreach (var playerName in startGameInfo.UserNames)
            {
                var userApp = UserService.GetUserOrNull(playerName);
                if (userApp == null)
                    continue;
                var player = GetPlayersFromGame(userApp, gameId).FirstOrDefault();
                if (gameInfo.GameService.PlayersScores.TryGetValue(player, out int score) == false)
                    continue;

                rateds[new RateOjbect(userApp.Rate, score)] = userApp;
            }

            var ratingService = new RatingService();
            var dict = ratingService.Calc(rateds.Keys.ToArray());

            foreach (var rating in dict)
            {
                rateds[rating.Key].Rate = (int)rating.Value;
            }

            UserService.SaveNewRating(rateds.Values.ToArray());
        }

        private static List<IPlayer> GetPlayersFromGame(UserAppInfo userApp, string gameId)
        {
            return userApp.UserPlayerConnections
                .Where(v => v.GameId == gameId)
                .Select(v => v.Player)
                .ToList();
        }
    }
}
