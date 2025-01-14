﻿using Common;
using GameCore;
using Microsoft.AspNetCore.SignalR;
using System;
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

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = UserService.GetUserOrNull(Context.UserIdentifier);

            user.UserPlayerConnections.Where(v => v.ConnectionId == Context.ConnectionId)
                .ToList()
                .ForEach(v => v.ConnectionId = null);

            return base.OnDisconnectedAsync(exception);
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

        public GameStartInfo StartGame(string gameId)
        {
            var gameInfo = GamesManagerService.GetGame(gameId);
            var startGameInfo = gameInfo?.StartGameInfo;

            if (gameInfo == null)
            {
                Clients.Caller.SendAsync("ShowMessage", $"Игры с таким id={gameId} не найдено");
                return null;
            }

            Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            GamesManagerService.StartGame(gameId, GameTick);
            JoinToGame(gameId);

            return startGameInfo;
        }

        public void JoinToGame(string gameId)
        {
            GamesManagerService.JoinGame(gameId, Context.ConnectionId, Context.UserIdentifier);
        }

        public void StopGame(string gameId)
        {
            GamesManagerService.StopGame(gameId);
        }

        private void GameTick(string gameId, GameService gameService)
        {
            var playersScores = GamesManagerService.GetPlayerInfos(gameId);
            var stringMap = gameService.ToStringMap();

            var gameStateView = new GameStateView()
            {
                Map = stringMap,
                Tick = gameService.CurrentTick,
                PlayersScores = playersScores,
                GameState = gameService.GameState, 
            };

            hubContext.Clients.Group(gameId).SendAsync("Receive", gameStateView);

            gameService.MakeGameTick();
        }

        public UserAppInfo GetMe()
        {
            return UserService.GetUserOrNull(Context.UserIdentifier);
        }

        public AnimationInfo GetAnimateInfo()
        {
            return AnimationInfo.Instanse;
        }
    }

    public class GameStateView
    {
        public string Map { get; set; }
        public int Tick { get; set; }
        public List<GamesManagerService.PlayerInfo> PlayersScores { get; set; }
        public object GameState { get; set; }
    }
}
