using Common;
using GameCore;
using GameSnake;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
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
        static GameService gameService = new SnakeGame2.SnakeGameService();
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

            var remotePlayer = RemotePlayers[this.Context.ConnectionId];
            var playerCommand = gameService.ParsePlayerCommand(command);
            remotePlayer.PlayerCommand = playerCommand;  
        }

        static bool IsFirstStart = true;

        public void StartGame()
        {
            if (IsFirstStart)
            {
                timer.Stop();
                timer = new System.Timers.Timer();

                timer.Elapsed += (o, e) => GameTick();
                timer.Interval = GameTickMs;
                timer.Enabled = true;

                GameTick();
            }

            IsFirstStart = false;

            var remotePlayer = new RemotePlayer();
            gameService.AddPlayer(remotePlayer);
            RemotePlayers[this.Context.ConnectionId] = remotePlayer;
        }

        void GameTick()
        {
            gameService.MakeGameTick();
            var stringMap = gameService.GameState.Map.MapToString();
            hubContext.Clients.All.SendAsync("Receive", stringMap, gameService.CurrentTick);
        }

        public void StopGame()
        {
            timer.Stop();
        }
    }
}
