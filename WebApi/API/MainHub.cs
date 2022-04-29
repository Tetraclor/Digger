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
using WebApi.DataSource;

namespace WebApi
{
    public class MainHub : Hub
    {
        public static List<MapInfo> Maps = new();

        public class MapInfo
        {
            public string Name { get; set; }
            public string Map { get; set; }
            public int ApplesCount { get; set; }
        }

        static MainHub()
        {
            foreach (var path in Directory.GetFiles("wwwroot/SnakeMaps/"))
            {
                var filename = Path.GetFileName(path);
                var map = File.ReadAllText(path);
                var mapInfo = new MapInfo() { Name = filename, Map = map };
                Maps.Add(mapInfo);
            }
        }

        public GamesHubService GamesHub { get; }
        public HttpContext HttpContext { get; }

        public MainHub(GamesHubService gamesHub)
        {
            GamesHub = gamesHub;
            //HttpContext = httpContext;
        }

        public void StartGame(StartGameInfo startGameInfo)
        {
            GamesHubService.GamesInfo.Add(startGameInfo);
        }

        public override Task OnConnectedAsync()
        {
            GamesHub.TryAddPlayer(Context.UserIdentifier);
            UserService.MarkOnline(Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            //  GamesHub.RemovePlayer(Context.UserIdentifier);
            UserService.MarkOffline(Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }

        public List<PlayerInfo> GetPlayers()
        {
            return GamesHubService.Players
                .Where(v => UserService.IsOnline(v.Name))
                .ToList();
        }

        public PlayerInfo GetMe()
        {
            return GamesHubService.Players
                .FirstOrDefault(v => v.Name == Context.UserIdentifier);
        }

        public List<StartGameInfo> GetGames()
        {
            return GamesHubService.GamesInfo;
        }

        public List<MapInfo> GetMaps()
        {
            Maps.ForEach(v => v.Map = v.Map.Replace("\r", "").Trim());
            return Maps;
        }

        public AnimationInfo GetAnimateInfo()
        {
            return new AnimationInfo();
        }
    }
}
