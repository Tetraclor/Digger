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
        static RemotePlayer remotePlayer = new RemotePlayer();
        static IPlayer randomBotPlayer = new ListBotPlayer(FourDirMove.Down, FourDirMove.Left);
        static GameService gameService = new SnakeGameService(10, 10);
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

            var playerCommand = gameService.ParsePlayerCommand(command);
            remotePlayer.PlayerCommand = playerCommand;  
        }

        public void StartGame()
        {
            remotePlayer = new RemotePlayer();
            gameService.AddPlayer(randomBotPlayer);

            timer.Stop();
            timer = new System.Timers.Timer();

            timer.Elapsed += (o, e) => aTimer_Elapsed();
            timer.Interval = GameTickMs;
            timer.Enabled = true;

            aTimer_Elapsed();

            void aTimer_Elapsed()
            {
                gameService.MakeGameTick();
                var stringMap = gameService.GameState.Map.MapToString();
                hubContext.Clients.All.SendAsync("Receive", stringMap, gameService.CurrentTick);
            }
        }

        public void StopGame()
        {
            timer.Stop();
        }
    }
}
