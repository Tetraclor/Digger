using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DataSource;

namespace WebApi
{
    public class BotsHub : Hub
    {
        public static List<User> JoinedBotUsers = new List<User>();
        static ConcurrentDictionary<string, User> connectionIdToUser = new();
        static ConcurrentDictionary<User, string> userToConnectionId = new();
        private static IHubContext<BotsHub> botsHub;
        private readonly GamesHubService gamesHubService;

        public BotsHub(IHubContext<BotsHub> botsHub, GamesHubService gamesHubService)
        {
            BotsHub.botsHub = botsHub;
            this.gamesHubService = gamesHubService;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            connectionIdToUser.TryRemove(Context.ConnectionId, out User user);
            userToConnectionId.TryRemove(user, out string _);
            JoinedBotUsers.Remove(user);
            UserService.MarkOffline(user.Name);
            return base.OnDisconnectedAsync(exception);
        }

        public void Join(string token)
        {
            var user = UserService.GetOrNull(token);
            if(user == null)
                return;
            connectionIdToUser[Context.ConnectionId] = user;
            userToConnectionId[user] = Context.ConnectionId;
            JoinedBotUsers.Add(user);
            UserService.MarkOnline(user.Name);
        }

        public static void JoinConnectedBotsToGame(string[] userNames, string gameId)
        {
            var connectedUsers = JoinedBotUsers
                .Where(v => userNames.Contains(v.Name))
                .ToList();

            if (connectedUsers.Count == 0)
                return;

            foreach (var user in connectedUsers)
            {
                var connectionId = userToConnectionId[user];
                botsHub.Clients.Client(connectionId).SendAsync("JoinToGame", gameId);
            }
        }
    }
}
