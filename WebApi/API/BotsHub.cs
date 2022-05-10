using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DataSource;
using WebApi.Services;

namespace WebApi
{
    public class BotsHub : Hub
    {
        static List<UserAppInfo> JoinedBotUsers = new();
        static ConcurrentDictionary<string, UserAppInfo> connectionIdToUser = new();
        static ConcurrentDictionary<UserAppInfo, string> userToConnectionId = new();
        static IHubContext<BotsHub> botsHub;

        public BotsHub(IHubContext<BotsHub> botsHub)
        {
            BotsHub.botsHub = botsHub;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            connectionIdToUser.TryRemove(Context.ConnectionId, out UserAppInfo user);
            userToConnectionId.TryRemove(user, out string _);
            JoinedBotUsers.Remove(user);
            UserService.MarkUserBotOffline(user.Name);
            return base.OnDisconnectedAsync(exception);
        }

        public void Join(string token)
        {
            var user = UserService.GetUserOrNull(token);
            if(user == null)
                return;
            connectionIdToUser[Context.ConnectionId] = user;
            userToConnectionId[user] = Context.ConnectionId;
            JoinedBotUsers.Add(user);
            UserService.MarkUserBotOnline(user.Name);
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
                var countBotCopies = userNames.Count(v => v == user.Name);

                for (int i = 0; i < countBotCopies; i++)
                {
                    botsHub.Clients.Client(connectionId).SendAsync("JoinToGame", gameId);
                }
            }
        }
    }
}
