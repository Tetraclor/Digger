using Microsoft.AspNetCore.SignalR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi
{
    public class MainHub : Hub
    {
        ILogger logger;

        public MainHub(ILogger logger)
        {
            this.logger = logger;
        }

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
            logger.Information("Context: " + Context.UserIdentifier);
            logger.Information("DB: " + UserService.GetUserOrNull(Context.UserIdentifier).Name);
            return UserService.GetUserOrNull(Context.UserIdentifier);
        }

        public string GetMyToken()
        {
            return UserService.GetToken(Context.UserIdentifier);
        }

        public List<GamesManagerService.GameProccesInfo> GetGames()
        {
            return GamesManagerService.GetAllGames();
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
