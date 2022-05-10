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
        public bool IsUserOnline { get; set; } = false;
        public bool IsBotOnline { get; set; } = false;

        public Func<IPlayer> CreateGamePlayer = () => new RemotePlayer();
        public List<UserPlayerConnection> UserPlayerConnections = new();

        public IPlayer RegisterPlayer(string gameId)
        {
            var userPlayerConnection = new UserPlayerConnection()
            {
                ConnectionId = null,
                Player = CreateGamePlayer(),
                GameId = gameId,
            };
            UserPlayerConnections.Add(userPlayerConnection);
            return userPlayerConnection.Player;
        }

        public IPlayer JoinGame(string conectionId, string gameId)
        {
            var connected = UserPlayerConnections
                .FirstOrDefault(x => x.GameId == gameId && x.IsConnected == false);

            var userPlayerConnection = new UserPlayerConnection()
            {
                ConnectionId = conectionId ?? "IsServerBot", // Если connectionId значит подключается бот со стороны сервера
                Player = connected.Player,
                GameId = gameId,
            };

            UserPlayerConnections.Remove(connected);
            UserPlayerConnections.Add(userPlayerConnection);

            return userPlayerConnection.Player;
        }

        public List<IPlayer> GetPlayersFromGame(string gameId)
        {
            return UserPlayerConnections
                .Where(v => v.GameId == gameId)
                .Select(v => v.Player)
                .ToList();
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

        public bool IsConnected => ConnectionId != null;
    }
}
