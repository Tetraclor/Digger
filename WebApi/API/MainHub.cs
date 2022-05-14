using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi
{
    public class MainHub : Hub
    {
        public void StartGame(GameStartInfo startGameInfo)
        {
            GamesManagerService.CreateGame(startGameInfo);
        }

        public override Task OnConnectedAsync()
        {
            UserService.MarkUserOnline(Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserService.MarkUserOffline(Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }

        public List<UserAppInfo> GetPlayers()
        {
            return UserService.Users
                .Where(v => v.IsBotOnline || v.IsUserOnline)
                .ToList();
        }

        public UserAppInfo GetMe()
        {
            return UserService.Users
                .FirstOrDefault(v => v.Name == Context.UserIdentifier);
        }

        public string GetMyToken()
        {
            return UserService.GetToken(Context.UserIdentifier);
        }

        public List<GameStartInfo> GetGames()
        {
            return GamesManagerService.GetAllGames()
                .Select(v => v.StartGameInfo)
                .ToList();
        }

        public List<MapService.MapInfo> GetMaps()
        {
            return MapService.Maps;
        }

        public AnimationInfo GetAnimateInfo()
        {
            return new AnimationInfo();
        }
    }
}
