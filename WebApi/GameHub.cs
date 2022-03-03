using GameCore;
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

        public GameHub(IHubContext<GameHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        static System.Timers.Timer timer = new System.Timers.Timer();
        static RemotePlayer remotePlayer;
        IHubContext<GameHub> hubContext { get; }

        public void SendTurn(string command)
        {
            if (command == null)
                return;
           
            var diggerMove = Enum.Parse<GameDigger.DiggerMove>(command, true);
            remotePlayer.PlayerCommand = new GameDigger.PlayerCommand() { Move = diggerMove };
        }

        public void StartGame()
        {
            remotePlayer = new RemotePlayer();
            var creature = GameService.GameState.Map[2, 1]; 
            GameService.GameState.AddPlayer(remotePlayer, creature);

            timer.Stop();
            timer = new System.Timers.Timer();
            var tick = 0;

            timer.Elapsed += aTimer_Elapsed;
            timer.Interval = 100;
            timer.Enabled = true;

            void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                GameService.Game.BeginAct();
                GameService.Game.EndAct();

                var stringMap = GameService.GameState.Map.MapToString();

                hubContext.Clients.All.SendAsync("Receive", stringMap, tick++);
            }
        }

        public void StopGame()
        {
            timer.Stop();
        }
    }
}
