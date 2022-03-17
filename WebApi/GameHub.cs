﻿using Common;
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

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            var player = RemotePlayers[this.Context.ConnectionId];

            gameService.RemovePlayer(player);

            return base.OnDisconnectedAsync(exception);
        }

        void GameTick()
        {
            gameService.MakeGameTick();

            var stringMap = gameService.ToStringMap();

            hubContext.Clients.All.SendAsync("Receive", stringMap, gameService.CurrentTick);
        }


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
                return gameService
                    .SnakeSpawners
                    .FirstOrDefault() // TODO
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
