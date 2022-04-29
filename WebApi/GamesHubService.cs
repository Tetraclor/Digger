﻿using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WebApi.DataSource;

namespace WebApi
{
    public class GamesHubService
    {
        public static List<StartGameInfo> GamesInfo { get; set; } = new();
        public static List<PlayerInfo> Players { get; set; } = new();

        public readonly ConcurrentDictionary<string, GameInfo> Games = new();
        public readonly ConcurrentDictionary<IPlayer, string> PlayerNames = new();
        public readonly ConcurrentDictionary<string, List<IPlayer>> GamePlayers = new();

        public GamesHubService()
        {
            var users = UserService.GetAllUser();

            foreach (var user in users)
            {
                Func<IPlayer> createBot = () => new RemotePlayer();

                if (user.Name == "SnakeBot")
                {
                    createBot = () => new SnakeBot();
                    UserService.MarkOnline(user.Name);
                }
                if (user.Name == "RandomBot")
                {
                    createBot = () => new RandomBotPlayer(7);
                    UserService.MarkOnline(user.Name);
                }

                var playerInfo = new PlayerInfo() { Name = user.Name, Rate = (int)user.Rating, CreateGamePlayer = createBot };

                Players.Add(playerInfo);
            }
        }

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

            if (playerInfo == null)
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


        public void ExcludeFromGame(string gameId, string playerId)
        {
            var playerInfo = Players.FirstOrDefault(v => v.Name == playerId);
            if (playerInfo == null) return;
            playerInfo.ExcludeFromGame(gameId);
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
}
