using Common;
using GameCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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

        public class GameInfo
        {
            public GameService GameService { get; set; }
            public System.Timers.Timer timer = new System.Timers.Timer();
            public int GameTickMs = 300;
            public int TicksToEnd = 1000;
        }

        public static List<StartGameInfo> StartGamesInfo { get; set; } = new();
        static readonly ConcurrentDictionary<string, GameInfo> Games = new();
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

        public static List<PlayerInfo> GetPlayerInfos(string gameId)
        {
            if (Games.TryGetValue(gameId, out GameInfo gameInfo) == false)
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

        public static void JoinGame(string connectionId, UserAppInfo userApp, StartGameInfo startGameInfo)
        {
            if (startGameInfo.Players.Contains(userApp.Name) == false)
                return;

            userApp.JoinGame(connectionId, startGameInfo.GameId);
        }

        public static bool CreateGame(StartGameInfo startGameInfo)
        {
            if (startGameInfo == null)
            {
                return false;
            }

            if(startGameInfo.IsNotStarted == false)
            {
                return false;
            }

            startGameInfo.MarkAsInProgress();

            var snakeGame = new AnimateSnakeGameService
            (
                (gameService) => null,
                MapService.GetMap(startGameInfo.MapName).Map
            );

            snakeGame.ApplesManager.SetMaxApplesCount(startGameInfo.ApplesCount);

            var gameInfo = new GameInfo();
            gameInfo.GameService = snakeGame;
            gameInfo.TicksToEnd = startGameInfo.TicksToEnd;
            Games[startGameInfo.GameId] = gameInfo;

            foreach(var userName in startGameInfo.Players)
            {
                var userApp = UserService.GetUserOrNull(userName);
                if (userApp == null)
                    continue;
                var player = userApp.RegisterPlayer(startGameInfo.GameId);
                PlayerNames[player] = userName;
                snakeGame.AddPlayer(player);
            }

            return true;
        }

        public static void StartGame(string gameId, Action<string, GameService> GameTick)
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

        public static void StopGame(string gameId)
        {
            if (Games.Remove(gameId, out GameInfo gameInfo) == false)
            {
                return;
            }
            gameInfo.timer.Stop();
            var startGameInfo = StartGamesInfo.FirstOrDefault(v => v.GameId == gameId);
            startGameInfo.MarkAsOver();

            CalcAndSaveRatings(startGameInfo, gameInfo, gameId);
        }

        private static void CalcAndSaveRatings(StartGameInfo startGameInfo, GameInfo gameInfo, string gameId)
        {
            if (startGameInfo.Players.Length < 2)
                return;

            var rateds = new Dictionary<IRated, UserAppInfo>();

            foreach (var playerName in startGameInfo.Players)
            {
                var userApp = UserService.GetUserOrNull(playerName);
                if (userApp == null)
                    continue;
                var player = userApp.GetPlayersFromGame(gameId).FirstOrDefault();
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
    }
}
