using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApi
{
    public class UserAppInfo
    {
        public string Name { get; set; }
        public int Rate { get; set; } = 1500;

        public Func<IPlayer> CreateGamePlayer = () => new RemotePlayer();
        public List<UserPlayerConnection> UserPlayerConnections = new();

        public IPlayer JoinGame(string gameId)
        {
            var player = CreateGamePlayer();
            var userPlayerConnection = new UserPlayerConnection()
            {
                Player = player,
                GameId = gameId,
            };
            UserPlayerConnections.Add(userPlayerConnection);
            return player;
        }

        public IPlayer GetPlayerFromGame(string gameId)
        {
            return UserPlayerConnections.FirstOrDefault(v => v.GameId == gameId).Player;
        }

        public int ExcludeFromGame(string gameId)
        {
            return UserPlayerConnections.RemoveAll(v => v.GameId == gameId);
        }
    }

    public class UserPlayerConnection
    {
        public string GameId { get; set; }
        public IPlayer Player { get; set; }
        public string ConnectionId { get; set; }
    }
}
