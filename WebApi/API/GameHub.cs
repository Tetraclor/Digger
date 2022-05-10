using Common;
using GameCore;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi
{
    public class GameHub : Hub
    {
        IHubContext<GameHub> hubContext { get; }

        public GameHub(IHubContext<GameHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public bool SendTurn(string command)
        {
            var connectionId = Context.ConnectionId;

            if (connectionId == null || command == null)
                return false;

            var userApp = UserService.GetUserOrNull(Context.UserIdentifier);

            if (userApp == null)
                return false;

            foreach (var connection in userApp.UserPlayerConnections)
            {
                if (connection.ConnectionId != connectionId)
                    continue;
                if (connection.Player is RemotePlayer remotePlayer)
                    remotePlayer.SetCommand(command);
            }

            return true;
        }

        public override Task OnConnectedAsync()
        {
          //  UserService.MarkUserOnline(Context.UserIdentifier); // Как-то определить бот это или человек
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
          //  UserService.MarkUserOffline(Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }

        public StartGameInfo StartGame(string gameId)
        {
            var startGameInfo = GamesManagerService.StartGamesInfo.FirstOrDefault(v => v.GameId == gameId);

            if (startGameInfo == null)
            {
                Clients.Caller.SendAsync("ShowMessage", $"Игры с таким id={gameId} не найдено");
                return null;
            }

            Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            if (startGameInfo.IsNotStarted)
            {
                GamesManagerService.CreateGame(startGameInfo);
                BotsHub.JoinConnectedBotsToGame(startGameInfo.Players, startGameInfo.GameId);
                GamesManagerService.StartGame(gameId, GameTick);
            }

            if (startGameInfo.Players.Contains(Context.UserIdentifier))
            {
                GamesManagerService.JoinGame(Context.ConnectionId, UserService.GetUserOrNull(Context.UserIdentifier), startGameInfo);
            }    

            return startGameInfo;
        }

        public void StopGame(string gameId)
        {
            GamesManagerService.StopGame(gameId);
        }

        private void GameTick(string gameId, GameService gameService)
        {
            var playersScores = GamesManagerService.GetPlayerInfos(gameId);
            var stringMap = gameService.ToStringMap();

            hubContext.Clients.Group(gameId).SendAsync("Receive", stringMap, gameService.CurrentTick, playersScores);

            gameService.MakeGameTick();
        }

        public AnimationInfo GetAnimateInfo()
        {
            return AnimationInfo.Instanse;
        }
    }
}
